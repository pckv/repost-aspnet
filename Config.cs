using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RepostAspNet
{
    public interface IConfig
    {
        IEnumerable<ApiResource> Apis { get; }
        IEnumerable<Client> Clients { get; }
        IEnumerable<string> Origins { get; }
        X509Certificate2 SigningCredential { get; }
    }

    public class Config : IConfig
    {
        private readonly List<(LogLevel, string)> _log;

        public Config(IConfiguration configuration, List<(LogLevel, string)> log)
        {
            Configuration = configuration;
            _log = log;
        }

        public IConfiguration Configuration { get; }

        public IEnumerable<ApiResource> Apis => new[]
        {
            // The name of the resource will also be the name of the scope
            new ApiResource("user", "User")
        };

        public IEnumerable<Client> Clients => new[]
        {
            new Client
            {
                ClientId = Configuration["ClientId"] ?? Configuration["CLIENT_ID"] ?? "repost",
                RequireClientSecret = false,
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = {"user"}
            }
        };

        public IEnumerable<string> Origins => (Configuration["Origins"] ?? Configuration["ORIGINS"] ??
            "http://localhost;http://localhost:8080").Split(';');

        public X509Certificate2 SigningCredential
        {
            get
            {
                var path = Configuration["SigningCredential.Path"] ?? Configuration["SIGNING_CREDENTIAL_PATH"];
                if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }

                if (!File.Exists(path))
                {
                    _log.Add((LogLevel.Error, "Provided signing credential does not exist"));
                    return null;
                }

                var password = Configuration["SigningCredential.Password"] ??
                               Configuration["SIGNING_CREDENTIAL_PASSWORD"];
                if (string.IsNullOrWhiteSpace(password))
                {
                    _log.Add((LogLevel.Warning, "Signing credential was provided without password"));
                    return null;
                }

                return new X509Certificate2(path, password);
            }
        }
    }
}