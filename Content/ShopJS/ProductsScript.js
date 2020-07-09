$(function () {

    /*Выбрать продукт из категории*/

    $("#SelectCategory").on("change", function () {
        var url = $(this).val();

        if (url) {
            window.location = "/admin/shop/Products?catId=" + url;
        }
        return false;
    });
    /*---------------------------------------------------------------*/

    /*Подтвердить удаление продукта*/

    $("a.delete").click(function () {
        if (!confirm("Confirm product deletion")) return false;
    });

    /*---------------------------------------------------------------*/
});