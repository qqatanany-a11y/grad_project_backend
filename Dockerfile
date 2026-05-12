FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["events/events.csproj", "events/"]
COPY ["Event.Application/Event.Application.csproj", "Event.Application/"]
COPY ["Event.Infrastructure/Event.Infrastructure.csproj", "Event.Infrastructure/"]
COPY ["events.domain/events.domain.csproj", "events.domain/"]

RUN dotnet restore "events/events.csproj"

COPY . .
WORKDIR /src/events
RUN dotnet publish "events.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Render provides PORT dynamically; this keeps a sane default for local container runs.
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "events.dll"]
