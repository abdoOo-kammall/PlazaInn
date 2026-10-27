namespace PlazaCore.Entites
{
    public class PackageTransport : BaseEntity
    {
        public int PackageId { get; set; }
        public Package Package { get; set; } = null!;
        public int TransportId { get; set; }
        public Transport Transport { get; set; } = null!;
    }
}
