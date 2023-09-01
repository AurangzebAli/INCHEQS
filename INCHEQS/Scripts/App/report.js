(function () {
    $(document).ready(function () {
        $(document).on("submit", "#reportParentForm", function () {
            console.log('check generate');
            $(".hidden-report-field").empty();
            var dataArray = $("input, select").not("[name='row_'],[name='reportType']").serializeArray();
            $(dataArray).each(function (i, field) {
                if (field.name != "reportTitle" && field.name != "bankDesc" && field.name != "StatusUpi")
                {
                    $('<input type="hidden" name="' + field.name + '" value = "' + field.value + '" />').appendTo($(".hidden-report-field"))
                }
            });
        })
    })

})();