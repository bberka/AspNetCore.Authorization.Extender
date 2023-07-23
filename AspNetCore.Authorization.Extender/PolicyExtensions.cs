using System.Collections;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCore.Authorization.Extender;

public static class PolicyExtensions
{
  /// <summary>
  /// Adds policies to the authorization options based on the list of permissions and policy name template.
  /// <br></br>
  /// <br></br>
  /// Policy name template must contain "{PermissionName}" placeholder.
  /// </summary>
  /// <param name="authorizationOptions"></param>
  /// <param name="permissions"></param>
  /// <param name="policyNameTemplate"></param>
  public static void AddRequiredPermissionPolicies(
    this AuthorizationOptions authorizationOptions,
    List<string> permissions,
    string policyNameTemplate = "RequirePermission:{PermissionName}") {
    var containsPermissionNameVariableInTemplate = policyNameTemplate.Contains("{PermissionName}");
    if (!containsPermissionNameVariableInTemplate) {
      throw new ArgumentException("AspNetCore.Authorization.Extender can not build policies: Policy name template must contain {PermissionName} placeholder.");
    }
    var dictionary = new Dictionary<string, string>();
    foreach (var permission in permissions) {
      var policyName = policyNameTemplate.Replace("{PermissionName}", permission);
      dictionary.Add(policyName, permission);
    }
    authorizationOptions.AddRequiredPermissionPolicies(dictionary);
    
  }

  /// <summary>
  /// Adds policies to the authorization options based on the dictionary of policy names and permissions.
  /// </summary>
  /// <param name="authorizationOptions"></param>
  /// <param name="policyNameAndPermissionsDictionary"></param>
  public static void AddRequiredPermissionPolicies(
    this AuthorizationOptions authorizationOptions,
    IDictionary<string,string> policyNameAndPermissionsDictionary) {
    var dictionaryAsObjectKeyValue = new Dictionary<object, object>();
    foreach (var (policyName, permission) in policyNameAndPermissionsDictionary) {
      dictionaryAsObjectKeyValue.Add(policyName, permission);
    }
    authorizationOptions.AddRequiredPermissionPolicies(dictionaryAsObjectKeyValue);
  }

  /// <summary>
  /// Adds policies to the authorization options based on the dictionary of policy names and permissions.
  ///
  /// <br></br>
  /// <br></br>
  /// The dictionary of key value objects are converted to string by ToString().
  /// <br></br>
  /// <br></br>
  /// This method is for allowing enum types as keys and values
  /// </summary>
  /// <param name="authorizationOptions"></param>
  /// <param name="policyNameAndPermissionsDictionary"></param>
  public static void AddRequiredPermissionPolicies(
    this AuthorizationOptions authorizationOptions,
    Dictionary<object, object> policyNameAndPermissionsDictionary) {
    foreach (var (policyNameObj, permission) in policyNameAndPermissionsDictionary) {
      var policyName = policyNameObj.ToString();
      var permissionName = permission.ToString();
      var policyExists = authorizationOptions.GetPolicy(policyName) != null;
      if (policyExists) {
        throw new ArgumentException($"AspNetCore.Authorization.Extender can not build policies: Policy with name {policyName} already exists.");
      }
      var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(ExtClaimTypes.EndPointPermissions, permissionName)
        .Build();
      authorizationOptions.AddPolicy(
        policyNameObj.ToString(),
        policy);
    }
  }
}