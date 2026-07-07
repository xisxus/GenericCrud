(function () {
    function getToken() {
        var el = document.querySelector('#entityForm input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function showAlert(message, type) {
        var box = document.getElementById('dynconfig-alert');
        if (!box) return;
        box.innerHTML = '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
            message + '<button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>';
    }

    document.addEventListener('DOMContentLoaded', function () {
        var modalEl = document.getElementById('entityModal');
        if (!modalEl || typeof bootstrap === 'undefined') return;
        var modal = new bootstrap.Modal(modalEl);
        var form = document.getElementById('entityForm');
        var tableSelect = document.getElementById('ent_EntityName');

        fetch('/DynamicConfig/GetTables')
            .then(function (r) { return r.json(); })
            .then(function (tables) {
                tables.forEach(function (t) {
                    var opt = document.createElement('option');
                    opt.value = t;
                    opt.textContent = t;
                    tableSelect.appendChild(opt);
                });
            });

        function resetForm() {
            form.reset();
            document.getElementById('ent_Id').value = 0;
            document.getElementById('ent_PrimaryKeyColumn').value = 'Id';
            document.getElementById('ent_SoftDeleteColumn').value = 'IsDeleted';
            document.getElementById('ent_PageSize').value = 10;
            document.getElementById('ent_DefaultSortDirection').value = 'ASC';
            document.getElementById('ent_IsActive').checked = true;
            document.getElementById('entityModalTitle').textContent = 'New Entity';
        }

        document.getElementById('btnNewEntity').addEventListener('click', function () {
            resetForm();
            modal.show();
        });

        document.querySelectorAll('.btn-edit-entity').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var data = JSON.parse(btn.getAttribute('data-entity'));
                resetForm();
                document.getElementById('ent_Id').value = data.Id;
                document.getElementById('ent_EntityName').value = data.EntityName;
                document.getElementById('ent_PageTitle').value = data.PageTitle;
                document.getElementById('ent_PrimaryKeyColumn').value = data.PrimaryKeyColumn;
                document.getElementById('ent_PageSize').value = data.PageSize;
                document.getElementById('ent_DefaultSortColumn').value = data.DefaultSortColumn || '';
                document.getElementById('ent_DefaultSortDirection').value = data.DefaultSortDirection;
                document.getElementById('ent_SoftDelete').checked = data.SoftDelete;
                document.getElementById('ent_SoftDeleteColumn').value = data.SoftDeleteColumn;
                document.getElementById('ent_IsActive').checked = data.IsActive;
                document.getElementById('entityModalTitle').textContent = 'Edit Entity';
                modal.show();
            });
        });

        form.addEventListener('submit', function (e) {
            e.preventDefault();
            var body = new URLSearchParams(new FormData(form));
            fetch('/DynamicConfig/SaveEntity', {
                method: 'POST',
                headers: { 'RequestVerificationToken': getToken() },
                body: body
            })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.isSuccess) {
                        window.location.reload();
                    } else {
                        showAlert(data.message || 'Save failed.', 'danger');
                        modal.hide();
                    }
                })
                .catch(function () {
                    showAlert('Save failed.', 'danger');
                    modal.hide();
                });
        });

        var deleteModalEl = document.getElementById('deleteEntityModal');
        var deleteModal = new bootstrap.Modal(deleteModalEl);
        var pendingDeleteId = null;

        document.querySelectorAll('.btn-delete-entity').forEach(function (btn) {
            btn.addEventListener('click', function () {
                pendingDeleteId = btn.getAttribute('data-id');
                document.getElementById('deleteEntityName').textContent = btn.getAttribute('data-name');
                deleteModal.show();
            });
        });

        document.getElementById('confirmDeleteEntityBtn').addEventListener('click', function () {
            if (!pendingDeleteId) return;
            fetch('/DynamicConfig/DeleteEntity/' + pendingDeleteId, {
                method: 'POST',
                headers: { 'RequestVerificationToken': getToken() }
            })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    deleteModal.hide();
                    if (data.isSuccess) {
                        window.location.reload();
                    } else {
                        showAlert(data.message || 'Delete failed.', 'danger');
                    }
                })
                .catch(function () {
                    deleteModal.hide();
                    showAlert('Delete failed.', 'danger');
                });
        });
    });
})();
