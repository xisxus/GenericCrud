namespace GenericCrud.Metadata
{
    public enum CrudInputType
    {
        Text,
        Number,
        Decimal,
        Checkbox,
        Date,
        Time,
        DateTime,
        Dropdown,
        File,
        TextArea
    }

    /// <summary>
    /// Describes a single scalar property on an entity, enough for the
    /// dynamic form/list/details views to render it without knowing the entity type.
    /// </summary>
    public class PropertyMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Type ClrType { get; set; } = default!;
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsRequired { get; set; }
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public int? MinLength { get; set; }
        public double? RangeMin { get; set; }
        public double? RangeMax { get; set; }
        public CrudInputType InputType { get; set; } = CrudInputType.Text;

        /// <summary>Set when IsForeignKey is true — points to the related FK metadata.</summary>
        public ForeignKeyMetadata? ForeignKey { get; set; }

        /// <summary>Underlying (non-nullable) type, e.g. int for int?.</summary>
        public Type UnderlyingType => Nullable.GetUnderlyingType(ClrType) ?? ClrType;
    }
}
