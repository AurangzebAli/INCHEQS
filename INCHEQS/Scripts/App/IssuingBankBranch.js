(function () {
    $(document).ready(function () {

        var $holder = $("#ajaxIssuingBankBranchList");
        if ($holder.length > 0) {
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/IssuingBankBranch",
                method: "POST",
                success: function (data) {
                    $.each(data, function (i, item) {
                        $holder.append("<option value=" + item.fldBranchId + ">" + item.fldBranchId + " - " + item.fldBranchDesc + "</option>");
                    });
                }

            });
        }



        var $holder2 = $("#ajaxHostStatusList");
        if ($holder2.length > 0) {
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/HostStatusList",
                method: "POST",
                success: function (data) {
                    $.each(data, function (i, item) {
                        $holder2.append("<option value=" + item.fldbankhoststatuscode + ">" + item.fldbankhoststatuscode + " - " + item.fldbankhoststatusdesc + "</option>");
                    });
                }

            });
        }


        var $holder3 = $("#ajaxPresentingBankBranchList");
        if ($holder3.length > 0) {
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/PresentingBankBranch",
                method: "POST",
                success: function (data) {
                    $.each(data, function (i, item) {
                        $holder3.append("<option value=" + item.fldBranchId + ">" + item.fldBranchId + " - " + item.fldBranchDesc + "</option>");
                    });
                }

            });
        }


    });

})();