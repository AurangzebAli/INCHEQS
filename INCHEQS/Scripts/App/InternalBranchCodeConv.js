(function () {
    $(document).ready(function () {

        //branchCode
        var ddlbranchIdConv = $('[name="branchIdConv"]');
        //var txtLoc = $('[name="location"]');
        var txtBankConv = $('[name="fldBankCodeConv"]');
        var txtStateConv = $('[name="fldStateCodeConv"]');
        var txtBranchConv = $('[name="branchcodeConv"]');
        var tmpStateConv = $('[name="stateCodeConv"]');
        var tmpBankConv = $('[name="bankCodeConv"]');
               
        BindonChangeEventConv(ddlbranchIdConv, txtStateConv, txtBankConv, txtBranchConv, tmpStateConv, tmpBankConv);
        $('[name="fldStateCodeConv"]').empty();
        $('[name="fldBankCodeConv"]').empty();
        $('[name="branchcodeConv"]').empty();
        $('[name="stateCodeConv"]').empty();
        $('[name="bankCodeConv"]').empty();

        var tmpbranchConv = ddlbranchIdConv.val();
        if (tmpbranchConv != "") {
            LoadStateDetailsConv(ddlbranchIdConv, txtStateConv);
            LoadBankDetailsConv(ddlbranchIdConv, txtBankConv);
        }   
    });

    function BindonChangeEventConv(ddlbranchIdConv, txtStateConv, txtBankConv, txtBranchConv, tmpStateConv, tmpBankConv) {
        $(ddlbranchIdConv).on("focusout", function (e) {
            //LoadLocDetails(ddlbranchId, txtLoc);
            LoadStateDetailsConv(ddlbranchIdConv, txtStateConv);
            LoadBankDetailsConv(ddlbranchIdConv, txtBankConv);
            LoadBranchDetailsConv(ddlbranchIdConv, txtBranchConv, tmpStateConv, tmpBankConv);
        });
    }

    function LoadStateDetailsConv(ddlbranchIdConv, txtStateConv) {
        var branchid = ddlbranchIdConv.val();
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/GetBranchStateCodeDetails",
            method: "POST",
            data: "strBranchId=" + branchid,
            success: function (data) {
                txtStateConv.val("");
                $.each(data, function (i, item) {
                    txtStateConv.val(item.fldStateDesc);
                });
            }
        });

    }

    function LoadBankDetailsConv(ddlbranchIdConv, txtBankConv) {
        var branchid = ddlbranchIdConv.val();
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/GetBranchBankCodeDetailsConv",
            method: "POST",
            data: "strBranchId=" + branchid,
            success: function (data) {
                txtBankConv.val("");
                $.each(data, function (i, item) {
                    txtBankConv.val(item.fldbankdesc);
                });
            }
        });

    }

    function LoadBranchDetailsConv(ddlbranchIdConv, txtBranchConv, tmpStateConv, tmpBankConv) {
        var branchid = ddlbranchIdConv.val();
        var tmpBrn = "";
        var varBnk = "";
        var varState = "";
        tmpStateConv.val("");
        tmpBankConv.val("");
        txtBranchConv.val("");
        if (branchid.length == 8) {
            varBnk = branchid.substring(0, 2).toString();
            varState = branchid.substring(2, 4).toString();
            tmpBrn = branchid.substring(5, 8).toString();
            tmpStateConv.val(varState);
            tmpBankConv.val(varBnk);
            txtBranchConv.val(tmpBrn);
        }
    }

})()
