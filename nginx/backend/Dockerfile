FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER root

RUN apk add --no-cache icu-libs ca-certificates tzdata openssl && \
    rm -rf /var/cache/apk/*

ENV TZ=America/Toronto

RUN mkdir /app
RUN mkdir /app/data-protection-keys
RUN mkdir /app/certs

COPY certificates /app/certs

RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

RUN chmod -R 777 /app/certs && \
    chown -R app:app /app

ENV LC_ALL=fr_CA.UTF-8 \
    LANG=fr_CA.UTF-8

USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
USER root
WORKDIR /src
COPY backend/backend.csproj .
RUN dotnet restore "backend.csproj"
RUN apk add --no-cache libc6-compat
COPY ./backend .
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
