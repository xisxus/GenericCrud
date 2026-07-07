(function () {
    function getToken(formId) {
        var el = document.querySelector('#' + formId + ' input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function showAlert(message, type) {
        var box = document.getElementById('dynconfig-alert');
        if (!box) return;
        box.innerHTML = '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
            message + '<button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>';
    }

    function addOptionRow(value, text) {
        var container = document.getElementById('optionsRows');
        var row = document.createElement('div');
        row.className = 'row g-2 mb-1 option-row';
        row.innerHTML =
            '<div class="col-md-5"><input type="text" class="form-control form-control-sm" name="OptionValues" placeholder="Value" value="' +
            (value || '').replace(/"/g, '&quot;') + '"></div>' +
            '<div class="col-md-5"><input type="text" class="form-control form-control-sm" name="OptionTexts" placeholder="Text" value="' +
            (text || '').replace(/"/g, '&quot;') + '"></div>' +
            '<div class="col-md-2"><button type="button" class="btn btn-sm btn-outline-danger btn-remove-option"><i class="bi bi-x"></i></button></div>';
        container.appendChild(row);
        row.querySelector('.btn-remove-option').addEventListener('click', function () { row.remove(); });
    }

    function toggleTypeSections() {
        var type = document.getElementById('fld_InputType').value;
        var isChoice = type === 'Dropdown' || type === 'Radio';
        var isFile = type === 'File';
        document.getElementById('optionsSection').classList.toggle('d-none', !isChoice);
        document.getElementById('fileSection').classList.toggle('d-none', !isFile);
    }

    document.addEventListener('DOMContentLoaded', function () {
        if (typeof bootstrap === 'undefined') return;

        // ---------- section modal ----------
        var sectionModalEl = document.getElementById('sectionModal');
        var sectionModal = new bootstrap.Modal(sectionModalEl);
        document.getElementById('btnNewSection').addEventListener('click', function () { sectionModal.show(); });

        document.getElementById('sectionForm').addEventListener('submit', function (e) {
            e.preventDefault();
            var body = new URLSearchParams(new FormData(e.target));
            fetch('/DynamicConfig/SaveSection', {
                method: 'POST',
                headers: { 'RequestVerificationToken': getToken('sectionForm') },
                body: body
            })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.isSuccess) window.location.reload();
                    else { showAlert(data.message || 'Save failed.', 'danger'); sectionModal.hide(); }
                });
        });

        // ---------- field modal ----------
        var fieldModalEl = document.getElementById('fieldModal');
        var fieldModal = new bootstrap.Modal(fieldModalEl);
        var fieldForm = document.getElementById('fieldForm');

        function resetFieldForm() {
            fieldForm.reset();
            document.getElementById('fld_Id').value = 0;
            document.getElementById('optionsRows').innerHTML = '';
            document.getElementById('fieldModalTitle').textContent = 'New Field';
            toggleTypeSections();
        }

        document.getElementById('fld_InputType').addEventListener('change', toggleTypeSections);
        document.getElementById('btnAddOption').addEventListener('click', function () { addOptionRow('', ''); });

        document.getElementById('btnNewField').addEventListener('click', function () {
            resetFieldForm();
            fieldModal.show();
        });

        document.querySelectorAll('.btn-edit-field').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var d = JSON.parse(btn.getAttribute('data-field'));
                resetFieldForm();

                document.getElementById('fld_Id').value = d.Id;
                document.getElementById('fld_FieldName').value = d.FieldName;
                document.getElementById('fld_Label').value = d.Label;
                document.getElementById('fld_InputType').value = d.InputType;
                document.getElementById('fld_SectionId').value = d.DynamicFieldSectionId || '';
                document.getElementById('fld_DefaultValue').value = d.DefaultValue || '';
                document.getElementById('fld_FormOrder').value = d.FormOrder;
                document.getElementById('fld_TableOrder').value = d.TableOrder;
                document.getElementById('fld_ShowInForm').checked = d.ShowInForm;
                document.getElementById('fld_ShowInTable').checked = d.ShowInTable;
                document.getElementById('fld_IsRequired').checked = d.IsRequired;

                document.getElementById('fld_MinLength').value = d.MinLength ?? '';
                document.getElementById('fld_MaxLength').value = d.MaxLength ?? '';
                document.getElementById('fld_MinValue').value = d.MinValue ?? '';
                document.getElementById('fld_MaxValue').value = d.MaxValue ?? '';
                document.getElementById('fld_Pattern').value = d.Pattern || '';
                document.getElementById('fld_ErrorMessage').value = d.ErrorMessage || '';

                document.getElementById('fld_ForeignTableName').value = d.ForeignTableName || '';
                document.getElementById('fld_ValueColumn').value = d.ValueColumn || 'Id';
                document.getElementById('fld_TextColumn').value = d.TextColumn || '';
                document.getElementById('fld_OrderByColumn').value = d.OrderByColumn || '';

                document.getElementById('fld_SaveFolder').value = d.SaveFolder || 'uploads';
                document.getElementById('fld_AllowedExtensions').value = d.AllowedExtensions || '.jpg,.jpeg,.png,.pdf';
                document.getElementById('fld_MaxSizeKb').value = d.MaxSizeKb || 2048;
                document.getElementById('fld_RenameToGuid').checked = d.RenameToGuid !== false;

                document.getElementById('fld_ConditionalOnFieldName').value = d.ConditionalOnFieldName || '';
                document.getElementById('fld_ConditionalOnValue').value = d.ConditionalOnValue || '';

                (d.Options || []).forEach(function (o) { addOptionRow(o.OptionValue, o.OptionText); });

                document.getElementById('fieldModalTitle').textContent = 'Edit Field';
                toggleTypeSections();
                fieldModal.show();
            });
        });

        fieldForm.addEventListener('submit', function (e) {
            e.preventDefault();
            var body = new URLSearchParams(new FormData(fieldForm));
            fetch('/DynamicConfig/SaveField', {
                method: 'POST',
                headers: { 'RequestVerificationToken': getToken('fieldForm') },
                body: body
            })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.isSuccess) window.location.reload();
                    else { showAlert(data.message || 'Save failed.', 'danger'); fieldModal.hide(); }
                })
                .catch(function () { showAlert('Save failed.', 'danger'); fieldModal.hide(); });
        });

        // ---------- delete field ----------
        var deleteModal = new bootstrap.Modal(document.getElementById('deleteFieldModal'));
        var pendingId = null;

        document.querySelectorAll('.btn-delete-field').forEach(function (btn) {
            btn.addEventListener('click', function () {
                pendingId = btn.getAttribute('data-id');
                document.getElementById('deleteFieldName').textContent = btn.getAttribute('data-name');
                deleteModal.show();
            });
        });

        document.getElementById('confirmDeleteFieldBtn').addEventListener('click', function () {
            if (!pendingId) return;
            fetch('/DynamicConfig/DeleteField/' + pendingId, {
                method: 'POST',
                headers: { 'RequestVerificationToken': getToken('fieldForm') }
            })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    deleteModal.hide();
                    if (data.isSuccess) window.location.reload();
                    else showAlert(data.message || 'Delete failed.', 'danger');
                });
        });
    });
})();
