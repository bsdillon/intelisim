FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src
COPY ["range.csproj", "./"]
COPY ["nuget.config", "./"]
RUN dotnet restore "range.csproj"
COPY . .
WORKDIR "/src/"
# RUN libman restore
RUN dotnet build "range.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "range.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "range.dll", "web", "--debug"]
