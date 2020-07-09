$(function () {

    /*Подтвердить удаление страницы*/

    $("a.delete").click(function () {
        if (!confirm("Confirm page deletion")) return false;
    });

    /*-------------------------------------------*/

    /*Скрипт для sorting*/

    $("table#pages tbody").sortable({
        items: "tr:not(.home)",
        placeholder: "ui-state-highlight",
        update: function () {
            var ids = $("table#pages tbody").sortable("serialize");
            var url = "/Admin/Pages/ReorderPages";

            $.post(url, ids, function (data) {
            });
        }

    });
});