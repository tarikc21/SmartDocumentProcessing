namespace SmartDocumentProcessing.Models
{
    public class ValidationIssue
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public Document Document { get; set; } = null!;

        public string Field { get; set; } = "";
        public string Message { get; set; } = "";
        public string Severity { get; set; } = "Error";
    }
}