# Plaza Inn API — Developer Documentation and Testing Guide

This guide is generated from the controllers, DTOs, services, and middleware in `Plaza_SOL` (ASP.NET Core 8). Routes, query parameters, and JSON property names match the code. Property names in JSON are **camelCase**.

There is no frontend in this repository. The intended client is an Angular app on `http://localhost:4200`.

**Do not copy secrets out of `Plaza/appsettings.json` into clients or tickets.** That file currently contains a SQL connection string, a JWT signing key, SMTP credentials, and Paymob test secrets. This guide names the configuration keys only. The seeded admin email and password below are the values the application is written to create.

---

## 1. How to Run & Access Swagger / كيفية تشغيل المشروع محلياً

### Prerequisites

- .NET 8 SDK
- SQL Server reachable by `ConnectionStrings:DefaultConnection`
- EF Core tools, matching the repository package (`Microsoft.EntityFrameworkCore.Tools` 8.0.20):

```powershell
dotnet tool install --global dotnet-ef --version 8.0.20
```

Run every command below from the solution root (`Plaza_SOL`, the folder that contains `Plaza_SOL.sln`).

### Configure the database

`Program.cs` does **not** call `Database.Migrate()` on startup. Apply migrations yourself before the first run. Point `ConnectionStrings:DefaultConnection` in `Plaza/appsettings.json` (or user secrets / environment variables) at the SQL Server database you intend to use.

### Build, migrate, and run

```powershell
dotnet build Plaza_SOL.sln
dotnet ef database update --project PlazaRepository --startup-project Plaza
dotnet run --project Plaza
```

`dotnet run --project Plaza` uses the first launch profile in `Plaza/Properties/launchSettings.json`, which is `http`:

| Setting | Value |
| --- | --- |
| URL | `http://localhost:5141` |
| Swagger UI | `http://localhost:5141/swagger` |
| OpenAPI JSON | `http://localhost:5141/swagger/v1/swagger.json` |
| Health check | `GET http://localhost:5141/health` |
| Environment | `Development` (`ASPNETCORE_ENVIRONMENT`) |

Swagger UI is registered for every environment (`app.UseSwagger()` / `app.UseSwaggerUI()`), not only Development. HTTPS redirection is off unless `UseHttpsRedirection` is `true`. The optional `https` profile is `https://localhost:7142` and `http://localhost:5141`:

```powershell
dotnet run --project Plaza --launch-profile https
```

Swagger for that profile: `https://localhost:7142/swagger`.

Startup also seeds Identity roles and the admin user. Seeding failure is logged and the API still starts. A missing migration or an unreachable database will fail the `/health` database check and any data endpoint.

### Seeded admin

Roles created by `DefaultRole`: `Admin`, `Client`, `User`.

Admin created by `DefaultUser`, overridden by configuration when present:

| Config key | Default in code and in `appsettings.json` |
| --- | --- |
| `AdminSeed:Email` | `admin@plaza.com` |
| `AdminSeed:Password` | `Admin@123` |

`EmailConfirmed` is set to `true`. The user is added to the `Admin` role.

### Log in and authorize Swagger

1. Open `http://localhost:5141/swagger`.
2. Expand **POST** `/api/Account/login`.
3. Click **Try it out** and send:

```json
{
  "email": "admin@plaza.com",
  "password": "Admin@123"
}
```

4. A successful response is HTTP 200:

```json
{
  "message": "Login successful.",
  "token": "<jwt>"
}
```

5. Copy the `token` value only. Do not include the word `Bearer` in the Swagger dialog. The security scheme is HTTP bearer (`BearerFormat: JWT`), and Swagger adds the `Bearer` prefix.
6. Click **Authorize** at the top of Swagger UI, paste the token, and confirm.
7. Protected calls then send `Authorization: Bearer <jwt>`.

The token expires **1 hour** after issue (`JwtTokenGenerator`). Clock skew is zero. Claims used by the API:

| Claim | Meaning |
| --- | --- |
| `Id` | ASP.NET Identity user id. Required by booking and payment actions. |
| `Email` | Stored on the booking as `customerEmail` when the claim is present. |
| `UserName` | Name claim type. |
| `role` | `Admin`, `Client`, or `User`. Role checks use this claim (`RoleClaimType = "role"`). |

Issuer is `Jwt:Issuer` (`PlazaAPI`). Audience is `Jwt:Audience` (`PlazaClient`).

Public registration (`POST /api/Account/register`) always assigns the **Client** role. Use the admin user above, or `POST /api/User` as an admin, when you need the Admin role.

---

## 2. System Lifecycle & Entity Relationships / دورة العمل الكاملة والترابط بين الموديولات

### 2.1 File uploads (`FileController`) and `PlazaInnType`

`POST /api/upload` is admin-only. It accepts `multipart/form-data` with one or more files in the form field **`files`**, plus a query parameter **`type`** of enum `PlazaInnType`.

The upload returns a JSON array of **raw integer image ids**, for example `[12, 13]`. Those integers are what create/update DTOs expect in `imageIds` (and the related id lists below). They are not encoded.

| `type` query value | Integer | Stored under | Allowed files | Passed into |
| --- | --- | --- | --- | --- |
| `hotel` | 0 | `wwwroot/images/hotel/` | `.jpg`, `.jpeg`, `.png`, `.webp` | `CreateHotelDto.imageIds` |
| `room` | 1 | `wwwroot/images/room/` | images | `CreateRoomDTO.imageIds` |
| `restaurant` | 2 | `wwwroot/images/restaurant/` | images | `CreateInsightHotelDto.restaurantImageIds` |
| `cafe` | 3 | `wwwroot/images/cafe/` | images | `cafeImageIds` |
| `restaurantMenu` | 4 | `wwwroot/menus/restaurantMenu/` | `.pdf` only | `restaurantMenuPdfIds` |
| `cafeMenu` | 5 | `wwwroot/menus/cafeMenu/` | `.pdf` only | `cafeMenuPdfIds` |
| `logo` | 6 | `wwwroot/images/logo/` | images | `CreateHotelDto.logoId` (single int) |
| `homeSlider` | 7 | `wwwroot/images/homeSlider/` | images | `CreateHomeSliderDto.imagesId` |
| `aboutUs` | 8 | `wwwroot/images/aboutUs/` | images | `UpdateAboutUsDto` square/circle ids |
| `None` | 9 | — | rejected by upload | used internally when a section image is detached |
| `trip` | 10 | `wwwroot/images/trip/` | images | `CreateTripDto.imageIds` |
| `transport` | 11 | `wwwroot/images/transport/` | images | `CreateTransportDto.imageIds` |
| `event` | 12 | `wwwroot/images/event/` | images | `CreateEventDto.imageIds` |
| `package` | 13 | `wwwroot/images/package/` | images | `CreatePackageDto.imageIds` |

`None` and any undefined integer are rejected with 400 `"Invalid file type."`.

Limits enforced by `FileController` and `FileService`:

- Whole request: 50 MB (`RequestSizeLimit`).
- Each file: 10 MB.
- Empty files are rejected.
- The extension must match the file signature (JPEG, PNG, WEBP, or PDF).
- A file whose SHA-256 hash already exists for the same `PlazaInnType` is not stored again. The existing image id is returned.

On hotel, room, trip, transport, event, package, insight, home slider, and about-us responses, those ids are expanded to `{ "id": <int>, "url": "<BaseUrl><relative path>" }`. `BaseUrl` in configuration is `http://plazainn.runasp.net/`. Example: `http://plazainn.runasp.net/images/trip/12.jpg`.

`GET /api/section-images` is different: it returns the relative path stored on the `Image` row (`images/trip/12.jpg`), not the absolute URL.

Typical sequence:

1. `POST /api/upload?type=trip` with the image files.
2. Read the integer ids from the response.
3. Send them as `imageIds` on `POST /api/Trip` (or the matching property on update).

### 2.2 ID encoding (`IdEncoder`)

Integer primary keys for hotels, rooms, trips, transports, events, packages, partners, and bookings are not exposed as raw numbers on those resources or on the admin dashboard. `IdEncoder` builds a URL-safe Base64 string of `{id}:PlazaInnKey2025` (UTF-8), then strips padding and replaces `+` with `-` and `/` with `_`.

Example: database id `1` encodes to `MTpQbGF6YUlubktleTIwMjU`.

| The API returns or expects an encoded string | The API uses a raw id |
| --- | --- |
| Hotel, room, trip, transport, event, package, partner, booking route ids (`encodedId`) | `POST /api/upload` response and every `imageIds` / `imagesId` / `logoId` / menu id list |
| `encodedHotelId`, `encodedRoomId`, `encodedTripId`, `encodedTransportId`, `encodedEventId`, `encodedPackageId`, `encodedBookingId` | `GET /api/bookings/my` and `GET /api/admin/bookings` query `hotelId` (the hotel’s integer primary key) |
| Package composition: `hotelId`, `roomId`, `tripIds`, `transportIds`, `eventIds` (encoded strings) | `GET /api/HomeSlider/{id}` and `AboutUsDto.id`, `ContactUsDto.id`, `HomeSliderDto.id`, `WorkingHourDto.id` |
| Lookup payloads: `{ "id": "<encoded>", "name", "nameAr" }` | `GET /api/User/{userId}` — ASP.NET Identity user id string (GUID) |
| | `GET /api/Payment/check-order/{paymobOrderId}` — Paymob’s numeric order id |

`IdEncoder.DecodeId` throws `ArgumentException("Invalid ID")` when the string is not a valid token. Controllers that catch it return 400 with a plain string such as `"Invalid hotel ID"`. `GET/cancel` on `/api/bookings/{encodedId}` do not catch it; the exception middleware turns it into a 400 `ProblemDetails` whose `detail` is `"Invalid ID"`.

Catalog list and get actions (`Trip`, `Transport`, `Event`, `Package`) hide rows with `isActive: false` from anonymous callers and from non-admin tokens. An admin token sees inactive rows. A non-admin who requests an inactive encoded id receives **404**.

### 2.3 Standalone services and bundled packages

`Trip`, `Transport`, and `Event` are independent catalogs. Each can be listed, fetched, and booked on its own. An admin can also attach any combination of them to a `Package`.

A package has its own `packagePrice` and `maxPackagesAvailable`. The price charged at booking time is **not** the sum of the component prices. It is `quantity * packagePrice`, then the booking tax (see below).

Optional stay trio on `CreatePackageDto` / `UpdatePackageDto`:

- `hotelId`, `roomId`, and `nightsCount` must be sent **together**, or all omitted.
- `hotelId` and `roomId` are encoded ids. The room must belong to that hotel.
- `nightsCount` is an integer from 1 to 365.
- If any one of the three is present and another is missing, validation returns 400: `"Hotel, room, and nights must be provided together."`

The package must include at least one item: the hotel trio, or at least one id in `tripIds`, `transportIds`, or `eventIds`. All of them may be set at once.

Capacity is **not** shared between a package and its trip, transport, or event components:

- A standalone trip booking consumes `trip.maxCapacity` (guest count) for that service date.
- A standalone transport booking consumes `availableVehicles` for that date.
- A standalone event booking consumes `totalTickets`.
- A package booking consumes `maxPackagesAvailable` only.
- If the package includes the hotel/room/nights trio, the booking also consumes that many rooms from the hotel room pool for `nightsCount` nights starting on `serviceDate`. That pool **is** shared with normal hotel bookings.

Public catalog reads are anonymous. Creating, updating, and deleting catalog rows requires `Admin`.

### 2.4 Booking and Paymob payment lifecycle

Configured under `Booking` (defaults in `BookingOptions` and `appsettings.json`):

| Key | Value | Effect |
| --- | --- | --- |
| `TaxRate` | `0.28` | Hotel and catalog totals are multiplied by `1.28` and rounded to 2 decimal places (midpoint away from zero). |
| `PendingExpiryMinutes` | `30` | A `Pending` booking holds inventory for 30 minutes from `createdAt` (UTC). |
| `CleanupIntervalMinutes` | `5` | `ExpiredBookingsCleanupService` runs on this interval. |

**Step 1 — `POST /api/bookings` (any authenticated user).**

The action is rate-limited (5 creates per 10 minutes per user id). The body is `BookingRequestDto`. `bookingType` defaults to `Hotel` (`0`) when omitted.

The insert runs inside a transaction with **Serializable** isolation so the capacity read and the insert cannot overbook.

- Status is set to `Pending`, `isPaid` is `false`.
- A merchant order id `PLZ-{guid:N}` is stored.
- `userId` comes from the JWT `Id` claim.
- If the JWT has an `Email` claim, that email **replaces** `customerEmail` on the saved booking.
- Inventory that counts: `Confirmed` bookings, and `Pending` bookings whose `createdAt` is still inside the 30-minute window. `Cancelled` rows and expired pending holds do not count.

Hotel (`bookingType` 0):

- Requires `encodedHotelId`, `encodedRoomId`, `checkIn`, `checkOut`, customer fields.
- The room must belong to the hotel.
- Check-in date cannot be before today (UTC). Stay length is 1 to 30 nights (`StayRules.MaxStayNights`).
- `roomsCount` is 1–20. Guests (`adults + children`) cannot exceed `maxSleep * roomsCount`, plus 1 when `extraBed` is true.
- `extraBed: true` requires `room.extraBed == true`.
- Price: `((pricePerNight * roomsCount) + (extraBed ? priceForExtraBed : 0)) * nights * 1.28`.
- Availability uses two pools on the hotel: `numOfAvailableRoomsToReserve` for `Single` and `Double`, and `numOfAvailableSuitesToReserve` for `Suite`. The busiest night in the stay wins. A row with `roomsBooked = 0` still occupies 1 room.
- Insufficient rooms → **409**.

Trip (`1`): `encodedTripId` and `serviceDate` required. Guests = `adults + children` (at least 1). `quantity` on the request is ignored; the stored quantity is the guest count. Date must fall inside `startDate`/`endDate` when those are set, and cannot be in the past (UTC). Price: `(adults * pricePerPerson + children * pricePerChild) * 1.28`. Over capacity → **409**.

Transport (`2`): `encodedTransportId` and `serviceDate` required. `quantity` is the number of vehicles (1–1000). Passengers (`adults + children`) cannot exceed `quantity * maxPassengers`. Price: `quantity * pricePerTrip * 1.28`.

Event (`3`): `encodedEventId` required. `quantity` is the ticket count. `serviceDate` is optional; when sent it must fall on the event dates, but the **stored** `serviceDate` is the catalog `eventDate`. Price: `quantity * ticketPrice * 1.28`. Ended events → **409**.

Package (`4`): `encodedPackageId` required. `quantity` is how many packages to buy. Price: `quantity * packagePrice * 1.28`. `serviceDate` is required when the package has a room or a start/end date. With a room, check-in is `serviceDate` and check-out is `serviceDate + nightsCount`, and that many rooms must be free.

Inactive catalog items → **409** `"This {item} is not available."` Missing rows → **404**.

**Step 2 — `POST /api/Payment/initiate` (alias `POST /api/Payment/test`).**

Any authenticated user who owns the booking, or an admin, may call it. The server loads the booking and:

- Rejects anything other than `Pending` with **409**.
- Rejects a hold older than 30 minutes with **409** `"The booking hold has expired. Please create a new booking."`
- Computes `amountCents` as `round(totalPrice * 100)`. `amountCents`, `currency`, and `items` on the request are ignored.
- Asks Paymob Accept (`https://accept.paymob.com/api`) for a payment link. `Paymob:IsLive` is `false`, so test keys under `PaymobTest` are used. Currency is `Paymob:Currency` or `EGP`.
- Each attempt gets a new merchant order id `PLZ-{bookingId}-{guid:N}` because Paymob rejects a reused merchant order id. That id, and Paymob’s order id, are saved on the booking (`paymentReference` = Paymob order id).

Response:

```json
{
  "paymentUrl": "https://accept.paymob.com/...",
  "paymobOrderId": 123456789,
  "amountCents": 128000,
  "currency": "EGP"
}
```

The guest opens `paymentUrl` and pays.

**Step 3 — confirmation, either path.**

- Paymob server callback: **`POST /api/Webhook/callback?hmac=<signature>`**. There is no `/api/Webhook/paymob` route. The body must contain `obj` (the transaction). `type`, when present, must be `TRANSACTION`; any other type returns 200 and is ignored. The HMAC is validated against the transaction fields. `success` must be true and `pending` must not be true. The API then calls Paymob again (`GetOrderStatusAsync`) and does not trust the body alone.
- Client poll: **`GET /api/Payment/check-order/{paymobOrderId}`** (authenticated owner or admin). Same confirmation method when Paymob reports the order paid.

`ConfirmBookingPaymentAsync` (Serializable transaction):

- Finds the booking by `paymentReference`, or, if a newer payment attempt replaced it, by the booking id embedded in `PLZ-{bookingId}-{guid}`.
- If already `Confirmed` for this order, returns success (idempotent). A second successful payment on an already confirmed booking is logged for a manual refund and still returns success.
- Paid cents must equal the expected cents or the booking stays unchanged.
- A late payment on an expired or cancelled hold is confirmed only when the inventory is still free. Otherwise it is logged for a manual refund and `booking_confirmed` is false.
- On success: `isPaid = true`, `status = Confirmed`, `paymentReference` updated.

**Step 4 — expiry.**

`ExpiredBookingsCleanupService` selects `Pending` rows with `createdAt` older than 30 minutes and sets `status` to `Cancelled`. Until that job runs, an expired hold already stops counting as inventory, and payment initiation is rejected.

**Cancel.** `POST /api/bookings/{encodedId}/cancel`. The owner may cancel a `Pending` booking. A `Confirmed` booking can be cancelled only by an admin (**409** otherwise). Cancelling an already `Cancelled` booking returns it unchanged. No Paymob refund is issued.

---

## 3. Full API Reference & Copy-Paste JSON Payloads / دليل الـ Endpoints الكامل مع أمثلة JSON

### Conventions used in every sample

**Authorization on a controller**

- No attribute, or `[AllowAnonymous]`: no token.
- `[Authorize]`: any valid JWT (`Admin`, `Client`, or `User`).
- `[Authorize(Roles = "Admin")]`: JWT whose `role` claim is `Admin`. A valid token with another role receives **403** with an empty body (the framework challenge), not `ProblemDetails`.

Class-level `[Authorize(Roles = "Admin")]` is the default for Hotel, Room, InsightHotel, Partner, User, Trip, Transport, Event, Package, File, and AdminDashboard. Methods marked `[AllowAnonymous]` override that for public reads.

**Enums in JSON bodies are integers.** There is no `JsonStringEnumConverter`. A string such as `"Hotel"` fails JSON deserialization with **400**. Query-string model binding does accept names (`?status=Pending`, `?bookingType=Trip`, `?type=trip`).

| Enum | Name = value |
| --- | --- |
| `BookingType` | `Hotel=0`, `Trip=1`, `Transport=2`, `Event=3`, `Package=4` |
| `BookingStatus` | `Pending=0`, `Confirmed=1`, `Cancelled=2` |
| `RoomType` | `Single=0`, `Double=1`, `Suite=2` |
| `Role` | `Admin=1`, `Client=2`, `User=3` (`0` is invalid) |
| `DayOfWeek` | `Sunday=0` … `Saturday=6` |
| `PlazaInnType` | see section 2.1 |

Dates are ISO-8601. The booking service uses the `.Date` (UTC calendar day). Send date-only values such as `"2026-11-10"`.

Encoded ids in the samples use the real encoding of database id `1` (`MTpQbGF6YUlubktleTIwMjU`) so the shape is real. Replace them with ids returned by create/list/lookup calls.

Validation failures from `[ApiController]` return **400** `ValidationProblemDetails` (`application/problem+json`) **before** the action runs:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Hotel name (English) is required"]
  },
  "traceId": "00-..."
}
```

Service exceptions use the shape in section 5. Several actions instead return `BadRequest("some string")`, which is a JSON **string**, not `ProblemDetails`.

---

### 3.1 Account / Auth — `api/Account`

No `[Authorize]`. Each action uses the `auth` rate limit: **10 requests per minute per client IP**.

#### `POST /api/Account/login`

Auth: anonymous. Body: `LoginDTO`.

| Field | Rules |
| --- | --- |
| `email` | Required, email format |
| `password` | Required, minimum 6 characters |

```json
{
  "email": "admin@plaza.com",
  "password": "Admin@123"
}
```

200:

```json
{
  "message": "Login successful.",
  "token": "<jwt>"
}
```

401 (unknown user, bad password):

```json
{ "message": "Invalid email or password." }
```

401 after 5 failed passwords for an existing user (lockout 15 minutes):

```json
{ "message": "Account is temporarily locked due to multiple failed login attempts. Please try again later." }
```

#### `POST /api/Account/register`

Body: `RegisterDTO`. Creates an Identity user and adds the **Client** role. Identity password rules: minimum 8, digit, lowercase, uppercase, special character not required.

| Field | Rules |
| --- | --- |
| `userName` | Required, 3–50 characters |
| `email` | Required, email format, must be unique |
| `password` | Required, 8–100 characters |
| `passwordConfirm` | Required, must equal `password` |

```json
{
  "userName": "sara.hassan",
  "email": "sara@example.com",
  "password": "Sara@1234",
  "passwordConfirm": "Sara@1234"
}
```

200:

```json
{ "message": "Registration successful." }
```

400 when the email exists or Identity rejects the password:

```json
{ "message": "Email is already registered." }
```

#### `POST /api/Account/forgot-password`

Body: `ForgotPasswordDTO`. Always returns the same 200, whether or not the email exists. When the user exists, an email is sent with a link to `{FrontendBaseUrl}/reset-password?email=...&token=...` (`FrontendBaseUrl` is `http://localhost:4200`).

```json
{ "email": "sara@example.com" }
```

200:

```json
{ "message": "If an account with that email exists, a password reset link has been sent." }
```

#### `POST /api/Account/reset-password`

Body: `ResetPasswordDTO`. The action URL-decodes `token` before calling Identity.

| Field | Rules |
| --- | --- |
| `email` | Required, email format |
| `token` | Required (the token from the email link) |
| `newPassword` | Required, 8–100 characters (Identity complexity still applies) |
| `confirmPassword` | Must equal `newPassword` |

```json
{
  "email": "sara@example.com",
  "token": "<token-from-email>",
  "newPassword": "Sara@5678",
  "confirmPassword": "Sara@5678"
}
```

200:

```json
{ "message": "Password reset successful." }
```

400:

```json
{ "message": "Invalid or expired password reset request." }
```

A successful reset clears lockout.

---

### 3.2 User — `api/User`

Class auth: `[Authorize(Roles = "Admin")]`. User ids are Identity strings, not encoded integers.

#### `GET /api/User`

200: array of `UserDTO`. `role` is an integer.

```json
[
  {
    "id": "8f3c0a1e-1111-2222-3333-444455556666",
    "userName": "admin@plaza.com",
    "email": "admin@plaza.com",
    "role": 1
  }
]
```

#### `GET /api/User/{userId}`

404 with an empty body when the user does not exist.

```json
{
  "id": "8f3c0a1e-1111-2222-3333-444455556666",
  "userName": "sara.hassan",
  "email": "sara@example.com",
  "role": 2
}
```

#### `POST /api/User`

Body: `CreateUserDTO`.

| Field | Rules |
| --- | --- |
| `userName` | Required, 3–50 |
| `email` | Required, email format |
| `role` | `1` Admin, `2` Client, `3` User |
| `password` | Required, 8–100. Identity also requires upper, lower, and a digit |
| `passwordConfirmed` | Must equal `password`. The action also checks this and may return the plain string `"Password and confirmation do not match."` |

```json
{
  "userName": "operator.one",
  "email": "operator@plaza.com",
  "role": 1,
  "password": "Operator@123",
  "passwordConfirmed": "Operator@123"
}
```

200: the JSON string `"User created successfully."`

400 when Identity fails: an array of `{ "code", "description" }`.

#### `PUT /api/User/{id}`

Body: `updateUserDTO`. `email` cannot be changed. An empty `password` leaves the password unchanged. A non-empty password must match `passwordConfirmed` and satisfy Identity rules. `role` replaces the user’s current roles.

```json
{
  "userName": "operator.one",
  "role": 3,
  "password": "Operator@456",
  "passwordConfirmed": "Operator@456"
}
```

204 on success. 404 `ProblemDetails` (`"User not found."`) when the id does not exist. 400 with Identity error objects when the password is rejected.

#### `DELETE /api/User/{id}`

204 on success. 404 `ProblemDetails` when the user does not exist.

---

### 3.3 File — `POST /api/upload`

Auth: Admin. Not JSON. `multipart/form-data`.

| Part | Rules |
| --- | --- |
| Query `type` | `PlazaInnType` name or integer. Required in practice. `None` (9) → 400 `"Invalid file type."` |
| Form `files` | One or more files. At least one. See section 2.1 for extensions and the 10 MB / 50 MB limits |

Example (curl):

```powershell
curl -X POST "http://localhost:5141/api/upload?type=trip" `
  -H "Authorization: Bearer <token>" `
  -F "files=@C:\images\nile.jpg"
```

200: a JSON array of raw image ids.

```json
[12, 13]
```

400 body is a JSON string, for example `"No files uploaded."` or `"File 'menu.exe' has an unsupported extension. Allowed: .jpg, .jpeg, .png, .webp."`.

There is no list, get, or delete file endpoint.

---

### 3.4 Hotel — `api/Hotel`

Public: `GET`, `GET {encodedId}`, `GET lookup`. Mutations: Admin.

#### `GET /api/Hotel`

No query filters. 200: array of `HotelDto`.

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Plaza Nile",
    "nameAr": "بلازا النيل",
    "description": "A riverside hotel in central Cairo.",
    "descriptionAr": "فندق على النيل في وسط القاهرة.",
    "address": "12 Corniche El Nil",
    "addressAr": "12 كورنيش النيل",
    "city": "Cairo",
    "cityAr": "القاهرة",
    "location": "https://maps.example.com/plaza-nile",
    "phone": "0225550100",
    "email": "nile@plaza.com",
    "rating": 4.5,
    "instagram": "https://instagram.com/plazanile",
    "facebook": "https://facebook.com/plazanile",
    "whatsApp": "201001112233",
    "space": null,
    "numOfAvailableRoomsToReserve": 20,
    "numOfAvailableSuitesToReserve": 4,
    "images": [
      { "id": 12, "url": "http://plazainn.runasp.net/images/hotel/12.jpg" }
    ],
    "logo": { "id": 13, "url": "http://plazainn.runasp.net/images/logo/13.png" },
    "insight": null
  }
]
```

`insight` is populated when an `InsightHotel` row exists for the hotel (`InsightHotelDto`, section 3.7).

#### `GET /api/Hotel/{encodedId}`

400 body is the JSON string `"Invalid hotel ID"` when the token cannot be decoded. 404 empty body when the hotel does not exist. 200: one `HotelDto` (same shape as an element above).

#### `GET /api/Hotel/lookup`

Anonymous. 200:

```json
[
  { "id": "MTpQbGF6YUlubktleTIwMjU", "name": "Plaza Nile", "nameAr": "بلازا النيل" }
]
```

#### `POST /api/Hotel` and `PUT /api/Hotel/{encodedId}`

Body: `CreateHotelDto` (update is the same type, `UpdateHotelDTO`). `imageIds` and `logoId` are raw integers from `POST /api/upload`.

| Field | Rules |
| --- | --- |
| `name`, `nameAr` | Required, max 100 |
| `description`, `descriptionAr` | Required, max 500 |
| `address`, `addressAr` | Required, max 200 |
| `city`, `cityAr` | Required, max 50 |
| `location` | Required absolute URL |
| `phone` | Required, digits only, 6–15 digits |
| `email` | Required, email format |
| `numOfAvailableRoomsToReserve` | Integer, default 0. This is the bookable non-suite pool |
| `numOfAvailableSuitesToReserve` | Integer, default 0. Suite pool |
| `rating` | Number, default 0 |
| `instagram`, `facebook`, `whatsApp`, `space` | Optional strings |
| `imageIds` | At least 1 integer |
| `logoId` | Integer. `0` is accepted by validation |

```json
{
  "name": "Plaza Nile",
  "nameAr": "بلازا النيل",
  "description": "A riverside hotel in central Cairo.",
  "descriptionAr": "فندق على النيل في وسط القاهرة.",
  "address": "12 Corniche El Nil",
  "addressAr": "12 كورنيش النيل",
  "city": "Cairo",
  "cityAr": "القاهرة",
  "location": "https://maps.example.com/plaza-nile",
  "phone": "0225550100",
  "email": "nile@plaza.com",
  "numOfAvailableRoomsToReserve": 20,
  "numOfAvailableSuitesToReserve": 4,
  "rating": 4.5,
  "instagram": "https://instagram.com/plazanile",
  "facebook": "https://facebook.com/plazanile",
  "whatsApp": "201001112233",
  "space": null,
  "imageIds": [12],
  "logoId": 13
}
```

`POST` returns **201** with a `HotelDto` body and a `Location` header pointing at `GET /api/Hotel/{encodedId}`.

`PUT` returns **204**. Unknown encoded id → 400 string `"Invalid hotel ID"`. Unknown hotel → 404 empty body.

#### `DELETE /api/Hotel/{encodedId}`

204. Invalid token → 400 string `"Invalid hotel ID"`. Missing hotel → 404 empty body.

---

### 3.5 Room — `api/Room`

Public: `GET`, `GET {encodedId}`, `GET hotel/{encodedId}`. Mutations: Admin.

`RoomDTO` does not include `hotelId`. It includes `hotelName`. `type` is `RoomType` as an integer.

#### `GET /api/Room`

200: array of `RoomDTO`.

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Deluxe Double",
    "nameAr": "مزدوجة ديلوكس",
    "hotelName": "Plaza Nile",
    "type": 1,
    "pricePerNight": 2000.00,
    "extraBed": true,
    "priceForExtraBed": 400.00,
    "maxSleep": 3,
    "features": { "wifi": true, "seaView": false },
    "images": [
      { "id": 21, "url": "http://plazainn.runasp.net/images/room/21.jpg" }
    ]
  }
]
```

#### `GET /api/Room/{encodedId}`

400 string `"Invalid Room Id"`. 404 empty body when missing. 200: one `RoomDTO`.

#### `GET /api/Room/hotel/{encodedId}`

Rooms for one hotel. 400 string `"Invalid Hotel Id"`. Unknown hotel or a hotel with no rooms returns **200** and `[]` (not 404).

#### `POST /api/Room` and `PUT /api/Room/{encodedId}`

Body: `CreateRoomDTO` / `UpdateRoomDTO`.

| Field | Rules |
| --- | --- |
| `hotelId` | Required **encoded** hotel id |
| `name`, `nameAr` | Required |
| `type` | `0` Single, `1` Double, `2` Suite |
| `pricePerNight` | Greater than 0 |
| `features` | Map of string → bool. May be `{}` |
| `extraBed` | Bool. Default false |
| `priceForExtraBed` | Decimal. Used only when a booking sets `extraBed: true` |
| `maxSleep` | Integer. `0` disables the guest-capacity check |
| `imageIds` | At least 1 raw image id (`type=room`) |

```json
{
  "hotelId": "MTpQbGF6YUlubktleTIwMjU",
  "name": "Deluxe Double",
  "nameAr": "مزدوجة ديلوكس",
  "type": 1,
  "pricePerNight": 2000.00,
  "features": { "wifi": true, "seaView": false },
  "extraBed": true,
  "priceForExtraBed": 400.00,
  "maxSleep": 3,
  "imageIds": [21]
}
```

`POST` returns **201** `RoomDTO` (includes `hotelName` and absolute image URLs). `PUT` returns **204**. Invalid room token on update/delete → 400 string `"Invalid Room ID"`. Missing room → 404 empty body.

#### `DELETE /api/Room/{encodedId}`

204 on success.

---

### 3.6 Room availability and quote — `api/hotels`

`RoomAvailabilityController` has **no** `[Authorize]`. Both actions are public. This controller is not the hotel CRUD controller (`api/Hotel`).

#### `POST /api/hotels/{encodedHotelId}/rooms`

Body: `RoomAvailabilityRequestDto`. Shows every room of the hotel and how many are free for the stay. It does not create a booking.

| Field | Rules |
| --- | --- |
| `checkIn`, `checkOut` | Check-out must be 1–30 nights after check-in |
| `roomsCount` | 1–20, default 1. `isAvailable` is true when `availableRooms >= roomsCount` |

```json
{
  "checkIn": "2026-11-10",
  "checkOut": "2026-11-12",
  "roomsCount": 1
}
```

200:

```json
{
  "success": true,
  "data": [
    {
      "encodedRoomId": "MTpQbGF6YUlubktleTIwMjU",
      "name": "Deluxe Double",
      "nameAr": "مزدوجة ديلوكس",
      "pricePerNight": 2000.00,
      "extraBedAvailable": true,
      "priceForExtraBed": 400.00,
      "maxSleep": 3,
      "isAvailable": true,
      "availableRooms": 18,
      "images": [
        { "id": 21, "url": "http://plazainn.runasp.net/images/room/21.jpg" }
      ]
    }
  ]
}
```

Invalid encoded hotel id → 400 `ProblemDetails` `"Invalid ID"`. Unknown hotel → 404 `ProblemDetails` `"Hotel not found"`. Impossible stay → 400 `ProblemDetails` with the stay message.

#### `POST /api/hotels/calculate-price`

Body: `BookingQuoteRequestDto`. Does not persist anything. Same stay rules as a hotel booking, including guest capacity and extra bed.

| Field | Rules |
| --- | --- |
| `encodedHotelId` | Required (validated; the price itself is calculated from the room) |
| `encodedRoomId` | Required |
| `roomsCount` | 1–20, default 1 |
| `adults` | 1–50, default 1 |
| `children` | 0–50, default 0 |
| `checkIn`, `checkOut` | 1–30 nights |
| `extraBed` | Default false |

```json
{
  "encodedHotelId": "MTpQbGF6YUlubktleTIwMjU",
  "encodedRoomId": "MTpQbGF6YUlubktleTIwMjU",
  "roomsCount": 1,
  "adults": 2,
  "children": 0,
  "checkIn": "2026-11-10",
  "checkOut": "2026-11-12",
  "extraBed": false
}
```

For the sample room (2000 per night, 2 nights, tax 0.28) the total is `5120.00`.

200:

```json
{
  "success": true,
  "data": { "totalPrice": 5120.00 }
}
```

---

### 3.7 Insight hotel — `api/InsightHotel`

One insight row per hotel (restaurant and cafe copy, images, menu PDFs, facility flags).

Public: `GET /api/InsightHotel/{encodedHotelId}`. Create, update, delete: Admin.

#### `POST /api/InsightHotel`

Body: `CreateInsightHotelDto`. No data annotations. `encodedHotelId` is decoded in the action.

```json
{
  "encodedHotelId": "MTpQbGF6YUlubktleTIwMjU",
  "restaurantDescription": "Nile grill and Egyptian kitchen.",
  "restaurantDescriptionAr": "مشاوي نيلية ومطبخ مصري.",
  "cafeDescription": "All-day cafe.",
  "cafeDescriptionAr": "مقهى طوال اليوم.",
  "facilities": { "wifi": true, "outdoorSeating": true },
  "restaurantImageIds": [31],
  "cafeImageIds": [32],
  "restaurantMenuPdfIds": [33],
  "cafeMenuPdfIds": [34]
}
```

Upload the images with `type=restaurant` or `type=cafe`, and the PDFs with `type=restaurantMenu` or `type=cafeMenu`.

**201** body is the **request DTO** (raw id lists and `encodedHotelId`), not `InsightHotelDto`. `Location` points at `GET /api/InsightHotel/{encodedHotelId}`.

Invalid hotel token → 400 string `"Invalid hotel ID"`. Unknown hotel → 404 empty body.

#### `GET /api/InsightHotel/{encodedHotelId}`

Anonymous. 200 `InsightHotelDto`. Image and PDF properties are `FileDto` arrays despite the names `restaurantImageIds` and so on. Section-image relative URLs are not used here; mapping builds absolute URLs.

```json
{
  "restaurantDescription": "Nile grill and Egyptian kitchen.",
  "restaurantDescriptionAr": "مشاوي نيلية ومطبخ مصري.",
  "restaurantImageIds": [
    { "id": 31, "url": "http://plazainn.runasp.net/images/restaurant/31.jpg" }
  ],
  "restaurantMenuPdfIds": [
    { "id": 33, "url": "http://plazainn.runasp.net/menus/restaurantMenu/33.pdf" }
  ],
  "cafeDescription": "All-day cafe.",
  "cafeDescriptionAr": "مقهى طوال اليوم.",
  "cafeImageIds": [
    { "id": 32, "url": "http://plazainn.runasp.net/images/cafe/32.jpg" }
  ],
  "cafeMenuPdfIds": [
    { "id": 34, "url": "http://plazainn.runasp.net/menus/cafeMenu/34.pdf" }
  ],
  "facilities": { "wifi": true, "outdoorSeating": true }
}
```

400 string `"Invalid hotel ID"`. 404 empty body when the hotel has no insight row.

The same object is embedded as `insight` on `HotelDto` when the hotel is loaded with its insight.

#### `PUT /api/InsightHotel/{encodedHotelId}`

Body: `UpdateInsightHotelDto` (same properties as create, including `encodedHotelId`). The route id selects the row; the body `encodedHotelId` is not used to find it.

```json
{
  "encodedHotelId": "MTpQbGF6YUlubktleTIwMjU",
  "restaurantDescription": "Updated restaurant copy.",
  "restaurantDescriptionAr": "وصف المطعم بعد التعديل.",
  "cafeDescription": "Updated cafe copy.",
  "cafeDescriptionAr": "وصف المقهى بعد التعديل.",
  "facilities": { "wifi": true, "outdoorSeating": false },
  "restaurantImageIds": [31],
  "cafeImageIds": [32],
  "restaurantMenuPdfIds": [33],
  "cafeMenuPdfIds": [34]
}
```

204. Invalid token → 400 string. Missing insight → 404 empty body.

#### `DELETE /api/InsightHotel/{encodedHotelId}`

204. Invalid token → 400 string `"Invalid hotel ID"`. Missing insight → 404 empty body.

---

### 3.8 Partner — `api/Partner`

Every action requires Admin. There is no anonymous partner read. Hotel links use **encoded** hotel ids.

#### `GET /api/Partner`

200: array of `PartnerDto`.

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Nile Travel",
    "email": "partners@niletravel.com",
    "hotelsIds": ["MTpQbGF6YUlubktleTIwMjU"]
  }
]
```

#### `GET /api/Partner/{encodedId}`

200: one `PartnerDto`. Invalid token → 400 string `"Invalid hotel ID"` (the message says hotel even for a partner id). Missing partner → 404 empty body.

#### `GET /api/Partner/hotel/{encodedId}`

Partners linked to one hotel. 200: array of `PartnerOnlyDto` (no `hotelsIds`). An empty link set returns **200** `[]`.

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Nile Travel",
    "email": "partners@niletravel.com"
  }
]
```

Invalid hotel token → 400 string `"Invalid hotel ID"`.

#### `POST /api/Partner`

Body: `CreatePartnerDto`. No data annotations. `hotelsIds` entries are encoded hotel ids and are decoded with `IdEncoder.DecodeId` (invalid entry → 400 `ProblemDetails` `"Invalid ID"`).

```json
{
  "name": "Nile Travel",
  "email": "partners@niletravel.com",
  "hotelsIds": ["MTpQbGF6YUlubktleTIwMjU"]
}
```

**201** with an empty body (`Created()`).

#### `PUT /api/Partner/{encodedId}`

Body: `UpdatePartnerDto` (same properties). Replaces the hotel link set with the hotels that exist for the decoded ids.

```json
{
  "name": "Nile Travel",
  "email": "partners@niletravel.com",
  "hotelsIds": ["MTpQbGF6YUlubktleTIwMjU"]
}
```

204. Invalid partner token → 400 string `"Invalid partner ID"`. Missing partner → 404 empty body.

#### `DELETE /api/Partner/{encodedId}`

204. Invalid token → 400 string `"Invalid hotel ID"`. Missing partner → 404 empty body.

#### `POST /api/Partner/broadcastEmail/hotel/{encodedId}`

Not JSON. `multipart/form-data` (`SendEmailRequestDto`). Sends one email per distinct partner email linked to the hotel. The hotel signature HTML is appended to `body`. At most 5 sends run at once.

| Part | Rules |
| --- | --- |
| Form `subject` | String |
| Form `body` | String. HTML is allowed; the hotel signature is concatenated after it |
| Form `attachments` | Optional files, repeat the field for more than one |

```powershell
curl -X POST "http://localhost:5141/api/Partner/broadcastEmail/hotel/MTpQbGF6YUlubktleTIwMjU" `
  -H "Authorization: Bearer <token>" `
  -F "subject=Stop sales 12 Nov" `
  -F "body=<p>Please stop sales for 12 November.</p>" `
  -F "attachments=@C:\docs\notice.pdf"
```

200 when every send succeeds, or when the hotel has no partners:

```json
{ "message": "Emails sent to all partners successfully", "sent": 2, "failed": 0 }
```

```json
{ "message": "No partners found for this hotel" }
```

207 when at least one send throws:

```json
{ "message": "Emails sent to 1 partner(s); 1 failed.", "sent": 1, "failed": 1 }
```

Invalid hotel token → 400 string `"Invalid hotel ID"`. Unknown hotel → 400 string `"hotelId Not Found"`.

---

### 3.9 Trip — `api/Trip`

Public reads (inactive rows hidden unless the caller is Admin). Mutations: Admin.

Query object: `TripFilterDto`.

| Query | Behavior |
| --- | --- |
| `city` | Contains-match on `city` or `cityAr` (max 100) |
| `search` | Contains-match on `name` or `nameAr` (max 150) |
| `date` | Trip window overlaps that calendar day. Open-ended when start or end is null |
| `minPrice`, `maxPrice` | Compared to `pricePerPerson`. Min cannot exceed max |
| `isActive` | Admin only. `true` or omitted with a non-admin token returns active rows. `false` returns inactive rows for an admin. A non-admin cannot list inactive rows |

Example: `GET /api/Trip?city=Aswan&search=Philae&date=2026-11-15&minPrice=100&maxPrice=5000&isActive=true`

#### `GET /api/Trip`

200: array of `TripDto`.

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Philae Day Trip",
    "nameAr": "رحلة فيلة ليوم واحد",
    "description": "Boat to Philae and a guided visit.",
    "descriptionAr": "قارب إلى فيلة وزيارة بصحبة مرشد.",
    "city": "Aswan",
    "cityAr": "أسوان",
    "itinerary": "08:00 pickup, 16:00 return.",
    "itineraryAr": "التحرك 08:00 والعودة 16:00.",
    "durationDays": 1,
    "durationHours": 8,
    "pricePerPerson": 1500.00,
    "pricePerChild": 900.00,
    "maxCapacity": 20,
    "startDate": "2026-11-01T00:00:00",
    "endDate": "2026-12-31T00:00:00",
    "isActive": true,
    "images": [
      { "id": 41, "url": "http://plazainn.runasp.net/images/trip/41.jpg" }
    ]
  }
]
```

#### `GET /api/Trip/lookup`

Anonymous (inactive hidden for non-admins). No filters.

```json
[
  { "id": "MTpQbGF6YUlubktleTIwMjU", "name": "Philae Day Trip", "nameAr": "رحلة فيلة ليوم واحد" }
]
```

#### `GET /api/Trip/{encodedId}`

400 string `"Invalid trip ID"`. 404 empty body when missing or inactive for a non-admin. 200: one `TripDto`.

#### `POST /api/Trip` and `PUT /api/Trip/{encodedId}`

Body: `CreateTripDto` / `UpdateTripDto`.

| Field | Rules |
| --- | --- |
| `name`, `nameAr` | Required, max 150 |
| `description`, `descriptionAr` | Required, max 4000 |
| `city`, `cityAr` | Required, max 100 |
| `itinerary`, `itineraryAr` | Required, max 8000 |
| `durationDays` | 0–365 |
| `durationHours` | 0–8784 |
| | At least one of days or hours must be greater than 0 |
| `pricePerPerson`, `pricePerChild` | Greater than 0, up to `999999999999.99` |
| `maxCapacity` | 1–100000 |
| `startDate` | Optional. Required when `endDate` is set |
| `endDate` | Optional. Must be on or after `startDate` |
| `isActive` | Default true |
| `imageIds` | At least 1 raw id from `type=trip` |

```json
{
  "name": "Philae Day Trip",
  "nameAr": "رحلة فيلة ليوم واحد",
  "description": "Boat to Philae and a guided visit.",
  "descriptionAr": "قارب إلى فيلة وزيارة بصحبة مرشد.",
  "city": "Aswan",
  "cityAr": "أسوان",
  "itinerary": "08:00 pickup, 16:00 return.",
  "itineraryAr": "التحرك 08:00 والعودة 16:00.",
  "durationDays": 1,
  "durationHours": 8,
  "pricePerPerson": 1500.00,
  "pricePerChild": 900.00,
  "maxCapacity": 20,
  "startDate": "2026-11-01",
  "endDate": "2026-12-31",
  "isActive": true,
  "imageIds": [41]
}
```

`POST` returns **201** `TripDto`. `PUT` returns **204**. Invalid id → 400 string `"Invalid trip ID"`. Missing trip on update/delete → 404 empty body.

#### `DELETE /api/Trip/{encodedId}`

204. A trip that is still attached to a package fails the delete with **409** `ProblemDetails`: `"This trip is included in a package and cannot be deleted."`

---

### 3.10 Transport — `api/Transport`

Same authorization pattern as Trip.

Query object: `TransportFilterDto`.

| Query | Behavior |
| --- | --- |
| `search` | Contains-match on `name` or `nameAr` (max 150) |
| `vehicleType` | Contains-match on `vehicleType` or `vehicleTypeAr` (max 100) |
| `pickupLocation` | Contains-match on English or Arabic pickup (max 250) |
| `dropoffLocation` | Contains-match on English or Arabic dropoff (max 250) |
| `passengers` | 1–1000. Keeps vehicles whose `maxPassengers` is at least this number |
| `minPrice`, `maxPrice` | Compared to `pricePerTrip` |
| `isActive` | Same admin rule as trips |

Example: `GET /api/Transport?search=Sedan&vehicleType=Sedan&pickupLocation=Aswan&dropoffLocation=Philae&passengers=3&minPrice=100&maxPrice=3000&isActive=true`

#### `GET /api/Transport`

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Aswan Private Sedan",
    "nameAr": "سيارة خاصة في أسوان",
    "description": "Private sedan with driver.",
    "descriptionAr": "سيارة خاصة مع سائق.",
    "vehicleType": "Sedan",
    "vehicleTypeAr": "سيدان",
    "pickupLocation": "Aswan Station",
    "pickupLocationAr": "محطة أسوان",
    "dropoffLocation": "Philae Marina",
    "dropoffLocationAr": "مارينا فيلة",
    "maxPassengers": 3,
    "maxLuggage": 3,
    "pricePerTrip": 800.00,
    "availableVehicles": 5,
    "isActive": true,
    "images": [
      { "id": 51, "url": "http://plazainn.runasp.net/images/transport/51.jpg" }
    ]
  }
]
```

#### `GET /api/Transport/lookup`

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Aswan Private Sedan",
    "nameAr": "سيارة خاصة في أسوان",
    "vehicleType": "Sedan"
  }
]
```

#### `GET /api/Transport/{encodedId}`

400 string `"Invalid transport ID"`. 404 when missing or inactive for a non-admin. 200: one `TransportDto`.

#### `POST /api/Transport` and `PUT /api/Transport/{encodedId}`

| Field | Rules |
| --- | --- |
| `name`, `nameAr` | Required, max 150 |
| `description`, `descriptionAr` | Required, max 4000 |
| `vehicleType`, `vehicleTypeAr` | Required, max 100 |
| `pickupLocation`, `pickupLocationAr` | Required, max 250 |
| `dropoffLocation`, `dropoffLocationAr` | Required, max 250 |
| `maxPassengers` | 1–1000 |
| `maxLuggage` | 0–1000 |
| `pricePerTrip` | Greater than 0 |
| `availableVehicles` | 1–100000 |
| `isActive` | Default true |
| `imageIds` | At least 1 raw id from `type=transport` |

```json
{
  "name": "Aswan Private Sedan",
  "nameAr": "سيارة خاصة في أسوان",
  "description": "Private sedan with driver.",
  "descriptionAr": "سيارة خاصة مع سائق.",
  "vehicleType": "Sedan",
  "vehicleTypeAr": "سيدان",
  "pickupLocation": "Aswan Station",
  "pickupLocationAr": "محطة أسوان",
  "dropoffLocation": "Philae Marina",
  "dropoffLocationAr": "مارينا فيلة",
  "maxPassengers": 3,
  "maxLuggage": 3,
  "pricePerTrip": 800.00,
  "availableVehicles": 5,
  "isActive": true,
  "imageIds": [51]
}
```

`POST` **201** `TransportDto`. `PUT` **204**. Invalid id → 400 string `"Invalid transport ID"`.

#### `DELETE /api/Transport/{encodedId}`

204, or 409 when a package still includes it: `"This transport is included in a package and cannot be deleted."`

---

### 3.11 Event — `api/Event`

Query object: `EventFilterDto`.

| Query | Behavior |
| --- | --- |
| `search` | Contains-match on `name` or `nameAr` (max 150) |
| `location` | Contains-match on `location` or `locationAr` (max 250) |
| `date` | The calendar day intersects `[eventDate, endDate]` (`endDate` defaults to `eventDate`) |
| `minPrice`, `maxPrice` | Compared to `ticketPrice` |
| `isActive` | Same admin rule as trips |

Example: `GET /api/Event?search=Sound%20and%20Light&location=Philae&date=2026-11-20&minPrice=100&maxPrice=2000&isActive=true`

#### `GET /api/Event`

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Philae Sound and Light",
    "nameAr": "الصوت والضوء في فيلة",
    "description": "Evening sound and light show.",
    "descriptionAr": "عرض الصوت والضوء المسائي.",
    "location": "Philae Temple",
    "locationAr": "معبد فيلة",
    "eventDate": "2026-11-20T19:00:00",
    "endDate": "2026-11-20T21:00:00",
    "ticketPrice": 500.00,
    "totalTickets": 100,
    "isActive": true,
    "images": [
      { "id": 61, "url": "http://plazainn.runasp.net/images/event/61.jpg" }
    ]
  }
]
```

#### `GET /api/Event/lookup`

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Philae Sound and Light",
    "nameAr": "الصوت والضوء في فيلة",
    "eventDate": "2026-11-20T19:00:00"
  }
]
```

#### `GET /api/Event/{encodedId}`

400 string `"Invalid event ID"`. 404 when missing or inactive for a non-admin. 200: one `EventDto`.

#### `POST /api/Event` and `PUT /api/Event/{encodedId}`

| Field | Rules |
| --- | --- |
| `name`, `nameAr` | Required, max 150 |
| `description`, `descriptionAr` | Required, max 4000 |
| `location`, `locationAr` | Required, max 250 |
| `eventDate` | Required |
| `endDate` | Optional. Must be on or after `eventDate` |
| `ticketPrice` | Greater than 0 |
| `totalTickets` | 1–1000000 |
| `isActive` | Default true |
| `imageIds` | At least 1 raw id from `type=event` |

```json
{
  "name": "Philae Sound and Light",
  "nameAr": "الصوت والضوء في فيلة",
  "description": "Evening sound and light show.",
  "descriptionAr": "عرض الصوت والضوء المسائي.",
  "location": "Philae Temple",
  "locationAr": "معبد فيلة",
  "eventDate": "2026-11-20T19:00:00",
  "endDate": "2026-11-20T21:00:00",
  "ticketPrice": 500.00,
  "totalTickets": 100,
  "isActive": true,
  "imageIds": [61]
}
```

`POST` **201** `EventDto`. `PUT` **204**. Invalid id → 400 string `"Invalid event ID"`.

#### `DELETE /api/Event/{encodedId}`

204, or 409: `"This event is included in a package and cannot be deleted."`

---

### 3.12 Package — `api/Package`

Query object: `PackageFilterDto`.

| Query | Behavior |
| --- | --- |
| `search` | Contains-match on `name` or `nameAr` (max 150) |
| `date` | Package window overlaps that day. Open-ended when start or end is null |
| `minPrice`, `maxPrice` | Compared to `packagePrice` (the flat package price, before tax) |
| `durationDays` | Exact match, 1–365 |
| `includesHotel` | `true` = has a hotel; `false` = no hotel; omit = either |
| `isActive` | Same admin rule as trips |

Example: `GET /api/Package?search=Aswan&date=2026-11-20&minPrice=1000&maxPrice=20000&durationDays=3&includesHotel=true&isActive=true`

#### `GET /api/Package`

`trips`, `transports`, and `events` are `PackageComponentDto` (`id`, `name`, `nameAr` only). Hotel and room ids on the response are encoded.

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Aswan Three-Day Bundle",
    "nameAr": "باقة أسوان لثلاثة أيام",
    "description": "Hotel, Philae trip, sedan, and the sound and light show.",
    "descriptionAr": "فندق ورحلة فيلة وسيارة وعرض الصوت والضوء.",
    "durationDays": 3,
    "packagePrice": 9000.00,
    "maxPackagesAvailable": 10,
    "startDate": "2026-11-01T00:00:00",
    "endDate": "2026-12-31T00:00:00",
    "isActive": true,
    "images": [
      { "id": 71, "url": "http://plazainn.runasp.net/images/package/71.jpg" }
    ],
    "hotelId": "MTpQbGF6YUlubktleTIwMjU",
    "hotelName": "Plaza Nile",
    "hotelNameAr": "بلازا النيل",
    "roomId": "MTpQbGF6YUlubktleTIwMjU",
    "roomName": "Deluxe Double",
    "roomNameAr": "مزدوجة ديلوكس",
    "nightsCount": 2,
    "trips": [
      { "id": "MTpQbGF6YUlubktleTIwMjU", "name": "Philae Day Trip", "nameAr": "رحلة فيلة ليوم واحد" }
    ],
    "transports": [
      { "id": "MTpQbGF6YUlubktleTIwMjU", "name": "Aswan Private Sedan", "nameAr": "سيارة خاصة في أسوان" }
    ],
    "events": [
      { "id": "MTpQbGF6YUlubktleTIwMjU", "name": "Philae Sound and Light", "nameAr": "الصوت والضوء في فيلة" }
    ]
  }
]
```

#### `GET /api/Package/lookup`

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Aswan Three-Day Bundle",
    "nameAr": "باقة أسوان لثلاثة أيام",
    "packagePrice": 9000.00
  }
]
```

#### `GET /api/Package/{encodedId}`

400 string `"Invalid package ID"`. 404 when missing or inactive for a non-admin. 200: one `PackageDto`.

#### `POST /api/Package` and `PUT /api/Package/{encodedId}`

Body: `CreatePackageDto` / `UpdatePackageDto`. Composition ids are **encoded**. `imageIds` are raw.

| Field | Rules |
| --- | --- |
| `name`, `nameAr` | Required, max 150 |
| `description`, `descriptionAr` | Required, max 4000 |
| `durationDays` | 1–365 |
| `packagePrice` | Greater than 0. This is the flat price per package, before the 28% tax |
| `maxPackagesAvailable` | 1–100000 |
| `startDate`, `endDate` | Optional. End requires start. End must be on or after start |
| `isActive` | Default true |
| `imageIds` | At least 1 raw id from `type=package` |
| `hotelId`, `roomId`, `nightsCount` | All three or none. `nightsCount` 1–365 when set |
| `tripIds`, `transportIds`, `eventIds` | Arrays of encoded ids. Empty arrays are valid. Blank entries are skipped |
| | At least one item overall (hotel trio and/or any component id) |

```json
{
  "name": "Aswan Three-Day Bundle",
  "nameAr": "باقة أسوان لثلاثة أيام",
  "description": "Hotel, Philae trip, sedan, and the sound and light show.",
  "descriptionAr": "فندق ورحلة فيلة وسيارة وعرض الصوت والضوء.",
  "durationDays": 3,
  "packagePrice": 9000.00,
  "maxPackagesAvailable": 10,
  "startDate": "2026-11-01",
  "endDate": "2026-12-31",
  "isActive": true,
  "imageIds": [71],
  "hotelId": "MTpQbGF6YUlubktleTIwMjU",
  "roomId": "MTpQbGF6YUlubktleTIwMjU",
  "nightsCount": 2,
  "tripIds": ["MTpQbGF6YUlubktleTIwMjU"],
  "transportIds": ["MTpQbGF6YUlubktleTIwMjU"],
  "eventIds": ["MTpQbGF6YUlubktleTIwMjU"]
}
```

A package with no hotel omits `hotelId`, `roomId`, and `nightsCount` (or sends them as `null`) and must include at least one component id.

`POST` **201** `PackageDto` (components expanded). `PUT` **204**. Invalid package token → 400 string `"Invalid package ID"`. Invalid component token → 400 string `"Invalid package item ID"`, `"Invalid hotel ID"`, or `"Invalid room ID"`. Unknown hotel, room, trip, transport, or event → 404 `ProblemDetails`. Room on the wrong hotel → 400 `ProblemDetails` `"The selected room does not belong to the selected hotel."`

#### `DELETE /api/Package/{encodedId}`

204, or 409: `"This package cannot be deleted because other records depend on it."` (a booking references it).

---

### 3.13 Booking — `api/bookings`

All four actions require `[Authorize]` (any role). Create uses the `booking-create` rate limit: **5 requests per 10 minutes per user id**.

`BookingResponseDto.status` and `bookingType` in **responses** are strings (`"Pending"`, `"Hotel"`), because the mapper calls `ToString()`. Request `bookingType` is still an integer.

Customer fields on every create:

| Field | Rules |
| --- | --- |
| `customerName` | Required, 2–100 characters |
| `customerEmail` | Required, email format. Replaced by the JWT email when that claim exists |
| `customerPhone` | Required, phone format, max 32 characters |

Shared defaults: `roomsCount` 1, `adults` 1, `children` 0, `extraBed` false, `quantity` 1, `bookingType` 0.

#### `POST /api/bookings`

The five ready-to-send bodies are in section 4. Success **200**:

```json
{
  "success": true,
  "data": {
    "encodedId": "MTpQbGF6YUlubktleTIwMjU",
    "bookingType": "Hotel",
    "encodedHotelId": "MTpQbGF6YUlubktleTIwMjU",
    "encodedRoomId": "MTpQbGF6YUlubktleTIwMjU",
    "encodedTripId": null,
    "encodedTransportId": null,
    "encodedEventId": null,
    "encodedPackageId": null,
    "itemName": "Plaza Nile",
    "itemNameAr": "بلازا النيل",
    "hotelName": "Plaza Nile",
    "hotelNameAr": "بلازا النيل",
    "roomName": "Deluxe Double",
    "roomNameAr": "مزدوجة ديلوكس",
    "serviceDate": null,
    "quantity": 1,
    "adults": 2,
    "children": 0,
    "roomsBooked": 1,
    "extraBed": false,
    "totalPrice": 5120.00,
    "checkIn": "2026-11-10T00:00:00",
    "checkOut": "2026-11-12T00:00:00",
    "customerName": "Sara Hassan",
    "customerEmail": "admin@plaza.com",
    "customerPhone": "01001234567",
    "isPaid": false,
    "paymentReference": null,
    "status": "Pending",
    "createdAt": "2026-10-25T06:00:00"
  }
}
```

For a trip, `itemName` is the trip name and `bookingType` is `"Trip"`. Transport, event, and package follow the same pattern. Hotel fields are null when the package has no stay.

401 when the token has no `Id` claim:

```json
{ "success": false, "message": "User id claim is missing in token." }
```

#### `GET /api/bookings/my`

The caller’s bookings only, newest `createdAt` first.

| Query | Meaning |
| --- | --- |
| `hotelId` | **Raw integer** hotel primary key, not an encoded id |
| `from` | Keep rows whose `checkIn` **or** `serviceDate` is on or after this instant |
| `to` | Keep rows whose `checkOut` **or** `serviceDate` is on or before this instant |
| `status` | `Pending`, `Confirmed`, `Cancelled`, or `0`, `1`, `2` |
| `bookingType` | `Hotel`, `Trip`, `Transport`, `Event`, `Package`, or `0`–`4` |

Example: `GET /api/bookings/my?hotelId=1&from=2026-11-01&to=2026-11-30&status=Pending&bookingType=Hotel`

200:

```json
{ "success": true, "data": [ /* BookingResponseDto, same shape as create */ ] }
```

#### `GET /api/bookings/{encodedId}`

Owner or admin. 200 `{ "success": true, "data": { /* BookingResponseDto */ } }`. Another user’s booking → 403 `ProblemDetails` `"You do not have access to this booking."` Missing booking → 404 `"Booking not found"`. Bad token → 400 `"Invalid ID"`.

#### `POST /api/bookings/{encodedId}/cancel`

No body. Owner can cancel `Pending`. Admin can also cancel `Confirmed`. 200 returns the booking with `"status": "Cancelled"`. Already cancelled → 200 with the same cancelled booking. Confirmed + non-admin → 409 `"Paid bookings can only be cancelled by an administrator."`

---

### 3.14 Admin bookings — `GET /api/admin/bookings`

Auth: Admin. Same query parameters as `GET /api/bookings/my` (`hotelId` is the raw integer, `from`, `to`, `status`, `bookingType`), across all users.

Example: `GET /api/admin/bookings?status=Confirmed&bookingType=Package`

200:

```json
{ "success": true, "data": [ /* BookingResponseDto */ ] }
```

There is no admin create, update, or delete route. Admins cancel through `POST /api/bookings/{encodedId}/cancel`.

---

### 3.15 Payment and webhook

#### `POST /api/Payment/initiate` and `POST /api/Payment/test`

Both attributes hit the same action. Auth: any authenticated user (owner or admin). Body: `PaymentRequestDto`.

| Field | Rules |
| --- | --- |
| `encodedBookingId` | Required |
| `amountCents` | Optional and **ignored** |
| `currency` | Optional and **ignored** |
| `items` | Optional and **ignored**. Shape if sent: `{ "name", "amountCents", "quantity", "description" }` (fields, not properties; they are not used) |
| `billingData.name` | Optional. Falls back to the booking customer name |
| `billingData.email` | Optional. Falls back to the booking customer email |
| `billingData.phone` | Optional. Falls back to the booking phone or `""` |

```json
{
  "encodedBookingId": "MTpQbGF6YUlubktleTIwMjU",
  "billingData": {
    "name": "Sara Hassan",
    "email": "sara@example.com",
    "phone": "01001234567"
  }
}
```

200:

```json
{
  "paymentUrl": "https://accept.paymob.com/api/acceptance/iframes/...",
  "paymobOrderId": 123456789,
  "amountCents": 512000,
  "currency": "EGP"
}
```

401 `{ "error": "User id claim is missing in token." }`. 403 if the booking belongs to someone else. 404 `"Booking not found"`. 409 if the status is not `Pending` or the hold has expired. 502 `ProblemDetails` title `"Payment gateway error"` when Paymob cannot issue a token or a link (`PaymentGatewayException`).

#### `GET /api/Payment/check-order/{paymobOrderId}`

`paymobOrderId` is the long integer returned as `paymobOrderId`, not the encoded booking id. Auth: owner or admin.

200:

```json
{
  "id": 123456789,
  "payment_status": "PAID",
  "is_paid": true,
  "amount_cents": 512000,
  "currency": "EGP",
  "booking_confirmed": true
}
```

`booking_confirmed` is false when Paymob has not marked the order paid, when the amount does not match, or when a late payment lost the inventory. `is_paid` can still be true in the amount-mismatch case. 404 `"No booking found for this payment."` 403 for a booking the caller does not own. 502 when Paymob cannot be queried.

#### `POST /api/Webhook/callback`

Anonymous. This is the Paymob transaction webhook. Query: `hmac` (required; missing or invalid → **401** with an empty body).

The action reads a raw JSON object. It does not bind `PaymobCallbackDto`. Expected shape:

```json
{
  "type": "TRANSACTION",
  "obj": {
    "id": 998877,
    "success": true,
    "pending": false,
    "amount_cents": 512000,
    "order": { "id": 123456789 }
  }
}
```

| Outcome | Response |
| --- | --- |
| Body is not an object, or `obj` is missing | 400 empty |
| `type` is present and is not `TRANSACTION` | 200 empty, ignored |
| HMAC missing or invalid | 401 empty |
| `success` is not true, or `pending` is true | 200 empty, ignored |
| `order.id` or `amount_cents` missing | 400 empty |
| Paymob status call fails | 400 empty |
| Paid order processed (confirmed, already confirmed, or left unchanged after a mismatch) | 200 empty |

Configure Paymob’s callback URL to `https://<public-host>/api/Webhook/callback`. Paymob must be able to reach that host. `localhost` will not receive the webhook unless you tunnel it. Use `check-order` while testing locally after paying in the test iframe.

---

### 3.16 CMS

#### About Us — `api/AboutUs`

`POST` is commented out in the controller and is not a route.

##### `GET /api/AboutUs`

Anonymous. 200: array of `AboutUsDto`. Image properties are `FileDto` objects (the property names still end in `ImageId`). `id` is the raw integer primary key.

```json
[
  {
    "id": 1,
    "tittle": "About Plaza",
    "subTittle": "Hotels and journeys",
    "description": "Plaza Inn operates hotels and curated trips.",
    "square1ImageId": { "id": 81, "url": "http://plazainn.runasp.net/images/aboutUs/81.jpg" },
    "circleImageId": { "id": 82, "url": "http://plazainn.runasp.net/images/aboutUs/82.jpg" },
    "square2ImageId": { "id": 83, "url": "http://plazainn.runasp.net/images/aboutUs/83.jpg" }
  }
]
```

The spelling `tittle` / `subTittle` is the DTO property name.

##### `PUT /api/AboutUs`

Auth: Admin. Body: `UpdateAboutUsDto`. Updates the first row, or inserts one if the table is empty. Image ids are raw integers from `type=aboutUs`.

```json
{
  "tittle": "About Plaza",
  "subTittle": "Hotels and journeys",
  "description": "Plaza Inn operates hotels and curated trips.",
  "square1ImageId": 81,
  "circleImageId": 82,
  "square2ImageId": 83
}
```

204 empty body.

#### Contact Us — `api/ContactUs`

##### `GET /api/ContactUs`

Anonymous. Returns **one object**, not an array. If no row exists, the body is an empty `ContactUsDto` (`workingHours: []`). `day` is `DayOfWeek` as an integer. `openTime` / `closeTime` are `TimeOnly` strings `HH:mm:ss`.

```json
{
  "id": 1,
  "email": "hello@plaza.com",
  "address": "12 Corniche El Nil, Cairo",
  "phone": "0225550100",
  "instagram": "https://instagram.com/plazainn",
  "facebook": "https://facebook.com/plazainn",
  "whatsApp": "201001112233",
  "linkedIn": "https://linkedin.com/company/plazainn",
  "workingHours": [
    { "id": 1, "day": 0, "openTime": "09:00:00", "closeTime": "18:00:00" },
    { "id": 1, "day": 1, "openTime": "09:00:00", "closeTime": "22:00:00" }
  ]
}
```

##### `PUT /api/ContactUs`

Auth: Admin. Body: `UpdateContactUsDto`. Creates the row if none exists. For each entry in `workingHours`, a row with the same `day` is updated, otherwise a row is added. Days omitted from the array are left as they are. Send `workingHours` as an array (it may be empty). A null `workingHours` throws and becomes **500**.

```json
{
  "email": "hello@plaza.com",
  "address": "12 Corniche El Nil, Cairo",
  "phone": "0225550100",
  "instagram": "https://instagram.com/plazainn",
  "facebook": "https://facebook.com/plazainn",
  "whatsApp": "201001112233",
  "linkedIn": "https://linkedin.com/company/plazainn",
  "workingHours": [
    { "day": 0, "openTime": "09:00:00", "closeTime": "18:00:00" },
    { "day": 1, "openTime": "09:00:00", "closeTime": "22:00:00" },
    { "day": 2, "openTime": "09:00:00", "closeTime": "22:00:00" },
    { "day": 3, "openTime": "09:00:00", "closeTime": "22:00:00" },
    { "day": 4, "openTime": "09:00:00", "closeTime": "22:00:00" },
    { "day": 5, "openTime": "10:00:00", "closeTime": "23:00:00" },
    { "day": 6, "openTime": "10:00:00", "closeTime": "23:00:00" }
  ]
}
```

204.

#### Home slider — `api/HomeSlider`

`id` is a raw integer. Reads are anonymous. Create, update, and delete require Admin.

##### `GET /api/HomeSlider`

```json
[
  {
    "id": 1,
    "tittle": "Winter on the Nile",
    "subTittle": "Hotels, trips, and packages",
    "buttonText": "Explore",
    "buttonLink": "/packages",
    "images": [
      { "id": 91, "url": "http://plazainn.runasp.net/images/homeSlider/91.jpg" }
    ]
  }
]
```

##### `POST /api/HomeSlider`

Body: `CreateHomeSliderDto`. `imagesId` is the raw id list from `type=homeSlider`. No validation attributes.

```json
{
  "tittle": "Winter on the Nile",
  "subTittle": "Hotels, trips, and packages",
  "buttonText": "Explore",
  "buttonLink": "/packages",
  "imagesId": [91]
}
```

200: the JSON string `"HomeSliders have been added Succesfully"`.

##### `GET /api/HomeSlider/{id}`

`id` is an integer. 200 `HomeSliderDto`. A missing id still returns **200** with a null body (the service maps a null entity).

##### `PUT /api/HomeSlider/{id}`

Body: `UpdateHomeSliderDto` (same properties). 204 when the row exists. A missing id returns **204** as well and changes nothing.

```json
{
  "tittle": "Winter on the Nile",
  "subTittle": "Updated subtitle",
  "buttonText": "Book",
  "buttonLink": "/hotels",
  "imagesId": [91, 92]
}
```

##### `DELETE /api/HomeSlider/{id}`

204 whether or not the row existed.

#### Section images — `api/section-images`

##### `GET /api/section-images?type={PlazaInnType}`

Anonymous. Returns every `Image` row whose `Type` equals the query value, as `FileDto` with the **stored relative** `url`.

`GET /api/section-images?type=aboutUs`

```json
[
  { "id": 81, "url": "images/aboutUs/81.jpg" }
]
```

If `type` is omitted, the non-nullable enum binds to `hotel` (0).

##### `PUT /api/section-images?type={PlazaInnType}`

Auth: Admin. Body: `UpdateSectionImagesDto`. Every image currently of that type is set to `None`. Every id in `imagesId` is then assigned this type. This retags existing `Image` rows; it does not upload files.

`PUT /api/section-images?type=homeSlider`

```json
{ "imagesId": [91, 92] }
```

204.

---

### 3.17 Admin dashboard — `GET /api/AdminDashboard/summary`

Auth: Admin (`[Authorize(Roles = "Admin")]`). One read-only payload for the Angular admin home. The body is `AdminDashboardSummaryDTO` itself. It is not wrapped in `{ "success": true, "data": ... }`.

```http
GET /api/AdminDashboard/summary
GET /api/AdminDashboard/summary?fromDate=2026-01-01&toDate=2026-10-28
Authorization: Bearer <admin token>
```

| Query | Meaning |
| --- | --- |
| `fromDate` | Optional. Inclusive start. A date-only value such as `2026-01-01` is midnight UTC on that calendar day. |
| `toDate` | Optional. Inclusive end. A date-only value includes the whole calendar day (`toDate=2026-10-28` keeps bookings through `2026-10-28T23:59:59.9999999Z`). A value that includes a time is an inclusive instant. |

Send `yyyy-MM-dd` or a UTC ISO-8601 instant. A value with no offset is treated as UTC and is not shifted by the server timezone. `fromDate` later than `toDate` returns **400** `ProblemDetails` with `detail` `"fromDate must be on or before toDate."` Missing or invalid JWT: **401** empty body. A valid non-admin token: **403** empty body.

The activity date of a booking is `serviceDate` when that column is set (trips, transports, events, and dated packages). Hotel bookings have no `serviceDate`, so the filter uses `createdAt`. `totalUsers` and `catalogCounts` are a current snapshot and ignore the date filter. `lowCapacityAlerts` is current inventory and also ignores the date filter.

When both dates are omitted, KPIs, revenue, top performers, and recent bookings are all-time, and `monthlyTrends` is the current UTC month plus the previous 11 months (zero-filled). When only `toDate` is set, KPIs include every booking up to that date and the chart is the 12 months ending at `toDate`. When only `fromDate` is set, the chart runs from that month through the current month, or the next 12 months when `fromDate` is in the future. A range longer than 36 months keeps the latest 36 months on the chart; the KPI totals still use the full range.

Money is rounded half away from zero to 2 decimal places. There is no `Completed` status and no `TaxAmount` column. Confirmed revenue is the sum of `totalPrice` for `Confirmed` bookings. Tax is derived from the tax-inclusive total and `Booking:TaxRate` (default `0.28`): `totalPrice * TaxRate / (1 + TaxRate)`. `revenueSharePercentage` is that type’s confirmed revenue divided by `kpis.totalConfirmedRevenue`. Shares are rounded to 2 decimals and the largest slice is adjusted so the five shares sum to `100.00` when revenue is positive.

`pendingBookings` counts `Pending` rows still inside the payment hold (`createdAt` within `Booking:PendingExpiryMinutes`, default 30). `expiredBookings` counts `Pending` rows older than that hold. The cleanup job then sets those rows to `Cancelled`, so `expiredBookings` is the queue still waiting to be swept, not a history of every hold that ever expired. `pendingRevenue` sums `totalPrice` for every `Pending` row, including holds the job has not cancelled yet. `cancelledBookings` is every `Cancelled` row.

`catalogCounts.*.active` uses `isActive` for trips, transports, events, and packages. Hotels and rooms have no `isActive` flag, so `active` equals `total`.

`revenueByServiceType` always contains these five objects, in this order: Hotel `0`, Trip `1`, Transport `2`, Event `3`, Package `4`. `bookingType` is the integer. `bookingTypeName` is the name.

`topPerformers` lists at most five hotels, trips, transports, events, and packages, ordered by confirmed revenue, then by booking count. `bookingsCount` includes every status in the window. `totalRevenue` is confirmed only. `encodedId` is `IdEncoder` (the same token used by the catalog APIs). `topDestinations` is the top five locations by booking count: hotel city, trip city, event location, transport pickup, and the linked hotel city of a package. Locations are trimmed and merged case-insensitively.

`operationalInsights.recentBookings` is the latest 10 bookings in the selected window (`createdAt` descending). `bookingReference` is `paymentReference`, otherwise `merchantOrderId`, otherwise the same `encodedId`. `status` is `Pending`, `Confirmed`, `Cancelled`, or `Expired`. `Expired` is only a display value for a pending hold past the payment window. Do not send it to `GET /api/admin/bookings?status=`. `paymentStatus` is `Paid` or `Unpaid` from `isPaid`. Hotel `itemName` is `"{hotel} - {room}"` when both names exist.

`lowCapacityAlerts` warns when remaining inventory is `<= 5`. Events use `totalTickets` minus ticket `quantity`. Packages use `maxPackagesAvailable` minus package `quantity`. Trips use `maxCapacity` minus `adults + children` for each service date from today onward. Confirmed bookings and pending bookings still inside the hold consume inventory. Cancelled bookings and expired holds do not. Active events, packages, and trips whose cap is already `<= 5` and which have no consuming booking are included with `booked: 0`. Past items (`endDate` before today) are omitted. `serviceDate` is the event date, the package start, or the trip day. It is null for a trip whose daily cap is low but which has no upcoming booked day. `itemType` is `Event`, `Package`, or `Trip`. The list is capped at 20 rows, lowest remaining first. Use `encodedId` with the matching catalog GET.

`monthlyTrends[].periodLabel` is `yyyy-MM`. Months with no bookings are included with zeros.

200 (chart shortened to two months; an unfiltered call returns 12):

```json
{
  "kpis": {
    "totalConfirmedRevenue": 30336.00,
    "totalTaxCollected": 6636.00,
    "pendingRevenue": 5120.00,
    "totalBookings": 18,
    "confirmedBookings": 12,
    "pendingBookings": 3,
    "cancelledBookings": 2,
    "expiredBookings": 1,
    "totalUsers": 40,
    "catalogCounts": {
      "hotels": { "active": 4, "total": 4 },
      "rooms": { "active": 12, "total": 12 },
      "trips": { "active": 6, "total": 8 },
      "transports": { "active": 5, "total": 7 },
      "events": { "active": 3, "total": 4 },
      "packages": { "active": 2, "total": 3 }
    }
  },
  "revenueByServiceType": [
    { "bookingType": 0, "bookingTypeName": "Hotel", "bookingsCount": 6, "confirmedCount": 4, "revenue": 11520.00, "taxAmount": 2520.00, "revenueSharePercentage": 37.96 },
    { "bookingType": 1, "bookingTypeName": "Trip", "bookingsCount": 4, "confirmedCount": 3, "revenue": 4992.00, "taxAmount": 1092.00, "revenueSharePercentage": 16.46 },
    { "bookingType": 2, "bookingTypeName": "Transport", "bookingsCount": 3, "confirmedCount": 2, "revenue": 1024.00, "taxAmount": 224.00, "revenueSharePercentage": 3.38 },
    { "bookingType": 3, "bookingTypeName": "Event", "bookingsCount": 3, "confirmedCount": 2, "revenue": 1280.00, "taxAmount": 280.00, "revenueSharePercentage": 4.22 },
    { "bookingType": 4, "bookingTypeName": "Package", "bookingsCount": 2, "confirmedCount": 1, "revenue": 11520.00, "taxAmount": 2520.00, "revenueSharePercentage": 37.98 }
  ],
  "monthlyTrends": [
    { "year": 2026, "month": 9, "periodLabel": "2026-09", "bookingsCount": 7, "confirmedRevenue": 12800.00 },
    { "year": 2026, "month": 10, "periodLabel": "2026-10", "bookingsCount": 11, "confirmedRevenue": 17536.00 }
  ],
  "topPerformers": {
    "hotels": [
      { "encodedId": "MTpQbGF6YUlubktleTIwMjU", "name": "Nile Plaza", "bookingsCount": 6, "totalRevenue": 11520.00 }
    ],
    "trips": [
      { "encodedId": "MTpQbGF6YUlubktleTIwMjU", "name": "Luxor Day Trip", "bookingsCount": 4, "totalRevenue": 4992.00 }
    ],
    "transports": [
      { "encodedId": "MTpQbGF6YUlubktleTIwMjU", "name": "Airport Sedan", "bookingsCount": 3, "totalRevenue": 1024.00 }
    ],
    "events": [
      { "encodedId": "MTpQbGF6YUlubktleTIwMjU", "name": "Sound and Light", "bookingsCount": 3, "totalRevenue": 1280.00 }
    ],
    "packages": [
      { "encodedId": "MTpQbGF6YUlubktleTIwMjU", "name": "Cairo Long Weekend", "bookingsCount": 2, "totalRevenue": 11520.00 }
    ],
    "topDestinations": [
      { "location": "Cairo", "bookingsCount": 8, "totalRevenue": 23040.00 }
    ]
  },
  "operationalInsights": {
    "recentBookings": [
      {
        "encodedId": "MTpQbGF6YUlubktleTIwMjU",
        "bookingReference": "PLZ-1-0f1e2d3c4b5a69788796a5b4c3d2e1f0",
        "userName": "Sara Hassan",
        "email": "sara@example.com",
        "bookingType": 0,
        "bookingTypeName": "Hotel",
        "itemName": "Nile Plaza - Deluxe Double",
        "totalPrice": 5120.00,
        "status": "Confirmed",
        "paymentStatus": "Paid",
        "createdAt": "2026-10-28T18:04:11Z"
      }
    ],
    "lowCapacityAlerts": [
      {
        "encodedId": "MTpQbGF6YUlubktleTIwMjU",
        "itemType": "Event",
        "name": "Sound and Light",
        "capacity": 40,
        "booked": 36,
        "remaining": 4,
        "serviceDate": "2026-11-20T00:00:00Z"
      }
    ]
  }
}
```

Each top-performer list contains up to five objects. The sample shows one. Call the catalog GET with `encodedId` (`GET /api/Hotel/{encodedId}`, and the Trip, Transport, Event, and Package equivalents).

---

## 4. 5 Ready-to-Test Booking Payloads / أمثلة عملية جاهزة للحجز للـ 5 أنواع

All five calls are:

`POST /api/bookings`  
`Authorization: Bearer <token>`  
Content-Type: `application/json`

Replace encoded ids with values from the API. `bookingType` must be an integer (or omitted for hotel). Dates below are after the guide date (25 October 2026) and inside the sample catalog windows from section 3.

The logged-in user’s email overwrites `customerEmail` when the token contains an `Email` claim. `totalPrice` in the response includes the 28% tax. These requests do not.

### 4.1 Hotel room booking

`bookingType` `0` is `Hotel`. Omit the property for the same result. Check-out is two nights after check-in. With the sample room (2000 / night, no extra bed) the server total is **5120.00**.

```json
{
  "bookingType": 0,
  "encodedHotelId": "MTpQbGF6YUlubktleTIwMjU",
  "encodedRoomId": "MTpQbGF6YUlubktleTIwMjU",
  "roomsCount": 1,
  "adults": 2,
  "children": 0,
  "checkIn": "2026-11-10",
  "checkOut": "2026-11-12",
  "extraBed": false,
  "customerName": "Sara Hassan",
  "customerEmail": "sara@example.com",
  "customerPhone": "01001234567"
}
```

The hotel must have `numOfAvailableRoomsToReserve` (or suites, when `type` is `2`) greater than overlapping active bookings. `roomsCount` is stored as both `quantity` and `roomsBooked`.

### 4.2 Standalone trip booking

`bookingType` `1`. `serviceDate` is required. Guest count is `adults + children`, and that count must fit in `maxCapacity` for that date. Request `quantity` is not used. Sample prices 1500 and 900 with 2 adults and 1 child: net 3900, with tax **4992.00**.

```json
{
  "bookingType": 1,
  "encodedTripId": "MTpQbGF6YUlubktleTIwMjU",
  "serviceDate": "2026-11-15",
  "adults": 2,
  "children": 1,
  "customerName": "Sara Hassan",
  "customerEmail": "sara@example.com",
  "customerPhone": "01001234567"
}
```

### 4.3 Standalone transport booking

`bookingType` `2`. `quantity` is the number of vehicles. Passengers (`adults + children`) must be less than or equal to `quantity * maxPassengers`. One sedan at 800: with tax **1024.00**.

```json
{
  "bookingType": 2,
  "encodedTransportId": "MTpQbGF6YUlubktleTIwMjU",
  "serviceDate": "2026-11-15",
  "quantity": 1,
  "adults": 2,
  "children": 1,
  "customerName": "Sara Hassan",
  "customerEmail": "sara@example.com",
  "customerPhone": "01001234567"
}
```

### 4.4 Standalone event booking

`bookingType` `3`. `quantity` is tickets. `serviceDate` is optional; `2026-11-20` falls on the sample event. Two tickets at 500: with tax **1280.00**.

```json
{
  "bookingType": 3,
  "encodedEventId": "MTpQbGF6YUlubktleTIwMjU",
  "serviceDate": "2026-11-20",
  "quantity": 2,
  "adults": 2,
  "children": 0,
  "customerName": "Sara Hassan",
  "customerEmail": "sara@example.com",
  "customerPhone": "01001234567"
}
```

### 4.5 Bundled package booking

`bookingType` `4`. `quantity` is how many packages. The sample package has a room and a date window, so `serviceDate` is required. Check-in becomes that date and check-out becomes that date plus `nightsCount` (2). One package at 9000: with tax **11520.00**. The room pool must have a free room for those nights, and `maxPackagesAvailable` must allow one more package. Component trip, transport, and event capacities are not reduced by this call.

```json
{
  "bookingType": 4,
  "encodedPackageId": "MTpQbGF6YUlubktleTIwMjU",
  "serviceDate": "2026-11-18",
  "quantity": 1,
  "adults": 2,
  "children": 0,
  "customerName": "Sara Hassan",
  "customerEmail": "sara@example.com",
  "customerPhone": "01001234567"
}
```

---

## 5. Angular Frontend Integration Checklist / دليل الربط مع الفرونت إند

### Headers

Send JSON as `Content-Type: application/json` except for `POST /api/upload` and `POST /api/Partner/broadcastEmail/hotel/{encodedId}`, which are `multipart/form-data`. Do not set `Content-Type` yourself on those calls; the browser must add the boundary.

Authenticated calls:

```http
Authorization: Bearer <token>
```

Store the login `token` and attach it with an HTTP interceptor. Treat HTTP 401 on a protected call as an expired or missing token (lifetime is 1 hour, clock skew 0) and send the user to login. Do not send the token to Paymob. Open `paymentUrl` as a normal browser navigation.

### CORS

Policy name `PlazaCors`. Credentials (cookies) are not enabled. Methods and headers are unrestricted. Origins are the `AllowedOrigins` array:

- `http://localhost:4200`
- `https://localhost:4200`
- `http://plazainn.runasp.net`

An Angular dev server on another origin is rejected by the browser before the action runs. Add that origin to `AllowedOrigins` and restart the API.

`FrontendBaseUrl` (`http://localhost:4200`) is used only to build the password-reset link. It is not the CORS list.

### Rate limiting

| Policy | Endpoints | Limit | Partition |
| --- | --- | --- | --- |
| `auth` | `login`, `register`, `forgot-password`, `reset-password` | 10 requests / 1 minute | Client IP |
| `booking-create` | `POST /api/bookings` | 5 requests / 10 minutes | JWT `Id`, or client IP if the user id is missing |

Queue limit is 0. The rejected response is **429** `application/problem+json`, and when the limiter provides a window the response includes `Retry-After` (seconds, integer, rounded up).

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Too many requests",
  "status": 429,
  "detail": "Too many requests. Please try again later.",
  "instance": "/api/bookings",
  "traceId": "00-...",
  "message": "Too many requests. Please try again later."
}
```

Read `Retry-After` before retrying login or booking creation. Other endpoints are not rate-limited by this API.

### Standardized errors

Two JSON error families exist. Branch on `Content-Type` and on whether `errors` is present.

**A. Validation problem (HTTP 400)** from `[ApiController]` model validation, or `return BadRequest(ModelState)`. Content type `application/problem+json`.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "CustomerName": ["The CustomerName field is required."],
    "EncodedTripId": ["Trip is required."]
  },
  "traceId": "00-..."
}
```

Keys inside `errors` are the C# property names (PascalCase).

**B. Exception middleware `ProblemDetails`.** Content type `application/problem+json`. Extra fields `traceId` and `message` (same text as `detail`).

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Conflict",
  "status": 409,
  "detail": "Only 2 room(s) of this type are available for the selected dates.",
  "instance": "/api/bookings",
  "traceId": "00-...",
  "message": "Only 2 room(s) of this type are available for the selected dates."
}
```

| Status | When the middleware or the action returns it |
| --- | --- |
| **400** | `ArgumentException` (`title`: `"Invalid request"`). Examples: invalid encoded id on booking routes (`"Invalid ID"`), room not in the hotel, stay longer than 30 nights, check-in in the past, extra bed not offered, too many guests, service date outside the catalog window, package trio incomplete. Also `BadRequest("...")` from controllers, which is a **JSON string**, not this object. Also model validation (shape A). |
| **401** | Login failure: `{ "message": "..." }` (not `ProblemDetails`). Missing `Id` claim on bookings: `{ "success": false, "message": "User id claim is missing in token." }`. Missing `Id` claim on payment: `{ "error": "User id claim is missing in token." }`. Missing or invalid JWT on `[Authorize]` routes: empty body plus `WWW-Authenticate: Bearer`. Webhook HMAC failure: empty 401. |
| **403** | `ForbiddenException` (`title`: `"Forbidden"`, `detail`: `"You do not have access to this booking."`) when a user reads, cancels, pays, or polls a booking they do not own. A non-admin calling an Admin route with a valid token: empty 403 from the authorization middleware. |
| **404** | `NotFoundException` (`title`: `"Resource not found"`). Messages include `"Room not found"`, `"Hotel not found"`, `"Trip not found"`, `"Transport not found"`, `"Event not found"`, `"Package not found"`, `"Booking not found"`, `"User not found."`, `"No booking found for this payment."`, `"One or more trips were not found."` (and the transport/event equivalents). Some controllers return `NotFound()` with an **empty** body instead (hotel, room, partner, insight, catalog get-by-id). Inactive catalog items for non-admins are also an empty 404. |
| **409** | `ConflictException` (`title`: `"Conflict"`). Examples: no rooms left, trip/transport/event/package sold out or inactive, only pending bookings can be paid, hold expired, paid booking cancelled by a non-admin, catalog delete blocked by a foreign key. |
| **429** | Rate-limit rejection. See the body above. Honor `Retry-After`. |
| **502** | `PaymentGatewayException` (`title`: `"Payment gateway error"`). Paymob token, payment-link, or order-status call failed. `detail` is a fixed sentence such as `"Failed to retrieve Paymob token."` The webhook maps the same failure to an **empty 400**, not 502. |
| **500** | Any other exception. `title`: `"Internal server error"`. `detail` and `message`: `"An unexpected error occurred. Please try again later."` The real exception is logged on the server and is not returned. |
| **207** | Partner broadcast only, when some emails fail. Body is `{ message, sent, failed }`, not `ProblemDetails`. |

Success wrappers to handle explicitly:

- Bookings and availability: `{ "success": true, "data": ... }`
- Admin dashboard `GET /api/AdminDashboard/summary`: the `AdminDashboardSummaryDTO` object itself (no `success` wrapper)
- Payment initiate: `{ paymentUrl, paymobOrderId, amountCents, currency }`
- Payment check-order: snake_case fields `payment_status`, `is_paid`, `amount_cents`, `booking_confirmed`
- Many admin writes: **204** with an empty body
- User create and home-slider create: **200** with a JSON string

Image `url` values on catalog DTOs are absolute and can be used directly in `<img>`. `GET /api/section-images` returns a relative path; prefix it with the API origin if you display those URLs.

---

## 6. Step-by-Step Manual Testing Script on Swagger / سيناريو اختبار عملي خطوة بخطوة

Use `http://localhost:5141/swagger` after section 1. Keep a notepad for the ids each step returns. Dates assume you are testing on or after 25 October 2026 and before the sample end dates.

1. **Log in as admin.** `POST /api/Account/login` with `email` `admin@plaza.com` and `password` `Admin@123`. Copy `token`. Click **Authorize**, paste the token only, and authorize. The lock icons on admin operations should show as authorized.

2. **Upload one image per catalog type.** Call `POST /api/upload` four times (Swagger file picker, form field `files`):
   - `type=trip` → save the returned integer as `tripImageId`
   - `type=transport` → `transportImageId`
   - `type=event` → `eventImageId`
   - `type=package` → `packageImageId`  
   Each response is a JSON array such as `[41]`. Use a real `.jpg`, `.jpeg`, `.png`, or `.webp` under 10 MB.

3. **Create a trip.** `POST /api/Trip` with the section 3.9 body and `"imageIds": [<tripImageId>]`. From the 201 body copy `id` as `encodedTripId`. Confirm `GET /api/Trip/{encodedId}` returns the same trip.

4. **Create a transport.** `POST /api/Transport` with the section 3.10 body and `"imageIds": [<transportImageId>]`. Copy `id` as `encodedTransportId`.

5. **Create an event.** `POST /api/Event` with the section 3.11 body (`eventDate` still in the future) and `"imageIds": [<eventImageId>]`. Copy `id` as `encodedEventId`.

6. **Bundle them into a package.** If you also want the hotel stay, create a hotel and a room first (`POST /api/upload?type=hotel`, `POST /api/upload?type=logo`, `POST /api/Hotel`, `POST /api/upload?type=room`, `POST /api/Room`) and pass those encoded ids as `hotelId`, `roomId`, and `nightsCount`. Otherwise omit the three stay fields. `POST /api/Package` with the section 3.12 body, your image id, and:

```json
"tripIds": ["<encodedTripId>"],
"transportIds": ["<encodedTransportId>"],
"eventIds": ["<encodedEventId>"]
```

Copy the 201 `id` as `encodedPackageId`. `GET /api/Package/{encodedId}` should list the three components.

7. **Book the package as the admin (the admin token is enough; booking is `[Authorize]`, not admin-only).** `POST /api/bookings` with the section 4.5 body and your `encodedPackageId`. Expect 200, `"bookingType": "Package"`, `"status": "Pending"`, `"isPaid": false`, and `totalPrice` equal to `packagePrice * 1.28` for quantity 1. Copy `data.encodedId` as `encodedBookingId`. A 409 here means the package is inactive, sold out, the service date is outside `startDate`/`endDate`, or (when a room is attached) the room pool is full.

8. **Start Paymob.** `POST /api/Payment/initiate` with `{ "encodedBookingId": "<encodedBookingId>" }`. Expect 200 with `paymentUrl`, `paymobOrderId`, `amountCents`, and `"currency": "EGP"`. Open `paymentUrl` in a browser and pay with Paymob’s test card. `amountCents` must match `round(totalPrice * 100)` from step 7. The API ignores any amount you might add to the body.

9. **Confirm the payment.** If Paymob can reach your host, its callback is `POST /api/Webhook/callback?hmac=...` and the booking moves to `Confirmed` without a Swagger call. On localhost, call `GET /api/Payment/check-order/{paymobOrderId}` with the number from step 8. A paid test order returns `"is_paid": true` and `"booking_confirmed": true`. If you skip payment, wait 30 minutes: `ExpiredBookingsCleanupService` sets the booking to `Cancelled` (the job wakes about every 5 minutes), and a later initiate returns 409.

10. **Verify both booking lists.** `GET /api/bookings/my?bookingType=Package` returns the booking for the logged-in admin, including `status` `Confirmed` after step 9 or `Pending` before it. `GET /api/admin/bookings?bookingType=Package` returns the same row because the caller is an admin. `GET /api/bookings/{encodedBookingId}` returns that single booking. Optional: `POST /api/bookings/{encodedBookingId}/cancel` is allowed for this admin even after confirmation; the next `GET` then shows `"status": "Cancelled"`. Cancelling does not refund Paymob.
