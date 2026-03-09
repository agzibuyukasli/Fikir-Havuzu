using FikirHavuzu.Entities;

namespace FikirHavuzu.Business.Abstract;

public interface IIdeaService
{
    void Update(Idea idea);
    List<Idea> GetIdeasWithUser();
    Idea GetById(int id);
    void Add(Idea idea);
}
