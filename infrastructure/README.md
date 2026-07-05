# Domu Local Infrastructure

This folder contains the local development infrastructure for Domu.

The current setup runs with Rancher Desktop and Docker Compose. Rancher Desktop must use the Docker-compatible `dockerd/moby` container engine for Compose support.

## Files

- `container-compose.yaml`: Main Compose file. Starts Postgres, Mailpit, Swagger UI, and includes the Zitadel Compose file.
- `container-compose.zitadel.yaml`: Zitadel, Zitadel Login, Traefik proxy, and optional cache/observability services.
- `.env`: Local environment values used by Compose. This file may contain secrets and should not be committed.
- `init-db.sh`: Postgres initialization script mounted into the database container.

## Requirements

- Rancher Desktop
- Rancher Desktop container engine set to `dockerd/moby`
- Docker Compose support available from the terminal
- A populated `.env` file in this folder

## Start

From this folder:

```powershell
docker compose -f container-compose.yaml up -d
```

To watch startup logs:

```powershell
docker compose -f container-compose.yaml logs -f
```

To stop the stack:

```powershell
docker compose -f container-compose.yaml down
```

## Services

| Service | Purpose | Local access |
| --- | --- | --- |
| `pgdb` | Postgres database | `localhost:5432` |
| `mailpit` | Local SMTP inbox | `http://localhost:8025` |
| `swaggerui` | Swagger UI for the backend OpenAPI document | `http://localhost:5001` |
| `proxy` | Traefik reverse proxy for Zitadel | Port configured by `PROXY_HTTP_PUBLISHED_PORT` |
| `zitadel-api` | Zitadel API and core service | Routed through Traefik |
| `zitadel-login` | Zitadel Login v2 UI | Routed through Traefik |

## Zitadel Notes

Zitadel is configured through environment variables in `.env`. Important values include:

- `ZITADEL_DOMAIN`
- `ZITADEL_EXTERNALPORT`
- `ZITADEL_EXTERNALSECURE`
- `ZITADEL_PUBLIC_SCHEME`
- `ZITADEL_MASTERKEY`
- `ZITADEL_DATABASE_POSTGRES_DSN`

The login UI is mounted at:

```text
{ZITADEL_PUBLIC_SCHEME}://{ZITADEL_DOMAIN}:{ZITADEL_EXTERNALPORT}/ui/v2/login/
```

The default login crendentials are:
- Username: zitadel-admin@zitadel.localhost
- Password: Password1!

The root route for the Zitadel domain is rewritten to the login UI.

## Callback and Logout URLs

The application callback and logout URLs are not finalized yet.

When the app integration is configured, document the exact values here.

For Expo running on an emulator, Zitadel must include the Expo redirect URL for the host machine IP address. The IP address can differ between networks and machines.

```text
exp://<host-machine-ip>:8081/--/ath/callback
```

Tracked values:

```text
Callback URL: exp://<host-machine-ip>:8081/--/ath/callback
Logout URL:
Post-logout redirect URL:
```

Example for a host machine at `192.168.0.60`:

```text
exp://192.168.0.60:8081/--/ath/callback
```

These values must match both:

- the Zitadel application/client configuration
- the frontend/backend authentication configuration

## Optional Profiles

The Zitadel Compose file defines optional services behind profiles:

- `cache`: starts Redis
- `observability`: starts the OpenTelemetry collector

Example:

```powershell
docker compose -f container-compose.yaml --profile cache up -d
```

## Persistent Volumes

Compose creates named volumes for persistent local state:

- `pgdata`: Postgres data
- `mailpit_data`: Mailpit data
- `zitadel-bootstrap`: Zitadel bootstrap/login client token material

Deleting these volumes resets local state and may require bootstrapping services again.
