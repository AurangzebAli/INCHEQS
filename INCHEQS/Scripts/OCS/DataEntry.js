var g_arrAcctNo = new Array();
var g_arrChqAmt = new Array();
var g_objTable = "";
var g_objTableChqAmt = "";
var g_obTxtVAcctNo = "";
var g_obTxtAccName = "";
var g_obTxtAccStatus = "";
var g_obTxtDepAmt = "";
var g_obTxtChqAmt = "";
var g_intEditVAIndex = 0;
var g_isIslamicOrConv = -1;
var g_chequeIsIslamicOrConv = -1;
var input = ""; //holds current input as a string

(function () {
    $(document).ready(function () {
        var P_Flag = false;
        jsInit();


        var CheckPExist = $("#tblAcctNoListDetail tbody");
        CheckPExist.find('tr').each(function (i) {
            var $tds = $(this).find('td'),
                intItems = $tds.eq(3).text()
            if (intItems == 'p' || intItems == 'P') {
                P_Flag = true;
            }
        });

        $("#btnAdd").click(function () {
            if ($("#txtChequeAccountNumber").val() != 'undefined' && $("#txtChequeAccountNumber").val() != null && $("#txtChequeAccountNumber").val() != "") {
                onAddButtonClicked();
                input = "";
                $("#frontBtn").trigger('click');
            }
            else {
                alert("Please Add Account Number.");
                $("#txtChequeAccountNumber").focus();
                return;
            }
        });

        $("#txtChequeAccountNumber").keydown(function (event) {
            var keycode = event.keyCode || event.which;
            if (keycode == 13) {
                if ($("#txtChequeAccountNumber").val() === 'undefined' || $("#txtChequeAccountNumber").val() === null || $("#txtChequeAccountNumber").val() === "") {
                    alert("Please Add Account Number.");
                    $("#txtChequeAccountNumber").focus();
                    return;
                }
                else {
                    onAddButtonClicked();
                    input = "";
                    var acclength = parseInt($("#txtChequeAccountNumber").val().length);
                    if (acclength == 17 || acclength == 13) {
                        $("#frontBtn").trigger('click');
                        $("#txtChequeAmount").focus();
                    }
                    
                }
            }
            else if (keycode == 107) {
                var objHfdChqAmt = window.document.getElementById("hfdChqAmt");
                if (objHfdChqAmt.value == "") {
                    alert("Account Listing Cannot be Empty.");
                    return;
                }
                else {
                    $("#Confirmbtn").trigger('click');
                }
            }
        });

        $(document).on("click", ".btnEditAcctNo", function (event) {
            event.preventDefault(); // do not follow the link or move to top
            var $row = $(this).closest("tr");
            var intIndex = $row.attr('data-id');
            EditVAcctNo(intIndex);
            input = "";
            event.stopImmediatePropagation();
        });

        $(document).on("click", ".btnRemoveAcctNo", function (event) {
            event.preventDefault(); // do not follow the link or move to top
            var $row = $(this).closest("tr");
            var intIndex = $row.attr('data-id');
            RemoveVAcctNo(intIndex);
            input = "";
            event.stopImmediatePropagation();
        });

        $("#txtChequeAmount").keydown(function (e) {
            //handle backspace key
            if (e.keyCode == 8 && input.length > 0) {
                input = input.slice(0, input.length - 1); //remove last digit
                $(this).val(addCommas(formatNumber(input)));
            }
            else if (e.keyCode == 13) {
                onAddButtonClicked();
                input = "";
                if (P_Flag == false) {
                    $("#BackBtn").trigger('click');
                }
                $("#txtChequeAccountNumber").focus();
            }
            else if (e.keyCode == 107) {
                var objHfdChqAmt = window.document.getElementById("hfdChqAmt");
                if (objHfdChqAmt.value == "") {
                    alert("Account Listing Cannot be Empty.");
                }
                else {
                    $("#Confirmbtn").trigger('click');
                }
            }
            else {
                var key = getKeyValue(e.keyCode);
                if (key) {
                    input += key; //add actual digit to the input string
                    $(this).val(addCommas(formatNumber(input))); //format input string and set the input box value to it
                }
            }
            return false;
        });
        function getKeyValue(keyCode) {
            if (keyCode > 57) { //also check for numpad keys
                keyCode -= 48;
            }
            if (keyCode >= 48 && keyCode <= 57) {
                return String.fromCharCode(keyCode);
            }
        }
        function formatNumber(input) {
            if (isNaN(parseFloat(input))) {
                return "0.00"; //if the input is invalid just set the value to 0.00
            }
            var num = parseFloat(input);
            return (num / 100).toFixed(2); //move the decimal up to places return a X.00 format
        }
        //function addCommas(num) {
        //    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        //}
    });

    function jsInit() {
        //Indicate transaction input before 
        var entryStatus = document.getElementById("hfdDoneEntry");
        entryStatus.value = "false";
        //debugger;
        jsInitAccNoList();
        jsShowRows();
    }

    // Dynamic Item's Account Entry(s) List          
    function jsInitAccNoList() {
        var objHfdRejectVAcctNoList = window.document.getElementById("hfdRejectVAcctList");
        g_objTable = window.document.getElementById("tblAcctNoList");
        g_objTableChqAmt = window.document.getElementById("tblChqAmtList");
        g_obTxtVAcctNo = window.document.getElementById("txtChequeAccountNumber");
        g_obTxtAccName = window.document.getElementById("txtAccName");
        g_obTxtAccStatus = window.document.getElementById("txtAccStatus");
        g_obTxtDepAmt = window.document.getElementById("txtChequeAmount");
        g_obTxtChqAmt = window.document.getElementById("txtTotalamount");

        //#06
        g_isIslamicOrConv = window.document.getElementById("hIslamicOrConv");
        g_chequeIsIslamicOrConv = window.document.getElementById("hfdBusinessType");

        // Clean Up List Of Reject VAccount List          
        objHfdRejectVAcctNoList.value = "";

        // Remove Table Dynamic Display
        RemoveAllRows();
        //RemoveAllRowsChqAmt();

        // Remove Item From Array
        g_arrAcctNo.splice(0, g_arrAcctNo.length);
        g_arrChqAmt.splice(0, g_arrChqAmt.length);
        
        // Check For Update Array
        CheckUpdateVAcctArray();

        if (g_obTxtVAcctNo.value == "" && g_obTxtVAcctNo.disabled == false) {
            document.getElementById("txtChequeAccountNumber").focus();
        }
        else if (g_obTxtDepAmt.value == "" && g_obTxtDepAmt.disabled == false) {
            document.getElementById("txtChequeAmount").focus();
        }
    }

    function jsShowRows() {
        //alert('jsShowRows');
        var strDepAmt = window.document.getElementById('txtChequeAmount').value;
        var strVAcctNo = window.document.getElementById('txtChequeAccountNumber').value;
        //jsShowRowsDetails(strDepAmt, strVAcctNo);
    }

    //Function to show row details(depends on account no. entered)
    function jsShowRowsDetails(strDepAmt, strVAcctNo) {

        if (strDepAmt == "" && strVAcctNo == "") {
            trAccName.style.display = "none";
            trAccStatus.style.display = "none";
        }
        else {
            trAccName.style.display = "none";
            trAccStatus.style.display = "none";
        }
    }
    //Acc No List Function 
    function DynamicRemoveAllTableRows() {
        RemoveAllRows();
    }

    // Remove Table All Row(s)
    function RemoveAllRows() {
        var objTbl = g_objTable;
        var intCount = parseInt(objTbl.rows.length);
        if (objTbl.rows.length != 0) {
            for (var i = 0; i <= intCount - 1; i++) {
                objTbl.deleteRow(0);
            }
        }
    }

    // -- Gather All VAItem Retrieve From Database
    function CheckUpdateVAcctArray() {
        var objHfdCItemChqAmtList = window.document.getElementById("hfdCItemChqAmtList");
        var strLstCItemChqAmt = objHfdCItemChqAmtList.value;

        var objHfdVItemNoList = window.document.getElementById("hfdVItemNoList");
        var objhfdAccNo = window.document.getElementById("hfdAccNo");

        if (objhfdAccNo.value == "") {
            var strLstVItemNo = objHfdVItemNoList.value;
        }
        else {
            var strLstVItemNo = objhfdAccNo.value;
        }


        if (strLstVItemNo == "")
            return;

        //alert('CheckUpdateVAcctArray1a-->');
        //alert(objHfdCItemChqAmtList.value);
        //alert('CheckUpdateVAcctArray1b-->');
        //alert(objHfdVItemNoList.value);
        
        var arrItemVAcct = strLstVItemNo.split(";");
        for (var i = 0; i < arrItemVAcct.length; i++) {
            var arrVAcctDetails = arrItemVAcct[i].split("+");
            var bintItemID = arrVAcctDetails[0];
            var strVAcctNo = arrVAcctDetails[1];
            var intDataFromDB = arrVAcctDetails[2];
            var strItemType = arrVAcctDetails[3];
            var strReasonCode = arrVAcctDetails[4];
            var strRemark = arrVAcctDetails[5];
            var strAccName = arrVAcctDetails[6];
            var strAccStatus = arrVAcctDetails[7];
            var strIslamicConvFlag = arrVAcctDetails[8];
            var strDepAmt = arrVAcctDetails[9];

            //alert(g_arrAcctNo);                 
            // -- If Current Active Item ID Found, Insert At The Beginning Of Array
            if (parseInt(window.document.getElementById("lblItemID").innerText) == parseInt(bintItemID)) {
                //alert('CheckUpdateVAcctArray2a-->');
                //alert(strVAcctNo);
                g_arrAcctNo.unshift(arrItemVAcct[i]);
                if (strDepAmt == 0)
                    g_intEditVAIndex = 0;
                else
                    g_intEditVAIndex = -1
            }
                // -- Insert Value At The End Of Array
            else {
                //alert('CheckUpdateVAcctArray2b-->');
                g_arrAcctNo.push(arrItemVAcct[i]);
            }
        }
        //alert('CheckUpdateVAcctArray');
        //alert(g_intEditVAIndex);
        g_arrChqAmt.push(strLstCItemChqAmt);
        GenerateHtmlDataTable();
    }

    // Generate HTML Table
    function GenerateHtmlDataTable() {
        SortAcctNoList();
        if (!g_objTable)
            DynamicCreateHtmlTable(g_objTable);
        RemoveAllRows();
        // Add New Row, Cell                                           
        objTr = g_objTable.insertRow();
        objTd = objTr.insertCell();
        // Update Column Acc Data Row Details
        objTd.innerHTML = GetStrTable();
    }

    function SortAcctNoList() {
        var arrSortedAcctNo = new Array(g_arrAcctNo.length);
        var strLastOrderItemType = "CP";
        var intLastItemIndex = 0;

        for (var i = 0; i < g_arrAcctNo.length; i++) {
            var arrVAcctDetails = "";
            var bintItemID = "";
            var strVAcctNo = "";
            var intDataFromDB = "";
            var strItemType = "";
            var strReasonCode = "";
            var strRemark = "";

            // Get Item Detail
            arrVAcctDetails = g_arrAcctNo[i].split("+");
            bintItemID = arrVAcctDetails[0];
            strVAcctNo = arrVAcctDetails[1];
            intDataFromDB = arrVAcctDetails[2];
            strItemType = arrVAcctDetails[3];
            strReasonCode = arrVAcctDetails[4];
            strRemark = arrVAcctDetails[5];

            // No Char Found


            //var sChar;
            //var intCount;
            //var strValidCharacterList;
            //var bool;
            //strValidCharacterList = 'C';
            //for (var i = 0; i < strItemType.length; i++) {
            //    sChar = strItemType.charAt(i);
            //    // Value Not Exist In Accepted Value List	
            //    if (strValidCharacterList.indexOf(sChar) == -1)
            //    {
            //        bool = false;
            //    }
            //    else {
            //        bool = true;
            //    }

            //}

            var bool = jsIsAllCharsValid(strItemType, "C");
            if (bool == false) {
                // Assign Item Detail To New Array
                arrSortedAcctNo[Array_GetLastEmptyItemIndex(arrSortedAcctNo)] = g_arrAcctNo[i];
            }
            else
                intLastItemIndex = i
        }

        // Assign The Item Type [C/P] Into Array Last Item 
        arrSortedAcctNo[Array_GetLastEmptyItemIndex(arrSortedAcctNo)] = g_arrAcctNo[intLastItemIndex];

        // Reassign To Global Array List
        g_arrAcctNo = arrSortedAcctNo;
    }

    // Get Last Item Index Of Empty Array
    function Array_GetLastEmptyItemIndex(objArray) {
        for (var i = 0; i < objArray.length; i++) {
            if (!objArray[i]) {
                return i;
            }
        }
    }

    function DynamicCreateHtmlTable(strTableName) {
        var objTable = window.document.createElement("<table id='tblAcctNoList'  class='table table-bordered mTop10'></table>");
        var objSpan = window.document.getElementById("divAccNoList");
        objSpan.appendChild(objTable);
    }

    function GetStrTable() {
        strTable = "";
        // Table Header
        strTable += "<table class='table table-bordered mTop10' id='tblAcctNoListDetail'>";
        // Table Subject Cells
        strTable += "<thead>";
        strTable += "<tr style='background-color:#cbe3ef;'>";
        strTable += "<th>Reject</th>";
        strTable += "<th style='width: 100px;'>Item ID</th>";
        strTable += "<th style='width: 100px;'>Action</th>";
        strTable += "<th style='width: 50px;'>Type</th>";
        strTable += "<th style='width: 150px;'>Cheque Amount</th>";
        strTable += "<th style='width: 200px;'>Account Entry</th>";
        strTable += "<th style='width: 150px;'>Account Name</th>";
        strTable += "<th style='width: 150px;'>Account Status</th>";
        strTable += "<th style='width: 150px;'>Account Dormant</th>";
        strTable += "<th style='width: 150px;'>Account Frozen</th>";
        strTable += "</tr>";
        strTable += "</thead>";
        // Table Detail Rows
        strTable += "<tbody>";
        strTable += GetStrDataRows();
        strTable += "</tbody>";
        // Table Footer 
        strTable += "</table>";
        return strTable;
    }


    function GetStrDataRows() {
        var strRows = "";
        var intIndex = 0;
        var intTotalChqAmt = 0;
        var strDisabled = "";
        var objTr, objTd;
        
        //Start Loading Account No Row
        for (var i = 0; i < g_arrAcctNo.length; i++) {
            var arrVAcctDetails = "";
            var bintItemID = "";
            var strVAcctNo = "";
            var intDataFromDB = "";
            var strItemType = "";
            var strReasonCode = "";
            var strRemark = "";
            var strAccName = "";
            var strAccStatus = "";
            var strIslamicConvFlag = "";
            var strDepAmt = "";
            intIndex = parseInt(i);

            // Get VA Item Detail
            arrVAcctDetails = g_arrAcctNo[i].split("+");
            bintItemID = arrVAcctDetails[0];
            strVAcctNo = arrVAcctDetails[1];
            intDataFromDB = arrVAcctDetails[2];
            strItemType = arrVAcctDetails[3];
            strReasonCode = arrVAcctDetails[4];
            strRemark = arrVAcctDetails[5];
            strAccName = arrVAcctDetails[6];
            strAccStatus = arrVAcctDetails[7];
            strIslamicConvFlag = arrVAcctDetails[8];
            strDepAmt = arrVAcctDetails[9];

            // Current Active Item ID Matched With Current Process Item ID
            if (bintItemID == parseInt(document.getElementById("lblItemID").innerText)) {
                document.getElementById("lblRemark").innerText = strRemark;
            }

            // Only First Virtual Account(VA) Account Is Active And Not Disabled   
            // Disabled All Records Get From Database Except First Record Which Currently Active For this form entry.         
            if (document.getElementById('hEnableButton').value != "Y") {
                strDisabled = "disabled";
            }

            //Added By Ying - Set Value For ItemId
            if (intIndex != 0) {
                bintItemID = intIndex;
            }

            //Fromat Display
            if (strDepAmt == 0) {
                strDepAmt = "";
            }

            if (strDepAmt != '') {
                var decDepAmt = parseFloat(strDepAmt.replace(/,/g, ''));
                if (strDepAmt.indexOf(".") == -1) {
                    decDepAmt = decDepAmt / 100;
                }
                strDepAmt = parseFloat(decDepAmt).toFixed(2);
            }

            //strRows += "<tbody>";
            strRows += "<tr data-id=" + intIndex + ">";
            strRows += "<td align='left'>" + GenerateRejectCheckbox(intDataFromDB, intIndex, bintItemID, strReasonCode, strItemType) + "</td>";
            strRows += "<td align='left'>" + bintItemID + "</td>";
            strRows += "<td align='left'>" + GenerateControls(intDataFromDB, intIndex, bintItemID, strReasonCode, i, strDisabled); +"</td>";
            strRows += "<td align='left'>" + jsEmptyStringReplace(strItemType, "&nbsp;") + "</td>";
            strRows += "<td align='left'><input id='txtDepAmt" + intIndex + "'  style='font-weight:bold;' type='text' value='" + toCurrency(strDepAmt) + "' disabled></td>";
            strRows += "<td align='right'><input id='txtAcctNo" + intIndex + "'  style='font-weight:bold;'  type='text' value='" + strVAcctNo + "' disabled></td>";
            strRows += jsGetPayeeName(strVAcctNo, intIndex);
            //strRows += "<td align='left' style='display:none;'><input id='txtAccName" + intIndex + "' type='text' value='" + strAccName + "' disabled></td>";
            //strRows += "<td align='left' style='display:none;'><input id='txtAccStatus" + intIndex + "' style='width:100px' type='text' value='" + strAccStatus + "' disabled></td>";
            strRows += "</tr>";
            // strRows += "</tbody>";
            //debugger;
            //ApplyChecking
            if (strDepAmt == "") {
                strDepAmt = 0;
                window.document.getElementById("hfZeroAmount").value = "true";
                strDepAmt = parseFloat(strDepAmt);
            }
            else {
                window.document.getElementById("hfZeroAmount").value = "false";
            }
            if (intTotalChqAmt == "") {
                intTotalChqAmt = 0;
                intTotalChqAmt = parseFloat(intTotalChqAmt);
            }

            if (strDepAmt != "") {
                strDepAmt = strDepAmt.replace(',', '');
            }

            intTotalChqAmt += parseFloat(strDepAmt);

            //Calculate Total Cheque Amount
            document.getElementById('txtTotalamount').value = addCommas(intTotalChqAmt.toFixed(2));
            g_arrChqAmt[0] = intTotalChqAmt.toFixed(2);
        }
        return strRows;
    }

    function addCommas(num) {
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    function jsEmptyStringReplace(strValue, strReplace) {
        try {
            if (strValue == "") {
                return strReplace;
            }
            else {
                return strValue;
            }
        }
        catch (e) {
        }
    }

    // -- Create Checkbox String
    function GenerateRejectCheckbox(intDataFromDB, intIndex, bintItemID, strReasonCode, strItemType) {
        var strCtrlID = "chkRejectVAcctNo";
        var strChecked = "";
        var strCheckboxDisabled = "";

        // -- Disable CheckBox               
        if (intIndex == 0) {
            strChecked = "";
            strCheckboxDisabled = "disabled";
        }
        else {
            strChecked = "";
            strCheckboxDisabled = "";
        }

        // -- Get Return String Of Checkbox
        return GetStrCheckbox(strCtrlID, bintItemID, "PrepareRejectedList", strChecked, strCheckboxDisabled, intIndex);
    }

    // -- Get Return Checkbox Control Apperance & Setting
    function GetStrCheckbox(strCtrlID, strValue, strOnClickFunc, strChecked, strCheckboxDisabled, intIndex) {
        var strHtml = "";
        strHtml = "<input id='" + strCtrlID + "_" + intIndex + "' name='" + +"_" + intIndex + "' type='checkbox' onclick=onCheckboxChecked('" + strOnClickFunc + "'); " + " value='" + strValue + "' " + strChecked + " " + strCheckboxDisabled + ">";
        return strHtml;
    }

    // -- Checkbox Checked
    function onCheckboxChecked(strFuncName) {
        // -- Convert String Into Function Name And Run Function
        eval(strFuncName)();
    }


    function GenerateControls(intDataFromDB, intIndex, bintItemID, strReasonCode, i, strDisabled) {
        var strHtml = "";
        strHtml += WriteStrButton(intDataFromDB, intIndex, bintItemID, strDisabled);
        return strHtml;
    }

    // -- Get Return Edit Button Control Apperance & Setting
    function WriteStrButton(intDataFromDB, intIndex, bintItemID, strDisabled) {
        var strHtml = "";
        strHtml += "<input id='btnEditAcctNo_" + intIndex + "' name='btnEditAcctNo_" + intIndex + "' class='btnEditAcctNo btn btn-default btn-xs' type='button' value='Edit' " + strDisabled + ">";
        if (intIndex == 0) {
            strHtml += "<input id='btnRemoveAcctNo_" + intIndex + "' name='btnRemoveAcctNo_" + intIndex + "' class='btnRemoveAcctNo btn btn-default btn-xs' type='button' value='Remove' disabled>";
        }
        else {
            strHtml += "<input id='btnRemoveAcctNo_" + intIndex + "' name='btnRemoveAcctNo_" + intIndex + "' class='btnRemoveAcctNo btn btn-default btn-xs'  type='button' value='Remove' " + strDisabled + ">";
        }
        return strHtml;
    }


    function jsGetPayeeName(AccNumber, intIndex) {
        var strHtml = "";
        if (AccNumber != 0 || AccNumber != "0") {
            $.ajax({
                cache: false,
                async: false,
                url: App.ContextPath + "CommonApi/MatchAIFMaster",
                method: "POST",
                data: "intAccNumber=" + AccNumber,
                success: function (data) {
                    if (data.length > 0) {
                        //debugger
                        $.each(data, function (i, item) {
                            
                            strHtml += "<td align='left' > "+ item.fldAccountName +"</td>";  // <input  id='txtAccName" + intIndex + "' type='text' value='" + item.fldAccountName + "' disabled></td>";
                            strHtml += "<td align='left' >" + item.fldAccountStatus + "</td>"; //<input id='txtAccStatus" + intIndex + "' type='text' value='" + item.fldAccountStatus + "' disabled></td>";
                            strHtml += "<td align='left' >" + item.fldAccountDormant + "</td>"; 
                            strHtml += "<td align='left' >" + item.fldAccountFrozen + "</td>"; 
                            if (item.fldAccountName.indexOf("Error in Calling API") >= 0) {
                                //$("#notice").removeClass("alert-success");
                                //$("#notice").addClass("alert-danger");
                                //$("#notice .notice-body").html(item.fldAccountName);
                                //$("#notice").removeClass("hidden");
                                window.document.getElementById("hfAccountNameStatus").value = "true";
                            }
                            else if (item.fldAccountName.indexOf("Record Not found") >= 0) {
                                window.document.getElementById("hfAccountNameStatus").value = "true";
                            }
                            else if (item.fldAccountName.indexOf("Invalid GL Code") >= 0) {
                                window.document.getElementById("hfAccountNameStatus").value = "true";
                            }
                            else {
                                window.document.getElementById("hfAccountNameStatus").value = "false";
                            }
                            
                        });
                    }
                    else {
                        strHtml += "<td align='left' >Record Not found.</td>";
                        strHtml += "<td align='left' >-</td>";
                        window.document.getElementById("hfAccountNameStatus").value = "true";
                    }
                }
            });
        }
        return strHtml;
    }
    // Generate HTML Table
    function GenerateHtmlDataTableChqAmt() {
        if (!g_objTableChqAmt)
            DynamicCreateHtmlTableChqAmt(g_objTableChqAmt);
        RemoveAllRowsChqAmt();
        // Add New Row, Cell                                           
        objTr = g_objTableChqAmt.insertRow();
        objTd = objTr.insertCell();
        // Update Column Acc Data Row Details
        objTd.innerHTML = GetStrTableChqAmt();
    }

    function DynamicCreateHtmlTableChqAmt(strTableName) {
        var objTable = window.document.createElement("<table id='tblChqAmtList' class='table table-bordered mTop10'></table>");
        var objSpan = window.document.getElementById("divChqAmtList");
        objSpan.appendChild(objTable);
    }

    // Remove Table All Row(s)
    function RemoveAllRowsChqAmt() {
        var objTbl = g_objTableChqAmt;
        var intCount = parseInt(objTbl.rows.length);
        if (objTbl.rows.length != 0) {
            for (var i = 0; i <= intCount - 1; i++) {
                objTbl.deleteRow(0);
            }
        }
    }

    function GetStrTableChqAmt() {
        strTable = "";

        // Table Header
        strTable += "<table class='table table-bordered mTop10'>";

        // Table Subject Cells
        strTable += "<thead>";
        strTable += "<tr>";
        strTable += "<th>Reject</th>";
        strTable += "<th>Item ID</th>";
        strTable += "<th>Action</th>";
        strTable += "<th>Type</th>";
        strTable += "<th>Amount Entry</th>";
        strTable += "</tr>";
        strTable += "</thead>";
        // Table Detail Rows
        strTable += GetStrDataRowsChqAmt();
        // Table Footer 
        strTable += "</table>";
        return strTable;
    }

    function GetStrDataRowsChqAmt() {
        var strRows = "";
        var intIndex = 0;
        var strDisabled = "";
        var objTr, objTd;

        //Start Loading Account No Row
        for (var i = 0; i < 1; i++) {
            var strChqAmt = "";
            intIndex = parseInt(i);

            // Get VA Item Cheque Amount
            strChqAmt = g_arrChqAmt[i];

            // Only First Virtual Account(VA) Account Is Active And Not Disabled   
            // Disabled All Records Get From Database Except First Record Which Currently Active For this form entry.         
            if (document.getElementById('hEnableButton').value != "Y") {
                strDisabled = "disabled";
            }

            strRows += "<tbody>";
            strRows += "<tr id=" + intIndex + ">";
            strRows += "<td>" + GenerateRejectCheckboxChqAmt(document.getElementById("lblTransNo").innerText) + "</td>";
            strRows += "<td>" + document.getElementById("lblTransNo").innerText + "</td>";
            strRows += "<td>" + GenerateControlsChqAmt(intIndex, strDisabled); +"</td>";
            strRows += "<td>C</td>";
            strRows += "<td><input id='txtChequeAmount" + intIndex + "' type='text' value='" + toCurrency(strChqAmt) + "' disabled></td>";
            strRows += "</tr>";
            strRows += "</tbody>";
        }
        return strRows;
    }

    // -- Create Checkbox String
    function GenerateRejectCheckboxChqAmt(bintItemID) {
        var strCtrlID = "chkRejectVAcctNo";
        var strChecked = "";
        var strCheckboxDisabled = "disabled";

        // -- Get Return String Of Checkbox
        return GetStrCheckboxChqAmt(strCtrlID, bintItemID, "PrepareRejectedList", strChecked, strCheckboxDisabled);
    }

    // -- Get Return Checkbox Control Apperance & Setting
    function GetStrCheckboxChqAmt(strCtrlID, strValue, strOnClickFunc, strChecked, strCheckboxDisabled) {
        var strHtml = "";
        strHtml = "<input id='" + strCtrlID + "' name='" + strCtrlID + "' type='checkbox' onclick=onCheckboxCheckedChqAmt('" + strOnClickFunc + "'); " + " value='" + strValue + "' " + strChecked + " " + strCheckboxDisabled + ">";
        return strHtml;
    }

    // -- Checkbox Checked
    function onCheckboxCheckedChqAmt(strFuncName) {
        // -- Convert String Into Function Name And Run Function
        eval(strFuncName)();
    }

    // -- VA Checkbox Checked / UnChecked
    // -- Edit Button Click For Edit Current Active Item [Index = 0]
    function EditVAcctNo(intIndex) {

        //alert('EditVAcctNo1');
        //alert(intIndex);
        //alert('EditVAcctNo2');
        //alert(g_arrChqAmt[0]);
        //alert('EditVAcctNo3');
        //alert(g_arrAcctNo[intIndex].split("+"));
            
        var strChqAmt = g_arrChqAmt[0];
        var arrVAcctDetails = g_arrAcctNo[intIndex].split("+");
        //var bintItemID            = arrVAcctDetails[0];
        var strVAcctNo = arrVAcctDetails[1];
        var intDataFromDB = arrVAcctDetails[2];
        //var strItemType           = arrVAcctDetails[3];
        //var strReasonCode         = arrVAcctDetails[4];
        //var strRemark             = arrVAcctDetails[5];
        var strAccName = arrVAcctDetails[6];
        var strAccStatus = arrVAcctDetails[7];
        var strDepAmt = arrVAcctDetails[9];

        //alert(intDataFromDB);

        // Update Input Text VA Value, Set Edit Index Number                
        g_obTxtVAcctNo.value = strVAcctNo;
        g_obTxtAccName.value = strAccName;
        g_obTxtAccStatus.value = strAccStatus;
        g_obTxtDepAmt.value = toCurrency(strDepAmt);
        g_obTxtChqAmt.value = toCurrency(strChqAmt);
        g_intEditVAIndex = parseInt(intIndex);

        //need to add this to enable editing after click button gray or front back
        window.document.getElementById("txtChequeAccountNumber").value = strVAcctNo;
        window.document.getElementById("txtAccName").value = strAccName;
        window.document.getElementById("txtAccStatus").value = strAccStatus;
        window.document.getElementById("txtChequeAmount").value = toCurrency(strDepAmt);
        window.document.getElementById("txtTotalamount").value = toCurrency(strChqAmt);

        // -- Show Row Details
        //jsShowRowsDetails(strDepAmt, strVAcctNo);
    }

    function GenerateControlsChqAmt(intIndex, strDisabled) {
        var strHtml = "";
        strHtml += WriteStrButtonChqAmt(intIndex, strDisabled);
        return strHtml;
    }

    // -- Get Return Edit Button Control Apperance & Setting
    function WriteStrButtonChqAmt(intIndex, strDisabled) {
        var strHtml = "";
        strHtml += "<input id='btnEditChqAmt' name='btnEditChqAmt' type='button' style='width: 100px;' onclick='EditVChqAmt(" + intIndex + ");' value='Edit' " + strDisabled + ">";

        return strHtml;
    }


    // -- VA Checkbox Checked / UnChecked
    // -- Edit Button Click For Edit Current Active Item [Index = 0]
    function RemoveVAcctNo(intIndex) {

        // Remove All VA Confirmation 
        if (!confirm("Confirm remove selected Account Entry?")) {
            window.event.returnValue = false;
            return false;
        }

        // Remove Item From Array  
        //alert('Index b4');
        //alert(intIndex);
        //alert('Remove b4');
        //alert(g_arrAcctNo.length);
        g_arrAcctNo.splice(intIndex, 1);
        //alert('Remove after');;
        //alert(g_arrAcctNo.length);

        // Remove Item From Array 
        GenerateHtmlDataTable();
        UpdateHiddenValue();
        document.getElementById("txtChequeAccountNumber").value = "";
        document.getElementById("txtChequeAmount").value = "";
        //alert("Account Entry removed");
    }


    function onAddButtonClicked() {
        
        event.keyCode = 13;
        getFuncKey(event);
    }
    function getFuncKey(ev) {
        //------------------Assign Window Value to Variable -----------------
        g_obTxtVAcctNo = window.document.getElementById("txtChequeAccountNumber");
        g_obTxtDepAmt = window.document.getElementById("txtChequeAmount")
        g_obTxtChqAmt = window.document.getElementById("txtTotalamount")

        //------------------Taking Value From Window Action -----------------
        var decimalPointFactor = 100; //this is to cater "." decimal point only, e.g. 2 decimal point = 100, 3 decimal point = 1000,...
        value = (window.external) ? event.keyCode : ev.keyCode;
        //tells you the code of the button which was pressed
        //F5 = 116, F8 = 119
        window.status = value;


        //------------------Start Checking-----------------
        var enableSave = "Y";
        //Check TextBox disable Status 
        if (document.getElementById('txtChequeAccountNumber').disabled == true && document.getElementById('txtChequeAmount').disabled == true) {
            enableSave = "N"
        }

        if (enableSave == "Y") {
            //-8 = [BackSpace] 
            if (value == 8) {
                if (objCurrentCtrl.name == "txtChequeAmount") {
                    var amt = objCurrentCtrl.value;
                    objCurrentCtrl.value = '' + amt.substring(0, amt.length - 1);
                    objCurrentCtrl.value = toCurrency(objCurrentCtrl.value);
                }
                else if (objCurrentCtrl.name == "txtChequeAccountNumber") {
                    var accNo = objCurrentCtrl.value;
                    objCurrentCtrl.value = '' + accNo.substring(0, accNo.length - 1);
                }
                else {
                    try {
                        var accNo = objCurrentCtrl.value;
                        objCurrentCtrl.value = '' + accNo.substring(0, accNo.length - 1);
                    } catch (e) {
                    }
                }
            }
            else if (value == 9) {      //Rotate the focus around the important buttons and fields
                //alert(objCurrentCtrl.name);
                if (objCurrentCtrl.name == "txtChequeAmount") {
                    //objCurrentCtrl = document.getElementById("txtChequeAccountNumber");
                    objCurrentCtrl = document.getElementById("btnAdd");
                    objCurrentCtrl.focus();
                    setSelectionRange(objCurrentCtrl);
                } else if (objCurrentCtrl.name == "txtChequeAccountNumber") {
                    //objCurrentCtrl = document.getElementById("btnAdd");
                    objCurrentCtrl = document.getElementById("txtChequeAmount");
                    objCurrentCtrl.focus();
                } else if (objCurrentCtrl.name == "btnAdd") {
                    //objCurrentCtrl = document.getElementById("txtChequeAmount");
                    objCurrentCtrl = document.getElementById("txtChequeAccountNumber");
                    objCurrentCtrl.focus();
                    setSelectionRange(objCurrentCtrl);
                } else {
                    if (document.getElementById("txtChequeAccountNumber") == "") {
                        objCurrentCtrl = document.getElementById("txtChequeAccountNumber");
                        objCurrentCtrl.focus();
                        setSelectionRange(objCurrentCtrl);
                    }
                    else if (document.getElementById("txtChequeAmount") == "") {
                        objCurrentCtrl = document.getElementById("txtChequeAmount");
                        objCurrentCtrl.focus();
                        setSelectionRange(objCurrentCtrl);
                    }
                }
                /*
                event.cancelBubble = true;
                event.returnValue = false;
                event.keyCode = false;
                return false;
                */
            }
                //-37 = [Left Arrow]   
            else if (value == 37) {
                objCurrentCtrl.focus();
                return false;
            }
                //-39 = [Right Arrow]
            else if (value == 39) {
                objCurrentCtrl.focus();
                return false;
            }
                //-48 to 57 || 96 to 105 = [Numeric]   
                //to return the numeric digits entered, removing all formatting commas and dots 
            else if (((value >= 48) && (value <= 57)) || ((value >= 96) && (value <= 105))) {
                //alert(objCurrentCtrl.name);
                if (objCurrentCtrl.name == "txtChequeAmount") {
                    //------------Part1------------
                    var leadingSymbol = ",";
                    var plainNumeric = objCurrentCtrl.value.replace(new RegExp(leadingSymbol, "g"), "")
                    var inputAmt = parseInt(plainNumeric * decimalPointFactor);
                    inputAmt = inputAmt + '';
                    if (inputAmt.length >= MAXCHEQUEAMTDIGIT) {
                        event.cancelBubble = true;
                        event.returnValue = false;
                        event.keyCode = false;
                        return false;
                    }
                    //------------Part2------------
                    if ((value >= 48) && (value <= 57)) {
                        //objCurrentCtrl.focus();
                        //setSelectionRange(objCurrentCtrl);
                        var amt = objCurrentCtrl.value;
                        var caretPos = getCaretPos(objCurrentCtrl);
                        if ((caretPos == 0) && (String.fromCharCode(value) == '0')) {
                            event.cancelBubble = true;
                            event.returnValue = false;
                            event.keyCode = false;
                            return false;
                        }
                        var amtA = '' + amt.substring(0, caretPos);
                        var amtB = '' + amt.substring(caretPos, amt.length);
                        var amtNew = '' + String.fromCharCode(value);
                        objCurrentCtrl.value = amtA + amtNew + amtB;
                        objCurrentCtrl.value = toCurrency(objCurrentCtrl.value);
                    }
                        //------------Part3------------
                    else if ((value >= 96) && (value <= 105)) {
                        //objCurrentCtrl.focus();
                        //setSelectionRange(objCurrentCtrl);
                        var amt = objCurrentCtrl.value;
                        var caretPos = getCaretPos(objCurrentCtrl);
                        if ((caretPos == 0) && (String.fromCharCode(value - 48) == '0')) {
                            event.cancelBubble = true;
                            event.returnValue = false;
                            event.keyCode = false;
                            return false;
                        }
                        var amtA = '' + amt.substring(0, caretPos);
                        var amtB = '' + amt.substring(caretPos, amt.length);
                        var amtNew = '' + String.fromCharCode(value - 48);
                        objCurrentCtrl.value = amtA + amtNew + amtB;
                        objCurrentCtrl.value = toCurrency(objCurrentCtrl.value);
                    }
                }
                else if (objCurrentCtrl.name == "txtChequeAccountNumber") {
                    //------------Part2------------
                    if ((value >= 48) && (value <= 57)) {
                        //objCurrentCtrl.focus();
                        var amt = objCurrentCtrl.value;
                        var caretPos = getCaretPos(objCurrentCtrl);
                        var amtA = '' + amt.substring(0, caretPos);
                        var amtB = '' + amt.substring(caretPos, amt.length);
                        var amtNew = '' + String.fromCharCode(value);
                        amtNew = amtA + amtB + amtNew;
                    }
                        //------------Part3------------
                    else if ((value >= 96) && (value <= 105)) {
                        //objCurrentCtrl.focus();
                        var amt = objCurrentCtrl.value;
                        var caretPos = getCaretPos(objCurrentCtrl);
                        var amtA = '' + amt.substring(0, caretPos);
                        var amtB = '' + amt.substring(caretPos, amt.length);
                        var amtNew = '' + String.fromCharCode(value - 48);
                        amtNew = amtA + amtB + amtNew;
                    }
                    //------------Part4------------
                    // Validate Max Length Account Entry        
                    MAXCHEQUEACCDIGIT = "17";
                    if (parseInt(objCurrentCtrl.value.length) >= parseInt(MAXCHEQUEACCDIGIT)) {
                        event.cancelBubble = true;
                        event.returnValue = false;
                        event.keyCode = false;
                        var value = (window.external) ? event.keyCode : ev.keyCode;
                        // 46 - [DEL], 8 - [<-Backspace] then no need focus to [Add] button
                        if (value != 46 && value != 8) {
                            window.document.getElementById("btnAdd").focus();
                        }
                    }
                    else {
                        objCurrentCtrl.value = amtNew;
                    }
                }
                else {
                    //------------Other than Chq Amt, Acc No------------
                    return false;
                }
            }
            else if (value == 13) {
                if (jsChkInput()) {
                    document.getElementById('hAddFirstTime').value = "Y";
                    if (VerifyAccountNumber(g_obTxtVAcctNo.value)) {
                        //Start Add AccNo
                        AddAcctNo();
                        jsShowRows();
                        document.getElementById("txtChequeAccountNumber").focus();
                        document.getElementById("hfdDoneEntry").value = "true";
                    }
                }
            }

        }
    }

    function jsChkInput() {
        
        //Cheque Account Is Empty
        if (document.getElementById('txtChequeAccountNumber').value == '') {
            alert("Account Number cannot be Empty");
            document.getElementById("txtChequeAccountNumber").select();
            document.getElementById("txtChequeAccountNumber").focus();
            return false;
        }

        //Minimum Cheque Account Is Applied
        //if (document.getElementById('txtChequeAccountNumber').value.length > 13 && document.getElementById('txtChequeAccountNumber').value.length < 17) {
        //    alert("Account Number Must be 17 Characters.");
        //    document.getElementById("txtChequeAccountNumber").select();
        //    document.getElementById("txtChequeAccountNumber").focus();
        //    return false;
        //}

        //if (document.getElementById('txtChequeAccountNumber').value.length < 17 && document.getElementById('txtChequeAccountNumber').value.length != 13) {
        //    alert("GL Code Must be 13 Characters.");
        //    document.getElementById("txtChequeAccountNumber").select();
        //    document.getElementById("txtChequeAccountNumber").focus();
        //    return false;
        //}

        //if (document.getElementById('txtChequeAccountNumber').value.length < 17 && document.getElementById('txtChequeAccountNumber').value.length == 13) {
        //    var GLCode = document.getElementById('txtChequeAccountNumber').value;
        //    var isValidGL = ajaxCheckBranch(GLCode);
        //    if (isValidGL == false) {
        //        alert("Invalid Branch Detected in GL Code.");
        //        document.getElementById("txtChequeAccountNumber").select();
        //        document.getElementById("txtChequeAccountNumber").focus();
        //        return false;
        //    }
        //}

        //Cheque Amount Is Empty
        if (document.getElementById('txtChequeAmount').value == '') {
            document.getElementById("txtChequeAmount").select();
            document.getElementById("txtChequeAmount").focus();
            return false;
        }


        if (jsValidateInputCheckAmount(document.getElementById('txtChequeAmount').value) == false) {
            document.getElementById("txtChequeAmount").select();
            document.getElementById("txtChequeAmount").focus();
            return false;
        }

        return true;
    }
    function jsValidateInputCheckAmount(string) {

        //***************************************
        // allowed Special character list
        //***************************************
        //  0
        //  1
        //  2
        //  3
        //  4
        //  5
        //  6
        //  7
        //  8
        //  9 
        //  ,
        //  .
        //***************************************
        var strChar = ""
        var bolAllow = false
        var arrInput = new Array()
        var key_code = window.event.keyCode;
        var list = new Array()
        list[0] = "0"
        list[1] = "1"
        list[2] = "2"
        list[3] = "3"
        list[4] = "4"
        list[5] = "5"
        list[6] = "6"
        list[7] = "7"
        list[8] = "8"
        list[9] = "9"
        list[10] = ","
        list[11] = "."

        //string = string + ''
        arrInput = string.split("")

        //validate for special character existance

        for (var x = 0; x <= arrInput.length - 1; x++) {
            intfoundmatch = 0
            bolExceptionFound = false

            //check exception
            for (var y = 0; y <= list.length - 1; y++) {
                if (arrInput[x] == list[y]) {
                    intfoundmatch = intfoundmatch + 1
                }
            }
            if (intfoundmatch == 0) {
                bolExceptionFound = true
            }

            if (bolExceptionFound) {
                event.cancelBubble = true;
                event.returnValue = false;
                event.keyCode = false;
                alert("Input cannot contain any special characters \n");
                jsSelectWord();
                return false;
            }
        }


        return true
    }

    function jsSelectWord() {
        try {
            var oSource = window.event.srcElement;
            if (!oSource.isTextEdit)
                oSource = window.event.srcElement.parentTextEdit;
            if (oSource != null) {
                var oText = window.event.srcElement;
                oText.select();
                var oTextRange = oSource.createTextRange();
            }
        }
        catch (e) {
        }
    }

    function VerifyAccountNumber(intAcctNo) {

        // Ensure All Added Account(s) Number In Array Is Unique
        if (g_arrAcctNo.length > 0) {
            // loop all all item(s) in array
            for (var i = 0; i < g_arrAcctNo.length; i++) {

                var arrVAcctDetails = g_arrAcctNo[i].split("+");
                var bintItemID = arrVAcctDetails[0];
                var strVAcctNo = arrVAcctDetails[1];

                //---Previous = Entered Item, Current = Entered Item, CurrentMode - Add New Account Into List, prompt Duplicate
                if (intAcctNo == arrVAcctDetails[1] && i != g_intEditVAIndex) {
                    alert("Account Entry already exist in your list");
                    document.getElementById("txtChequeAccountNumber").value = "";
                    document.getElementById("txtChequeAmount").value = "";
                    g_intEditVAIndex = -1; //Workaround : Force edit to that account if it exist
                    return false;
                }
            }
        }
        return true;
    }
    // Add VA Account Entry
    function AddAcctNo() {
        // Declaration - Screen TextBox Value
        var strDataRows = "";
        var bintItemID = 0;
        var strVAcctNo = g_obTxtVAcctNo.value;
        var strAccName = g_obTxtAccName.value.toUpperCase();
        var strAccStatus = g_obTxtAccStatus.value.toUpperCase();
        var strDepAmt = g_obTxtDepAmt.value;
        var isIslamicOrConv = g_isIslamicOrConv.value;
        var chequeIsIslamicOrConv = g_chequeIsIslamicOrConv.value;
        //debugger;
        //alert('AddAcctNo');
        //alert(g_intEditVAIndex);
        // Get Index Of Any Empty Account Entry In Existing VAcct(g_arrAcctNo-holding added acct no)
        // 0  = Editing first record
        // -1 = Add New VAccount Into DB
        var intIndex = g_intEditVAIndex;
        if (intIndex == -1) {
            if (strVAcctNo != '') {
                var objhitemtype = window.document.getElementById("hitemtype");
                if (objhitemtype.value == "P" || objhitemtype.value == "p") {
                    alert("Payslip is selected, you are not allowed to Add More Accounts.");
                    $("#txtChequeAccountNumber").val("");
                    $("#txtChequeAmount").val("");
                    $("#txtChequeAccountNumber").focus();
                    return;
                }
                else {
                    g_arrAcctNo.push("0+" + strVAcctNo + "+0+V+++" + strAccName + "+" + strAccStatus + "+" + isIslamicOrConv + "+" + strDepAmt);
                }
            }

        }
            // Prepare To Update Selected Item's VAccount
        else {
            if (strVAcctNo != '') {
                var strLstVItemNo = g_arrAcctNo[intIndex];
                var arrVAcctDetails = strLstVItemNo.split("+");
                var bintItemID = arrVAcctDetails[0];
                //var strVAcctNo            = arrVAcctDetails[1]; 
                //var intDataFromDB         = 1 //Default to 1 -->For Updating Mode
                var intDataFromDB = arrVAcctDetails[2];
                var strItemType = arrVAcctDetails[3];
                var strReasonCode = arrVAcctDetails[4];
                var strRemark = arrVAcctDetails[5];

                //Added By Ying To cater leading zero changes                
                g_arrAcctNo[intIndex] = bintItemID + "+" + strVAcctNo + "+" + intDataFromDB + "+" + strItemType + "+" + strReasonCode + "+" + strRemark + "+" + strAccName + "+" + strAccStatus + "+" + isIslamicOrConv + "+" + strDepAmt;
            }
        }
        if (strVAcctNo != '') {
            //Enable Checkbox AccList Row for Editing 
            document.getElementById('hEnableButton').value = "Y";
            GenerateHtmlDataTable();
            UpdateHiddenValue();

            ResetVAEditMode(null, false);
            window.document.getElementById("txtChequeAccountNumber").value = "";   //Added to cater postback   
            window.document.getElementById("txtChequeAmount").value = "";   //Added to cater postback   
            window.document.getElementById("txtAccName").value = "";   //Added to cater postback  
            //window.document.getElementById("txtAccStatus").value = "";   //Added to cater postback   

            return true;
        }
    }

    // Reset VA Edit Mode
    function ResetVAEditMode(intIndex, blnOnlyOne) {
        // Reset Input Text VA Value , reset Edit Index Mode    
        g_obTxtVAcctNo.value = "";
        g_obTxtAccName.value = "";
        g_obTxtAccStatus.value = "";
        //g_obTxtDepAmt.value               = "";  
        g_obTxtChqAmt.value = addCommas(g_arrChqAmt[0]);
        g_intEditVAIndex = -1;
    }

    function UpdateHiddenValue() {

        var strAcctNoList = "";
        var objHfdChqAmt = window.document.getElementById("hfdChqAmt");
        var objHfdAcctNo = window.document.getElementById("hfdAccNo");

        if (parseInt(g_arrAcctNo.length) != 0) {
            for (var i = 0; i < g_arrAcctNo.length; i++) {
                var strLstVItemNo = g_arrAcctNo[i];
                var arrVAcctDetails = strLstVItemNo.split("+");
                var bintItemID = arrVAcctDetails[0];
                var strVAcctNo = arrVAcctDetails[1];
                var intDataFromDB = arrVAcctDetails[2];
                var strItemType = arrVAcctDetails[3];
                var strReasonCode = arrVAcctDetails[4];
                var strRemark = arrVAcctDetails[5];
                var strAccName = arrVAcctDetails[6];
                var strAccStatus = arrVAcctDetails[7];
                var strIslamicConvFlag = arrVAcctDetails[8];
                var strDepAmt = arrVAcctDetails[9];
                var Acc = "";
                var PayeeAccName = "";
                var PayeeAcccStatus = "";

                var table1 = document.getElementById('tblAcctNoListDetail');
                for (var aa = 0; aa < table1.tBodies.length; aa++) {
                    var rows = table1.tBodies[aa].rows;
                    for (var jj = 0; jj < rows.length; jj++) {
                        var cells = rows[jj].cells;
                        Acc = rows[jj].cells[5].childNodes[0].value;
                        if (Acc == strVAcctNo) {
                            PayeeAccName = rows[jj].cells[6].innerText;
                            PayeeAcccStatus = rows[jj].cells[7].innerText;
                        }
                    }
                }

                var strVAcctDetail = bintItemID + "+" + strVAcctNo + "+" + intDataFromDB + "+" + strItemType + "+" + strReasonCode + "+" + strRemark + "+" + strAccName + "+" + strAccStatus + "+" + strIslamicConvFlag + "+" + strDepAmt+ "+" + PayeeAccName + "+" + PayeeAcccStatus;
                if (strVAcctNo != "") {
                    if (i == 0)
                        strAcctNoList += strVAcctDetail;
                    else
                        strAcctNoList += ";" + strVAcctDetail;
                }
            }
            objHfdChqAmt.value = g_arrChqAmt[0];
            objHfdAcctNo.value = strAcctNoList;
            //alert(objHfdChqAmt.value);
            //alert(objHfdAcctNo.value);
        }
    }

    function jsIsAllCharsValid(strValue, strValidCharacterList) {
        var sChar;
        var intCount;

        for (var i = 0; i < strValue.length; i++) {
            sChar = strValue.charAt(i);
            // Value Not Exist In Accepted Value List	
            if (strValidCharacterList.indexOf(sChar) == -1) return false;
        }

        return true;
    }

    function toCurrency(amount) {
        var i;
        var rev;
        var amt;

        if (amount < 1) {
            rev = amount + '';
            amount = rev.substring(rev.indexOf('.') + 1, rev.length);
            amount = amount * 1;
        } else {
            rev = amount + '';
            var leadingSymbol = ",";
            var rev = rev.replace(new RegExp(leadingSymbol, "g"), "")
        }
        if (amount == 0) {
            return '';
        }

        rev = '';
        amt = '';
        amount = amount + '';
        i = amount.length - 1;
        for (i; i >= 0; i--) {
            if (amount.charAt(i) == '.') {
            }
            else if (amount.charAt(i) == ',') {
            }
            else {
                rev = rev + amount.charAt(i);
            }
        }

        if (rev.length > 2) {
            for (i = 0; i < rev.length; i++) {
                switch (i) {
                    case 2:
                        amt = amt + '.';
                        break;
                    case 5:
                        amt = amt + ',';
                        break;
                    case 8:
                        amt = amt + ',';
                        break;
                    case 11:
                        amt = amt + ',';
                        break;
                    case 14:
                        amt = amt + ',';
                        break;
                    case 17:
                        amt = amt + ',';
                        break;
                }
                amt = amt + rev.charAt(i);
            }
            rev = '';
            i = amt.length - 1;
            for (i; i >= 0; i--) {
                rev = rev + amt.charAt(i);
            }
        }
        else {
            if (rev.length == 1)
                amt = rev + '0.0';
            if (rev.length == 2)
                amt = rev + '.0';
            rev = '';
            i = amt.length - 1;
            for (i; i >= 0; i--) {
                rev = rev + amt.charAt(i);
            }
        }
        return rev;
    }
    // Generate A Set Of Going to Reject VAccount List 
    function PrepareRejectedList() {

        var objColCheckBox = window.document.getElementById("chkRejectVAcctNo");//window.document.frmFrontEndDataEntry.chkRejectVAcctNo;
        var objHfdRejectVAcctNoList = window.document.getElementById("hfdRejectVAcctList");
        var objChkBox;
        var intMaxLoop = 0;

        // Only 1 Checkbox
        if (isNaN(objColCheckBox.length)) {
            objHfdRejectVAcctNoList.value = "";
            UpdateRejectVAcctNoList(window.document.getElementById("chkRejectVAcctNo"), objHfdRejectVAcctNoList);
        }
        else {
            intMaxLoop = parseInt(objColCheckBox.length);
            objHfdRejectVAcctNoList.value = "";
            for (var i = 0; i < intMaxLoop; i++) {
                objChkBox = objColCheckBox[i];
                //alert('Prepare');
                //alert(i);
                //Update Only All Checked And Not Disabled Checkbox  
                if (objChkBox.disabled == false) {
                    UpdateRejectVAcctNoList(objChkBox, objHfdRejectVAcctNoList);
                }
            }
        }
    }

    function UpdateRejectVAcctNoList(objChkBox, objHfdRejectVAcctNoList) {
        var bintItemID = objChkBox.value;
        if (objChkBox.checked) {
            //alert('Here1');
            //alert(bintItemID);
            if (bintItemID != "") {
                //alert('Here1a');
                if (objHfdRejectVAcctNoList.value == "") {
                    //alert('Here1b');
                    objHfdRejectVAcctNoList.value = bintItemID;
                    //alert(objHfdRejectVAcctNoList.value);
                }
                else {
                    //alert('Here1c');
                    objHfdRejectVAcctNoList.value += ";" + bintItemID;
                    //alert(objHfdRejectVAcctNoList.value);
                }
            }
        }
            // Unchecked Item
        else {
            //alert('Here2');
            if (objHfdRejectVAcctNoList.value != "") {
                //alert('Here2a');
                var arrRejectVAcctList = objHfdRejectVAcctNoList.value.split(";");
                var arrUpdatedList = new Array();

                for (var i = 0; i < arrRejectVAcctList.length; i++) {
                    //alert('Here2b');
                    if (parseInt(arrRejectVAcctList[i]) != parseInt(bintItemID)) {
                        //alert('Here2bb');
                        arrUpdatedList.push(arrRejectVAcctList[i]);
                        //alert(arrUpdatedList.value);
                    }
                }

                if (objHfdRejectVAcctNoList.value != 0) {
                    //alert('Here2c');
                    objHfdRejectVAcctNoList.value = arrUpdatedList.join(";");
                    //alert(objHfdRejectVAcctNoList.value);
                }
                else {
                    //alert('Here2d');
                    objHfdRejectVAcctNoList.value = "";
                    //alert(objHfdRejectVAcctNoList.value);
                }
            }
        }
        //--------------Added By Ying---------------------
        window.document.getElementById("hfdRejectVAcctList").value = objHfdRejectVAcctNoList.value;
    }

    function ajaxCheckBranch(BranchCode) {
        var blnSuccess;
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "CommonApi/CheckBranchCode",
            method: "POST",
            data: "BranchCode=" + BranchCode,
            success: function (data) {
                if (data == true) {
                    blnSuccess = true;
                }
                else {
                    blnSuccess = false;
                }
            }
        });
        return blnSuccess;
    }
})();