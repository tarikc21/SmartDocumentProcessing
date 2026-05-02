using SmartDocumentProcessing.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SmartDocumentProcessing.Services
{
    public class DocumentExtractionService
    {
        public Document ExtractFromText(string text)
        {
            var document = new Document();

            // 1. Prva linija (Invoice 3724 ili Invoice TXT-0)
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length > 0)
            {
                var firstLine = lines[0].Trim();

                if (firstLine.Contains("invoice", StringComparison.OrdinalIgnoreCase))
                {
                    document.DocumentType = "Invoice";

                    // pokušaj izvući broj dokumenta
                    var parts = firstLine.Split(' ');
                    if (parts.Length > 1)
                        document.DocumentNumber = parts[1];
                }
            }

            // 2. Supplier (linija koja počinje sa Supplier)
            var supplierLine = lines.FirstOrDefault(l =>
                l.ToLower().StartsWith("supplier"));

            if (supplierLine != null)
            {
                // npr: Supplier Img 0
                document.SupplierName = supplierLine.Replace("Supplier", "").Trim();
            }

            // 3. Total + Currency (Total: 1123 BAM)
            var totalMatch = Regex.Match(text, @"Total:\s*(\d+(\.\d+)?)\s*([A-Z]{3})");

            if (totalMatch.Success)
            {
                document.Total = decimal.Parse(
                    totalMatch.Groups[1].Value,
                    CultureInfo.InvariantCulture);

                document.Currency = totalMatch.Groups[3].Value;
            }

            document.Status = "Needs Review";

            return document;
        }
        public Document ExtractFromCsv(string csvContent)
        {
            var document = new Document();

            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
            {
                document.Status = "Needs Review";
                return document;
            }

            decimal totalSum = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',').Select(p => p.Trim()).ToArray();

                if (parts.Length < 4)
                    continue;

                var lineItem = new LineItem
                {
                    Description = parts[0]
                };

                if (decimal.TryParse(parts[1], out var quantity))
                    lineItem.Quantity = quantity;

                if (decimal.TryParse(parts[2], out var unitPrice))
                    lineItem.UnitPrice = unitPrice;

                if (decimal.TryParse(parts[3], out var lineTotal))
                    lineItem.Total = lineTotal;

                document.LineItems.Add(lineItem);
                totalSum += lineItem.Total;
            }

            document.DocumentType = "Invoice";
            document.DocumentNumber = $"CSV-{Guid.NewGuid().ToString()[..6]}";
            document.Currency = "EUR";
            document.Total = totalSum;
            document.Subtotal = totalSum;
            document.Tax = 0;
            document.Status = "Needs Review";

            return document;
        }
    }
}