using System.Globalization;
using GenericCrud.Metadata;
using Microsoft.AspNetCore.Http;

namespace GenericCrud.Services
{
    public class ReflectionService : IReflectionService
    {
        public object CreateInstance(EntityMetadata metadata, IFormCollection form)
        {
            var entity = Activator.CreateInstance(metadata.ClrType)
                ?? throw new InvalidOperationException($"Could not create instance of {metadata.ClrType.Name}");

            ApplyFormValues(metadata, entity, form, includeIdentity: false);
            return entity;
        }

        public void ApplyFormValues(EntityMetadata metadata, object entity, IFormCollection form) =>
            ApplyFormValues(metadata, entity, form, includeIdentity: false);

        private void ApplyFormValues(EntityMetadata metadata, object entity, IFormCollection form, bool includeIdentity)
        {
            foreach (var prop in metadata.Properties)
            {
                // Never overwrite an identity/auto-generated primary key from posted data.
                if (prop.IsIdentity && !includeIdentity) continue;

                var clrProp = metadata.ClrType.GetProperty(prop.Name);
                if (clrProp == null || !clrProp.CanWrite) continue;

                // Checkboxes only post a value when checked — absence means "false".
                if (prop.InputType == CrudInputType.Checkbox)
                {
                    var isChecked = form.ContainsKey(prop.Name) &&
                                     (form[prop.Name].ToString().Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                      form[prop.Name].ToString().Equals("on", StringComparison.OrdinalIgnoreCase));
                    clrProp.SetValue(entity, isChecked);
                    continue;
                }

                if (!form.ContainsKey(prop.Name)) continue;

                var raw = form[prop.Name].ToString();

                // Optional FK / nullable value left blank in the dropdown or input — leave as null.
                if (string.IsNullOrWhiteSpace(raw))
                {
                    if (prop.IsNullable) clrProp.SetValue(entity, null);
                    continue;
                }

                var converted = ConvertValue(raw, prop.UnderlyingType);
                clrProp.SetValue(entity, converted);
            }
        }

        private static object ConvertValue(string raw, Type targetType)
        {
            if (targetType == typeof(string)) return raw;
            if (targetType == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(long)) return long.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(short)) return short.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(decimal)) return decimal.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(double)) return double.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(float)) return float.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(bool)) return bool.Parse(raw);
            if (targetType == typeof(Guid)) return Guid.Parse(raw);
            if (targetType == typeof(DateTime)) return DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None);
            if (targetType == typeof(DateOnly)) return DateOnly.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(TimeOnly)) return TimeOnly.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType.IsEnum) return Enum.Parse(targetType, raw);

            // Fallback for anything else EF might expose.
            return Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
        }
    }
}
