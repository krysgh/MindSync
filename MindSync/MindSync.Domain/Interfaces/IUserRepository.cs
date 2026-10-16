using MindSync.Domain.Entities;
using MindSync.Domain.ValueObjects;

namespace MindSync.Domain.Interfaces;

public interface IUserRepository
{
    Task InsertAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(Email email);
    Task<bool> ExistsByEmailAsync(Email email);
}
