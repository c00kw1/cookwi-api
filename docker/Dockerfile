FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ARG path_to_binaries="./api-package"
ARG environment="Homologation"

COPY $path_to_binaries/ /var/www/api/

EXPOSE 5000

WORKDIR /var/www/api/
ENV ASPNETCORE_ENVIRONMENT $environment
ENV ASPNETCORE_URLS "http://*:5000"
ENTRYPOINT [ "dotnet", "Api.Hosting.dll" ]