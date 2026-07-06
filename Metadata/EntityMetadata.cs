namespace GenericCrud.Metadata
{
    /// <summary>
    /// Full description of an entity built from EF Core's model metadata.
    /// The controller and views work only against this class — never against the CLR entity directly.
    /// </summary>
    public class EntityMetadata
    {
        public string EntityName { get; set; } = string.Empty;
        public Type ClrType { get; set; } = default!;
        public string TableName { get; set; } = string.Empty;

        public PropertyMetadata PrimaryKey { get; set; } = default!;

        /// <summary>All visible scalar properties, in declaration order, excluding navigation/collection/excluded columns.</summary>
        public List<PropertyMetadata> Properties { get; set; } = new();

        /// <summary>All foreign keys detected on this entity.</summary>
        public List<ForeignKeyMetadata> ForeignKeys { get; set; } = new();

        /// <summary>Properties to show in the list grid (PK + FKs + everything else, minus excluded columns).</summary>
        public List<PropertyMetadata> ListProperties =>
            Properties.Where(p => !p.IsPrimaryKey || true).ToList();

        public PropertyMetadata? GetProperty(string name) =>
            Properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
