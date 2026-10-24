using PlazaCore.Entites;

namespace PlazaCore.Specification.HotelSpecification
{
    public class HotelsByIdsSpec : Specification<Hotel>
    {
        public HotelsByIdsSpec(IReadOnlyCollection<int> ids)
            : base(h => ids.Contains(h.Id))
        {
        }
    }
}
