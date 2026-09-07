using Microsoft.AspNetCore.Http;
using MindSync.Application.Common.Interfaces;
using System.Security.Claims;

namespace MindSync.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst(ClaimTypes.NameIdentifier) ?? user?.FindFirst("sub");

            return Guid.TryParse(claim?.Value, out var userId) ? userId : Guid.Empty;
        }
    }
}