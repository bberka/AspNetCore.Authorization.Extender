using System.Security.Claims;

namespace AspNetCore.Authorization.Extender;



public static class ExtenderClaimTypes
{
  /// <summary>
  ///   Claim type for initializing User authorization in order to use <see cref="HttpMethodAuthorizationMiddleware" />.
  ///   <br />
  ///   <br />
  ///   Value must contain <see cref="HttpMethod" /> strings separated with comma ",".
  /// </summary>
  public static string HttpMethodPermissions => "AspNetCore.Authorization.Extender::5b0ea63fdc71f29af99957d9ddd6381d05dbc00d005b71006887485227e52b9b";

  /// <summary>
  ///   Claim type for initializing User authorization in order to use <see cref="RequirePermissionAttribute" />.
  ///   Value must contain permission strings separated with comma ",".
  ///   <br />
  ///   <br />
  ///   It is recommended to have a <see cref="Enum" /> type for defining permissions and add the intended permissions to
  ///   <see cref="ClaimsPrincipal" /> claims
  ///   and then define required permissions in Controller or Action with <see cref="RequirePermissionAttribute" />.
  /// </summary>
  public static string EndPointPermissions => "AspNetCore.Authorization.Extender::46729b745167f7f710b5cdbd244c82c031b8768b9d602e09f8c3b739a0d50327";

  /// <summary>
  ///   Claim type for giving a user all permissions for <see cref="RequirePermissionAttribute" /> and
  ///   <see cref="HttpMethodAuthorizationMiddleware" />.
  ///   Once this claim is set, all other permissions will be ignored and user will be allowed to access all endpoints.
  ///   <br />
  ///   <br />
  ///   <see cref="Claim.Value" /> doesn't matter and can be empty <see cref="string" />.
  /// </summary>
  public static string AllPermissions => "AspNetCore.Authorization.Extender::e20cc70031c73f3ae47a9d0823c5c4a31cc8da26a56693badb9101ccd17199fa";
}