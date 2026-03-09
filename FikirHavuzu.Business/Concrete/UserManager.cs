using FikirHavuzu.Business.Abstract;
using FikirHavuzu.DataAccess.Abstract;
using FikirHavuzu.Entities;
using FikirHavuzu.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FikirHavuzu.Business.Concrete;

public class UserManager : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly AppDbContext _context;

    public UserManager(IUserRepository userRepository, AppDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public List<User> GetAll()
    {
        return _userRepository.GetAll();
    }

    public User GetById(int id)
    {
        return _userRepository.GetById(id);
    }

    public void Add(User user)
    {
        _userRepository.Add(user);
    }

    public void Update(User user)
    {
        _userRepository.Update(user);
    }

    public void Delete(int id)
    {
        _userRepository.Delete(id);
    }

    public void AssignAuthority(int userId, int authorityId)
    {
        var exists = _context.UserAuthorities.Any(ua => ua.UserId == userId && ua.AuthorityId == authorityId);
        if (!exists)
        {
            _context.UserAuthorities.Add(new UserAuthority { UserId = userId, AuthorityId = authorityId });
            _context.SaveChanges();
        }
    }

    public void RemoveAuthority(int userId, int authorityId)
    {
        var userAuth = _context.UserAuthorities.FirstOrDefault(ua => ua.UserId == userId && ua.AuthorityId == authorityId);
        if (userAuth != null)
        {
            _context.UserAuthorities.Remove(userAuth);
            _context.SaveChanges();
        }
    }

    public bool HasAuthority(int userId, string authorityCode)
    {
        return _context.UserAuthorities
            .Include(ua => ua.Authority)
            .Any(ua => ua.UserId == userId && ua.Authority.Code == authorityCode);
    }

}