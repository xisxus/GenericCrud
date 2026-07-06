using System.ComponentModel.DataAnnotations;
using GenericCrud.Metadata;
using GenericCrud.Services;
using GenericCrud.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenericCrud.Controllers
{
    // Route: /Crud/{entity} for the list, /Crud/{action}/{entity} for everything else.
    // e.g. /Crud/Employee (list), /Crud/Create/Employee, /Crud/Edit/Employee/5
    [Route("Crud")]
    public class CrudController : Controller
    {
        private readonly IEntityMetadataService _metadataService;
        private readonly ICrudService _crudService;
        private readonly IDropdownService _dropdownService;

        public CrudController(
            IEntityMetadataService metadataService,
            ICrudService crudService,
            IDropdownService dropdownService)
        {
            _metadataService = metadataService;
            _crudService = crudService;
            _dropdownService = dropdownService;
        }

        // GET /Crud/Employee
        [HttpGet("{entity}")]
        public async Task<IActionResult> Index(string entity)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null) return EntityNotFound(entity);

            var rows = await _crudService.GetAllAsync(metadata);

            var vm = new CrudListViewModel
            {
                EntityName = metadata.EntityName,
                Metadata = metadata,
                Rows = new List<ViewModels.CrudRow>()
            };

            foreach (var row in rows)
            {
                var crudRow = new ViewModels.CrudRow
                {
                    Id = metadata.ClrType.GetProperty(metadata.PrimaryKey.Name)!.GetValue(row)?.ToString() ?? string.Empty
                };

                foreach (var prop in metadata.Properties)
                {
                    var rawValue = metadata.ClrType.GetProperty(prop.Name)!.GetValue(row);

                    if (prop.IsForeignKey && prop.ForeignKey != null)
                    {
                        var displayText = await _dropdownService.GetDisplayTextAsync(prop.ForeignKey, rawValue);
                        crudRow.Values[prop.Name] = displayText ?? "(none)";
                    }
                    else
                    {
                        crudRow.Values[prop.Name] = FormatValue(prop, rawValue);
                    }
                }

                vm.Rows.Add(crudRow);
            }

            return View(vm);
        }

        // GET /Crud/Create/Employee
        [HttpGet("Create/{entity}")]
        public async Task<IActionResult> Create(string entity)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null) return EntityNotFound(entity);

            var vm = await BuildFormViewModel(metadata, isEdit: false, id: null, values: null);
            return View("Form", vm);
        }

        // POST /Crud/Save/Employee
        [HttpPost("Save/{entity}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string entity)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null) return EntityNotFound(entity);

            var entityInstance = await _crudService.CreateAsync(metadata, Request.Form);
            var errors = ValidateEntity(entityInstance);

            if (errors.Count > 0)
            {
                // Roll back the speculative insert and re-show the form with errors.
                await _crudService.DeleteAsync(metadata, metadata.ClrType.GetProperty(metadata.PrimaryKey.Name)!.GetValue(entityInstance)!);

                var vm = await BuildFormViewModel(metadata, isEdit: false, id: null, values: Request.Form);
                vm.Errors = errors;
                return View("Form", vm);
            }

            TempData["Success"] = $"{metadata.EntityName} created successfully.";
            return RedirectToAction("Index", new { entity = metadata.EntityName });
        }

        // GET /Crud/Edit/Employee/5
        [HttpGet("Edit/{entity}/{id}")]
        public async Task<IActionResult> Edit(string entity, string id)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null) return EntityNotFound(entity);

            var key = _crudService.ConvertKey(metadata, id);
            var record = await _crudService.GetByIdAsync(metadata, key);
            if (record == null) return NotFound($"{metadata.EntityName} with id '{id}' was not found.");

            var vm = await BuildFormViewModel(metadata, isEdit: true, id: id, values: null, sourceEntity: record);
            return View("Form", vm);
        }

        // POST /Crud/Update/Employee/5
        [HttpPost("Update/{entity}/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string entity, string id)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null) return EntityNotFound(entity);

            var key = _crudService.ConvertKey(metadata, id);
            var existing = await _crudService.GetByIdAsync(metadata, key);
            if (existing == null) return NotFound($"{metadata.EntityName} with id '{id}' was not found.");

            var ok = await _crudService.UpdateAsync(metadata, key, Request.Form);
            var errors = ok ? ValidateEntity(existing) : new List<string> { "Update failed." };

            if (errors.Count > 0)
            {
                var vm = await BuildFormViewModel(metadata, isEdit: true, id: id, values: Request.Form);
                vm.Errors = errors;
                return View("Form", vm);
            }

            TempData["Success"] = $"{metadata.EntityName} updated successfully.";
            return RedirectToAction("Index", new { entity = metadata.EntityName });
        }

        // GET /Crud/Delete/Employee/5  (confirmation page)
        [HttpGet("Delete/{entity}/{id}")]
        public async Task<IActionResult> Delete(string entity, string id)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null) return EntityNotFound(entity);

            var key = _crudService.ConvertKey(metadata, id);
            var record = await _crudService.GetByIdAsync(metadata, key);
            if (record == null) return NotFound($"{metadata.EntityName} with id '{id}' was not found.");

            var vm = await BuildFormViewModel(metadata, isEdit: true, id: id, values: null, sourceEntity: record);
            return View(vm);
        }

        // POST /Crud/DeleteConfirmed/Employee/5
        [HttpPost("DeleteConfirmed/{entity}/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string entity, string id)
        {
            var metadata = _metadataService.GetEntityMetadata(entity);
            if (metadata == null) return EntityNotFound(entity);

            var key = _crudService.ConvertKey(metadata, id);
            await _crudService.DeleteAsync(metadata, key);

            TempData["Success"] = $"{metadata.EntityName} deleted successfully.";
            return RedirectToAction("Index", new { entity = metadata.EntityName });
        }

        // ---------- helpers ----------

        private async Task<CrudFormViewModel> BuildFormViewModel(
            EntityMetadata metadata,
            bool isEdit,
            string? id,
            IFormCollection? values,
            object? sourceEntity = null)
        {
            var vm = new CrudFormViewModel
            {
                EntityName = metadata.EntityName,
                Metadata = metadata,
                IsEdit = isEdit,
                Id = id
            };

            foreach (var prop in metadata.Properties)
            {
                string? current = null;

                if (values != null && values.ContainsKey(prop.Name))
                {
                    current = values[prop.Name].ToString();
                }
                else if (sourceEntity != null)
                {
                    var raw = metadata.ClrType.GetProperty(prop.Name)!.GetValue(sourceEntity);
                    current = raw switch
                    {
                        null => null,
                        DateTime dt => dt.ToString("yyyy-MM-dd"),
                        DateOnly d => d.ToString("yyyy-MM-dd"),
                        TimeOnly t => t.ToString("HH:mm"),
                        bool b => b.ToString().ToLowerInvariant(),
                        _ => raw.ToString()
                    };
                }

                vm.Values[prop.Name] = current;

                if (prop.IsForeignKey && prop.ForeignKey != null)
                {
                    vm.DropdownOptions[prop.Name] = await _dropdownService.GetOptionsAsync(prop.ForeignKey);
                }
            }

            return vm;
        }

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
