using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Models
{
    public class TicketFilter
    {
        public int? CreatedBy { get; set; }
        public int? AssignedTo { get; set; }
        public bool IncludeAll { get; set; }
        public string SearchTerm { get; set; }
        public TicketStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CategoryId { get; set; }
    }
}