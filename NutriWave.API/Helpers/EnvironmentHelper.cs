namespace NutriWave.API.Helpers;

public static class EnvironmentHelper
{
    public static string DbConnectionString => GetVariable("DB_CONNECTION_STRING");

    public static string NutritionixApiKey => GetVariable("NUTRITIONIX_API_KEY");

    public static string NutritionixApiUrl => GetVariable("NUTRITIONIX_API_URL");

    public static string NutritionixAppId => GetVariable("NUTRITIONIX_APP_ID");

    public static string RedisConnectionString => GetVariable("REDIS_CONNECTION_STRING");

    public static string AnthropicApiKey => GetVariable("ANTHROPIC_API_KEY");


    private static string GetVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException($"{name} variable not set in the environment.");
    }
}
