using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Email
{
    public class SendEmailInternal
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<(MemoryStream Stream, string FileName)> AttachStreams { get; set; } = new();
    }

}
