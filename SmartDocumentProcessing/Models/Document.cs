namespace SmartDocumentProcessing.Models
{
    public class Document
    {
        public int Id { get; set; }

        public string DocumentType { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public string DocumentNumber { get; set; } = "";

        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }

        public string Currency { get; set; } = "";

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        public string Status { get; set; } = "Uploaded";

        public string OriginalFileName { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<LineItem> LineItems { get; set; } = new();
    }
}