using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class UserAnswer
{
    public int UserAnswerId { get; set; }

    public int? UserQuizId { get; set; }

    public int? QuestionId { get; set; }

    public int? AnswerId { get; set; }

    public virtual Answer? Answer { get; set; }

    public virtual Question? Question { get; set; }

    public virtual UserQuiz? UserQuiz { get; set; }
}
