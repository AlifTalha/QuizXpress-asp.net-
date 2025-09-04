using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuizBoard.Data;
using QuizBoard.Models;

namespace QuizBoard.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveUserAsync(UserInfo user)
        {
            try
            {
                // Check if daily limit is exceeded before saving
                var dailyCount = await GetDailyPlayCountAsync(user.PhoneNumber);
                if (dailyCount >= 3)
                {
                    return false; // Daily limit exceeded
                }

                user.CreatedDate = DateTime.Now;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserQuizResultAsync(int userId, int score, int timeTaken, DateTime completedDate)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Score = score;
                    user.TimeTaken = timeTaken;
                    user.QuizCompletedDate = completedDate;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserInfo> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<int> GetDailyPlayCountAsync(string phoneNumber)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var count = await _context.Users
                .Where(u => u.PhoneNumber == phoneNumber &&
                           u.CreatedDate >= today &&
                           u.CreatedDate < tomorrow)
                .CountAsync();

            return count;
        }

        public async Task<List<UserInfo>> GetUserPlayHistoryAsync(string phoneNumber)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.Users
                .Where(u => u.PhoneNumber == phoneNumber &&
                           u.CreatedDate >= today &&
                           u.CreatedDate < tomorrow)
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> CanUserPlayTodayAsync(string phoneNumber)
        {
            var dailyCount = await GetDailyPlayCountAsync(phoneNumber);
            return dailyCount < 3;
        }
    }
}