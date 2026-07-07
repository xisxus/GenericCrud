using GenericCrud.Metadata.Dynamic;
using GenericCrud.Models.Dynamic;
using GenericCrud.Services;
using GenericCrud.ViewModels.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace GenericCrud.Controllers
{
    // Route: /DynamicCrud/{action}/{entity}[/{id}]
    // One controller drives list/create/edit/delete for every entity configured via /DynamicConfig.
    [Route("DynamicCrud")]
    public class DynamicCrudController : Controller
    {
        private readonly IDynamicMetadataService _metadataService;
        private readonly IDynamicCrudService _crudService;

        public DynamicCrudController(IDynamicMetadataService metadataService, IDynamicCrudService crudService)
        {
            _metadataService = metadataService;
            _crudService = crudService;
        }

        // GET /DynamicCrud/Index/ProductCategories
        [HttpGet("Index/{entity}")]
        public async Task<IActionResult> Index(string entity, string? search, string? sort, string? dir, int page = 1)
        {
            var metadata = await _metadataService.GetEntityMetadataAsync(entity);
            if (metadata == null) return EntityNotFound(entity);

            var (rows, total) = await _crudService.GetAllAsync(metadata, search, sort, dir, page, metadata.PageSize);

            var vm = new DynamicListViewModel
            {
                Metadata = metadata,
                Rows = rows,
                Search = search,
                SortColumn = sort,
                SortDirection = dir,
                Page = page < 1 ? 1 : page,
                TotalCount = total
            };

            foreach (var field in metadata.TableFields.Where(f => f.ForeignKey != null))
            {
                var options = await _crudService.GetForeignKeyOptionsAsync(field.ForeignKey!);
                vm.FkDisplay[field.FieldName] = options.ToDictionary(o => o.Value, o => o.Text);
            }

            ViewBag.EntityName = entity;
            return View(vm);
        }

        // GET /DynamicCrud/Create/ProductCategories
        [HttpGet("Create/{entity}")]
        public async Task<IActionResult> Create(string entity)
        {
            var metadata = await _metadataService.GetEntityMetadataAsync(entity);
            if (metadata == null) return EntityNotFound(entity);

            ViewBag.EntityName = entity;
            return View(await BuildFormViewModel(metadata, null));
        }

        // POST /DynamicCrud/Create/ProductCategories
        [HttpPost("Create/{entity}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string entity, IFormCollection form)
        {
            var metadata = await _metadataService.GetEntityMetadataAsync(entity);
            if (metadata == null) return EntityNotFound(entity);

            var errors = await _crudService.CreateAsync(metadata, form);
            if (errors.Count > 0)
            {
                var vm = await BuildFormViewModel(metadata, null);
                vm.Errors = errors;
                vm.SubmittedValues = metadata.FormFields
                    .Where(f => f.InputType != DynamicInputType.File)
                    .ToDictionary(f => f.FieldName, f => form[f.FieldName].ToString());

                ViewBag.EntityName = entity;
                return View(vm);
            }

            TempData["Success"] = $"{metadata.PageTitle} created successfully.";
            return RedirectToAction("Index", new { entity });
        }

        // GET /DynamicCrud/Edit/ProductCategories/5
        [HttpGet("Edit/{entity}/{id}")]
        public async Task<IActionResult> Edit(string entity, string id)
        {
            var metadata = await _metadataService.GetEntityMetadataAsync(entity);
            if (metadata == null) return EntityNotFound(entity);

            var row = await _crudService.GetByIdAsync(metadata, id);
            if (row == null) return NotFound($"{metadata.PageTitle} with id '{id}' was not found.");

            var vm = await BuildFormViewModel(metadata, row);
            vm.Id = id;

            ViewBag.EntityName = entity;
            return View(vm);
        }

        // POST /DynamicCrud/Edit/ProductCategories/5
        [HttpPost("Edit/{entity}/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string entity, string id, IFormCollection form)
        {
            var metadata = await _metadataService.GetEntityMetadataAsync(entity);
            if (metadata == null) return EntityNotFound(entity);

            var errors = await _crudService.UpdateAsync(metadata, id, form);
            if (errors.Count > 0)
            {
                var row = await _crudService.GetByIdAsync(metadata, id);
                var vm = await BuildFormViewModel(metadata, row);
                vm.Id = id;
                vm.Errors = errors;
                vm.SubmittedValues = metadata.FormFields
                    .Where(f => f.InputType != DynamicInputType.File)
                    .ToDictionary(f => f.FieldName, f => form[f.FieldName].ToString());

                ViewBag.EntityName = entity;
                return View(vm);
            }

            TempData["Success"] = $"{metadata.PageTitle} updated successfully.";
            return RedirectToAction("Index", new { entity });
        }

        // POST /DynamicCrud/Delete/ProductCategories/5  (AJAX)
        [HttpPost("Delete/{entity}/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string entity, string id)
        {
            var metadata = await _metadataService.GetEntityMetadataAsync(entity);
            if (metadata == null)
                return Json(new { isSuccess = false, message = $"Entity '{entity}' not found." });

            var ok = await _crudService.DeleteAsync(metadata, id);
            return Json(new
            {
                isSuccess = ok,
                message = ok ? $"{metadata.PageTitle} deleted successfully." : "Delete failed."
            });
        }

        private async Task<DynamicFormViewModel> BuildFormViewModel(DynamicEntityMetadata metadata, Dictionary<string, object?>? row)
        {
            var vm = new DynamicFormViewModel
            {
                Metadata = metadata,
                Values = row ?? new Dictionary<string, object?>()
            };

            foreach (var field in metadata.FormFields.Where(f => f.ForeignKey != null))
            {
                vm.FkOptions[field.FieldName] = await _crudService.GetForeignKeyOptionsAsync(field.ForeignKey!);
            }

            return vm;
        }

        private IActionResult EntityNotFound(string entity) =>
            NotFound($"Dynamic entity '{entity}' is not configured. Set it up first at /DynamicConfig.");
    }
}
