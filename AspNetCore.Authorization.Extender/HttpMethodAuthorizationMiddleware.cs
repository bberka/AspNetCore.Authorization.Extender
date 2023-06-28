using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Authorization.Extender;

/// <summary>
///   Authorize user by <see cref="HttpMethod" /> permissions defined in <see cref="Claim" />'s with claim type
///   <see cref="ExtenderClaimTypes.HttpMethodPermissions" />
///   <br /><br />
///   This will not check <see cref="IIdentity.IsAuthenticated" /> value. If it is false, it will not do anything.
///   <br /><br />
///   You must add <see cref="HttpMethodAuthorizationMiddleware" /> to <see cref="IApplicationBuilder" /> in Program.cs
/// </summary>
public class HttpMethodAuthorizationMiddleware
{
  private readonly RequestDelegate _next;

  public HttpMethodAuthorizationMiddleware(RequestDelegate next) {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext httpContext) {
    if (httpContext.User.Identity is not { IsAuthenticated: true }) {
      await _next(httpContext);
      return;
    }
    var hasAllPermissions = httpContext.User.FindFirst(ExtenderClaimTypes.AllPermissions) != null;
    if (hasAllPermissions) {
      await _next(httpContext);
      return;
    }
    
    var claim = httpContext.User.FindFirst(ExtenderClaimTypes.HttpMethodPermissions);
    var permissionString = claim?.Value;
    var permList = InternalHelper.SplitPermissions(permissionString ?? string.Empty);
    var hasAnyPermission = permList.Length != 0;
    var httpMethod = httpContext.Request.Method;
    var hasPermission = permList.Contains(httpMethod);
    if (!hasAnyPermission || !hasPermission) {
      httpContext.Response.StatusCode = 403;
      return;
    }

    await _next(httpContext);
  }
}