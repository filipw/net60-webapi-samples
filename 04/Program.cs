using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddSingleton<ContactService>()
    .AddIdentityServer().AddTestConfig().Services
    .AddAuthorization(options =>
    {
        options.AddPolicy("contacts.manage", 
            p => p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().RequireClaim("scope", "contacts.manage"));
    })
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Authority = "http://localhost:5000/openid";
        o.Audience = "embedded";
        o.RequireHttpsMetadata = false;
    });
var app = builder.Build();

// The call is ambiguous between the following methods or properties: 'Microsoft.AspNetCore.Builder.MapExtensions.Map(Microsoft.AspNetCore.Builder.IApplicationBuilder, Microsoft.AspNetCore.Http.PathString, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder>)' and 'Microsoft.AspNetCore.Builder.MinimalActionEndpointRouteBuilderExtensions.Map(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder, string, System.Delegate)'
app.Map("/openid", false, id =>
{
    // use embedded identity server to issue tokens
    id.UseIdentityServer();
});

app.UseAuthentication().UseAuthorization();

app.MapGet("/contacts", async (ContactService service) => Results.Ok(await service.GetAll()));

// limited by https://github.com/dotnet/aspnetcore/issues/35314
app.MapGet("/contacts/{id}", async (ContactService service, int id) => {
    var contact = await service.Get(id);
    return contact is {} ? Results.Ok(contact) : Results.NotFound();
}).WithMetadata(new RouteNameMetadata("ContactById"));

app.MapPost("/contacts", [Authorize("contacts.manage")] async (ContactService service, Contact contact) => {
    var id = await service.Add(contact);
    return Results.CreatedAtRoute("ContactById", new { id });
});

app.MapDelete("/contacts/{id}", [Authorize("contacts.manage")] async (ContactService service, int id) => {
    await service.Delete(id);
    return Results.NoContent();
});

// binding of ClaimsPrincipal is supported
// + different way of expressing that authorization is required 
app.MapGet("/current-user", (ClaimsPrincipal principal) => Results.Ok(principal.Claims.ToDictionary(c => c.Type, c => c.Value))).RequireAuthorization();

app.Run();

public record Contact(int ContactId, string Name, string Address, string City);

public class ContactService
{
    private readonly List<Contact> _contacts = new()
    {
            new Contact(1, "Filip W", "Bahnhofstrasse 1", "Zurich"),
            new Contact(2, "Josh Donaldson", "1 Blue Jays Way", "Toronto"),
            new Contact(3, "Aaron Sanchez", "1 Blue Jays Way", "Toronto"),
            new Contact(4, "Jose Bautista", "1 Blue Jays Way", "Toronto"),
            new Contact(5, "Edwin Encarnacion", "1 Blue Jays Way", "Toronto")
        };

    public Task<IEnumerable<Contact>> GetAll() => Task.FromResult(_contacts.AsEnumerable());

    public Task<Contact?> Get(int id) => Task.FromResult(_contacts.FirstOrDefault(x => x.ContactId == id));

    public Task<int> Add(Contact contact)
    {
        var newId = (_contacts.LastOrDefault()?.ContactId ?? 0) + 1;
        _contacts.Add(contact with { ContactId = newId });
        return Task.FromResult(newId);
    }

    public async Task Delete(int id)
    {
        var contact = await Get(id);
        if (contact == null)
        {
            throw new InvalidOperationException(string.Format("Contact with id '{0}' does not exists", id));
        }

        _contacts.Remove(contact);
    }
}