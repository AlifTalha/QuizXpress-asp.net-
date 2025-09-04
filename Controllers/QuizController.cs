using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizBoard.Data;
using QuizBoard.Models;
using QuizBoard.Services;

namespace QuizBoard.Controllers
{
    public class QuizController : Controller
    {
        private readonly IUserService _userService;
        private readonly IQuizService _quizService;
        private readonly ApplicationDbContext _context;

        public QuizController(IUserService userService, IQuizService quizService, ApplicationDbContext context)
        {
            _userService = userService;
            _quizService = quizService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveUser([FromBody] UserInfo user)
        {
            if (ModelState.IsValid)
            {
                // Check daily play limit before saving user
                var dailyPlayCount = await GetDailyPlayCount(user.PhoneNumber);

                if (dailyPlayCount >= 3)
                {
                    return Json(new
                    {
                        success = false,
                        limitExceeded = true,
                        redirectUrl = Url.Action("Sorry")
                    });
                }

                var success = await _userService.SaveUserAsync(user);
                if (success)
                {
                    // Store user ID in session for quiz
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    return Json(new { success = true, redirectUrl = Url.Action("Instruction") });
                }
            }
            return Json(new { success = false, message = "Failed to save user information" });
        }

        // Add this new method to check daily limit before opening modal
        [HttpPost]
        public async Task<IActionResult> CheckDailyLimit([FromBody] CheckLimitRequest request)
        {
            try
            {
                var dailyPlayCount = await GetDailyPlayCount(request.PhoneNumber);
                var canPlay = dailyPlayCount < 3;

                return Json(new
                {
                    success = true,
                    canPlay = canPlay,
                    dailyPlayCount = dailyPlayCount,
                    maxDailyPlays = 3,
                    remainingPlays = canPlay ? (3 - dailyPlayCount) : 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<int> GetDailyPlayCount(string phoneNumber)
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

        public IActionResult Instruction()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StartQuiz()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User session expired" });
            }

            var session = await _quizService.StartQuizAsync(userId.Value);
            return Json(new { success = true, redirectUrl = Url.Action("Quiz") });
        }

        public async Task<IActionResult> Quiz()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var session = await _quizService.GetQuizSessionAsync(userId.Value);
            if (session == null || session.Questions == null || !session.Questions.Any())
            {
                return RedirectToAction("Instruction");
            }

            // Check if quiz is completed
            if (session.CurrentQuestionIndex >= session.Questions.Count)
            {
                return RedirectToAction("Result");
            }

            var currentQuestion = session.Questions[session.CurrentQuestionIndex];

            var viewModel = new
            {
                Question = currentQuestion,
                QuestionNumber = session.CurrentQuestionIndex + 1,
                TotalQuestions = session.Questions.Count,
                Score = session.Score,
                Correct = session.Correct,
                Wrong = session.Wrong,
                TimeRemaining = 20 * 60, // 20 minutes in seconds
                StartTime = session.StartTime,
                TimeTaken = session.TimeTakenFormatted
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User session expired" });
            }

            var success = await _quizService.SubmitAnswerAsync(userId.Value, request.QuestionId, request.SelectedAnswer);

            if (success)
            {
                var session = await _quizService.GetQuizSessionAsync(userId.Value);
                var isCompleted = session.CurrentQuestionIndex >= session.Questions.Count;

                if (isCompleted)
                {
                    // Complete the quiz and save results
                    var completedSession = await _quizService.CompleteQuizAsync(userId.Value);

                    return Json(new
                    {
                        success = true,
                        isCompleted = true,
                        finalScore = completedSession.Score,
                        correct = completedSession.Correct,
                        wrong = completedSession.Wrong,
                        timeTaken = completedSession.TimeTakenFormatted,
                        redirectUrl = Url.Action("Result")
                    });
                }
                else
                {
                    // Get next question data
                    var nextQuestion = session.Questions[session.CurrentQuestionIndex];
                    return Json(new
                    {
                        success = true,
                        isCompleted = false,
                        nextQuestion = new
                        {
                            Id = nextQuestion.Id,
                            QuestionText = nextQuestion.QuestionText,
                            Q1 = nextQuestion.Q1,
                            Q2 = nextQuestion.Q2,
                            Q3 = nextQuestion.Q3,
                            Q4 = nextQuestion.Q4
                        },
                        questionNumber = session.CurrentQuestionIndex + 1,
                        score = session.Score,
                        correct = session.Correct,
                        wrong = session.Wrong,
                        timeTaken = session.TimeTakenFormatted
                    });
                }
            }

            return Json(new { success = false, message = "Failed to submit answer" });
        }

        public async Task<IActionResult> Result()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            // Get user information with quiz results
            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> TimeUp()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User session expired" });
            }

            // Complete quiz due to time up
            var session = await _quizService.CompleteQuizAsync(userId.Value);

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Result"),
                message = "Time's up! Quiz completed automatically."
            });
        }

        // Add Sorry page action
        public IActionResult Sorry()
        {
            return View();
        }

        // Add this method to your existing QuizController.cs
        [HttpGet]
        public async Task<IActionResult> GetFooterStats()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // Get today's completed quizzes
                var todaysQuizzes = await _context.Users
                    .Where(u => u.QuizCompletedDate.HasValue &&
                               u.QuizCompletedDate >= today &&
                               u.QuizCompletedDate < tomorrow &&
                               u.Score.HasValue)
                    .ToListAsync();

                var totalQuizzes = todaysQuizzes.Count;
                var averageScore = totalQuizzes > 0 ? Math.Round(todaysQuizzes.Average(u => u.Score ?? 0), 1) : 0;
                var topScore = totalQuizzes > 0 ? todaysQuizzes.Max(u => u.Score ?? 0) : 0;
                var activeUsers = todaysQuizzes.Count; // Same as total quizzes for today

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        totalQuizzes = totalQuizzes,
                        averageScore = averageScore,
                        topScore = topScore,
                        activeUsers = activeUsers
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    data = new
                    {
                        totalQuizzes = 0,
                        averageScore = 0,
                        topScore = 0,
                        activeUsers = 0
                    }
                });
            }
        }
    }

    public class CheckLimitRequest
    {
        public string PhoneNumber { get; set; }
    }

    public class SubmitAnswerRequest
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
    }
}