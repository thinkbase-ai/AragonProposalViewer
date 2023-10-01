namespace AragonProposalViewer.Models
{
    public class ProposalView
    {
        public enum Status { open, succeeded, failed, executed, pending }
        public string title { get; set; }
        public string summary { get; set; }
        public List<Document> resources { get; set; }
        public DateTime createdAt { get; set; }
        public Status status { get; set; }
    }
}
