namespace PlazaCore.Entites
{
    public class PackageEvent : BaseEntity
    {
        public int PackageId { get; set; }
        public Package Package { get; set; } = null!;
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;
    }
}
