FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Rwd.WF.sln ./
COPY src/ ./src/

RUN dotnet restore ./Rwd.WF.sln
RUN dotnet publish ./src/Rwd.WF.API/Rwd.WF.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Rwd.WF.API.dll"]

