using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class UserAnswer
{
    public int UserAnswerId { get; set; }

    public int? UserQuizId { get; set; }

    public string? QuestionId { get; set; }

    public string? AnswerId { get; set; }

    public int? SkinTypeId { get; set; }
    [JsonIgnore]
    public virtual SkinType? SkinType { get; set; }
    [JsonIgnore]
    public virtual UserQuiz? UserQuiz { get; set; }
}
