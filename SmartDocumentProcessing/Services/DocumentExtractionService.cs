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
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length > 0)
            {
                var firstLine = lines[0].Trim();

                if (firstLine.Contains("invoice", StringComparison.OrdinalIgnoreCase))
                {
                    document.DocumentType = "Invoice";

                    var parts = firstLine.Split(' ');
                    if (parts.Length > 1)
                        document.DocumentNumber = parts[1];
                }
                else if (firstLine.Contains("purchase order", StringComparison.OrdinalIgnoreCase))
                {
                    document.DocumentType = "Purchase Order";
                }
            }

            var supplierLine = lines.FirstOrDefault(l =>
                l.Trim().StartsWith("supplier", StringComparison.OrdinalIgnoreCase));

            if (supplierLine != null)
            {
                document.SupplierName = supplierLine
                    .Replace("Supplier:", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("Supplier", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
            }

            var totalMatch = Regex.Match(text, @"Total:\s*(\d+([.,]\d+)?)\s*([A-Z]{3})", RegexOptions.IgnoreCase);

            if (totalMatch.Success)
            {
                document.Total = ParseDecimal(totalMatch.Groups[1].Value);
                document.Currency = totalMatch.Groups[3].Value.ToUpper();
            }

            var subtotalMatch = Regex.Match(text, @"Subtotal:\s*(\d+([.,]\d+)?)", RegexOptions.IgnoreCase);
            if (subtotalMatch.Success)
                document.Subtotal = ParseDecimal(subtotalMatch.Groups[1].Value);

            var taxMatch = Regex.Match(text, @"Tax:\s*(\d+([.,]\d+)?)", RegexOptions.IgnoreCase);
            if (taxMatch.Success)
                document.Tax = ParseDecimal(taxMatch.Groups[1].Value);

            if (document.Subtotal == 0 && document.Total > 0)
            {
                document.Subtotal = document.Total - document.Tax;
            }

            var issueMatch = Regex.Match(text, @"Issue Date[:\s]+([0-9\-\.\/]+)", RegexOptions.IgnoreCase);
            if (issueMatch.Success && DateTime.TryParse(issueMatch.Groups[1].Value, out var issueDate))
                document.IssueDate = issueDate;

            var dueMatch = Regex.Match(text, @"Due Date[:\s]+([0-9\-\.\/]+)", RegexOptions.IgnoreCase);
            if (dueMatch.Success && DateTime.TryParse(dueMatch.Groups[1].Value, out var dueDate))
                document.DueDate = dueDate;

            document.Status = "Needs Review";
            return document;
        }

        public Document ExtractFromCsv(string csvContent)
        {
            var document = new Document();

            var lines = csvContent
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .ToList();

            if (lines.Count < 2)
            {
                document.Status = "Needs Review";
                return document;
            }

            var headers = lines[0]
                .Split(',')
                .Select(h => h.Trim().ToLower())
                .ToList();

            int descIndex = headers.IndexOf("desc");
            int qtyIndex = headers.IndexOf("qty");
            int priceIndex = headers.IndexOf("price");
            int totalIndex = headers.IndexOf("total");

            decimal subtotal = 0;

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i]
                    .Split(',')
                    .Select(p => p.Trim())
                    .ToArray();

                if (parts.Length < 4)
                    continue;

                var lineItem = new LineItem();

                if (descIndex >= 0 && descIndex < parts.Length)
                    lineItem.Description = parts[descIndex];

                if (qtyIndex >= 0 && qtyIndex < parts.Length)
                    lineItem.Quantity = ParseDecimal(parts[qtyIndex]);

                if (priceIndex >= 0 && priceIndex < parts.Length)
                    lineItem.UnitPrice = ParseDecimal(parts[priceIndex]);

                if (totalIndex >= 0 && totalIndex < parts.Length)
                    lineItem.Total = ParseDecimal(parts[totalIndex]);

                document.LineItems.Add(lineItem);
                subtotal += lineItem.Total;
            }

            document.DocumentType = "Invoice";
            document.DocumentNumber = $"CSV-{lines[1].GetHashCode()}";
            document.Currency = "EUR";

            document.Subtotal = subtotal;
            document.Tax = 0;
            document.Total = document.Subtotal + document.Tax;

            document.Status = "Needs Review";

            return document;
        }

        private decimal ParseDecimal(string value)
        {
            value = value.Replace(",", ".");

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return 0;
        }
    }
}