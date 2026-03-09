using FikirHavuzu.DataAccess.Abstract;
using FikirHavuzu.Entities;
using Microsoft.EntityFrameworkCore;

namespace FikirHavuzu.DataAccess.Concrete;

public class EfIdeaRepository : IIdeaRepository
{
    private readonly AppDbContext _context;
    public EfIdeaRepository(AppDbContext context) => _context = context;

    public List<Idea> GetAllWithUser() => _context.Ideas.Include(x => x.User).ToList();

    public Idea GetById(int id) => _context.Ideas.Include(x => x.User).FirstOrDefault(x => x.Id == id)!;

    public void Add(Idea idea)
    {
        _context.Ideas.Add(idea);
        _context.SaveChanges();
    }

    public void Update(Idea idea)
    {
        _context.Ideas.Update(idea);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var idea = _context.Ideas.Find(id);
        if (idea != null)
        {
            _context.Ideas.Remove(idea);
            _context.SaveChanges();
        }
    }
}