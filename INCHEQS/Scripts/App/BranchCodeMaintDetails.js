(function () {
    $(document).ready(function () {

        //branchCode
        var ddlbranchId = $('[name="branchId"]');
        //var txtLoc = $('[name="location"]');
        var txtBank = $('[name="fldBankCode"]');
        var txtState = $('[name="fldStateCode"]');
        var txtBranch = $('[name="branchcode"]');
        var tmpState = $('[name="stateCode"]');
        var tmpBank = $('[name="bankCode"]');
               
        BindonChangeEvent(ddlbranchId, txtState, txtBank, txtBranch, tmpState, tmpBank);
        $('[name="fldStateCode"]').empty();
        $('[name="fldBankCode"]').empty();
        $('[name="branchcode"]').empty();
        $('[name="stateCode"]').empty();
        $('[name="bankCode"]').empty();

        var tmpbranch = ddlbranchId.val();
        if (tmpbranch != "") {
            LoadStateDetails(ddlbranchId, txtState);
            LoadBankDetails(ddlbranchId, txtBank);
        }
    });

    function BindonChangeEvent(ddlbranchId, txtState, txtBank, txtBranch, tmpState, tmpBank) {
        $(ddlbranchId).on("focusout", function (e) {
            //LoadLocDetails(ddlbranchId, txtLoc);
            LoadStateDetails(ddlbranchId, txtState);
            LoadBankDetails(ddlbranchId, txtBank);
            LoadBranchDetails(ddlbranchId, txtBranch, tmpState, tmpBank);
        });
    }

    function LoadStateDetails(ddlbranchId, txtState) {
        var branchid = ddlbranchId.val();
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/GetBranchStateCodeDetails",
            method: "POST",
            data: "strBranchId=" + branchid,
            success: function (data) {
                txtState.val("");
                $.each(data, function (i, item) {
                    txtState.val(item.fldStateDesc);
                });
            }
        });

    }

    function LoadBankDetails(ddlbranchId, txtBank) {
        var branchid = ddlbranchId.val();
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/GetBranchBankCodeDetails",
            method: "POST",
            data: "strBranchId=" + branchid,
            success: function (data) {
                txtBank.val("");
                $.each(data, function (i, item) {
                    txtBank.val(item.fldbankdesc);
                });
            }
        });

    }

    function LoadBranchDetails(ddlbranchId, txtBranch, tmpState, tmpBank) {
        var branchid = ddlbranchId.val();
        var tmpBrn = "";
        var varBnk = "";
        var varState = "";
        tmpState.val("");
        tmpBank.val("");
        txtBranch.val("");
        if (branchid.length == 8) {
            varBnk = branchid.substring(0, 2).toString();
            varState = branchid.substring(3, 5).toString();
            tmpBrn = branchid.substring(5, 8).toString();
            tmpState.val(varState);
            tmpBank.val(varBnk);
            txtBranch.val(tmpBrn);
        }
        
       

    }

})()
