(function () {
    $(document).ready(function () {

        var ddlbranchId = $('[name="branchId"]');
        var txtLoc = $('[name="location"]');
        var txtBank = $('[name="bankcode"]');
        var txtBranch = $('[name="branchcode"]');

        
        BindonChangeEvent(ddlbranchId, txtLoc, txtBank, txtBranch);
        $('[name="location"]').empty();
        $('[name="bankcode"]').empty();
        $('[name="branchcode"]').empty();

        LoadDetails(ddlbranchId, txtLoc, txtBank, txtBranch);    
    });

    function BindonChangeEvent(ddlbranchId, txtLoc, txtBank, txtBranch) {
        $(ddlbranchId).on("change", function (e) {
            LoadDetails(ddlbranchId, txtLoc, txtBank, txtBranch);
        });
    }

    function LoadDetails(ddlbranchId,txtLoc,txtBank,txtBranch) {
        var branchid = ddlbranchId.val();
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/GetBankBranchDetails",
            method: "POST",
            data: "strBranchId=" + branchid,
            success: function (data) {
                txtLoc.empty();
                txtBank.empty();
                txtBranch.empty();
                $.each(data, function (i, item) {
                    txtLoc.text(item.fldlocationdesc);
                    txtBank.text(item.fldbankdesc);
                    txtBranch.text(item.fldbranchdesc);
                });
            }
        });

    }

})()
