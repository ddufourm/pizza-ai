FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER root

RUN apk add --no-cache icu-libs && \
    rm -rf /var/cache/apk/*

RUN mkdir /app
RUN mkdir /app/data-protection-keys
RUN mkdir /app/certs

COPY ./certificates /app/certs

RUN chown -R app:app /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
USER root
WORKDIR /src
COPY ["backend.csproj", "."]
RUN dotnet restore "backend.csproj"
RUN apk add --no-cache libc6-compat
COPY . .
RUN dotnet build "backend.csproj" -c Release -o /app/build

FROM build AS publish
USER root
RUN dotnet publish "backend.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true && \
	chown -R app:app /app/publish

FROM base AS final
USER app
WORKDIR /app
COPY --from=publish --chown=app:app /app/publish .
ENTRYPOINT ["dotnet", "backend.dll"]
