(function () {
    $(document).ready(function () {
        
        var ddlbankcode = $('[name="fldbankcode"]');
        var ddlBrances = $('[name="fldbranchcode"]');

        BindonChangeEvent(ddlbankcode, ddlBrances);
        $('[name="fldbranchcode"]').empty();
        $('[name="fldbranchcode"]').append('<option value="">ALL</option>');

        LoadDetails(ddlbankcode, ddlBrances);

    });

    function BindonChangeEvent(ddlbankcode, ddlBrances) {
        $(ddlbankcode).on("change", function (e) {
            LoadDetails(ddlbankcode, ddlBrances);
        });
    }

    function LoadDetails(ddlbankcode, ddlBrances) {
        var bankcode = ddlbankcode.val();
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/GetBankBranchesOcsWorkStationInformation",
            method: "POST",
            data: "strBankCode=" + bankcode,
            success: function (data) {
                ddlBrances.empty();
                ddlBrances.append('<option value="">ALL</option>');
                $.each(data, function (i, item) {
                    ddlBrances.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
            }
        });
    }
})()
