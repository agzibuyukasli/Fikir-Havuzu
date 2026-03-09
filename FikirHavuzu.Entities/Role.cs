using System.Collections.Generic;

namespace FikirHavuzu.Entities;

public class Role
{
    public int Id { get; set; }
    public string RoleName { get; set; } = null!;
    
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}