using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services;

public class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task SaveFoodNutrients(GetInfoRequest request, Dictionary<int, float> apiNutrients)
    {
        var cacheKey = GetCacheKey(request);
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.Today.AddDays(1) // Expires at midnight
        };
        var json = JsonSerializer.Serialize(apiNutrients);

        await cache.SetStringAsync(cacheKey, json, cacheEntryOptions);
    }
    public async Task<Dictionary<int, float>?> GetFoodNutrients(GetInfoRequest request)
    {
        var cacheKey = GetCacheKey(request);
        var json = await cache.GetStringAsync(cacheKey);
        return json is null ? null : JsonSerializer.Deserialize<Dictionary<int, float>>(json);
    }

    public async Task RemoveFoodNutrients(GetInfoRequest request)
    {
        var cacheKey = GetCacheKey(request);
        await cache.RemoveAsync(cacheKey);
    }

    private static string GetCacheKey(GetInfoRequest request)
    {
        return $"{request.UserId}_{request.Description}_{DateTime.Today:yyyyMMdd}";
    }
}
