using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public string QuizName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DateCreated { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<UserQuiz> UserQuizzes { get; set; } = new List<UserQuiz>();
}
