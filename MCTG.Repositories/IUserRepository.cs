using MCTG.Models;

namespace MCTG.Repositories;

public interface IUserRepository
{
    ValueTask<User?> GetByUserName(string name);

    ValueTask<User> Create(User user);

    ValueTask<User> Update(User user);
    ValueTask<User?> GetByToken(string token);
}