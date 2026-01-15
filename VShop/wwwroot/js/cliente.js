// Funcionalidades específicas para el cliente

// Función para mostrar notificaciones
function showNotification(type, title, message) {
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        showDuration: "300",
        hideDuration: "1000",
        timeOut: "5000",
        extendedTimeOut: "1000",
        showEasing: "swing",
        hideEasing: "linear",
        showMethod: "fadeIn",
        hideMethod: "fadeOut"
    };

    switch (type) {
        case 'success':
            toastr.success(message, title);
            break;
        case 'error':
            toastr.error(message, title);
            break;
        case 'info':
            toastr.info(message, title);
            break;
        case 'warning':
            toastr.warning(message, title);
            break;
    }
}

// Funcionalidad de búsqueda con sugerencias
$(document).ready(function () {
    // Sugerencias de búsqueda
    $('#searchInput').on('input', function () {
        var query = $(this).val();
        if (query.length >= 2) {
            $.ajax({
                url: '/Productos/SearchSuggestions',
                type: 'GET',
                data: { query: query },
                success: function (suggestions) {
                    var container = $('#searchSuggestions');
                    container.empty();

                    if (suggestions.length > 0) {
                        suggestions.forEach(function (suggestion) {
                            container.append(
                                '<div class="search-suggestion-item" data-product-id="' + suggestion.id + '">' +
                                '<strong>' + suggestion.name + '</strong><br>' +
                                '<small class="text-muted">' + (suggestion.category || '') + '</small>' +
                                '</div>'
                            );
                        });
                        container.addClass('show');
                    } else {
                        container.removeClass('show');
                    }
                }
            });
        } else {
            $('#searchSuggestions').removeClass('show');
        }
    });

    // Cerrar sugerencias al hacer clic fuera
    $(document).click(function (e) {
        if (!$(e.target).closest('#searchInput, #searchSuggestions').length) {
            $('#searchSuggestions').removeClass('show');
        }
    });

    // Manejar clic en sugerencia
    $(document).on('click', '.search-suggestion-item', function () {
        var productId = $(this).data('product-id');
        window.location.href = '/Productos/Detalle/' + productId;
    });

    // Funcionalidad de lista de deseos
    $('.btn-wishlist').click(function (e) {
        e.preventDefault();
        var productId = $(this).data('product-id');
        var $icon = $(this).find('i');

        $.ajax({
            url: '/Wishlist/Toggle',
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                if (response.isInWishlist) {
                    $icon.removeClass('far').addClass('fas');
                    showNotification('success', 'Lista de deseos', 'Producto agregado a favoritos');
                } else {
                    $icon.removeClass('fas').addClass('far');
                    showNotification('info', 'Lista de deseos', 'Producto removido de favoritos');
                }
            }
        });
    });

    // Funcionalidad para compartir
    $('.btn-share').click(function (e) {
        e.preventDefault();
        var url = window.location.href;
        var title = document.title;

        if (navigator.share) {
            navigator.share({
                title: title,
                text: 'Mira este producto en VShop',
                url: url
            });
        } else {
            // Fallback para copiar al portapapeles
            navigator.clipboard.writeText(url).then(function () {
                showNotification('success', 'Compartir', 'Enlace copiado al portapapeles');
            });
        }
    });

    // Filtros responsivos
    $('#toggleFilters').click(function () {
        $('#filterSidebar').toggleClass('show');
    });

    // Contador de productos por página
    $('#itemsPerPage').change(function () {
        var itemsPerPage = $(this).val();
        var currentUrl = new URL(window.location.href);
        currentUrl.searchParams.set('registrosPorPagina', itemsPerPage);
        window.location.href = currentUrl.toString();
    });
});

// Funcionalidad del carrito
class ShoppingCart {
    constructor() {
        this.items = JSON.parse(localStorage.getItem('cart')) || [];
        this.updateCartCount();
    }

    addItem(product) {
        const existingItem = this.items.find(item => item.id === product.id);

        if (existingItem) {
            existingItem.quantity += product.quantity || 1;
        } else {
            this.items.push({
                id: product.id,
                name: product.name,
                price: product.price,
                image: product.image,
                quantity: product.quantity || 1
            });
        }

        this.save();
        this.updateCartCount();
        return true;
    }

    removeItem(productId) {
        this.items = this.items.filter(item => item.id !== productId);
        this.save();
        this.updateCartCount();
    }

    updateQuantity(productId, quantity) {
        const item = this.items.find(item => item.id === productId);
        if (item) {
            item.quantity = quantity;
            if (item.quantity <= 0) {
                this.removeItem(productId);
            } else {
                this.save();
            }
        }
    }

    getTotal() {
        return this.items.reduce((total, item) => total + (item.price * item.quantity), 0);
    }

    getItemCount() {
        return this.items.reduce((count, item) => count + item.quantity, 0);
    }

    save() {
        localStorage.setItem('cart', JSON.stringify(this.items));
    }

    updateCartCount() {
        const count = this.getItemCount();
        $('.fa-shopping-cart').next('.badge').text(count);
    }

    renderCart() {
        const cartItems = $('#cartItems');
        cartItems.empty();

        if (this.items.length === 0) {
            cartItems.html(`
                <div class="text-center py-5">
                    <i class="fas fa-shopping-cart fa-3x text-muted mb-3"></i>
                    <p class="text-muted">Tu carrito está vacío</p>
                </div>
            `);
            return;
        }

        this.items.forEach(item => {
            cartItems.append(`
                <div class="cart-item" data-product-id="${item.id}">
                    <div class="row align-items-center">
                        <div class="col-3">
                            <img src="${item.image}" class="img-fluid rounded" alt="${item.name}">
                        </div>
                        <div class="col-7">
                            <h6 class="mb-1">${item.name}</h6>
                            <div class="d-flex align-items-center">
                                <div class="quantity-control">
                                    <button class="quantity-btn btn-decrease">-</button>
                                    <input type="number" class="quantity-input" value="${item.quantity}" min="1">
                                    <button class="quantity-btn btn-increase">+</button>
                                </div>
                            </div>
                        </div>
                        <div class="col-2 text-end">
                            <button class="btn btn-sm btn-danger btn-remove">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            `);
        });

        // Actualizar totales
        this.updateCartTotals();
    }

    updateCartTotals() {
        const subtotal = this.getTotal();
        const shipping = subtotal > 500 ? 0 : 20;
        const total = subtotal + shipping;

        $('.cart-subtotal').text(`$${subtotal.toFixed(2)}`);
        $('.cart-shipping').text(subtotal > 500 ? 'Gratis' : `$${shipping.toFixed(2)}`);
        $('.cart-total').text(`$${total.toFixed(2)}`);
    }
}

// Inicializar el carrito
const cart = new ShoppingCart();

// Eventos del carrito
$(document).on('click', '.btn-add-to-cart', function (e) {
    e.preventDefault();

    const productId = $(this).data('product-id');
    const productName = $(this).data('product-name');
    const productPrice = $(this).data('product-price');
    const productImage = $(this).data('product-image');

    const added = cart.addItem({
        id: productId,
        name: productName,
        price: productPrice,
        image: productImage,
        quantity: 1
    });

    if (added) {
        showNotification('success', 'Carrito', 'Producto agregado al carrito');
        cart.renderCart();
    }
});

$(document).on('click', '.btn-remove', function () {
    const productId = $(this).closest('.cart-item').data('product-id');
    cart.removeItem(productId);
    cart.renderCart();
    showNotification('info', 'Carrito', 'Producto removido del carrito');
});

$(document).on('click', '.btn-decrease', function () {
    const input = $(this).siblings('.quantity-input');
    const newQuantity = parseInt(input.val()) - 1;
    if (newQuantity >= 1) {
        input.val(newQuantity);
        const productId = $(this).closest('.cart-item').data('product-id');
        cart.updateQuantity(productId, newQuantity);
        cart.renderCart();
    }
});

$(document).on('click', '.btn-increase', function () {
    const input = $(this).siblings('.quantity-input');
    const newQuantity = parseInt(input.val()) + 1;
    input.val(newQuantity);
    const productId = $(this).closest('.cart-item').data('product-id');
    cart.updateQuantity(productId, newQuantity);
    cart.renderCart();
});

// Renderizar carrito cuando se abre el offcanvas
$('#cartOffcanvas').on('show.bs.offcanvas', function () {
    cart.renderCart();
});

// Formatear precios automáticamente
function formatPrice(price) {
    return new Intl.NumberFormat('es-MX', {
        style: 'currency',
        currency: 'MXN',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(price);
}

// Aplicar formato a todos los precios en la página
$(document).ready(function () {
    // Función para formatear elementos con precios
    function formatAllPrices() {
        $('.product-price').each(function () {
            var $priceContainer = $(this);

            // Buscar elementos con precios
            $priceContainer.find('span').each(function () {
                var text = $(this).text().trim();
                var priceMatch = text.match(/(\d+\.?\d*)/);

                if (priceMatch) {
                    var price = parseFloat(priceMatch[1]);
                    var formattedPrice = formatPrice(price);
                    $(this).text(text.replace(priceMatch[0], formattedPrice));
                }
            });
        });
    }

    // Aplicar formato inicial
    formatAllPrices();

    // Actualizar precios en tiempo real si hay cambios
    $(document).on('DOMNodeInserted', function (e) {
        if ($(e.target).hasClass('product-price') ||
            $(e.target).find('.product-price').length) {
            setTimeout(formatAllPrices, 100);
        }
    });

    // Mejorar la visualización de los precios con descuento
    $('.product-price').each(function () {
        var $this = $(this);

        // Agregar clase especial a precios con descuento
        if ($this.find('.text-danger').length > 0) {
            $this.addClass('price-discount');
        }
    });

    // Mostrar tooltip con información de ahorro
    $('.price-discount').hover(
        function () {
            var $container = $(this);
            var regularPrice = $container.find('.text-decoration-line-through').text();
            var discountPrice = $container.find('.text-danger').text();

            // Extraer valores numéricos
            var regular = parseFloat(regularPrice.replace(/[^0-9.-]+/g, ""));
            var discount = parseFloat(discountPrice.replace(/[^0-9.-]+/g, ""));

            if (!isNaN(regular) && !isNaN(discount)) {
                var savings = regular - discount;
                var percentage = Math.round((savings / regular) * 100);

                // Crear tooltip personalizado
                $container.attr('title', `Ahorras ${formatPrice(savings)} (${percentage}% de descuento)`);

                // Inicializar tooltip de Bootstrap
                new bootstrap.Tooltip($container[0]);
            }
        },
        function () {
            // Eliminar tooltip al salir
            $(this).tooltip('dispose');
        }
    );

    // Efecto especial al agregar al carrito
    $('.btn-add-to-cart').click(function (e) {
        e.preventDefault();

        var $button = $(this);
        var productId = $button.data('product-id');
        var productName = $button.data('product-name');
        var productPrice = $button.data('product-price');
        var productImage = $button.data('product-image');

        // Efecto visual
        $button.addClass('adding');
        $button.html('<i class="fas fa-spinner fa-spin me-2"></i>Agregando...');

        // Simular proceso de agregar al carrito
        setTimeout(function () {
            // Agregar al carrito
            const added = cart.addItem({
                id: productId,
                name: productName,
                price: productPrice,
                image: productImage,
                quantity: 1
            });

            if (added) {
                // Efecto de confirmación
                $button.removeClass('adding').addClass('added');
                $button.html('<i class="fas fa-check me-2"></i>¡Agregado!');

                // Mostrar notificación
                showNotification('success', 'Carrito', `"${productName}" agregado al carrito`);

                // Restaurar botón después de 2 segundos
                setTimeout(function () {
                    $button.removeClass('added');
                    $button.html('<i class="fas fa-cart-plus me-2"></i>Agregar al Carrito');
                }, 2000);

                // Renderizar carrito si está abierto
                if ($('#cartOffcanvas').hasClass('show')) {
                    cart.renderCart();
                }
            }
        }, 800);
    });

    // Funcionalidad para compartir
    $('.btn-share').click(function (e) {
        e.preventDefault();
        var social = $(this).data('social');
        var url = window.location.href;
        var title = $('h1').text();

        var shareUrls = {
            facebook: 'https://www.facebook.com/sharer/sharer.php?u=' + encodeURIComponent(url),
            twitter: 'https://twitter.com/intent/tweet?url=' + encodeURIComponent(url) + '&text=' + encodeURIComponent(title),
            whatsapp: 'https://wa.me/?text=' + encodeURIComponent(title + ' - ' + url)
        };

        if (shareUrls[social]) {
            window.open(shareUrls[social], '_blank', 'width=600,height=400');
        }
    });

    // Actualizar estado de stock visualmente
    $('.badge.bg-success, .badge.bg-warning, .badge.bg-danger').each(function () {
        var $badge = $(this);
        var text = $badge.text().toLowerCase();

        if (text.includes('disponible') || text.includes('disponible')) {
            $badge.addClass('stock-available');
        } else if (text.includes('bajo') || text.includes('low')) {
            $badge.addClass('stock-low');
        } else if (text.includes('agotado') || text.includes('out')) {
            $badge.addClass('stock-out');
        }
    });
});