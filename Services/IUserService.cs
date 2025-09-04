using System.Threading.Tasks;
using QuizBoard.Models;

namespace QuizBoard.Services
{
    public interface IUserService
    {
        Task<bool> SaveUserAsync(UserInfo user);
        Task<bool> UpdateUserQuizResultAsync(int userId, int score, int timeTaken, DateTime completedDate);
        Task<UserInfo> GetUserByIdAsync(int userId);
        Task<int> GetDailyPlayCountAsync(string phoneNumber);
        Task<List<UserInfo>> GetUserPlayHistoryAsync(string phoneNumber);
        Task<bool> CanUserPlayTodayAsync(string phoneNumber);
    }
}