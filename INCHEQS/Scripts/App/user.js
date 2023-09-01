
(function () {
    $(document).ready(function () {

        var str = $('.secure-form').attr('action');
        var n = str.lastIndexOf('/');
        var result = str.substring(n + 1);

        $('#fldDisableLogin').click(function () {
            if ($("#fldDisableLogin").is(':checked')) {
                $("#fldDisableLogin").val("Y");
            }
            else {
                $("#fldDisableLogin").val("N");
            }
        });

        $('#fldZeroMaker').click(function () {
            if ($("#fldZeroMaker").is(':checked')) {
                $("#fldZeroMaker").val("Y");
            }
            else {
                $("#fldZeroMaker").val("N");
            }
        });
        //debugger;
        if ( result == "Update") {
            var isSystemCheck = $("input[name='userType']:checked").val();
            if (isSystemCheck == 'System') {
                $("#Passwordredstar").css('visibility', 'hidden');
                $("#ConfirmPasswordredstar").css('visibility', 'hidden');
                $("#fldPassword").prop("disabled", true)
                $("#fldConfirmPassword").prop("disabled", true)
                $("#chkboxPassword").prop("disabled", true)
                $("#fldDisableLogin").prop("disabled", true)
                $("#fldDisableLogin").prop("checked", false)
                $("input#Admin").prop("disabled", true)
                $("input#branch").prop("disabled", true)
                $("#fldBranchCode").prop("disabled", true)
                $("#fldVerificationClass").prop("disabled", true)
                $("#fldVerificationLimit").prop("disabled", true)
                $("#VerificationLimitredstar").css('visibility', 'hidden');
                $("#VerificationClassredstar").css('visibility', 'hidden');
            }
            else if (isSystemCheck == 'Admin') {
                //debugger;
                $("#fldBranchCode").prop("disabled", true )
                $("input#System").prop("disabled", true)
                $("#fldPassword").prop("disabled", true)
                $("#fldConfirmPassword").prop("disabled", true)
            }
            else if (isSystemCheck == 'Branch') {
                //debugger;
                $("#fldPassword").prop("disabled", true)
                $("#fldConfirmPassword").prop("disabled", true)
            }
            else {
                $("input#System").prop("disabled", true)
                $("#fldVerificationClass").prop("disabled", true)
                $("#VerificationClassredstar").css('visibility', 'hidden');
            }
        }

        if (result == "Update") {
            $("#fldPassword").prop("disabled", true)
            $("#fldConfirmPassword").prop("disabled", true)
            $("#editUserIdredstar").css('visibility', 'hidden');
        }
        else {
            $("#fldBranchCode").prop("disabled", true)
            $("#fldPassword").prop("disabled", true)
            $("#fldConfirmPassword").prop("disabled", true)
        }

        $('#chkboxPassword').click(function () {
            if ($("#chkboxPassword").is(':checked')) {
                $("#fldPassword").prop("disabled", false)
                $("#fldConfirmPassword").prop("disabled", false)
            }
            else {
                $("#fldPassword").prop("disabled", true)
                $("#fldConfirmPassword").prop("disabled", true)
            }
        });

        $("input[name='userType']").change(function () {
            if (this.value == 'branch') {
                $('#SelectOfficer').hide();
                $('#SelectVerifierBranch').hide();
                $("#fldVerificationClass").val("")
                checkBranch();
            } 
            else if (this.value == 'System') {
                checkSystem();
            }
            else if (this.value == 'Admin') {
                checkAdmin();
            }
        });

        function checkSystem() {
            $("select#fldBranchCode").prop('selectedIndex', 0);
            $("#System").prop("checked", true);
            $("#BranchCoderedstar").css('visibility', 'hidden');
            $("#Passwordredstar").css('visibility', 'hidden');
            $("#ConfirmPasswordredstar").css('visibility', 'hidden');
            if (result != "Update") {
                $("#fldDisableLogin").prop("disabled", true)
                $("#fldDisableLogin").prop("checked", false)
            }
            $("#fldBranchCode").prop("disabled", true)
            $("#chkboxPassword").prop("disabled", true)
            $("#fldPassword").prop("disabled", true)
            $("#fldConfirmPassword").prop("disabled", true)
            $("#fldVerificationClass").prop("disabled", true)
            $("#fldVerificationLimit").prop("disabled", true)
            $("#VerificationLimitredstar").css('visibility', 'hidden');
            $("#VerificationClassredstar").css('visibility', 'hidden');
            $("#chkboxPassword").prop("checked", false)
         
            if (result != "Update") {
                $("#fldPassword").val("")
                $("#fldConfirmPassword").val("")
            }
        }
        function checkAdmin() {
            $("select#fldBranchCode").prop('selectedIndex', 0);
            $("#Admin").prop("checked", true);
            $("#BranchCoderedstar").css('visibility', 'hidden');
            $("#Passwordredstar").css('visibility', 'visible');
            $("#ConfirmPasswordredstar").css('visibility', 'visible');
            $("#fldBranchCode").prop("disabled", true)
            if (result != "Update") {
                $("#fldDisableLogin").prop("disabled", false)
                $("#fldDisableLogin").prop("checked", false)
                $("#fldPassword").val("")
                $("#fldConfirmPassword").val("")
            }
          //  debugger;
            if (result == "Update" && isSystemCheck == 'Admin') {
                $("#fldVerificationClass").val(["fldVerificationClass"]);
            }
            $("#chkboxPassword").prop("disabled", false)
            $("#chkboxPassword").prop("checked", false)
            if (result == "Update") {
                $("#fldPassword").val("")
                $("#fldConfirmPassword").val("")
            } else {
                $("#fldPassword").prop("disabled", false)
                $("#fldConfirmPassword").prop("disabled", false)
            }
            $("#fldVerificationClass").prop("disabled", false)
            $("#fldVerificationLimit").prop("disabled", false)
            $("#VerificationLimitredstar").css('visibility', 'visible');
            $("#VerificationClassredstar").css('visibility', 'visible');
            if ($("#chkboxPassword").is(':checked')) {
                $("#chkboxPassword").val("Y");
            }
            else {
                $("#chkboxPassword").val("N");
            }
        }

        function checkBranch() {
            $("#BranchCoderedstar").css('visibility', 'visible');
            $("#Passwordredstar").css('visibility', 'visible');
            $("#ConfirmPasswordredstar").css('visibility', 'visible');
            $("#branch").prop("checked", true);
            $("#fldBranchCode").prop("disabled", false)
            if (result != "Update") {
                $("#fldDisableLogin").prop("disabled", false)
                $("#fldDisableLogin").prop("checked", false)
                $("#fldPassword").val("")
                $("#fldConfirmPassword").val("")
            }
            $("#chkboxPassword").prop("checked", false)
            $("#chkboxPassword").prop("disabled", false)
            if (result == "Update") {
                $("#fldPassword").val("")
                $("#fldConfirmPassword").val("")
                $("#fldVerificationClass").prop("disabled", true)

            } else {
                $("#fldPassword").prop("disabled", false)
                $("#fldConfirmPassword").prop("disabled", false)
            }
            $("#fldVerificationClass").prop("disabled", true)
            $("#fldVerificationLimit").prop("disabled", false)
            $("#VerificationLimitredstar").css('visibility', 'visible');
            $("#VerificationClassredstar").css('visibility', 'hidden');
            if ($("#chkboxPassword").is(':checked')) {
                $("#chkboxPassword").val("Y");
            }
            else {
                $("#chkboxPassword").val("N");
            }
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