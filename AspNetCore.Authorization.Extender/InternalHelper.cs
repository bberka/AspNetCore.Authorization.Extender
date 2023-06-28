namespace AspNetCore.Authorization.Extender;

internal static class InternalHelper
{
  internal const char PermissionSeparator = ',';

  internal static HttpMethod? GetHttpMethod(string permission) {
    var parse = Enum.TryParse(typeof(HttpMethod), permission, out var obj);
    if (!parse) return null;
    return (HttpMethod?)obj;
  }

  internal static string[] SplitPermissions(string permissionString) {
    return permissionString.Split(PermissionSeparator).ToArray();
  }
}