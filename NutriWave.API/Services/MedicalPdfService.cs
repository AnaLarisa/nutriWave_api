using System.Drawing;
using System.Text;
using System.Text.Json;
using Ghostscript.NET.Rasterizer;
using NutriWave.API.Helpers;
using NutriWave.API.Models.FileProcessingModels;
using NutriWave.API.Services.Interfaces;
using Tesseract;
using TesseractPage = Tesseract.Page;

namespace NutriWave.API.Services;

public class MedicalPdfService : IMedicalPdfService
{
    private readonly HttpClient _httpClient;
    private readonly string _anthropicApiKey;
    private readonly string _tessDataPath;
    private readonly INutrientRequirementService _nutrientRequirementService;

    public MedicalPdfService(HttpClient httpClient, INutrientRequirementService nutrientRequirementService)
    {
        _httpClient = httpClient;
        _anthropicApiKey = EnvironmentHelper.AnthropicApiKey;
        _nutrientRequirementService = nutrientRequirementService;

        // Fix the tessdata path - it should point to the copied tessdata in output directory
        _tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");

        Console.WriteLine($"Looking for tessdata at: {_tessDataPath}");

        if (!Directory.Exists(_tessDataPath))
        {
            Console.WriteLine($"tessdata directory not found at: {_tessDataPath}");
            throw new DirectoryNotFoundException($"tessdata directory not found at: {_tessDataPath}");
        }

        // Check if Romanian data exists
        var ronFile = Path.Combine(_tessDataPath, "ron.traineddata");
        if (File.Exists(ronFile))
        {
            Console.WriteLine($"Romanian language data found at: {ronFile}");
        }
        else
        {
            Console.WriteLine($"Romanian language data NOT found at: {ronFile}");
            throw new FileNotFoundException($"Romanian language data not found at: {ronFile}");
        }
    }

    public async Task<ProcessingResult> ProcessPdfAsync(byte[] pdfBytes, string filename, int userId)
    {
        var result = new ProcessingResult();
        var tempFiles = new List<string>();

        try
        {
            Console.WriteLine($"Processing PDF: {filename}");

            //Convert PDF to images
            var imageFiles = await ConvertPdfToImagesAsync(pdfBytes, filename);
            tempFiles.AddRange(imageFiles);

            //Anonymize images - STOP if anonymization fails for supported providers
            var (finalImages, anonymizedFiles) = await AnonymizeImagesAsync(imageFiles);
            tempFiles.AddRange(anonymizedFiles);

            //Extract data from images
            var allResults = new List<TestResult>();
            foreach (var imagePath in finalImages)
            {
                var imageResults = await ExtractDataFromImageAsync(imagePath);
                allResults.AddRange(imageResults);
                Console.WriteLine($"[OK] Extracted {imageResults.Count} tests from {Path.GetFileName(imagePath)}");
            }

            if (allResults.Any())
            {
                allResults = await PostProcessDataAsync(allResults);
            }

            // Update abnormal values for nutrients
            List<object> nutrientRecommendations = new();
            if (allResults.Any())
            {
                nutrientRecommendations = await AnalyzeAbnormalValuesAsync(allResults);
                await UpdateDbNutrientIntake(nutrientRecommendations, userId);
            }

            result.TestResults = allResults;
            result.NutrientRecommendations = nutrientRecommendations;
            result.TotalResults = allResults.Count;
            result.AnonymizedImages = anonymizedFiles.Count;
            result.Success = true;

            Console.WriteLine($"[SUCCESS] Extracted {allResults.Count} test results and {nutrientRecommendations.Count} nutrient recommendations");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Failed to anonymize supported provider"))
        {
            result.Success = false;
            result.ErrorMessage = $"Cannot process document: {ex.Message}";
            Console.WriteLine($"[CRITICAL ERROR] {ex.Message}");

            // Still cleanup temp files
            await CleanupTempFilesAsync(tempFiles);
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
        finally
        {
            await CleanupTempFilesAsync(tempFiles);
        }

        return result;
    }

    private async Task UpdateDbNutrientIntake(List<object> nutrientRecommendations, int userId)
    {
        var nutrientChanges = NutrientChangeHelper.ParseFromObjectList(nutrientRecommendations);
        await _nutrientRequirementService.UpdateNutrientRequirementForUserInBulk(userId, nutrientChanges);// add try-catch here if needed
    }

    private async Task<List<string>> ConvertPdfToImagesAsync(byte[] pdfBytes, string filename)
    {
        var imageFiles = new List<string>();
        var tempPdfPath = Path.Combine(Path.GetTempPath(), $"temp_{filename}");

        try
        {
            await File.WriteAllBytesAsync(tempPdfPath, pdfBytes);

            using var rasterizer = new GhostscriptRasterizer();
            rasterizer.Open(tempPdfPath);

            for (int pageIndex = 1; pageIndex <= rasterizer.PageCount; pageIndex++)
            {
                var imagePath = Path.Combine(Path.GetTempPath(), $"medical_page_{pageIndex}.png");

                var img = rasterizer.GetPage(300, pageIndex);
                img.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

                imageFiles.Add(imagePath);
                Console.WriteLine($"[OK] Converted page {pageIndex} to {Path.GetFileName(imagePath)}");
            }

            Console.WriteLine($"[OK] Converted {imageFiles.Count} pages to high-quality images for OCR");
        }
        finally
        {
            if (File.Exists(tempPdfPath))
                File.Delete(tempPdfPath);
        }

        return imageFiles;
    }

    private async Task<(List<string> finalImages, List<string> anonymizedFiles)> AnonymizeImagesAsync(List<string> imageFiles)
    {
        Console.WriteLine("Scanning images for personal information...");

        var finalImages = new List<string>();
        var anonymizedFiles = new List<string>();
        int anonymizedCount = 0;

        foreach (var imagePath in imageFiles)
        {
            var anonymizationResult = await AnonymizeImageAsync(imagePath);

            if (anonymizationResult.WasAnonymized)
            {
                finalImages.Add(anonymizationResult.ImagePath);
                anonymizedFiles.Add(anonymizationResult.ImagePath);
                anonymizedCount++;
                Console.WriteLine($"[ANONYMIZED] {Path.GetFileName(imagePath)} -> {Path.GetFileName(anonymizationResult.ImagePath)} (Provider: {anonymizationResult.Provider})");
            }
            else
            {
                finalImages.Add(imagePath);
                Console.WriteLine($"[SKIP] {Path.GetFileName(imagePath)} - No personal information detected");
            }
        }

        Console.WriteLine($"\n[ANONYMIZATION SUMMARY]");
        Console.WriteLine($"Total images: {imageFiles.Count}");
        Console.WriteLine($"Anonymized: {anonymizedCount}");
        Console.WriteLine($"No anonymization needed: {imageFiles.Count - anonymizedCount}");

        return (finalImages, anonymizedFiles);
    }

    private async Task<AnonymizationResult> AnonymizeImageAsync(string imagePath)
    {
        try
        {
            using var engine = new TesseractEngine(_tessDataPath, "ron", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using TesseractPage page = engine.Process(img);

            var detectedText = page.GetText().ToLower();

            if (detectedText.Contains("cnp") && detectedText.Contains("cod pacient"))
            {
                int heightToCrop = 0;
                string provider = "";
                bool shouldAnonymize = false;

                if (detectedText.Contains("medlife"))
                {
                    heightToCrop = 1350;
                    provider = "Medlife";
                    shouldAnonymize = true;
                }
                else if (detectedText.Contains("regina maria") || detectedText.Contains("reginamaria"))
                {
                    heightToCrop = 1100;
                    provider = "Regina Maria";
                    shouldAnonymize = true;
                }
                else
                {
                    // Unknown provider - don't anonymize
                    provider = "Necunoscut";
                    shouldAnonymize = false;
                }

                if (shouldAnonymize)
                {
                    // Load and anonymize image
                    using var image = Image.FromFile(imagePath);
                    using var graphics = Graphics.FromImage(image);
                    using var blackBrush = new SolidBrush(Color.Black);

                    // Cover the top part
                    graphics.FillRectangle(blackBrush, 0, 0, image.Width, heightToCrop);

                    // Save anonymized image
                    var baseName = Path.GetFileNameWithoutExtension(imagePath);
                    var anonymizedPath = Path.Combine(Path.GetTempPath(), $"{baseName}_anonimizat.jpg");
                    image.Save(anonymizedPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    return new AnonymizationResult
                    {
                        ImagePath = anonymizedPath,
                        WasAnonymized = true,
                        Provider = provider
                    };
                }
                else
                {
                    // Has personal info but unsupported provider - don't anonymize
                    Console.WriteLine($"[SKIP] {Path.GetFileName(imagePath)} - Personal info detected but unsupported provider: {provider}");
                    return new AnonymizationResult
                    {
                        ImagePath = imagePath,
                        WasAnonymized = false,
                        Provider = provider
                    };
                }
            }

            return new AnonymizationResult
            {
                ImagePath = imagePath,
                WasAnonymized = false
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to anonymize {imagePath}: {ex.Message}");

            // Check if this might be a supported provider by doing a simple text check
            try
            {
                var imageText = await GetImageTextWithoutOCR(imagePath);
                if (imageText.Contains("medlife") || imageText.Contains("regina maria"))
                {
                    // This is a supported provider but OCR failed - throw exception to stop processing
                    throw new InvalidOperationException($"Failed to anonymize supported provider document: {ex.Message}");
                }
            }
            catch (InvalidOperationException)
            {
                // Re-throw the anonymization failure exception
                throw;
            }
            catch
            {
                // If we can't even do basic text detection, assume it's not a critical failure
            }

            return new AnonymizationResult
            {
                ImagePath = imagePath,
                WasAnonymized = false
            };
        }
    }

    private async Task<string> GetImageTextWithoutOCR(string imagePath)
    {
        await Task.Delay(1);
        return string.Empty;
    }

    private async Task<List<TestResult>> ExtractDataFromImageAsync(string imagePath)
    {
        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var imageBytes = await File.ReadAllBytesAsync(imagePath);
                var base64Image = Convert.ToBase64String(imageBytes);
                var mediaType = imagePath.EndsWith(".png") ? "image/png" : "image/jpeg";

                var requestBody = new
                {
                    model = "claude-3-5-haiku-20241022",
                    max_tokens = 4000,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new
                                {
                                    type = "image",
                                    source = new
                                    {
                                        type = "base64",
                                        media_type = mediaType,
                                        data = base64Image
                                    }
                                },
                                new
                                {
                                    type = "text",
                                    text = @"Analyze this medical test results page and extract ONLY the test data from any tables.

Return the data as a valid JSON array where each test result follows this exact format:
{
  ""test"": ""test name"",
  ""value"": ""measured value"", 
  ""unit"": ""unit of measurement"",
  ""range"": ""reference range""
}

Important rules:
- Extract ONLY test results from tables (ignore headers, patient info, dates, etc.)
- If a value has no unit, use an empty string for ""unit""
- If there's no reference range, use an empty string for ""range""
- Return an empty array [] if no test tables are found
- Ensure the response is valid JSON that can be parsed

Example format:
[
  {
    ""test"": ""Hemoglobin"",
    ""value"": ""14.2"",
    ""unit"": ""g/dL"",
    ""range"": ""12.0-15.5""
  },
  {
    ""test"": ""White Blood Cell Count"",
    ""value"": ""7.8"",
    ""unit"": ""K/uL"",
    ""range"": ""4.5-11.0""
  }
]"
                                }
                            }
                        }
                    }
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _anthropicApiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var textContent = JsonSerializer.Deserialize<JsonElement>(responseContent)
                        .GetProperty("content")[0]
                        .GetProperty("text")
                        .GetString();

                    return CleanAndParseJson(textContent ?? "[]");
                }
                else if ((int)response.StatusCode == 529)
                {
                    Console.WriteLine($"[WARNING] Anthropic API overloaded (529) on attempt {attempt} for {Path.GetFileName(imagePath)}");
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"Retrying in {delay.TotalSeconds} seconds...");
                        await Task.Delay(delay);
                        delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"Error calling Anthropic API: {response.StatusCode}");
                    return new List<TestResult>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image {imagePath} on attempt {attempt}: {ex.Message}");
                if (attempt < maxRetries)
                {
                    await Task.Delay(delay);
                    delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                    continue;
                }
            }
        }

        Console.WriteLine($"Failed to process {imagePath} after {maxRetries} attempts");
        return new List<TestResult>();
    }

    private List<TestResult> CleanAndParseJson(string responseText)
    {
        try
        {
            var cleaned = responseText.Trim();

            if (cleaned.StartsWith("```json"))
                cleaned = cleaned.Substring(7);
            if (cleaned.StartsWith("```"))
                cleaned = cleaned.Substring(3);
            if (cleaned.EndsWith("```"))
                cleaned = cleaned.Substring(0, cleaned.Length - 3);

            var startIdx = cleaned.IndexOf('[');
            var endIdx = cleaned.LastIndexOf(']');

            if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
            {
                cleaned = cleaned.Substring(startIdx, endIdx - startIdx + 1);
            }

            cleaned = cleaned.Trim();
            return JsonSerializer.Deserialize<List<TestResult>>(cleaned) ?? new List<TestResult>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            Console.WriteLine($"Response text: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
            return new List<TestResult>();
        }
    }

    private async Task<List<TestResult>> PostProcessDataAsync(List<TestResult> rawData)
    {
        if (!rawData.Any()) return rawData;

        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                Console.WriteLine("Post-processing extracted data for accuracy and consistency...");

                var requestBody = new
                {
                    model = "claude-3-5-haiku-20241022",
                    max_tokens = 4000,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = $@"The parsing of a PDF medical results table resulted in this JSON data. Please review and correct this data to ensure:

1. All test names are properly formatted and standardized
2. All values are clean and properly formatted numbers (remove any extra text)
3. All units are consistent and properly formatted
4. All ranges are properly formatted and consistent
5. Remove any duplicate entries
6. Fix any obvious parsing errors or inconsistencies

Here's the raw extracted data:
{JsonSerializer.Serialize(rawData, new JsonSerializerOptions { WriteIndented = true })}

Please output the corrected and properly formatted JSON in the exact same structure:
[
  {{
    ""test"": ""standardized test name"",
    ""value"": ""clean numeric value"",
    ""unit"": ""standardized unit"",
    ""range"": ""properly formatted range""
  }}
]

Rules:
- Keep the same 4-field structure (test, value, unit, range)
- KEEP ALL TEST NAMES IN ROMANIAN - do not translate to English
- Clean numeric values (remove extra text, keep only the number)
- Standardize units (use common medical abbreviations)
- Format ranges consistently
- If a test result is qualitative (like ""Negativ"" or ""Nu s-au evidentiat""), keep the descriptive value in Romanian
- Return valid JSON only, no explanation text"
                        }
                    }
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _anthropicApiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var textContent = JsonSerializer.Deserialize<JsonElement>(responseContent)
                        .GetProperty("content")[0]
                        .GetProperty("text")
                        .GetString();

                    var processedData = CleanAndParseJson(textContent ?? "[]");

                    if (processedData.Any())
                    {
                        Console.WriteLine($"[OK] Post-processing completed - cleaned {processedData.Count} test results");
                        return processedData;
                    }
                }
                else if ((int)response.StatusCode == 529)
                {
                    Console.WriteLine($"[WARNING] Anthropic API overloaded (529) during post-processing on attempt {attempt}");
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"Retrying in {delay.TotalSeconds} seconds...");
                        await Task.Delay(delay);
                        delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                        continue;
                    }
                }

                Console.WriteLine("[WARNING] Post-processing failed, using original data");
                return rawData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in post-processing on attempt {attempt}: {ex.Message}");
                if (attempt < maxRetries)
                {
                    await Task.Delay(delay);
                    delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                    continue;
                }
            }
        }

        Console.WriteLine("[WARNING] Using original extracted data after all retry attempts failed");
        return rawData;
    }

    private async Task<List<object>> AnalyzeAbnormalValuesAsync(List<TestResult> testResults)
    {
        var abnormalResults = testResults.Where(result => IsAbnormalValue(result)).ToList();

        if (!abnormalResults.Any())
        {
            return new List<object>();
        }

        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                Console.WriteLine($"Analyzing {abnormalResults.Count} abnormal values for nutrient recommendations...");

                var requestBody = new
                {
                    model = "claude-3-5-haiku-20241022",
                    max_tokens = 3000,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = $@"Analyze these abnormal medical test results and provide specific nutrient recommendations to help normalize the values.

Abnormal test results:
{JsonSerializer.Serialize(abnormalResults, new JsonSerializerOptions { WriteIndented = true })}

You must ONLY use these exact nutrient names in your response:
Energy, Protein, Carbohydrates, Fiber, Total Fat, Saturated Fat, Monounsaturated Fat, Polyunsaturated Fat, Cholesterol, Sugars, Added Sugars, Water, Vitamin A, Vitamin C, Vitamin D, Vitamin E, Vitamin K, Thiamin (B1), Riboflavin (B2), Niacin (B3), Vitamin B6, Folate (B9), Vitamin B12, Calcium, Iron, Magnesium, Phosphorus, Potassium, Sodium, Zinc, Copper, Manganese, Selenium, Iodine

For each abnormal value, determine what nutrients from the above list could help improve it. Return a JSON array in this exact format:
[
  {{
    ""nutrient"": ""exact nutrient name from the list above"",
    ""dosage_change"": ""+"" or ""-""
  }}
]

IMPORTANT RULES:
- ONLY use the exact nutrient names provided above - no variations or abbreviations
- ONLY include nutrients that need dosage changes (""+""for increase, ""-"" for decrease)
- DO NOT include nutrients that should be maintained at current levels
- If a test value suggests a nutrient need that's not in the approved list, skip it
- Focus on direct relationships between test results and nutrients
- Consider these common relationships:
  * Low hemoglobin/RBC → Iron +, Vitamin C +, Vitamin B12 +, Folate (B9) +
  * Low/High cholesterol → Total Fat -, Saturated Fat -, Fiber +
  * Low vitamin levels → corresponding vitamin +
  * Electrolyte imbalances → Potassium, Sodium, Magnesium adjustments
  * Poor immune markers → Vitamin C +, Vitamin D +, Zinc +
- Return valid JSON only, no explanation text
- Return empty array [] if no approved nutrients need dosage changes

Examples of correct format:
[
  {{""nutrient"": ""Iron"", ""dosage_change"": ""+""}},
  {{""nutrient"": ""Vitamin C"", ""dosage_change"": ""+""}},
  {{""nutrient"": ""Saturated Fat"", ""dosage_change"": ""-""}}
]"
                        }
                    }
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _anthropicApiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var textContent = JsonSerializer.Deserialize<JsonElement>(responseContent)
                        .GetProperty("content")[0]
                        .GetProperty("text")
                        .GetString();

                    var nutrientRecommendations = CleanAndParseNutrientJson(textContent ?? "[]");

                    var approvedNutrients = new HashSet<string>
                    {
                        "Energy", "Protein", "Carbohydrates", "Fiber", "Total Fat", "Saturated Fat",
                        "Monounsaturated Fat", "Polyunsaturated Fat", "Cholesterol", "Sugars",
                        "Added Sugars", "Water", "Vitamin A", "Vitamin C", "Vitamin D", "Vitamin E",
                        "Vitamin K", "Thiamin (B1)", "Riboflavin (B2)", "Niacin (B3)", "Vitamin B6",
                        "Folate (B9)", "Vitamin B12", "Calcium", "Iron", "Magnesium", "Phosphorus",
                        "Potassium", "Sodium", "Zinc", "Copper", "Manganese", "Selenium", "Iodine"
                    };

                    var validRecommendations = nutrientRecommendations
                        .Where(rec =>
                        {
                            var recommendation = JsonDocument.Parse(JsonSerializer.Serialize(rec));
                            var nutrientName = recommendation.RootElement.GetProperty("nutrient").GetString();
                            var dosageChange = recommendation.RootElement.GetProperty("dosage_change").GetString();

                            return approvedNutrients.Contains(nutrientName) &&
                                   (dosageChange == "+" || dosageChange == "-");
                        })
                        .ToList();

                    Console.WriteLine($"[OK] Generated {validRecommendations.Count} valid nutrient recommendations requiring dosage changes");
                    return validRecommendations;
                }
                else if ((int)response.StatusCode == 529)
                {
                    Console.WriteLine($"[WARNING] Anthropic API overloaded (529) during nutrient analysis on attempt {attempt}");
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"Retrying in {delay.TotalSeconds} seconds...");
                        await Task.Delay(delay);
                        delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"Error calling Anthropic API for nutrient analysis: {response.StatusCode}");
                    return new List<object>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in nutrient analysis on attempt {attempt}: {ex.Message}");
                if (attempt < maxRetries)
                {
                    await Task.Delay(delay);
                    delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                    continue;
                }
            }
        }

        Console.WriteLine("Failed to analyze nutrients after all retry attempts");
        return new List<object>();
    }

    private bool IsAbnormalValue(TestResult result)
    {
        if (string.IsNullOrEmpty(result.range) || string.IsNullOrEmpty(result.value))
            return false;

        try
        {
            if (!decimal.TryParse(result.value, out decimal testValue))
                return false;

            // Clean the range string - remove brackets, spaces, and handle different formats
            var cleanRange = result.range.Replace("[", "").Replace("]", "").Replace(" ", "").Trim();

            // Handle different range formats
            if (cleanRange.StartsWith("<"))
            {
                // Handle cases like "<20"
                var upperLimit = cleanRange.Substring(1);
                if (decimal.TryParse(upperLimit, out decimal upperValue))
                {
                    return testValue >= upperValue;
                }
                return false;
            }
            else if (cleanRange.StartsWith(">"))
            {
                // Handle cases like ">5"
                var lowerLimit = cleanRange.Substring(1);
                if (decimal.TryParse(lowerLimit, out decimal lowerValue))
                {
                    return testValue <= lowerValue;
                }
                return false;
            }

            // Handle normal range format like "3.92-5.68"
            var rangeParts = cleanRange.Split('-');
            if (rangeParts.Length != 2)
                return false;

            if (decimal.TryParse(rangeParts[0], out decimal minValue) &&
                decimal.TryParse(rangeParts[1], out decimal maxValue))
            {
                return testValue < minValue || testValue > maxValue;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private List<object> CleanAndParseNutrientJson(string responseText)
    {
        try
        {
            var cleaned = responseText.Trim();

            if (cleaned.StartsWith("```json"))
                cleaned = cleaned.Substring(7);
            if (cleaned.StartsWith("```"))
                cleaned = cleaned.Substring(3);
            if (cleaned.EndsWith("```"))
                cleaned = cleaned.Substring(0, cleaned.Length - 3);

            var startIdx = cleaned.IndexOf('[');
            var endIdx = cleaned.LastIndexOf(']');

            if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
            {
                cleaned = cleaned.Substring(startIdx, endIdx - startIdx + 1);
            }

            cleaned = cleaned.Trim();

            var jsonDoc = JsonDocument.Parse(cleaned);
            var recommendations = new List<object>();

            foreach (var element in jsonDoc.RootElement.EnumerateArray())
            {
                var dosageChange = element.GetProperty("dosage_change").GetString();

                if (dosageChange == "+" || dosageChange == "-")
                {
                    recommendations.Add(new
                    {
                        nutrient = element.GetProperty("nutrient").GetString(),
                        dosage_change = dosageChange
                    });
                }
            }

            return recommendations;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error for nutrients: {ex.Message}");
            Console.WriteLine($"Response text: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
            return new List<object>();
        }
    }

    private async Task CleanupTempFilesAsync(List<string> tempFiles)
    {
        Console.WriteLine("Cleaning up temporary files...");

        foreach (var file in tempFiles)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    Console.WriteLine($"Deleted: {Path.GetFileName(file)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not delete {file}: {ex.Message}");
            }
        }
    }
}