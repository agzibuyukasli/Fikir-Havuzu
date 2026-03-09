namespace FikirHavuzu.Entities;

public class IdeaEvaluation
{
    public int Id { get; set; }
    public int IdeaId { get; set; }
    public Idea Idea { get; set; }

    public int EvaluatorUserId { get; set; }
    public User EvaluatorUser { get; set; }

    public int Score { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public DateTime EvaluationDate { get; set; } = DateTime.Now;
}