using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class UserQuiz
{
    public int UserQuizId { get; set; }

    public int? UserId { get; set; }

    public int? QuizId { get; set; }

    public DateTime? DateTaken { get; set; }

    public virtual Quiz? Quiz { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
