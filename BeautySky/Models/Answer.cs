using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class Answer
{
    public int AnswerId { get; set; }

    public int? QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public string? SkinTypeId { get; set; }

    public string? Point { get; set; }
    [JsonIgnore]

    public virtual Question? Question { get; set; }
}
