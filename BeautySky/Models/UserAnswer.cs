using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class UserAnswer
{
    public int UserAnswerId { get; set; }

    public int? UserQuizId { get; set; }

    public int? QuestionId { get; set; }

    public int? AnswerId { get; set; }

    [JsonIgnore]
    public virtual Answer? Answer { get; set; }

    [JsonIgnore]
    public virtual Question? Question { get; set; }

    [JsonIgnore]
    public virtual UserQuiz? UserQuiz { get; set; }
}
