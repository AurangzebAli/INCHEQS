(function () {
    $(document).ready(function () {
        $("#OneTime").attr("checked", true);
        $("#Week").prop("disabled", true)
        $(".Days").prop("disabled", true)
        $("#Month").prop("disabled", true)
        //$("#MonthRadio").prop("disabled", true)
        //$("#dropdownWeek option:selected").attr('disabled', 'disabled');
        $("#dropdownWeek").prop("disabled", true)
        $("#dropdownDay").prop("disabled", true)
        $("#Year").prop("disabled", true)
        $("#fldYearDate").datepicker('disable');
        $("#fldYearDate").prop("disabled", true)
        $('#active').click(function () {
            if ($("#active").is(':checked')) {
                $("#active").val("Y");
            }
            else {
                $("#active").val("N");
            }
        });
        function checkOneTime()
        {
            $("#Recurring").prop("checked", false);
            $("#OneTime").prop("checked", true);
            $("#Week").prop("disabled", true)
            $("#Week").prop("checked", false)
            $(".Days").prop("disabled", true)
            $(".Days").prop("checked", false)
            $("#Month").prop("disabled", true)
            $("#Month").prop("checked", false)
            $("#Year").prop("disabled", true)
            $("#Year").prop("checked", false)
            $("#fldDate").datepicker("option", "disabled", false);
            $("#fldYearDate").datepicker('disable');
            $("#fldYearDate").prop("disabled", true)
            $("#fldYearDate").prop("checked", false)
            $("#dropdownWeek").prop("disabled", true)
            $("#dropdownDay").prop("disabled", true)
 
        }

        function checkRecurring()
        {
            $("#OneTime").prop("checked", false);
            $("#Recurring").prop("checked", true);
            $("#Week").prop("disabled", false)
            $("#Month").prop("disabled", false)
            $("#Year").prop("disabled", false)
            $("#fldDate").datepicker('disable');
            $("#fldDate").prop("disabled", true)
            $("#fldYearDate").datepicker("option", "disabled", false);
        }

        function checkWeek()
        {
            $(".Days").prop("disabled", false)
            $("#Month").prop("checked", false)
            $("#Year").prop("checked", false)
            $("#fldYearDate").prop("disabled", true)
            $("#fldDate").prop("disabled", true)
            $("#dropdownWeek").prop("disabled", true)
            $("#dropdownDay").prop("disabled", true)
        }

        function checkMonth()
        {
            $(".Days").prop("disabled", true)
            $(".Days").prop("checked", false)
            $("#Week").prop("checked", false)
            $("#Year").prop("checked", false)
            $("#fldYearDate").prop("disabled", true)
            $("#fldDate").prop("disabled", true)
            $("#dropdownWeek").prop("disabled", false)
            $("#dropdownDay").prop("disabled", false)
        }

        function checkYear()
        {
            $(".Days").prop("disabled", true)
            $(".Days").prop("checked", false)
            $("#Week").prop("checked", false)
            $("#Month").prop("checked", false)
            $("#fldYearDate").prop("disabled", false)
            $("#fldDate").prop("disabled", true)
            $("#dropdownWeek").prop("disabled", true)
            $("#dropdownDay").prop("disabled", true)
        }

        $("#OneTime").click(function () {
            checkOneTime();
        });

        $("#Recurring").click(function () {
            checkRecurring();
        });

        $("#Week").click(function () {
            checkWeek();
        });

        $("#Month").click(function () {
            checkMonth();
        });

        $("#Year").click(function () {
            checkYear();
        });

        if ($("#Week").val() == "W")
        {
            $("#Week").prop("checked", true)
            $("#Week").val("Week");
        }
        if ($("#Month").val() == "M")
        {
            $("#Month").prop("checked", true)
            $("#Month").val("Month");
        }
        if ($("#Year").val() == "Y")
        {
            $("#Year").prop("checked", true)
            $("#Year").val("Year");
        }
        if ($("#OneTime").val() == "Recurring")
        {
            $("#Recurring").prop("checked", true)
        }
    })
})();