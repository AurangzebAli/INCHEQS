(function () {
    $(document).ready(function () {

        //branchCode
        var ddlbranchIdIslamic = $('[name="branchIdIslamic"]');
        //var txtLoc = $('[name="location"]');
        var txtBankIslamic = $('[name="fldBankCodeIslamic"]');
        var txtStateIslamic = $('[name="fldStateCodeIslamic"]');
        var txtBranchIslamic = $('[name="branchcodeIslamic"]');
        var tmpStateIslamic = $('[name="stateCodeIslamic"]');
        var tmpBankIslamic = $('[name="bankCodeIslamic"]');
               
        BindonChangeEventIslamic(ddlbranchIdIslamic, txtStateIslamic, txtBankIslamic, txtBranchIslamic, tmpStateIslamic, tmpBankIslamic);
        $('[name="fldStateCodeIslamic"]').empty();
        $('[name="fldBankCodeIslamic"]').empty();
        $('[name="branchcodeIslamic"]').empty();
        $('[name="stateCodeIslamic"]').empty();
        $('[name="bankCodeIslamic"]').empty();

        var tmpbranchIslamic = ddlbranchIdIslamic.val();
        if (tmpbranchIslamic != "") {
            LoadStateDetailsIslamic(ddlbranchIdIslamic, txtStateIslamic);
            LoadBankDetailsIslamic(ddlbranchIdIslamic, txtBankIslamic);
        }
    });

    function BindonChangeEventIslamic(ddlbranchIdIslamic, txtStateIslamic, txtBankIslamic, txtBranchIslamic, tmpStateIslamic, tmpBankIslamic) {
        $(ddlbranchIdIslamic).on("focusout", function (e) {
            //LoadLocDetails(ddlbranchId, txtLoc);
            LoadStateDetailsIslamic(ddlbranchIdIslamic, txtStateIslamic);
            LoadBankDetailsIslamic(ddlbranchIdIslamic, txtBankIslamic);
            LoadBranchDetailsIslamic(ddlbranchIdIslamic, txtBranchIslamic, tmpStateIslamic, tmpBankIslamic);
        });
    }

    function LoadStateDetailsIslamic(ddlbranchIdIslamic, txtStateIslamic) {
        var branchid = ddlbranchIdIslamic.val();
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/GetBranchStateCodeDetails",
            method: "POST",
            data: "strBranchId=" + branchid,
            success: function (data) {
                txtStateIslamic.val("");
                $.each(data, function (i, item) {
                    txtStateIslamic.val(item.fldStateDesc);
                });
            }
        });

    }

    function LoadBankDetailsIslamic(ddlbranchIdIslamic, txtBankIslamic) {
        var branchid = ddlbranchIdIslamic.val();
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/GetBranchBankCodeDetailsIslamic",
            method: "POST",
            data: "strBranchId=" + branchid,
            success: function (data) {
                txtBankIslamic.val("");
                $.each(data, function (i, item) {
                    txtBankIslamic.val(item.fldbankdesc);
                });
            }
        });

    }

    function LoadBranchDetailsIslamic(ddlbranchIdIslamic, txtBranchIslamic, tmpStateIslamic, tmpBankIslamic) {
        var branchid = ddlbranchIdIslamic.val();
        var tmpBrn = "";
        var varBnk = "";
        var varState = "";
        tmpStateIslamic.val("");
        tmpBankIslamic.val("");
        txtBranchIslamic.val("");
        if (branchid.length == 7) {
            varBnk = branchid.substring(0, 2).toString();
            varState = branchid.substring(2, 4).toString();
            tmpBrn = branchid.substring(4, 7).toString();
            tmpStateIslamic.val(varState);
            tmpBankIslamic.val(varBnk);
            txtBranchIslamic.val(tmpBrn);
        }
        
       

    }

})()
