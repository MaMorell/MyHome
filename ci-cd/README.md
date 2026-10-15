# Raspberry Pi deployment artifacts

The **Build Raspberry Pi deployment** GitHub Actions workflow runs on every push
to `main`. It can also be started manually from the repository's **Actions** tab
using **Run workflow** (once the workflow is on the default branch).

The workflow uses a GitHub-hosted Linux runner with .NET 10 and Aspire CLI 13.5.4.
It builds Release containers for Linux ARM64, matching a Raspberry Pi running a
64-bit OS. It does not require a self-hosted runner or application secrets.

Each successful run uploads an artifact named
`myhome-linux-arm64-<commit SHA>`, retained for seven days, containing:

- `myhome-api.tar.gz` (`myhome-api:latest`)
- `myhome-web.tar.gz` (`myhome-web:latest`)
- `docker-compose.yml`, generated from the Aspire AppHost

Download it from the workflow run's **Artifacts** section. Only these three
files are uploaded; Aspire's generated `.env` file is intentionally excluded.

## Using the artifacts on the Pi

This workflow only builds artifacts; it does not deploy or restart the app.
A future deployment job can download the artifact from the same run on the
Pi's self-hosted runner.

For manual deployment, extract the artifact into the Pi's deployment directory
and supply a local `.env` file using the same variables as `publish-myhome.ps1`:

```dotenv
MYHOME_API_IMAGE=myhome-api:latest
MYHOME_WEB_IMAGE=myhome-web:latest
MYHOME_API_PORT=5000
MYHOME_WEB_PORT=5001
TIBBERAPICLIENTACCESSTOKEN=<Tibber access token>
THERMOSTATEBECOPASSWORD=<Ebeco password>
HOMEASSISTANTACCESSTOKEN=<Home Assistant access token>
```

Keep `.env` private and out of source control and artifacts. From that directory
on the Pi, run:

```bash
docker load -i myhome-api.tar.gz
docker load -i myhome-web.tar.gz
docker compose up -d
```

The existing Windows deployment script remains unchanged.
