using System.ComponentModel.DataAnnotations;
using GenericCrud.Metadata;
using GenericCrud.Services;
using GenericCrud.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenericCrud.Controllers
{
    // Route: /CrudMaster/{entity}
    [Route("CrudMaster")]
    public class CrudMasterController : Controller
    {
        private readonly IEntityMetadataService _metadataService;
        private readonly ICrudService _crudService;
        private readonly IDropdownService _dropdownService;

        public CrudMasterController(
            IEntityMetadataService metadataService,
            ICrudService crudService,
            IDropdownService dropdownService)
        {
            _metadataService = metadataService;
            _crudService = crudService;
            _dropdownService = dropdownService;
        }

        // GET /CrudMaster/Employee
        [HttpGet("{entity}")]
        public async Task<IActionResult> Index(string entity)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null) return EntityNotFound(entity);

            var rows = await _crudService.GetAllAsync(metadata);

            var vm = new CrudListViewModel2
            {
                EntityName = metadata.EntityName,
                Metadata = metadata,
                Rows = new List<CrudRow2>()
            };

            // build table rows
            foreach (var row in rows)
            {
                var crudRow = new CrudRow2
                {
                    Id = metadata.ClrType
                             .GetProperty(metadata.PrimaryKey.Name)!
                             .GetValue(row)?.ToString() ?? string.Empty
                };

                foreach (var prop in metadata.Properties)
                {
                    var rawValue = metadata.ClrType.GetProperty(prop.Name)!.GetValue(row);

                    crudRow.Values[prop.Name] = prop.IsForeignKey && prop.ForeignKey != null
                        ? await _dropdownService.GetDisplayTextAsync(prop.ForeignKey, rawValue) ?? "(none)"
                        : FormatValue(prop, rawValue);
                }

                vm.Rows.Add(crudRow);
            }

            // build form dropdown options (for the left-side form)
            foreach (var prop in metadata.Properties.Where(p => p.IsForeignKey && p.ForeignKey != null))
            {
                vm.FormDropdownOptions[prop.Name] = await _dropdownService.GetOptionsAsync(prop.ForeignKey!);
            }

            return View(vm);
        }

        // POST /CrudMaster/SaveAjax/Employee  (AJAX create)
        [HttpPost("SaveAjax/{entity}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAjax(string entity)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null)
                return Json(new { isSuccess = false, message = $"Entity '{entity}' not found." });

            var entityInstance = await _crudService.CreateAsync(metadata, Request.Form);
            var errors = ValidateEntity(entityInstance);

            if (errors.Count > 0)
            {
                // roll back the speculative insert
                await _crudService.DeleteAsync(
                    metadata,
                    metadata.ClrType.GetProperty(metadata.PrimaryKey.Name)!.GetValue(entityInstance)!);

                return Json(new { isSuccess = false, errors });
            }

            return Json(new { isSuccess = true, message = $"{metadata.EntityName} created successfully." });
        }

        // POST /CrudMaster/UpdateAjax/Employee/5  (AJAX update)
        [HttpPost("UpdateAjax/{entity}/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAjax(string entity, string id)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null)
                return Json(new { isSuccess = false, message = $"Entity '{entity}' not found." });

            var key = _crudService.ConvertKey(metadata, id);
            var existing = await _crudService.GetByIdAsync(metadata, key);
            if (existing == null)
                return Json(new { isSuccess = false, message = $"{metadata.EntityName} with id '{id}' was not found." });

            var ok = await _crudService.UpdateAsync(metadata, key, Request.Form);
            var errors = ok ? ValidateEntity(existing) : new List<string> { "Update failed." };

            if (errors.Count > 0)
                return Json(new { isSuccess = false, errors });

            return Json(new { isSuccess = true, message = $"{metadata.EntityName} updated successfully." });
        }

        // POST /CrudMaster/DeleteAjax/Employee/5  (AJAX delete)
        [HttpPost("DeleteAjax/{entity}/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(string entity, string id)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null)
                return Json(new { isSuccess = false, message = $"Entity '{entity}' not found." });

            var key = _crudService.ConvertKey(metadata, id);
            await _crudService.DeleteAsync(metadata, key);

            return Json(new { isSuccess = true, message = $"{metadata.EntityName} deleted successfully." });
        }

        // ---------- helpers ----------

        private static List<string> ValidateEntity(object entity)
        {
            var context = new ValidationContext(entity);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(entity, context, results, validateAllProperties: true);
            return results.Select(r => r.ErrorMessage ?? "Invalid value.").ToList();
        }

        private static string FormatValue(PropertyMetadata prop, object? value)
        {
            if (value == null) return "";
            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd"),
                DateOnly d => d.ToString("yyyy-MM-dd"),
                TimeOnly t => t.ToString("HH:mm"),
                bool b => b ? "Yes" : "No",
                decimal dec => dec.ToString("N2"),
                _ => value.ToString() ?? ""
            };
        }

        private IActionResult EntityNotFound(string entity) =>
            NotFound($"Entity '{entity}' is not registered in the DbContext.");
    }
}