FROM node:22-alpine
WORKDIR /app
COPY package.json ./
RUN npm install
RUN npm install -g @angular/cli@19
COPY . .
RUN ng build --configuration production
