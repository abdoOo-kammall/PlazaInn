using PlazaCore.Entites;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Specification.SectionImageSpecification
{
    public class ImagesByTypeSpecification : Specification<Image>
    {
        public ImagesByTypeSpecification(PlazaInnType type)
            : base(i => i.Type == type)
        {
        }
    }

}
