FROM node:22-alpine AS builder
WORKDIR /app
COPY frontend/package.json ./
RUN npm ci
COPY ./frontend .
RUN npm run build -- --configuration production

FROM nginx:1.25-alpine

RUN mkdir -p /etc/ssl/cloudflare
COPY certificates/certificate.pem /etc/ssl/cloudflare
COPY certificates/private.key /etc/ssl/cloudflare

COPY --from=builder /app/dist/pizza-ai /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
