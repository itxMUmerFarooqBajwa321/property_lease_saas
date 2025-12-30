// Minimal vanilla JS for UI interactions

// Sidebar offcanvas toggle (for mobile)
document.addEventListener('DOMContentLoaded', function() {
    const offcanvasToggle = document.querySelector('[data-bs-toggle="offcanvas"]');
    if (offcanvasToggle) {
        offcanvasToggle.addEventListener('click', function() {
            const offcanvas = new bootstrap.Offcanvas(document.getElementById('sidebarOffcanvas'));
            offcanvas.show();
        });
    }

    // Confirm delete modal (example for generic confirmations)
    const confirmDeleteBtns = document.querySelectorAll('[data-bs-toggle="modal"][data-bs-target="#confirmDeleteModal"]');
    confirmDeleteBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            // No preventDefault here; just show modal
        });
    });

    // Toast notifications (example trigger)
    const showToastBtn = document.getElementById('showToastBtn');
    if (showToastBtn) {
        showToastBtn.addEventListener('click', function() {
            const toast = new bootstrap.Toast(document.getElementById('liveToast'));
            toast.show();
        });
    }
});