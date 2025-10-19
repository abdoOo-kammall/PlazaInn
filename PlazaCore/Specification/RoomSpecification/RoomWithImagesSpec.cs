using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.Specification.RoomSpecification
{
    public class RoomWithImagesSpec : Specification<Room>
    {
        public RoomWithImagesSpec()
        {
            //AddInclude(x=>x.Images);
            AddInclude(X => X.Hotel);
        }
        public RoomWithImagesSpec(int id ):base(x=>x.Id==id)
        {
            //AddInclude(x=>x.Images);
            AddInclude(X => X.Hotel);

        }
    }
}
