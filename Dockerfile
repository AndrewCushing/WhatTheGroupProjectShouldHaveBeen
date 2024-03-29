FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./LunaCinemasBackEndInDotNet/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./LunaCinemasBackEndInDotNet ./
RUN dotnet publish -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
WORKDIR /app
COPY --from=build-env /app/out .
COPY --from=build-env /app/Properties ./Properties
ENTRYPOINT ["dotnet", "./LunaCinemasBackEndInDotNet.dll", "dummy-connection-string"]