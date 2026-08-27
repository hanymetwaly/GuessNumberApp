# Deployment guide — Azure VM + Docker Compose (HTTPS)

This deploys the whole stack to a single Azure Linux VM using `docker-compose.yml`:

```
Internet ──► Caddy (80/443, auto-HTTPS)
                ├── /api/*  → backend  (.NET, :8080, internal)
                └── /*      → frontend (nginx SPA, :80, internal)
                                        backend ──► db (Postgres, internal)
```

Only Caddy is exposed publicly. Postgres data persists in a named volume.

---

## 0. Prerequisites
- An Azure account (free tier is fine; a card is required for verification).
- The code pushed to GitHub (already at `github.com/hanymetwaly/GuessNumberApp`).

## 1. Create the VM
In the Azure Portal → **Virtual machines → Create**:
- **Image:** Ubuntu Server 22.04 LTS
- **Size:** `Standard_B1s` (free-tier eligible for 12 months)
- **Authentication:** SSH public key
- **Inbound ports:** allow **SSH (22)**, **HTTP (80)**, **HTTPS (443)**

After creation, open the VM's **Public IP** resource → **Configuration** → set a
**DNS name label**, e.g. `guessnumber-demo`. This gives you a free FQDN like:

```
guessnumber-demo.westeurope.cloudapp.azure.com
```

> This FQDN is what makes real HTTPS possible — Let's Encrypt issues a cert for it.

## 2. Install Docker on the VM
SSH in (`ssh azureuser@<public-ip>`), then:

```bash
sudo apt-get update
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER
newgrp docker   # or log out/in so the group applies
```

## 3. Get the code
```bash
git clone https://github.com/hanymetwaly/GuessNumberApp.git
cd GuessNumberApp
```

## 4. Configure environment
```bash
cp .env.example .env
nano .env
```
Set these values (use your real FQDN):

```dotenv
POSTGRES_DB=guessnumber
POSTGRES_USER=postgres
POSTGRES_PASSWORD=<a strong password>

JWT_KEY=<run: openssl rand -base64 48>
JWT_ISSUER=GuessNumberApi
JWT_AUDIENCE=GuessNumberClient

SITE_ADDRESS=guessnumber-demo.westeurope.cloudapp.azure.com
TLS_EMAIL=you@example.com
PUBLIC_URL=https://guessnumber-demo.westeurope.cloudapp.azure.com
```

## 5. Build and run
```bash
docker compose up -d --build
```
First boot: the backend auto-applies EF Core migrations, and Caddy fetches a TLS
certificate for your FQDN (takes ~30s; ports 80 and 443 must be open).

Check status / logs:
```bash
docker compose ps
docker compose logs -f caddy      # watch certificate issuance
docker compose logs -f backend
```

## 6. Share the link
Send your team:

```
https://guessnumber-demo.westeurope.cloudapp.azure.com
```

Register a user, play, and the best score persists in Postgres.

---

## Operations

Update after pushing new code:
```bash
git pull
docker compose up -d --build
```

Stop / start:
```bash
docker compose down        # stop (keeps the DB volume)
docker compose up -d
```

Back up the database:
```bash
docker compose exec db pg_dump -U postgres guessnumber > backup.sql
```

## Local testing (plain HTTP, no cert)
On your machine you can smoke-test the same stack without a domain by setting
`SITE_ADDRESS=:80` and `PUBLIC_URL=http://localhost` in `.env`, then:
```bash
docker compose up -d --build
# open http://localhost
```

## Troubleshooting
- **Cert not issued / HTTPS fails:** ensure ports 80 **and** 443 are open in the
  Azure NSG and `SITE_ADDRESS` is the exact FQDN (not the IP). Watch
  `docker compose logs -f caddy`.
- **Backend can't reach DB:** it connects to host `db` on the internal network;
  confirm the `db` service is healthy (`docker compose ps`).
- **502 from Caddy:** the backend or frontend container may still be starting or
  failed its build — check `docker compose logs backend frontend`.
