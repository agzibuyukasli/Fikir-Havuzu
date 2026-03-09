namespace FikirHavuzu.WebUI.Models;

public class UserAuthorityViewModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<AuthoritySelection> Authorities { get; set; } = new();
}

public class AuthoritySelection
{
    public int AuthorityId { get; set; }
    public string AuthorityName { get; set; } = string.Empty;
    public string AuthorityCode { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}