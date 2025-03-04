using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class UserQuiz
{
    public int UserQuizId { get; set; }

    public int? UserId { get; set; }

    public int? QuizId { get; set; }

    public DateTime? DateTaken { get; set; }
    [JsonIgnore]
    public virtual Quiz? Quiz { get; set; }
    [JsonIgnore]
    public virtual User? User { get; set; }
    [JsonIgnore]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
