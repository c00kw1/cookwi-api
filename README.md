# Cookwi .net core Api

![Build](https://github.com/gjdass/cookwi-api/workflows/Build/badge.svg)

Written in C#, using dotnet core framework.

## First steps

Both ways are pretty easy.

### Using Visual Studio 2019 (windows)

Pretty straight forward. Just [download Visual Studio 2019](https://visualstudio.microsoft.com/fr/thank-you-downloading-visual-studio/?sku=Community&rel=16) and [select Core SDK](https://docs.microsoft.com/fr-fr/dotnet/core/install/sdk?pivots=os-windows#install-with-visual-studio) during its installation.

That's it.

`Api.Hosting` is the startup project.

### Using Visual Studio Code

You will have to install a minimal dotnet core environment - [Full documentation here](https://code.visualstudio.com/docs/languages/dotnet).

## Security

[FusionAuth](https://fusionauth.io/docs/v1/tech/) is used as our own SSO.

## Useful links

* [here](https://medium.com/@JohGeoCoder/deploying-a-net-core-2-0-web-application-to-a-production-environment-on-ubuntu-16-04-using-nginx-683b7e831e6) - how to deploy a production / homologation netcore api
