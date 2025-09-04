using System;
using System.ComponentModel.DataAnnotations;

namespace QuizBoard.Models
{
    public class UserInfo
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Quiz result fields
        public int? Score { get; set; }                    // Final quiz score
        public int? TimeTaken { get; set; }                // Time taken in seconds
        public DateTime? QuizCompletedDate { get; set; }   // When quiz was completed
    }
}