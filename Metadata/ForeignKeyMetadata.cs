namespace GenericCrud.Metadata
{
    /// <summary>
    /// Describes a foreign key relationship detected from EF Core model metadata.
    /// </summary>
    public class ForeignKeyMetadata
    {
        /// <summary>Name of the scalar FK property, e.g. "DepartmentId".</summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>CLR entity name of the principal (parent) entity, e.g. "Department".</summary>
        public string PrincipalEntityName { get; set; } = string.Empty;

        /// <summary>CLR type of the principal entity.</summary>
        public Type PrincipalClrType { get; set; } = default!;

        /// <summary>Primary key property name on the principal entity, e.g. "Id".</summary>
        public string PrincipalKeyName { get; set; } = string.Empty;

        /// <summary>Property used as the human-readable label in dropdowns (Name/Title/Description/Code/first string prop).</summary>
        public string DisplayPropertyName { get; set; } = string.Empty;

        /// <summary>Is the FK column nullable (optional relationship)?</summary>
        public bool IsNullable { get; set; }
    }
}
