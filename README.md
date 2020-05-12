![Build](https://github.com/gjdass/cookwi-api/workflows/Build/badge.svg)

# Cookwi .net core Api

Written in C#, using dotnet core framework so it is fully x-platform.

## First steps

Both ways are pretty easy.

### Using Visual Studio 2019 (windows)

Pretty straight forward. Just [download Visual Studio 2019](https://visualstudio.microsoft.com/fr/thank-you-downloading-visual-studio/?sku=Community&rel=16) and [select Core SDK](https://docs.microsoft.com/fr-fr/dotnet/core/install/sdk?pivots=os-windows#install-with-visual-studio) during its installation.

That's it.

`Api.Hosting` is the startup project.

### Using Visual Studio Code

You will have to install a minimal dotnet core environment - [Full documentation here](https://code.visualstudio.com/docs/languages/dotnet).

## Security

Authentication (and scope Authorization) are managed by OAuth2 protocol via [Auth0](https://auth0.com) thirdparty. It's very handy and quite effortless when you don't want to code the SSO part.
