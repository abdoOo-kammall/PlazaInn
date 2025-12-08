using PlazaCore.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Specification.SectionImageSpecification
{
    public class ImagesByIdsSpecification : Specification<Image>
    {
        public ImagesByIdsSpecification(List<int> ids)
            : base(i => ids.Contains(i.Id))
        {
        }
    }

}
