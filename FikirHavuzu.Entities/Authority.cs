namespace FikirHavuzu.Entities;

public class Authority
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; } 
    
    public List<UserAuthority> UserAuthorities { get; set; } = new();
}