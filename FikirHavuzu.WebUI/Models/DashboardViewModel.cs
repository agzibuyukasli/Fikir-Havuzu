namespace FikirHavuzu.WebUI.Models;

public class DashboardViewModel
{
    public int TotalIdeas { get; set; }
    public int TotalUsers { get; set; }
    public string TopContributor { get; set; } = string.Empty;
    public string LatestIdeaTitle { get; set; } = string.Empty;
    public DateTime LastIdeaDate { get; set; }
}