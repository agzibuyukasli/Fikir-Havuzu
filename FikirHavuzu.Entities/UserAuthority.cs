namespace FikirHavuzu.Entities;

public class UserAuthority
{
    public int UserId { get; set; }
    public User User { get; set; }

    public int AuthorityId { get; set; }
    public Authority Authority { get; set; }
}