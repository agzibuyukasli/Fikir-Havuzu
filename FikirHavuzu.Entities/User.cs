using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace FikirHavuzu.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string RegistrationNumber { get; set; } = null!;
    public string TCIdentityNumber { get; set; } = null!;
    public string Password { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    public List<UserAuthority> UserAuthorities { get; set; } = new();
    public int TotalScore { get; set; } = 0;
    public string Role { get; set; } = "User";
    [NotMapped]
    public string? Rank => Role == "Admin" ? "Yönetici" : TotalScore switch
{
    < 100 => "Yeni Fikirci",
    < 300 => "Kaşif",
    < 500 => "Geliştirici",
    < 1000 => "Kıdemli",
    _ => "Üstad"
};
}
