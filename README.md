# MedAI Healthcare Platform: Backend API

ASP.NET Core Web API for a healthcare platform that connects patients and doctors. Patients book appointments and upload chest X-ray images for AI-assisted screening. Doctors review each AI result and confirm or correct it before the patient sees the final diagnosis.

> Graduation project (team of 6). This repository is the backend API. The classification model runs as a separate Python service and is called over HTTP.

---

## Features

- **Authentication:** registration and login with ASP.NET Core Identity, JWT access tokens, and refresh tokens that are revoked and reissued on every refresh.
- **Roles:** `Patient`, `Doctor`, and `Admin`, enforced with role-based authorization on every endpoint.
- **AI X-ray workflow:**
  - A patient uploads an image. The API stores it, sends it to the AI service, and saves the predicted class and confidence score.
  - A patient with a scan still waiting for review cannot upload another one.
  - Doctors list unreviewed scans, then confirm or override the AI result and add notes. The scan is marked approved when the doctor agrees with the AI and edited when not.
  - Patients can read the result only after a doctor has reviewed it.
- **Appointments:**
  - Doctors publish available slots with a date, time range, consultation fee, and capacity.
  - Overlapping slots are rejected. A slot closes automatically when it is full and reopens when a booking is cancelled.
  - Patients cannot book the same slot twice.
- **Dashboards:** separate summaries for patients, doctors, and admins.
- **Admin tools:** create, update, and delete doctor accounts.
- **API quality:** Result Pattern for error handling, FluentValidation on requests, Mapster mapping, server-side pagination, a global exception handler, and Scalar API docs.

## Tech Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core Web API, .NET 10 |
| Data | Entity Framework Core, SQLite |
| Auth | ASP.NET Core Identity, JWT Bearer |
| Validation and mapping | FluentValidation, Mapster |
| API docs | OpenAPI, Scalar |
| AI service | Python, PyTorch (separate service) |

## API Overview

34 endpoints across seven controllers.

| Group | Base route | Access | What it does |
|---|---|---|---|
| Auth | `/auth` | Public | Register, login, refresh token |
| Account | `/me` | Authenticated | Profile, change password, update info, patient dashboard |
| Doctors | `/api/doctors` | Admin, Doctor, public | Doctor CRUD, doctor profile, schedule view, appointments, dashboard |
| Schedules | `/api/schedules` | Doctor | Create and delete slots, list slots, update capacity |
| Bookings | `/api/bookings` | Patient | Book a slot, list my bookings, cancel, dashboard |
| X-rays | `/api/xrays` | Patient, Doctor | Upload, review queue, confirm, history, results |
| Admin | `/api/admin` | Admin | Platform dashboard |

Full interactive documentation is available in Scalar once the app is running.

## AI Service Contract

The API calls the AI service at `http://127.0.0.1:5000/` (configured in `Program.cs`) with:

```http
POST /predict
Content-Type: application/json

{ "image": "<base64-encoded image>" }
```

It expects a JSON response containing `class` and `confidence`. If the service is unreachable or returns an invalid body, the upload fails with a clear error and nothing is saved.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- The AI service running locally (only needed for the X-ray upload endpoint)

### Run locally

```bash
git clone https://github.com/Ahmeddkhalill/MedAI.git
cd MedAI

# set your own JWT signing key (at least 32 characters)
dotnet user-secrets set "Jwt:Key" "your-own-secret-key-at-least-32-characters"

# create or update the SQLite database
dotnet ef database update

dotnet run
```

API documentation: `https://localhost:<port>/scalar/v1`

The database needs the roles `Admin`, `Doctor`, and `Patient`.

## Project Structure

```
MedAI/
├── Abstractions/     Result, Error, PaginatedList
├── Authentication/   JWT provider and options
├── Contracts/        Request and response models with validators
├── Controllers/      API endpoints
├── Entities/         Domain entities
├── Errors/           Typed error definitions per feature
├── Mapping/          Mapster configuration
├── Persistence/      DbContext, entity configurations, migrations
└── Services/         Business logic behind service interfaces
```

## Disclaimer

This is a graduation project for learning purposes. It is not a medical device.
