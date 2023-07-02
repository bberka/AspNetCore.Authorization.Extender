using System.Security;
using System.Security.Claims;

namespace AspNetCore.Authorization.Extender;

public static class AuthorizationExtensions
{
  internal static List<string> SplitPermissions(this string permissionString) {
    return permissionString.Split(InternalHelper.PermissionSeparator).ToList();
  }
  public static string CreatePermissionString<T>(this IEnumerable<T> permissionList) where T : Enum {
    return string.Join(InternalHelper.PermissionSeparator, permissionList);
  }
  public static string CreatePermissionString(this IEnumerable<string> permissionList){
    return string.Join(InternalHelper.PermissionSeparator, permissionList);
  }
  public static bool HasPermission(this ClaimsPrincipal user, Enum permission) {
    return user.HasPermission(permission.ToString());
  }
  public static bool HasPermission<T>(this ClaimsPrincipal user, T permission) where T : Enum {
    return user.HasPermission(permission.ToString());
  }

 
  public static bool HasPermission(this ClaimsPrincipal user, string permission, bool checkAllPermissions = true) {
    var hasAllPermissions = user.FindFirst(ExtClaimTypes.AllPermissions) != null;
    if (hasAllPermissions && checkAllPermissions) {
      return true;
    }
    var claim = user.FindFirst(ExtClaimTypes.EndPointPermissions);
    if (claim is null) return false;
    var permissionString = claim.Value;
    var permList = SplitPermissions(permissionString);
    var hasAnyPermission = permList.Count != 0;
    if (!hasAnyPermission) return false;
    var hasPermission = permList.Contains(permission);
    if (!hasPermission) return false;
    return true;
  }


  public static List<string> GetPermissions(this ClaimsPrincipal user) {
    var claim = user.FindFirst(ExtClaimTypes.EndPointPermissions);
    if (claim is null) return new List<string>();
    var permissionString = claim?.Value;
    if (claim is null) return new List<string>();
    var permList = SplitPermissions(permissionString ?? string.Empty);
    return permList;
  }

  public static void AddPermissions(this List<Claim> claims, IEnumerable<string> permissions) {
    var permissionString = permissions.CreatePermissionString();
    claims.Add(new Claim(ExtClaimTypes.EndPointPermissions, permissionString));
  }
  public static void AddPermissions(this List<Claim> claims, IEnumerable<Enum> permissions) {
    var permissionString = permissions.CreatePermissionString();
    claims.Add(new Claim(ExtClaimTypes.EndPointPermissions, permissionString));
  }


}