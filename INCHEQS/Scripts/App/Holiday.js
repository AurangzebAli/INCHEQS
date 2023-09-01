
(function () {

    $(document).ready(function () {

        $('#active').click(function () {
            if ($("#active").is(':checked')) {
                $("#active").val("Y");
            }
            else {
                $("#active").val("N");
            }
        });

        if ($("#recurrTypeVal").val() == "Year") {
            $("#Year").prop("checked", true)
           
        }

        $("#OneTime").prop("disabled", true)
        $("#Year").prop("disabled", true)
        $("#fldDate").prop("disabled", true)
        $("#fldDate").datepicker('disable');
        $("#fldYearDate").datepicker('disable');
        $("#fldYearDate").prop("disabled", true)
        $("#Recurring").prop("disabled", true)
        $("#Week").prop("disabled", true)
        $(".Days").prop("disabled", true)
    })

})();