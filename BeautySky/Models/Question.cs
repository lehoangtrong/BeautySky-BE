using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int? QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int OrderNumber { get; set; }

    [JsonIgnore]
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    [JsonIgnore]
    public virtual Quiz? Quiz { get; set; }

    [JsonIgnore]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
