using PlazaCore.Entites;
using Shared.Enums;

namespace PlazaCore.Specification.SectionImageSpecification
{
    public class ImageByHashSpec : Specification<Image>
    {
        public ImageByHashSpec(string hash, PlazaInnType type)
            : base(i => i.Hash == hash && i.Type == type)
        {
        }
    }
}
