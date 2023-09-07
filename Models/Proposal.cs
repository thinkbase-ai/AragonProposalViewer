namespace AragonProposalViewer.Models
{
    public class Proposal
    {
        public string metadata { get; set; } = string.Empty;
        public int createdAt { get; set; }
        public bool open { get; set; } = false;
        public bool executed { get; set; } = false;
        public string yes { get; set; }
        public string no { get; set; }
        public string abstain { get; set; }
        public string supportThreshold { get; set; }
        public string minVotingPower { get; set; }
        public string castedVotingPower { get; set; }
        public string totalVotingPower { get; set; }
    }
}
