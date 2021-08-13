// there is a breaking change https://docs.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/6.0/middleware-new-use-overload
// that prevents us from using "Use" easily

// WebApplication is IEndpointRouteBuilder and can have route mapping made directly on it
// WebHost is in Microsoft.AspNetCore namespace which is not included by default
var a = WebApplication.Create(args);
a.MapGet("/", () => "Hello world");
a.Run();