
(function () {
    $(document).ready(function () {

        var $holder = $("#ajaxStateSelectList");
        if ($holder.length > 0) {
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/StateList",
                method: "POST",
                success: function (data) {
                    $.each(data, function (i, item) {
                        $holder.append("<option value=" + item.fldStatecode + item.fldStateDesc + ">" + item.fldStatecode + " - " + item.fldStateDesc + "</option>");
                    });
                }

            });
        }

    });

})();