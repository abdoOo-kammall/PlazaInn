using PlazaCore.Entites;

namespace PlazaCore.Specification.RoomSpecification
{
    public class RoomWithHotelSpec : Specification<Room>
    {
        public RoomWithHotelSpec(int roomId) : base(r => r.Id == roomId)
        {
            AddInclude(r => r.Hotel);
        }
    }
}
