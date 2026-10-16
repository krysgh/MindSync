using MindSync.Domain.Entities;

namespace MindSync.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}