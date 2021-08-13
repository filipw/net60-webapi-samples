using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;

public static class IdentityServerBuilderTestExtensions
{
    public static IIdentityServerBuilder AddTestConfig(this IIdentityServerBuilder builder) =>
        builder.AddInMemoryClients(new[] { new Client
            {
                ClientId = "client1",
                ClientSecrets =
                {
                    new Secret("secret1".Sha256())
                },
                AllowedGrantTypes = { GrantType.ClientCredentials },
                AllowedScopes = { "contacts.manage" }
            }}).AddInMemoryApiResources(new[]
            {
                new ApiResource("embedded")
                {
                    Scopes = { "contacts.manage" },
                    Enabled = true
                },
            }).AddInMemoryApiScopes(new[] { new ApiScope("contacts.manage") })
            .AddDeveloperSigningCredential();
}