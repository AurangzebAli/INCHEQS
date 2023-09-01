
(function () {
    $(document).ready(function () {
        var $holder = $("#ajaxTransactionTypeSelectList");
        if ($holder.length > 0) {
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/TransactionTypeList",
                method: "POST",
                success: function (data) {
                    $.each(data, function (i, item) {
                        $holder.append("<option value=" + item.fldTransactionType + ">" + item.fldTransactionDesc + "</option>");
                    });
                }
            });
        }
    });
})();