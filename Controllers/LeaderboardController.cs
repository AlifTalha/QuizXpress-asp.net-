using Microsoft.AspNetCore.Mvc;
using QuizBoard.Services;

namespace QuizBoard.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodaysLeaderboard()
        {
            try
            {
                var leaderboard = await _leaderboardService.GetTodaysLeaderboardAsync();
                return PartialView("_TodaysLeaderboard", leaderboard);
            }
            catch (Exception ex)
            {
                // Log the error (you can add proper logging here)
                var errorModel = new QuizBoard.Models.TodaysLeaderboardResponse
                {
                    Leaderboard = new List<QuizBoard.Models.LeaderboardViewModel>(),
                    TotalParticipants = 0,
                    Date = DateTime.Today
                };
                return PartialView("_TodaysLeaderboard", errorModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTodaysLeaderboardData()
        {
            try
            {
                var leaderboard = await _leaderboardService.GetTodaysLeaderboardAsync();
                return Json(new { success = true, data = leaderboard });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}