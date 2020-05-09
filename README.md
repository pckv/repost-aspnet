# Repost ASP.NET
This is the [ASP.NET Core Web APIs](https://dotnet.microsoft.com/apps/aspnet/apis)
implementation of the Repost API.

## Installation
[.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) is required.

1. Clone the repository:
```bash
git clone https://github.com/pckv/repost-aspnet.git
```

2. Navigate to the `repost-spring` directory and publish the configuration.
```bash
cd repost-fastapi
dotnet publish --configuration Release
```

## Configurations
Configurations are set by environment variables. The following environment variables
can be used for configuring during runtime.

- **REPOST_CLIENT_ID** - The OAuth2 client_id. Default is `repost`
- **REPOST_SIGNING_CREDENTIAL_PATH** - Path to a X509Certificate file used for 
[JSON Web Tokens](https://jwt.io/)
- **ASPNETCORE_URLS** - URL the server should be hosted on. For a public server use
`http://0.0.0.0:8002` (change port if needed)
- **REPOST_DATABASE_CONNECTION_STRING** - An 
[Npgsql Connection String](https://www.connectionstrings.com/npgsql/) for a 
PostgreSQL database
- **REPOST_ORIGINS** - A list of 
[CORS](https://en.wikipedia.org/wiki/Cross-origin_resource_sharing) URLs separated by `;`

## Running the API
The API can now be executed using the compiled binary found in the `bin` folder.
```bash
bin/Release/netcoreapp3.1/publish/RepostAspNet
```
