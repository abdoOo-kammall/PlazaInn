# دليل المطوّر والاختبار — Plaza Inn API

هذا الدليل مأخوذ من الـ Controllers وملفات الـ DTO والخدمات والـ Middleware في حل `Plaza_SOL` (ASP.NET Core 8). المسارات ومعاملات الاستعلام وأسماء خصائص الـ JSON مطابقة للكود. أسماء الخصائص في الـ JSON بصيغة **camelCase**.

لا توجد واجهة أمامية داخل هذا المستودع. العميل المتوقع هو تطبيق Angular على `http://localhost:4200`.

**لا تنسخ الأسرار من `Plaza/appsettings.json` إلى العملاء أو التذاكر.** الملف يحتوي حالياً على سلسلة اتصال SQL ومفتاح توقيع JWT وبيانات SMTP وأسرار Paymob التجريبية. هذا الدليل يذكر أسماء مفاتيح الإعداد فقط. بريد المدير وكلمة المرور المزروعان أدناه هما القيمتان اللتان يُنشئهما التطبيق.

---

## 1. How to Run & Access Swagger / كيفية تشغيل المشروع محلياً

### المتطلبات

- .NET 8 SDK
- SQL Server يمكن الوصول إليه عبر `ConnectionStrings:DefaultConnection`
- أدوات EF Core بنفس إصدار الحزمة في المستودع (`Microsoft.EntityFrameworkCore.Tools` 8.0.20):

```powershell
dotnet tool install --global dotnet-ef --version 8.0.20
```

شغّل كل الأوامر التالية من جذر الحل (`Plaza_SOL`، المجلد الذي يحتوي `Plaza_SOL.sln`).

### إعداد قاعدة البيانات

`Program.cs` لا يستدعي `Database.Migrate()` عند الإقلاع. طبّق الـ Migrations بنفسك قبل أول تشغيل. وجّه `ConnectionStrings:DefaultConnection` في `Plaza/appsettings.json` (أو User Secrets أو متغيرات البيئة) إلى قاعدة SQL Server التي تريد استخدامها.

### البناء وتطبيق الـ Migrations والتشغيل

```powershell
dotnet build Plaza_SOL.sln
dotnet ef database update --project PlazaRepository --startup-project Plaza
dotnet run --project Plaza
```

الأمر `dotnet run --project Plaza` يستخدم أول ملف تشغيل في `Plaza/Properties/launchSettings.json` وهو `http`:

| الإعداد | القيمة |
| --- | --- |
| العنوان | `http://localhost:5141` |
| Swagger UI | `http://localhost:5141/swagger` |
| OpenAPI JSON | `http://localhost:5141/swagger/v1/swagger.json` |
| فحص الصحة | `GET http://localhost:5141/health` |
| البيئة | `Development` (`ASPNETCORE_ENVIRONMENT`) |

واجهة Swagger مسجّلة في كل البيئات (`app.UseSwagger()` / `app.UseSwaggerUI()`)، وليس في Development فقط. إعادة التوجيه إلى HTTPS متوقفة ما لم تكن `UseHttpsRedirection` تساوي `true`. ملف التشغيل الاختياري `https` هو `https://localhost:7142` و `http://localhost:5141`:

```powershell
dotnet run --project Plaza --launch-profile https
```

Swagger لهذا الملف: `https://localhost:7142/swagger`.

عند الإقلاع يُزرع أيضاً أدوار Identity وحساب المدير. فشل الزراعة يُسجَّل في السجل ويستمر تشغيل الـ API. غياب الـ Migration أو تعذّر الوصول إلى قاعدة البيانات يُفشل فحص `/health` وأي نقطة تقرأ البيانات.

### حساب المدير المزروع

الأدوار التي ينشئها `DefaultRole`: `Admin` و `Client` و `User`.

حساب المدير الذي ينشئه `DefaultUser`، ويمكن تجاوزه من الإعدادات إن وُجدت:

| مفتاح الإعداد | القيمة الافتراضية في الكود وفي `appsettings.json` |
| --- | --- |
| `AdminSeed:Email` | `admin@plaza.com` |
| `AdminSeed:Password` | `Admin@123` |

`EmailConfirmed` يُضبط على `true`. يُضاف المستخدم إلى دور `Admin`.

### تسجيل الدخول وتفويض Swagger

1. افتح `http://localhost:5141/swagger`.
2. افتح **POST** `/api/Account/login`.
3. اضغط **Try it out** وأرسل:

```json
{
  "email": "admin@plaza.com",
  "password": "Admin@123"
}
```

4. الاستجابة الناجحة HTTP 200:

```json
{
  "message": "Login successful.",
  "token": "<jwt>"
}
```

5. انسخ قيمة `token` فقط. لا تكتب كلمة `Bearer` داخل نافذة Swagger. مخطط الأمان هو HTTP bearer (`BearerFormat: JWT`)، وSwagger يضيف البادئة `Bearer` بنفسه.
6. اضغط **Authorize** أعلى Swagger UI، الصق الرمز، ثم أكّد.
7. الطلبات المحمية تُرسل بعد ذلك `Authorization: Bearer <jwt>`.

تنتهي صلاحية الرمز بعد **ساعة واحدة** من إصداره (`JwtTokenGenerator`). انحياز الساعة يساوي صفراً. المطالبات التي يستخدمها الـ API:

| Claim | المعنى |
| --- | --- |
| `Id` | معرّف مستخدم ASP.NET Identity. مطلوب في إجراءات الحجز والدفع. |
| `Email` | يُحفظ على الحجز في `customerEmail` عندما تكون المطالبة موجودة. |
| `UserName` | نوع مطالبة الاسم. |
| `role` | `Admin` أو `Client` أو `User`. فحص الأدوار يستخدم هذه المطالبة (`RoleClaimType = "role"`). |

المُصدِر هو `Jwt:Issuer` (`PlazaAPI`). الجمهور هو `Jwt:Audience` (`PlazaClient`).

التسجيل العام (`POST /api/Account/register`) يمنح دائماً دور **Client**. استخدم حساب المدير أعلاه، أو `POST /api/User` بصلاحية مدير، عندما تحتاج دور Admin.

---

## 2. System Lifecycle & Entity Relationships / دورة العمل الكاملة والترابط بين الموديولات

### 2.1 رفع الملفات (`FileController`) و `PlazaInnType`

`POST /api/upload` للمدير فقط. يقبل `multipart/form-data` مع ملف واحد أو أكثر في حقل النموذج **`files`**، بالإضافة إلى معامل الاستعلام **`type`** من النوع `PlazaInnType`.

يعيد الرفع مصفوفة JSON من **أرقام صور صحيحة خام**، مثل `[12, 13]`. هذه الأرقام هي ما تتوقعه دوال الإنشاء والتعديل في `imageIds` (والقوائم المرتبطة أدناه). ليست معرّفات مشفّرة.

| قيمة `type` | الرقم | مكان الحفظ | الملفات المسموحة | تُمرَّر إلى |
| --- | --- | --- | --- | --- |
| `hotel` | 0 | `wwwroot/images/hotel/` | `.jpg` و `.jpeg` و `.png` و `.webp` | `CreateHotelDto.imageIds` |
| `room` | 1 | `wwwroot/images/room/` | صور | `CreateRoomDTO.imageIds` |
| `restaurant` | 2 | `wwwroot/images/restaurant/` | صور | `CreateInsightHotelDto.restaurantImageIds` |
| `cafe` | 3 | `wwwroot/images/cafe/` | صور | `cafeImageIds` |
| `restaurantMenu` | 4 | `wwwroot/menus/restaurantMenu/` | `.pdf` فقط | `restaurantMenuPdfIds` |
| `cafeMenu` | 5 | `wwwroot/menus/cafeMenu/` | `.pdf` فقط | `cafeMenuPdfIds` |
| `logo` | 6 | `wwwroot/images/logo/` | صور | `CreateHotelDto.logoId` (رقم واحد) |
| `homeSlider` | 7 | `wwwroot/images/homeSlider/` | صور | `CreateHomeSliderDto.imagesId` |
| `aboutUs` | 8 | `wwwroot/images/aboutUs/` | صور | معرّفات المربع والدائرة في `UpdateAboutUsDto` |
| `None` | 9 | — | مرفوض في الرفع | يُستخدم داخلياً عند فك ربط صورة قسم |
| `trip` | 10 | `wwwroot/images/trip/` | صور | `CreateTripDto.imageIds` |
| `transport` | 11 | `wwwroot/images/transport/` | صور | `CreateTransportDto.imageIds` |
| `event` | 12 | `wwwroot/images/event/` | صور | `CreateEventDto.imageIds` |
| `package` | 13 | `wwwroot/images/package/` | صور | `CreatePackageDto.imageIds` |

`None` وأي رقم غير معرّف يُرفضان بـ 400 والنص `"Invalid file type."`.

الحدود التي يفرضها `FileController` و `FileService`:

- الطلب كاملاً: 50 ميجابايت (`RequestSizeLimit`).
- كل ملف: 10 ميجابايت.
- الملفات الفارغة مرفوضة.
- الامتداد يجب أن يطابق توقيع الملف (JPEG أو PNG أو WEBP أو PDF).
- الملف الذي تطابق بصمة SHA-256 له ملفاً موجوداً لنفس `PlazaInnType` لا يُحفظ مرة أخرى. يُعاد معرّف الصورة الموجود.

في استجابات الفندق والغرفة والرحلة والنقل والفعالية والباقة وInsight والـ Home Slider وAbout Us تُوسَّع هذه المعرّفات إلى `{ "id": <int>, "url": "<BaseUrl><relative path>" }`. قيمة `BaseUrl` في الإعداد هي `http://plazainn.runasp.net/`. مثال: `http://plazainn.runasp.net/images/trip/12.jpg`.

`GET /api/section-images` مختلف: يعيد المسار النسبي المخزَّن على صف `Image` (`images/trip/12.jpg`)، وليس الرابط المطلق.

التسلسل المعتاد:

1. `POST /api/upload?type=trip` مع ملفات الصور.
2. اقرأ الأرقام الصحيحة من الاستجابة.
3. أرسلها في `imageIds` ضمن `POST /api/Trip` (أو الخاصية المقابلة عند التعديل).

### 2.2 تشفير المعرّفات (`IdEncoder`)

المفاتيح الأساسية الصحيحة للفنادق والغرف والرحلات والنقل والفعاليات والباقات والشركاء والحجوزات لا تُعرض كأرقام خام على هذه الموارد ولا على لوحة تحليلات المدير. `IdEncoder` يبني نص Base64 آمناً لعناوين URL من `{id}:PlazaInnKey2025` (UTF-8)، ثم يحذف الحشو ويستبدل `+` بـ `-` و `/` بـ `_`.

مثال: معرّف قاعدة البيانات `1` يُشفَّر إلى `MTpQbGF6YUlubktleTIwMjU`.

| الـ API يعيد أو يتوقع نصاً مشفّراً | الـ API يستخدم معرّفاً خاماً |
| --- | --- |
| معرّفات المسار للفندق والغرفة والرحلة والنقل والفعالية والباقة والشريك والحجز (`encodedId`) | استجابة `POST /api/upload` وكل قوائم `imageIds` / `imagesId` / `logoId` / قوائم قوائم الطعام |
| `encodedHotelId` و `encodedRoomId` و `encodedTripId` و `encodedTransportId` و `encodedEventId` و `encodedPackageId` و `encodedBookingId` | معامل `hotelId` في `GET /api/bookings/my` و `GET /api/admin/bookings` (المفتاح الأساسي الصحيح للفندق) |
| تكوين الباقة: `hotelId` و `roomId` و `tripIds` و `transportIds` و `eventIds` (نصوص مشفّرة) | `GET /api/HomeSlider/{id}` و `AboutUsDto.id` و `ContactUsDto.id` و `HomeSliderDto.id` و `WorkingHourDto.id` |
| أجسام الـ lookup: `{ "id": "<encoded>", "name", "nameAr" }` | `GET /api/User/{userId}` — نص معرّف مستخدم Identity (GUID) |
| | `GET /api/Payment/check-order/{paymobOrderId}` — رقم طلب Paymob |

`IdEncoder.DecodeId` يرمي `ArgumentException("Invalid ID")` عندما لا يكون النص رمزاً صالحاً. المتحكمات التي تلتقط الاستثناء تعيد 400 بنص عادي مثل `"Invalid hotel ID"`. `GET` والإلغاء على `/api/bookings/{encodedId}` لا يلتقطان الاستثناء؛ وسيط الاستثناءات يحوّله إلى 400 من نوع `ProblemDetails` وقيمة `detail` هي `"Invalid ID"`.

عمليات القراءة للقوائم والتفاصيل (`Trip` و `Transport` و `Event` و `Package`) تخفي الصفوف ذات `isActive: false` عن الزائر المجهول وعن الرمز الذي ليس لمدير. رمز المدير يرى الصفوف غير النشطة. غير المدير الذي يطلب معرّفاً مشفّراً لعنصر غير نشط يحصل على **404**.

### 2.3 الخدمات المستقلة والباقات المجمّعة

`Trip` و `Transport` و `Event` كتالوجات مستقلة. يمكن عرض كل منها وجلبه وحجزه وحده. يستطيع المدير أيضاً ربط أي مجموعة منها بـ `Package`.

للباقة `packagePrice` و `maxPackagesAvailable` خاصان بها. السعر الذي يُحصَّل عند الحجز **ليس** مجموع أسعار المكوّنات. هو `quantity * packagePrice` ثم ضريبة الحجز (أدناه).

ثلاثية الإقامة الاختيارية في `CreatePackageDto` / `UpdatePackageDto`:

- `hotelId` و `roomId` و `nightsCount` يجب إرسالها **معاً**، أو حذفها كلها.
- `hotelId` و `roomId` معرّفان مشفّران. يجب أن تنتمي الغرفة إلى ذلك الفندق.
- `nightsCount` رقم صحيح من 1 إلى 365.
- إذا وُجد أحد الثلاثة وغاب آخر، يعيد التحقق 400: `"Hotel, room, and nights must be provided together."`

يجب أن تحتوي الباقة على عنصر واحد على الأقل: ثلاثية الفندق، أو معرّفاً واحداً على الأقل في `tripIds` أو `transportIds` أو `eventIds`. يمكن تعيينها كلها معاً.

السعة **لا تُشارَك** بين الباقة ومكوّنات الرحلة أو النقل أو الفعالية:

- حجز رحلة مستقل يستهلك `trip.maxCapacity` (عدد الضيوف) لذلك تاريخ الخدمة.
- حجز نقل مستقل يستهلك `availableVehicles` لذلك التاريخ.
- حجز فعالية مستقل يستهلك `totalTickets`.
- حجز الباقة يستهلك `maxPackagesAvailable` فقط.
- إذا تضمنت الباقة ثلاثية الفندق/الغرفة/الليالي، يستهلك الحجز أيضاً هذا العدد من الغرف من مجمّع غرف الفندق لمدة `nightsCount` بدءاً من `serviceDate`. هذا المجمّع **مشترك** مع حجوزات الفندق العادية.

قراءة الكتالوج عامة. إنشاء صفوف الكتالوج وتعديلها وحذفها يتطلب `Admin`.

### 2.4 دورة الحجز ودفع Paymob

الإعداد تحت `Booking` (القيم الافتراضية في `BookingOptions` و `appsettings.json`):

| المفتاح | القيمة | الأثر |
| --- | --- | --- |
| `TaxRate` | `0.28` | إجمالي الفندق والكتالوج يُضرب في `1.28` ويُقرَّب إلى منزلتين (منتصف بعيد عن الصفر). |
| `PendingExpiryMinutes` | `30` | الحجز `Pending` يحجز المخزون لمدة 30 دقيقة من `createdAt` (UTC). |
| `CleanupIntervalMinutes` | `5` | `ExpiredBookingsCleanupService` يعمل بهذا الفاصل. |

**الخطوة 1 — `POST /api/bookings` (أي مستخدم مصادق).**

الإجراء محدود المعدل (5 عمليات إنشاء كل 10 دقائق لكل معرّف مستخدم). الجسم هو `BookingRequestDto`. `bookingType` يساوي `Hotel` (`0`) عند حذفه.

يتم الإدراج داخل معاملة بعزل **Serializable** حتى لا يستطيع قراءة السعة والإدراج تجاوز الحجز المتزامن.

- الحالة تُضبط على `Pending`، و `isPaid` يساوي `false`.
- يُخزَّن معرّف طلب تاجر `PLZ-{guid:N}`.
- `userId` يأتي من مطالبة `Id` في JWT.
- إذا كان لدى JWT مطالبة `Email`، فإن هذا البريد **يستبدل** `customerEmail` على الحجز المحفوظ.
- المخزون الذي يُحتسب: حجوزات `Confirmed`، وحجوزات `Pending` التي ما زال `createdAt` لها داخل نافذة 30 دقيقة. صفوف `Cancelled` والحجوزات المعلقة المنتهية لا تُحتسب.

فندق (`bookingType` يساوي 0):

- يتطلب `encodedHotelId` و `encodedRoomId` و `checkIn` و `checkOut` وحقول العميل.
- يجب أن تنتمي الغرفة إلى الفندق.
- تاريخ الوصول لا يمكن أن يكون قبل اليوم (UTC). مدة الإقامة من 1 إلى 30 ليلة (`StayRules.MaxStayNights`).
- `roomsCount` من 1 إلى 20. الضيوف (`adults + children`) لا يمكن أن يتجاوزوا `maxSleep * roomsCount`، زائد 1 عندما يكون `extraBed` صحيحاً.
- `extraBed: true` يتطلب `room.extraBed == true`.
- السعر: `((pricePerNight * roomsCount) + (extraBed ? priceForExtraBed : 0)) * nights * 1.28`.
- التوفر يستخدم مجمّعين على الفندق: `numOfAvailableRoomsToReserve` لـ `Single` و `Double`، و `numOfAvailableSuitesToReserve` لـ `Suite`. الليلة الأكثر ازدحاماً في الإقامة هي التي تُعتمد. الصف الذي `roomsBooked` فيه يساوي 0 يشغل غرفة واحدة مع ذلك.
- الغرف غير الكافية → **409**.

رحلة (`1`): `encodedTripId` و `serviceDate` مطلوبان. الضيوف = `adults + children` (واحد على الأقل). `quantity` في الطلب يُتجاهل؛ الكمية المخزَّنة هي عدد الضيوف. التاريخ يجب أن يقع داخل `startDate`/`endDate` عند تعيينهما، ولا يمكن أن يكون في الماضي (UTC). السعر: `(adults * pricePerPerson + children * pricePerChild) * 1.28`. تجاوز السعة → **409**.

نقل (`2`): `encodedTransportId` و `serviceDate` مطلوبان. `quantity` هو عدد المركبات (1–1000). الركاب (`adults + children`) لا يمكن أن يتجاوزوا `quantity * maxPassengers`. السعر: `quantity * pricePerTrip * 1.28`.

فعالية (`3`): `encodedEventId` مطلوب. `quantity` هو عدد التذاكر. `serviceDate` اختياري؛ عند إرساله يجب أن يقع ضمن تواريخ الفعالية، لكن `serviceDate` **المخزَّن** هو `eventDate` في الكتالوج. السعر: `quantity * ticketPrice * 1.28`. الفعاليات المنتهية → **409**.

باقة (`4`): `encodedPackageId` مطلوب. `quantity` هو عدد الباقات المراد شراؤها. السعر: `quantity * packagePrice * 1.28`. `serviceDate` مطلوب عندما يكون للباقة غرفة أو تاريخ بداية/نهاية. مع وجود غرفة، تسجيل الوصول هو `serviceDate` وتسجيل المغادرة هو `serviceDate + nightsCount`، ويجب أن يتوفر هذا العدد من الغرف.

عناصر الكتالوج غير النشطة → **409** `"This {item} is not available."` الصفوف المفقودة → **404**.

**الخطوة 2 — `POST /api/Payment/initiate` (والاسم البديل `POST /api/Payment/test`).**

أي مستخدم مصادق يملك الحجز، أو مدير، يمكنه استدعاؤها. يحمّل الخادم الحجز ثم:

- يرفض أي حالة غير `Pending` بـ **409**.
- يرفض الحجز الأقدم من 30 دقيقة بـ **409** `"The booking hold has expired. Please create a new booking."`
- يحسب `amountCents` كـ `round(totalPrice * 100)`. قيم `amountCents` و `currency` و `items` في الطلب تُتجاهل.
- يطلب من Paymob Accept (`https://accept.paymob.com/api`) رابط دفع. `Paymob:IsLive` يساوي `false`، لذلك تُستخدم مفاتيح الاختبار تحت `PaymobTest`. العملة هي `Paymob:Currency` أو `EGP`.
- كل محاولة تحصل على معرّف طلب تاجر جديد `PLZ-{bookingId}-{guid:N}` لأن Paymob يرفض إعادة استخدام معرّف طلب التاجر. يُحفظ هذا المعرّف ومعرّف طلب Paymob على الحجز (`paymentReference` = معرّف طلب Paymob).

الاستجابة:

```json
{
  "paymentUrl": "https://accept.paymob.com/...",
  "paymobOrderId": 123456789,
  "amountCents": 128000,
  "currency": "EGP"
}
```

يفتح الضيف `paymentUrl` ويدفع.

**الخطوة 3 — التأكيد، بأي من المسارين.**

- استدعاء Paymob من الخادم: **`POST /api/Webhook/callback?hmac=<signature>`**. لا يوجد مسار `/api/Webhook/paymob`. يجب أن يحتوي الجسم على `obj` (المعاملة). `type`، عند وجوده، يجب أن يكون `TRANSACTION`؛ أي نوع آخر يعيد 200 ويُتجاهل. يُتحقق من HMAC مقابل حقول المعاملة. يجب أن يكون `success` صحيحاً وألا يكون `pending` صحيحاً. ثم يستدعي الـ API خدمة Paymob مرة أخرى (`GetOrderStatusAsync`) ولا يثق بالجسم وحده.
- استعلام العميل: **`GET /api/Payment/check-order/{paymobOrderId}`** (المالك المصادق أو المدير). نفس أسلوب التأكيد عندما تُبلغ Paymob أن الطلب مدفوع.

`ConfirmBookingPaymentAsync` (معاملة Serializable):

- يجد الحجز عبر `paymentReference`، أو، إذا استبدلت محاولة دفع أحدث هذا المرجع، عبر معرّف الحجز المضمَّن في `PLZ-{bookingId}-{guid}`.
- إذا كان الحجز `Confirmed` لهذا الطلب أصلاً، يعيد نجاحاً (idempotent). دفعة ثانية ناجحة على حجز مؤكد تُسجَّل لاسترداد يدوي وتعيد نجاحاً أيضاً.
- القروش المدفوعة يجب أن تساوي القروش المتوقعة وإلا يبقى الحجز كما هو.
- الدفع المتأخر على حجز منتهٍ أو ملغى يُؤكَّد فقط عندما يكون المخزون ما زال متاحاً. وإلا يُسجَّل لاسترداد يدوي وتكون `booking_confirmed` تساوي false.
- عند النجاح: `isPaid = true`، و `status = Confirmed`، ويُحدَّث `paymentReference`.

**الخطوة 4 — انتهاء المهلة.**

`ExpiredBookingsCleanupService` يختار صفوف `Pending` التي `createdAt` فيها أقدم من 30 دقيقة ويضبط `status` على `Cancelled`. إلى أن تعمل هذه المهمة، الحجز المنتهي يتوقف أصلاً عن احتساب المخزون، ويُرفض بدء الدفع.

**الإلغاء.** `POST /api/bookings/{encodedId}/cancel`. يمكن للمالك إلغاء حجز `Pending`. حجز `Confirmed` لا يلغيه إلا مدير (**409** لغير ذلك). إلغاء حجز `Cancelled` أصلاً يعيده دون تغيير. لا يُصدر الـ API استرداداً من Paymob.

---

## 3. Full API Reference & Copy-Paste JSON Payloads / دليل الـ Endpoints الكامل مع أمثلة JSON

### اصطلاحات كل الأمثلة

**التفويض على المتحكم**

- بلا سمة، أو `[AllowAnonymous]`: لا رمز.
- `[Authorize]`: أي JWT صالح (`Admin` أو `Client` أو `User`).
- `[Authorize(Roles = "Admin")]`: JWT مطالبة `role` فيه هي `Admin`. الرمز الصالح بدور آخر يحصل على **403** بجسم فارغ (تحدي الإطار)، وليس `ProblemDetails`.

`[Authorize(Roles = "Admin")]` على مستوى الصنف هو الافتراضي لـ Hotel و Room و InsightHotel و Partner و User و Trip و Transport و Event و Package و File و AdminDashboard. الدوال المعلَّمة بـ `[AllowAnonymous]` تتجاوز ذلك للقراءات العامة.

**قيم الـ Enum في أجسام JSON أرقام صحيحة.** لا يوجد `JsonStringEnumConverter`. نص مثل `"Hotel"` يفشل في فك JSON ويعيد **400**. ربط معاملات الاستعلام يقبل الأسماء (`?status=Pending` و `?bookingType=Trip` و `?type=trip`).

| Enum | الاسم = القيمة |
| --- | --- |
| `BookingType` | `Hotel=0`، `Trip=1`، `Transport=2`، `Event=3`، `Package=4` |
| `BookingStatus` | `Pending=0`، `Confirmed=1`، `Cancelled=2` |
| `RoomType` | `Single=0`، `Double=1`، `Suite=2` |
| `Role` | `Admin=1`، `Client=2`، `User=3` (`0` غير صالح) |
| `DayOfWeek` | `Sunday=0` … `Saturday=6` |
| `PlazaInnType` | انظر القسم 2.1 |

التواريخ بصيغة ISO-8601. خدمة الحجز تستخدم `.Date` (اليوم التقويمي UTC). أرسل قيماً بتاريخ فقط مثل `"2026-11-10"`.

المعرّفات المشفّرة في الأمثلة تستخدم الترميز الحقيقي لمعرّف قاعدة البيانات `1` (`MTpQbGF6YUlubktleTIwMjU`) حتى يكون الشكل حقيقياً. استبدلها بالمعرّفات التي تعيدها عمليات الإنشاء أو القوائم أو lookup.

فشل التحقق من `[ApiController]` يعيد **400** من نوع `ValidationProblemDetails` (`application/problem+json`) **قبل** تنفيذ الإجراء:

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

استثناءات الخدمات تستخدم الشكل في القسم 5. بعض الإجراءات تعيد `BadRequest("some string")`، وهو **نص** JSON وليس `ProblemDetails`.

---

### 3.1 Account / Auth — `api/Account`

لا توجد سمة `[Authorize]`. كل إجراء يستخدم حد `auth`: **10 طلبات في الدقيقة لكل عنوان IP للعميل**.

#### `POST /api/Account/login`

التفويض: مجهول. الجسم: `LoginDTO`.

| الحقل | القواعد |
| --- | --- |
| `email` | مطلوب، صيغة بريد |
| `password` | مطلوب، 6 أحرف على الأقل |

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

401 (مستخدم غير موجود أو كلمة مرور خاطئة):

```json
{ "message": "Invalid email or password." }
```

401 بعد 5 كلمات مرور فاشلة لمستخدم موجود (قفل 15 دقيقة):

```json
{ "message": "Account is temporarily locked due to multiple failed login attempts. Please try again later." }
```

#### `POST /api/Account/register`

الجسم: `RegisterDTO`. ينشئ مستخدم Identity ويضيف دور **Client**. قواعد كلمة مرور Identity: 8 على الأقل، رقم، حرف صغير، حرف كبير، والحرف الخاص غير مطلوب.

| الحقل | القواعد |
| --- | --- |
| `userName` | مطلوب، 3–50 حرفاً |
| `email` | مطلوب، صيغة بريد، ويجب أن يكون فريداً |
| `password` | مطلوب، 8–100 حرف |
| `passwordConfirm` | مطلوب، يجب أن يساوي `password` |

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

400 عندما يكون البريد موجوداً أو ترفض Identity كلمة المرور:

```json
{ "message": "Email is already registered." }
```

#### `POST /api/Account/forgot-password`

الجسم: `ForgotPasswordDTO`. يعيد دائماً نفس 200، سواء وُجد البريد أم لا. عند وجود المستخدم تُرسل رسالة برابط `{FrontendBaseUrl}/reset-password?email=...&token=...` (`FrontendBaseUrl` هو `http://localhost:4200`).

```json
{ "email": "sara@example.com" }
```

200:

```json
{ "message": "If an account with that email exists, a password reset link has been sent." }
```

#### `POST /api/Account/reset-password`

الجسم: `ResetPasswordDTO`. الإجراء يفك ترميز `token` عبر URL قبل استدعاء Identity.

| الحقل | القواعد |
| --- | --- |
| `email` | مطلوب، صيغة بريد |
| `token` | مطلوب (الرمز من رابط البريد) |
| `newPassword` | مطلوب، 8–100 حرفاً (تعقيد Identity ما زال سارياً) |
| `confirmPassword` | يجب أن يساوي `newPassword` |

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

إعادة التعيين الناجحة تزيل القفل.

---

### 3.2 User — `api/User`

تفويض الصنف: `[Authorize(Roles = "Admin")]`. معرّفات المستخدم نصوص Identity وليست أرقاماً مشفّرة.

#### `GET /api/User`

200: مصفوفة من `UserDTO`. `role` رقم صحيح.

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

404 بجسم فارغ عندما لا يوجد المستخدم.

```json
{
  "id": "8f3c0a1e-1111-2222-3333-444455556666",
  "userName": "sara.hassan",
  "email": "sara@example.com",
  "role": 2
}
```

#### `POST /api/User`

الجسم: `CreateUserDTO`.

| الحقل | القواعد |
| --- | --- |
| `userName` | مطلوب، 3–50 |
| `email` | مطلوب، صيغة بريد |
| `role` | `1` Admin، `2` Client، `3` User |
| `password` | مطلوب، 8–100. Identity تتطلب أيضاً حرفاً كبيراً وصغيراً ورقماً |
| `passwordConfirmed` | يجب أن يساوي `password`. الإجراء يفحص ذلك أيضاً وقد يعيد النص `"Password and confirmation do not match."` |

```json
{
  "userName": "operator.one",
  "email": "operator@plaza.com",
  "role": 1,
  "password": "Operator@123",
  "passwordConfirmed": "Operator@123"
}
```

200: النص JSON `"User created successfully."`

400 عند فشل Identity: مصفوفة من `{ "code", "description" }`.

#### `PUT /api/User/{id}`

الجسم: `updateUserDTO`. لا يمكن تغيير `email`. كلمة مرور فارغة تُبقي كلمة المرور كما هي. كلمة المرور غير الفارغة يجب أن تطابق `passwordConfirmed` وتستوفي قواعد Identity. `role` يستبدل أدوار المستخدم الحالية.

```json
{
  "userName": "operator.one",
  "role": 3,
  "password": "Operator@456",
  "passwordConfirmed": "Operator@456"
}
```

204 عند النجاح. 404 `ProblemDetails` (`"User not found."`) عندما لا يوجد المعرّف. 400 بكائنات أخطاء Identity عندما تُرفض كلمة المرور.

#### `DELETE /api/User/{id}`

204 عند النجاح. 404 `ProblemDetails` عندما لا يوجد المستخدم.

---

### 3.3 File — `POST /api/upload`

التفويض: Admin. ليس JSON. `multipart/form-data`.

| الجزء | القواعد |
| --- | --- |
| الاستعلام `type` | اسم `PlazaInnType` أو رقمه. مطلوب عملياً. `None` (9) → 400 `"Invalid file type."` |
| النموذج `files` | ملف واحد أو أكثر. واحد على الأقل. انظر القسم 2.1 للامتدادات وحد 10 ميجابايت / 50 ميجابايت |

مثال (curl):

```powershell
curl -X POST "http://localhost:5141/api/upload?type=trip" `
  -H "Authorization: Bearer <token>" `
  -F "files=@C:\images\nile.jpg"
```

200: مصفوفة JSON من معرّفات الصور الخام.

```json
[12, 13]
```

جسم 400 نص JSON، مثل `"No files uploaded."` أو `"File 'menu.exe' has an unsupported extension. Allowed: .jpg, .jpeg, .png, .webp."`.

لا توجد نقطة لسرد الملفات أو جلب ملف أو حذفه.

---

### 3.4 Hotel — `api/Hotel`

عام: `GET` و `GET {encodedId}` و `GET lookup`. التعديلات: Admin.

#### `GET /api/Hotel`

لا مرشحات استعلام. 200: مصفوفة من `HotelDto`.

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

`insight` يُملأ عندما يوجد صف `InsightHotel` للفندق (`InsightHotelDto`، القسم 3.7).

#### `GET /api/Hotel/{encodedId}`

400 والجسم هو النص JSON `"Invalid hotel ID"` عندما لا يمكن فك الرمز. 404 بجسم فارغ عندما لا يوجد الفندق. 200: كائن `HotelDto` واحد (نفس شكل العنصر أعلاه).

#### `GET /api/Hotel/lookup`

مجهول. 200:

```json
[
  { "id": "MTpQbGF6YUlubktleTIwMjU", "name": "Plaza Nile", "nameAr": "بلازا النيل" }
]
```

#### `POST /api/Hotel` و `PUT /api/Hotel/{encodedId}`

الجسم: `CreateHotelDto` (التعديل من نفس النوع، `UpdateHotelDTO`). `imageIds` و `logoId` أرقام صحيحة خام من `POST /api/upload`.

| الحقل | القواعد |
| --- | --- |
| `name` و `nameAr` | مطلوب، 100 كحد أقصى |
| `description` و `descriptionAr` | مطلوب، 500 كحد أقصى |
| `address` و `addressAr` | مطلوب، 200 كحد أقصى |
| `city` و `cityAr` | مطلوب، 50 كحد أقصى |
| `location` | مطلوب، رابط مطلق |
| `phone` | مطلوب، أرقام فقط، من 6 إلى 15 رقماً |
| `email` | مطلوب، صيغة بريد |
| `numOfAvailableRoomsToReserve` | عدد صحيح، الافتراضي 0. هذا مجمّع الغرف القابلة للحجز غير الأجنحة |
| `numOfAvailableSuitesToReserve` | عدد صحيح، الافتراضي 0. مجمّع الأجنحة |
| `rating` | رقم، الافتراضي 0 |
| `instagram` و `facebook` و `whatsApp` و `space` | نصوص اختيارية |
| `imageIds` | عدد صحيح واحد على الأقل |
| `logoId` | عدد صحيح. `0` يقبله التحقق |

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

`POST` يعيد **201** بجسم `HotelDto` وترويسة `Location` تشير إلى `GET /api/Hotel/{encodedId}`.

`PUT` يعيد **204**. معرّف مشفّر غير معروف → 400 نص `"Invalid hotel ID"`. فندق غير موجود → 404 بجسم فارغ.

#### `DELETE /api/Hotel/{encodedId}`

204. رمز غير صالح → 400 نص `"Invalid hotel ID"`. فندق مفقود → 404 بجسم فارغ.

---

### 3.5 Room — `api/Room`

عام: `GET` و `GET {encodedId}` و `GET hotel/{encodedId}`. التعديلات: Admin.

`RoomDTO` لا يتضمن `hotelId`. يتضمن `hotelName`. `type` هو `RoomType` كرقم صحيح.

#### `GET /api/Room`

200: مصفوفة من `RoomDTO`.

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

400 نص `"Invalid Room Id"`. 404 بجسم فارغ عند الغياب. 200: كائن `RoomDTO` واحد.

#### `GET /api/Room/hotel/{encodedId}`

غرف فندق واحد. 400 نص `"Invalid Hotel Id"`. فندق غير معروف أو فندق بلا غرف يعيد **200** و `[]` (وليس 404).

#### `POST /api/Room` و `PUT /api/Room/{encodedId}`

الجسم: `CreateRoomDTO` / `UpdateRoomDTO`.

| الحقل | القواعد |
| --- | --- |
| `hotelId` | مطلوب، معرّف فندق **مشفّر** |
| `name` و `nameAr` | مطلوبان |
| `type` | `0` Single، `1` Double، `2` Suite |
| `pricePerNight` | أكبر من 0 |
| `features` | خريطة من نص إلى bool. يمكن أن تكون `{}` |
| `extraBed` | Bool. الافتراضي false |
| `priceForExtraBed` | عدد عشري. يُستخدم فقط عندما يضبط الحجز `extraBed: true` |
| `maxSleep` | عدد صحيح. `0` يعطّل فحص سعة الضيوف |
| `imageIds` | معرّف صورة خام واحد على الأقل (`type=room`) |

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

`POST` يعيد **201** `RoomDTO` (يتضمن `hotelName` وروابط صور مطلقة). `PUT` يعيد **204**. رمز غرفة غير صالح في التعديل/الحذف → 400 نص `"Invalid Room ID"`. غرفة مفقودة → 404 بجسم فارغ.

#### `DELETE /api/Room/{encodedId}`

204 عند النجاح.

---

### 3.6 توفر الغرف وعرض السعر — `api/hotels`

`RoomAvailabilityController` بلا `[Authorize]`. الإجراءان عامّان. هذا المتحكم ليس متحكم CRUD للفندق (`api/Hotel`).

#### `POST /api/hotels/{encodedHotelId}/rooms`

الجسم: `RoomAvailabilityRequestDto`. يعرض كل غرف الفندق وعدد المتاح منها للإقامة. لا ينشئ حجزاً.

| الحقل | القواعد |
| --- | --- |
| `checkIn` و `checkOut` | المغادرة يجب أن تكون بعد 1–30 ليلة من الوصول |
| `roomsCount` | 1–20، الافتراضي 1. `isAvailable` صحيح عندما `availableRooms >= roomsCount` |

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

معرّف فندق مشفّر غير صالح → 400 `ProblemDetails` `"Invalid ID"`. فندق غير موجود → 404 `ProblemDetails` `"Hotel not found"`. إقامة مستحيلة → 400 `ProblemDetails` برسالة الإقامة.

#### `POST /api/hotels/calculate-price`

الجسم: `BookingQuoteRequestDto`. لا يحفظ شيئاً. نفس قواعد الإقامة لحجز الفندق، بما فيها سعة الضيوف والسرير الإضافي.

| الحقل | القواعد |
| --- | --- |
| `encodedHotelId` | مطلوب (يُتحقق منه؛ السعر نفسه يُحسب من الغرفة) |
| `encodedRoomId` | مطلوب |
| `roomsCount` | 1–20، الافتراضي 1 |
| `adults` | 1–50، الافتراضي 1 |
| `children` | 0–50، الافتراضي 0 |
| `checkIn` و `checkOut` | 1–30 ليلة |
| `extraBed` | الافتراضي false |

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

للغرفة النموذجية (2000 في الليلة، ليلتان، ضريبة 0.28) الإجمالي `5120.00`.

200:

```json
{
  "success": true,
  "data": { "totalPrice": 5120.00 }
}
```

---

### 3.7 InsightHotel — `api/InsightHotel`

صف insight واحد لكل فندق (نص المطعم والمقهى، والصور، وملفات قوائم الطعام PDF، وأعلام المرافق).

عام: `GET /api/InsightHotel/{encodedHotelId}`. الإنشاء والتعديل والحذف: Admin.

#### `POST /api/InsightHotel`

الجسم: `CreateInsightHotelDto`. لا سمات تحقق. `encodedHotelId` يُفك داخل الإجراء.

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

ارفع الصور بـ `type=restaurant` أو `type=cafe`، وملفات PDF بـ `type=restaurantMenu` أو `type=cafeMenu`.

جسم **201** هو **DTO الطلب** (قوائم معرّفات خام و `encodedHotelId`)، وليس `InsightHotelDto`. `Location` يشير إلى `GET /api/InsightHotel/{encodedHotelId}`.

رمز فندق غير صالح → 400 نص `"Invalid hotel ID"`. فندق غير موجود → 404 بجسم فارغ.

#### `GET /api/InsightHotel/{encodedHotelId}`

مجهول. 200 `InsightHotelDto`. خصائص الصور وPDF هي مصفوفات `FileDto` رغم الأسماء `restaurantImageIds` وما شابهها. روابط section-images النسبية لا تُستخدم هنا؛ الربط يبني روابط مطلقة.

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

400 نص `"Invalid hotel ID"`. 404 بجسم فارغ عندما لا يوجد صف insight للفندق.

نفس الكائن يُضمَّن كـ `insight` داخل `HotelDto` عندما يُحمَّل الفندق مع insight الخاص به.

#### `PUT /api/InsightHotel/{encodedHotelId}`

الجسم: `UpdateInsightHotelDto` (نفس خصائص الإنشاء، بما فيها `encodedHotelId`). معرّف المسار يختار الصف؛ `encodedHotelId` في الجسم لا يُستخدم للبحث عنه.

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

204. رمز غير صالح → 400 نص. insight مفقود → 404 بجسم فارغ.

#### `DELETE /api/InsightHotel/{encodedHotelId}`

204. رمز غير صالح → 400 نص `"Invalid hotel ID"`. insight مفقود → 404 بجسم فارغ.

---

### 3.8 Partner — `api/Partner`

كل إجراء يتطلب Admin. لا توجد قراءة عامة للشركاء. روابط الفنادق تستخدم معرّفات فنادق **مشفّرة**.

#### `GET /api/Partner`

200: مصفوفة من `PartnerDto`.

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

200: كائن `PartnerDto` واحد. رمز غير صالح → 400 نص `"Invalid hotel ID"` (الرسالة تقول hotel حتى لمعرّف شريك). شريك مفقود → 404 بجسم فارغ.

#### `GET /api/Partner/hotel/{encodedId}`

الشركاء المرتبطون بفندق واحد. 200: مصفوفة من `PartnerOnlyDto` (بلا `hotelsIds`). مجموعة روابط فارغة تعيد **200** `[]`.

```json
[
  {
    "id": "MTpQbGF6YUlubktleTIwMjU",
    "name": "Nile Travel",
    "email": "partners@niletravel.com"
  }
]
```

رمز فندق غير صالح → 400 نص `"Invalid hotel ID"`.

#### `POST /api/Partner`

الجسم: `CreatePartnerDto`. لا سمات تحقق. عناصر `hotelsIds` معرّفات فنادق مشفّرة وتُفك بـ `IdEncoder.DecodeId` (عنصر غير صالح → 400 `ProblemDetails` `"Invalid ID"`).

```json
{
  "name": "Nile Travel",
  "email": "partners@niletravel.com",
  "hotelsIds": ["MTpQbGF6YUlubktleTIwMjU"]
}
```

**201** بجسم فارغ (`Created()`).

#### `PUT /api/Partner/{encodedId}`

الجسم: `UpdatePartnerDto` (نفس الخصائص). يستبدل مجموعة روابط الفنادق بالفنادق الموجودة للمعرّفات المفكوكة.

```json
{
  "name": "Nile Travel",
  "email": "partners@niletravel.com",
  "hotelsIds": ["MTpQbGF6YUlubktleTIwMjU"]
}
```

204. رمز شريك غير صالح → 400 نص `"Invalid partner ID"`. شريك مفقود → 404 بجسم فارغ.

#### `DELETE /api/Partner/{encodedId}`

204. رمز غير صالح → 400 نص `"Invalid hotel ID"`. شريك مفقود → 404 بجسم فارغ.

#### `POST /api/Partner/broadcastEmail/hotel/{encodedId}`

ليس JSON. `multipart/form-data` (`SendEmailRequestDto`). يرسل بريداً واحداً لكل بريد شريك مميز مرتبط بالفندق. يُلحَق HTML توقيع الفندق بـ `body`. يعمل 5 إرسالات كحد أقصى في الوقت نفسه.

| الجزء | القواعد |
| --- | --- |
| النموذج `subject` | نص |
| النموذج `body` | نص. HTML مسموح؛ يُضم توقيع الفندق بعده |
| النموذج `attachments` | ملفات اختيارية، كرّر الحقل لأكثر من ملف |

```powershell
curl -X POST "http://localhost:5141/api/Partner/broadcastEmail/hotel/MTpQbGF6YUlubktleTIwMjU" `
  -H "Authorization: Bearer <token>" `
  -F "subject=Stop sales 12 Nov" `
  -F "body=<p>Please stop sales for 12 November.</p>" `
  -F "attachments=@C:\docs\notice.pdf"
```

200 عندما ينجح كل إرسال، أو عندما لا يوجد شركاء للفندق:

```json
{ "message": "Emails sent to all partners successfully", "sent": 2, "failed": 0 }
```

```json
{ "message": "No partners found for this hotel" }
```

207 عندما يرمي إرسال واحد على الأقل:

```json
{ "message": "Emails sent to 1 partner(s); 1 failed.", "sent": 1, "failed": 1 }
```

رمز فندق غير صالح → 400 نص `"Invalid hotel ID"`. فندق غير موجود → 400 نص `"hotelId Not Found"`.

---

### 3.9 Trip — `api/Trip`

قراءات عامة (الصفوف غير النشطة مخفية ما لم يكن المستدعي Admin). التعديلات: Admin.

كائن الاستعلام: `TripFilterDto`.

| الاستعلام | السلوك |
| --- | --- |
| `city` | تطابق Contains على `city` أو `cityAr` (100 كحد أقصى) |
| `search` | تطابق Contains على `name` أو `nameAr` (150 كحد أقصى) |
| `date` | نافذة الرحلة تتقاطع مع ذلك اليوم التقويمي. مفتوحة الطرف عندما تكون البداية أو النهاية null |
| `minPrice` و `maxPrice` | تُقارَن بـ `pricePerPerson`. الحد الأدنى لا يمكن أن يتجاوز الحد الأقصى |
| `isActive` | للمدير فقط. `true` أو الحذف مع رمز غير مدير يعيد الصفوف النشطة. `false` يعيد الصفوف غير النشطة للمدير. غير المدير لا يستطيع سرد الصفوف غير النشطة |

مثال: `GET /api/Trip?city=Aswan&search=Philae&date=2026-11-15&minPrice=100&maxPrice=5000&isActive=true`

#### `GET /api/Trip`

200: مصفوفة من `TripDto`.

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

مجهول (غير النشط مخفي لغير المدير). بلا مرشحات.

```json
[
  { "id": "MTpQbGF6YUlubktleTIwMjU", "name": "Philae Day Trip", "nameAr": "رحلة فيلة ليوم واحد" }
]
```

#### `GET /api/Trip/{encodedId}`

400 نص `"Invalid trip ID"`. 404 بجسم فارغ عند الغياب أو عند عنصر غير نشط لغير مدير. 200: كائن `TripDto` واحد.

#### `POST /api/Trip` و `PUT /api/Trip/{encodedId}`

الجسم: `CreateTripDto` / `UpdateTripDto`.

| الحقل | القواعد |
| --- | --- |
| `name` و `nameAr` | مطلوب، 150 كحد أقصى |
| `description` و `descriptionAr` | مطلوب، 4000 كحد أقصى |
| `city` و `cityAr` | مطلوب، 100 كحد أقصى |
| `itinerary` و `itineraryAr` | مطلوب، 8000 كحد أقصى |
| `durationDays` | 0–365 |
| `durationHours` | 0–8784 |
| | يجب أن يكون أحد الأيام أو الساعات أكبر من 0 |
| `pricePerPerson` و `pricePerChild` | أكبر من 0، حتى `999999999999.99` |
| `maxCapacity` | 1–100000 |
| `startDate` | اختياري. مطلوب عند تعيين `endDate` |
| `endDate` | اختياري. يجب أن يكون في تاريخ `startDate` أو بعده |
| `isActive` | الافتراضي true |
| `imageIds` | معرّف خام واحد على الأقل من `type=trip` |

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

`POST` يعيد **201** `TripDto`. `PUT` يعيد **204**. معرّف غير صالح → 400 نص `"Invalid trip ID"`. رحلة مفقودة عند التعديل/الحذف → 404 بجسم فارغ.

#### `DELETE /api/Trip/{encodedId}`

204. رحلة ما زالت مربوطة بباقة يفشل حذفها بـ **409** `ProblemDetails`: `"This trip is included in a package and cannot be deleted."`

---

### 3.10 Transport — `api/Transport`

نفس نمط التفويض في Trip.

كائن الاستعلام: `TransportFilterDto`.

| الاستعلام | السلوك |
| --- | --- |
| `search` | تطابق Contains على `name` أو `nameAr` (150 كحد أقصى) |
| `vehicleType` | تطابق Contains على `vehicleType` أو `vehicleTypeAr` (100 كحد أقصى) |
| `pickupLocation` | تطابق Contains على نقطة الالتقاط بالإنجليزية أو العربية (250 كحد أقصى) |
| `dropoffLocation` | تطابق Contains على نقطة الإنزال بالإنجليزية أو العربية (250 كحد أقصى) |
| `passengers` | 1–1000. يُبقي المركبات التي `maxPassengers` لها لا يقل عن هذا العدد |
| `minPrice` و `maxPrice` | تُقارَن بـ `pricePerTrip` |
| `isActive` | نفس قاعدة المدير في الرحلات |

مثال: `GET /api/Transport?search=Sedan&vehicleType=Sedan&pickupLocation=Aswan&dropoffLocation=Philae&passengers=3&minPrice=100&maxPrice=3000&isActive=true`

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

400 نص `"Invalid transport ID"`. 404 عند الغياب أو عنصر غير نشط لغير مدير. 200: كائن `TransportDto` واحد.

#### `POST /api/Transport` و `PUT /api/Transport/{encodedId}`

| الحقل | القواعد |
| --- | --- |
| `name` و `nameAr` | مطلوب، 150 كحد أقصى |
| `description` و `descriptionAr` | مطلوب، 4000 كحد أقصى |
| `vehicleType` و `vehicleTypeAr` | مطلوب، 100 كحد أقصى |
| `pickupLocation` و `pickupLocationAr` | مطلوب، 250 كحد أقصى |
| `dropoffLocation` و `dropoffLocationAr` | مطلوب، 250 كحد أقصى |
| `maxPassengers` | 1–1000 |
| `maxLuggage` | 0–1000 |
| `pricePerTrip` | أكبر من 0 |
| `availableVehicles` | 1–100000 |
| `isActive` | الافتراضي true |
| `imageIds` | معرّف خام واحد على الأقل من `type=transport` |

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

`POST` **201** `TransportDto`. `PUT` **204**. معرّف غير صالح → 400 نص `"Invalid transport ID"`.

#### `DELETE /api/Transport/{encodedId}`

204، أو 409 عندما ما زالت باقة تتضمنه: `"This transport is included in a package and cannot be deleted."`

---

### 3.11 Event — `api/Event`

كائن الاستعلام: `EventFilterDto`.

| الاستعلام | السلوك |
| --- | --- |
| `search` | تطابق Contains على `name` أو `nameAr` (150 كحد أقصى) |
| `location` | تطابق Contains على `location` أو `locationAr` (250 كحد أقصى) |
| `date` | اليوم التقويمي يتقاطع مع `[eventDate, endDate]` (`endDate` يساوي `eventDate` عند غيابه) |
| `minPrice` و `maxPrice` | تُقارَن بـ `ticketPrice` |
| `isActive` | نفس قاعدة المدير في الرحلات |

مثال: `GET /api/Event?search=Sound%20and%20Light&location=Philae&date=2026-11-20&minPrice=100&maxPrice=2000&isActive=true`

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

400 نص `"Invalid event ID"`. 404 عند الغياب أو عنصر غير نشط لغير مدير. 200: كائن `EventDto` واحد.

#### `POST /api/Event` و `PUT /api/Event/{encodedId}`

| الحقل | القواعد |
| --- | --- |
| `name` و `nameAr` | مطلوب، 150 كحد أقصى |
| `description` و `descriptionAr` | مطلوب، 4000 كحد أقصى |
| `location` و `locationAr` | مطلوب، 250 كحد أقصى |
| `eventDate` | مطلوب |
| `endDate` | اختياري. يجب أن يكون في تاريخ `eventDate` أو بعده |
| `ticketPrice` | أكبر من 0 |
| `totalTickets` | 1–1000000 |
| `isActive` | الافتراضي true |
| `imageIds` | معرّف خام واحد على الأقل من `type=event` |

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

`POST` **201** `EventDto`. `PUT` **204**. معرّف غير صالح → 400 نص `"Invalid event ID"`.

#### `DELETE /api/Event/{encodedId}`

204، أو 409: `"This event is included in a package and cannot be deleted."`

---

### 3.12 Package — `api/Package`

كائن الاستعلام: `PackageFilterDto`.

| الاستعلام | السلوك |
| --- | --- |
| `search` | تطابق Contains على `name` أو `nameAr` (150 كحد أقصى) |
| `date` | نافذة الباقة تتقاطع مع ذلك اليوم. مفتوحة الطرف عندما تكون البداية أو النهاية null |
| `minPrice` و `maxPrice` | تُقارَن بـ `packagePrice` (سعر الباقة المقطوع، قبل الضريبة) |
| `durationDays` | تطابق تام، 1–365 |
| `includesHotel` | `true` = لها فندق؛ `false` = بلا فندق؛ الحذف = الاثنان |
| `isActive` | نفس قاعدة المدير في الرحلات |

مثال: `GET /api/Package?search=Aswan&date=2026-11-20&minPrice=1000&maxPrice=20000&durationDays=3&includesHotel=true&isActive=true`

#### `GET /api/Package`

`trips` و `transports` و `events` من نوع `PackageComponentDto` (`id` و `name` و `nameAr` فقط). معرّفا الفندق والغرفة في الاستجابة مشفّران.

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

400 نص `"Invalid package ID"`. 404 عند الغياب أو باقة غير نشطة لغير مدير. 200: كائن `PackageDto` واحد.

#### `POST /api/Package` و `PUT /api/Package/{encodedId}`

الجسم: `CreatePackageDto` / `UpdatePackageDto`. معرّفات التكوين **مشفّرة**. `imageIds` خام.

| الحقل | القواعد |
| --- | --- |
| `name` و `nameAr` | مطلوب، 150 كحد أقصى |
| `description` و `descriptionAr` | مطلوب، 4000 كحد أقصى |
| `durationDays` | 1–365 |
| `packagePrice` | أكبر من 0. هذا السعر المقطوع لكل باقة، قبل ضريبة 28% |
| `maxPackagesAvailable` | 1–100000 |
| `startDate` و `endDate` | اختياريان. النهاية تتطلب بداية. النهاية يجب أن تكون في تاريخ البداية أو بعده |
| `isActive` | الافتراضي true |
| `imageIds` | معرّف خام واحد على الأقل من `type=package` |
| `hotelId` و `roomId` و `nightsCount` | الثلاثة معاً أو لا شيء. `nightsCount` من 1 إلى 365 عند التعيين |
| `tripIds` و `transportIds` و `eventIds` | مصفوفات معرّفات مشفّرة. المصفوفات الفارغة صالحة. العناصر الفارغة تُتجاوز |
| | عنصر واحد على الأقل إجمالاً (ثلاثية الفندق و/أو أي معرّف مكوّن) |

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

الباقة بلا فندق تحذف `hotelId` و `roomId` و `nightsCount` (أو ترسلها `null`) ويجب أن تتضمن معرّف مكوّن واحداً على الأقل.

`POST` **201** `PackageDto` (المكوّنات موسَّعة). `PUT` **204**. رمز باقة غير صالح → 400 نص `"Invalid package ID"`. رمز مكوّن غير صالح → 400 نص `"Invalid package item ID"` أو `"Invalid hotel ID"` أو `"Invalid room ID"`. فندق أو غرفة أو رحلة أو نقل أو فعالية غير موجودة → 404 `ProblemDetails`. غرفة على فندق غير فندقها → 400 `ProblemDetails` `"The selected room does not belong to the selected hotel."`

#### `DELETE /api/Package/{encodedId}`

204، أو 409: `"This package cannot be deleted because other records depend on it."` (حجز يشير إليها).

---

### 3.13 Booking — `api/bookings`

الإجراءات الأربعة تتطلب `[Authorize]` (أي دور). الإنشاء يستخدم حد `booking-create`: **5 طلبات كل 10 دقائق لكل معرّف مستخدم**.

`BookingResponseDto.status` و `bookingType` في **الاستجابات** نصوص (`"Pending"` و `"Hotel"`)، لأن الـ Mapper يستدعي `ToString()`. `bookingType` في الطلب يبقى رقماً صحيحاً.

حقول العميل في كل إنشاء:

| الحقل | القواعد |
| --- | --- |
| `customerName` | مطلوب، 2–100 حرف |
| `customerEmail` | مطلوب، صيغة بريد. يُستبدل ببريد JWT عندما توجد مطالبة `Email` |
| `customerPhone` | مطلوب، صيغة هاتف، 32 حرفاً كحد أقصى |

القيم الافتراضية المشتركة: `roomsCount` يساوي 1، و `adults` يساوي 1، و `children` يساوي 0، و `extraBed` يساوي false، و `quantity` يساوي 1، و `bookingType` يساوي 0.

#### `POST /api/bookings`

الأجسام الخمسة الجاهزة للإرسال في القسم 4. النجاح **200**:

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

في الرحلة يكون `itemName` اسم الرحلة و `bookingType` يساوي `"Trip"`. النقل والفعالية والباقة تتبع نفس النمط. حقول الفندق تكون null عندما لا تتضمن الباقة إقامة.

401 عندما لا يحتوي الرمز على مطالبة `Id`:

```json
{ "success": false, "message": "User id claim is missing in token." }
```

#### `GET /api/bookings/my`

حجوزات المستدعي فقط، الأحدث في `createdAt` أولاً.

| الاستعلام | المعنى |
| --- | --- |
| `hotelId` | المفتاح الأساسي **الصحيح** للفندق، وليس معرّفاً مشفّراً |
| `from` | يُبقي الصفوف التي `checkIn` **أو** `serviceDate` فيها في هذا الوقت أو بعده |
| `to` | يُبقي الصفوف التي `checkOut` **أو** `serviceDate` فيها في هذا الوقت أو قبله |
| `status` | `Pending` أو `Confirmed` أو `Cancelled`، أو `0` أو `1` أو `2` |
| `bookingType` | `Hotel` أو `Trip` أو `Transport` أو `Event` أو `Package`، أو `0`–`4` |

مثال: `GET /api/bookings/my?hotelId=1&from=2026-11-01&to=2026-11-30&status=Pending&bookingType=Hotel`

200:

```json
{ "success": true, "data": [ /* BookingResponseDto، نفس شكل الإنشاء */ ] }
```

#### `GET /api/bookings/{encodedId}`

المالك أو المدير. 200 `{ "success": true, "data": { /* BookingResponseDto */ } }`. حجز مستخدم آخر → 403 `ProblemDetails` `"You do not have access to this booking."` حجز مفقود → 404 `"Booking not found"`. رمز تالف → 400 `"Invalid ID"`.

#### `POST /api/bookings/{encodedId}/cancel`

بلا جسم. يمكن للمالك إلغاء `Pending`. يمكن للمدير أيضاً إلغاء `Confirmed`. 200 يعيد الحجز مع `"status": "Cancelled"`. الملغى أصلاً → 200 بنفس الحجز الملغى. مؤكد + غير مدير → 409 `"Paid bookings can only be cancelled by an administrator."`

---

### 3.14 AdminBookings — `GET /api/admin/bookings`

التفويض: Admin. نفس معاملات الاستعلام في `GET /api/bookings/my` (`hotelId` رقم صحيح خام، و `from` و `to` و `status` و `bookingType`)، عبر كل المستخدمين.

مثال: `GET /api/admin/bookings?status=Confirmed&bookingType=Package`

200:

```json
{ "success": true, "data": [ /* BookingResponseDto */ ] }
```

لا يوجد مسار إنشاء أو تعديل أو حذف للمدير. المديرون يلغون عبر `POST /api/bookings/{encodedId}/cancel`.

---

### 3.15 Payment و Webhook

#### `POST /api/Payment/initiate` و `POST /api/Payment/test`

السمتان تصلان إلى نفس الإجراء. التفويض: أي مستخدم مصادق (المالك أو المدير). الجسم: `PaymentRequestDto`.

| الحقل | القواعد |
| --- | --- |
| `encodedBookingId` | مطلوب |
| `amountCents` | اختياري و**يُتجاهل** |
| `currency` | اختياري و**يُتجاهل** |
| `items` | اختياري و**يُتجاهل**. الشكل إن أُرسل: `{ "name", "amountCents", "quantity", "description" }` (حقول وليست خصائص؛ لا تُستخدم) |
| `billingData.name` | اختياري. يرجع إلى اسم عميل الحجز |
| `billingData.email` | اختياري. يرجع إلى بريد عميل الحجز |
| `billingData.phone` | اختياري. يرجع إلى هاتف الحجز أو `""` |

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

401 `{ "error": "User id claim is missing in token." }`. 403 إذا كان الحجز لمستخدم آخر. 404 `"Booking not found"`. 409 إذا لم تكن الحالة `Pending` أو انتهت مهلة الحجز. 502 `ProblemDetails` بعنوان `"Payment gateway error"` عندما لا تستطيع Paymob إصدار رمز أو رابط (`PaymentGatewayException`).

#### `GET /api/Payment/check-order/{paymobOrderId}`

`paymobOrderId` هو العدد الطويل المعاد كـ `paymobOrderId`، وليس معرّف الحجز المشفّر. التفويض: المالك أو المدير.

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

`booking_confirmed` تكون false عندما لا تعلّم Paymob الطلب مدفوعاً، أو عندما لا يتطابق المبلغ، أو عندما خسر دفع متأخر المخزون. يمكن أن تبقى `is_paid` صحيحة في حالة اختلاف المبلغ. 404 `"No booking found for this payment."` 403 لحجز لا يملكه المستدعي. 502 عندما يتعذر الاستعلام من Paymob.

#### `POST /api/Webhook/callback`

مجهول. هذا هو webhook معاملة Paymob. الاستعلام: `hmac` (مطلوب؛ الغياب أو عدم الصلاحية → **401** بجسم فارغ).

يقرأ الإجراء كائن JSON خاماً. لا يربط `PaymobCallbackDto`. الشكل المتوقع:

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

| النتيجة | الاستجابة |
| --- | --- |
| الجسم ليس كائناً، أو `obj` مفقود | 400 فارغ |
| `type` موجود وليس `TRANSACTION` | 200 فارغ، يُتجاهل |
| HMAC مفقود أو غير صالح | 401 فارغ |
| `success` ليس true، أو `pending` يساوي true | 200 فارغ، يُتجاهل |
| `order.id` أو `amount_cents` مفقود | 400 فارغ |
| فشل استدعاء حالة Paymob | 400 فارغ |
| طُولب الطلب المدفوع (أُكّد، أو كان مؤكداً، أو بقي دون تغيير بعد اختلاف) | 200 فارغ |

اضبط رابط استدعاء Paymob على `https://<public-host>/api/Webhook/callback`. يجب أن تستطيع Paymob الوصول إلى ذلك المضيف. `localhost` لن يستقبل الـ webhook ما لم تستخدم نفقاً. استخدم `check-order` أثناء الاختبار المحلي بعد الدفع في iframe الاختبار.

---

### 3.16 CMS

#### AboutUs — `api/AboutUs`

`POST` معلّق في المتحكم وليس مساراً.

##### `GET /api/AboutUs`

مجهول. 200: مصفوفة من `AboutUsDto`. خصائص الصور كائنات `FileDto` (أسماء الخصائص ما زالت تنتهي بـ `ImageId`). `id` هو المفتاح الأساسي الصحيح.

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

الإملاء `tittle` / `subTittle` هو اسم الخاصية في الـ DTO.

##### `PUT /api/AboutUs`

التفويض: Admin. الجسم: `UpdateAboutUsDto`. يعدّل الصف الأول، أو يدرج صفاً إذا كان الجدول فارغاً. معرّفات الصور أرقام صحيحة خام من `type=aboutUs`.

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

204 بجسم فارغ.

#### ContactUs — `api/ContactUs`

##### `GET /api/ContactUs`

مجهول. يعيد **كائناً واحداً**، وليس مصفوفة. إذا لم يوجد صف، الجسم `ContactUsDto` فارغ (`workingHours: []`). `day` هو `DayOfWeek` كرقم صحيح. `openTime` / `closeTime` نصوص `TimeOnly` بالشكل `HH:mm:ss`.

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

التفويض: Admin. الجسم: `UpdateContactUsDto`. ينشئ الصف إن لم يوجد. لكل عنصر في `workingHours` يُحدَّث الصف الذي له نفس `day`، وإلا يُضاف صف. الأيام المحذوفة من المصفوفة تبقى كما هي. أرسل `workingHours` كمصفوفة (يمكن أن تكون فارغة). `workingHours` بقيمة null يرمي استثناءً ويصبح **500**.

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

#### HomeSlider — `api/HomeSlider`

`id` رقم صحيح خام. القراءات مجهولة. الإنشاء والتعديل والحذف تتطلب Admin.

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

الجسم: `CreateHomeSliderDto`. `imagesId` قائمة المعرّفات الخام من `type=homeSlider`. لا سمات تحقق.

```json
{
  "tittle": "Winter on the Nile",
  "subTittle": "Hotels, trips, and packages",
  "buttonText": "Explore",
  "buttonLink": "/packages",
  "imagesId": [91]
}
```

200: النص JSON `"HomeSliders have been added Succesfully"`.

##### `GET /api/HomeSlider/{id}`

`id` عدد صحيح. 200 `HomeSliderDto`. المعرّف المفقود يعيد أيضاً **200** بجسم null (الخدمة تربط كياناً null).

##### `PUT /api/HomeSlider/{id}`

الجسم: `UpdateHomeSliderDto` (نفس الخصائص). 204 عندما يوجد الصف. المعرّف المفقود يعيد **204** أيضاً ولا يغيّر شيئاً.

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

204 سواء وُجد الصف أم لا.

#### SectionImages — `api/section-images`

##### `GET /api/section-images?type={PlazaInnType}`

مجهول. يعيد كل صف `Image` الذي يساوي `Type` فيه قيمة الاستعلام، كـ `FileDto` مع `url` **النسبي المخزَّن**.

`GET /api/section-images?type=aboutUs`

```json
[
  { "id": 81, "url": "images/aboutUs/81.jpg" }
]
```

إذا حُذف `type`، يرتبط الـ enum غير القابل للإلغاء بقيمة `hotel` (0).

##### `PUT /api/section-images?type={PlazaInnType}`

التفويض: Admin. الجسم: `UpdateSectionImagesDto`. كل صورة من هذا النوع حالياً تُضبط على `None`. ثم يُسنَد كل معرّف في `imagesId` إلى هذا النوع. هذا يعيد وسم صفوف `Image` الموجودة؛ ولا يرفع ملفات.

`PUT /api/section-images?type=homeSlider`

```json
{ "imagesId": [91, 92] }
```

204.

---

### 3.17 لوحة تحليلات المدير — `GET /api/AdminDashboard/summary`

التفويض: Admin (`[Authorize(Roles = "Admin")]`). حمولة واحدة للقراءة فقط لصفحة المدير في Angular. الجسم هو `AdminDashboardSummaryDTO` نفسه، وليس مغلوفاً في `{ "success": true, "data": ... }`.

```http
GET /api/AdminDashboard/summary
GET /api/AdminDashboard/summary?fromDate=2026-01-01&toDate=2026-10-28
Authorization: Bearer <admin token>
```

| الاستعلام | المعنى |
| --- | --- |
| `fromDate` | اختياري. بداية شاملة. القيمة التاريخية فقط مثل `2026-01-01` هي منتصف الليل UTC في ذلك اليوم. |
| `toDate` | اختياري. نهاية شاملة. القيمة التاريخية فقط تضم اليوم كاملاً (`toDate=2026-10-28` يُبقي الحجوزات حتى `2026-10-28T23:59:59.9999999Z`). القيمة التي تحمل وقتاً هي لحظة شاملة. |

أرسِل `yyyy-MM-dd` أو لحظة ISO-8601 بتوقيت UTC. القيمة بلا إزاحة تُعامَل كـ UTC ولا يزيحها توقيت الخادم. إذا كان `fromDate` بعد `toDate` فالاستجابة **400** من نوع `ProblemDetails` و `detail` هو `"fromDate must be on or before toDate."` JWT مفقود أو غير صالح: **401** بجسم فارغ. رمز صالح لغير المدير: **403** بجسم فارغ.

تاريخ النشاط للحجز هو `serviceDate` عندما يكون العمود موجوداً (الرحلات والنقل والفعاليات والباقات المؤرخة). حجوزات الفندق بلا `serviceDate`، لذلك يستخدم المرشح `createdAt`. `totalUsers` و `catalogCounts` لقطة حالية ويتجاهلان مرشح التاريخ. `lowCapacityAlerts` مخزون حالي ويتجاهل مرشح التاريخ أيضاً.

عند حذف التاريخين، مؤشرات الأداء والإيراد والأفضل أداءً والحجوزات الأخيرة تكون لكل الوقت، و `monthlyTrends` هو شهر UTC الحالي مع الـ 11 شهراً السابقة (الأشهر الفارغة تُملأ بأصفار). عند إرسال `toDate` فقط، المؤشرات تضم كل حجز حتى ذلك التاريخ والرسم هو 12 شهراً تنتهي عند `toDate`. عند إرسال `fromDate` فقط، الرسم يمتد من ذلك الشهر حتى الشهر الحالي، أو الـ 12 شهراً التالية إذا كان `fromDate` في المستقبل. المدى الأطول من 36 شهراً يُبقي أحدث 36 شهراً في الرسم؛ مجاميع المؤشرات تبقى على المدى الكامل.

المبالغ تُقرَّب نصفها بعيداً عن الصفر إلى منزلتين. لا توجد حالة `Completed` ولا عمود `TaxAmount`. إيراد التأكيد هو مجموع `totalPrice` لحجوزات `Confirmed`. الضريبة مشتقة من الإجمالي الشامل للضريبة ومن `Booking:TaxRate` (الافتراضي `0.28`): `totalPrice * TaxRate / (1 + TaxRate)`. `revenueSharePercentage` هي إيراد النوع المؤكد مقسوماً على `kpis.totalConfirmedRevenue`. النسب تُقرَّب إلى منزلتين، وتُعدَّل أكبر شريحة حتى يصبح مجموع الحصص الخمس `100.00` عندما يكون الإيراد موجباً.

`pendingBookings` يعد صفوف `Pending` التي ما زالت داخل مهلة الدفع (`createdAt` ضمن `Booking:PendingExpiryMinutes`، الافتراضي 30). `expiredBookings` يعد صفوف `Pending` الأقدم من هذه المهلة. مهمة التنظيف تحوّل هذه الصفوف بعدها إلى `Cancelled`، لذلك `expiredBookings` هو الطابور الذي لم يُمسح بعد، وليس تاريخ كل مهلة انتهت سابقاً. `pendingRevenue` يجمع `totalPrice` لكل صف `Pending`، بما فيها المهل التي لم تلغِها المهمة بعد. `cancelledBookings` هو كل صف `Cancelled`.

`catalogCounts.*.active` يستخدم `isActive` للرحلات والنقل والفعاليات والباقات. الفنادق والغرف بلا علامة `isActive`، لذلك `active` يساوي `total`.

`revenueByServiceType` يحتوي دائماً هذه الكائنات الخمسة بهذا الترتيب: Hotel `0` و Trip `1` و Transport `2` و Event `3` و Package `4`. `bookingType` هو العدد الصحيح. `bookingTypeName` هو الاسم.

`topPerformers` يعرض خمسة على الأكثر من الفنادق والرحلات والنقل والفعاليات والباقات، مرتبة بإيراد التأكيد ثم بعدد الحجوزات. `bookingsCount` يضم كل الحالات داخل النافذة. `totalRevenue` للمؤكد فقط. `encodedId` من `IdEncoder` (نفس الرمز الذي تستخدمه واجهات الكتالوج). `topDestinations` هو أفضل خمس مواقع بعدد الحجوزات: مدينة الفندق، ومدينة الرحلة، وموقع الفعالية، ونقطة انطلاق النقل، ومدينة فندق الباقة المرتبط. المواقع تُقص وتُدمج دون حساسية لحالة الأحرف.

`operationalInsights.recentBookings` هو أحدث 10 حجوزات في النافذة المختارة (`createdAt` تنازلياً). `bookingReference` هو `paymentReference`، وإلا `merchantOrderId`، وإلا نفس `encodedId`. `status` هو `Pending` أو `Confirmed` أو `Cancelled` أو `Expired`. `Expired` قيمة عرض فقط لمهلة `Pending` تجاوزت نافذة الدفع. لا ترسلها إلى `GET /api/admin/bookings?status=`. `paymentStatus` هو `Paid` أو `Unpaid` من `isPaid`. `itemName` للفندق هو `"{hotel} - {room}"` عندما يوجد الاسمان.

`lowCapacityAlerts` ينبّه عندما يتبقى من المخزون `<= 5`. الفعاليات تستخدم `totalTickets` ناقص `quantity` للتذاكر. الباقات تستخدم `maxPackagesAvailable` ناقص `quantity`. الرحلات تستخدم `maxCapacity` ناقص `adults + children` لكل تاريخ خدمة من اليوم فصاعداً. الحجوزات المؤكدة والمعلقة التي ما زالت داخل المهلة تستهلك المخزون. الملغاة والمهل المنتهية لا تستهلكه. الفعاليات والباقات والرحلات النشطة التي سقفها أصلاً `<= 5` وليس لها حجز مستهلك تُدرج مع `booked: 0`. العناصر الماضية (`endDate` قبل اليوم) تُستبعد. `serviceDate` هو تاريخ الفعالية أو بداية الباقة أو يوم الرحلة. يكون null لرحلة سقفها اليومي منخفض وليس لها يوم محجوز قادم. `itemType` هو `Event` أو `Package` أو `Trip`. القائمة بحد أقصى 20 صفاً، الأقل تبقياً أولاً. استخدم `encodedId` مع GET الكتالوج المقابل.

`monthlyTrends[].periodLabel` بالشكل `yyyy-MM`. الأشهر بلا حجوزات تُدرج بأصفار.

200 (الرسم مختصر لشهرين؛ الاستدعاء بلا تاريخ يعيد 12 شهراً):

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

كل قائمة في الأفضل أداءً تحتوي حتى خمسة كائنات. العينة تعرض واحداً. استدعِ GET الكتالوج بـ `encodedId` (`GET /api/Hotel/{encodedId}`، وما يقابلها لـ Trip و Transport و Event و Package).

---

## 4. 5 Ready-to-Test Booking Payloads / أمثلة عملية جاهزة للحجز للـ 5 أنواع

الاستدعاءات الخمسة كلها:

`POST /api/bookings`  
`Authorization: Bearer <token>`  
Content-Type: `application/json`

استبدل المعرّفات المشفّرة بقيم من الـ API. `bookingType` يجب أن يكون رقماً صحيحاً (أو يُحذف للفندق). التواريخ أدناه بعد تاريخ الدليل (25 أكتوبر 2026) وداخل نوافذ الكتالوج النموذجية من القسم 3.

بريد المستخدم المسجّل دخوله يستبدل `customerEmail` عندما يحتوي الرمز على مطالبة `Email`. `totalPrice` في الاستجابة يتضمن ضريبة 28%. هذه الطلبات لا تتضمنها.

### 4.1 حجز غرفة فندق

`bookingType` بقيمة `0` هو `Hotel`. احذف الخاصية لنفس النتيجة. المغادرة بعد ليلتين من الوصول. مع الغرفة النموذجية (2000 / ليلة، بلا سرير إضافي) إجمالي الخادم **5120.00**.

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

يجب أن يكون لدى الفندق `numOfAvailableRoomsToReserve` (أو الأجنحة عندما يكون `type` يساوي `2`) أكبر من الحجوزات النشطة المتداخلة. `roomsCount` يُخزَّن كـ `quantity` وكـ `roomsBooked`.

### 4.2 حجز رحلة مستقلة

`bookingType` بقيمة `1`. `serviceDate` مطلوب. عدد الضيوف هو `adults + children`، ويجب أن يتسع في `maxCapacity` لذلك التاريخ. `quantity` في الطلب غير مستخدم. الأسعار النموذجية 1500 و 900 مع بالغين وطفل: الصافي 3900، ومع الضريبة **4992.00**.

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

### 4.3 حجز نقل مستقل

`bookingType` بقيمة `2`. `quantity` هو عدد المركبات. الركاب (`adults + children`) يجب أن يكونوا أقل من أو يساووا `quantity * maxPassengers`. سيارة واحدة بسعر 800: مع الضريبة **1024.00**.

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

### 4.4 حجز فعالية مستقلة

`bookingType` بقيمة `3`. `quantity` هو التذاكر. `serviceDate` اختياري؛ `2026-11-20` يقع على الفعالية النموذجية. تذكرتان بسعر 500: مع الضريبة **1280.00**.

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

### 4.5 حجز باقة مجمّعة

`bookingType` بقيمة `4`. `quantity` هو عدد الباقات. الباقة النموذجية لها غرفة ونافذة تاريخ، لذلك `serviceDate` مطلوب. تسجيل الوصول يصبح ذلك التاريخ وتسجيل المغادرة يصبح ذلك التاريخ زائد `nightsCount` (2). باقة واحدة بسعر 9000: مع الضريبة **11520.00**. يجب أن يتوفر في مجمّع الغرف غرفة لتلك الليالي، ويجب أن يسمح `maxPackagesAvailable` بباقة إضافية. سعات الرحلة والنقل والفعالية المكوّنة لا تنقص بهذا الاستدعاء.

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

### الترويسات

أرسل JSON كـ `Content-Type: application/json` باستثناء `POST /api/upload` و `POST /api/Partner/broadcastEmail/hotel/{encodedId}`، وهما `multipart/form-data`. لا تضبط `Content-Type` بنفسك في هذين الاستدعاءين؛ يجب أن يضيف المتصفح حد الـ boundary.

الطلبات المصادق عليها:

```http
Authorization: Bearer <token>
```

خزّن `token` من تسجيل الدخول وأرفقه عبر HTTP interceptor. عامل HTTP 401 على طلب محمي كرمز منتهٍ أو مفقود (العمر ساعة واحدة، وانحياز الساعة 0) وأعد المستخدم إلى تسجيل الدخول. لا ترسل الرمز إلى Paymob. افتح `paymentUrl` كتنقل متصفح عادي.

### CORS

اسم السياسة `PlazaCors`. بيانات الاعتماد (الكوكيز) غير مفعّلة. الطرق والترويسات غير مقيّدة. الأصول هي مصفوفة `AllowedOrigins`:

- `http://localhost:4200`
- `https://localhost:4200`
- `http://plazainn.runasp.net`

خادم تطوير Angular على أصل آخر يرفضه المتصفح قبل تنفيذ الإجراء. أضف ذلك الأصل إلى `AllowedOrigins` وأعد تشغيل الـ API.

`FrontendBaseUrl` (`http://localhost:4200`) يُستخدم فقط لبناء رابط إعادة تعيين كلمة المرور. ليس قائمة CORS.

### تحديد المعدل

| السياسة | النقاط | الحد | التقسيم |
| --- | --- | --- | --- |
| `auth` | `login` و `register` و `forgot-password` و `reset-password` | 10 طلبات / دقيقة واحدة | عنوان IP للعميل |
| `booking-create` | `POST /api/bookings` | 5 طلبات / 10 دقائق | `Id` في JWT، أو عنوان IP للعميل إذا غاب معرّف المستخدم |

حد قائمة الانتظار 0. الاستجابة المرفوضة هي **429** `application/problem+json`، وعندما يوفّر المحدّد نافذة تتضمن الاستجابة `Retry-After` (ثوانٍ، عدد صحيح، مقرب لأعلى).

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

اقرأ `Retry-After` قبل إعادة محاولة تسجيل الدخول أو إنشاء الحجز. بقية النقاط غير محدودة المعدل في هذا الـ API.

### معالجة الأخطاء الموحّدة

توجد عائلتان من أخطاء JSON. فرّع حسب `Content-Type` وحسب وجود `errors`.

**أ. مشكلة التحقق (HTTP 400)** من تحقق نموذج `[ApiController]`، أو `return BadRequest(ModelState)`. نوع المحتوى `application/problem+json`.

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

المفاتيح داخل `errors` هي أسماء خصائص C# (PascalCase).

**ب. `ProblemDetails` من وسيط الاستثناءات.** نوع المحتوى `application/problem+json`. حقول إضافية `traceId` و `message` (نفس نص `detail`).

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

| الحالة | متى يعيدها الوسيط أو الإجراء |
| --- | --- |
| **400** | `ArgumentException` (`title`: `"Invalid request"`). أمثلة: معرّف مشفّر غير صالح في مسارات الحجز (`"Invalid ID"`)، غرفة ليست في الفندق، إقامة أطول من 30 ليلة، وصول في الماضي، سرير إضافي غير متاح، ضيوف أكثر من اللازم، تاريخ خدمة خارج نافذة الكتالوج، ثلاثية الباقة ناقصة. وكذلك `BadRequest("...")` من المتحكمات، وهو **نص JSON** وليس هذا الكائن. وكذلك تحقق النموذج (الشكل أ). |
| **401** | فشل تسجيل الدخول: `{ "message": "..." }` (ليس `ProblemDetails`). غياب مطالبة `Id` في الحجوزات: `{ "success": false, "message": "User id claim is missing in token." }`. غياب مطالبة `Id` في الدفع: `{ "error": "User id claim is missing in token." }`. JWT مفقود أو غير صالح على مسارات `[Authorize]`: جسم فارغ مع `WWW-Authenticate: Bearer`. فشل HMAC في الـ webhook: 401 فارغ. |
| **403** | `ForbiddenException` (`title`: `"Forbidden"`، `detail`: `"You do not have access to this booking."`) عندما يقرأ مستخدم حجزاً لا يملكه أو يلغيه أو يدفعه أو يستعلم عنه. غير المدير الذي يستدعي مسار Admin برمز صالح: 403 فارغ من وسيط التفويض. |
| **404** | `NotFoundException` (`title`: `"Resource not found"`). الرسائل تشمل `"Room not found"` و `"Hotel not found"` و `"Trip not found"` و `"Transport not found"` و `"Event not found"` و `"Package not found"` و `"Booking not found"` و `"User not found."` و `"No booking found for this payment."` و `"One or more trips were not found."` (وما يقابلها للنقل والفعالية). بعض المتحكمات تعيد `NotFound()` **بجسم فارغ** بدلاً من ذلك (الفندق والغرفة والشريك وinsight وجلب الكتالوج بالمعرّف). عناصر الكتالوج غير النشطة لغير المديرين هي أيضاً 404 فارغ. |
| **409** | `ConflictException` (`title`: `"Conflict"`). أمثلة: لا غرف متبقية، رحلة/نقل/فعالية/باقة نفدت أو غير نشطة، الدفع للحجوزات المعلقة فقط، انتهت مهلة الحجز، حجز مدفوع ألغاه غير مدير، حذف كتالوج منعه مفتاح أجنبي. |
| **429** | رفض حد المعدل. انظر الجسم أعلاه. التزم بـ `Retry-After`. |
| **502** | `PaymentGatewayException` (`title`: `"Payment gateway error"`). فشل استدعاء رمز Paymob أو رابط الدفع أو حالة الطلب. `detail` جملة ثابتة مثل `"Failed to retrieve Paymob token."` الـ webhook يحوّل نفس الفشل إلى **400 فارغ**، وليس 502. |
| **500** | أي استثناء آخر. `title`: `"Internal server error"`. `detail` و `message`: `"An unexpected error occurred. Please try again later."` الاستثناء الحقيقي يُسجَّل على الخادم ولا يُعاد. |
| **207** | بث الشركاء فقط، عندما تفشل بعض الرسائل. الجسم `{ message, sent, failed }`، وليس `ProblemDetails`. |

أغلفة النجاح التي يجب معالجتها صراحة:

- الحجوزات والتوفر: `{ "success": true, "data": ... }`
- لوحة المدير `GET /api/AdminDashboard/summary`: كائن `AdminDashboardSummaryDTO` نفسه (بدون غلاف `success`)
- بدء الدفع: `{ paymentUrl, paymobOrderId, amountCents, currency }`
- فحص الطلب: حقول snake_case هي `payment_status` و `is_paid` و `amount_cents` و `booking_confirmed`
- كثير من كتابات المدير: **204** بجسم فارغ
- إنشاء المستخدم وإنشاء الـ Home Slider: **200** بنص JSON

قيم `url` للصور في DTO الكتالوج مطلقة ويمكن استخدامها مباشرة في `<img>`. `GET /api/section-images` يعيد مساراً نسبياً؛ أضف أمامه أصل الـ API إذا عرضت هذه الروابط.

---

## 6. Step-by-Step Manual Testing Script on Swagger / سيناريو اختبار عملي خطوة بخطوة

استخدم `http://localhost:5141/swagger` بعد القسم 1. احتفظ بدفتر للمعرّفات التي يعيدها كل خطوة. التواريخ تفترض أنك تختبر في 25 أكتوبر 2026 أو بعده وقبل تواريخ النهاية النموذجية.

1. **سجّل الدخول كمدير.** `POST /api/Account/login` مع `email` بالقيمة `admin@plaza.com` و `password` بالقيمة `Admin@123`. انسخ `token`. اضغط **Authorize**، الصق الرمز فقط، ثم فوّض. أيقونات القفل على عمليات المدير يجب أن تظهر مفوَّضة.

2. **ارفع صورة واحدة لكل نوع كتالوج.** استدعِ `POST /api/upload` أربع مرات (منتقي ملفات Swagger، حقل النموذج `files`):
   - `type=trip` → احفظ الرقم المعاد كـ `tripImageId`
   - `type=transport` → `transportImageId`
   - `type=event` → `eventImageId`
   - `type=package` → `packageImageId`  
   كل استجابة مصفوفة JSON مثل `[41]`. استخدم ملفاً حقيقياً `.jpg` أو `.jpeg` أو `.png` أو `.webp` أصغر من 10 ميجابايت.

3. **أنشئ رحلة.** `POST /api/Trip` بجسم القسم 3.9 و `"imageIds": [<tripImageId>]`. من جسم 201 انسخ `id` كـ `encodedTripId`. تأكد أن `GET /api/Trip/{encodedId}` يعيد نفس الرحلة.

4. **أنشئ نقلاً.** `POST /api/Transport` بجسم القسم 3.10 و `"imageIds": [<transportImageId>]`. انسخ `id` كـ `encodedTransportId`.

5. **أنشئ فعالية.** `POST /api/Event` بجسم القسم 3.11 (`eventDate` ما زال في المستقبل) و `"imageIds": [<eventImageId>]`. انسخ `id` كـ `encodedEventId`.

6. **اجمعها في باقة.** إذا أردت إقامة الفندق أيضاً، أنشئ فندقاً وغرفة أولاً (`POST /api/upload?type=hotel` ثم `POST /api/upload?type=logo` ثم `POST /api/Hotel` ثم `POST /api/upload?type=room` ثم `POST /api/Room`) ومرّر تلك المعرّفات المشفّرة كـ `hotelId` و `roomId` و `nightsCount`. وإلا احذف حقول الإقامة الثلاثة. `POST /api/Package` بجسم القسم 3.12 ومعرّف صورتك و:

```json
"tripIds": ["<encodedTripId>"],
"transportIds": ["<encodedTransportId>"],
"eventIds": ["<encodedEventId>"]
```

انسخ `id` من 201 كـ `encodedPackageId`. `GET /api/Package/{encodedId}` يجب أن يسرد المكوّنات الثلاثة.

7. **احجز الباقة بصلاحية المدير (رمز المدير كافٍ؛ الحجز `[Authorize]` وليس للمدير فقط).** `POST /api/bookings` بجسم القسم 4.5 و `encodedPackageId` الخاص بك. توقع 200، و `"bookingType": "Package"`، و `"status": "Pending"`، و `"isPaid": false`، و `totalPrice` يساوي `packagePrice * 1.28` للكمية 1. انسخ `data.encodedId` كـ `encodedBookingId`. 409 هنا يعني أن الباقة غير نشطة أو نفدت، أو أن تاريخ الخدمة خارج `startDate`/`endDate`، أو (عند إرفاق غرفة) أن مجمّع الغرف ممتلئ.

8. **ابدأ Paymob.** `POST /api/Payment/initiate` مع `{ "encodedBookingId": "<encodedBookingId>" }`. توقع 200 مع `paymentUrl` و `paymobOrderId` و `amountCents` و `"currency": "EGP"`. افتح `paymentUrl` في المتصفح وادفع ببطاقة اختبار Paymob. `amountCents` يجب أن يطابق `round(totalPrice * 100)` من الخطوة 7. الـ API يتجاهل أي مبلغ قد تضيفه إلى الجسم.

9. **أكّد الدفع.** إذا استطاعت Paymob الوصول إلى مضيفك، فإن استدعاءها هو `POST /api/Webhook/callback?hmac=...` وينتقل الحجز إلى `Confirmed` دون استدعاء Swagger. على localhost استدعِ `GET /api/Payment/check-order/{paymobOrderId}` بالرقم من الخطوة 8. طلب اختبار مدفوع يعيد `"is_paid": true` و `"booking_confirmed": true`. إذا تخطيت الدفع، انتظر 30 دقيقة: `ExpiredBookingsCleanupService` يضبط الحجز على `Cancelled` (المهمة تستيقظ كل 5 دقائق تقريباً)، وبدء الدفع لاحقاً يعيد 409.

10. **تحقق من قائمتي الحجوزات.** `GET /api/bookings/my?bookingType=Package` يعيد حجز المدير المسجّل دخوله، بما في ذلك `status` بقيمة `Confirmed` بعد الخطوة 9 أو `Pending` قبلها. `GET /api/admin/bookings?bookingType=Package` يعيد نفس الصف لأن المستدعي مدير. `GET /api/bookings/{encodedBookingId}` يعيد ذلك الحجز وحده. اختياري: `POST /api/bookings/{encodedBookingId}/cancel` مسموح لهذا المدير حتى بعد التأكيد؛ طلب `GET` التالي يظهر `"status": "Cancelled"`. الإلغاء لا يسترد مبلغ Paymob.
