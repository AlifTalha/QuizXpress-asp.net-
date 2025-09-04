using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizBoard.Models
{
    [Table("QuestionTable")]
    public class Question
    {
        public int Id { get; set; }

        [Required]
        [Column("Question")]
        public string QuestionText { get; set; }

        public string Q1 { get; set; }
        public string Q2 { get; set; }
        public string Q3 { get; set; }
        public string Q4 { get; set; }
        public string Ans { get; set; }
    }

    public class QuizResult
    {
        public int UserId { get; set; }
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime AnsweredAt { get; set; }
    }

    public class QuizSession
    {
        public int UserId { get; set; }
        public List<Question> Questions { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int Score { get; set; }
        public int Correct { get; set; }
        public int Wrong { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }  // Added end time tracking
        public List<QuizResult> Results { get; set; }

        // Calculate time taken in seconds
        public int TimeTakenInSeconds
        {
            get
            {
                if (EndTime.HasValue)
                {
                    return (int)(EndTime.Value - StartTime).TotalSeconds;
                }
                return (int)(DateTime.Now - StartTime).TotalSeconds;
            }
        }

        // Format time taken as MM:SS
        public string TimeTakenFormatted
        {
            get
            {
                int totalSeconds = TimeTakenInSeconds;
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
        }
    }
}