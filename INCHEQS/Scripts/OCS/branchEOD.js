
(function () {
    $(document).ready(function () {

        var $holder = $("#ajaxBranchSelectList");
        if ($holder.length > 0) {
            $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/BranchList",
                method: "POST",
                success: function (data) {
                    $.each(data, function (i, item) {
                        $holder.append("<option value=" + item.fldbranchid + ">" + item.fldbranchid + " - " + item.fldBranchDesc + "</option>");
                    });


                }
            });
        }
        
        jsInitialize();
        jsPopulateData();
        jsGetBranchEOD();

        //----------------------------------------------------------------------------------------------------------
        //-------EVENTS
        //-----------------

        //branch dropdown
        $("#ajaxBranchSelectList").change(function (e) {
            jsInitialize();
            jsPopulateData();
            jsGetBranchEOD();
        })

        //save button
        $("#btnEOD").on("click", function (e) {
            
            var eodstatus = "N";  //0
            var branchcode = $("#ajaxBranchSelectList").val();
            var OCSprocessdate = $("#currentdate").val();

            var resultDataEntry = ajaxDataEntryPendingItem(branchcode).split("-");
            var resultAuthorization = ajaxAuthorizationPendingItem(branchcode).split("-");



            if (resultDataEntry != "") {
                alert("There are still pending item/s in the Batch Entry.");
                //return;
            }
            else if (resultDataEntry != "") {
                alert("There are still pending item/s in the Authorization.");
                //return;
            }
            var totalamount = parseFloat($("#sGrandTotalAmount").text().replace(/,/g, ""));
            //var eodamount = $("#txtAmount").val();
            var eodamount = parseFloat($("#txtAmount").text().replace(/,/g, ""));
            if (eodamount == "" || eodamount == null) { eodamount = "0"; }
            //eodamount = parseFloat((eodamount.replace(/,/g, "")));

            //var difference = totalamount - eodamount;
            //$("#txtDifference").val(jsFormatAmount(difference));
            
            var difference = parseFloat($("#txtDifference").text().replace(/,/g, ""));

            if (difference == 0) {
                $("#btnEOD").prop("disabled", true);
                eodstatus = "Y"; //1
            }
            else {
                eodstatus = "N"; //0
            }

            if (eodstatus == "N") //0
            {
                alert("Please complete outstanding items before performing End of Day")
            }
            else if (eodstatus == "Y") //1
            {
                //alert("EOD TAKE PLACE")
                $('#myModal').modal('show');

                var modbranchcode = $("#mobranchcode").val();
                modbranchcode = branchcode;

                $('#username, #password').attr('value', '');

                $("#submitButton").on("click", function (e) {
                    //debugger;
                    var username = document.getElementById("username").value;
                    var userpassword = document.getElementById("password").value;

                    var message = $("#message").val();

                    if (username == "" || username == null || userpassword == "" || userpassword == null) {
                        $("#message").text("Please fill all fields");
                        // alert("Enter User Login Id")
                    } else {
                        //alert(username + "," + userpassword);
                        jsGetUserValidation(username, userpassword);
                    }
                    e.stopImmediatePropagation();
                })

                function jsGetUserValidation(username, password) {
                    //debugger;
                    var result = ajaxGetOCSValidateUser(username, password).split("-");
                    if (result != "") {
            
                        var tempusername = "";
                        var tempuserpwd = "";
                        tempusername = result[0];
                        tempuserpwd = result[1];

                        var pass = ajaxGetOCSEncryptedPassword(password);


                        if (($("#username").val()).toUpperCase() == tempusername.toUpperCase() && tempuserpwd == pass) {
                            //("Username and password does match")
                            var blnInsertBranchEndOfDay = ajaxInsertHubBranchEndOfDay(branchcode, eodstatus);
                            jsGetBranchEOD();
                            $('#username, #password').attr('value', '');
                            $('#myModal').modal('toggle');

                        }
                        else {
                            alert("Invalid User Login ID & Password")
                        }
                    }
                }
                //var blnInsertBranchEndOfDay = ajaxInsertBranchEndOfDay(branchcode, totalamount.toString(), difference.toString(), eodstatus);

            }


            jsGetBranchEOD();

            if ($("#sEODStatus").text() == "Done") {
                $("#txtAmount").prop('readonly', true);
            }
            else {
                $("#txtAmount").prop('readonly', false);
            }

        })



        //----------------------------------------------------------------------------------------------------------
        //-------FUNCTIONS
        //-----------------

        function jsInitialize() {
            $("#sReadyForSubmissionTotalItem").text("0");
            $("#sReadyForSubmissionTotalAmount").text("0.00");
            $("#sSubmittedtoClearingHouseTotalItem").text("0");
            $("#sSubmittedtoClearingHouseTotalAmount").text("0.00");
            $("#sGrandTotalItem").text("0");
            $("#sGrandTotalAmount").text("0.00");

            $("#sHubReadyForSubmissionTotalItem").text("0");
            $("#sHubReadyForSubmissionTotalAmount").text("0.00");
            $("#sHubSubmittedtoClearingHouseTotalItem").text("0");
            $("#sHubSubmittedtoClearingHouseTotalAmount").text("0.00");
            $("#sHubGrandTotalItem").text("0");
            $("#sHubGrandTotalAmount").text("0.00");

            $("#sEODStatus").text("");
            $("#txtAmount").val("0.00");
            $("#txtDifference").val("0.00");

            $("#btnEOD").prop("disabled", false);

            $("#txtDifference").css("color", "white");

        }

        function jsPopulateData() {
            var branchcode = $("#ajaxBranchSelectList").val();

            var resultForSubmission = ajaxGetItemReadyForSubmission(branchcode).split("-");
            var totalitemForSubmission = new Number(resultForSubmission[0]);
            var totalamountForSubmission = new Number(resultForSubmission[1]);

            var resultSubmitted = ajaxGetItemSubmitted(branchcode).split("-");
            var totalitemSubmitted = new Number(resultSubmitted[0]);
            var totalamountSubmitted = new Number(resultSubmitted[1]);

            var totalitem = totalitemForSubmission + totalitemSubmitted;
            var totalamount = totalamountForSubmission + totalamountSubmitted;
            
            //Hub
            var resultHubForSubmission = ajaxGetHubItemReadyForSubmission(branchcode).split("-");
            var totalHubitemForSubmission = new Number(resultHubForSubmission[0]);
            var totalHubamountForSubmission = new Number(resultHubForSubmission[1]);

            var resultHubSubmitted = ajaxGetItemSubmitted(branchcode).split("-");
            var totalHubitemSubmitted = new Number(resultHubSubmitted[0]);
            var totalHubamountSubmitted = new Number(resultHubSubmitted[1]);

            var totalHubitem = totalHubitemForSubmission + totalHubitemSubmitted;
            var totalHubamount = totalHubamountForSubmission + totalHubamountSubmitted;

            $("#sReadyForSubmissionTotalItem").text(jsFormatNumber(totalitemForSubmission));
            $("#sReadyForSubmissionTotalAmount").text(jsFormatAmount(totalamountForSubmission));
            $("#sSubmittedtoClearingHouseTotalItem").text(jsFormatNumber(totalitemSubmitted));
            $("#sSubmittedtoClearingHouseTotalAmount").text(jsFormatAmount(totalamountSubmitted));
            $("#sGrandTotalItem").text(jsFormatNumber(totalitem));
            $("#sGrandTotalAmount").text(jsFormatAmount(totalamount));

            $("#txtAmount").text(jsFormatAmount(totalamountSubmitted));

            $("#txtDifference").text(jsFormatAmount(totalitemForSubmission));

            $("#sHubReadyForSubmissionTotalItem").text(jsFormatNumber(totalHubitemForSubmission));
            $("#sHubReadyForSubmissionTotalAmount").text(jsFormatAmount(totalHubamountForSubmission));
            $("#sHubSubmittedtoClearingHouseTotalItem").text(jsFormatNumber(totalHubitemSubmitted));
            $("#sHubSubmittedtoClearingHouseTotalAmount").text(jsFormatAmount(totalHubamountSubmitted));
            $("#sHubGrandTotalItem").text(jsFormatNumber(totalHubitem));
            $("#sHubGrandTotalAmount").text(jsFormatAmount(totalHubamount));
        }

        function jsGetBranchEOD() {
            //debugger;
            var branchcode = $("#ajaxBranchSelectList").val();
            var sUserId = $("#sUserId").text();

            var totalitemForSubmission = $("#sReadyForSubmissionTotalItem").text();
            var totalitemSubmitted = $("#sSubmittedtoClearingHouseTotalItem").text();
            if (totalitemForSubmission != 0) {
                $("#btnEOD").prop("disabled", true);
            }
            else {
                if (totalitemSubmitted == 0) {
                    $("#btnEOD").prop("disabled", true);
                }
            }

            //---user able perform EOD branch even with no transaction 
            if (totalitemForSubmission == "0" && totalitemSubmitted == "0") {
                $("#txtAmount").val("0.00");
                $("#txtAmount").prop('readonly', true);
                $("#btnEOD").prop("disabled", false);
            }
            else if (totalitemSubmitted != "0" && eodstatus != "Y") //1
            {
                $("#txtAmount").val("");
                if (totalitemForSubmission == "0") {
                    $("#txtAmount").prop('readonly', false);
                    $("#btnEOD").prop("disabled", false);
                }
                else {
                    $("#txtAmount").val("0.00");
                }
            }

            //-------------------------------
            if (branchcode == "All")
            {
                var result1 = ajaxGetAllBranchEndOfDay(sUserId);
                //debugger;
                if (result1 != null && result1.length > 0)
                {
                    if (result1 == 0)
                    {
                        $("#sEODStatus").text("Done");
                        $('#sEODStatus').css("color", "green");
                        $("#btnEOD").prop("disabled", true);
                    }
                    else
                    {
                        $("#sEODStatus").text("Not Done");
                        $('#sEODStatus').css("color", "red");
                    }
                }
                else
                {
                    $("#sEODStatus").text("Not Done");
                    $('#sEODStatus').css("color", "red");
                    $("#txtDifference").css("background-color", "red");
                }
          
            }
            else
            {
                var result = ajaxGetBranchEndOfDay(branchcode).split("-");
                if (result != "") {
                    var eodstatus = result[0];
                    var amount = new Number(result[1]);
                    var difference = new Number(result[2]);


                    if (eodstatus == "Y") //1
                    {
                        $("#sEODStatus").text("Done");
                        $('#sEODStatus').css("color", "green");
                        $("#txtDifference").css("background-color", "green");
                        $("#btnEOD").prop("disabled", true);
                    }
                    else {
                        $("#sEODStatus").text("Not Done");
                        $('#sEODStatus').css("color", "red");
                        $("#txtDifference").css("background-color", "red");

                    }

                    $("#txtAmount").val(jsFormatAmount(amount));
                    $("#txtDifference").val(jsFormatAmount(difference));
                }
                else {
                    $("#sEODStatus").text("Not Done");
                    $('#sEODStatus').css("color", "red");
                    $("#txtDifference").css("background-color", "red");
                }
            }
            
            $("#btnEOD").prop("disabled", false);
        }

        function jsFormatAmount(x) {
            x = x.toFixed(2)
            var parts = x.toString().split(".");
            parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            return parts.join(".");
        }

        function jsFormatNumber(x) {
            var parts = x.toString().split(".");
            parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            return parts.join(".");
        }

        //----------------------------------------------------------------------------------------------------------
        //-------AJAX
        //----------------- 

        function ajaxGetItemReadyForSubmission(strBranchCode) {
            var strReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSGetItemReadyForSubmission",
                method: "POST",
                data: "strBranchCode=" + strBranchCode,
                success: function (data) {
                    var totalitem = 0;
                    var totalamount = 0;
                    $.each(data, function (i, item) {
                        var tempItem = new Number(item.TotalItem);
                        var tempAmount = new Number(item.TotalAmount);
                        totalitem = totalitem + tempItem;
                        totalamount = totalamount + tempAmount;
                    });
                    strReturn = totalitem + "-" + totalamount
                }
            });
            return strReturn;
        }

        function ajaxGetItemSubmitted(strBranchCode) {
            var strReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSGetItemSubmitted",
                method: "POST",
                data: "strBranchCode=" + strBranchCode,
                success: function (data) {
                    var totalitem = 0;
                    var totalamount = 0;
                    $.each(data, function (i, item) {
                        var tempItem = new Number(item.TotalItem);
                        var tempAmount = new Number(item.TotalAmount);
                        totalitem = totalitem + tempItem;
                        totalamount = totalamount + tempAmount;
                    });
                    strReturn = totalitem + "-" + totalamount
                }
            });
            return strReturn;
        }

        function ajaxGetHubItemReadyForSubmission(strUserId) {
            var strHubReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSGetHubItemReadyForSubmission",
                method: "POST",
                data: "strUserId=" + strUserId,
                success: function (data) {
                    var totalitem = 0;
                    var totalamount = 0;
                    $.each(data, function (i, item) {
                        var tempItem = new Number(item.TotalItem);
                        var tempAmount = new Number(item.TotalAmount);
                        totalitem = totalitem + tempItem;
                        totalamount = totalamount + tempAmount;
                    });
                    strHubReturn = totalitem + "-" + totalamount
                }
            });
            return strHubReturn;
        }

        function ajaxGetHubItemSubmitted(strUserId) {
            var strReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSGetHubItemSubmitted",
                method: "POST",
                data: "strUserId=" + strUserId,
                success: function (data) {
                    var totalitem = 0;
                    var totalamount = 0;
                    $.each(data, function (i, item) {
                        var tempItem = new Number(item.TotalItem);
                        var tempAmount = new Number(item.TotalAmount);
                        totalitem = totalitem + tempItem;
                        totalamount = totalamount + tempAmount;
                    });
                    strReturn = totalitem + "-" + totalamount
                }
            });
            return strReturn;
        }

        function ajaxGetBranchEndOfDay(strBranchCode) {
            var strReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSGetBranchEndOfDay",
                method: "POST",
                data: "strBranchCode=" + strBranchCode,
                success: function (data) {
                    $.each(data, function (i, item) {
                        strReturn = item.fldEODStatus + "-" + item.fldAmount + "-" + item.fldDifference
                    });
                }
            });
            return strReturn;
        }

        function ajaxGetAllBranchEndOfDay(strUserId) {
            //debugger;
            var strReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSGetAllBranchEndOfDay",
                method: "POST",
                data: "strUserId=" + strUserId,
                success: function (data) {
                    $.each(data, function (i, item) {
                        strReturn = item.cntbranchid
                    });
                }
            });
            return strReturn;
        }

        function ajaxGetHubItemReadyForSubmission(strUserId) {
            var strHubReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSGetHubItemReadyForSubmission",
                method: "POST",
                data: "strUserId=" + strUserId,
                success: function (data) {
                    var totalitem = 0;
                    var totalamount = 0;
                    $.each(data, function (i, item) {
                        var tempItem = new Number(item.TotalItem);
                        var tempAmount = new Number(item.TotalAmount);
                        totalitem = totalitem + tempItem;
                        totalamount = totalamount + tempAmount;
                    });
                    strHubReturn = totalitem + "-" + totalamount
                }
            });
            return strHubReturn;
        }

        function ajaxDataEntryPendingItem(strBranchCode) {
            var strReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSDataEntryPendingItem",
                method: "POST",
                data: "strBranchCode=" + strBranchCode,
                success: function (data) {
                    $.each(data, function (i, item) {
                        strReturn = item.Cnt
                    });
                }
            });
            return strReturn;
        }

        function ajaxAuthorizationPendingItem(strBranchCode) {
            var strReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSAuthorizationPendingItem",
                method: "POST",
                data: "strBranchCode=" + strBranchCode,
                success: function (data) {
                    $.each(data, function (i, item) {
                        strReturn = item.Cnt
                    });
                }
            });
            return strReturn;
        }

            function ajaxGetOCSValidateUser(username, password) {
                var strReturn = "";
                var tempPassword = encodeURIComponent(password);

                $.ajax({
                    async: false,
                    cache: false,
                    url: App.ContextPath + "CommonApi/OCSValidateUser",
                    method: "POST",
                    data: "strusername=" + username + "&strpassword=" + tempPassword,
                    success: function (data) {
                        var tempusername = "";
                        var tempuserpwd = "";
                        $.each(data, function (i, item) {
                            var tempusername = item.fldUserAbb;
                            var tempuserpwd = item.fldPassword;
                            strReturn = tempusername + "-" + tempuserpwd
                        });
                    }
                });
                return strReturn;
            }

        function ajaxGetOCSEncryptedPassword(password) {
           // debugger;
            var strReturn = "";
            var tempPassword = encodeURIComponent(password);
                $.ajax({
                    async: false,
                    cache: false,
                    url: App.ContextPath + "CommonApi/OCSEncryptedPassword",
                    method: "POST",
                    data: "strpassword=" + tempPassword,
                    success: function (data) {
                        var tempuserpwd = "";
                        tempuserpwd = data;
                        strReturn = tempuserpwd
                    }
                });
                return strReturn;
            }

        function ajaxInsertBranchEndOfDay(strBranchId, strAmount, strDifference, strEODStatus) {
            var blnSuccess;
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/OCSInsertBranchEndOfDay",
                method: "POST",
                data: "strBranchId=" + strBranchId + "&strAmount=" + strAmount + "&strDifference=" + strDifference + "&strEODStatus=" + strEODStatus,
                success: function (data) {
                    blnSuccess = true;
                }
            });
            return blnSuccess;
        }

            function ajaxInsertHubBranchEndOfDay(strBranchId, strEODStatus) {
                var blnSuccess;
                $.ajax({
                    async: false,
                    cache: false,
                    url: App.ContextPath + "CommonApi/OCSHubInsertBranchEndOfDay",
                    method: "POST",
                    data: "strBranchId=" + strBranchId + "&strEODStatus=" + strEODStatus,
                    success: function (data) {
                        blnSuccess = true;
                        alert("End of Day performed successfully")
                    }
                });
                return blnSuccess;
            }

        
    });
})();


