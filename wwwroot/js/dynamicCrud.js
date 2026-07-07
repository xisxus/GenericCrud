(function () {
    // ---------- conditional field show/hide ----------
    function initConditionalFields() {
        var configEl = document.getElementById('conditional-config');
        if (!configEl) return;

        var rules;
        try {
            rules = JSON.parse(configEl.value || '[]');
        } catch (e) {
            rules = [];
        }
        if (!rules.length) return;

        function getFieldValue(name) {
            var els = document.getElementsByName(name);
            if (!els.length) return '';
            if (els.length === 1) {
                var el = els[0];
                if (el.type === 'checkbox') return el.checked ? 'true' : 'false';
                return el.value;
            }
            // radio group
            for (var i = 0; i < els.length; i++) {
                if (els[i].checked) return els[i].value;
            }
            return '';
        }

        function applyRules() {
            rules.forEach(function (rule) {
                var wrapper = document.querySelector('[data-field-name="' + rule.field + '"]');
                if (!wrapper) return;
                var currentValue = getFieldValue(rule.on);
                var match = (currentValue || '').toLowerCase() === (rule.value || '').toLowerCase();
                wrapper.classList.toggle('d-none', !match);
            });
        }

        var triggerNames = rules.map(function (r) { return r.on; });
        triggerNames.forEach(function (name) {
            var els = document.getElementsByName(name);
            for (var i = 0; i < els.length; i++) {
                els[i].addEventListener('change', applyRules);
                els[i].addEventListener('input', applyRules);
            }
        });

        applyRules();
    }

    // ---------- delete confirm modal ----------
    function initDeleteModal() {
        var modalEl = document.getElementById('dynDeleteModal');
        if (!modalEl || typeof bootstrap === 'undefined') return;

        var modal = new bootstrap.Modal(modalEl);
        var confirmBtn = document.getElementById('dynDeleteConfirmBtn');
        var pendingEntity = null;
        var pendingId = null;
        var pendingRow = null;

        document.querySelectorAll('.dyn-delete-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                pendingEntity = btn.getAttribute('data-entity');
                pendingId = btn.getAttribute('data-id');
                pendingRow = btn.closest('tr');
                modal.show();
            });
        });

        if (confirmBtn) {
            confirmBtn.addEventListener('click', function () {
                if (!pendingEntity || !pendingId) return;

                var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
                var token = tokenEl ? tokenEl.value : '';

                fetch('/DynamicCrud/Delete/' + encodeURIComponent(pendingEntity) + '/' + encodeURIComponent(pendingId), {
                    method: 'POST',
                    headers: { 'RequestVerificationToken': token }
                })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        modal.hide();
                        if (data.isSuccess) {
                            if (pendingRow) pendingRow.remove();
                        } else {
                            alert(data.message || 'Delete failed.');
                        }
                    })
                    .catch(function () {
                        modal.hide();
                        alert('Delete failed.');
                    });
            });
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        initConditionalFields();
        initDeleteModal();
    });
})();
