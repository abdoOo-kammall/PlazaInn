using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.DTO.Payment
{
    public class AuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
