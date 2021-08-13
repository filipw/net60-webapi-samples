var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ContactService>();

var app = builder.Build();

app.MapGet("/contacts", async (ContactService service) => Results.Ok(await service.GetAll()));
app.MapGet("/contacts/{id}", async (ContactService service, int id) => {
    var contact = await service.Get(id);
    return contact is {} ? Results.Ok(contact) : Results.NotFound();
});
app.MapPost("/contacts", async (ContactService service, Contact contact) => {
    await service.Add(contact);
    return Results.StatusCode(201);
});
app.MapDelete("/contacts/{id}", async (ContactService service, int id) => {
    await service.Delete(id);
    return Results.NoContent();
});

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