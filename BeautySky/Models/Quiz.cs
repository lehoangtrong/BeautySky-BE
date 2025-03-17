using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public string QuizName { get; set; } = null!;

    public string? Description { get; set; }
    [JsonIgnore]
    public DateTime? DateCreated { get; set; }
    [JsonIgnore]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    [JsonIgnore]
    public virtual ICollection<UserQuiz> UserQuizzes { get; set; } = new List<UserQuiz>();
}
