using QuizBoard.Models;

namespace QuizBoard.Services
{
    public interface IQuizService
    {
        Task<List<Question>> GetAllQuestionsAsync();
        Task<List<Question>> GetRandomQuestionsAsync(int count = 10);
        Task<Question> GetQuestionByIdAsync(int id);
        Task<QuizSession> StartQuizAsync(int userId);
        Task<bool> SubmitAnswerAsync(int userId, int questionId, string selectedAnswer);
        Task<QuizSession> GetQuizSessionAsync(int userId);
        Task<QuizSession> CompleteQuizAsync(int userId);
    }
}