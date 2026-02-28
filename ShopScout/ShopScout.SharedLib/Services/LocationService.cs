using Microsoft.JSInterop;
using ShopScout.SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Services;

public class LocationService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _httpClient;
    private readonly StorageService _storageService;
    private IJSObjectReference? _jsModule;

    public LocationService(IJSRuntime js, StorageService storageService, HttpClient httpClient)
    {
        _js = js;
        _httpClient = httpClient;
        _storageService = storageService;
    }

    private async Task<IJSObjectReference> GetJSModuleAsync()
    {
        _jsModule ??= await _js.InvokeAsync<IJSObjectReference>("import", "/js/location.js");
        return _jsModule;
    }

    /// <summary>
    /// Asynchronously retrieves the user's current geographic location, using cached data when available and
    /// sufficiently accurate.
    /// </summary>
    /// <remarks>If a previously cached location is available and within 0.2 kilometers of the newly detected
    /// location, the cached value is returned. Otherwise, the cache is updated with the new location. This method
    /// returns <see langword="null"/> if an error occurs during location retrieval.</remarks>
    /// <returns>A <see cref="LocationResult"/> representing the user's location if retrieval succeeds; otherwise, <see
    /// langword="null"/> if the location cannot be determined.</returns>
    public async Task<LocationResult?> GetUserLocationAsync()
    {
        try
        {
            var module = await GetJSModuleAsync();
            var location = await module.InvokeAsync<LocationResult>("getLocation");
            var userLocation = await GetCachedUserLocation();
            if (userLocation != null)
            {
                if (CalculateDistance(location.Latitude, location.Longitude, userLocation.Latitude, userLocation.Longitude) <= .2)
                {
                    return userLocation;
                }
                else
                {
                    await _storageService.RemoveAllStartingWithAsync($"{userLocation.Latitude},{userLocation.Longitude}");
                }
            }

            await _storageService.SetValue("cachedUserLocation", location);
            return location;
        }
        catch
        {
            var userLocation = await GetCachedUserLocation();
            return userLocation;
        }
    }

    /// <summary>
    /// Retrieves the cached geographic location of the user, if available.
    /// </summary>
    /// <returns>A tuple containing the latitude and longitude of the user's cached location, or null if no cached location is
    /// found.</returns>
    private async Task<LocationResult?> GetCachedUserLocation()
    {
        return await _storageService.GetValue<LocationResult?>("cachedUserLocation", null);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // ignore
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }
    }

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers

        // Convert degrees to radians
        double lat1Rad = DegreesToRadians(lat1);
        double lat2Rad = DegreesToRadians(lat2);
        double deltaLat = DegreesToRadians(lat2 - lat1);
        double deltaLon = DegreesToRadians(lon2 - lon1);

        // Haversine formula
        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = R * c;

        return distance; // Returns distance in kilometers
    }

    private double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    /// <summary>
    /// Calculates the road distance between two coordinates using OSRM API
    /// </summary>
    /// <param name="lat1">Latitude of first point</param>
    /// <param name="lon1">Longitude of first point</param>
    /// <param name="lat2">Latitude of second point</param>
    /// <param name="lon2">Longitude of second point</param>
    /// <returns>Tuple with distance value and unit ("m" or "km"), or null if request fails</returns>
    public async Task<(double distance, string unit)?> GetRoadDistanceAsync(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        try
        {
            // OSRM expects coordinates in lon,lat format
            string url = $"https://router.project-osrm.org/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            string jsonResponse = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            // Check if route was found
            if (root.GetProperty("code").GetString() != "Ok")
                return null;

            // Get distance in meters from the first route
            double distanceInMeters = root
                .GetProperty("routes")[0]
                .GetProperty("distance")
                .GetDouble();

            // Return in meters if less than 1000, otherwise in kilometers
            if (distanceInMeters < 1000)
            {
                return (distanceInMeters, "m");
            }
            else
            {
                return (distanceInMeters / 1000.0, "km");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating distance: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Retrieves distances and durations from a single origin to multiple destinations in one API call.
    /// Checks the local storage cache first, and only queries the API for uncached destinations.
    /// </summary>
    public async Task<List<RouteToStore?>> GetBulkRouteInfoAsync(
        double originLat, double originLon,
        List<(double Lat, double Lon)> destinations)
    {
        try
        {
            var finalResults = new RouteToStore?[destinations.Count];

            var destinationsToFetch = new List<(double Lat, double Lon)>();
            var fetchIndices = new List<int>();

            for (int i = 0; i < destinations.Count; i++)
            {
                var dest = destinations[i];

                var key = $"{originLat},{originLon};{dest.Lat},{dest.Lon}";
                var cachedRoute = await _storageService.GetValue<RouteToStore>(key, null);

                if (cachedRoute != null)
                {
                    finalResults[i] = cachedRoute; // Found in cache, slot it in
                    finalResults[i].Distance = Math.Round(finalResults[i].Distance, 1);
                    finalResults[i].Duration = Math.Round(finalResults[i].Distance);
                }
                else
                {
                    destinationsToFetch.Add(dest); // Needs to be fetched
                    fetchIndices.Add(i);           // Remember where it belongs
                }
            }

            if (destinationsToFetch.Count == 0)
            {
                return finalResults.ToList();
            }

            var coordinates = new List<string> { $"{originLon.ToString(CultureInfo.InvariantCulture)},{originLat.ToString(CultureInfo.InvariantCulture)}" };
            coordinates.AddRange(destinationsToFetch.Select(d => $"{d.Lon.ToString(CultureInfo.InvariantCulture)},{d.Lat.ToString(CultureInfo.InvariantCulture)}"));

            string coordsString = string.Join(";", coordinates);
            string url = $"https://router.project-osrm.org/table/v1/driving/{coordsString}?sources=0&annotations=duration,distance";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return finalResults.ToList();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            if (root.GetProperty("code").GetString() != "Ok")
                return finalResults.ToList();

            var distances = root.GetProperty("distances")[0];
            var durations = root.GetProperty("durations")[0];

            for (int i = 1; i < distances.GetArrayLength(); i++)
            {
                int originalIndex = fetchIndices[i - 1];
                var dest = destinationsToFetch[i - 1];

                if (distances[i].ValueKind == JsonValueKind.Null)
                {
                    finalResults[originalIndex] = null;
                    continue;
                }

                var routeToStore = new RouteToStore
                {
                    Distance = Math.Round(distances[i].GetDouble() / 1000.0, 1),
                    Duration = Math.Round(durations[i].GetDouble() / 60.0)
                };

                finalResults[originalIndex] = routeToStore;

                var key = $"{originLat},{originLon};{dest.Lat},{dest.Lon}";
                await _storageService.SetValue(key, routeToStore);
            }

            return finalResults.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting bulk route info: {ex.Message}");
            // In case of a hard crash, we still want to return whatever cached data we managed to pull
            return new List<RouteToStore?>();
        }
    }

    /// <summary>
    /// Retrieves route information, including distance and estimated duration, for a driving route between two
    /// geographic coordinates.
    /// </summary>
    /// <remarks>This method uses an external routing service to calculate the route and may cache results for
    /// repeated queries. If the route cannot be found or an error occurs, the method returns <see langword="null"/>.
    /// The method is asynchronous and should be awaited.</remarks>
    /// <param name="lat1">The latitude of the starting point, in decimal degrees.</param>
    /// <param name="lon1">The longitude of the starting point, in decimal degrees.</param>
    /// <param name="lat2">The latitude of the destination point, in decimal degrees.</param>
    /// <param name="lon2">The longitude of the destination point, in decimal degrees.</param>
    /// <returns>A <see cref="RouteToStore"/> object containing the distance in kilometers and estimated duration in minutes for
    /// the route, or <see langword="null"/> if the route cannot be determined.</returns>
    public async Task<RouteToStore?> GetRouteInfoAsync(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        try
        {
            var key = $"{lat1},{lon1};{lat2},{lon2}";
            var cachedRoute = await _storageService.GetValue<RouteToStore>(key, null);
            if (cachedRoute != null)
            {
                return cachedRoute;
            }

            string url = $"https://router.project-osrm.org/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            string jsonResponse = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            if (root.GetProperty("code").GetString() != "Ok")
                return null;

            var route = root.GetProperty("routes")[0];

            double distanceKm = route.GetProperty("distance").GetDouble() / 1000.0;
            double durationMin = route.GetProperty("duration").GetDouble() / 60.0;

            RouteToStore routeToStore = new()
            {
                Distance = distanceKm,
                Duration = durationMin
            };

            await _storageService.SetValue(key, routeToStore);

            return routeToStore;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting route info: {ex.Message}");
            return null;
        }
    }
}

public class LocationResult
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}