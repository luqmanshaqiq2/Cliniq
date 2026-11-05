# Cliniq API

The API exposes authenticated resource endpoints under `/api`. Register a patient account at `POST /api/auth/register`, then exchange credentials at `POST /api/auth/login` and send the returned access token as a bearer token. Public registration cannot assign privileged roles; `/api/users` requires an administrator token.

Set `JwtOptions__SignKey` to a secret of at least 32 bytes before starting the API. Configure an initial administrator on the first startup using `BootstrapAdmin__FullName`, `BootstrapAdmin__Email`, and `BootstrapAdmin__Password` (at least 12 and no more than 72 UTF-8 bytes). The bootstrap account is created only when no administrator exists. Do not commit these secrets.

Place static images manually in `StaticFiles` (for example `StaticFiles/images`) and request them under `/StaticFiles` (for example `/StaticFiles/images/logo.png`). OpenAPI is available at `/openapi/v1.json` in Development.
