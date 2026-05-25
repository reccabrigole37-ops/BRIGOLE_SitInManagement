// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(() => {
        document.querySelectorAll('.alert-toast').forEach(el => {
            const bsAlert = new bootstrap.Alert(el);
            bsAlert.close();
        });
    }, 5000);

    // Highlight active nav link
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.sidebar-nav .nav-link').forEach(link => {
        if (link.getAttribute('href') && path.includes(link.getAttribute('href').toLowerCase())) {
            link.classList.add('active');
        }
    });
});
