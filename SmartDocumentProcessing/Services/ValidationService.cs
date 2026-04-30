using Microsoft.EntityFrameworkCore;
using SmartDocumentProcessing.Data;
using SmartDocumentProcessing.Models;

namespace SmartDocumentProcessing.Services
{
    public class ValidationService
    {
        private readonly AppDbContext _context;

        public ValidationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ValidationIssue>> ValidateDocumentAsync(Document document)
        {
            var issues = new List<ValidationIssue>();

            if (string.IsNullOrWhiteSpace(document.SupplierName))
            {
                issues.Add(CreateIssue(document.Id, "SupplierName", "Supplier name is missing."));
            }

            if (string.IsNullOrWhiteSpace(document.DocumentNumber))
            {
                issues.Add(CreateIssue(document.Id, "DocumentNumber", "Document number is missing."));
            }

            if (string.IsNullOrWhiteSpace(document.Currency))
            {
                issues.Add(CreateIssue(document.Id, "Currency", "Currency is missing."));
            }

            if (document.Total <= 0)
            {
                issues.Add(CreateIssue(document.Id, "Total", "Total amount is missing or invalid."));
            }

            if (document.DueDate.HasValue && document.IssueDate.HasValue &&
                document.DueDate.Value < document.IssueDate.Value)
            {
                issues.Add(CreateIssue(document.Id, "DueDate", "Due date cannot be before issue date."));
            }

            var duplicateExists = await _context.Documents
                .AnyAsync(d => d.Id != document.Id &&
                               d.DocumentNumber == document.DocumentNumber &&
                               !string.IsNullOrWhiteSpace(document.DocumentNumber));

            if (duplicateExists)
            {
                issues.Add(CreateIssue(document.Id, "DocumentNumber", "Duplicate document number detected."));
            }

            return issues;
        }

        private ValidationIssue CreateIssue(int documentId, string field, string message)
        {
            return new ValidationIssue
            {
                DocumentId = documentId,
                Field = field,
                Message = message,
                Severity = "Error"
            };
        }
    }
}