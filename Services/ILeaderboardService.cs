using QuizBoard.Models;

namespace QuizBoard.Services
{
    public interface ILeaderboardService
    {
        Task<TodaysLeaderboardResponse> GetTodaysLeaderboardAsync();
        Task<List<LeaderboardViewModel>> GetTopScorersAsync(int count = 10);
    }
}