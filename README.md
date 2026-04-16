# CarbonPulse Scheduler

A carbon-aware toy job scheduler that demonstrates how flexible workloads can be shifted into cleaner electricity windows. Users submit fictitious jobs with timing constraints, and the backend recommends optimal start times based on carbon-intensity forecasts. A web UI visualizes job lifecycles, timelines, and carbon-offset statistics using a controllable virtual clock.

![A screenshot of CarbonPulse Scheduler showing the web interface and three active jobs.](.\docs\Screenshot.png)

## Architecture

- **Backend (C# / ASP.NET Core)** — REST API with in-memory job store, carbon-aware scheduling engine, and virtual time model
- **Frontend (Node.js / Express)** — Single-page dashboard that proxies API calls to the backend

## Prerequisites

- [.NET 10 SDK](https://dot.net/download) (or whichever version matches your `global.json` / installed SDK)
- [Node.js](https://nodejs.org/) (v18+)

## Running Locally

### 1. Start the backend

```bash
cd backend
dotnet run --launch-profile http
```

The API will be available at **http://localhost:5170**.

### 2. Start the frontend

```bash
cd frontend
npm install
npm start
```

The UI will be available at **http://localhost:3000**. API requests are proxied to the backend automatically.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/jobs` | Create and schedule a new job |
| `GET` | `/api/jobs` | List all jobs |
| `GET` | `/api/jobs/{id}` | Get a single job |
| `POST` | `/api/jobs/{id}/cancel` | Cancel a job |
| `GET` | `/api/clock` | Get current virtual time |
| `POST` | `/api/clock` | Control virtual time (set, reset, accelerate, pause, resume) |
| `GET` | `/api/forecast?region=...&start=...&end=...` | Get carbon intensity forecast |

## Configuration

The backend reads these settings from `appsettings.json` (or environment variables):

| Key | Values | Default | Description |
|-----|--------|---------|-------------|
| `Scheduler` | `CarbonAware` / `Dummy` | `CarbonAware` | Scheduling strategy |
| `CarbonProvider` | `Mock` / `Sdk` | `Mock` | Carbon intensity data source |
| `CarbonAwareSdk:BaseUrl` | URL | `http://localhost:8080` | Carbon Aware SDK endpoint |

By default the app uses a mock provider with synthetic data so it works out of the box. To use real carbon data, set up the Carbon Aware SDK (see below) and change `CarbonProvider` to `Sdk`.

## Using the Carbon Aware SDK

To get real carbon-intensity forecasts instead of synthetic data, run a local instance of the [Carbon Aware SDK](https://github.com/Green-Software-Foundation/carbon-aware-sdk).

> **Note:** The SDK ships with pre-generated JSON test data, so it works **without** any external API credentials. Only configure WattTime/ElectricityMaps if you want live data.

### Option A: Run from source (simplest)

```bash
git clone https://github.com/Green-Software-Foundation/carbon-aware-sdk.git
cd carbon-aware-sdk/src/CarbonAware.WebApi/src
dotnet run
```

The SDK WebAPI will start on **http://localhost:5073** with built-in sample data.

To use **live WattTime data**, set these environment variables before `dotnet run`:

```bash
# Linux/macOS
export DataSources__EmissionsDataSource="WattTime"
export DataSources__ForecastDataSource="WattTime"
export DataSources__Configurations__WattTime__Type="WattTime"
export DataSources__Configurations__WattTime__username="<YOUR_USERNAME>"
export DataSources__Configurations__WattTime__password="<YOUR_PASSWORD>"
```

```powershell
# Windows PowerShell
$env:DataSources__EmissionsDataSource="WattTime"
$env:DataSources__ForecastDataSource="WattTime"
$env:DataSources__Configurations__WattTime__Type="WattTime"
$env:DataSources__Configurations__WattTime__username="<YOUR_USERNAME>"
$env:DataSources__Configurations__WattTime__password="<YOUR_PASSWORD>"
```

WattTime accounts are created via their API (there is no web registration form):

```bash
curl -X POST https://api.watttime.org/register \
  -H "Content-Type: application/json" \
  -d '{"username":"myuser","password":"mypass123!","email":"me@example.com"}'
```

The free tier gives access to the `CAISO_NORTH` region (Northern California). For broader coverage, see [WattTime data plans](https://watttime.org/docs-dev/data-plans/). You can also use `ElectricityMaps` as the data source — see the [SDK quickstart](https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/casdk-docs/docs/quickstart.md) for config keys.

### Option B: Docker

```bash
docker run -d --name carbon-aware-sdk \
  -p 5073:8080 \
  ghcr.io/green-software-foundation/carbon-aware-sdk:latest
```

This starts the SDK with built-in sample data on **http://localhost:5073**. To use live data, add the environment variables above with `-e` flags.

### Connect CarbonPulse to the SDK

Update `backend/appsettings.json`:

```json
{
  "CarbonProvider": "Sdk",
  "CarbonAwareSdk": {
    "BaseUrl": "http://localhost:5073"
  }
}
```

Then restart the backend. Job scheduling will now use forecasts from the Carbon Aware SDK.

> **Note:** The SDK's region identifiers (e.g. `westus`, `eastus`) are defined in its [location data files](https://github.com/Green-Software-Foundation/carbon-aware-sdk/tree/dev/src/data/location-sources). These must match the regions used in the frontend.

## Features

- **Carbon-aware scheduling** — picks the lowest-intensity window within a job's allowable time range
- **Virtual clock** — set time, accelerate (1×/5×/10×/60×), pause, and resume
- **Job lifecycle** — automatic Scheduled → Running → Completed transitions driven by virtual time
- **Timeline visualization** — horizontal bars showing allowable windows and execution periods
- **Region mini-map** — pulsing indicators for regions with running jobs
- **Statistics** — total jobs, running count, completed count, carbon-hours shifted
