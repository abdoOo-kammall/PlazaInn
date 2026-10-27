using Shared.Security;

namespace Plaza.Extensions
{
    public static class IdEncoding
    {
        public static bool TryDecode(string? encodedId, out int id)
        {
            if (string.IsNullOrWhiteSpace(encodedId))
            {
                id = 0;
                return false;
            }

            try
            {
                id = IdEncoder.DecodeId(encodedId);
                return true;
            }
            catch (ArgumentException)
            {
                id = 0;
                return false;
            }
        }

        public static bool TryDecodeMany(IEnumerable<string>? encodedIds, out List<int> ids)
        {
            ids = new List<int>();
            if (encodedIds == null)
                return true;

            foreach (var encodedId in encodedIds)
            {
                if (string.IsNullOrWhiteSpace(encodedId))
                    continue;

                if (!TryDecode(encodedId, out var id))
                {
                    ids = new List<int>();
                    return false;
                }

                ids.Add(id);
            }

            return true;
        }
    }
}
