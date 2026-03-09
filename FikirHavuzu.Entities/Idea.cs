namespace FikirHavuzu.Entities;

public class Idea
{
    public int Id { get; set; }
    public string Title { get; set; } = null!; 
    public string Subject { get; set; } = null!;  
    public string Benefit { get; set; } = null!; 
    public string Description { get; set; } = string.Empty; 
    public string? DocumentPath { get; set; } 
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string? Status { get; set; }
    public string? EvaluationDescription { get; set; }
    public int? Score { get; set; }
    public string Priority { get; set; } = "Orta";
    public virtual ICollection<IdeaEvaluation> Evaluations { get; set; } = new List<IdeaEvaluation>();
    public bool IsApproved { get; set; } = false;
    
}