# AspNetCore.Authorization.Extender
Enhance your AspNetCore authorization and authentication with permission based authorization rather than basic role based.
You can set permission for each endpoint or controller and have more control over it.

## Installation
via NuGet
```bash
Install-Package AspNetCore.Authorization.Extender
```
via CLI
```bash
dotnet add package AspNetCore.Authorization.Extender
```

## ClaimTypes
```csharp
// Claim type for endpoint permissions
ExtClaimTypes.EndPointPermissions;
// Claim type for allowing every action, ve careful when using this
// Only recommended to be used in Development environment
ExtClaimTypes.AllPermissions; 
```
## Defining permissions as Enum Type
You can use enum types or strings to define permissions.
It's your preference however it will be more manageable if you use enum types.
```csharp
public enum Permissions
{
    AccessAccountSettings,
    UpdateUser,
    UpdateOtherUsers,
    UpdateUserPassword,
    //... etc.
}
```


## Authentication with Endpoint permissions
RequirePermissionAttribute is used to check if user has permission to access an endpoint. 

This is different from role based authorization because instead of assigning multiple roles to each action to allow only specific roles.
By using this you can set specific permission for each endpoint and assign those to users.

This gives overall more control over authorization and rather than editing controllers to change who accesses which endpoint.

### Implementing permission to an Endpoint
```csharp
//Action filter that checks if user has permission to access endpoint
[RequirePermission(Permissions.User)] 
public class UserController : Controller {
    [RequirePermission(Permissions.AccessAccountSettings)] 
    public IActionResult AccountSettings(){ /* Your code */}

    [AllowAnonymous]
    public IActionResult Login(){ /* Your code */}
}
```
## Recommended usage with Database
Create a Permissions table in your database and always sync this table with your Permissions Enum Type.

Create a Roles table in your database and define roles

Create a RolePermissions table in your database and define which permissions are allowed for each role. (Many to Many relationship)

In your Users table add the role you want to the user.

Once user logins, get all permissions of User's role and create a permission string by using extension methods provided by the library.

Then simply add this to user's claims and you are done.

However keep in mind that once a claim is created it will not be removed until user logs out and logs back in.

## Creating user claims
In the example EndPointPermissions are added to claims.

When authenticated user tries to access an endpoint,
RequirePermissionAttribute will check if user has permission to access endpoint.

If user does not have permission then 403 Forbidden will be returned.

If user is not authenticated then 401 Unauthorized will be returned.

If permission is set in controller and you want to disable permission for a single action/endpoint you can do it by using AllowAnonymousAttribute which is provided by default
```csharp
var endpointPermissions = new List<Permissions>(){
    Permissions.AccessAccountSettings,
    Permissions.UpdateUser,
    Permissions.UpdateUserPassword,
    //... etc.
}; // Get permissions from database
// Create claim list
var claimList = new List<Claim>(); 
// Add permissions to claim list merges list with comma (,) separator
claimList.Add(new Claim(EasMeClaimType.EndPointPermissions, endpointPermissions.CreatePermissionString())); 
//Or use extension method
claimList.AddPermissions(endpointPermissions);

//Sign in user or create JWT token with claims
var identity = new ClaimsIdentity(claimList, "login"); // Create identity
var principal = new ClaimsPrincipal(identity); // Create principal
await HttpContext.SignInAsync(principal); // Sign in user

//.. or create JWT token with claims
```

## Extension Methods
```csharp
// Create permission string from List<string> or List<Enum>
var permissionString = permissions.CreatePermissionString();

//Add permissions to Enumerable<Claim>
var newClaims = claimEnumerable.AddPermissions(permissions); //returns new Claim Enumerable

//Add permissions to List<Claim>
claimList.AddPermissions(permissions);

//Has permission
var hasPermission = HttpContext.User.HasPermission(Permissions.AccessAccountSettings); //bool
//Or
var hasPermission = HttpContext.User.HasPermission("AccessAccountSettings"); //bool

var permissions = HttpContext.User.GetPermissions();//returns List<string> of current users permissions
```