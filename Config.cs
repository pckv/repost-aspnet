using System.Collections.Generic;
using IdentityServer4.Models;

namespace RepostAspNet
{
    public class Config
    {
        public static IEnumerable<ApiResource> Apis => new[]
        {
            // The name of the resource will also be the name of the scope
            new ApiResource("user", "User")
        };

        public static IEnumerable<Client> Clients => new[]
        {
            new Client
            {
                ClientId = "client",
                ClientSecrets = {new Secret("secret".Sha256())},
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = {"user"}
            }
        };
    }
}