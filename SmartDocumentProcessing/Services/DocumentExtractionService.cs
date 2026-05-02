using SmartDocumentProcessing.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace SmartDocumentProcessing.Services
{
    public class DocumentExtractionService
    {
        public Document ExtractFromText(string text)
        {
            var document = new Document();

            var normalizedText = text
                .Replace("\r", "\n")
                .Replace("Supplier:", "\nSupplier:", StringComparison.OrdinalIgnoreCase)
                .Replace("Number:", "\nNumber:", StringComparison.OrdinalIgnoreCase)
                .Replace("Date:", "\nDate:", StringComparison.OrdinalIgnoreCase)
                .Replace("Subtotal", "\nSubtotal", StringComparison.OrdinalIgnoreCase)
                .Replace("Tax", "\nTax", StringComparison.OrdinalIgnoreCase);
                

            var lines = normalizedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (normalizedText.Contains("purchase order", StringComparison.OrdinalIgnoreCase))
                document.DocumentType = "Purchase Order";
            else if (normalizedText.Contains("invoice", StringComparison.OrdinalIgnoreCase))
                document.DocumentType = "Invoice";
            else
                document.DocumentType = "Unknown";

            var supplierMatch = Regex.Match(normalizedText, @"Supplier:\s*(.+)", RegexOptions.IgnoreCase);
            if (supplierMatch.Success)
                document.SupplierName = supplierMatch.Groups[1].Value.Trim();

            var numberMatch = Regex.Match(normalizedText, @"Number:\s*([A-Z0-9\-]+)", RegexOptions.IgnoreCase);
            if (numberMatch.Success)
                document.DocumentNumber = numberMatch.Groups[1].Value.Trim();
            
            if (string.IsNullOrWhiteSpace(document.DocumentNumber))
            {
                var firstLineMatch = Regex.Match(normalizedText, @"Invoice\s+([A-Z0-9\-]+)", RegexOptions.IgnoreCase);

                if (firstLineMatch.Success)
                {
                    document.DocumentNumber = firstLineMatch.Groups[1].Value.Trim();
                }
            }

            var dateMatch = Regex.Match(normalizedText, @"Date:\s*([0-9\-\.\/]+)", RegexOptions.IgnoreCase);
            if (dateMatch.Success && DateTime.TryParse(dateMatch.Groups[1].Value, out var parsedDate))
                document.IssueDate = parsedDate;

            var issueMatch = Regex.Match(normalizedText, @"Issue Date[:\s]+([0-9\-\.\/]+)", RegexOptions.IgnoreCase);
            if (issueMatch.Success && DateTime.TryParse(issueMatch.Groups[1].Value, out var issueDate))
                document.IssueDate = issueDate;

            var dueMatch = Regex.Match(normalizedText, @"Due Date[:\s]+([0-9\-\.\/]+)", RegexOptions.IgnoreCase);
            if (dueMatch.Success && DateTime.TryParse(dueMatch.Groups[1].Value, out var dueDate))
                document.DueDate = dueDate;

            var subtotalMatch = Regex.Match(normalizedText, @"Subtotal\s*:?\s*(\d+([.,]\d+)?)", RegexOptions.IgnoreCase);
            if (subtotalMatch.Success)
                document.Subtotal = ParseDecimal(subtotalMatch.Groups[1].Value);

            var taxMatch = Regex.Match(normalizedText, @"Tax\s*(\(\d+%\))?\s*:?\s*(\d+([.,]\d+)?)", RegexOptions.IgnoreCase);
            if (taxMatch.Success)
                document.Tax = ParseDecimal(taxMatch.Groups[2].Value);

            var totalMatches = Regex.Matches(normalizedText, @"Total\s*:?\s*(\d+([.,]\d+)?)", RegexOptions.IgnoreCase);
            if (totalMatches.Count > 0)
            {
                var lastTotal = totalMatches[totalMatches.Count - 1];
                document.Total = ParseDecimal(lastTotal.Groups[1].Value);
            }

            document.Currency = "EUR";

            if (document.Subtotal == 0 && document.Total > 0)
                document.Subtotal = document.Total - document.Tax;

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
        public Document ExtractFromPdf(string filePath)
        {
            var text = "";

            using (var pdf = PdfDocument.Open(filePath))
            {
                foreach (var page in pdf.GetPages())
                {
                    text += page.Text + Environment.NewLine;
                }
            }

            return ExtractFromText(text);
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