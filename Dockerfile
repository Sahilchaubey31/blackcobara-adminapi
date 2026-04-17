FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WebApplication1.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV Twilio__AccountSid=AC2a0ce541ee77e2ffb9fb42a9f7d3ae2a
ENV Twilio__AuthToken=489a033b98198caa78dcf5795e617975
ENV Twilio__FromNumber=+12184268340
ENTRYPOINT ["dotnet", "WebApplication1.dll"]
