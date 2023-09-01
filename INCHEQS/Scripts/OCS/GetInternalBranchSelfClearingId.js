(function () {
    $(document).ready(function () {

        var ddlCapturingBranchCode = $('[name="fldCapturingBranchCode"]');
        var txtClearingBranchCode = $('[name="fldClearingBranchCode"]');
        BindonChangeEvent(ddlCapturingBranchCode, txtClearingBranchCode);
        LoadDetails(ddlCapturingBranchCode, txtClearingBranchCode);
    });

    function BindonChangeEvent(ddlCapturingBranchCode, txtClearingBranchCode) {
        $(ddlCapturingBranchCode).on("change", function (e) {
            if (ddlCapturingBranchCode.val() == "") {
                alert("Capturing Branch Cannot be Empty.");
            }
            else {
                LoadDetails(ddlCapturingBranchCode, txtClearingBranchCode);
            }
           
        });
    }

    function LoadDetails(ddlCapturingBranchCode, txtClearingBranchCode) {
        var branchid = ddlCapturingBranchCode.val();
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/GetSelfClearingBranchId",
            method: "POST",
            data: "strBranchId=" + branchid,
            success: function (data) {
                txtClearingBranchCode.val(data[0].fldClearingBranchId.trim())
                //$.each(data, function (i, item) {
                //    txtClearingBranchCode.text(item.fldClearingBranchId);
                //});
            }
        });

    }

})()
