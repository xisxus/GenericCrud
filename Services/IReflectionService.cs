using GenericCrud.Metadata;
using Microsoft.AspNetCore.Http;

namespace GenericCrud.Services
{
    public interface IReflectionService
    {
        /// <summary>Creates a new CLR instance of the entity and populates it from posted form values.</summary>
        object CreateInstance(EntityMetadata metadata, IFormCollection form);

        /// <summary>Applies posted form values onto an existing tracked entity (used for Edit — PK/identity columns are skipped).</summary>
        void ApplyFormValues(EntityMetadata metadata, object entity, IFormCollection form);
    }
}
