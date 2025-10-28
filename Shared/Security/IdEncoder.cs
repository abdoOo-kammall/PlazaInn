using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Security
{
    public static class IdEncoder
    {
        private static readonly string secretKey = "PlazaInnKey2025";
        public static string EncodeId (int id) {
            var plainText = $"{id}:{secretKey}";
            var bytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes)
                          .Replace("+", "-")
                          .Replace("/", "_")
                          .Replace("=", "");

        }
        public static int DecodeId(string encoded)
        {
            try
            {
                encoded = encoded.Replace("-", "+").Replace("_", "/");
                var bytes = Convert.FromBase64String(PadBase64(encoded));
                var decoded = Encoding.UTF8.GetString(bytes);
                var parts = decoded.Split(':');
                if (parts.Length == 2 && parts[1] == secretKey)
                {
                    return int.Parse(parts[0]);
                }
            }
            catch { }

            throw new Exception("Invalid ID");
        }

        private static string PadBase64(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return base64;
        }
    }
}
