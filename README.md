# CarbonPulse Scheduler

A carbon-aware toy job scheduler that demonstrates how flexible workloads can be shifted into cleaner electricity windows. Users submit fictitious jobs with timing constraints, and the backend recommends optimal start times based on carbon-intensity forecasts. A web UI visualizes job lifecycles, timelines, and carbon-offset statistics using a controllable virtual clock.

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

## Features

- **Carbon-aware scheduling** — picks the lowest-intensity window within a job's allowable time range
- **Virtual clock** — set time, accelerate (1×/5×/10×/60×), pause, and resume
- **Job lifecycle** — automatic Scheduled → Running → Completed transitions driven by virtual time
- **Timeline visualization** — horizontal bars showing allowable windows and execution periods
- **Region mini-map** — pulsing indicators for regions with running jobs
- **Statistics** — total jobs, running count, completed count, carbon-hours shifted
