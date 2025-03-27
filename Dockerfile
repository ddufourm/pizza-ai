FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER root

RUN apk add --no-cache icu-libs ca-certificates tzdata openssl && \
    rm -rf /var/cache/apk/*

ENV TZ=America/Toronto

RUN mkdir /app && \
	mkdir /app/data-protection-keys && \
	mkdir /app/certs && \
	mkdir /app/wwwroot

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

FROM build AS backend
USER root
RUN dotnet publish "backend.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true && \
	chown -R app:app /app/publish

FROM node:22-alpine AS frontend
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY ./frontend .
ARG CSP_NONCE
RUN if [ -z "$CSP_NONCE" ]; then echo "CSP_NONCE is not set!" && exit 1; fi
RUN sed -i "s|cspNonce: ''|cspNonce: '${CSP_NONCE}'|g" ./src/environments/environment.prod.ts
RUN npm run build

FROM base AS final
USER app
WORKDIR /app
COPY --from=backend --chown=app:app /app/publish .
COPY --from=frontend --chown=app:app /app/dist/frontend/browser ./wwwroot
ENTRYPOINT ["dotnet", "backend.dll"]
