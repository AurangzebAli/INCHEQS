(function () {
    $(document).ready(function () {

        $(document).on("click", ".BtnReadyforClearingBatchDetail", function (event) {
            event.stopImmediatePropagation();
            //debugger
            var $row = $(this).closest("tr");    // Find the row
            var Data = $row.find('input:checkbox').val();
            jsReadyforClearingDetails(Data);
        });

        $(document).on("click", ".BtnClearedBatchDetail", function (event) {
            event.stopImmediatePropagation();
            var $row = $(this).closest("tr");    // Find the 
            //debugger
            var CapturingBranch = $row.find("td:eq(1)").text().trim();
            var Clearingbatch = $row.find("td:eq(2)").text().trim();
            var CapturingDate = $row.find("td:eq(3)").text().trim();
            jsClearedDetails(CapturingBranch, CapturingDate, Clearingbatch);
        });

        function jsReadyforClearingDetails(Data) {
            var eTable = "<table id='CenterClearedItemsDetailModal_rows'><thead><tr><th style='width: 200px;'>UIC</th><th style='width: 100px;'>Check Digit</th><th style='width: 100px;'>Type</th><th style='width: 100px;'>Location</th><th style='width: 100px;'>Issuing Bank</th><th style='width: 100px;'>Issuing Branch</th><th style='width: 100px;'>Cheque No</th><th style='width: 180px;'>Issuer Account Number</th><th style='width: 100px;'>Amount</th><th style='width: 150px;'>Creditor Account Number</th></tr></thead><tbody>"
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/CenterItemReadyForClearing",
                method: "POST",
                data: "strSelectedRow=" + Data,
                success: function (response) {
                    $.each(response, function (i, item) {
                        eTable += "<tr>";
                        eTable += "<td>" + item.fldUIC + "</td>";
                        eTable += "<td>" + item.fldCheckDigit + "</td>";
                        eTable += "<td>" + item.fldType + "</td>";
                        eTable += "<td>" + item.fldStateCode + "</td>";
                        eTable += "<td>" + item.fldBankCode + "</td>";
                        eTable += "<td>" + item.fldBranchCode + "</td>";
                        eTable += "<td>" + item.fldSerial + "</td>";
                        eTable += "<td>" + item.fldIssuerAccNo + "</td>";
                        eTable += "<td>" + jsFormatAmount((item.fldAmount / 100).toFixed(2)) + "</td>";
                        eTable += "<td>" + item.fldPVaccNo + "</td>";
                        // eTable += "<td>" + item.fldItemID + "</td>";
                        //eTable += "<td>" + item.fldNonConformStatus + "</td>";
                        eTable += "</tr>";
                    });
                    eTable += "</tbody></table>";
                    $("#CenterClearedItemsDetailModal .modal-title").html("Item Ready For Clearing Details");
                    $("#CenterClearedItemsDetailModal_body").html(eTable);
                    $("#CenterClearedItemsDetailModal").modal();
                }
            });
        }
        function jsClearedDetails(CapturingBranch, CapturingDate, Clearingbatch) {
            var eTable = "<table id='CenterClearedItemsDetailModal_rows'><thead><tr><th style='width: 200px;'>UIC</th><th style='width: 100px;'>Check Digit</th><th style='width: 100px;'>Cheque No</th><th style='width: 100px;'>Bank Code</th><th style='width: 100px;'>Location</th><th style='width: 180px;'>Issuer Account Number</th><th style='width: 100px;'>Amount</th><th style='width: 150px;'>Payee Account Number</th></tr></thead><tbody>"
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/CenterClearedItems",
                method: "POST",
                data: "strCapturingBranch=" + CapturingBranch + "&strCapturingDate=" + CapturingDate + "&strClearingbatch=" + Clearingbatch,
                success: function (response) {
                    $.each(response, function (i, item) {
                        eTable += "<tr>";
                        eTable += "<td>" + item.fldUIC + "</td>";
                        eTable += "<td>" + item.fldCheckDigit + "</td>";
                        eTable += "<td>" + item.fldSerial + "</td>";
                        eTable += "<td>" + item.fldBankCode + "</td>";
                        eTable += "<td>" + item.fldStateCode + "</td>";
                        eTable += "<td>" + item.fldIssuerAccNo + "</td>";
                        eTable += "<td>" + jsFormatAmount((item.fldAmount / 100).toFixed(2)) + "</td>";
                        eTable += "<td>" + item.fldPVaccNo + "</td>";
                        //eTable += "<td>" + item.fldItemID + "</td>";
                        //eTable += "<td>" + item.fldNonConformStatus + "</td>";
                        eTable += "</tr>";
                    });
                    eTable += "</tbody></table>";
                    $("#CenterClearedItemsDetailModal .modal-title").html("Cleared Items Details");
                    $("#CenterClearedItemsDetailModal_body").html(eTable);
                    $("#CenterClearedItemsDetailModal").modal();
                }
            });
        }

        function jsFormatAmount(x) {
            var parts = x.toString().split(".");
            parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            return parts.join(".");
        }
    });
})();
