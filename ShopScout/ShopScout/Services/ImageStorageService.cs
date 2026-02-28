using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace ShopScout.Services;

public interface IImageStorageService
{
    Task<string> SaveImageAsync(IFormFile file, string productCode);
    Task DeleteImageAsync(string imageUrl);
}

public class GoogleCloudImageStorage : IImageStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    private readonly ILogger<GoogleCloudImageStorage> _logger;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public GoogleCloudImageStorage(IConfiguration config, ILogger<GoogleCloudImageStorage> logger)
    {
        _logger = logger;
        _bucketName = config["GoogleCloud:BucketName"] ?? throw new ArgumentNullException("BucketName not configured");

        // Get credentials JSON from environment variable
        var credentialsJson = Environment.GetEnvironmentVariable("GOOGLE_STORAGE_CREDENTIALS");

        if (string.IsNullOrEmpty(credentialsJson))
            throw new InvalidOperationException("GOOGLE_STORAGE_CREDENTIALS environment variable is not set");

        try
        {
            // Create GoogleCredential from JSON string
            var credential = GoogleCredential.FromJson(credentialsJson);

            // Create StorageClient with the credential
            _storageClient = StorageClient.Create(credential);

            _logger.LogInformation("Google Cloud Storage client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Google Cloud Storage client");
            throw new InvalidOperationException("Failed to initialize Google Cloud Storage", ex);
        }
    }

    public async Task<string> SaveImageAsync(IFormFile file, string productCode)
    {
        // Validate file
        if (file.Length > MaxFileSize)
            throw new InvalidOperationException("A fájl mérete meghaladja az 5MB limitet");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Érvénytelen fájl típus");

        // Generate unique object name
        var objectName = $"shopscout/{productCode}/{Guid.NewGuid()}{extension}";

        try
        {
            using var stream = file.OpenReadStream();

            // Upload with public read access
            var uploadedObject = await _storageClient.UploadObjectAsync(
                bucket: _bucketName,
                objectName: objectName,
                contentType: file.ContentType,
                source: stream,
                options: new UploadObjectOptions
                {
                    PredefinedAcl = PredefinedObjectAcl.PublicRead
                }
            );

            // Return public URL
            var publicUrl = $"https://storage.googleapis.com/{_bucketName}/{objectName}";
            _logger.LogInformation($"Image uploaded successfully: {publicUrl}");

            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to upload image for product {productCode}");
            throw new InvalidOperationException("Nem sikerült feltölteni a képet", ex);
        }
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        try
        {
            var objectName = ExtractObjectNameFromUrl(imageUrl);

            if (string.IsNullOrEmpty(objectName))
            {
                _logger.LogWarning($"Invalid image URL format: {imageUrl}");
                return;
            }

            await _storageClient.DeleteObjectAsync(_bucketName, objectName);
            _logger.LogInformation($"Image deleted successfully: {objectName}");
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning($"Image not found for deletion: {imageUrl}");
            // Don't throw - image already doesn't exist
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete image: {imageUrl}");
            throw new InvalidOperationException("Nem sikerült törölni a képet", ex);
        }
    }

    private string? ExtractObjectNameFromUrl(string imageUrl)
    {
        // Example URL: https://storage.googleapis.com/bucket-name/shopscout/userId/guid.jpg
        try
        {
            var uri = new Uri(imageUrl);
            var path = uri.AbsolutePath.TrimStart('/');

            // Remove bucket name from path
            if (path.StartsWith(_bucketName + "/"))
            {
                return path.Substring(_bucketName.Length + 1);
            }

            return path;
        }
        catch
        {
            return null;
        }
    }
}