using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int? QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int OrderNumber { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Quiz? Quiz { get; set; }
}
