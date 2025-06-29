using iTextSharp.text;
using iTextSharp.text.pdf;
using NutriWave.API.Models;
using NutriWave.API.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace NutriWave.API.Services;

public class ReportGeneratorService(IAuthService authService) : IReportGeneratorService
{
    public async Task<byte[]> GeneratePdfReportBytes(int userId, IEnumerable<UserNutrientIntake> nutrientIntakes, IEnumerable<FoodLog> foodLogs,
        DateTime startDate, DateTime endDate)
    {
        using var memoryStream = new MemoryStream();
        var document = new iTextSharp.text.Document(PageSize.A4, 25, 25, 30, 30);
        var writer = PdfWriter.GetInstance(document, memoryStream);

        document.Open();

        // Title
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var title = new Paragraph("Nutrient Intake Report", titleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 20
        };
        document.Add(title);

        // Date range and user info
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
        var userInfo = await authService.GetUserInformationById(userId);
        var userName = userInfo.FirstName + " " + userInfo.LastName;

        document.Add(new Paragraph($"User: {userName}", headerFont));
        document.Add(new Paragraph($"Birth date: {userInfo.BirthDate.Date}", headerFont));
        var age = DateTime.Now.Year - userInfo.BirthDate.Year;
        document.Add(new Paragraph($"Age: {age}", headerFont));
        document.Add(new Paragraph($"Report Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", headerFont));
        document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}", headerFont));
        document.Add(new Paragraph(" "));

        // Nutrient Intakes Section
        var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
        document.Add(new Paragraph("Nutrient Intakes Summary", sectionFont));
        document.Add(new Paragraph(" "));

        // Create table for nutrient intakes
        var intakeTable = new PdfPTable(4) { WidthPercentage = 100 };
        intakeTable.SetWidths([2f, 3f, 2f, 2f]);

        // Table headers
        var cellFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        intakeTable.AddCell(new PdfPCell(new Phrase("Date", cellFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        intakeTable.AddCell(new PdfPCell(new Phrase("Nutrient", cellFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        intakeTable.AddCell(new PdfPCell(new Phrase("Quantity", cellFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        intakeTable.AddCell(new PdfPCell(new Phrase("Unit", cellFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

        // Table data
        var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        foreach (var intake in nutrientIntakes)
        {
            intakeTable.AddCell(new PdfPCell(new Phrase(intake.Date.ToString("yyyy-MM-dd"), dataFont)));
            intakeTable.AddCell(new PdfPCell(new Phrase(intake.Nutrient.Name, dataFont)));
            intakeTable.AddCell(new PdfPCell(new Phrase(intake.Quantity.ToString("F2"), dataFont)));
            intakeTable.AddCell(new PdfPCell(new Phrase(intake.Nutrient.Unit, dataFont)));
        }

        document.Add(intakeTable);
        document.Add(new Paragraph(" "));

        // Food Logs Section
        document.Add(new Paragraph("Food Intake Logs", sectionFont));
        document.Add(new Paragraph(" "));

        var foodTable = new PdfPTable(2) { WidthPercentage = 100 };
        foodTable.SetWidths([2f, 6f]);

        foodTable.AddCell(new PdfPCell(new Phrase("Date", cellFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        foodTable.AddCell(new PdfPCell(new Phrase("Food Description", cellFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

        foreach (var log in foodLogs)
        {
            foodTable.AddCell(new PdfPCell(new Phrase(log.Date.ToString("yyyy-MM-dd"), dataFont)));
            foodTable.AddCell(new PdfPCell(new Phrase(log.Description, dataFont)));
        }

        document.Add(foodTable);
        document.Close();

        return memoryStream.ToArray();
    }


    public async Task<string> GenerateCsvReport(int userId, IEnumerable<UserNutrientIntake> nutrientIntakes,
        IEnumerable<FoodLog> foodLogs,
        DateTime startDate, DateTime endDate)
    {
        var csv = new StringBuilder();
        // CSV Header with metadata
        var userInfo = await authService.GetUserInformationById(userId);
        var userName = userInfo.FirstName + " " + userInfo.LastName;

        csv.AppendLine($"# Nutrient Intake Report");
        csv.AppendLine($"# User: {userName}");
        csv.AppendLine($"Birth date: {userInfo.BirthDate}");
        var age = DateTime.Now.Year - userInfo.BirthDate.Year;
        csv.AppendLine($"Age: {age}");
        csv.AppendLine($"# Report Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        csv.AppendLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
        csv.AppendLine();
        // Nutrient Intakes Section
        csv.AppendLine("NUTRIENT_INTAKES");
        csv.AppendLine("Date,Nutrient_Name,Quantity,Unit");
        foreach (var intake in nutrientIntakes)
        {
            csv.AppendLine($"{intake.Date:yyyy-MM-dd},{EscapeCsvField(intake.Nutrient.Name)},{intake.Quantity:F2},{EscapeCsvField(intake.Nutrient.Unit)}");
        }
        csv.AppendLine();
        // Food Logs Section
        csv.AppendLine("FOOD_LOGS");
        csv.AppendLine("Date,Time,Description");
        foreach (var log in foodLogs)
        {
            csv.AppendLine($"{log.Date:yyyy-MM-dd},{log.Date:HH:mm},{EscapeCsvField(log.DisplayName)}");
        }
        return csv.ToString();
    }

    public string GenerateHl7Report(int userId, string firstName, string lastName, string email,
        IEnumerable<UserNutrientIntake> nutrientIntakes, IEnumerable<FoodLog> foodLogs,
        DateTime startDate, DateTime endDate)
    {
        string Escape(string s) => string.IsNullOrEmpty(s)
            ? ""
            : s.Replace("\\", @"\\").Replace("|", @"\F\").Replace("^", @"\S\")
                .Replace("~", @"\R\").Replace("&", @"\T\");

        string FormatDateTime(DateTime dt) => dt.ToString("yyyyMMddHHmmss");

        var hl7 = new StringBuilder();
        var msgTime = FormatDateTime(DateTime.UtcNow);
        var msgId = $"NUT-{userId}-{msgTime}";

        
        hl7.AppendLine($"MSH|^~\\&|NutritionApp|NutritionFac|ReceivingApp|ReceivingFac|{msgTime}||ORU^R01|{msgId}|P|2.5");// MSH – Message Header
        hl7.AppendLine($"PID|1||{Escape(userId.ToString())}||{Escape(lastName)}^{Escape(firstName)}||");                // PID – Patient Identification
        hl7.AppendLine("ORC|RE|1");                                                                                       // ORC – Order Control
        hl7.AppendLine($"OBR|1|||Nutrition Report|||{FormatDateTime(startDate)}|||{FormatDateTime(endDate)}");            // OBR – Observation Request

        var obxSeq = 1;

        foreach (var intake in nutrientIntakes)                                                                           // OBX – Nutrient intake observations
        {
            var value = intake.Quantity.ToString("0.##", CultureInfo.InvariantCulture);
            var date = FormatDateTime(intake.Date);

            hl7.AppendLine($"OBX|{obxSeq++}|NM|{Escape(intake.Nutrient.Name)}^Nutrient Intake||{value}|{Escape(intake.Nutrient.Unit)}|^|F|||{date}");
        }

        foreach (var log in foodLogs)                                                                                     // OBX – Food logs
        {
            var text = Escape(log.Description);
            var date = FormatDateTime(log.Date);

            hl7.AppendLine($"OBX|{obxSeq++}|TX|FOODLOG^Food Intake Log||{text}|||^|F|||{date}");
        }

        return hl7.ToString();
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
