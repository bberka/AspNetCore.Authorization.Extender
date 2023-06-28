# AspNetCore.Authorization.Extender
Enhance your AspNetCore authorization and authentication with permission based authorization rather than basic role based.
There are 2 options in this library. 
- You can set permissions for specific http method to each user.
- You can set permission for each endpoint or controller.

**WARNING:** HttpMethodAuthorizationMiddleware do not force IIdentity.IsAuthenticated to be true. 
In order to make it work you must use default [Authorize] attribute in your controllers or actions.

### ClaimTypes
```csharp
ExtenderClaimTypes.HttpMethodPermissions; // Claim type for http method permissions
ExtenderClaimTypes.EndPointPermissions; // Claim type for endpoint permissions
ExtenderClaimTypes.AllPermissions; // Claim type for allowing all. Be careful when using this
```
### Defining permissions as Enum Type
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


### Authentication with Endpoint permissions
RequirePermissionAttribute attribute is used to check if user has permission to access an endpoint. 

This is different from role based authorization because instead of assigning multiple roles to each action to allow only specific roles.
By using this you can set specific permission for each endpoint and assign those to users.
This gives overall more control over you and rather than editing controllers to change who accesses which endpoint.

You can assign a role group to a user and then define which permissions are allowed for each role group in your database.
This way when user logins you take all the permissions in users role group and add as claim and the library will do the rest.
This way when you want to remove a permission from a user you just remove it from the role group and it will be removed from user.

However keep in mind that once a claim is created it will not be removed until user logs out and logs back in.

#### Implementing permission to an Endpoint
```csharp
//Action filter that checks if user has permission to access endpoint
[RequirePermission(Permissions.AccessAccountSettings)] 
[HttpGet] // or something else does not matter
public IActionResult AccountSettings(){ /* Your code */}
```

### Implementing HttpMethod permission filter
With this you can only allow specific http methods to be used by a user. 
Middleware will apply to all endpoints and will check if user has permission to use http method.
This may not be optimal for all cases however there are some scenarios where this might be needed.

#### Add Middleware to Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);
//Other configurations
var app = builder.Build();
//Other configurations
app.UseMiddleware<HttpMethodAuthorizationMiddleware>();
```
### Creating user claims
In the example both EndPointPermissions and HttpMethodPermissions are added to claims.
You can use both at same time. Or you can use only one of them.


#### Using EndPointPermissions
When authenticated user tries to access an endpoint,
RequirePermissionAttribute will check if user has permission to access endpoint.
If user does not have permission then 403 Forbidden will be returned.


#### Using HttpMethodPermissions:
Once user is authenticated and tries to access an endpoint HttpMethodAuthorizationMiddleware checks has permission for HttpMethod.

```csharp
var httpMethodPermissions = new List<string>(){
    HttpMethod.GET.ToString(),
    HttpMethod.POST.ToString(),
    //... etc.
}; // Get permissions from database
var endpointPermissions = new List<string>(){
    Permissions.AccessAccountSettings.ToString(),
    Permissions.UpdateUser.ToString(),
    Permissions.UpdateUserPassword.ToString(),
    //... etc.
}; // Get permissions from database
var claimList = new List<Claim>(); // Create claim list
claimList.Add(new Claim(EasMeClaimType.HttpMethodPermissions, string.Join(",",httpMethodPermissions))); // Add permissions to claim list
claimList.Add(new Claim(EasMeClaimType.EndPointPermissions, string.Join(",",endpointPermissions))); // Add permissions to claim list

//Sign in user or create JWT token with claims
var identity = new ClaimsIdentity(claimList, "login"); // Create identity
var principal = new ClaimsPrincipal(identity); // Create principal
await HttpContext.SignInAsync(principal); // Sign in user
```