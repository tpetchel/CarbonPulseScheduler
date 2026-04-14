# CarbonPulse Scheduler — Starter Specification

**Status:** Draft (Hackathon MVP)  
**Audience:** Contributors, hackathon collaborators  
**Last updated:** 4/14/2026

---

## 1. Overview

CarbonPulse Scheduler is a **carbon-aware toy job scheduler** designed to demonstrate how flexible workloads can be shifted into cleaner electricity windows using carbon-intensity forecasts.

The system allows users to submit *fictitious jobs* with timing constraints (regions, allowed window, duration). A backend scheduler recommends an optimal start time based on carbon-awareness heuristics, and a web UI visualizes job lifecycles, timelines, and carbon-offset statistics using a controllable **virtual clock**.

This is an **educational / demonstration system**, not a production scheduler.

---

## 2. Goals & Non‑Goals

### Goals

- Demonstrate **carbon-aware scheduling decisions** end‑to‑end
- Make the decision-making process **observable and visual**
- Provide a clean, extensible backend API surface
- Support rapid experimentation via a **virtual time model**

### Non‑Goals

- Accurate modeling of real workload energy consumption
- Real emissions accounting or reporting
- Production-grade persistence, reliability, or security
- Real cloud infrastructure orchestration

---

## 3. High-Level Architecture

```plaintext
┌──────────────┐        REST       ┌────────────────────┐
│   Frontend   │  ─────────────▶  │      Backend        │
│   (Node.js)  │                   │      (C#)          │
└──────┬───────┘                   └──────┬─────────────┘
       │                                    │
       │                                    │
 Timeline / Maps / Stats          Scheduler + Job Store
       │                                    │
       ▼                                    ▼
 Virtual Clock                     Carbon-Aware Decision
                                  (real or dummy engine)
```

---

## 4. Backend (C#)

### 4.1 Core Responsibilities

- Maintain job definitions and lifecycle state
- Recommend & assign start times based on scheduling strategy
- Advance job state according to **virtual time**
- Expose REST APIs for job creation, cancellation, and polling

---

### 4.2 Domain Model (Initial)

```csharp
enum JobStatus
{
    Pending,
    Scheduled,
    Running,
    Completed,
    Cancelled
}

class Job
{
    Guid JobId;
    string Region;
    DateTimeOffset EarliestStart;
    DateTimeOffset LatestEnd;
    TimeSpan Duration;

    DateTimeOffset? ScheduledStart;
    DateTimeOffset? ScheduledEnd;

    JobStatus Status;
}
```

---

### 4.3 Scheduler Abstractions

To keep the system testable and extensible, scheduling logic is abstracted.

```csharp
public interface IJobScheduler
{
    SchedulingDecision Recommend(Job job, SchedulingContext context);
}
```

```csharp
class SchedulingDecision
{
    DateTimeOffset RecommendedStart;
    string Rationale; // e.g. "Lowest forecasted carbon intensity"
}
```

#### Implementations

- **CarbonAwareScheduler**
  - Uses Carbon Aware SDK to select the best start time within `[EarliestStart, LatestEnd]`
- **DummyScheduler (for testing & demos)**
  - Uses simple heuristics (e.g. midpoint of window, random low point)
  - Requires no external data dependencies

---

### 4.4 Carbon-Aware Integration Abstraction

The backend should **not depend directly** on the SDK everywhere.

```csharp
public interface ICarbonIntensityProvider
{
    IReadOnlyList<CarbonIntensityPoint> GetForecast(
        string region,
        DateTimeOffset start,
        DateTimeOffset end);
}
```

```csharp
class CarbonIntensityPoint
{
    DateTimeOffset Timestamp;
    double Intensity; // abstract units; relative comparison only
}
```

#### Implementations

- **CarbonAwareSdkProvider**
- **MockCarbonProvider** (static or generated curves for demos)

---

### 4.5 Persistence Layer

Start with in-memory storage, but design for replacement.

```csharp
public interface IJobRepository
{
    Job Create(Job job);
    Job? Get(Guid jobId);
    IReadOnlyList<Job> List();
    void Update(Job job);
    void Delete(Guid jobId);
}
```

#### Initial implementation

- `InMemoryJobRepository`
  - Thread-safe dictionary
  - Resettable for demos

---

### 4.6 REST API Surface (v1)

#### Create Job

```plaintext
POST /api/jobs
```

Request:

```json
{
  "region": "west-us",
  "earliestStart": "2026-04-15T10:00:00Z",
  "latestEnd": "2026-04-15T16:00:00Z",
  "durationMinutes": 45
}
```

Response:

```json
{
  "jobId": "...",
  "status": "Scheduled",
  "scheduledStart": "...",
  "scheduledEnd": "...",
  "rationale": "Lowest forecasted carbon intensity"
}
```

---

#### Cancel Job

```plaintext
POST /api/jobs/{jobId}/cancel
```

---

#### Poll Job Status

```plaintext
GET /api/jobs/{jobId}
```

---

#### List Jobs

```plaintext
GET /api/jobs
```

---

#### Virtual Clock Control

```plaintext
POST /api/clock
```

```json
{
  "mode": "set | reset | accelerate",
  "value": "2026-04-15T12:00:00Z | 5 | 10"
}
```

---

## 5. Virtual Time Model

The system operates on **virtual time**, not wall-clock time.

### Capabilities

- Set virtual time arbitrarily
- Reset to current real time
- Accelerate time at fixed multipliers (1x, 5x, 10x)

### Effects

- Job transitions (Scheduled → Running → Completed)
- UI timeline updates
- Carbon-offset statistics accumulation

---

## 6. Frontend (Node.js)

### 6.1 Responsibilities

- Submit jobs (manual or randomized)
- Control virtual time
- Visualize job schedules and execution
- Display carbon-aware statistics

---

### 6.2 Key UI Components

#### Timeline View

- Horizontal time axis (virtual time)
- Bars for each job:
  - Scheduled window
  - Actual execution window
- Color coding:
  - Pending / Running / Completed
  - Carbon-aware vs baseline

---

#### Job Submission Panel

Inputs:

- Region (dropdown)
- Allowable window (start/end picker)
- Duration
- "Generate random job" button

A global **rate limiter** enforces:

- **60 job submissions per minute**

---

#### Virtual Clock Control

- Current virtual time display
- Buttons:
  - Reset to real time
  - 1x / 5x / 10x speed
  - Pause

---

#### Statistics Panel

Example metrics:

- Total jobs scheduled
- Jobs currently running
- "Carbon-hours shifted" (relative units)
- Avg deferral time per job

These are **toy metrics** intended to show *directional impact*, not real emissions.

---

#### Mini-Map Visualization

- Flat, non-political world map
- Highlight regions where jobs are currently **running**
- Subtle pulsing or glow effect per region
- Aggregated count per region

---

## 7. Demo & Storytelling Enhancements (Optional)

- Side-by-side comparison:
  - Naive scheduler vs carbon-aware scheduler
- Toggle scheduling strategies live
- "What would have happened?" overlays
- Exportable decision logs per job

---

## 8. Future Extensions

- Pluggable scheduling policies
- Multiple carbon providers
- Scenario replay / time rewind
- Export demo data for talks or blogs
- Real CI workload simulation

---

## 9. Hackathon Scope Guidance

This spec is intentionally modular. A hackathon MVP is successful if:

- Jobs can be created
- Start times are recommended intelligently
- The UI makes the decision visible and intuitive

Everything else is a stretch goal.

---

## 10. Summary

CarbonPulse Scheduler is a **visual, end-to-end demonstration** of carbon-aware scheduling decisions. By combining a clean backend abstraction, a virtual time model, and a rich web UI, it makes climate-aware software behavior understandable, debuggable, and compelling.
