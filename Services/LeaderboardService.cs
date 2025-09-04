using Microsoft.EntityFrameworkCore;
using QuizBoard.Data;
using QuizBoard.Models;

namespace QuizBoard.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TodaysLeaderboardResponse> GetTodaysLeaderboardAsync()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var todaysResults = await _context.Users
                    .Where(u => u.QuizCompletedDate.HasValue &&
                               u.QuizCompletedDate >= today &&
                               u.QuizCompletedDate < tomorrow &&
                               u.Score.HasValue &&
                               u.TimeTaken.HasValue)
                    .OrderByDescending(u => u.Score)
                    .ThenBy(u => u.TimeTaken)
                    .ThenBy(u => u.QuizCompletedDate)
                    .ToListAsync();

                var leaderboardItems = todaysResults.Select((u, index) => new LeaderboardViewModel
                {
                    Rank = index + 1,
                    PhoneNumber = u.PhoneNumber ?? "Unknown",
                    Score = u.Score ?? 0,
                    TimeTakenFormatted = FormatTime(u.TimeTaken ?? 0),
                    QuizCompletedDate = u.QuizCompletedDate ?? DateTime.Now
                }).ToList();

                return new TodaysLeaderboardResponse
                {
                    Leaderboard = leaderboardItems,
                    TotalParticipants = leaderboardItems.Count,
                    Date = today
                };
            }
            catch (Exception)
            {
                // Return empty leaderboard on error
                return new TodaysLeaderboardResponse
                {
                    Leaderboard = new List<LeaderboardViewModel>(),
                    TotalParticipants = 0,
                    Date = DateTime.Today
                };
            }
        }

        public async Task<List<LeaderboardViewModel>> GetTopScorersAsync(int count = 10)
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var topScorers = await _context.Users
                    .Where(u => u.QuizCompletedDate.HasValue &&
                               u.QuizCompletedDate >= today &&
                               u.QuizCompletedDate < tomorrow &&
                               u.Score.HasValue &&
                               u.TimeTaken.HasValue)
                    .OrderByDescending(u => u.Score)
                    .ThenBy(u => u.TimeTaken)
                    .Take(count)
                    .ToListAsync();

                return topScorers.Select((u, index) => new LeaderboardViewModel
                {
                    Rank = index + 1,
                    PhoneNumber = u.PhoneNumber ?? "Unknown",
                    Score = u.Score ?? 0,
                    TimeTakenFormatted = FormatTime(u.TimeTaken ?? 0),
                    QuizCompletedDate = u.QuizCompletedDate ?? DateTime.Now
                }).ToList();
            }
            catch (Exception)
            {
                return new List<LeaderboardViewModel>();
            }
        }

        private string FormatTime(int seconds)
        {
            if (seconds <= 0) return "00:00";

            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            return $"{minutes:D2}:{remainingSeconds:D2}";
        }
    }
}