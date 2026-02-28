using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace ShopScout.SharedLib.Services;

public class StorageService
{
    private readonly IJSRuntime JSRuntime;

    public StorageService(IJSRuntime jsRuntime)
    {
        JSRuntime = jsRuntime;
    }

    public async Task SetValue<TValue>(string key, TValue value)
    {
        await JSRuntime.InvokeVoidAsync("eval", $"localStorage.setItem('{key}', '{JsonSerializer.Serialize(value)}')");
    }

    /// <summary>
    /// Removes all items from the browser's local storage whose keys start with the specified prefix.
    /// </summary>
    /// <remarks>This method executes JavaScript in the browser context to remove matching keys from local
    /// storage. The operation is case-sensitive and only affects keys that begin with the specified prefix.</remarks>
    /// <param name="startsWith">The prefix to match against the beginning of each local storage key. All keys that start with this value will be
    /// removed. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    public async Task RemoveAllStartingWithAsync(string startsWith)
    {
        await JSRuntime.InvokeVoidAsync("eval", $@"
            const keysToRemove = [];
            for (let i = 0; i < localStorage.length; i++) {{
                const key = localStorage.key(i);
                if (key && key.startsWith('{startsWith}')) {{
                    keysToRemove.push(key);
                }}
            }}
            keysToRemove.forEach(key => localStorage.removeItem(key));
        ");
    }

    public async Task<TValue?> GetValue<TValue>(string key, TValue? def)
    {
        try
        {
            var cValue = JsonSerializer.Deserialize<TValue>(await JSRuntime.InvokeAsync<string>("eval", $"localStorage.getItem('{key}');"));
            return cValue ?? def;
        }
        catch
        {
            var cValue = await JSRuntime.InvokeAsync<string>("eval", $"localStorage.getItem('{key}');");
            if (cValue == null)
                return def;

            var newValue = GetValueI<TValue>(cValue);
            return newValue ?? def;
        }
    }

    public static T? GetValueI<T>(string value)
    {
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
        if (converter != null)
        {
            try
            {
                return (T?)converter.ConvertFrom(value);
            }
            catch
            {
                return default;
            }
        }
        return default;
        //return (T)Convert.ChangeType(value, typeof(T));
    }

    public async Task RemoveValue(string key)
    {
        await JSRuntime.InvokeVoidAsync("eval", $"localStorage.removeItem('{key}')");
    }
}
