using GenericCrud.Metadata;

namespace GenericCrud.Services
{
    public interface IEntityMetadataService
    {
        /// <summary>Builds (and caches) full metadata for the entity matching the given name (case-insensitive).</summary>
        EntityMetadata? GetEntityMetadata(string entityName);

        /// <summary>Names of every entity registered in the DbContext, for building nav menus etc.</summary>
        List<string> GetAllEntityNames();
    }
}
