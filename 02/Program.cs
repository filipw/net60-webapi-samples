var a = WebApplication.Create(args);
a.MapGet("/", () => "Hello world!");
a.MapGet("hello/{name}", (string name) => Results.Ok($"Hello, {name}"));
a.Run();