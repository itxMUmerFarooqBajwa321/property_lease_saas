// Enhanced Properties Page Functionality
document.addEventListener('DOMContentLoaded', function() {
    // Search functionality
    const searchInput = document.querySelector('input[placeholder="Search properties..."]');
    if (searchInput) {
        searchInput.addEventListener('input', function(e) {
            const searchTerm = e.target.value.toLowerCase();
            const rows = document.querySelectorAll('.table tbody tr');
            
            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(searchTerm) ? '' : 'none';
            });
        });
    }

    // Add loading state to buttons
    document.querySelectorAll('.btn').forEach(button => {
        button.addEventListener('click', function(e) {
            if (this.classList.contains('btn-primary')) {
                this.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';
                this.disabled = true;
            }
        });
    });

    // Confirmation for delete
    document.querySelectorAll('.btn-outline-danger').forEach(button => {
        button.addEventListener('click', function(e) {
            if (!confirm('Are you sure you want to delete this property? This action cannot be undone.')) {
                e.preventDefault();
            }
        });
    });

    // Add data-label attributes for mobile responsive table
    if (window.innerWidth <= 768) {
        const headers = document.querySelectorAll('.table thead th');
        const rows = document.querySelectorAll('.table tbody tr');
        
        headers.forEach((header, index) => {
            rows.forEach(row => {
                const cell = row.children[index];
                if (cell) {
                    cell.setAttribute('data-label', header.textContent.trim());
                }
            });
        });
    }
});