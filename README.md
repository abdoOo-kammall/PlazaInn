# Plaza Inn API (`Plaza_SOL`)

**Arabic A-to-Z guide (recommended if you are coming back to this codebase):** [README.ar.md](./README.ar.md)

ASP.NET Core 8 backend for **Plaza Inn**: a bilingual (English / Arabic) hotel group website and booking platform. The API serves a public marketing site, guest room search and reservation, Paymob payments, and an admin/CMS layer for hotels, rooms, media, partners, and contact content.

There is **no frontend in this repository**. The intended client is a web app (password-reset emails point at `http://localhost:4200`, typically Angular). Explore and try endpoints at Swagger: `http://localhost:5141/swagger` or `https://localhost:7142/swagger`.

---

## What this project does

| Audience | What they can do |
| --- | --- |
| **Guest / site visitor** | Browse hotels, rooms, home slider, About Us, Contact Us, restaurant/cafe insights and menus. Check room availability and prices. Create a booking and pay through Paymob. |
| **Registered client** | Register, log in (JWT), list their own bookings. Start a Paymob payment and poll order status. |
| **Admin** | Manage hotels, rooms, users, CMS content, partners, and (when authorized) all bookings. Broadcast emails to hotel partners. |
| **Paymob** | Calls `POST /api/Webhook/callback` after a payment so the booking can move from `Pending` to `Confirmed`. |

---

## Solution structure (clean architecture)

```
Plaza_SOL.sln
├── Plaza                 # ASP.NET Core Web API (controllers, JWT, CORS, Swagger, wwwroot)
├── PlazaCore             # Domain entities, repository/service contracts, specification pattern
├── PlazaRepository       # EF Core DbContext, Identity, migrations, generic repository, seeds
├── PlazaService          # Business logic (hotels, booking, Paymob, email, files, auth)
└── Shared                # DTOs, enums, IdEncoder, email HTML helpers
```

**Request flow**

1. HTTP hits a controller in `Plaza`.
2. Controller calls a service in `PlazaService`.
3. Service uses `IGenericRepository<T>` and/or Identity / `HttpClient` (Paymob).
4. Repository applies **specifications** (includes, filters) against SQL Server.
5. AutoMapper maps entities ↔ DTOs. Public IDs are **encoded** (`IdEncoder`), not raw integers.

---

## Tech stack

- **.NET 8**, ASP.NET Core Web API
- **Entity Framework Core 8** + **SQL Server**
- **ASP.NET Identity** (`ApplicationUser`) + **JWT Bearer** (1-hour tokens)
- **AutoMapper 12**
- **Swashbuckle** (Swagger UI always enabled)
- **Paymob Accept** (`accept.paymob.com`) payment links
- **Gmail SMTP** for password reset and partner broadcasts
- Static files from `Plaza/wwwroot`

---

## Domain model

| Entity | Role |
| --- | --- |
| `Hotel` | Property: bilingual name/description/address/city, contact, social links, logo/image IDs, room/suite counts, rating. |
| `Room` | Belongs to a hotel. Type (`Single`, `Double`, `Suite`), price per night, extra bed, max occupancy, feature flags, images. |
| `Booking` | Guest stay: hotel, room, customer name/email, dates, total price, `IsPaid`, Paymob `PaymentReference`, status. |
| `InsightHotel` | Restaurant/cafe copy, image IDs, menu PDF IDs, facility flags (one per hotel). |
| `Partners` | Email contacts linked to many hotels (B2B “stop sales” / broadcast). |
| `Image` | Uploaded file: URL, SHA hash, `PlazaInnType`. |
| `HomeSlider` | Homepage slides (title, subtitle, button, image IDs). |
| `AboutUs` | About-us CMS block. |
| `ContactUs` + `WorkingHour` | Company contact, socials, weekly hours. |
| `ApplicationUser` | Identity user. Roles: `Admin`, `Client`, `User`. Public register assigns **Client**. |

**Booking status:** `Pending` → `Confirmed` (after successful payment) or `Cancelled` (enum exists; cancellation API is not implemented).

**File types (`PlazaInnType`):** `hotel`, `room`, `restaurant`, `cafe`, `restaurantMenu`, `cafeMenu`, `logo`, `homeSlider`, `aboutUs`.

**Opaque IDs:** integer primary keys are Base64-encoded with a shared secret (`PlazaInnKey2025`) so clients send `encodedId` instead of `1`, `2`, …

**Pricing:** nights × `PricePerNight` × **1.28** (28% markup in `RoomAvailabilityService.CalculateTotalPriceAsync`).

---

## Configuration (do not commit secrets)

Set these in **User Secrets**, environment variables, or a private local file — not in git.

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server |
| `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` | Token signing (`Issuer` = `PlazaAPI`, `Audience` = `PlazaClient`) |
| `Email:SmtpServer`, `Port`, `Username`, `Password`, `From`, `Signature` | SMTP |
| `PaymobTest:ApiKeyTest`, `IntegrationIdTest`, `HmacTest` | Paymob sandbox |
| `Paymob:ApiKeyLive`, `IntegrationIdLive`, `HmacLive` | Paymob production (live IDs currently empty) |

**Identity seed** (`DefaultUser` / `DefaultRole`): roles `Admin`, `Client`, `User` are created at startup. The default admin seed currently uses a **blank/whitespace email**, so the seeded admin may fail to log in until that seed is fixed.

**Password policy:** Identity is relaxed (min length 6, no digit/case/symbol required).

---

## How to run

Prerequisites: .NET 8 SDK, SQL Server (local or hosted).

1. Clone the repo and open `Plaza_SOL.sln`.
2. Point `DefaultConnection` at your database.
3. From the `Plaza` project (startup):

```bash
dotnet restore
dotnet ef database update --project ../PlazaRepository --startup-project .
dotnet run --launch-profile https
```

4. Open Swagger: `https://localhost:7142/swagger`.
5. CORS is `AllowAll` (any origin, method, header) so a local SPA can call the API.

Uploads are stored under `Plaza/wwwroot` and served as static files.

---

## Usage cycles

These are the real product loops the API is built for.

### Cycle A — Public website (no login)

```
Home slider  →  Hotels list / lookup  →  Hotel details + rooms
            →  About Us + section images
            →  Contact Us + working hours
            →  Insight (restaurant / cafe / menus)
```

Typical calls:

1. `GET /api/HomeSlider`
2. `GET /api/Hotel` or `GET /api/Hotel/lookup`
3. `GET /api/Hotel/{encodedId}`
4. `GET /api/Room/hotel/{encodedHotelId}`
5. `GET /api/InsightHotel/{encodedHotelId}`
6. `GET /api/AboutUs`, `GET /api/section-images?type=aboutUs`
7. `GET /api/ContactUs`

### Cycle B — Check dates, price, and book (guest)

Availability **does not currently block** creating a booking (`CheckAvailabilityAsync` is commented out in `CreateBookingAsync`). Availability is still useful for the UI.

```
Pick hotel + dates
  → POST /api/hotels/{encodedHotelId}/rooms     (which rooms overlap existing bookings)
  → POST /api/hotels/calculate-price            (nights × rate × 1.28)
  → POST /api/bookings                          (creates Pending, unpaid)
```

`POST /api/bookings` body:

```json
{
  "encodedHotelId": "...",
  "encodedRoomId": "...",
  "checkIn": "2026-10-01",
  "checkOut": "2026-10-03",
  "customerName": "Guest Name",
  "customerEmail": "guest@example.com"
}
```

Response includes encoded booking id, `totalPrice`, `status: Pending`, `isPaid: false`.

**This endpoint is not authenticated.** Anyone who knows encoded hotel/room ids can create a reservation.

### Cycle C — Pay with Paymob (client JWT)

```
Login  →  POST /api/Payment/test  (creates Paymob payment link, stores Paymob order id on booking)
       →  Guest opens client_url and pays
       →  Either:
            Paymob webhook  POST /api/Webhook/callback
            or client poll  GET /api/Payment/check-order/{paymobOrderId}
       →  Booking: IsPaid = true, Status = Confirmed
```

`POST /api/Payment/test` requires `[Authorize]`. Body includes `encodedBookingId`, `amountCents`, `currency`, `billingData`, `items`.

The API:

1. Authenticates with Paymob (`/api/auth/tokens`).
2. Creates a payment link (`/api/ecommerce/payment-links`) using **test** integration id when `isLive` is false.
3. Saves Paymob `order` id as `Booking.PaymentReference`.

Confirmation:

- **Webhook:** if `success` is true, the API re-fetches Paymob order status. If `payment_status == "PAID"` and `is_paid`, it confirms the booking.
- **Polling:** same confirmation logic for the logged-in client.
- **Legacy:** `POST /api/bookings/confirm-payment?paymentRef=` is **unauthenticated** and confirms by payment reference without talking to Paymob.

HMAC signature validation (`ValidateHmacSignature`) is **not implemented**. The webhook trusts the payload then re-queries Paymob.

### Cycle D — Client account and “my bookings”

```
POST /api/Account/register   →  Client role
POST /api/Account/login      →  JWT (claims: Id, UserName, Email, role)
GET  /api/bookings/my        →  bookings for token Email (optional hotelId, from, to, status)

Forgot password:
  POST /api/Account/forgot-password  →  email with Angular reset URL
  POST /api/Account/reset-password
```

JWT lifetime: **1 hour**.

### Cycle E — Admin CMS (content and inventory)

Most hotel/room/user/CMS write endpoints have `[Authorize(Roles = "Admin")]` **commented out**, so they are currently public. Only `GET /api/admin/bookings` is actually role-protected.

Admin loop:

```
Upload files  POST /api/upload?type=hotel|room|logo|...
  → Create/update Hotel (attach image/logo ids)
  → Create/update Rooms for that hotel
  → Optional InsightHotel (restaurant/cafe/menus)
  → Home slider, About Us, section images, Contact Us
  → Users CRUD  /api/User
  → List all bookings  GET /api/admin/bookings
```

### Cycle F — Hotel partners (B2B email)

```
CRUD partners  /api/Partner
Link partners to hotels
POST /api/Partner/broadcastEmail/hotel/{encodedId}
  → emails all partners of that hotel (subject, body, attachments, hotel signature HTML)
```

Used for operational messages such as stop-sales notices.

---

## API reference

Base URL (dev): `https://localhost:7142` or `http://localhost:5141`.

### Account — `/api/Account`

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| POST | `/login` | No | JWT |
| POST | `/register` | No | Creates Client |
| POST | `/forgot-password` | No | SMTP reset link |
| POST | `/reset-password` | No | Apply new password |

### Hotels & rooms

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/Hotel` | All hotels (images + rooms) |
| GET | `/api/Hotel/lookup` | Encoded id + names only |
| GET | `/api/Hotel/{encodedId}` | One hotel |
| POST / PUT / DELETE | `/api/Hotel` … | Create / update / delete |
| GET | `/api/Room` | All rooms |
| GET | `/api/Room/{encodedId}` | One room |
| GET | `/api/Room/hotel/{encodedId}` | Rooms for hotel |
| POST / PUT / DELETE | `/api/Room` … | CRUD |

### Availability & booking

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/hotels/{encodedHotelId}/rooms` | No | Rooms + `isBooked` for date range |
| POST | `/api/hotels/calculate-price` | No | Quote |
| POST | `/api/bookings` | No | Create `Pending` booking |
| POST | `/api/bookings/confirm-payment` | No | Force-confirm by Paymob order id |
| GET | `/api/bookings/my` | JWT | Guest bookings (filter hotel/dates/status) |
| GET | `/api/admin/bookings` | JWT **Admin** | All bookings |

### Payment

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/Payment/test` | JWT | Create Paymob link + attach order id |
| GET | `/api/Payment/check-order/{paymobOrderId}` | JWT | Poll Paymob and confirm if paid |
| POST | `/api/Webhook/callback` | No | Paymob server callback |

### CMS & media

| Method | Path | Description |
| --- | --- | --- |
| POST | `/api/upload?type=` | Multipart files → image/PDF ids |
| GET/POST/PUT/DELETE | `/api/HomeSlider` | Homepage slides |
| GET/PUT | `/api/AboutUs` | About content |
| GET/PUT | `/api/section-images?type=` | Images by `PlazaInnType` |
| GET/PUT | `/api/ContactUs` | Contact + hours |
| CRUD | `/api/InsightHotel/{encodedHotelId}` | F&B insights |
| CRUD + broadcast | `/api/Partner` | Partners and hotel emails |
| CRUD | `/api/User` | Identity users and roles |

---

## Architecture notes (review)

### What is in good shape

- Clear **layered** split (API / Core / Repository / Service / Shared).
- **Specification pattern** for hotels, rooms, bookings, contact, section images.
- Encoded ids so the SPA does not expose sequential database keys.
- Payment path talks to Paymob with structured errors (`PaymentGatewayException`) and logging.
- SQL Server retry on transient failures.
- Global exception middleware for `ArgumentException`, `NotFoundException`, and generic 500s.
- Bilingual content fields throughout hotels/rooms/insights.
- File upload with hashing/dedup intent in `FileService`.
- Partner broadcast with concurrency limit (5 parallel emails) and hotel HTML signature.

### Gaps and risks (priority)

1. **Secrets in source control** — connection string, JWT key, SMTP app password, and Paymob test keys live in `appsettings*.json`. Rotate them and move to User Secrets / environment variables. Never commit live keys.
2. **Admin authorization is mostly off** — Hotel, Room, User, File, Insight, Partner mutations are public. Only admin booking list is `[Authorize(Roles = "Admin")]`.
3. **Double booking** — `CreateBookingAsync` does not call `CheckAvailabilityAsync`. Overlapping dates on the same room are allowed. Availability also counts **all** overlapping bookings (including unpaid `Pending`), which can block rooms that never get paid.
4. **Unprotected payment confirm** — `POST /api/bookings/confirm-payment` can mark a booking paid without a gateway check.
5. **Webhook HMAC unused** — `ValidateHmacSignature` throws `NotImplementedException`. Relying only on a follow-up GET to Paymob is better than nothing, but HMAC should still be verified.
6. **Live vs test Paymob** — `GetOrderStatusAsync` always requests a **test** token (`isLive: false`). Production confirmations will break until this follows the same live/test flag as link creation.
7. **Admin seed email is whitespace** — `DefaultUser` may never create a usable admin.
8. **JWT role claim** — tokens emit claim type `"role"`. Confirm `[Authorize(Roles = "Admin")]` maps that correctly in your host (it often does via inbound claim mapping; verify with a real Admin token).
9. **CORS `AllowAnyOrigin`** + JWT + public writes is unsafe for production.
10. **List endpoints load all rows then filter in memory** (`GetUserBookingsAsync`, `GetAdminBookingsAsync`) — will not scale.
11. **Missing not-found semantics** — several GETs return `200 OK` with empty body instead of `404`.
12. **No automated tests**, leftover commented code, mixed namespaces (`Plaza`, `PlazaAPI`, `PlazaApi`).
13. **IdEncoder is obfuscation, not encryption** — the secret is in the client-visible algorithm; treat it as hiding ids, not as access control.
14. **Password reset** hard-codes `localhost:4200`; configure a public frontend URL per environment.

### Suggested next steps

- Turn Admin `[Authorize]` back on for all write APIs; keep GET catalog public.
- Enforce availability (and optionally expire unpaid `Pending` bookings) inside `CreateBookingAsync` with a transaction.
- Remove or protect `confirm-payment`; keep webhook + authenticated poll only.
- Implement HMAC; pass `isLive` into order-status calls.
- Move secrets out of the repo and rotate anything that was committed.
- Fix admin seed; add integration tests for booking + payment confirmation.
- Push booking filters into SQL specifications instead of `GetAllAsync()`.

---

## Frontend contract (for SPA authors)

- Send `Authorization: Bearer <token>` for login-only routes.
- Use **encoded** hotel/room/booking ids everywhere except Identity user ids (those are GUID strings).
- After creating a booking, start payment with the encoded booking id and amount in **cents**.
- After Paymob redirect, poll `GET /api/Payment/check-order/{paymobOrderId}` until paid, and/or rely on the webhook.
- Display `Name` / `NameAr` (and other `*Ar` fields) from the same payloads.
- Upload media first (`/api/upload`), then persist returned ids on hotel/room/slider DTOs.

---

## License / ownership

Private Plaza Inn backend. No license file is included in this repository.
