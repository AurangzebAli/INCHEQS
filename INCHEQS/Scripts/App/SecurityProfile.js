
(function () {
    $(document).ready(function () {

        var isSystemCheck = $("input[name='fldUserAuthMethod']:checked").val();
            if (isSystemCheck == 'AD') {
                $("#fldUserPwdLengthMin").prop("disabled", true)
                $("#fldUserPwdLengthMax").prop("disabled", true)
                $("#fldUserPwdHistoryMax").prop("disabled", true)
                $("#fldUserPwdExpiry").prop("disabled", true)
                $("#fldUserPwdExpiryInt").prop("disabled", true)
                $("#fldUserPwdNotification").prop("disabled", true)
                $("#fldUserPwdNotificationInt").prop("disabled", true)
                $("#fldUserPwdExpAction").prop("disabled", true)
                $("#fldUserPwdExpAction2").prop("disabled", true)

                $("#fldUserPwdLengthMinredstar").css('visibility', 'hidden');
                $("#fldUserPwdLengthMaxredstar").css('visibility', 'hidden');
                $("#fldUserPwdHistoryMaxredstar").css('visibility', 'hidden');
                $("#fldUserPwdExpiryIntredstar").css('visibility', 'hidden');
                $("#fldUserPwdNotificationIntredstar").css('visibility', 'hidden');
                $("#fldUserPwdExpActionredstar").css('visibility', 'hidden');

            }
            else if (isSystemCheck == 'LP') {
                $("#fldUserADDomain").prop("disabled", true)
                $("#fldUserADDomainredstar").css('visibility', 'hidden');

            }
           

        $("input[name='fldUserAuthMethod']").change(function () {
            if (this.value == 'AD') {
                checkAD();
            } 
            else if (this.value == 'LP') {
                checkLP();
            }
    
        });

        function checkAD() {
            $("#fldUserPwdLengthMin").prop("disabled", true)
            $("#fldUserPwdLengthMax").prop("disabled", true)
            $("#fldUserPwdHistoryMax").prop("disabled", true)
            $("#fldUserPwdExpiry").prop("disabled", true)
            $("#fldUserPwdExpiryInt").prop("disabled", true)
            $("#fldUserPwdNotification").prop("disabled", true)
            $("#fldUserPwdNotificationInt").prop("disabled", true)
            $("#fldUserPwdExpAction").prop("disabled", true)
            $("#fldUserPwdExpAction2").prop("disabled", true)

            $("#fldUserADDomain").prop("disabled", false)

            $("#fldUserPwdLengthMinredstar").css('visibility', 'hidden');
            $("#fldUserPwdLengthMaxredstar").css('visibility', 'hidden');
            $("#fldUserPwdHistoryMaxredstar").css('visibility', 'hidden');
            $("#fldUserPwdExpiryIntredstar").css('visibility', 'hidden');
            $("#fldUserPwdNotificationIntredstar").css('visibility', 'hidden');
            $("#fldUserPwdExpActionredstar").css('visibility', 'hidden');
            $("#fldUserADDomainredstar").css('visibility', 'visible');
        }
        function checkLP() {
            $("#fldUserADDomain").prop("disabled", true)
            $("#fldUserPwdLengthMin").prop("disabled", false)
            $("#fldUserPwdLengthMax").prop("disabled", false)
            $("#fldUserPwdHistoryMax").prop("disabled", false)
            $("#fldUserPwdExpiry").prop("disabled", false)
            $("#fldUserPwdExpiryInt").prop("disabled", false)
            $("#fldUserPwdNotification").prop("disabled", false)
            $("#fldUserPwdNotificationInt").prop("disabled", false)
            $("#fldUserPwdExpAction").prop("disabled", false)
            $("#fldUserPwdExpAction2").prop("disabled", false)

            $("#fldUserADDomainredstar").css('visibility', 'hidden');

            $("#fldUserPwdLengthMinredstar").css('visibility', 'visible');
            $("#fldUserPwdLengthMaxredstar").css('visibility', 'visible');
            $("#fldUserPwdHistoryMaxredstar").css('visibility', 'visible');
            $("#fldUserPwdExpiryIntredstar").css('visibility', 'visible');
            $("#fldUserPwdNotificationIntredstar").css('visibility', 'visible');
            $("#fldUserPwdExpActionredstar").css('visibility', 'visible');
        }

    })

})();
(function validateQty(event) {
    var key = window.event ? event.keyCode : event.which;
    if (event.keyCode == 8 || event.keyCode == 46
     || event.keyCode == 37 || event.keyCode == 39) {
        return true;
    }
    else if (key < 48 || key > 57) {
        return false;
    }
    else return true;
})