using System.Collections.Generic;
using IdentityServer4.Models;

namespace RepostAspNet
{
    public class Config
    {
        public static IEnumerable<ApiResource> Apis => new[]
        {
            new ApiResource("users", "Users")
        };

        public static IEnumerable<Client> Clients => new[]
        {
            new Client
            {
                ClientId = "client",
                ClientSecrets = {new Secret("secret".Sha256())},
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = {"users"}
            }
        };
    }
}