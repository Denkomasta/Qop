# Stage 1: Build
FROM node:22-alpine AS build
WORKDIR /app

# Accept the build arguments from docker-compose
ARG VITE_API_BASE_URL
ARG VITE_PORT

# Set them as Environment Variables for the 'yarn build' process
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_PORT=$VITE_PORT

COPY package.json yarn.lock* ./
RUN yarn install --frozen-lockfile || yarn install

COPY . .
# This is where 'vite build' runs. It will see the ENV variables above.
RUN yarn build

# Stage 2: Serve
FROM nginx:alpine
RUN rm -rf /usr/share/nginx/html/*
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf.template /etc/nginx/templates/default.conf.template
EXPOSE 80 443
CMD ["nginx", "-g", "daemon off;"]
