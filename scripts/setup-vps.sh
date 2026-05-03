#!/usr/bin/env bash

set -Eeuo pipefail

# ==========================================
# 1. CONFIGURATION
# ==========================================
# Required public settings
DOMAIN="sqeez.yourdomain.com"
LETSENCRYPT_EMAIL="your-email@example.com"
GHCR_OWNER="Denkomasta"
GITHUB_REPO="https://github.com/Denkomasta/Sqeez.git"
GITHUB_BRANCH="main"

# Required secrets
DB_PASS="TodoSecurePassword"
JWT_SECRET="This_Is_My_Super_Secret_Key_That_Must_Be_At_Least_64_Characters_Long_123456789!"
SUPER_USER_EMAIL="admin@example.com"
SUPER_USER_DEFAULT_PASSWORD="ChangeThisPassword123!"
SUPER_USER_FIRST_NAME="System"
SUPER_USER_LAST_NAME="Administrator"
SUPER_USER_DEPARTMENT="Administration"
SUPER_USER_PHONE_NUMBER=""

# Optional SMTP settings
SMTP_SERVER="smtp.example.com"
SMTP_PORT="587"
SMTP_SENDER_NAME="Sqeez App"
SMTP_SENDER_EMAIL="noreply@example.com"
SMTP_USERNAME="your_smtp_username"
SMTP_PASSWORD="your_smtp_password"

# Optional GHCR login. Leave GHCR_TOKEN empty for public packages.
GHCR_USERNAME="$GHCR_OWNER"
GHCR_TOKEN=""

# Runtime settings
DEPLOY_DIR="/root/Sqeez"
POSTGRES_USER="postgres"
POSTGRES_DB="SqeezDb"
NGINX_CLIENT_MAX_BODY_SIZE="100m"
RUN_SEED="true"

# ==========================================
# 2. HELPERS
# ==========================================
log() {
  echo "--> $*"
}

require_root() {
  if [ "${EUID}" -ne 0 ]; then
    echo "This script must be run as root. Try: sudo bash scripts/setup-vps.sh"
    exit 1
  fi
}

require_config() {
  local name="$1"
  local value="$2"

  if [ -z "$value" ] || [[ "$value" == your-* ]] || [[ "$value" == *"yourdomain.com"* ]]; then
    echo "Please configure $name at the top of this script before running it."
    exit 1
  fi
}

install_packages() {
  log "Installing required packages..."
  apt-get update
  apt-get install -y ca-certificates curl git wget certbot docker.io docker-compose-plugin

  systemctl enable --now docker
}

ensure_certificate() {
  local cert_path="/etc/letsencrypt/live/${DOMAIN}/fullchain.pem"

  if [ -f "$cert_path" ]; then
    log "Existing Let's Encrypt certificate found for ${DOMAIN}."
    return
  fi

  log "Generating Let's Encrypt certificate for ${DOMAIN}..."

  if command -v systemctl >/dev/null 2>&1; then
    systemctl stop nginx 2>/dev/null || true
  fi

  docker ps --format '{{.Names}}' | grep -qx 'sqeez-frontend' && docker stop sqeez-frontend || true

  certbot certonly \
    --standalone \
    -d "$DOMAIN" \
    --non-interactive \
    --agree-tos \
    -m "$LETSENCRYPT_EMAIL"
}

write_env_file() {
  log "Writing ${DEPLOY_DIR}/.env..."

  cat > "${DEPLOY_DIR}/.env" <<EOF
GHCR_OWNER=${GHCR_OWNER}

POSTGRES_USER=${POSTGRES_USER}
POSTGRES_PASSWORD=${DB_PASS}
POSTGRES_DB=${POSTGRES_DB}

ConnectionStrings__DefaultConnection=Host=sqeez-postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${DB_PASS}
TokenKey=${JWT_SECRET}
FrontendUrl=https://${DOMAIN}

NGINX_SERVER_NAME=${DOMAIN}
NGINX_SSL_CERTIFICATE=/etc/letsencrypt/live/${DOMAIN}/fullchain.pem
NGINX_SSL_CERTIFICATE_KEY=/etc/letsencrypt/live/${DOMAIN}/privkey.pem
NGINX_CERTIFICATE_HOST_PATH=/etc/letsencrypt
NGINX_BACKEND_URL=http://backend:8080
NGINX_CLIENT_MAX_BODY_SIZE=${NGINX_CLIENT_MAX_BODY_SIZE}

SUPER_USER_EMAIL=${SUPER_USER_EMAIL}
SUPER_USER_DEFAULT_PASSWORD=${SUPER_USER_DEFAULT_PASSWORD}
SUPER_USER_FIRST_NAME=${SUPER_USER_FIRST_NAME}
SUPER_USER_LAST_NAME=${SUPER_USER_LAST_NAME}
SUPER_USER_DEPARTMENT=${SUPER_USER_DEPARTMENT}
SUPER_USER_PHONE_NUMBER=${SUPER_USER_PHONE_NUMBER}

SmtpSettings__Server=${SMTP_SERVER}
SmtpSettings__Port=${SMTP_PORT}
SmtpSettings__SenderName=${SMTP_SENDER_NAME}
SmtpSettings__SenderEmail=${SMTP_SENDER_EMAIL}
SmtpSettings__Username=${SMTP_USERNAME}
SmtpSettings__Password=${SMTP_PASSWORD}
EOF

  chmod 600 "${DEPLOY_DIR}/.env"
}

download_compose_file() {
  log "Downloading docker-compose.yml..."
  wget -qO "${DEPLOY_DIR}/docker-compose.yml" \
    "https://raw.githubusercontent.com/${GHCR_OWNER}/Sqeez/${GITHUB_BRANCH}/src/docker-compose.yml"
}

docker_login_if_needed() {
  if [ -n "$GHCR_TOKEN" ]; then
    log "Logging in to GitHub Container Registry..."
    echo "$GHCR_TOKEN" | docker login ghcr.io -u "$GHCR_USERNAME" --password-stdin
  fi
}

wait_for_postgres() {
  local timeout_seconds=120
  local elapsed=0

  log "Waiting for PostgreSQL to accept connections..."

  until docker exec sqeez_db pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB" >/dev/null 2>&1; do
    if [ "$elapsed" -ge "$timeout_seconds" ]; then
      echo "PostgreSQL did not become ready within ${timeout_seconds} seconds."
      docker logs sqeez_db || true
      exit 1
    fi

    sleep 2
    elapsed=$((elapsed + 2))
  done

  log "PostgreSQL is ready."
}

generate_migration_script() {
  local temp_dir
  temp_dir="$(mktemp -d)"

  log "Cloning repository into ${temp_dir}..."
  git clone --depth 1 --branch "$GITHUB_BRANCH" "$GITHUB_REPO" "${temp_dir}/SqeezRepo"

  log "Generating idempotent migration script in a temporary .NET SDK container..."
  docker run --rm \
    -v "${temp_dir}/SqeezRepo:/src" \
    -w /src/src/backend/Sqeez.Api \
    mcr.microsoft.com/dotnet/sdk:10.0 \
    bash -lc 'dotnet tool install --global dotnet-ef && export PATH="$PATH:/root/.dotnet/tools" && dotnet restore && dotnet ef migrations script --idempotent --output /src/migrate.sql'

  cp "${temp_dir}/SqeezRepo/migrate.sql" "${DEPLOY_DIR}/migrate.sql"
  rm -rf "$temp_dir"
}

apply_migrations() {
  log "Applying database migrations..."
  cat "${DEPLOY_DIR}/migrate.sql" | docker exec -i sqeez_db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"
  rm -f "${DEPLOY_DIR}/migrate.sql"
}

run_seed_if_enabled() {
  if [ "$RUN_SEED" != "true" ]; then
    log "Skipping database seed because RUN_SEED is not true."
    return
  fi

  log "Running database seeder..."
  docker compose --env-file .env run --rm backend dotnet Sqeez.Api.dll seed
}

# ==========================================
# 3. MAIN
# ==========================================
require_root
require_config "DOMAIN" "$DOMAIN"
require_config "LETSENCRYPT_EMAIL" "$LETSENCRYPT_EMAIL"
require_config "GHCR_OWNER" "$GHCR_OWNER"
require_config "GITHUB_REPO" "$GITHUB_REPO"
require_config "SUPER_USER_EMAIL" "$SUPER_USER_EMAIL"
require_config "SUPER_USER_DEFAULT_PASSWORD" "$SUPER_USER_DEFAULT_PASSWORD"

install_packages
ensure_certificate

log "Preparing ${DEPLOY_DIR}..."
mkdir -p "$DEPLOY_DIR"
cd "$DEPLOY_DIR"

download_compose_file
write_env_file
docker_login_if_needed

log "Pulling container images..."
docker compose --env-file .env pull

log "Starting PostgreSQL only..."
docker compose --env-file .env up -d sqeez-postgres
wait_for_postgres

generate_migration_script
apply_migrations
run_seed_if_enabled

log "Starting full Sqeez stack..."
docker compose --env-file .env up -d

log "Setup complete. Sqeez should be available at https://${DOMAIN}"
