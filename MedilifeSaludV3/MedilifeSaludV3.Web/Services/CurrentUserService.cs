using MedilifeSaludV3.Web.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public string GetUsername()
    {
        return _http.HttpContext?.User?.Identity?.Name
               ?? "system";
    }
}