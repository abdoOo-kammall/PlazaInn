using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using PlazaRepository;
using Shared.DTO.Dashboard;
using Shared.Enums;
using Shared.Options;
using Shared.Security;

namespace PlazaService.Dashboard
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private const int TopLimit = 5;
        private const int RecentLimit = 10;
        private const int AlertLimit = 20;
        private const int LowCapacityThreshold = 5;
        private const int DefaultTrendMonths = 12;
        private const int MaxTrendMonths = 36;

        private static readonly BookingType[] AllServiceTypes =
        {
            BookingType.Hotel,
            BookingType.Trip,
            BookingType.Transport,
            BookingType.Event,
            BookingType.Package
        };

        private readonly PlazaDbContext _db;
        private readonly BookingOptions _options;

        public AdminDashboardService(PlazaDbContext db, IOptions<BookingOptions> options)
        {
            _db = db;
            _options = options.Value;
        }

        public async Task<AdminDashboardSummaryDTO> GetDashboardSummaryAsync(
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            var range = NormalizeRange(fromDate, toDate);
            var filtered = BookingsInRange(range);
            var cutoff = DateTime.UtcNow.AddMinutes(-_options.PendingExpiryMinutes);
            var taxRate = _options.TaxRate <= 0m ? 0m : _options.TaxRate;
            var taxDivisor = 1m + taxRate;

            // One scoped DbContext cannot run queries in parallel.
            var kpis = await LoadKpiCountsAsync(filtered, cutoff, cancellationToken);
            kpis.TotalUsers = await _db.Users.AsNoTracking().CountAsync(cancellationToken);
            kpis.CatalogCounts = await LoadCatalogCountsAsync(cancellationToken);

            var breakdown = await LoadBreakdownAsync(filtered, taxRate, taxDivisor, cancellationToken);
            kpis.TotalConfirmedRevenue = breakdown.Sum(item => item.Revenue);
            kpis.TotalTaxCollected = breakdown.Sum(item => item.TaxAmount);

            return new AdminDashboardSummaryDTO
            {
                Kpis = kpis,
                RevenueByServiceType = breakdown,
                MonthlyTrends = await LoadTrendsAsync(filtered, range, cancellationToken),
                TopPerformers = await LoadTopPerformersAsync(filtered, cancellationToken),
                OperationalInsights = new RecentActivityAndAlertsDTO
                {
                    RecentBookings = await LoadRecentAsync(filtered, cutoff, cancellationToken),
                    LowCapacityAlerts = await LoadAlertsAsync(cutoff, cancellationToken)
                }
            };
        }

        private async Task<KpiSummaryDTO> LoadKpiCountsAsync(
            IQueryable<Booking> filtered,
            DateTime cutoff,
            CancellationToken ct)
        {
            var row = await filtered
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalBookings = g.Count(),
                    ConfirmedBookings = g.Count(b => b.Status == BookingStatus.Confirmed),
                    PendingBookings = g.Count(b => b.Status == BookingStatus.Pending && b.CreatedAt >= cutoff),
                    ExpiredBookings = g.Count(b => b.Status == BookingStatus.Pending && b.CreatedAt < cutoff),
                    CancelledBookings = g.Count(b => b.Status == BookingStatus.Cancelled),
                    PendingRevenue = g.Sum(b => b.Status == BookingStatus.Pending ? (decimal?)b.TotalPrice : 0m) ?? 0m
                })
                .FirstOrDefaultAsync(ct);

            if (row == null)
                return new KpiSummaryDTO();

            return new KpiSummaryDTO
            {
                TotalBookings = row.TotalBookings,
                ConfirmedBookings = row.ConfirmedBookings,
                PendingBookings = row.PendingBookings,
                ExpiredBookings = row.ExpiredBookings,
                CancelledBookings = row.CancelledBookings,
                PendingRevenue = RoundMoney(row.PendingRevenue)
            };
        }

        private async Task<CatalogCountsDTO> LoadCatalogCountsAsync(CancellationToken ct)
        {
            var hotels = await _db.Hotels.AsNoTracking().CountAsync(ct);
            var rooms = await _db.Rooms.AsNoTracking().CountAsync(ct);

            var trips = await _db.Trips.AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new { Total = g.Count(), Active = g.Count(t => t.IsActive) })
                .FirstOrDefaultAsync(ct);
            var transports = await _db.Transports.AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new { Total = g.Count(), Active = g.Count(t => t.IsActive) })
                .FirstOrDefaultAsync(ct);
            var events = await _db.Events.AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new { Total = g.Count(), Active = g.Count(e => e.IsActive) })
                .FirstOrDefaultAsync(ct);
            var packages = await _db.Packages.AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new { Total = g.Count(), Active = g.Count(p => p.IsActive) })
                .FirstOrDefaultAsync(ct);

            return new CatalogCountsDTO
            {
                Hotels = new CatalogCountDTO { Active = hotels, Total = hotels },
                Rooms = new CatalogCountDTO { Active = rooms, Total = rooms },
                Trips = ToCatalogCount(trips?.Active, trips?.Total),
                Transports = ToCatalogCount(transports?.Active, transports?.Total),
                Events = ToCatalogCount(events?.Active, events?.Total),
                Packages = ToCatalogCount(packages?.Active, packages?.Total)
            };
        }

        private async Task<List<ServiceTypeBreakdownDTO>> LoadBreakdownAsync(
            IQueryable<Booking> filtered,
            decimal taxRate,
            decimal taxDivisor,
            CancellationToken ct)
        {
            var rows = await filtered
                .GroupBy(b => b.BookingType)
                .Select(g => new BreakdownRow
                {
                    BookingType = g.Key,
                    BookingsCount = g.Count(),
                    ConfirmedCount = g.Count(b => b.Status == BookingStatus.Confirmed),
                    Revenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m,
                    TaxAmount = g.Sum(b => b.Status == BookingStatus.Confirmed
                        ? (decimal?)(b.TotalPrice * taxRate / taxDivisor)
                        : 0m) ?? 0m
                })
                .ToListAsync(ct);

            var byType = rows.ToDictionary(row => row.BookingType);
            var items = new List<ServiceTypeBreakdownDTO>(AllServiceTypes.Length);
            foreach (var type in AllServiceTypes)
            {
                byType.TryGetValue(type, out var row);
                items.Add(new ServiceTypeBreakdownDTO
                {
                    BookingType = (int)type,
                    BookingTypeName = type.ToString(),
                    BookingsCount = row?.BookingsCount ?? 0,
                    ConfirmedCount = row?.ConfirmedCount ?? 0,
                    Revenue = RoundMoney(row?.Revenue ?? 0m),
                    TaxAmount = RoundMoney(row?.TaxAmount ?? 0m)
                });
            }

            AssignShares(items);
            return items;
        }

        private async Task<List<MonthlyTrendDTO>> LoadTrendsAsync(
            IQueryable<Booking> filtered,
            DashboardRange range,
            CancellationToken ct)
        {
            var window = TrendWindow(range);
            var endExclusive = window.LastMonth.AddMonths(1);

            var rows = await filtered
                .Where(b => (b.ServiceDate ?? b.CreatedAt) >= window.FirstMonth
                            && (b.ServiceDate ?? b.CreatedAt) < endExclusive)
                .GroupBy(b => new
                {
                    Year = (b.ServiceDate ?? b.CreatedAt).Year,
                    Month = (b.ServiceDate ?? b.CreatedAt).Month
                })
                .Select(g => new TrendRow
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    BookingsCount = g.Count(),
                    ConfirmedRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                })
                .ToListAsync(ct);

            var lookup = rows.ToDictionary(row => (row.Year, row.Month));
            var result = new List<MonthlyTrendDTO>();
            for (var cursor = window.FirstMonth; cursor <= window.LastMonth; cursor = cursor.AddMonths(1))
            {
                lookup.TryGetValue((cursor.Year, cursor.Month), out var row);
                result.Add(new MonthlyTrendDTO
                {
                    Year = cursor.Year,
                    Month = cursor.Month,
                    PeriodLabel = $"{cursor.Year:D4}-{cursor.Month:D2}",
                    BookingsCount = row?.BookingsCount ?? 0,
                    ConfirmedRevenue = RoundMoney(row?.ConfirmedRevenue ?? 0m)
                });
            }

            return result;
        }

        private async Task<TopPerformersDTO> LoadTopPerformersAsync(
            IQueryable<Booking> filtered,
            CancellationToken ct)
        {
            return new TopPerformersDTO
            {
                Hotels = await RankHotelsAsync(filtered, ct),
                Trips = await RankTripsAsync(filtered, ct),
                Transports = await RankTransportsAsync(filtered, ct),
                Events = await RankEventsAsync(filtered, ct),
                Packages = await RankPackagesAsync(filtered, ct),
                TopDestinations = await LoadDestinationsAsync(filtered, ct)
            };
        }

        private Task<List<TopPerformingItemDTO>> RankHotelsAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            RankAsync(
                filtered
                    .Where(b => b.BookingType == BookingType.Hotel && b.HotelId != null)
                    .GroupBy(b => new { Id = b.HotelId!.Value, Name = b.Hotel!.Name })
                    .Select(g => new RankedRow
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        BookingsCount = g.Count(),
                        TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                    }),
                ct);

        private Task<List<TopPerformingItemDTO>> RankTripsAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            RankAsync(
                filtered
                    .Where(b => b.BookingType == BookingType.Trip && b.TripId != null)
                    .GroupBy(b => new { Id = b.TripId!.Value, Name = b.Trip!.Name })
                    .Select(g => new RankedRow
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        BookingsCount = g.Count(),
                        TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                    }),
                ct);

        private Task<List<TopPerformingItemDTO>> RankTransportsAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            RankAsync(
                filtered
                    .Where(b => b.BookingType == BookingType.Transport && b.TransportId != null)
                    .GroupBy(b => new { Id = b.TransportId!.Value, Name = b.Transport!.Name })
                    .Select(g => new RankedRow
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        BookingsCount = g.Count(),
                        TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                    }),
                ct);

        private Task<List<TopPerformingItemDTO>> RankEventsAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            RankAsync(
                filtered
                    .Where(b => b.BookingType == BookingType.Event && b.EventId != null)
                    .GroupBy(b => new { Id = b.EventId!.Value, Name = b.Event!.Name })
                    .Select(g => new RankedRow
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        BookingsCount = g.Count(),
                        TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                    }),
                ct);

        private Task<List<TopPerformingItemDTO>> RankPackagesAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            RankAsync(
                filtered
                    .Where(b => b.BookingType == BookingType.Package && b.PackageId != null)
                    .GroupBy(b => new { Id = b.PackageId!.Value, Name = b.Package!.Name })
                    .Select(g => new RankedRow
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        BookingsCount = g.Count(),
                        TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                    }),
                ct);

        private static async Task<List<TopPerformingItemDTO>> RankAsync(
            IQueryable<RankedRow> ranked,
            CancellationToken ct)
        {
            var rows = await ranked
                .OrderByDescending(row => row.TotalRevenue)
                .ThenByDescending(row => row.BookingsCount)
                .ThenBy(row => row.Name)
                .Take(TopLimit)
                .ToListAsync(ct);

            return rows.Select(row => new TopPerformingItemDTO
            {
                EncodedId = IdEncoder.EncodeId(row.Id),
                Name = row.Name ?? string.Empty,
                BookingsCount = row.BookingsCount,
                TotalRevenue = RoundMoney(row.TotalRevenue)
            }).ToList();
        }

        private async Task<List<TopDestinationDTO>> LoadDestinationsAsync(
            IQueryable<Booking> filtered,
            CancellationToken ct)
        {
            var totals = new Dictionary<string, LocationTotal>(StringComparer.OrdinalIgnoreCase);
            MergeLocations(totals, await HotelLocationsAsync(filtered, ct));
            MergeLocations(totals, await TripLocationsAsync(filtered, ct));
            MergeLocations(totals, await EventLocationsAsync(filtered, ct));
            MergeLocations(totals, await TransportLocationsAsync(filtered, ct));
            MergeLocations(totals, await PackageLocationsAsync(filtered, ct));

            return totals.Values
                .OrderByDescending(row => row.BookingsCount)
                .ThenByDescending(row => row.TotalRevenue)
                .ThenBy(row => row.Location)
                .Take(TopLimit)
                .Select(row => new TopDestinationDTO
                {
                    Location = row.Location,
                    BookingsCount = row.BookingsCount,
                    TotalRevenue = RoundMoney(row.TotalRevenue)
                })
                .ToList();
        }

        private static Task<List<LocationTotal>> HotelLocationsAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            filtered
                .Where(b => b.BookingType == BookingType.Hotel && b.HotelId != null && b.Hotel!.City.Trim() != "")
                .GroupBy(b => b.Hotel!.City.Trim())
                .Select(g => new LocationTotal
                {
                    Location = g.Key,
                    BookingsCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                })
                .ToListAsync(ct);

        private static Task<List<LocationTotal>> TripLocationsAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            filtered
                .Where(b => b.BookingType == BookingType.Trip && b.TripId != null && b.Trip!.City.Trim() != "")
                .GroupBy(b => b.Trip!.City.Trim())
                .Select(g => new LocationTotal
                {
                    Location = g.Key,
                    BookingsCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                })
                .ToListAsync(ct);

        private static Task<List<LocationTotal>> EventLocationsAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            filtered
                .Where(b => b.BookingType == BookingType.Event && b.EventId != null && b.Event!.Location.Trim() != "")
                .GroupBy(b => b.Event!.Location.Trim())
                .Select(g => new LocationTotal
                {
                    Location = g.Key,
                    BookingsCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                })
                .ToListAsync(ct);

        private static Task<List<LocationTotal>> TransportLocationsAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            filtered
                .Where(b => b.BookingType == BookingType.Transport && b.TransportId != null && b.Transport!.PickupLocation.Trim() != "")
                .GroupBy(b => b.Transport!.PickupLocation.Trim())
                .Select(g => new LocationTotal
                {
                    Location = g.Key,
                    BookingsCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                })
                .ToListAsync(ct);

        private static Task<List<LocationTotal>> PackageLocationsAsync(IQueryable<Booking> filtered, CancellationToken ct) =>
            filtered
                .Where(b => b.BookingType == BookingType.Package && b.HotelId != null && b.Hotel!.City.Trim() != "")
                .GroupBy(b => b.Hotel!.City.Trim())
                .Select(g => new LocationTotal
                {
                    Location = g.Key,
                    BookingsCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.Status == BookingStatus.Confirmed ? (decimal?)b.TotalPrice : 0m) ?? 0m
                })
                .ToListAsync(ct);

        private async Task<List<RecentBookingDTO>> LoadRecentAsync(
            IQueryable<Booking> filtered,
            DateTime cutoff,
            CancellationToken ct)
        {
            var rows = await filtered
                .OrderByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.Id)
                .Take(RecentLimit)
                .Select(b => new RecentRow
                {
                    Id = b.Id,
                    PaymentReference = b.PaymentReference,
                    MerchantOrderId = b.MerchantOrderId,
                    CustomerName = b.CustomerName,
                    CustomerEmail = b.CustomerEmail,
                    BookingType = b.BookingType,
                    HotelName = b.Hotel != null ? b.Hotel.Name : null,
                    RoomName = b.Room != null ? b.Room.Name : null,
                    TripName = b.Trip != null ? b.Trip.Name : null,
                    TransportName = b.Transport != null ? b.Transport.Name : null,
                    EventName = b.Event != null ? b.Event.Name : null,
                    PackageName = b.Package != null ? b.Package.Name : null,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    IsPaid = b.IsPaid,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync(ct);

            return rows.Select(row => new RecentBookingDTO
            {
                EncodedId = IdEncoder.EncodeId(row.Id),
                BookingReference = ResolveReference(row),
                UserName = row.CustomerName,
                Email = row.CustomerEmail,
                BookingType = (int)row.BookingType,
                BookingTypeName = row.BookingType.ToString(),
                ItemName = ResolveItemName(row),
                TotalPrice = RoundMoney(row.TotalPrice),
                Status = row.Status == BookingStatus.Pending && row.CreatedAt < cutoff
                    ? "Expired"
                    : row.Status.ToString(),
                PaymentStatus = row.IsPaid ? "Paid" : "Unpaid",
                CreatedAt = row.CreatedAt
            }).ToList();
        }

        private async Task<List<LowCapacityAlertDTO>> LoadAlertsAsync(DateTime cutoff, CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;

            var eventAlerts = await (
                from usage in HoldingInventory(_db.Bookings.AsNoTracking(), cutoff)
                    .Where(b => b.BookingType == BookingType.Event && b.EventId != null)
                    .GroupBy(b => b.EventId!.Value)
                    .Select(g => new { Id = g.Key, Used = g.Sum(b => b.Quantity) })
                join item in _db.Events.AsNoTracking() on usage.Id equals item.Id
                where item.IsActive
                    && (item.EndDate ?? item.EventDate) >= today
                    && item.TotalTickets - usage.Used <= LowCapacityThreshold
                orderby item.TotalTickets - usage.Used, item.Name
                select new CapacityRow
                {
                    Id = item.Id,
                    Name = item.Name,
                    ItemType = "Event",
                    Capacity = item.TotalTickets,
                    Used = usage.Used,
                    ServiceDate = item.EventDate
                })
                .ToListAsync(ct);

            var packageAlerts = await (
                from usage in HoldingInventory(_db.Bookings.AsNoTracking(), cutoff)
                    .Where(b => b.BookingType == BookingType.Package && b.PackageId != null)
                    .GroupBy(b => b.PackageId!.Value)
                    .Select(g => new { Id = g.Key, Used = g.Sum(b => b.Quantity) })
                join item in _db.Packages.AsNoTracking() on usage.Id equals item.Id
                where item.IsActive
                    && (item.EndDate == null || item.EndDate >= today)
                    && item.MaxPackagesAvailable - usage.Used <= LowCapacityThreshold
                orderby item.MaxPackagesAvailable - usage.Used, item.Name
                select new CapacityRow
                {
                    Id = item.Id,
                    Name = item.Name,
                    ItemType = "Package",
                    Capacity = item.MaxPackagesAvailable,
                    Used = usage.Used,
                    ServiceDate = item.StartDate
                })
                .ToListAsync(ct);

            var tripAlerts = await (
                from usage in HoldingInventory(_db.Bookings.AsNoTracking(), cutoff)
                    .Where(b => b.BookingType == BookingType.Trip
                        && b.TripId != null
                        && b.ServiceDate != null
                        && b.ServiceDate >= today)
                    .GroupBy(b => new { Id = b.TripId!.Value, Day = b.ServiceDate!.Value.Date })
                    .Select(g => new { g.Key.Id, g.Key.Day, Used = g.Sum(b => b.Adults + b.Children) })
                join item in _db.Trips.AsNoTracking() on usage.Id equals item.Id
                where item.IsActive
                    && (item.EndDate == null || item.EndDate >= today)
                    && item.MaxCapacity - usage.Used <= LowCapacityThreshold
                orderby item.MaxCapacity - usage.Used, item.Name, usage.Day
                select new CapacityRow
                {
                    Id = item.Id,
                    Name = item.Name,
                    ItemType = "Trip",
                    Capacity = item.MaxCapacity,
                    Used = usage.Used,
                    ServiceDate = usage.Day
                })
                .ToListAsync(ct);

            var seenEvents = eventAlerts.Select(row => row.Id).ToList();
            var seenPackages = packageAlerts.Select(row => row.Id).ToList();
            var seenTrips = tripAlerts.Select(row => row.Id).ToList();

            var tinyEvents = await TinyEventsAsync(today, seenEvents, ct);
            var tinyPackages = await TinyPackagesAsync(today, seenPackages, ct);
            var tinyTrips = await TinyTripsAsync(today, seenTrips, ct);

            return eventAlerts
                .Concat(tinyEvents)
                .Concat(packageAlerts)
                .Concat(tinyPackages)
                .Concat(tripAlerts)
                .Concat(tinyTrips)
                .Select(ToAlert)
                .OrderBy(alert => alert.Remaining)
                .ThenBy(alert => alert.ItemType)
                .ThenBy(alert => alert.Name)
                .ThenBy(alert => alert.ServiceDate)
                .Take(AlertLimit)
                .ToList();
        }

        private async Task<List<CapacityRow>> TinyEventsAsync(DateTime today, List<int> seenIds, CancellationToken ct)
        {
            var query = _db.Events.AsNoTracking()
                .Where(item => item.IsActive
                    && (item.EndDate ?? item.EventDate) >= today
                    && item.TotalTickets <= LowCapacityThreshold);
            if (seenIds.Count > 0)
                query = query.Where(item => !seenIds.Contains(item.Id));

            return await query
                .OrderBy(item => item.TotalTickets)
                .ThenBy(item => item.Name)
                .Select(item => new CapacityRow
                {
                    Id = item.Id,
                    Name = item.Name,
                    ItemType = "Event",
                    Capacity = item.TotalTickets,
                    Used = 0,
                    ServiceDate = item.EventDate
                })
                .Take(AlertLimit)
                .ToListAsync(ct);
        }

        private async Task<List<CapacityRow>> TinyPackagesAsync(DateTime today, List<int> seenIds, CancellationToken ct)
        {
            var query = _db.Packages.AsNoTracking()
                .Where(item => item.IsActive
                    && (item.EndDate == null || item.EndDate >= today)
                    && item.MaxPackagesAvailable <= LowCapacityThreshold);
            if (seenIds.Count > 0)
                query = query.Where(item => !seenIds.Contains(item.Id));

            return await query
                .OrderBy(item => item.MaxPackagesAvailable)
                .ThenBy(item => item.Name)
                .Select(item => new CapacityRow
                {
                    Id = item.Id,
                    Name = item.Name,
                    ItemType = "Package",
                    Capacity = item.MaxPackagesAvailable,
                    Used = 0,
                    ServiceDate = item.StartDate
                })
                .Take(AlertLimit)
                .ToListAsync(ct);
        }

        private async Task<List<CapacityRow>> TinyTripsAsync(DateTime today, List<int> seenIds, CancellationToken ct)
        {
            var query = _db.Trips.AsNoTracking()
                .Where(item => item.IsActive
                    && (item.EndDate == null || item.EndDate >= today)
                    && item.MaxCapacity <= LowCapacityThreshold);
            if (seenIds.Count > 0)
                query = query.Where(item => !seenIds.Contains(item.Id));

            return await query
                .OrderBy(item => item.MaxCapacity)
                .ThenBy(item => item.Name)
                .Select(item => new CapacityRow
                {
                    Id = item.Id,
                    Name = item.Name,
                    ItemType = "Trip",
                    Capacity = item.MaxCapacity,
                    Used = 0,
                    ServiceDate = null
                })
                .Take(AlertLimit)
                .ToListAsync(ct);
        }

        private IQueryable<Booking> BookingsInRange(DashboardRange range)
        {
            var query = _db.Bookings.AsNoTracking();
            if (range.FromInclusive.HasValue)
            {
                var from = range.FromInclusive.Value;
                query = query.Where(b => (b.ServiceDate ?? b.CreatedAt) >= from);
            }

            if (range.ToExclusive.HasValue)
            {
                var to = range.ToExclusive.Value;
                query = query.Where(b => (b.ServiceDate ?? b.CreatedAt) < to);
            }

            return query;
        }

        private static IQueryable<Booking> HoldingInventory(IQueryable<Booking> source, DateTime cutoff) =>
            source.Where(b =>
                b.Status != BookingStatus.Cancelled &&
                (b.Status != BookingStatus.Pending || b.CreatedAt >= cutoff));

        private static DashboardRange NormalizeRange(DateTime? fromDate, DateTime? toDate)
        {
            DateTime? from = fromDate.HasValue ? AsUtcCalendar(fromDate.Value) : null;
            DateTime? toExclusive = toDate.HasValue ? ExclusiveUpper(toDate.Value) : null;

            if (from.HasValue && toExclusive.HasValue && from.Value >= toExclusive.Value)
                throw new ArgumentException("fromDate must be on or before toDate.");

            return new DashboardRange(from, toExclusive);
        }

        private static DateTime AsUtcCalendar(DateTime value)
        {
            if (value.Kind == DateTimeKind.Local && value.TimeOfDay != TimeSpan.Zero)
                return value.ToUniversalTime();

            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private static DateTime ExclusiveUpper(DateTime toDate)
        {
            var to = AsUtcCalendar(toDate);
            if (to.TimeOfDay != TimeSpan.Zero)
                return to == DateTime.MaxValue ? to : to.AddTicks(1);

            if (to.Year == 9999 && to.Month == 12 && to.Day == 31)
                return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

            return DateTime.SpecifyKind(to.Date.AddDays(1), DateTimeKind.Utc);
        }

        private static MonthWindow TrendWindow(DashboardRange range)
        {
            var currentMonth = MonthStart(DateTime.UtcNow);
            var lastMonth = range.ToExclusive.HasValue
                ? MonthStart(range.ToExclusive.Value.AddTicks(-1))
                : currentMonth;

            DateTime firstMonth;
            if (!range.HasFilter)
            {
                firstMonth = lastMonth.AddMonths(-(DefaultTrendMonths - 1));
            }
            else if (range.FromInclusive.HasValue)
            {
                firstMonth = MonthStart(range.FromInclusive.Value);
                if (!range.ToExclusive.HasValue && firstMonth > currentMonth)
                    lastMonth = firstMonth.AddMonths(DefaultTrendMonths - 1);
            }
            else
            {
                firstMonth = lastMonth.AddMonths(-(DefaultTrendMonths - 1));
            }

            var earliest = lastMonth.AddMonths(-(MaxTrendMonths - 1));
            if (firstMonth < earliest)
                firstMonth = earliest;
            if (firstMonth > lastMonth)
                firstMonth = lastMonth;

            return new MonthWindow(firstMonth, lastMonth);
        }

        private static DateTime MonthStart(DateTime value) =>
            new(value.Year, value.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        private static void AssignShares(List<ServiceTypeBreakdownDTO> items)
        {
            var total = items.Sum(item => item.Revenue);
            if (total <= 0m)
                return;

            ServiceTypeBreakdownDTO? largest = null;
            decimal assigned = 0m;
            foreach (var item in items)
            {
                item.RevenueSharePercentage = RoundMoney(item.Revenue / total * 100m);
                assigned += item.RevenueSharePercentage;
                if (largest == null || item.Revenue > largest.Revenue)
                    largest = item;
            }

            if (largest == null || largest.Revenue <= 0m)
                return;

            var drift = RoundMoney(100m - assigned);
            if (drift != 0m)
                largest.RevenueSharePercentage = RoundMoney(largest.RevenueSharePercentage + drift);
        }

        private static CatalogCountDTO ToCatalogCount(int? active, int? total) =>
            new() { Active = active ?? 0, Total = total ?? 0 };

        private static LowCapacityAlertDTO ToAlert(CapacityRow row) =>
            new()
            {
                EncodedId = IdEncoder.EncodeId(row.Id),
                ItemType = row.ItemType,
                Name = row.Name,
                Capacity = row.Capacity,
                Booked = row.Used,
                Remaining = row.Capacity - row.Used,
                ServiceDate = row.ServiceDate
            };

        private static string ResolveReference(RecentRow row)
        {
            if (!string.IsNullOrWhiteSpace(row.PaymentReference))
                return row.PaymentReference.Trim();
            if (!string.IsNullOrWhiteSpace(row.MerchantOrderId))
                return row.MerchantOrderId.Trim();
            return IdEncoder.EncodeId(row.Id);
        }

        private static string ResolveItemName(RecentRow row)
        {
            return row.BookingType switch
            {
                BookingType.Hotel => JoinName(row.HotelName, row.RoomName),
                BookingType.Trip => row.TripName ?? string.Empty,
                BookingType.Transport => row.TransportName ?? string.Empty,
                BookingType.Event => row.EventName ?? string.Empty,
                BookingType.Package => row.PackageName ?? string.Empty,
                _ => string.Empty
            };
        }

        private static string JoinName(string? left, string? right)
        {
            var hasLeft = !string.IsNullOrWhiteSpace(left);
            var hasRight = !string.IsNullOrWhiteSpace(right);
            if (hasLeft && hasRight)
                return $"{left!.Trim()} - {right!.Trim()}";
            if (hasLeft)
                return left!.Trim();
            return hasRight ? right!.Trim() : string.Empty;
        }

        private static decimal RoundMoney(decimal value) =>
            Math.Round(value, 2, MidpointRounding.AwayFromZero);

        private static void MergeLocations(Dictionary<string, LocationTotal> totals, IEnumerable<LocationTotal> rows)
        {
            foreach (var row in rows)
            {
                var location = row.Location?.Trim();
                if (string.IsNullOrEmpty(location))
                    continue;

                if (!totals.TryGetValue(location, out var existing))
                {
                    totals[location] = new LocationTotal
                    {
                        Location = location,
                        BookingsCount = row.BookingsCount,
                        TotalRevenue = row.TotalRevenue
                    };
                    continue;
                }

                existing.BookingsCount += row.BookingsCount;
                existing.TotalRevenue += row.TotalRevenue;
            }
        }

        private readonly record struct DashboardRange(DateTime? FromInclusive, DateTime? ToExclusive)
        {
            public bool HasFilter => FromInclusive.HasValue || ToExclusive.HasValue;
        }

        private readonly record struct MonthWindow(DateTime FirstMonth, DateTime LastMonth);

        private sealed class BreakdownRow
        {
            public BookingType BookingType { get; set; }
            public int BookingsCount { get; set; }
            public int ConfirmedCount { get; set; }
            public decimal Revenue { get; set; }
            public decimal TaxAmount { get; set; }
        }

        private sealed class TrendRow
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int BookingsCount { get; set; }
            public decimal ConfirmedRevenue { get; set; }
        }

        private sealed class RankedRow
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public int BookingsCount { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        private sealed class LocationTotal
        {
            public string Location { get; set; } = "";
            public int BookingsCount { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        private sealed class RecentRow
        {
            public int Id { get; set; }
            public string? PaymentReference { get; set; }
            public string? MerchantOrderId { get; set; }
            public string CustomerName { get; set; } = "";
            public string CustomerEmail { get; set; } = "";
            public BookingType BookingType { get; set; }
            public string? HotelName { get; set; }
            public string? RoomName { get; set; }
            public string? TripName { get; set; }
            public string? TransportName { get; set; }
            public string? EventName { get; set; }
            public string? PackageName { get; set; }
            public decimal TotalPrice { get; set; }
            public BookingStatus Status { get; set; }
            public bool IsPaid { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private sealed class CapacityRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string ItemType { get; set; } = "";
            public int Capacity { get; set; }
            public int Used { get; set; }
            public DateTime? ServiceDate { get; set; }
        }
    }
}
