(function () {
    $(document).ready(function () {
        //Amount and Item Calculation for Items Ready for Submission
        var TotalItem = 0;
        var TotalAmount = 0;
        var table = $("#tblBranchSubmissionItemListing tbody");
        table.find('tr').each(function (i) {
            var $tds = $(this).find('td'),
                intItems = $tds.eq(6).text().replace(/[^0-9]/g, ''),
                intAmount = $tds.eq(7).text().replace(/[^0-9]/g, '');
            if (intItems > 0 && intAmount > 0) {
                TotalItem = parseInt(TotalItem) + parseInt(intItems);
                TotalAmount = parseInt(TotalAmount) + parseInt(intAmount);
                TotalAmount = jsFormatAmount((TotalAmount / 100).toFixed(2));
                $("#BSReadyTotalIem").text(TotalItem);
                $("#BSReadyTotalAmount").text(TotalAmount);
            }
            else {
                $("#BSReadyTotalIem").text("0");
                $("#BSReadyTotalAmount").text("0.00");
            }
        });
        //Amount and Item Calculation for Items Already Submitted
        var CheckSubmittedRowsCount = $('#tblBranchSubmittedListing tr').length;
        if (CheckSubmittedRowsCount > 0) {
            var table = $("#tblBranchSubmittedListing tbody");
            table.find('tr').each(function (i) {
                var $tds1 = $(this).find('td'),
                    intItems = $tds1.eq(6).text().replace(/[^0-9]/g, ''),
                    intAmount = $tds1.eq(7).text().replace(/[^0-9]/g, '');
                if (intItems > 0 && intAmount > 0) {
                    TotalItem = parseInt(TotalItem) + parseInt(intItems);
                    TotalAmount = parseInt(TotalAmount) + parseInt(intAmount);
                    TotalAmount = jsFormatAmount((TotalAmount / 100).toFixed(2));
                    $("#BSSubmittedTotalIem").text(TotalItem);
                    $("#BSSubmittedTotalAmount").text(TotalAmount);
                }
                else {
                    $("#BSSubmittedTotalIem").text("0");
                    $("#BSSubmittedTotalAmount").text("0.00");
                }
            });
        } else {
            $("#BSSubmittedTotalIem").text("0");
            $("#BSSubmittedTotalAmount").text("0.00");
        }


        $(".BtnSubmitBatch").click(function () {
            var $row = $(this).closest("tr");    // Find the row
            var CapturingBranch = $row.find("td:eq(0)").text();
            var CapturingBranchDesc = $row.find("td:eq(1)").text();
            var CapturingDate = $row.find("td:eq(2)").text();
            var ScannerId = $row.find("td:eq(3)").text();
            var BatchNumber = $row.find("td:eq(4)").text();
            var CapturingMode = $row.find("td:eq(5)").text();
            var intItems = $row.find("td:eq(6)").text().replace(/[^0-9]/g, '');
            var intAmount = $row.find("td:eq(7)").text().replace(/[^0-9]/g, '');
            var IsBranchSubmissionSuccess = ajaxdobranchSubmission(CapturingBranch, CapturingDate, ScannerId, BatchNumber);
            if (IsBranchSubmissionSuccess) {
                alert("Batch is Submitted Successfully.");
                $('#row_' + CapturingBranch).remove();
                var BSReadyTotalIem = $("#BSReadyTotalIem").text().replace(/[^0-9]/g, '');
                var BSReadyTotalAmount = $("#BSReadyTotalAmount").text().replace(/[^0-9]/g, '');
                if (BSReadyTotalIem > 0 && BSReadyTotalAmount > 0) {
                    var NewTotalItem = parseInt(BSReadyTotalIem) - parseInt(intItems);
                    var NewTotalAmount = parseInt(BSReadyTotalAmount) - parseInt(intAmount);
                    NewTotalAmount = jsFormatAmount((NewTotalAmount / 100).toFixed(2));
                    if (NewTotalAmount > 0) {
                        $("#BSReadyTotalIem").text(NewTotalItem);
                        $("#BSReadyTotalAmount").text(NewTotalAmount);
                    } else {
                        $("#BSReadyTotalIem").text("0");
                        $("#BSReadyTotalAmount").text("0.00");
                    }
                   
                }
                else {
                    $("#BSReadyTotalIem").text("0");
                    $("#BSReadyTotalAmount").text("0.00");
                }
                var submittedTable = "<tr id=row_" + CapturingBranch + ">";
                submittedTable = submittedTable + "<td>" + CapturingBranch + "</td>";
                submittedTable = submittedTable + "<td>" + CapturingBranchDesc + "</td>"
                submittedTable = submittedTable + "<td>" + CapturingDate + "</td>"
                submittedTable = submittedTable + "<td>" + ScannerId + "</td>"
                submittedTable = submittedTable + "<td>" + BatchNumber + "</td>"
                submittedTable = submittedTable + "<td>" + CapturingMode + "</td>"
                submittedTable = submittedTable + "<td>" + intItems + "</td>"
                submittedTable = submittedTable + "<td>" + jsFormatAmount((intAmount / 100).toFixed(2)) + "</td>"
                submittedTable = submittedTable + "<td><button type='button' id='BtnSubmittedDetailBatch' class='btn btn-default btn-xs BtnSubmittedDetailBatch'>Details</button></td>"
                submittedTable = submittedTable + "</tr>"
                $("#tblBranchSubmittedListing tbody").append(submittedTable);
                var BSSubmittedTotalIem = $("#BSSubmittedTotalIem").text().replace(/[^0-9]/g, '');
                var BSSubmittedTotalAmount = $("#BSSubmittedTotalAmount").text().replace(/[^0-9]/g, '');
                var NewSubTotalItem = parseInt(BSSubmittedTotalIem) + parseInt(intItems);
                var NewSubTotalAmount = parseInt(BSSubmittedTotalAmount) + parseInt(intAmount);
                NewSubTotalAmount = jsFormatAmount((NewSubTotalAmount / 100).toFixed(2));
                $("#BSSubmittedTotalIem").text(NewSubTotalItem);
                $("#BSSubmittedTotalAmount").text(NewSubTotalAmount);

            } 
        });

        $(document).on("click", ".BtnDetailBatch", function (event) {
            event.stopImmediatePropagation();
            var $row = $(this).closest("tr");    // Find the row
            var CapturingBranch = $row.find("td:eq(3)").text().trim();
            var CapturingDate = $row.find("td:eq(2)").text().trim();
            var ScannerId = $row.find("td:eq(5)").text().trim();
            var BatchNumber = $row.find("td:eq(6)").text().trim();
            jsReadyDetails(CapturingBranch, CapturingDate, ScannerId, BatchNumber);
        });

        $(document).on("click", ".BtnSubmittedDetailBatch", function (event) {
            event.stopImmediatePropagation();
            var $row = $(this).closest("tr");    // Find the 
            var CapturingBranch = $row.find("td:eq(3)").text().trim();
            var CapturingDate = $row.find("td:eq(2)").text().trim();
            var ScannerId = $row.find("td:eq(5)").text().trim();
            var BatchNumber = $row.find("td:eq(6)").text().trim();
            jsSubmittedDetails(CapturingBranch, CapturingDate, ScannerId, BatchNumber);
        });
       
        function jsReadyDetails(CapturingBranch, CapturingDate, ScannerId, BatchNumber)
        {
            var eTable = "<table id='BranchSubmissionDetailModal_rows'><thead><tr><th style='width: 200px;'>UIC</th><th style='width: 100px;'>Check Digit</th><th style='width: 100px;'>Cheque No</th><th style='width: 100px;'>Bank Code</th><th style='width: 100px;'>Location</th><th style='width: 180px;'>Issuer Account Number</th><th style='width: 100px;'>Amount</th><th style='width: 150px;'>Payee Account Number</th></tr></thead><tbody>"
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/BranchSubmissionDetails",
                method: "POST",
                data: "strCapturingBranch=" + CapturingBranch + "&strCapturingDate=" + CapturingDate + "&strScannerId=" + ScannerId + "&strBatchNumber=" + BatchNumber,
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
                       // eTable += "<td>" + item.fldItemID + "</td>";
                        //eTable += "<td>" + item.fldNonConformStatus + "</td>";
                        eTable += "</tr>";
                    });
                    eTable += "</tbody></table>";
                    $("#BranchSubmissionDetailModal .modal-title").html("Item Ready For Submittion Details");
                    $("#BranchSubmissionDetailModal_body").html(eTable);
                    $("#BranchSubmissionDetailModal").modal();
                }
            });
        }
        function jsSubmittedDetails(CapturingBranch, CapturingDate, ScannerId, BatchNumber) {


            var eventData1 = "";
            var dateString = (new Date()).toISOString().split("T")[0];
            eventData1 += '{"date":"' + dateString+ '","badge": true,"title": "Todays Date","classname": "rose"} , ';

            var eTable = "<table id='BranchSubmissionDetailModal_rows'><thead><tr><th style='width: 200px;'>UIC</th><th style='width: 100px;'>Check Digit</th><th style='width: 100px;'>Cheque No</th><th style='width: 100px;'>Bank Code</th><th style='width: 100px;'>Location</th><th style='width: 180px;'>Issuer Account Number</th><th style='width: 100px;'>Amount</th><th style='width: 150px;'>Payee Account Number</th></tr></thead><tbody>"
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/ReturnSubmittedDetails",
                method: "POST",
                data: "strCapturingBranch=" + CapturingBranch + "&strCapturingDate=" + CapturingDate + "&strScannerId=" + ScannerId + "&strBatchNumber=" + BatchNumber,
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
                    $("#BranchSubmissionDetailModal .modal-title").html("Submitted Items Details");
                    $("#BranchSubmissionDetailModal_body").html(eTable);
                    $("#BranchSubmissionDetailModal").modal();
                }
            });
        }
        function ajaxdobranchSubmission(CapturingBranch, CapturingDate, ScannerId, BatchNumber) {
            var blnSuccess;
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/BranchSubmission",
                method: "POST",
                data: "strCapturingBranch=" + CapturingBranch + "&strCapturingDate=" + CapturingDate + "&strScannerId=" + ScannerId + "&strBatchNumber=" + BatchNumber,
                success: function (data) {
                    blnSuccess = true;
                }
            });
            return blnSuccess;
        }
        function jsFormatAmount(x) {
            var parts = x.toString().split(".");
            parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            return parts.join(".");
        }
    });
})();
