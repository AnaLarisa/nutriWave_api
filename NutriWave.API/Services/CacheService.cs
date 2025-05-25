using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Services;

public class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task SaveFoodNutrients(InfoRequest request, Dictionary<int, float> apiNutrients)
    {
        var cacheKey = GetNutritionCacheKey(request);
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.Today.AddDays(1) // Expires at midnight
        };
        var json = JsonSerializer.Serialize(apiNutrients);

        await cache.SetStringAsync(cacheKey, json, cacheEntryOptions);
    }
    public async Task<Dictionary<int, float>?> GetFoodNutrients(InfoRequest request)
    {
        var cacheKey = GetNutritionCacheKey(request);
        var json = await cache.GetStringAsync(cacheKey);
        return json is null ? null : JsonSerializer.Deserialize<Dictionary<int, float>>(json);
    }

    public async Task RemoveFoodNutrients(InfoRequest request)
    {
        var cacheKey = GetNutritionCacheKey(request);
        await cache.RemoveAsync(cacheKey);
    }

    public async Task SaveSportInfo(InfoRequest request, IList<SportUsefulData> sportInfo)
    {
        var cacheKey = GetSportCacheKey(request);
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.Today.AddDays(1) // Expires at midnight
        };
        var json = JsonSerializer.Serialize(sportInfo);

        await cache.SetStringAsync(cacheKey, json, cacheEntryOptions);
    }

    public async Task<IList<SportUsefulData>?> GetSportInfo(InfoRequest request)
    {
        var cacheKey = GetSportCacheKey(request);
        var json = await cache.GetStringAsync(cacheKey);
        return json is null ? null : JsonSerializer.Deserialize<IList<SportUsefulData>>(json);
    }

    public async Task RemoveSportInfo(InfoRequest request)
    {
        var cacheKey = GetSportCacheKey(request);
        await cache.RemoveAsync(cacheKey);
    }

    private static string GetNutritionCacheKey(InfoRequest request)
    {
        return $"Nutrients_{request.UserId}_{request.Description}_{DateTime.Today:yyyyMMdd}";
    }

    private static string GetSportCacheKey(InfoRequest request)
    {
        return $"Sport_{request.UserId}_sport_{request.Description}_{DateTime.Today:yyyyMMdd}";
    }
}
