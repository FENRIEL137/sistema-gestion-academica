// Site.js - Funciones globales para SIGA

// Esperar a que el DOM esté cargado
document.addEventListener('DOMContentLoaded', function () {

    // Inicializar tooltips de Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Inicializar popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-ocultar alertas después de 5 segundos
    setTimeout(function () {
        var alerts = document.querySelectorAll('.alert-dismissible');
        alerts.forEach(function (alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);

    // Confirmación de eliminación
    document.querySelectorAll('.btn-delete').forEach(function (button) {
        button.addEventListener('click', function (e) {
            if (!confirm('¿Está seguro de eliminar este registro? Esta acción no se puede deshacer.')) {
                e.preventDefault();
            }
        });
    });

    // Formatear números como moneda
    document.querySelectorAll('.currency').forEach(function (element) {
        var value = parseFloat(element.textContent);
        if (!isNaN(value)) {
            element.textContent = '$' + value.toFixed(2);
        }
    });

    // Envío automático de formularios al cambiar select
    document.querySelectorAll('.auto-submit').forEach(function (select) {
        select.addEventListener('change', function () {
            this.form.submit();
        });
    });
});

// Función para mostrar mensajes toast (notificaciones)
function showToast(message, type = 'info') {
    var toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    var toastId = 'toast-' + Date.now();
    var bgClass = type === 'success' ? 'bg-success' : type === 'error' ? 'bg-danger' : type === 'warning' ? 'bg-warning' : 'bg-info';
    var icon = type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : type === 'warning' ? 'exclamation-triangle' : 'info-circle';

    var toastHTML = `
        <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi bi-${icon}"></i> ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHTML);
    var toastElement = document.getElementById(toastId);
    var toast = new bootstrap.Toast(toastElement, { delay: 4000 });
    toast.show();

    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}

// Función para imprimir tabla
function printTable(tableId) {
    var table = document.getElementById(tableId);
    if (!table) return;

    var printWindow = window.open('', '', 'height=600,width=800');
    printWindow.document.write('<html><head><title>Imprimir</title>');
    printWindow.document.write('<link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css">');
    printWindow.document.write('</head><body>');
    printWindow.document.write('<div class="container mt-4">');
    printWindow.document.write(table.outerHTML);
    printWindow.document.write('</div></body></html>');
    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
    printWindow.close();
}

// Función para exportar tabla a CSV
function exportTableToCSV(tableId, filename) {
    var table = document.getElementById(tableId);
    if (!table) return;

    var rows = table.querySelectorAll('tr');
    var csv = [];

    rows.forEach(function (row) {
        var cols = row.querySelectorAll('td, th');
        var rowData = [];
        cols.forEach(function (col) {
            rowData.push('"' + col.innerText.replace(/"/g, '""') + '"');
        });
        csv.push(rowData.join(','));
    });

    var csvContent = csv.join('\n');
    var blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    var link = document.createElement('a');
    var url = URL.createObjectURL(blob);

    link.setAttribute('href', url);
    link.setAttribute('download', filename || 'export.csv');
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}