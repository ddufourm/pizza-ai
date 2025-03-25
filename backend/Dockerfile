FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER root

RUN apk add --no-cache icu-libs && \
    rm -rf /var/cache/apk/*
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
RUN dotnet publish "backend.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true

FROM base AS final

USER root

RUN mkdir -p /app/certs && \
    chown -R app:app /app && \
    chmod 755 /app/certs

USER app

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "backend.dll"]
