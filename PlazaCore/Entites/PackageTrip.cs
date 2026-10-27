namespace PlazaCore.Entites
{
    public class PackageTrip : BaseEntity
    {
        public int PackageId { get; set; }
        public Package Package { get; set; } = null!;
        public int TripId { get; set; }
        public Trip Trip { get; set; } = null!;
    }
}
