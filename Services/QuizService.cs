using Microsoft.EntityFrameworkCore;
using QuizBoard.Data;
using QuizBoard.Models;
using QuizBoard.Services;

namespace QuizBoard.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private static Dictionary<int, QuizSession> _activeSessions = new Dictionary<int, QuizSession>();

        public QuizService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            return await _context.QuestionTable.ToListAsync();
        }

        // New method to get random 10 questions
        public async Task<List<Question>> GetRandomQuestionsAsync(int count = 10)
        {
            var allQuestions = await _context.QuestionTable.ToListAsync();

            if (allQuestions.Count <= count)
            {
                // If total questions are less than or equal to requested count, return all
                return allQuestions.OrderBy(x => Guid.NewGuid()).ToList();
            }

            // Select random questions using GUID ordering for better randomness
            return allQuestions.OrderBy(x => Guid.NewGuid()).Take(count).ToList();
        }

        public async Task<Question> GetQuestionByIdAsync(int id)
        {
            return await _context.QuestionTable.FindAsync(id);
        }

        public async Task<QuizSession> StartQuizAsync(int userId)
        {
            // Get only 10 random questions instead of all questions
            var questions = await GetRandomQuestionsAsync(10);

            var session = new QuizSession
            {
                UserId = userId,
                Questions = questions,
                CurrentQuestionIndex = 0,
                Score = 0,
                Correct = 0,
                Wrong = 0,
                StartTime = DateTime.Now,
                EndTime = null,
                Results = new List<QuizResult>()
            };

            _activeSessions[userId] = session;
            return session;
        }

        public async Task<bool> SubmitAnswerAsync(int userId, int questionId, string selectedAnswer)
        {
            if (!_activeSessions.ContainsKey(userId))
                return false;

            var session = _activeSessions[userId];
            var question = await GetQuestionByIdAsync(questionId);

            if (question == null)
                return false;

            // Check if the selected answer matches any of the options and then compare with correct answer
            string correctAnswerText = "";
            bool isCorrect = false;

            // Get the correct answer text based on the Ans column value
            switch (question.Ans.ToUpper())
            {
                case "Q1":
                    correctAnswerText = question.Q1;
                    break;
                case "Q2":
                    correctAnswerText = question.Q2;
                    break;
                case "Q3":
                    correctAnswerText = question.Q3;
                    break;
                case "Q4":
                    correctAnswerText = question.Q4;
                    break;
                default:
                    // If Ans contains the actual text instead of Q1,Q2,Q3,Q4
                    correctAnswerText = question.Ans;
                    break;
            }

            // Check if selected answer matches the correct answer
            isCorrect = selectedAnswer.Equals(correctAnswerText, StringComparison.OrdinalIgnoreCase);

            var result = new QuizResult
            {
                UserId = userId,
                QuestionId = questionId,
                SelectedAnswer = selectedAnswer,
                IsCorrect = isCorrect,
                AnsweredAt = DateTime.Now
            };

            session.Results.Add(result);

            if (isCorrect)
            {
                session.Correct++;
                session.Score++;
            }
            else
            {
                session.Wrong++;
            }

            session.CurrentQuestionIndex++;
            return true;
        }

        public async Task<QuizSession> GetQuizSessionAsync(int userId)
        {
            return _activeSessions.ContainsKey(userId) ? _activeSessions[userId] : null;
        }

        public async Task<QuizSession> CompleteQuizAsync(int userId)
        {
            if (_activeSessions.ContainsKey(userId))
            {
                var session = _activeSessions[userId];
                session.EndTime = DateTime.Now;

                // Calculate time taken in seconds
                int timeTaken = session.TimeTakenInSeconds;

                // Update user record with quiz results
                await _userService.UpdateUserQuizResultAsync(
                    userId,
                    session.Score,
                    timeTaken,
                    session.EndTime.Value
                );

                // Remove from active sessions
                _activeSessions.Remove(userId);

                return session;
            }
            return null;
        }
    }
}