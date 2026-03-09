using FikirHavuzu.Entities;

namespace FikirHavuzu.Business.Abstract;

public interface IUserService
{
    List<User> GetAll();
    User GetById(int id);
    void Add(User user);
    void Update(User user);
    void Delete(int id);
    void AssignAuthority(int userId, int authorityId);
    void RemoveAuthority(int userId, int authorityId);
    bool HasAuthority(int userId, string authorityCode);
}