FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . .
ARG APP_PROJECT=Sentinel.Api
RUN dotnet publish "src/${APP_PROJECT}/${APP_PROJECT}.csproj" -c Release -o /publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ARG APP_PROJECT=Sentinel.Api
ENV APP_ENTRYPOINT="${APP_PROJECT}.dll"
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /publish .
USER $APP_UID
ENTRYPOINT ["sh", "-c", "exec dotnet \"$APP_ENTRYPOINT\""]
