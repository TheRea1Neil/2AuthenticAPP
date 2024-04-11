$(document).ready(function () {
    // Add to Cart button click event
    $(".btn-add-to-cart").click(function () {
        var productId = $(this).data("product-id");
        addToCart(productId);
    });

    // Remove from Cart button click event
    $(".btn-remove").click(function () {
        var productId = $(this).data("product-id");
        removeFromCart(productId);
    });

    // Increase Quantity button click event
    $(".btn-increase-qty").click(function () {
        var productId = $(this).data("product-id");
        increaseQuantity(productId);
    });

    // Decrease Quantity button click event
    $(".btn-decrease-qty").click(function () {
        var productId = $(this).data("product-id");
        decreaseQuantity(productId);
    });
});

function addToCart(productId) {
    // Send an AJAX request to add the item to the cart
    $.ajax({
        url: "/Home/AddToCart",
        type: "POST",
        data: { productId: productId },
        success: function () {
            // Refresh the cart page or update the UI as needed
            location.reload();
        }
    });
}

function removeFromCart(productId) {
    // Send an AJAX request to remove the item from the cart
    $.ajax({
        url: "/Home/RemoveFromCart",
        type: "POST",
        data: { productId: productId },
        success: function () {
            // Refresh the cart page or update the UI as needed
            location.reload();
        }
    });
}

function increaseQuantity(productId) {
    // Send an AJAX request to increase the quantity
    $.ajax({
        url: "/Home/IncreaseQuantity",
        type: "POST",
        data: { productId: productId },
        success: function () {
            // Refresh the cart page or update the UI as needed
            location.reload();
        }
    });
}

function decreaseQuantity(productId) {
    // Send an AJAX request to decrease the quantity
    $.ajax({
        url: "/Home/DecreaseQuantity",
        type: "POST",
        data: { productId: productId },
        success: function () {
            // Refresh the cart page or update the UI as needed
            location.reload();
        }
    });
}