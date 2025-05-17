using NutriWave.API.Helpers;
using NutriWave.API.Models.NutritionixApiModels;
using System.Text.Json;
using System.Text;

namespace NutriWave.API.Clients;

public class NutritionixClient(HttpClient httpClient) : INutritionixClient
{
    public async Task<NutritionixResponse?> GetFoodInfoAsync(string food)
    {
        var requestUri = new Uri(httpClient.BaseAddress!, Constants.NutritionixFoodInfoApiEndpoint);
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        var body = new
        {
            query = food,
            timezone = TimeZoneInfo.Local.StandardName,
        };
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NutritionixResponse>();

        if (result is { Foods.Count: > 0 })
        {
            ApiMappingHelper.ReplaceAttrIdsWithDbIds(result);
        }

        return result;
    }

    public async Task<NutritionixResponse> GetBarcodeInfo(string barcodeId)
    {
        var requestUri = new Uri(httpClient.BaseAddress!, Constants.NutritionixBarcodeInfoApiEndpoint);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NutritionixResponse>();
        if (result is { Foods.Count: > 0 })
        {
            ApiMappingHelper.ReplaceAttrIdsWithDbIds(result);
        }

        return result;
    }

    public async Task<ExerciseResponse?> GetSportInfoAsync(string sport)
    {
        var requestUri = new Uri(httpClient.BaseAddress!, Constants.NutritionixExerciseEndpoint);
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        var body = new
        {
            query = sport,
            timezone = TimeZoneInfo.Local.StandardName,
        };
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ExerciseResponse>();
    }
}
