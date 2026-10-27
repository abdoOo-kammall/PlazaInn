using Microsoft.EntityFrameworkCore;
using Plaza.Exceptions;

namespace PlazaService.Catalog
{
    internal static class CatalogPersistence
    {
        public static async Task SaveOrConflictAsync(Func<Task> save, string conflictMessage)
        {
            try
            {
                await save();
            }
            catch (DbUpdateException ex) when (IsReferenceConflict(ex))
            {
                throw new ConflictException(conflictMessage);
            }
        }

        private static bool IsReferenceConflict(DbUpdateException ex)
        {
            for (Exception? current = ex; current != null; current = current.InnerException)
            {
                if (current.Message.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
                    || current.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase)
                    || current.Message.Contains("DELETE statement conflicted", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
