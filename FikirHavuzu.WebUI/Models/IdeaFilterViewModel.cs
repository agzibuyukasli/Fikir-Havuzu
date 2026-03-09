using FikirHavuzu.Entities;

public class IdeaFilterViewModel
{
    public string? Title { get; set; }
    public string? Subject { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? UserName { get; set; }

    public List<Idea> Results { get; set; } = new();
}