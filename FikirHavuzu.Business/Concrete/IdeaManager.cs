using FikirHavuzu.Business.Abstract;
using FikirHavuzu.DataAccess.Abstract;
using FikirHavuzu.Entities;

namespace FikirHavuzu.Business.Concrete;

public class IdeaManager : IIdeaService
{
    private readonly IIdeaRepository _ideaRepository;

    public IdeaManager(IIdeaRepository ideaRepository) => _ideaRepository = ideaRepository;

    public void Add(Idea idea)
    {
        if (string.IsNullOrEmpty(idea.Title)) throw new Exception("Fikir başlığı boş olamaz.");
        _ideaRepository.Add(idea);
    }

    public List<Idea> GetIdeasWithUser() => _ideaRepository.GetAllWithUser();

    public Idea GetById(int id) => _ideaRepository.GetById(id);

    public void Update(Idea idea)
    {
        _ideaRepository.Update(idea);
    }
}