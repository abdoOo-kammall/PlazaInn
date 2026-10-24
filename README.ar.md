# دليل مشروع Plaza Inn API — من الألف للياء

الملف ده مكتوب عشان تفتكر المشروع لو رجعت له بعد وقت طويل.  
ده **باك إند فقط** (ASP.NET Core 8). الفرونت مش موجود في الريبو ده، ورسائل إعادة كلمة السر بتبعت لينك على `http://localhost:4200` (غالباً Angular).

- Swagger محلي: `http://localhost:5141/swagger` أو `https://localhost:7142/swagger`
- النسخة الإنجليزية المختصرة: [README.md](./README.md)

---

## 1) المشروع ده بيتعمل إيه؟

منصة مجموعة فنادق **Plaza Inn**:

| مين | بيعمل إيه |
| --- | --- |
| زائر الموقع | يشوف الفنادق، الغرف، السلايدر، About Us، Contact Us، المطعم/الكافيه والمنيو، ويحجز ويدفع |
| عميل مسجّل (Client) | يسجّل / يعمل لوجين، يشوف حجوزاته، يفتح لينك دفع Paymob |
| أدمن (Admin) | يدير الفنادق والغرف والمحتوى واليوزرز والشركاء وكل الحجوزات |
| Paymob | بعد الدفع ينادي Webhook عشان الحجز يتأكد |

اللغة في الداتا **إنجليزي + عربي** (`Name` / `NameAr` وهكذا).

---

## 2) شكل الحل (Solution)

افتح `Plaza_SOL.sln`. فيه 5 مشاريع:

```
Plaza_SOL.sln
├── Plaza              → الـ API نفسه (Controllers, JWT, CORS, Swagger, wwwroot)
├── PlazaCore          → الكيانات + العقود (Interfaces) + Specification
├── PlazaRepository    → EF Core + SQL Server + Identity + Migrations + Seed
├── PlazaService       → البيزنس لوجيك
└── Shared             → DTOs + Enums + تشفير الـ Id + مساعدات الإيميل
```

**رحلة أي ريكوست:**

1. يجي HTTP على Controller في `Plaza`
2. الـ Controller ينادي Service في `PlazaService`
3. الـ Service يستخدم `IGenericRepository<T>` أو Identity أو `HttpClient` (Paymob)
4. الـ Repository يقرأ/يكتب SQL Server (غالباً بـ Specification)
5. AutoMapper يحوّل Entity ↔ DTO
6. الـ Id اللي بيرجع للفرونت **مش رقم خام**، متشفّر بـ `IdEncoder`

---

## 3) أفكار لازم تفتكرها قبل ما تلمس الكود

### الـ Id المتشفّر (`EncodedId`)

في الداتابيز الـ Id رقم (`1`, `2`, `3`).  
برا الـ API بنبعت سترينج Base64 فيه الرقم + مفتاح `PlazaInnKey2025`.

- الكلاس: `Shared/Security/IdEncoder.cs`
- الفرونت دايماً يبعت `encodedId` في المسار أو في البودي
- **استثناء:** يوزرز Identity الـ Id بتاعهم GUID سترينج زي ما هو

ده إخفاء للأرقام المتسلسلة، مش حماية أمنية حقيقية.

### الصور والملفات

1. ارفع الملف: `POST /api/upload?type=hotel` (أو `room` / `logo` / `homeSlider` / `aboutUs` / `restaurant` / `cafe` / `restaurantMenu` / `cafeMenu`)
2. الرد: ليستة أرقام Ids
3. لما تعمل Create/Update للفندق أو الأوضة، تبعت الـ Ids دي
4. في الداتابيز بتتخزن كـ **سترينج** (مثلاً `"1,5,9"`)
5. لما تقرأ، AutoMapper يحوّلها لـ `FileDto` فيها الـ URL

الملفات بتتسجل في:

- صور: `Plaza/wwwroot/images/{type}/`
- منيو PDF: `Plaza/wwwroot/menus/{type}/`

لو نفس الملف اترفع قبل كده (نفس الـ SHA256 + نفس الـ type) بيرجع الـ Id القديم ومش بيكرّر الحفظ.

### الأدوار

Enum في `Shared/Enums/Role.cs`: `Admin = 1`, `Client = 2`, `User = 3`

- التسجيل العام `POST /api/Account/register` → دور **Client**
- التوكن فيه كليمز: `Id`, `UserName`, `Email`, `role`
- صلاحية التوكن: **ساعة واحدة**
- معظم الـ `[Authorize(Roles = "Admin")]` على Controllers الفنادق/الغرف/اليوزرز **متعلّق بكومنت** حالياً. الوحيد المتقفّل فعلاً: `GET /api/admin/bookings`

### سعر الحجز

```
عدد الليالي × سعر الليلة × 1.28
```

الـ 28% زيادة موجودة في `RoomAvailabilityService.CalculateTotalPriceAsync`.

### حالة الحجز

`Pending` → بعد دفع ناجح → `Confirmed`  
`Cancelled` موجود في الـ enum لكن مفيش API للإلغاء.

---

## 4) الكيانات (الدومين)

| كيان | الملف | المعنى |
| --- | --- | --- |
| `Hotel` | `PlazaCore/Entites/Hotel.cs` | فندق: اسم/وصف/عنوان باللغتين، تليفون، إيميل، سوشيال، لوجو، صور، عدد غرف/سويتس، تقييم |
| `Room` | `Room.cs` | غرفة تابعة لفندق: نوع Single/Double/Suite، سعر الليلة، سرير إضافي، MaxSleep، Features، صور |
| `Booking` | `Booking.cs` | حجز: فندق + غرفة + اسم/إيميل الضيف + تواريخ + سعر + هل مدفوع + مرجع Paymob + Status |
| `InsightHotel` | `InsightHotel.cs` | صفحة المطعم/الكافيه للفندق + صور + PDF المنيو + Facilities |
| `Partners` | `Partners.cs` | شريك (اسم + إيميل) مرتبط بعدة فنادق |
| `Image` | `Image.cs` | ملف مرفوع: Url + Type + Hash |
| `HomeSlider` | `HomeSlider.cs` | سلايد الصفحة الرئيسية |
| `AboutUs` | `AboutUs.cs` | عنوان/وصف + 3 صور (مربع/دايرة/مربع) |
| `ContactUs` | `ContactUs.cs` | إيميل، عنوان، تليفون، سوشيال |
| `WorkingHour` | `WorkingHour.cs` | يوم + وقت فتح/قفل مرتبط بـ ContactUs |
| `ApplicationUser` | `ApplicationUser.cs` | يوزر Identity (فاضي، بيرث من IdentityUser) |

`PlazaDbContext` في `PlazaRepository/PlazaDbContext.cs`.

---

## 5) تشغيل المشروع محلياً

المطلوب: .NET 8 SDK + SQL Server.

1. غيّر `ConnectionStrings:DefaultConnection` في `Plaza/appsettings.json` على الداتابيز بتاعتك.
2. من فولدر `Plaza`:

```bash
dotnet restore
dotnet ef database update --project ../PlazaRepository --startup-project .
dotnet run --launch-profile https
```

3. افتح Swagger وجرّب.

CORS مفتوح لكل الدومينات (`AllowAll`) عشان الفرونت المحلي يشتغل.

### الإعدادات المهمة (من غير أسرار)

| المفتاح | وظيفته |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server |
| `Jwt:Key` / `Issuer` / `Audience` | توقيع التوكن (`PlazaAPI` / `PlazaClient`) |
| `Email:*` | SMTP (Gmail) لإعادة الباسورد وإيميلات الشركاء |
| `PaymobTest:*` | مفاتيح Paymob التجريبي (في `appsettings.Development.json`) |
| `Paymob:*` | لايف — حالياً فاضي |

**تحذير:** فيه أسرار مكتوبة جوه ملفات الإعدادات. لو الريبو اتعرض لأي حد، غيّر الباسوردات والمفاتيح.

### الـ Seed عند الـ Startup

- الأدوار: Admin, Client, User (`DefaultRole.cs`)
- أدمن افتراضي (`DefaultUser.cs`): الـ Email حالياً **مسافات فاضية**، فغالباً الأدمن الافتراضي **مش بيتخلق**. لو محتاج أدمن، اعمله من `POST /api/User` أو صلّح الـ seed.

سياسة الباسورد مفكوكة: طول 6، من غير شرط أرقام أو رموز.

---

## 6) دورات الاستخدام (إزاي النظام بيشتغل)

### أ) الموقع العام (من غير لوجين)

```
السلايدر → ليستة الفنادق → تفاصيل فندق + غرفه
         → About Us + صور الأقسام
         → Contact Us + مواعيد العمل
         → Insight (مطعم / كافيه / منيو)
```

| الترتيب | الطلب |
| --- | --- |
| 1 | `GET /api/HomeSlider` |
| 2 | `GET /api/Hotel` أو `GET /api/Hotel/lookup` |
| 3 | `GET /api/Hotel/{encodedId}` |
| 4 | `GET /api/Room/hotel/{encodedHotelId}` |
| 5 | `GET /api/InsightHotel/{encodedHotelId}` |
| 6 | `GET /api/AboutUs` و `GET /api/section-images?type=aboutUs` |
| 7 | `GET /api/ContactUs` |

### ب) حساب العميل

**تسجيل**

`POST /api/Account/register`

```json
{
  "userName": "ahmed",
  "email": "ahmed@example.com",
  "password": "123456",
  "passwordConfirm": "123456"
}
```

بيتعمل يوزر بدور Client.

**لوجين**

`POST /api/Account/login`

```json
{ "email": "ahmed@example.com", "password": "123456" }
```

الرد فيه `token`. حطه في الهيدر:

```
Authorization: Bearer <token>
```

**نسيت الباسورد**

1. `POST /api/Account/forgot-password` `{ "email": "..." }`
2. إيميل فيه لينك: `http://localhost:4200/reset-password?email=...&token=...`
3. `POST /api/Account/reset-password` بالإيميل والتوكن والباسورد الجديد

### ج) الحجز والدفع (أهم سيكل في المشروع)

```
اختار فندق + تواريخ
  → شوف الغرف المتاحة
  → احسب السعر
  → أنشئ حجز Pending
  → لوجين (لو هتدفع من الـ API الحالي)
  → اعمل لينك Paymob
  → الضيف يدفع
  → Webhook أو Poll
  → Confirmed + IsPaid = true
```

**1) التوفر**

`POST /api/hotels/{encodedHotelId}/rooms`

```json
{ "checkIn": "2026-10-01", "checkOut": "2026-10-03" }
```

كل أوضة ترجع ومعاها `isBooked` لو فيه حجز متداخل في نفس الفترة.

**مهم:** فحص التوفر **متوقف** جوه `CreateBookingAsync` (السطر متعمل له كومنت). يعني الحجز بيتقبل حتى لو الأوضة محجوزة.

**2) السعر**

`POST /api/hotels/calculate-price`

نفس شكل `BookingRequestDto` (فندق + أوضة متشفّرين + تواريخ + اسم/إيميل).

**3) إنشاء الحجز (من غير توكن)**

`POST /api/bookings`

```json
{
  "encodedHotelId": "...",
  "encodedRoomId": "...",
  "checkIn": "2026-10-01",
  "checkOut": "2026-10-03",
  "customerName": "Ahmed Ali",
  "customerEmail": "ahmed@example.com"
}
```

النتيجة: `status: Pending`, `isPaid: false`, `encodedId` للحجز، `totalPrice`.

**4) لينك الدفع (محتاج JWT)**

`POST /api/Payment/test`

```json
{
  "encodedBookingId": "...",
  "amountCents": 12800,
  "currency": "EGP",
  "billingData": { "name": "Ahmed", "email": "ahmed@example.com", "phone": "01000000000" },
  "items": []
}
```

الـ API:

1. يجيب توكن من Paymob
2. يعمل Payment Link
3. يخزّن رقم الأوردر في `Booking.PaymentReference`

الرد: `paymentUrl` + `paymobOrderId`. افتح الـ URL وادفع.

**5) تأكيد الدفع (واحد من دول)**

| الطريقة | المسار | ملاحظة |
| --- | --- | --- |
| Webhook من Paymob | `POST /api/Webhook/callback` | لو `success` يستعلم تاني من Paymob ولو PAID يأكد الحجز |
| استعلام من العميل | `GET /api/Payment/check-order/{paymobOrderId}` | محتاج JWT |
| تأكيد يدوي قديم | `POST /api/bookings/confirm-payment?paymentRef=` | **من غير لوجين** — خطر، يأكد من غير ما يسأل Paymob |

التحقق من توقيع HMAC لـ Paymob **مش متبرمج** (`ValidateHmacSignature` بيرمي NotImplemented).  
استعلام حالة الأوردر حالياً بياخد **توكن تجريبي** دايماً (`isLive: false`) فالدفع اللايف مش هيتأكد صح لحد ما يتظبط.

**6) حجوزاتي**

`GET /api/bookings/my?hotelId=&from=&to=&status=`  
محتاج JWT. بيفلتر على `CustomerEmail` من كليم التوكن.

**7) كل الحجوزات للأدمن**

`GET /api/admin/bookings`  
`[Authorize(Roles = "Admin")]`

### د) لوحة الأدمن / المحتوى

```
ارفع ملفات → اعمل فندق → اعمل غرف → Insight (مطعم/كافيه)
→ سلايدر / About / Contact / صور الأقسام
→ يوزرز
→ شركاء + إيميل جماعي
```

**رفع ملف**

`POST /api/upload?type=hotel`  
`multipart/form-data` فيلد اسمه `files`

أنواع `PlazaInnType`:  
`hotel`, `room`, `restaurant`, `cafe`, `restaurantMenu`, `cafeMenu`, `logo`, `homeSlider`, `aboutUs`

**فندق**

- `GET/POST /api/Hotel`
- `GET/PUT/DELETE /api/Hotel/{encodedId}`
- `GET /api/Hotel/lookup` → id متشفّر + الاسمين بس

Create يحتاج على الأقل صورة واحدة + `logoId` + حقول EN/AR.

**غرفة**

- CRUD على `/api/Room`
- غرف فندق: `GET /api/Room/hotel/{encodedHotelId}`
- النوع: `Single` / `Double` / `Suite`

**Insight**

CRUD على `/api/InsightHotel` مربوط بـ `encodedHotelId`

**سلايدر / About / Contact / Section images**

| المسار | الوظيفة |
| --- | --- |
| `/api/HomeSlider` | GET الكل، POST، GET/PUT/DELETE بالـ id الرقمي |
| `/api/AboutUs` | GET / PUT |
| `/api/ContactUs` | GET / PUT (مع مواعيد العمل) |
| `/api/section-images?type=` | GET / PUT صور حسب النوع |

**يوزرز**

`/api/User` — GET الكل، GET بالـ GUID، POST (باسورد + Role)، PUT، DELETE

**شركاء**

- CRUD `/api/Partner`
- شركاء فندق: `GET /api/Partner/hotel/{encodedId}`
- إيميل لكل شركاء الفندق: `POST /api/Partner/broadcastEmail/hotel/{encodedId}`  
  فورم: Subject, Body, Attachments — بيتبعت بالتوازي بحد أقصى 5 في نفس الوقت + توقيع HTML للفندق

---

## 7) خريطة الـ API كاملة

Base: `https://localhost:7142`

### Account — `/api/Account`

| Method | Path | Auth |
| --- | --- | --- |
| POST | `/login` | لا |
| POST | `/register` | لا |
| POST | `/forgot-password` | لا |
| POST | `/reset-password` | لا |

### Hotel / Room

| Method | Path |
| --- | --- |
| GET POST | `/api/Hotel` |
| GET PUT DELETE | `/api/Hotel/{encodedId}` |
| GET | `/api/Hotel/lookup` |
| GET POST | `/api/Room` |
| GET PUT DELETE | `/api/Room/{encodedId}` |
| GET | `/api/Room/hotel/{encodedId}` |

### Availability + Booking + Payment

| Method | Path | Auth |
| --- | --- | --- |
| POST | `/api/hotels/{encodedHotelId}/rooms` | لا |
| POST | `/api/hotels/calculate-price` | لا |
| POST | `/api/bookings` | لا |
| POST | `/api/bookings/confirm-payment` | لا (خطر) |
| GET | `/api/bookings/my` | JWT |
| GET | `/api/admin/bookings` | JWT Admin |
| POST | `/api/Payment/test` | JWT |
| GET | `/api/Payment/check-order/{paymobOrderId}` | JWT |
| POST | `/api/Webhook/callback` | Paymob |

### CMS

| Method | Path |
| --- | --- |
| POST | `/api/upload?type=` |
| CRUD | `/api/HomeSlider` |
| GET PUT | `/api/AboutUs` |
| GET PUT | `/api/section-images?type=` |
| GET PUT | `/api/ContactUs` |
| CRUD | `/api/InsightHotel/{encodedHotelId}` |
| CRUD + broadcast | `/api/Partner` |
| CRUD | `/api/User` |

لو كيان مش موجود، كتير من الـ GET بيرجع `200 OK` فاضي بدل `404`.

---

## 8) فين الملفات لو هتعدّل حاجة

| عايز تعدّل | المكان |
| --- | --- |
| Endpoints | `Plaza/Controllers/` |
| تسجيل الخدمات | `Plaza/Extensions/ApplicationServices.cs` |
| JWT / CORS / Static files | `Plaza/Program.cs` |
| أخطاء عامة | `Plaza/MiddelWare/ExceptionHandlingMiddleware.cs` |
| مابينج | `Plaza/Mapping/` |
| حجز ودفع | `PlazaService/BookingService/` و `PlazaService/Payment/PaymentService.cs` |
| لوجين | `PlazaService/Account/AuthService.cs` و `JwtTokenGenerator.cs` |
| رفع ملفات | `PlazaService/Hotels/FileService.cs` |
| كيانات | `PlazaCore/Entites/` |
| فلاتر SQL | `PlazaCore/Specification/` |
| DTOs | `Shared/DTO/` |
| Migrations | `PlazaRepository/Data/Migrations/` |

`IMenuService` مسجّل في DI ومفيش MenuController منفصل — المنيو عبر InsightHotel (PDF ids).

---

## 9) عقد الفرونت (لو هترجع للـ Angular)

- الكتالوج عام. الدفع و my bookings محتاجين Bearer token.
- استخدم encoded ids للفندق/الأوضة/الحجز.
- ارفع الصور الأول، بعدين ابعت الـ ids في Create/Update.
- بعد الحجز: ادفع بالـ encoded booking id والمبلغ **بالقروش** (`amountCents`).
- بعد ما الضيف يرجع من Paymob: `GET /api/Payment/check-order/{paymobOrderId}`.
- اعرض الحقول العربي والإنجليزي من نفس الـ JSON.
- لينك الريست باسورد متثبت على localhost:4200 — غيّره حسب البيئة.

---

## 10) مشاكل معروفة وأنت كاتب المشروع

1. إنشاء الحجز مش بيفحص التوفر.
2. التوفر بيحسب **كل** الحجوزات المتداخلة بما فيها Pending مش مدفوع (ممكن تقفل أوضة على حجز معرفش يتدفع).
3. تأكيد الدفع اليدوي مفتوح.
4. HMAC مش شغال.
5. Order status دايماً test.
6. صلاحيات الأدمن على الكتابة متوقفة.
7. Seed الأدمن بإيميل فاضي.
8. ليستة الحجوزات بتحمل الجدول كله في الميموري بعدين تفِلتر.
9. مفيش تستات.
10. Namespaces مختلطة: `Plaza` / `PlazaAPI` / `PlazaApi`.
11. كود قديم متعمل له كومنت في أكثر من Controller.

خطوات تحويله لبرودكشن بالترتيب في القسم اللي تحت، وفي الرسالة اللي اتشرحتلك بالعربي.

---

## 11) تشيك ليست البرودكشن (ملخص)

1. طلّع كل الأسرار من git وغيّرها.
2. قفّل الكتابة على Admin JWT.
3. امنع الحجز المزدوج + expire لـ Pending.
4. شيل confirm-payment المفتوح، كمّل HMAC، فرّق test/live في Paymob.
5. CORS لدومين الفرونت بس، HTTPS إجباري، Swagger يتقفل برّه Development.
6. صلّح Seed الأدمن، وURL الريست باسورد، وفلاتر الحجوزات في SQL.
7. لوجينج + باك أب SQL + اختبار حجز كامل على Paymob test بعدين live.
8. بعد كده تحسينات: إلغاء حجز، ريتري للـ webhook، تستات، pagination.

ده المشروع من أوله لآخره. لو هتكمل شغل، ابدأ من النقطتين 1 و 2 قبل أي فيتشر جديدة.
