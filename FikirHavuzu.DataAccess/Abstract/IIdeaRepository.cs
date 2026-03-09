using FikirHavuzu.Entities;

namespace FikirHavuzu.DataAccess.Abstract;

public interface IIdeaRepository
{
    List<Idea> GetAllWithUser();
    Idea GetById(int id);
    void Add(Idea idea);
    void Update(Idea idea);
    void Delete(int id);
}