$(document).ready(function() {
    // Aseguramos que el menú siempre esté visible en pantallas grandes
    if ($(window).width() >= 992) {
        $('.navbar-collapse').addClass('show');
    }
    
    // Convertir los elementos del menú a dropdown de Bootstrap
    $('.top-menu > li.submenu').addClass('nav-item dropdown');
    $('.top-menu > li.submenu > a').addClass('nav-link dropdown-toggle').attr('data-bs-toggle', 'dropdown');
    $('.top-menu > li.submenu > ul').addClass('dropdown-menu');
    $('.top-menu > li.submenu > ul > li > a').addClass('dropdown-item');
    
    // Gestionar la clase activa
    let currentUrl = window.location.pathname;
    $('.top-menu .dropdown-item').each(function() {
        let menuItemUrl = $(this).attr('href');
        if (menuItemUrl && currentUrl.indexOf(menuItemUrl) !== -1) {
            $(this).addClass('active');
            $(this).closest('.dropdown').addClass('active');
        }
    });

    // Inicializar tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});