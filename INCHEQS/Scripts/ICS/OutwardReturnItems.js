(function () {
    $(document).ready(function () {

        $(document).on("click", ".BtnReadyforReturnICLDetail", function (event) {
            event.stopImmediatePropagation();
           
            var $row = $(this).closest("tr");    // Find the row
            var Data = $row.find('input:checkbox').val();
            jsReadyforReturnICLDetails(Data);
        });

        $(document).on("click", ".BtnReturnedICLDetail", function (event) {
            event.stopImmediatePropagation();
            var $row = $(this).closest("tr");    // Find the 
            //debugger
            var Data = $row.find('input:checkbox').val();
            //var CapturingBranch = $row.find("td:eq(1)").text().trim();
            //var Clearingbatch = $row.find("td:eq(2)").text().trim();
            //var CapturingDate = $row.find("td:eq(3)").text().trim();
            jsReturnedICLDetails(Data);
        });

        function jsReadyforReturnICLDetails(Data) {
            //var eTable = "<table class='table table-bordered' id='CenterClearedItemsDetailModal_rows'><thead><tr><th style='width: 200px;'>UIC</th><th style='width: 100px;'>Check Digit</th><th style='width: 100px;'>Type</th><th style='width: 100px;'>Location</th><th style='width: 100px;'>Issuing Bank</th><th style='width: 100px;'>Issuing Branch</th><th style='width: 100px;'>Cheque Number</th><th style='width: 180px;'>Issuer Account Number</th><th style='width: 100px;'>Amount</th><th style='width: 100px;'>Return Reason</th><th style='width: 200px;'>Return Desc</th></thead><th style='width: 150px;'>Return Remarks</th></tr><tbody>"
            var eTable = "<table class='table table-bordered' id='CenterClearedItemsDetailModal_rows'><thead><tr><th>No</th><th style='width: 200px;'>UIC</th><th style='width: 100px;'>Check Digit</th><th style='width: 100px;'>Type</th><th style='width: 100px;'>Location</th><th style='width: 100px;'>Issuing Bank</th><th style='width: 100px;'>Issuing Branch</th><th style='width: 100px;'>Cheque Number</th><th style='width: 150px;'>Issuer Account Number</th><th style='width: 100px;'>Amount</th><th style='width: 100px;'>Return Reason</th><th style='width: 100px;'>Return Desc</th><th style='width: 100px;'>Return Remarks</th></tr></thead><tbody>"
            var count = 1;
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/CenterItemReadyForReturnICL",
                method: "POST",
                data: "strSelectedRow=" + Data,
                success: function (response) {
                    $.each(response, function (i, item) {
                        
                        eTable += "<tr>";
                        eTable += "<td>" + count++ + "</td>";
                        eTable += "<td>" + item.fldUIC + "</td>";
                        eTable += "<td>" + item.fldIssueDigit + "</td>";
                        eTable += "<td>" + item.fldIssueChequeType + "</td>";
                        eTable += "<td>" + item.fldissuestatecode + "</td>";
                        eTable += "<td>" + item.fldIssueBankCode + "</td>";
                        eTable += "<td>" + item.fldIssueBranchCode + "</td>";
                        eTable += "<td>" + item.fldChequeSerialNo + "</td>";
                        eTable += "<td>" + item.fldHostAccountNo + "</td>";
                        eTable += "<td>" + numberWithCommas(item.fldAmount) + "</td>";
                        eTable += "<td>" + item.fldRejectCode + "</td>";
                        eTable += "<td>" + item.fldRejectDesc + "</td>";
                        eTable += "<td>" + item.fldExtRemarks + "</td>";
                        
                        eTable += "</tr>";

                        //eTable += "<tr>";
                        //eTable += "<td>" + item.fldUIC + "</td>";
                        //eTable += "<td>" + item.fldIssueDigit + "</td>";
                        //eTable += "<td>" + item.fldIssueChequeType + "</td>";
                        //eTable += "<td>" + item.fldChequeSerialNo + "</td>";
                        //eTable += "<td>" + item.fldHostAccountNo + "</td>";
                        //eTable += "<td>" + item.fldAmount + "</td>";
                        //eTable += "<td>" + item.fldRejectCode + "</td>";
                        //eTable += "<td style='width: 100px;'>" + item.fldRejectDesc + "</td>";
                        //eTable += "<td style='width: 100px;'>" + item.fldExtRemarks + "</td>";

                        //eTable += "</tr>";
                    });
                    eTable += "</tbody></table>";
                    $("#CenterClearedItemsDetailModal .modal-title").html("Item Ready For Outward Return ICL Details");
                    $("#CenterClearedItemsDetailModal_body").html(eTable);
                    $("#CenterClearedItemsDetailModal").modal();
                }
            });
        }
        function jsReturnedICLDetails(Data) {
            var eTable = "<table class='table table-bordered' id='CenterClearedItemsDetailModal_rows'><thead><tr><th>No</th><th style='width: 200px;'>UIC</th><th style='width: 100px;'>Check Digit</th><th style='width: 100px;'>Type</th><th style='width: 100px;'>Location</th><th style='width: 100px;'>Issuing Bank</th><th style='width: 100px;'>Issuing Branch</th><th style='width: 100px;'>Cheque Number</th><th style='width: 150px;'>Issuer Account Number</th><th style='width: 100px;'>Amount</th><th style='width: 100px;'>Return Reason</th><th style='width: 100px;'>Return Desc</th><th style='width: 100px;'>Return Remarks</th></tr></thead><tbody>"
            var count = 1;
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/CenterItemForReturnedICL",
                method: "POST",
                data: "strSelectedRow=" + Data,
                success: function (response) {
                    $.each(response, function (i, item) {
                        eTable += "<tr>";
                        eTable += "<td>" + count++ + "</td>";
                        eTable += "<td>" + item.fldUIC + "</td>";
                        eTable += "<td>" + item.fldIssueDigit + "</td>";
                        eTable += "<td>" + item.fldIssueChequeType + "</td>";
                        eTable += "<td>" + item.fldissuestatecode + "</td>";
                        eTable += "<td>" + item.fldIssueBankCode + "</td>";
                        eTable += "<td>" + item.fldIssueBranchCode + "</td>";
                        eTable += "<td>" + item.fldChequeSerialNo + "</td>";
                        eTable += "<td>" + item.fldHostAccountNo + "</td>";
                        eTable += "<td>" + numberWithCommas(item.fldAmount) + "</td>";
                        eTable += "<td>" + item.fldRejectCode + "</td>";
                        eTable += "<td>" + item.fldRejectDesc + "</td>";
                        eTable += "<td>" + item.fldExtRemarks + "</td>";

                        eTable += "</tr>";

                        //eTable += "<tr>";
                        //eTable += "<td>" + item.fldUIC + "</td>";
                        //eTable += "<td>" + item.fldIssueDigit + "</td>";
                        //eTable += "<td>" + item.fldIssueChequeType + "</td>";
                        //eTable += "<td>" + item.fldChequeSerialNo + "</td>";
                        //eTable += "<td>" + item.fldHostAccountNo + "</td>";
                        //eTable += "<td>" + item.fldAmount + "</td>";
                        //eTable += "<td>" + item.fldRejectCode + "</td>";
                        //eTable += "<td style='width: 100px;'>" + item.fldRejectDesc + "</td>";
                        //eTable += "<td style='width: 100px;'>" + item.fldExtRemarks + "</td>";

                        //eTable += "</tr>";
                    });
                    eTable += "</tbody></table>";
                    $("#CenterClearedItemsDetailModal .modal-title").html("Generated Items Detail");
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
        function numberWithCommas(x) {
            return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        }
    });
})();
