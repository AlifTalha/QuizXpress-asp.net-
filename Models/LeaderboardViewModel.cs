namespace QuizBoard.Models
{
    public class LeaderboardViewModel
    {
        public int Rank { get; set; }
        public string PhoneNumber { get; set; }
        public int Score { get; set; }
        public string TimeTakenFormatted { get; set; }
        public DateTime QuizCompletedDate { get; set; }
    }

    public class TodaysLeaderboardResponse
    {
        public List<LeaderboardViewModel> Leaderboard { get; set; }
        public int TotalParticipants { get; set; }
        public DateTime Date { get; set; }
    }
}