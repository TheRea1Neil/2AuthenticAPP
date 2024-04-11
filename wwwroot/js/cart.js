console.log("cart.js loaded");

function removeFromCart(productId) {
    console.log("Removing product:", productId);
    $.ajax({
        url: "/Home/RemoveFromCart",
        type: "POST",
        data: { productId: productId },
        success: function (data, status, xhr) {
            console.log("Remove from cart success:", data, status, xhr);
            location.reload();
        },
        error: function (xhr, status, error) {
            console.log("Remove from cart error:", xhr.status, error);
        }
    });
}

function increaseQuantity(productId) {
    console.log("Increasing quantity for product:", productId);
    $.ajax({
        url: "/Home/IncreaseQuantity",
        type: "POST",
        data: { productId: productId },
        success: function (data, status, xhr) {
            console.log("Increase quantity success:", data, status, xhr);
            location.reload();
        },
        error: function (xhr, status, error) {
            console.log("Increase quantity error:", xhr.status, error);
        }
    });
}

function decreaseQuantity(productId) {
    console.log("Decreasing quantity for product:", productId);
    $.ajax({
        url: "/Home/DecreaseQuantity",
        type: "POST",
        data: { productId: productId },
        success: function (data, status, xhr) {
            console.log("Decrease quantity success:", data, status, xhr);
            location.reload();
        },
        error: function (xhr, status, error) {
            console.log("Decrease quantity error:", xhr.status, error);
        }
    });
}