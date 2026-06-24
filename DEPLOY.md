# Deploying to Render (free tier)

This app is deployed as a Docker container on Render's free Web Service tier.
The container uses SQLite for the database (the SQL Server LocalDB used locally
is not available on Render). All data in the container filesystem is **ephemeral**
— it resets on every redeploy and after an idle spin-down. This is fine for a
portfolio / demo deployment.

---

## Prerequisites

- The repository is pushed to GitHub (or GitLab/Bitbucket).
- You have a free account on [render.com](https://render.com).

---

## Step-by-step deployment

### 1. Create a new Web Service

1. In the Render dashboard click **New → Web Service**.
2. Connect your GitHub account and choose this repository.
3. Render detects the `Dockerfile` automatically and sets **Environment: Docker**.
4. Give the service a name (e.g. `shipment-tracking`).
5. Choose the **Free** instance type.
6. Click **Create Web Service** — Render starts the first build.

### 2. Set environment variables

In the service's **Environment** tab, add these variables before (or after) the
first deploy — a new deploy is triggered automatically when you save them.

| Key | Value | Notes |
|-----|-------|-------|
| `DatabaseProvider` | `Sqlite` | Switches EF Core and Dapper from SQL Server to SQLite |
| `ConnectionStrings__DefaultConnection` | `Data Source=/app/data/shipment.db` | Note the **double underscore** — ASP.NET Core maps `__` to `:` in nested config keys |
| `ApiKey` | *(choose a strong random string)* | Protects all admin endpoints; never commit this value |

`PORT` is **injected by Render automatically** — do not set it yourself.

### 3. Verify the build

Watch the **Logs** tab. On a successful deploy you should see:
- `dotnet publish` completing with no errors
- ASP.NET Core starting and Kestrel listening on the Render-assigned port
- EF Core applying the `InitialCreate` migration (first deploy only)

### 4. Test the live URL

Render provides a public URL in the form `https://<service-name>.onrender.com`.

**Public tracking page (no key):**
```
https://<service-name>.onrender.com/
```
The page loads; track a shipment number once you have created one.

**Admin page:**
```
https://<service-name>.onrender.com/admin.html
```
Enter the `ApiKey` value you set in step 2. Then:
1. **Create a shipment** — note the tracking number returned.
2. **Refresh** the shipments list — the new row appears.
3. **Update the status** to the next legal value (e.g. `Created` → `AtCustoms`).
4. **Public track** — go to the public page, enter the tracking number, confirm
   the status card and timeline appear.
5. Try an admin request without the API key — should return 401.
6. Try an illegal status transition (e.g. `Created` → `Delivered`) — should
   return 400 with the transition error message.

---

## Free-tier behaviour to expect

- **Cold start:** Render's free tier spins down the container after ~15 minutes
  of inactivity. The first request after a spin-down takes 30–60 seconds while
  the container restarts. Subsequent requests are fast.
- **Ephemeral storage:** The SQLite file lives inside the container at
  `/app/data/shipment.db`. It is lost on every redeploy and whenever the
  container restarts after a spin-down. If you need data to survive restarts,
  upgrade to a paid plan with a persistent disk, or switch to a managed database.
- **HTTPS:** Render terminates TLS at its edge and forwards plain HTTP to the
  container on `PORT`. The app's `UseHttpsRedirection` middleware is a no-op
  inside the container (no HTTPS port is configured there).
