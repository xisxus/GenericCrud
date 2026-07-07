using GenericCrud.Data;
using GenericCrud.Models.Dynamic;
using GenericCrud.Services;
using GenericCrud.ViewModels.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenericCrud.Controllers
{
    // Route: /DynamicConfig — admin UI that manages the DynamicEntity/DynamicField* config tables
    // consumed by DynamicMetadataService + DynamicCrudService.
    [Route("DynamicConfig")]
    public class DynamicConfigController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDynamicMetadataService _metadataService;

        public DynamicConfigController(AppDbContext context, IDynamicMetadataService metadataService)
        {
            _context = context;
            _metadataService = metadataService;
        }

        // GET /DynamicConfig
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var entities = await _context.DynamicEntities.OrderBy(e => e.PageTitle).ToListAsync();
            return View(entities);
        }

        // POST /DynamicConfig/SaveEntity
        [HttpPost("SaveEntity")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEntity(DynamicEntity input)
        {
            if (string.IsNullOrWhiteSpace(input.EntityName) || string.IsNullOrWhiteSpace(input.PageTitle))
                return Json(new { isSuccess = false, message = "Entity name and page title are required." });

            input.EntityName = input.EntityName.Trim();

            try
            {
                if (input.Id > 0)
                {
                    var existing = await _context.DynamicEntities.FindAsync(input.Id);
                    if (existing == null) return Json(new { isSuccess = false, message = "Entity not found." });

                    var oldName = existing.EntityName;
                    existing.EntityName = input.EntityName;
                    existing.PageTitle = input.PageTitle;
                    existing.PrimaryKeyColumn = string.IsNullOrWhiteSpace(input.PrimaryKeyColumn) ? "Id" : input.PrimaryKeyColumn.Trim();
                    existing.SoftDelete = input.SoftDelete;
                    existing.SoftDeleteColumn = string.IsNullOrWhiteSpace(input.SoftDeleteColumn) ? "IsDeleted" : input.SoftDeleteColumn.Trim();
                    existing.DefaultSortColumn = input.DefaultSortColumn;
                    existing.DefaultSortDirection = string.IsNullOrWhiteSpace(input.DefaultSortDirection) ? "ASC" : input.DefaultSortDirection;
                    existing.PageCode = input.PageCode;
                    existing.PageSize = input.PageSize <= 0 ? 10 : input.PageSize;
                    existing.IsActive = input.IsActive;

                    await _context.SaveChangesAsync();
                    _metadataService.InvalidateCache(oldName);
                    _metadataService.InvalidateCache(existing.EntityName);
                }
                else
                {
                    input.Id = 0;
                    input.CreatedAt = DateTime.UtcNow;
                    if (string.IsNullOrWhiteSpace(input.PrimaryKeyColumn)) input.PrimaryKeyColumn = "Id";
                    if (string.IsNullOrWhiteSpace(input.SoftDeleteColumn)) input.SoftDeleteColumn = "IsDeleted";
                    if (string.IsNullOrWhiteSpace(input.DefaultSortDirection)) input.DefaultSortDirection = "ASC";
                    if (input.PageSize <= 0) input.PageSize = 10;

                    _context.DynamicEntities.Add(input);
                    await _context.SaveChangesAsync();
                }

                _metadataService.InvalidateAllCache();
                return Json(new { isSuccess = true });
            }
            catch (DbUpdateException)
            {
                return Json(new
                {
                    isSuccess = false,
                    message = $"Could not save '{input.EntityName}'. Make sure the name is unique and the physical table already exists in the database."
                });
            }
        }

        // POST /DynamicConfig/DeleteEntity/5
        [HttpPost("DeleteEntity/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEntity(int id)
        {
            var entity = await _context.DynamicEntities.FindAsync(id);
            if (entity == null) return Json(new { isSuccess = false, message = "Entity not found." });

            var name = entity.EntityName;
            _context.DynamicEntities.Remove(entity); // cascades to fields/sections/sub-config rows
            await _context.SaveChangesAsync();

            _metadataService.InvalidateCache(name);
            _metadataService.InvalidateAllCache();
            return Json(new { isSuccess = true });
        }

        // GET /DynamicConfig/Fields/5
        [HttpGet("Fields/{entityId:int}")]
        public async Task<IActionResult> Fields(int entityId)
        {
            var entity = await _context.DynamicEntities
                .Include(e => e.Sections)
                .Include(e => e.Fields).ThenInclude(f => f.Validation)
                .Include(e => e.Fields).ThenInclude(f => f.Options)
                .Include(e => e.Fields).ThenInclude(f => f.ForeignKey)
                .Include(e => e.Fields).ThenInclude(f => f.FileConfig)
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.Id == entityId);

            if (entity == null) return NotFound($"Dynamic entity #{entityId} was not found.");

            return View(entity);
        }

        // POST /DynamicConfig/SaveSection
        [HttpPost("SaveSection")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSection(int dynamicEntityId, string title, int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Json(new { isSuccess = false, message = "Section title is required." });

            var section = new DynamicFieldSection
            {
                DynamicEntityId = dynamicEntityId,
                Title = title.Trim(),
                SortOrder = sortOrder
            };
            _context.DynamicFieldSections.Add(section);
            await _context.SaveChangesAsync();

            var entityName = (await _context.DynamicEntities.FindAsync(dynamicEntityId))?.EntityName;
            if (entityName != null) _metadataService.InvalidateCache(entityName);

            return Json(new { isSuccess = true, id = section.Id, title = section.Title });
        }

        // POST /DynamicConfig/SaveField
        [HttpPost("SaveField")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveField(DynamicFieldInputModel input)
        {
            if (string.IsNullOrWhiteSpace(input.FieldName) || string.IsNullOrWhiteSpace(input.Label))
                return Json(new { isSuccess = false, message = "Field name and label are required." });

            DynamicField? field;
            if (input.Id > 0)
            {
                field = await _context.DynamicFields
                    .Include(f => f.Validation)
                    .Include(f => f.Options)
                    .Include(f => f.ForeignKey)
                    .Include(f => f.FileConfig)
                    .FirstOrDefaultAsync(f => f.Id == input.Id);

                if (field == null) return Json(new { isSuccess = false, message = "Field not found." });
            }
            else
            {
                field = new DynamicField { DynamicEntityId = input.DynamicEntityId };
                _context.DynamicFields.Add(field);
            }

            field.FieldName = input.FieldName.Trim();
            field.Label = input.Label.Trim();
            field.InputType = input.InputType;
            field.FormOrder = input.FormOrder;
            field.TableOrder = input.TableOrder;
            field.ShowInForm = input.ShowInForm;
            field.ShowInTable = input.ShowInTable;
            field.IsRequired = input.IsRequired;
            field.DefaultValue = input.DefaultValue;
            field.DynamicFieldSectionId = input.DynamicFieldSectionId;
            field.ConditionalOnFieldName = string.IsNullOrWhiteSpace(input.ConditionalOnFieldName) ? null : input.ConditionalOnFieldName;
            field.ConditionalOnValue = string.IsNullOrWhiteSpace(input.ConditionalOnValue) ? null : input.ConditionalOnValue;

            // ---- validation sub-config ----
            var hasValidation = input.MinLength != null || input.MaxLength != null || input.MinValue != null ||
                                 input.MaxValue != null || !string.IsNullOrWhiteSpace(input.Pattern) ||
                                 input.MaxFileSizeKb != null || !string.IsNullOrWhiteSpace(input.ErrorMessage);

            if (hasValidation)
            {
                field.Validation ??= new DynamicFieldValidation();
                field.Validation.MinLength = input.MinLength;
                field.Validation.MaxLength = input.MaxLength;
                field.Validation.MinValue = input.MinValue;
                field.Validation.MaxValue = input.MaxValue;
                field.Validation.Pattern = input.Pattern;
                field.Validation.MaxFileSizeKb = input.MaxFileSizeKb;
                field.Validation.ErrorMessage = input.ErrorMessage;
            }
            else if (field.Validation != null)
            {
                _context.DynamicFieldValidations.Remove(field.Validation);
                field.Validation = null;
            }

            // ---- foreign key sub-config (dropdown/radio only) ----
            var isChoiceField = input.InputType == DynamicInputType.Dropdown || input.InputType == DynamicInputType.Radio;

            if (isChoiceField && !string.IsNullOrWhiteSpace(input.ForeignTableName))
            {
                field.ForeignKey ??= new DynamicForeignKey();
                field.ForeignKey.ForeignTableName = input.ForeignTableName.Trim();
                field.ForeignKey.ValueColumn = string.IsNullOrWhiteSpace(input.ValueColumn) ? "Id" : input.ValueColumn.Trim();
                field.ForeignKey.TextColumn = (input.TextColumn ?? string.Empty).Trim();
                field.ForeignKey.OrderByColumn = string.IsNullOrWhiteSpace(input.OrderByColumn) ? null : input.OrderByColumn.Trim();
            }
            else if (field.ForeignKey != null)
            {
                _context.DynamicForeignKeys.Remove(field.ForeignKey);
                field.ForeignKey = null;
            }

            // ---- file upload sub-config ----
            if (input.InputType == DynamicInputType.File)
            {
                field.FileConfig ??= new DynamicFileConfig();
                field.FileConfig.SaveFolder = string.IsNullOrWhiteSpace(input.SaveFolder) ? "uploads" : input.SaveFolder.Trim();
                field.FileConfig.AllowedExtensions = string.IsNullOrWhiteSpace(input.AllowedExtensions)
                    ? ".jpg,.jpeg,.png,.pdf" : input.AllowedExtensions.Trim();
                field.FileConfig.MaxSizeKb = input.MaxSizeKb ?? 2048;
                field.FileConfig.RenameToGuid = input.RenameToGuid;
            }
            else if (field.FileConfig != null)
            {
                _context.DynamicFileConfigs.Remove(field.FileConfig);
                field.FileConfig = null;
            }

            // ---- static options (dropdown/radio without an FK source) ----
            if (field.Options.Count > 0)
                _context.DynamicFieldOptions.RemoveRange(field.Options);

            var newOptions = new List<DynamicFieldOption>();
            if (isChoiceField && field.ForeignKey == null && input.OptionValues != null && input.OptionTexts != null)
            {
                var count = Math.Min(input.OptionValues.Count, input.OptionTexts.Count);
                for (var i = 0; i < count; i++)
                {
                    var optVal = input.OptionValues[i]?.Trim();
                    if (string.IsNullOrWhiteSpace(optVal)) continue;

                    var optText = input.OptionTexts[i]?.Trim();
                    newOptions.Add(new DynamicFieldOption
                    {
                        OptionValue = optVal,
                        OptionText = string.IsNullOrWhiteSpace(optText) ? optVal : optText,
                        SortOrder = i
                    });
                }
            }
            field.Options = newOptions;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Json(new { isSuccess = false, message = $"A field named '{field.FieldName}' already exists on this entity." });
            }

            var entityName = (await _context.DynamicEntities.FindAsync(field.DynamicEntityId))?.EntityName;
            if (entityName != null) _metadataService.InvalidateCache(entityName);

            return Json(new { isSuccess = true });
        }

        // POST /DynamicConfig/DeleteField/5
        [HttpPost("DeleteField/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteField(int id)
        {
            var field = await _context.DynamicFields.Include(f => f.DynamicEntity).FirstOrDefaultAsync(f => f.Id == id);
            if (field == null) return Json(new { isSuccess = false, message = "Field not found." });

            var entityName = field.DynamicEntity?.EntityName;
            _context.DynamicFields.Remove(field);
            await _context.SaveChangesAsync();

            if (entityName != null) _metadataService.InvalidateCache(entityName);
            return Json(new { isSuccess = true });
        }
    }
}
