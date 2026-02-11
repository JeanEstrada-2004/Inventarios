/* ========================================
   SISTEMA DE INVENTARIO - FUNCIONES JS
   ======================================== */

// Esperar a que el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    initializeTooltips();
    initializeConfirmDialogs();
    initializeFormValidation();
    animateOnScroll();
});

/**
 * Inicializar tooltips
 */
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

/**
 * Diálogos de confirmación para acciones críticas
 */
function initializeConfirmDialogs() {
    // Confirmación para eliminar
    const deleteLinks = document.querySelectorAll('a[data-confirm="delete"]');
    deleteLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            if (!confirm('¿Estás seguro de que deseas eliminar este elemento?\nEsta acción no se puede deshacer.')) {
                e.preventDefault();
            }
        });
    });

    // Confirmación para devoluciones
    const returnButtons = document.querySelectorAll('button[data-confirm="return"]');
    returnButtons.forEach(btn => {
        btn.addEventListener('click', function(e) {
            if (!confirm('¿Confirmas que deseas solicitar la devolución de esta herramienta?')) {
                e.preventDefault();
            }
        });
    });
}

/**
 * Validación de formularios
 */
function initializeFormValidation() {
    const forms = document.querySelectorAll('form');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            // Validar campos requeridos
            const requiredFields = form.querySelectorAll('[required]');
            let isValid = true;

            requiredFields.forEach(field => {
                if (field.type === 'text' || field.type === 'email' || field.type === 'password') {
                    if (field.value.trim() === '') {
                        field.classList.add('is-invalid');
                        isValid = false;
                    } else {
                        field.classList.remove('is-invalid');
                    }
                }
            });

            if (!isValid) {
                e.preventDefault();
                showNotification('Por favor completa todos los campos requeridos', 'warning');
            }
        });

        // Remover clase de error cuando el usuario empieza a escribir
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.addEventListener('input', function() {
                this.classList.remove('is-invalid');
            });
        });
    });
}

/**
 * Animaciones al hacer scroll
 */
function animateOnScroll() {
    const elements = document.querySelectorAll('.card, .feature-box, .table');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.animation = 'fadeIn 0.6s ease forwards';
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    elements.forEach(el => observer.observe(el));
}

/**
 * Mostrar notificaciones
 */
function showNotification(message, type = 'info') {
    // Crear contenedor de notificaciones si no existe
    let notificationContainer = document.getElementById('notification-container');
    if (!notificationContainer) {
        notificationContainer = document.createElement('div');
        notificationContainer.id = 'notification-container';
        notificationContainer.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            max-width: 500px;
        `;
        document.body.appendChild(notificationContainer);
    }

    // Crear notificación
    const notification = document.createElement('div');
    notification.className = `alert alert-${type}`;
    notification.role = 'alert';
    notification.textContent = message;
    notification.style.cssText = `
        margin-bottom: 10px;
        animation: slideIn 0.3s ease;
    `;

    notificationContainer.appendChild(notification);

    // Remover después de 5 segundos
    setTimeout(() => {
        notification.style.animation = 'fadeOut 0.3s ease';
        setTimeout(() => notification.remove(), 300);
    }, 5000);
}

/**
 * Formatear cantidad disponible / total
 */
function formatStock(available, total) {
    const percentage = (available / total) * 100;
    let color = '#27ae60'; // Verde

    if (percentage < 25) {
        color = '#e74c3c'; // Rojo
    } else if (percentage < 50) {
        color = '#e67e22'; // Naranja
    } else if (percentage < 75) {
        color = '#f39c12'; // Amarillo
    }

    return `<span style="color: ${color}; font-weight: 600;">${available}/${total}</span>`;
}

/**
 * Buscar en tabla
 */
function filterTable(inputId, tableId) {
    const input = document.getElementById(inputId);
    const table = document.getElementById(tableId);
    
    if (!input || !table) return;

    input.addEventListener('keyup', function() {
        const filter = this.value.toUpperCase();
        const rows = table.querySelectorAll('tbody tr');

        rows.forEach(row => {
            const text = row.textContent.toUpperCase();
            row.style.display = text.includes(filter) ? '' : 'none';
        });
    });
}

/**
 * Exportar tabla a CSV
 */
function exportTableToCSV(tableId, filename = 'export.csv') {
    const table = document.getElementById(tableId);
    if (!table) return;

    let csv = [];
    const rows = table.querySelectorAll('tr');

    rows.forEach(row => {
        const cols = row.querySelectorAll('td, th');
        const rowData = [];
        cols.forEach(col => {
            rowData.push('"' + col.textContent.trim() + '"');
        });
        csv.push(rowData.join(','));
    });

    const csvContent = 'data:text/csv;charset=utf-8,' + csv.join('\n');
    const link = document.createElement('a');
    link.setAttribute('href', encodeURI(csvContent));
    link.setAttribute('download', filename);
    link.click();
}

/**
 * Imprimir tabla
 */
function printTable(tableId) {
    const table = document.getElementById(tableId);
    if (!table) return;

    const printWindow = window.open('', '', 'width=800,height=600');
    printWindow.document.write('<html><head><title>Inventario</title>');
    printWindow.document.write('<link rel="stylesheet" href="/css/site.css">');
    printWindow.document.write('</head><body>');
    printWindow.document.write(table.outerHTML);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    printWindow.print();
}

/**
 * Validar email
 */
function isValidEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

/**
 * Validar contraseña fuerte
 */
function isStrongPassword(password) {
    return password.length >= 6 && 
           /[a-z]/.test(password) && 
           /[0-9]/.test(password);
}

/**
 * Mostrar/ocultar contraseña
 */
function togglePasswordVisibility(inputId, buttonId) {
    const input = document.getElementById(inputId);
    const button = document.getElementById(buttonId);

    if (!input) return;

    button?.addEventListener('click', function() {
        if (input.type === 'password') {
            input.type = 'text';
            button.textContent = '👁️‍🗨️ Ocultar';
        } else {
            input.type = 'password';
            button.textContent = '👁️ Mostrar';
        }
    });
}

/**
 * Contador de caracteres para textareas
 */
function initCharCounter(textareaId, counterId, maxLength = 200) {
    const textarea = document.getElementById(textareaId);
    const counter = document.getElementById(counterId);

    if (!textarea || !counter) return;

    textarea.addEventListener('input', function() {
        const remaining = maxLength - this.value.length;
        counter.textContent = `${this.value.length}/${maxLength}`;
        
        if (remaining < 20) {
            counter.style.color = '#e74c3c';
        } else if (remaining < 50) {
            counter.style.color = '#f39c12';
        } else {
            counter.style.color = '#95a5a6';
        }
    });
}

/**
 * Formato de número con separadores
 */
function formatNumber(num) {
    return new Intl.NumberFormat('es-PE').format(num);
}

/**
 * Calcular tiempo transcurrido
 */
function timeAgo(date) {
    const now = new Date();
    const diffMs = now - new Date(date);
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Justo ahora';
    if (diffMins < 60) return `Hace ${diffMins} minuto(s)`;
    if (diffHours < 24) return `Hace ${diffHours} hora(s)`;
    if (diffDays < 7) return `Hace ${diffDays} día(s)`;
    
    return new Date(date).toLocaleDateString('es-PE');
}

/**
 * Mostrar loader
 */
function showLoader(message = 'Cargando...') {
    const loader = document.createElement('div');
    loader.id = 'global-loader';
    loader.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(26, 26, 46, 0.7);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 10000;
        flex-direction: column;
        gap: 20px;
    `;
    loader.innerHTML = `
        <div class="loading"></div>
        <p style="color: white; font-size: 1.1rem;">${message}</p>
    `;
    document.body.appendChild(loader);
}

/**
 * Ocultar loader
 */
function hideLoader() {
    const loader = document.getElementById('global-loader');
    if (loader) {
        loader.remove();
    }
}

/**
 * Copiar texto al portapapeles
 */
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showNotification('¡Copiado al portapapeles!', 'success');
    }).catch(() => {
        showNotification('Error al copiar', 'danger');
    });
}

/**
 * Debounce para búsquedas
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Validar formulario de préstamo
 */
function validateLoanForm() {
    const herramientaSelect = document.getElementById('HerramientaUnidadId');
    if (herramientaSelect && herramientaSelect.value === '') {
        showNotification('Debes seleccionar una herramienta', 'warning');
        return false;
    }
    return true;
}

/**
 * Actualizar disponibilidad en tiempo real
 */
function updateAvailability(herramientaId, disponible, total) {
    const element = document.querySelector(`[data-herramienta-id="${herramientaId}"]`);
    if (element) {
        element.innerHTML = formatStock(disponible, total);
    }
}
