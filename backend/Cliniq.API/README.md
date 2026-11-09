# Cliniq API

The API exposes authenticated resource endpoints under `/api`. Register a patient account at `POST /api/auth/register`, then exchange credentials at `POST /api/auth/login` and send the returned access token as a bearer token. Public registration cannot assign privileged roles; `/api/users` requires an administrator token.

Set `JwtOptions__SignKey` to a secret of at least 32 bytes before starting the API. Configure an initial administrator on the first startup using `BootstrapAdmin__FullName`, `BootstrapAdmin__Email`, and `BootstrapAdmin__Password` (at least 12 and no more than 72 UTF-8 bytes). The bootstrap account is created only when no administrator exists. Do not commit these secrets.

Place static images manually in `StaticFiles` (for example `StaticFiles/images`) and request them under `/StaticFiles` (for example `/StaticFiles/images/logo.png`). In Development, Swagger UI is available at `/swagger` and the OpenAPI document at `/openapi/v1.json`.

## Docker

Build the API image from the repository root:

```sh
docker build -t cliniq-api .
```

Run it with a persistent SQLite database and a signing key of at least 32 bytes:

```sh
docker run --name cliniq-api -p 8080:8080 --mount source=cliniq-data,target=/app/data -e JwtOptions__SignKey="replace-with-a-random-secret-at-least-32-bytes" cliniq-api
```

The API listens on port `8080`; its SQLite database is stored at `/app/data/cliniq.db`. To create the initial administrator on first startup, also set `BootstrapAdmin__FullName`, `BootstrapAdmin__Email`, and `BootstrapAdmin__Password` (12–72 UTF-8 bytes). Supply secrets through your deployment's secret manager rather than committing them. Static files can be added by mounting a directory at `/app/StaticFiles`.
