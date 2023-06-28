using System.Diagnostics;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCore.Authorization.Extender;

/// <summary>
///     EndPoint authorization filter to select permission to each endpoint for <see cref="HttpContext.User" />.
///     This also checks <see cref="IIdentity.IsAuthenticated"/> value. If it is false, it will not check permissions and return 401.
///     <br />
///     <example>
///         <br />
///         Example: Add ActionCode to <see cref="HttpContext.User" /> claims with
///         <see cref="ExtenderClaimTypes.EndPointPermissions" /> claim type and
///         separate multiple ActionCode with ","
///         <br />
///         <br />
///         It is recommended to use Enum types for ActionCodes and add the intended permissions to
///         <see cref="HttpContext.User" /> claims.
///     </example>
/// </summary>
public class RequirePermissionAttribute : ActionFilterAttribute
{
  private readonly string _permissionName;

  /// <summary>
  ///  Constructor for <see cref="RequirePermissionAttribute" />. Converts given enum to string and use it as ActionCode.
  /// </summary>
  /// <param name="enumPermission"></param>
  /// <exception cref="ArgumentNullException"></exception>
  public RequirePermissionAttribute(Enum enumPermission) {
    _permissionName = enumPermission.ToString();
  }

  /// <summary>
  ///  Constructor for <see cref="RequirePermissionAttribute" />. Use given string as ActionCode.
  /// </summary>
  /// <param name="permissionName"></param>
  /// <exception cref="ArgumentNullException"></exception>
  public RequirePermissionAttribute(string permissionName) {
    _permissionName = permissionName;
    if (string.IsNullOrEmpty(_permissionName))
      throw new ArgumentNullException(nameof(_permissionName));
  }

  public override void OnActionExecuting(ActionExecutingContext actionExecutingContext) {
    if (actionExecutingContext.HttpContext.User.Identity is { IsAuthenticated: false }) {
      actionExecutingContext.Result = new UnauthorizedResult();
      return;
    }
    var hasAllPermissions = actionExecutingContext.HttpContext.User.FindFirst(ExtenderClaimTypes.AllPermissions) != null;
    if (hasAllPermissions) return;
    
    
    var claim = actionExecutingContext.HttpContext.User.FindFirst(ExtenderClaimTypes.EndPointPermissions);
    var endPointPermissionString = claim?.Value;
    if (string.IsNullOrEmpty(endPointPermissionString)) {
      actionExecutingContext.Result = new ForbidResult();
      return;
    }

    var permList = InternalHelper.SplitPermissions(endPointPermissionString);
    var hasAnyPermissions = permList.Length != 0;
    var hasPermission = permList.Contains(_permissionName);
    if (hasAnyPermissions || !hasPermission) {
      actionExecutingContext.Result = new ForbidResult();
      return;
    }
  }
}