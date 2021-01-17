FROM mcr.microsoft.com/dotnet/aspnet:5.0

ARG path_to_binaries="./api-package"
ARG environment="Homologation"

COPY $path_to_binaries/ /var/www/api/

RUN apt-get update && apt-get install -y libc6-dev libgdiplus libx11-dev

EXPOSE 5000

WORKDIR /var/www/api/
ENV ASPNETCORE_ENVIRONMENT $environment
ENV ASPNETCORE_URLS "http://*:5000"
ENTRYPOINT [ "dotnet", "Api.Hosting.dll" ]