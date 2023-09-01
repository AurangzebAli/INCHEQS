(function () {
    $(document).ready(function () {

        var $holder = $("#ajaxReturnReasonList");
        if ($holder.length > 0) {
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/ReturnReason",
                method: "POST",
                success: function (data) {
                    $.each(data, function (i, item) {
                        $holder.append("<option value=" + item.fldRejectCode + ">" + item.fldRejectCode + " - " + item.fldRejectDesc + "</option>");
                    });
                }

            });
        }

    });

})();