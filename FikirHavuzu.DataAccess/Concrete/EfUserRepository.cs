using FikirHavuzu.DataAccess.Abstract;
using FikirHavuzu.Entities;

namespace FikirHavuzu.DataAccess.Concrete;

public class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public EfUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public List<User> GetAll() => _context.Users.ToList();

    public User GetById(int id) => _context.Users.Find(id)!;

    public void Update(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user != null)
        {
            user.IsActive = false;
            _context.SaveChanges();
        }
    }
}