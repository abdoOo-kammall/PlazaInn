using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO.File;

namespace Shared.DTO.AboutUs
{
    public class AboutUsDto
    {
        public int Id { get; set; }
        public string? Tittle { get; set; }
        public string? SubTittle { get; set; }
        public string? Description { get; set; }
        //public List<FileDto> Images { get; set; }

        public FileDto? Square1ImageId { get; set; }
        public FileDto? CircleImageId { get; set; }
        public FileDto? Square2ImageId { get; set; }

    }
}
