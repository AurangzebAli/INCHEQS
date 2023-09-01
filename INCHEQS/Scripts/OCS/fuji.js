 var scnControl = document.getElementById('scnControl');
var myControl1 = document.getElementById('myControl1');

var strDriveName;
var strClientProcessDate;

var strFileName;
var strFolderName;
var strFilePath;

var g_intScannerId;
var g_intUserId;
var g_intSetZeroValue;
var g_intEjectErrorMICR;

var MaxItemPerBatch;
var _isAllowForceMICR;

var micrForceCtr = 0;
var iqaForceCtr = 0;
var removeCtr = 0;
var ActualScancount = 0;
(function () {

    $(document).ready(function () {
        //debugger
        //$("#capturingtitleindex").hide();
        //----------------------------------------------------------------------------------------------------------
        //-------BUTTON EVENTS
        //-----------------

        //alert(JSON.stringify(myControl1));
        //scan button
        jsIQACount();
        jsMICCount();
        $("#btnScan").on("click", function (e) {

            //debugger
            $("#divNotice").hide();
            $("#divWarning").hide();
            $("#divErrorMsg").hide();

            // if (confirm('Are you sure you want to import?')) {
            jsShowLoadingMsg("Scanning check in progess. Please wait ...", "1")
            var UIC;
            var UICStatus;
            var intBatchNo;
            var intSeqNo;
            var intScannerId;
            var intUserId;
            var intTaskId;
            var blnUpdateUICInfo;
            var strMICR;
            var strActualMICR;
            var intEndorseLine;
            var strScannerModel;

            var strCheckType;

            intUserId = $("hUserId").val();
            strMICR = null;//Define strMICR as null
            strActualMICR = null;
            strScannerModel = $("hScannerModel").val();

            if ($("#hScannerOn").val() == 1) {
                scnControl.InitConfig();
                $("#hScannerOn").val("");
                $("#hScannerOn").val(1);
            }

            //     If Err.number <> 0 Then
            //     Call jsAlert(jsRetrieveMessage("msgStrActiveXControlError5"))
            //     Call jsReloadPage()
            //     Exit Function
            //     End If

            intScannerId = $("#sScannerId").text().toString();
            intTaskId = $("#hTaskId").val();
            intEndorseLine = $("#hEndorseLine").val();
            scnControl.intEndorseLine = intEndorseLine;

            //Disable all the button
            $("#btnScan").prop("disabled", true);
            $("#btnEject").prop("disabled", true);
            $("#btnEndBatch").prop("disabled", true);
            $("#btnRemove").prop("disabled", true);
            $("#btnRemoveAll").prop("disabled", true);
            $("#btnIQA").prop("disabled", true);
            $("#btnRescan").prop("disabled", true);

            jsCreateDataXMLFile();
            scnControl.mLngRet = 0;//reset error code

            //*** Initial Start *****
            var strUIC;

            //Set Boolean
            var blnGetUICInfo;
            var blnDeleteUICInfo;
            var blnCheckMaxSeqValue;
            var blnDataEntry;
            var blnDataEntryResult;
            var blnQuitMaxSeqValue;
            var blnCheckMICR;
            var blnQuitCheckMICR;
            //Total counter to check maximum value
            var intTotalCounter;
            //Get Capture Mode and Cheque Type
            var strCaptureMode;
            var strChqType;
            var blnEditXML;
            var strUICNext;
            //*** Initial End *****

            //Check the max sequence value if 999 is will auto complete
            blnCheckMaxSeqValue = false;
            //All the capture mode must check MICR, besides cheque with deposit slip
            blnCheckMICR = false;
            //Cheque Only will happen immediate entry
            blnDataEntry = false;

            //Get Capture Mode
            strCaptureMode = $("#hSelectedCapMode").val();
            strChqType = $("hChqType").val();

            if (strCaptureMode == "CO" || strCaptureMode == "CR" || (strChqType == "99" && strCaptureMode != "CP")) {
                blnCheckMICR = true;
            }

            if (strCaptureMode == "CO" && $("#optEntryYes").prop("checked")) {
                //alert("check1");
                blnDataEntry = true;
            }

            if (scnControl.mLngRet == 0) {

                if (parseInt($("#sSeqNo").text()) != "") {
                    intTotalCounter = parseInt($("#hTotalCounter").val());
                }
                else {
                    //alert("seqNo1");
                    intTotalCounter = 0;//total counter for auto complete batch used
                    $("#hTotalCounter").val(0);
                }
                var strSpace;
                strSpace = "                      ";
                //Get UIC from database and build UIC
                intBranchId = parseInt($("#hWorkstationBranchId").val());
                blnGetUICInfo = ajaxGetUICInfoDataTableWithUpdate(intScannerId, intBranchId);
                strUIC = jsBuildUIC();

                if (strUIC == null) {
                    //("uic error");
                    $("#sErrorMsg").text("uic error");
                    $("#divErrorMsg").show();
                    $("#btnScan").prop("disabled", false);
                    $("#btnEject").prop("disabled", false);
                    $("#btnEndBatch").prop("disabled", false);
                    $("#btnRemove").prop("disabled", false);
                    $("#btnRemoveAll").prop("disabled", false);
                    $("#btnIQA").prop("disabled", false);
                    $("#btnRescan").prop("disabled", true);


                    return;
                }
                //alert("bb");
                //Set item configuration to scanner
                //1 = on; 0-off endorsement mode
                bEndorsement = jsGetScannerTuning("bEndorsement").toString();
                //alert(strCaptureMode);
                if (strCaptureMode == "IR") {
                    jsScanningItemConfig2(strUIC, 0);
                }
                else {
                    jsScanningItemConfig2(strUIC, bEndorsement);
                }


                //Quick scan                
                if (parseInt($("#hCompletedSeqNo").val()) - intTotalCounter == 0) {
                    scnControl.lngQuickTransport = 0;
                }
                //========================================================
                //================== START SCAN ==========================
                //========================================================

                //if (intTotalCounter >= ($("#hCompletedSeqNo").val())) {
                //    // debugger;
                //    blnCheckMaxSeqValue = true;
                //}

                // if ($("#sSeqNo").text() == 9999)
                //if (blnCheckMaxSeqValue == false) {

                //    scnControl.ScanCheque_quick();
                //    //alert("2");
                //    jsShowLoadingMsg("Loading Cheque data in progress...", "1");
                //}
                //else {
                //    $("#sErrorMsg").text("Maximum item per batch has been reached. Please end the batch.");
                //    $("#divErrorMsg").show();
                //}




                if (scnControl.mLngRet != 0) {
                    if (scnControl.mLngRet != 1) {
                        strScannerErrDesc = jsRetrieveScannerErr(scnControl.mLngRet);
                        //if (strScannerErrDesc == "Not Connected" && intTotalCounter > MaxItemPerBatch)
                        if (intTotalCounter >= MaxItemPerBatch) {
                            //("Maximum item per batch has been reached. Please end the batch.");
                            $("#sWarning").text("Maximum item per batch has been reached. Please end the batch.");
                            $("#divWarning").show();
                        }
                        else {
                            //(strScannerErrDesc);
                            $("#sErrorMsg").text(strScannerErrDesc);
                            $("#divErrorMsg").show();
                        }
                        jsShowLoadingMsg("", "1")
                        $("#txtStatus").val(strScannerErrDesc);

                    }

                    //if (scnControl.intCompleteCount == 0) {
                    //blnDeleteUICInfo = jsDeleteUICInfoSeqNo(intScannerId, intUserId, intTaskId);
                    //if (blnDeleteUICInfo == false) {

                    //    $("#btnScan").prop("disabled", false);
                    //    $("#btnEject").prop("disabled", false);
                    //    $("#btnEndBatch").prop("disabled", false);
                    //    $("#btnRemove").prop("disabled", false);
                    //    $("#btnRemoveAll").prop("disabled", false);
                    //    $("#btnIQA").prop("disabled", false);
                    //    $("#btnRescan").prop("disabled", true);
                    //    return;
                    //}
                    //}

                    if (scnControl.mLngRet != 1) {
                        $("#btnScan").prop("disabled", true);
                        $("#btnEject").prop("disabled", false);

                        if (strScannerModel != "1") {
                            $("#btnEndBatch").prop("disabled", true);
                            $("#btnRemove").prop("disabled", true);
                            $("#btnRemoveall").prop("disabled", true);
                        }
                        else {
                            $("#btnEndBatch").prop("disabled", false);
                            $("#btnRemove").prop("disabled", true);
                            $("#btnRemoveAll").prop("disabled", false);
                        }
                        $("#btnIQA").prop("disabled", true);
                        $("#btnRescan").prop("disabled", true);
                    }
                    else {
                        var SEQNum = parseInt($("#sSeqNo").text()) - 1;
                        SEQNum = SEQNum.toString();
                        $("#sSeqNo").text(leftPad(SEQNum, 4));

                        $("#btnScan").prop("disabled", false)
                        $("#btnEject").prop("disabled", false);

                        if (strScannerModel != "1") {
                            $("#btnEndBatch").prop("disabled", true);
                            $("#btnRemove").prop("disabled", true);
                            $("#btnRemoveAll").prop("disabled", true);
                        }
                        else {
                            $("#btnEndBatch").prop("disabled", false);
                            $("#btnRemove").prop("disabled", true);
                            $("#btnRemoveAll").prop("disabled", false);
                        }
                        $("#btnIQA").prop("disabled", true);
                        $("#btnRescan").prop("disabled", true);
                    }

                    if (scnControl.intCompleteCount == 0) {
                        $("#sErrorMsg").text("An error has occured during importing");
                        $("#divErrorMsg").show();
                        $("#btnEndBatch").prop("disabled", false);
                        $("#btnRemove").prop("disabled", false);
                        $("#btnRemoveAll").prop("disabled", false);
                        return;
                    }

                    //Auto Check IQA
                    // Check on data.xml consist of data, if yes, it will show endbatch button
                    var filesys, objfileSize, createdate;
                    filesys = CreateObject("Scripting.FileSystemObject");

                    //     If Err.number <> 0 Then
                    //     Call jsAlert(jsRetrieveMessage("msgStrActiveXControlError6"))
                    //     Call jsReloadPage()
                    //     Exit Function
                    //     End If

                    objfileSize = filesys.GetFile(strFilePath);
                    if (objfileSize.Size > 2000) {
                        $("#btnEndBatch").prop("disabled", false);
                        $("#btnRemove").prop("disabled", false);
                        $("#btnRemoveAll").prop("disabled", false);
                        $("#btnIQA").prop("disabled", false);
                        $("#btnRescan").prop("disabled", true);
                    }
                    else {
                        $("#btnEndBatch").prop("disabled", true);
                        $("#btnRemove").prop("disabled", false);
                        $("#btnRemoveAll").prop("disabled", true);
                        $("#btnIQA").prop("disabled", true);
                        $("#btnRescan").prop("disabled", true);
                    }

                    return;
                }






                //(Add NCF Required)
                var strNCFRequired;
                strNCFRequired = $("#hNCFRequired").val();

                //(Add Source Branch and FloatDays)
                var strBranchSource;
                var strFloatDays;
                strBranchSource = $("#hSourceBranch").val();
                strFloatDays = $("#hFloatDays").val();
                //Copystarthere

                if (scnControl.mLngRet == 0) {
                    do {
                        if (intTotalCounter < parseInt($("#hCompletedSeqNo").val())) {
                            blnCheckMaxSeqValue = false;
                            if (scnControl.mLngRet == 0) {
                                if (blnCheckMaxSeqValue == false) {

                                    scnControl.ScanCheque_quick();

                                    if (scnControl.mLngRet == 0) {
                                        ActualScancount++;
                                    }
                                    else if (scnControl.mLngRet == 1) {
                                        break;
                                    }

                                    //jsShowLoadingMsg("Loading Cheque data in progress...", "1");
                                    //strMICR = jsTrim(scnControl.strMICR);
                                    strMICR = ReplaceSymbolToAlphabet(jsTrim(scnControl.strActualMICR));
                                    $("#hMICR").val(strMICR);
                                    $("#hTotalCounter").val(intTotalCounter);

                                    blnEditXML = jsEditDataXmlNCF(strUIC, strMICR, intEndorseLine, strBranchSource, strFloatDays, strCaptureMode, "0");
                                    if (blnEditXML == false) {
                                        intTotalCounter = parseInt($("#hTotalCounter").val());//intTotalCounter - 1;
                                        jsCheckBatch();
                                        return;
                                    }

                                    var blnRefresh;
                                    blnQuitCheckMICR = jsCheckMICR(blnCheckMICR, strMICR, intTotalCounter);
                                    if (blnQuitCheckMICR == true) {
                                        if ($("#hRefreshPage").val() == "Y") {
                                            var blnRefresh;
                                            blnRefresh = false;
                                            if (blnRefresh == true) {
                                                return;
                                            }
                                        }
                                        jsCheckBatch();
                                        return;
                                    }
                                }
                                else {
                                    $("#sWarning").text("Maximum item per batch has been reached. Please end the batch.");
                                    $("#divWarning").show();
                                }


                                //intTotalCounter = parseInt($("#hTotalCounter").val());

                                if ($("#hRefreshPage").val() == "N") {
                                    if (blnDataEntry == true) {
                                        blnlDataEntryResult = jsShowImmediateEntry(strUIC);

                                        if (blnDataEntryResult == false) {
                                            jsCheckBatch();
                                            return;
                                        }
                                    }
                                }

                                //Get UIC from database and build UIC
                                //blnGetUICInfo = ajaxGetUICInfoDataTableWithUpdate(intScannerId);
                                blnGetUICInfo = ajaxGetUICInfoDataTableWithUpdate(intScannerId, intBranchId);
                                strUIC = jsBuildUIC();
                                if (strUIC == null) {
                                    $("#sErrorMsg").text("uic error");
                                    $("#divErrorMsg").show();
                                    jsCheckBatch();
                                    return;
                                }

                                $("#sSEqNo").text((leftPad(parseInt($("#sSeqNo").text()) + 1, 4)).toString());
                                // if ($("#sSeqNo").text() == 9999)
                                //if (intTotalCounter == ($("#hCompletedSeqNo").val()))
                                //{

                                //    blnCheckMaxSeqValue = true;
                                //}


                                $("#btnScan").prop("disabled", true);
                                $("#btnEject").prop("disabled", true);
                                if (strScannerModel != "1") {
                                    $("#btnEndBatch").prop("disabled", true);
                                    $("#btnRemove").prop("disabled", true);
                                    $("#btnRemoveAll").prop("disabled", true);
                                }
                                else {
                                    $("#btnEndBatch").prop("disabled", false);
                                    $("#btnRemove").prop("disabled", false);
                                    $("#btnRemoveAll").prop("disabled", false);
                                }
                                $("#btnIQA").prop("disabled", true);
                                $("#btnRescan").prop("disabled", true);

                                //Set item configuration to scanner
                                //1 = on; 0-off endorsement mode
                                bEndorsement = jsGetScannerTuning("bEndorsement").toString();
                                //alert(strCaptureMode);
                                if (strCaptureMode == "IR") {
                                    jsScanningItemConfig2(strUIC, 0);
                                }
                                else {
                                    jsScanningItemConfig2(strUIC, bEndorsement);
                                }

                                if ((parseInt($("#hCompletedSeqNo").val()) - intTotalCounter) == 1) {
                                    scnControl.lngQuickTransport = 0
                                }




                                if (scnControl.mLngRet != 0) {
                                    if (scnControl.mLngRet != 1) {
                                        strScannerErrDesc = jsRetrieveScannerErr(scnControl.mLngRet);
                                        //(strScannerErrDesc);
                                        $("#sErrorMsg").text(strScannerErrDesc);
                                        $("#divErrorMsg").show();
                                        $("#txtStatus").val("strScannerErrDesc");
                                        $("#btnScan").prop("disabled", true);
                                        $("#btnEject").prop("disabled", true);
                                        $("#btnEndBatch").prop("disabled", true);
                                        $("#btnRemove").prop("disabled", true);
                                        $("#btnRemoveAll").prop("disabled", true);
                                        $("#btnIQA").prop("disabled", true);
                                        $("#btnRescan").prop("disabled", true);
                                        //scnControl.mLngRet = 0;
                                    }
                                    else {
                                        var SEQNum = parseInt($("#sSeqNo").text()) - 1;
                                        SEQNum = SEQNum.toString();
                                        $("#sSeqNo").text(leftPad(SEQNum, 4));
                                    }
                                }

                            }
                            else {
                                $("#btnScan").prop("disabled", false);
                                $("#btnEject").prop("disabled", false);
                                $("#btnEndBatch").prop("disabled", false);
                                $("#btnRemove").prop("disabled", false);
                                $("#btnRemoveAll").prop("disabled", false);
                                $("#btnRescan").prop("disabled", false);
                            }
                            intTotalCounter = intTotalCounter + 1

                        }
                        //else
                        //{
                        //    strMICR = jsTrim(scnControl.strMICR);
                        //    $("#hMICR").val(strMICR);
                        //    $("#hTotalCounter").val(intTotalCounter);
                        //    blnEditXML = jsEditDataXmlNCF(strUIC, strMICR, intEndorseLine, strBranchSource, strFloatDays, strCaptureMode, "0");
                        //    if (blnEditXML == false) {
                        //        intTotalCounter = parseInt($("#hTotalCounter").val());//intTotalCounter - 1;
                        //        jsCheckBatch();
                        //        return;
                        //    }
                        //    blnQuitCheckMICR = jsCheckMICR(blnCheckMICR, strMICR, intTotalCounter);
                        //    if (blnQuitCheckMICR == true) {
                        //        if ($("#hRefreshPage").val() == "Y") {
                        //            var blnRefresh;
                        //            blnRefresh = false;
                        //            if (blnRefresh == true) {
                        //                return;
                        //            }
                        //        }
                        //        jsCheckBatch();
                        //        return;
                        //    }
                        //    intTotalCounter = parseInt($("#hTotalCounter").val());

                        //    if ($("#hRefreshPage").val() == "N") {
                        //        if (blnDataEntry == true) {
                        //            blnlDataEntryResult = jsShowImmediateEntry(strUIC);

                        //            if (blnDataEntryResult == false) {
                        //                $("#btnScan").prop("disabled", false);
                        //                $("#btnEject").prop("disabled", false);
                        //                $("#btnEndBatch").prop("disabled", false);
                        //                $("#btnRemove").prop("disabled", false);
                        //                $("#btnRemoveAll").prop("disabled", false);
                        //                $("#btnRescan").prop("disabled", false);
                        //                return;
                        //            }
                        //        }
                        //    }
                        //    if (intTotalCounter = parseInt($("#hCompletedSeqNo").val())) {
                        //        //Prompt pop-up box to mention has reached the auto completed limit
                        //        var strQuery;
                        //        var blnResult;
                        //        if (strCaptureMode != "CP") {
                        //            if ($("#hPromptMaxMessasge").val() == 1) {
                        //                $("#hPromptMaxMessage").val(2);

                        //                //blnReturnResult = ("Batch Ended");
                        //                $("#sNotice").text("Batch Ended");
                        //                $("#divNotice").show();
                        //                blnReturnResult = true;
                        //            }
                        //            else {
                        //                blnReturnResult = false;
                        //            }

                        //            if (blnReturnResult == true) {
                        //                $("#btnScan").prop("disabled", true);//frmCapturing.btnScan.disabled = true
                        //                $("#btnEject").prop("disabled", true);//frmCapturing.btnEject.disabled = true                            

                        //                $("#btnEndBatch").prop("disabled", false);//frmCapturing.btnEndBatch.disabled = false
                        //                $("#btnRemove").prop("disabled", false);//frmCapturing.btnRemove.disabled = false
                        //                $("#btnRemoveAll").prop("disabled", false);//frmCapturing.btnRemoveAll.disabled = false
                        //                /* $("#btnIQA").prop("disabled", false);*///frmCapturing.btnIQA.disabled= false
                        //                $("#btnRescan").prop("disabled", true);//frmCapturing.btnRescan.disabled = true
                        //                return;
                        //            }
                        //            else {
                        //                $("#btnScan").prop("disabled", false);//frmCapturing.btnScan.disabled = false
                        //                $("#btnEject").prop("disabled", false);//frmCapturing.btnEject.disabled = false
                        //                $("#btnEndBatch").prop("disabled", false);//frmCapturing.btnEndBatch.disabled = false
                        //                $("#btnRemove").prop("disabled", false);//frmCapturing.btnRemove.disabled = false
                        //                $("#btnRemoveAll").prop("disabled", false);//frmCapturing.btnRemoveAll.disabled = false
                        //                /*  $("#btnIQA").prop("disabled", false);*///frmCapturing.btnIQA.disabled= false
                        //                $("#btnRescan").prop("disabled", true);//frmCapturing.btnRescan.disabled = true
                        //                return;
                        //            }
                        //        }
                        //        else {
                        //            if ($("#hPromptMaxMessage").val() == 1) {
                        //                $("#hPromptMaxMessage".val(2));
                        //                //strQuery = jsRetrieveMessage("msgStrMaximumItemPerBatch")

                        //                //blnReturnResult = jsAlert(strQuery , null, "Batch Ended")
                        //                blnReturnResult = true;//blnReturnResult = true
                        //            }
                        //            else {
                        //                blnReturnResult = false;
                        //            }

                        //            // if yes, end the batch
                        //            // if no, will not end the batch
                        //            if (blnReturnResult = true) {
                        //                $("#btnScan").prop("disabled", true);
                        //                $("#btnEject").prop("disabled", true);

                        //                $("#btnEndBatch").prop("disabled", false);
                        //                $("#btnRemove").prop("disabled", false);
                        //                $("#btnRemoveAll").prop("disabled", false);
                        //                //$("#btnIQA").prop("disabled", false);
                        //                $("#btnRescan").prop("disabled", true);
                        //                return;
                        //            }
                        //            else if (blnReturnResult == false) {
                        //                $("#btnScan").prop("disabled", false);
                        //                $("#btnEject").prop("disabled", false);
                        //                $("#btnEndBatch").prop("disabled", false);
                        //                $("#btnRemove").prop("disabled", false);
                        //                $("#btnRemoveAll").prop("disabled", false);
                        //                //$("#btnIQA").prop("disabled", false);
                        //                $("#btnRescan").prop("disabled", true);
                        //                return;
                        //            }
                        //        }
                        //    }
                        //} 
                        //intTotalCounter = intTotalCounter + 1
                    }
                    //while (scnControl.mLngRet == 0)
                    while (intTotalCounter < parseInt($("#hCompletedSeqNo").val()))
                    if (scnControl.mLngRet != 1) {
                        if (ActualScancount == parseInt($("#hCompletedSeqNo").val()) || intTotalCounter > parseInt($("#hCompletedSeqNo").val())) {
                            $("#sWarning").text("Maximum item per batch has been reached. Please end the batch.");
                            $("#divWarning").show();
                        }
                    }

                }


                //$("#btnScan").prop("disabled", true);
                //$("#btnEject").prop("disabled", true);
                //$("#btnEndBatch").prop("disabled", true);
                //$("#btnRemove").prop("disabled", true);
                //$("#btnRemoveAll").prop("disabled", true);
                ////$("#btnIQA").prop("disabled", true);
                //$("#btnRescan").prop("disabled", true);

                //if (scnControl.intCompleteCount > 0) {
                //    var i, intCount;
                //    intCount = scnControl.intCompleteCount - 1;

                //    jsShowLoadingMsg("Loading Cheque data in progress[0 % ]", "1");
                //    for (i = 0; i <= intCount; i++) {
                //        scnControl.GetBatchScanResult(i);
                //        strMICR = jsTrim(scnControl.strMICR);
                //        strUIC = scnControl.strUIC;
                //        $("#hMICR").val(strMICR);
                //        $("#hTotalCounter").val(intTotalCounter + i);

                //        blnEditXML = jsEditDataXmlNCF(strUIC, strMICR, intEndorseLine, strBranchSource, strFloatDays, strCaptureMode, "0");

                //        if (blnEditXML == false) {
                //            intTotalCounter = intTotalCounter - 1;
                //            jsCheckBatch();
                //            return;
                //        }
                //        var blnRefresh;
                //        blnQuitCheckMICR = jsCheckMICR(blnCheckMICR, strMICR, intTotalCounter);
                //        if ($("#hRefreshPage").val() == "N") {
                //            if (blnDataEntry == true) {
                //                blnlDataEntryResult = jsShowImmediateEntry(strUIC);

                //                if (blnDataEntryResult == false) {
                //                    jsCheckBatch();
                //                    return;
                //                }
                //            }
                //        }
                //        $("#sSEqNo").text((parseInt($("#sSeqNo").text()) + 1).toString());
                //        if ($("#sSeqNo").text() == 100) {
                //            blnCheckMaxSeqValue = true;
                //        }

                //        $("#btnScan").prop("disabled", true);
                //        $("#btnEject").prop("disabled", true);
                //        if (strScannerModel != "1") {
                //            $("#btnEndBatch").prop("disabled", true);
                //            $("#btnRemove").prop("disabled", true);
                //            $("#btnRemoveAll").prop("disabled", true);
                //        }
                //        else {
                //            $("#btnEndBatch").prop("disabled", false);
                //            $("#btnRemove").prop("disabled", false);
                //            $("#btnRemoveAll").prop("disabled", false);
                //        }
                //        //$("#btnIQA").prop("disabled", true);
                //        $("#btnRescan").prop("disabled", true);

                //        jsShowLoadingMsg("Loading Cheque data in progress[" + ((i / intCount) * 100) + " % ]", "1");

                //    }
                //    jsShowLoadingMsg("Loading Cheque data in progress[100 % ]", "1");

                //}
            }
            if (blnCheckMaxSeqValue == false) {
                jsShowLoadingMsg("", "1");
                jsXMLDuplicatedMICR();

                jsModifyIQA();
                jsIQACount();
                jsMICCount();
                jsHouseChq();


            }



            $("#btnScan").prop("disabled", false);
            $("#btnEject").prop("disabled", false);
            $("#btnEndBatch").prop("disabled", false);
            $("#btnRemove").prop("disabled", false);
            $("#btnRemoveAll").prop("disabled", false);
            $("#btnIQA").prop("disabled", false);
            $("#btnRescan").prop("disbled", true);
            // alert("0");
            jsLoadcheckControl();

            //}
        })

        //eject button
        $("#btnEject").on("click", function (e) {
            $("#divNotice").hide();
            $("#divWarning").hide();
            $("#divErrorMsg").hide();

            jsEject();

        })

        //rescan button 
        $("#btnRescan").on("click", function (e) {
            //alert("Reimport");
            // debugger;
            $("#divNotice").hide();
            $("#divWarning").hide();
            $("#divErrorMsg").hide();

            //  if (confirm('Are you sure you want to re-import?')) {
            var strUIC;
            var strMICR;

            strUIC = $("#hRowClickUIC").val();

            var blnDataEntry;
            var blnDataEntryResult;
            var strCaptureMode;

            blnDataEntry = false;
            blnDataEntryResult = false;

            //Get Capture Mode
            strCaptureMode = $("hSelectedCapMode").val();

            if (strCaptureMode == "CO" && $("#optEntryYes").prop("checked", true)) {
                //alert("check2");
                blnDataEntry = true;
                blnCheckMICR = true;
            }
            else if (strCaptureMode == "CO") {
                blnCheckMICR = true;
            }
            else if (strCaptureMode = "CR") {
                blnCheckMICR = true;
            }

            if (scnControl.mLngRet <= 0) {
                scnControl.InitConfig();
            }

            //If Err.number <> 0 Then
            //Call jsAlert(jsRetrieveMessage("msgStrActiveXControlError4"))
            //Call jsReloadPage()
            //Exit Function
            //End If
            // alert("Before Scanning Item Config");
            jsScanningItemConfig(strUIC, strUIC, 0);//use back the same UIC no.
            // alert("After Scanning Item Config");
            //Quick scan
            scnControl.lngQuickTransport = 0;

           // scnControl.ScanCheque();
            scnControl.ScanCheque_quick();

            scnControl.lngQuickTransport = 0;

            if (scnControl.mLngRet == 0) {

                //strMICR = jsTrim(scnControl.strMICR);
                strMICR = ReplaceSymbolToAlphabet(jsTrim(scnControl.strActualMICR));
                //alert(strMICR);
                $("#hMICR").val(strMICR);
                blnQuitCheckMICR = jsCheckMICR(blnCheckMICR, strMICR, $("#hTotalCounter").val());

                if (blnQuitCheckMICR == true) {
                    if ($("#hRefreshPage").val() == "Y") {
                        var blnRefresh;
                        blnRefresh = false;
                        if (blnRefresh == true) {
                            return;
                        }
                    }
                    jsCheckBatch();
                    return;
                }

                // Reinsert into data.xml for MICR value
                jsReinsertXML(0);

                if ($("hRefreshPage").val() == "N") {
                    if (blnDataEntry == true) {
                        blnDataEntryResult = jsShowImmediateEntry(strUIC);
                        if (blnDataEntryResult == false) {
                            $("#btnScan").prop("disabled", false);
                            $("#btnEject").prop("disabled", false);
                            $("#btnEndBatch").prop("disabled", false);
                            $("#btnRemove").prop("disabled", false);
                            $("#btnRemoveAll").prop("disabled", false);
                            $("#btnIQA").prop("disabled", false);
                            $("#btnRescan").prop("disabled", true);

                            return;
                        }
                    }
                }
                jsXMLDuplicatedMICR();
                jsModifyIQA();
                jsIQACount();
                jsMICCount();
                jsHouseChq();
                var intTotalCounter;
                intTotalCounter = parseInt($("#hTotalCounter").val());

                // Check whether has reached maximum limit batch item
                if (intTotalCounter = parseInt($("#hCompletedSeqNo").val())) {
                    //Prompt pop-up box to mention has reached the auto completed limit
                    var strQuery;
                    var blnResult;
                    if (strCaptureMode != "CP") {
                        if ($("#hPromptMaxMessasge").val() == 1) {
                            $("#hPromptMaxMessage").val(2);

                            //blnReturnResult = ("Batch Ended");
                            $("#sNotice").text("Batch Ended");
                            $("#divNotice").show();
                            blnReturnResult = true;
                        }
                        else {
                            blnReturnResult = false;
                        }

                        if (blnReturnResult == true) {
                            $("#btnScan").prop("disabled", true);//frmCapturing.btnScan.disabled = true
                            $("#btnEject").prop("disabled", true);//frmCapturing.btnEject.disabled = true                            

                            $("#btnEndBatch").prop("disabled", false);//frmCapturing.btnEndBatch.disabled = false
                            $("#btnRemove").prop("disabled", false);//frmCapturing.btnRemove.disabled = false
                            $("#btnRemoveAll").prop("disabled", false);//frmCapturing.btnRemoveAll.disabled = false
                            $("#btnIQA").prop("disabled", false);//frmCapturing.btnIQA.disabled= false
                            $("#btnRescan").prop("disabled", true);//frmCapturing.btnRescan.disabled = true
                            return;
                        }
                        else {
                            $("#btnScan").prop("disabled", false);//frmCapturing.btnScan.disabled = false
                            $("#btnEject").prop("disabled", false);//frmCapturing.btnEject.disabled = false
                            $("#btnEndBatch").prop("disabled", false);//frmCapturing.btnEndBatch.disabled = false
                            $("#btnRemove").prop("disabled", false);//frmCapturing.btnRemove.disabled = false
                            $("#btnRemoveAll").prop("disabled", false);//frmCapturing.btnRemoveAll.disabled = false
                            $("#btnIQA").prop("disabled", false);//frmCapturing.btnIQA.disabled= false
                            $("#btnRescan").prop("disabled", true);//frmCapturing.btnRescan.disabled = true
                            return;
                        }
                    }
                    else {
                        if ($("#hPromptMaxMessage").val() == 1) {
                            $("#hPromptMaxMessage".val(2));
                            //strQuery = jsRetrieveMessage("msgStrMaximumItemPerBatch")

                            //blnReturnResult = jsAlert(strQuery , null, "Batch Ended")
                            blnReturnResult = true;//blnReturnResult = true
                        }
                        else {
                            blnReturnResult = false;
                        }

                        // if yes, end the batch
                        // if no, will not end the batch
                        if (blnReturnResult = true) {
                            $("#btnScan").prop("disabled", true);
                            $("#btnEject").prop("disabled", true);

                            $("#btnEndBatch").prop("disabled", false);
                            $("#btnRemove").prop("disabled", false);
                            $("#btnRemoveAll").prop("disabled", false);
                            $("#btnIQA").prop("disabled", false);
                            $("#btnRescan").prop("disabled", true);
                            return;
                        }
                        else if (blnReturnResult == false) {
                            $("#btnScan").prop("disabled", false);
                            $("#btnEject").prop("disabled", false);
                            $("#btnEndBatch").prop("disabled", false);
                            $("#btnRemove").prop("disabled", false);
                            $("#btnRemoveAll").prop("disabled", false);
                            $("#btnIQA").prop("disabled", false);
                            $("#btnRescan").prop("disabled", true);
                            return;
                        }
                    }
                }
                jsReCapture = true;
            }
            else {
                jsReCapture = false;
            }
            //if (confirm('Are you sure want to Re-Import for this Cheque?')) {
            //    jsReCapture();
            //    jsRefreshPage();
            //} else {
            //    return;
            //}
            // }
        })

        //force iqa pass button
        //$("#btnForceIQAPass").on("click", function (e) {
        //    jsForceIQAPass();
        //})
        $("#btnForceIQAPass").on("click", function (e) {
            $("#divNotice").hide();
            $("#divWarning").hide();
            $("#divErrorMsg").hide();

            //  if (confirm('Are you sure you want to force the cheque(s) failed IQA to pass ?')) {
            jsForceIQAPass();
            if (iqaForceCtr > 0) {
                alert("Failed IQA cheque has successfully forced to pass");
            }


            jsRefreshPage();
            jsIQACount();
            jsMICCount();


            //jsRefreshPage();
            // } else {
            //    return;
            // }
        })

        ////force micr button
        //$("#btnForceMICRPass").on("click", function (e) {
        //    jsForceMICRPass();
        //})
        $("#btnForceMICRPass").on("click", function (e) {
            // debugger
            $("#divNotice").hide();
            $("#divWarning").hide();
            $("#divErrorMsg").hide();

            //  if (confirm('Are you sure want to Force MICR Pass for this Cheque?')) {
            jsForceMICRPass();

            if (micrForceCtr > 0) {
                alert("MICR has been forced to pass");
            }

            jsRefreshPage();
            jsIQACount();
            jsMICCount();

            //  } else {
            //      return;
            // }
        })

        //remove button
        //$("#btnRemove").on("click", function (e) {
        //    jsDeleteDataXMLFilePerUIC();
        //    jsRefreshPage();
        //})
        $("#btnRemove").on("click", function (e) {
            //debugger
            $("#divNotice").hide();
            $("#divWarning").hide();
            $("#divErrorMsg").hide();

            jsDeleteDataXMLFilePerUIC();
            if (removeCtr > 0) {
                alert("The selected cheque(s) has been removed");

            }

            jsIQACount();
            jsMICCount();
            //alert("The selected cheque(s) has been removed");

        })

        //remove all button
        $("#btnRemoveAll").on("click", function (e) {
            $("#divNotice").hide();
            $("#divWarning").hide();
            $("#divErrorMsg").hide();

            jsDeleteDataXMLFile();
            //alert("All the cheque has been removed");
            jsRefreshPage();
            //alert("All the cheque has been removed");

        })

        //end batch button
        $("#btnEndBatch").on("click", function (e) {
            //debugger
            $("#divNotice").hide();
            $("#divWarning").hide();
            $("#divErrorMsg").hide();

            var blnGetUICInfo;
            var strUIC;
            var intScannerId;
            var intTaskId;
            var intUserId;
            var strTransAmt;
            var strMICR;

            if ($("#hSelectedCapMode").val() == "SO") {
                //jsEditDataXml_SlipMode();
            }
            else if ($("#hSelectedCapMode").val() == "TC") {
                //strTransAmt = $("#txtTransAmount").val(); 
                //if (strTransAmt == "") {
                //    alert("Transmittal Amount must be more than 0!");
                //    return;
                //}
                //else if (strTransAmt <= 0) {
                //    alert("Transmittal Amount must be more than 0!");
                //    return;
                //}
                //$("#hTransAmount").val("");
                //$("#hTransUIC").val("");


                //intScannerId = Cstr($("sScannerId").text());
                //intUserId = $("#hUserId").val();
                //intTaskId = $("#hTaskId").val();

                ////Get UIC from database and build UIC
                //blnGetUICInfo = jsGetUICInfo(intScannerID, intUserId, intTaskId);
                //strUIC = jsBuildUIC();
                //strTransAmt = $("#txtTransAmount").val();


                //jsEditDataXml_TransModeData(strUIC, strTransAmt);
            }
            if (confirm('Are you sure you want to end batch?')) {
                jsCompleteBatch(4);
                //jsRefreshPage();
            } else {
                return;
            }
            //jsCompleteBatch(4);
            //jsRefreshPage();
        })

        //close button
        $("#btnClose").on("click", function (e) {
            //$("#divNotice").hide();
            //$("#divWarning").hide();
            //$("#    divErrorMsg").hide();

            jsExitScanner();
            $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").show();
        })

        //iqa button
        $("#btnIQA").on("click", function (e) {
            jsModifyIQA();
        })



        //----------------------------------------------------------------------------------------------------------
        //-------INITIALIAZE
        //-----------------

        jsInit();



        //----------------------------------------------------------------------------------------------------------
        //-------FUNCTIONS
        //-----------------

        //initialize page
        function jsInit() {
            //debugger;
            //MaxItemPerBatch = 9999;
            MaxItemPerBatch = ($("#hCompletedSeqNo").val());
            _isAllowForceMICR = $("#hAllowForceMICR").val();


            $("#btnEndBatch").prop("disabled", true);
            $("#btnRemove").prop("disabled", true);
            $("#btnRemoveAll").prop("disabled", true);

            strDriveName = $("#hCapturingPath").val();
            strClientProcessDate = jsFormatProcessDateToDigit($("#sProcessingDate").text());

            strFileName = "data.xml";
            strFolderName = strDriveName + strClientProcessDate + "\\spool";
            strFilePath = strFolderName + "\\" + strFileName;

            $("#hCaptureFileLocation").val(strFilePath);
            g_intScannerId = $("#sScannerId").text();
            g_intUserId = $("#hUserId").val();
            g_intSetZeroValue = $("#hZeroValue").val();
            g_intEjectErrorMICR = 1;

            //-------Check if Charge Slip -> default as NONCICS
            if ($("#hSelectedCapMode").val() == "CS") {
                // alert("check3");
                $("#optBothCICS").prop("disabled", true);
                $("#optCICSMode").prop("disabled", true);
                $("#optNONCICSMode").prop("disabled", false);

                $("#optBothCICS").prop("checked", false);
                $("#optCICSMode").prop("checked", false);
                $("#optNONCICSMode").prop("checked", true);
            }

            //-------Initialize Scanner
            var blnResult = jsInitScanner();
            if (blnResult == false) {
                return false;
            }

            //-------XML
            var objXMLField = $("#xmlFields");
            var objTblFooter = document.getElementById("tblFooter");
            var objTblBody = document.getElementById("tblBody");
            var objDivScanLog = $("#divScanLog");

            var blnLoadXMLStatus = jsLoadXMLDoc();

            if (blnLoadXMLStatus == false) {
                $("#btnEndBatch").prop("disabled", false);
                $("#btnRemove").prop("disabled", false);
                $("#btnRemoveAll").prop("disabled", false);
                $("#btnIQA").prop("disabled", false);
                $("#btnRescan").prop("disabled", true);
            }
            else {
                objDivScanLog.show();
                jsBindLineItems(objXMLField, objTblFooter, objTblBody);
            }

            var objPriority = $("#hPriority").val();
            var objImmediateEntry = $("#hImmediateEntry").val();
            var strSelectedCaptureMode = $("#hSelectedCapMode").val();

            var strFloatDays = "00";
            $("#hFloatDays").val(strFloatDays);

            //-------Check Priority
            //if (objPriority != "") {
            //    if (objPriority > 1) {
            //        $("#optDirectMark").prop("checked", true);
            //        $("#optDirectMark").val(objPriority);
            //        if (objPriority > 3) {
            //            jsUncheckOptionButton(optPriority);
            //        }
            //    }
            //}
            //else {
            //    jsUncheckOptionButton(optPriority);
            //}
            //<tr runat="server" id="trImmediateEntry" style="display:none;">

            if (strSelectedCaptureMode == "CO") {
                if (objImmediateEntry != "") {
                    if (objImmediateEntry == "Y") {
                        // alert("check5");
                        $("#optEntryYes").prop("checked", true);
                    }
                    else {
                        // alert("check6");
                        $("#optEntryNo").prop("checked", true);
                    }
                }
            }

            if (($("#sBatchNo").text() != "") && (parseInt($("#sSeqNo").text()) != "")) {
                var intScannerId
                var intBatchNo
                var intSeqNo
                var intUserId
                var blnUpdateUICInfo

                intUserId = $("#hUserId").val();

                if (parseInt($("#sBatchNo").text(), 10) > parseInt($("#hServerBatchNo").val(), 10)) {
                    intScannerId = $("#sScannerId").text();
                    intBatchNo = parseInt($("#sBatchNo").text())
                    intSeqNo = parseInt($("#sSeqNo").text());
                    intBranchId = parseInt($("#sClrBranchId").text());
                    if (intSeqNo == "NaN") intSeqNo = 0;

                    intTaskId = $("#hTaskId").val();
                    //blnUpdateUICInfo = ajaxUpdateUICInfo(intScannerId, intBatchNo, intSeqNo, intUserId, intBranchId);
                    //if (blnUpdateUICInfo == false) {
                    //    //("Fail to update Batch No. and Sequence No. Please check your connection.")
                    //    $("#sErrorMsg").text("Fail to update Batch No. and Sequence No. Please check your connection.");
                    //    $("#divErrorMsg").show();
                    //}
                }
                else if (parseInt($("#sBatchNo").text(), 10) == parseInt($("#hServerBatchNo").val(), 10)) {
                    if (parseInt($("#sSeqNo").text(), 10) > parseInt($("#hServerSeqNo").val(), 10)) {
                        intScannerId = $("#sScannerId").text();
                        intBatchNo = $("#sBatchNo").text()
                        intSeqNo = $("#sSeqNo").text();
                        intBranchId = parseInt($("#sClrBranchId").text());
                        if (intSeqNo == "NaN") { intSeqNo = 0; }

                        intTaskId = $("#hTaskId").val();

                        //blnUpdateUICInfo = ajaxUpdateUICInfo(intScannerId, intBatchNo, intSeqNo, intUserId, intBranchId);

                        //if (blnUpdateUICInfo == false) {
                        //    //("Fail to update Batch No. and Sequence No. Please check database connection.");
                        //    $("#sErrorMsg").text("Fail to update Batch No. and Sequence No. Please check database connection.");
                        //    $("#divErrorMsg").show();
                        //}
                    }
                }
                else if (parseInt($("#sBatchNo").text(), 10) < parseInt($("#hServerBatchNo").val(), 10)) {
                    $("#sBatchNo").text($("#hServerBatchNo").val());
                    $("#sSeqNo").text(leftPad($("#hSeqBatchNo").val(), 4));
                }
            }
            //alert("00");
            jsLoadcheckControl();
        }

        //Format process date
        function jsFormatProcessDateToDigit(strDate) {
            var d = new Date(strDate);

            var mValue = d.getMonth() + 1;
            var dValue = d.getDate();

            var strMonth;
            var strDay;

            strMonth = mValue.toString();
            strDay = dValue.toString();

            if (strMonth.length < 2) {
                strMonth = "0" + strMonth;
            }

            if (strDay.length < 2) {
                strDay = "0" + strDay;
            }

            return d.getFullYear() + strMonth + strDay;
        }

        //Load XML document
        function jsLoadXMLDoc() {
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
            }
            catch (e) {
                //("Fail to load local file");
                $("#sErrorMsg").text("Fail to load local file");
                $("#divErrorMsg").show();
            }

            if (objFS.FileExists(strFilePath)) {
                var f = objFS.GetFile(strFilePath);
                var size = f.size;
                if (size) {
                    var objFile = objFS.OpenTextFile(strFilePath);
                    var outputDoc = objFile.readAll();
                    objFile.close();

                    $("#xmlFields").html(outputDoc);
                    return true;
                }
            }
            else {
                return false;
            }



            ////======================

            //        try {
            //            var xmlDoc;
            //            if (window.DOMParser) {
            //                // code for modern browsers
            //                parser = new DOMParser();
            //                xmlDoc = parser.parseFromString(outputDoc, "text/xml");
            //            } else {
            //                // code for old IE browsers
            //                xmlDoc = new ActiveXObject("Microsoft.FreeThreadedXMLDOM");
            //                xmlDoc.async = false;
            //                xmlDoc.loadXML(outputDoc);
            //            }
            //        }
            //        catch (e) {
            //            alert("Fail to load xml file: " + e);
            //        }
            ////==============================
            //try {
            //    var objFS = new ActiveXObject("Scripting.FileSystemObject");
            //}
            //catch (e) {
            //    alert("Fail to load xml file");
            //    return false;
            //}
            ////alert(strFilePath + "#strFilePath");
            //if (objFS.FileExists(strFilePath)) {
            //    var f = objFS.GetFile(strFilePath);
            //    var size = f.size;
            //    //alert("fileexist");
            //    if (size) {
            //        //alert("size");
            //        var objFile = objFS.OpenTextFile(strFilePath);
            //        var outputDoc = objFile.readAll();
            //        objFile.close();

            //        try {
            //            var xmlDoc = new ActiveXObject("Microsoft.FreeThreadedXMLDOM");
            //        }
            //        catch (e) {
            //            jsAlert(jsRetrieveMessage("msgStrActiveXControlError"))
            //            alert("Fail to load xml file");
            //        }
            //        xmlDoc.async = "false";
            //        xmlDoc.loadXML(outputDoc);

            //        var objChequeInfo = xmlDoc.documentElement;
            //        document.getElementById('xmlFields').innerHTML = objChequeInfo.xml;

            //        return true
            //    }
            //}
            //else {
            //    //alert("file not exists");
            //    return false;
            //}
        }

        //Load check control
        function jsLoadcheckControl() {
            var exceptionMsg = '';
            var checkResult = 0;
            var foundDepositSlip = 0;
            try {

                var objFS = new ActiveXObject("Scripting.FileSystemObject")
                var i
                if (objFS.FileExists(strFilePath)) {
                    // alert("1");
                    var f = objFS.GetFile(strFilePath);
                    var size = f.size;
                    if (size) {
                        //alert("2");
                        var objFile = objFS.OpenTextFile(strFilePath)
                        var outputDoc = objFile.readAll();
                        objFile.close()

                        //var xmlDoc = new ActiveXObject("MICROSOFT.FreeThreadedXMLDOM");
                        //xmlDoc.async = "false";
                        //xmlDoc.loadXML(outputDoc);

                        var parser = new DOMParser();
                        var xmlDoc = parser.parseFromString(outputDoc, "text/xml");


                        var nodeList = xmlDoc.getElementsByTagName("data");
                        var strDataXmlUIC;
                        if (nodeList.length > 0) {
                            // alert("3");
                            for (i = 0; i < nodeList.length; i++) {
                                if (xmlDoc.getElementsByTagName("iqa")[i].textContent.substring(0, 1) != '1') {
                                    // alert("4");
                                    //checkResult = 1;
                                    document.getElementById("cb_" + xmlDoc.getElementsByTagName("uic")[i].textContent).checked = true;
                                    jsAddToUIC(xmlDoc.getElementsByTagName("uic")[i].textContent, document.getElementById("cb_" + xmlDoc.getElementsByTagName("uic")[i].textContent).checked);
                                    //  alert("5");
                                }
                                if (xmlDoc.getElementsByTagName("valid_micr")[i].textContent.substring(0, 1) != 'P' && xmlDoc.getElementsByTagName("valid_micr")[i].textContent != "N/A") {
                                    // alert("4");
                                    //checkResult = 1;
                                    document.getElementById("cb_" + xmlDoc.getElementsByTagName("uic")[i].textContent).checked = true;
                                    jsAddToUIC(xmlDoc.getElementsByTagName("uic")[i].textContent, document.getElementById("cb_" + xmlDoc.getElementsByTagName("uic")[i].textContent).checked);
                                    //  alert("5");
                                }
                                if (xmlDoc.getElementsByTagName("checktype")[i].textContent.substring(0, 1) != 'P') {
                                    // alert("4");
                                    //checkResult = 1;
                                    document.getElementById("cb_" + xmlDoc.getElementsByTagName("uic")[i].textContent).checked = true;
                                    jsAddToUIC(xmlDoc.getElementsByTagName("uic")[i].textContent, document.getElementById("cb_" + xmlDoc.getElementsByTagName("uic")[i].textContent).checked);
                                    //  alert("5");
                                }
                            }
                            //for (i = 0; i < nodeList.length; i++) {
                            //    if (xmlDoc.getElementsByTagName("valid_micr")[i].textContent == "D") {
                            //        //alert("4");
                            //        //checkResult = 1;
                            //        document.getElementById("cb_" + xmlDoc.getElementsByTagName("uic")[i].textContent).checked = true;
                            //        jsAddToUIC(xmlDoc.getElementsByTagName("uic")[i].textContent, document.getElementById("cb_" + xmlDoc.getElementsByTagName("uic")[i].textContent).checked);
                            //        //alert("5");
                            //    }
                            //}
                        }

                        var serializer = new XMLSerializer();
                        // alert("6");
                        var xmlstring = serializer.serializeToString(xmlDoc);
                        //alert("7");
                        var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                        // alert("8");
                        objFileForWrite.write(xmlstring);
                        //  alert("9");
                        objFileForWrite.close();
                    }
                }
            }
            catch (e) {
                // alert("jsError1");
                exceptionMsg = "(jsXMLEndBatchValidation) Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                $("#sErrorMsg").text(exceptionMsg);
                $("#divErrorMsg").show();
                return "Error";
            }
        }

        //Initialize scanner
        function jsInitScanner() {
            var strScannerErrDesc;

            $("#btnScan").prop("disabled", true);
            $("#btnEject").prop("disabled", true);
            $("#btnEndBatch").prop("disabled", true);
            $("#btnRemove").prop("disabled", true);
            $("#btnRemoveAll").prop("disabled", true);
            $("#btnIQA").prop("disabled", true);
            $("#btnRescan").prop("disabled", true);

            if ($("#hScannerOn").val() == 0) {
                scnControl.intScannerModel = $("#hScannerModel").val();
                jsConfigScanner();
                scnControl.InitConfig();
                $("#hScannerOn").val("");
                $("#hScannerOn").val(1);
            }

            if (scnControl.mLngRet < 0) {
                if (scnControl.mLngRet != 1) {
                    strScannerErrDesc = jsRetrieveScannerErr(scnControl.mLngRet);
                    //(strScannerErrDesc);
                    $("#sErrorMsg").text(strScannerErrDesc);
                    $("#divErrorMsg").show();
                    $("#txtStatus").val("strScannerErrDesc");
                    $("#btnScan").prop("disabled", true);
                    $("#btnEject").prop("disabled", true);
                    $("#btnEndBatch").prop("disabled", true);
                    $("#btnRemove").prop("disabled", true);
                    $("#btnRemoveAll").prop("disabled", true);
                    //$("#btnIQA").prop("disabled", true);
                    $("#btnRescan").prop("disabled", true);
                }
            }
            else {
                jsConfigScanner();
                scnControl.InitConfig();
                $("#hScannerOn").val("");
                $("#hScannerOn").val(1);

                if (scnControl.mLngRet == -114 || scnControl.mLngRet == 0) {
                    $("#txtStatus").val("Scanner Connected");
                    $("#btnScan").prop("disabled", false);
                    $("#btnEject").prop("disabled", true);
                    $("#btnEndBatch").prop("disabled", true);
                    $("#btnRemove").prop("disabled", true);
                    $("#btnRemoveAll").prop("disabled", true);
                    $("#btnIQA").prop("disabled", true);
                    $("#btnRescan").prop("disabled", true);
                }
                else {
                    $("#txtStatus").val("Scanner Disconnected");
                    $("#btnScan").prop("disabled", true);
                    $("#btnEject").prop("disabled", true);
                    $("#btnEndBatch").prop("disabled", true);
                    $("#btnRemove").prop("disabled", true);
                    $("#btnRemoveAll").prop("disabled", true);
                    $("#btnIQA").prop("disabled", true);
                    $("#btnRescan").prop("disabled", true);
                }
            }

            return true;
        }

        //Set xmlFields values into table
        function jsBindLineItems(objXml, tblTemplate, tblBody) {
            var xmlNode;
            var intCount;
            var i;
            var strOutput;

            //total checks and total deposit counter
            totalChecks = 0;
            totalDp = 0;

            //force micr hidden element value
            hSelectUIC = document.getElementById("hSelectedUIC");
            hSelectUIC.Value = "";
            g_uic = [];

            var outputDoc = $("#xmlFields").html();
            try {
                var objXml;
                if (window.DOMParser) {
                    // code for modern browsers
                    parser = new DOMParser();
                    objXml = parser.parseFromString(outputDoc, "text/xml");
                } else {
                    // code for old IE browsers
                    objXml = new ActiveXObject("Microsoft.FreeThreadedXMLDOM");
                    objXml.async = false;
                    objXml.loadXML(outputDoc);
                }
            }
            catch (e) {
                //("Fail to load xml file: " + e);
                $("#sErrorMsg").text("Fail to load xml file: " + e);
                $("#divErrorMsg").show();
            }

            strOutput = outputDoc;
            if ((strOutput.indexOf("<scanner>") != -1) && (objXml.getElementsByTagName("capturing_mode")[0].textContent != '')) {

                var iqa0 = document.getElementById("hIQA0").value
                var iqa1 = document.getElementById("hIQA1").value
                var iqa2 = document.getElementById("hIQA2").value

                var strBatchCapMode = objXml.getElementsByTagName("capturing_mode")[0].textContent;
                var strChequeTypeId = objXml.getElementsByTagName("cheque_type")[0].textContent;
                var strChequeTypeCode;

                //User selected Capturing Mode and Cheque Type
                var strSelectedCapMode = document.getElementById("hSelectedCapMode").value;
                var strSelectedCapModeDesc = document.getElementById("hSelectedCapModeDesc").value;
                var strSelectedChequeType = document.getElementById("hChqType").value;
                var strSelectedChqTypeDesc = document.getElementById("hChqTypeDesc").value;

                var intChequeStatus = document.getElementById("hChequeStatus").value;
                // var intCICSMode  =  document.getElementById("hCICSMode").value;
                var intScannerId = objXml.getElementsByTagName("scanner_id")[0].textContent;
                var intTotalRecord = objXml.getElementsByTagName("total_rec")[0].textContent;
                var intBatchId = objXml.getElementsByTagName("batch_id")[0].textContent;
                intBatchId = jsRight("0000" + jsTrim(intBatchId), 4)
                var intCapturingDate = objXml.getElementsByTagName("capturing_date")[0].textContent;
                var intCapturingTime = objXml.getElementsByTagName("capturing_time")[0].textContent;

                var intLate = objXml.getElementsByTagName("late")[0].textContent;
                var intCurrency = objXml.getElementsByTagName("currency")[0].textContent;
                var intClearingBranch = jsTrim(objXml.getElementsByTagName("clearing_branch")[0].textContent);
                var intBranchId = jsTrim(objXml.getElementsByTagName("capturing_branch")[0].textContent);
                var intPriority = objXml.getElementsByTagName("priority")[0].textContent;
                var intNC = objXml.getElementsByTagName("nc_flag")[0].textContent;
                var intflag5 = objXml.getElementsByTagName("flag5")[0].textContent;
                var intflag3 = objXml.getElementsByTagName("flag3")[0].textContent;
                var intGetChqType;

                var strPDCId = "";
                var strPaymentMode = "";
                var strPDCChequeType = "";
                var strNumOfPayment = "";
                var strPNN = "";
                var intFirstPaymentDate = "";
                var dblPaymentAmt = "";
                var dblLastPaymentAmt = "";
                var dblTotalPymentAmt = "";
                //if (objXml.selectSingleNode("//root/pdc")) {
                //    strPDCId = objXml.selectSingleNode("//root/pdc/pdc_id").text;
                //    strPaymentMode = objXml.selectSingleNode("//root/pdc/payment_mode").text;
                //    strPDCChequeType = objXml.selectSingleNode("//root/pdc/cheque_type").text;
                //    strNumOfPayment = objXml.selectSingleNode("//root/pdc/num_of_payment").text;
                //    strPNN = objXml.selectSingleNode("//root/pdc/payment_note_num").text;
                //    intFirstPaymentDate = objXml.selectSingleNode("//root/pdc/first_payment_date").text;
                //    dblPaymentAmt = objXml.selectSingleNode("//root/pdc/payment_amount").text;
                //    dblLastPaymentAmt = objXml.selectSingleNode("//root/pdc/last_payment_amount").text;
                //    dblTotalPymentAmt = objXml.selectSingleNode("//root/pdc/total_payment_amount").text;
                //}

                //Display ChequeType
                var intChequeType = document.getElementById("hChequeType").value;

                //if empty total record is null then set to 0
                if (objXml.getElementsByTagName("total_rec")[0].textContent == "") {
                    intTotalRecord = 0;
                }

                //Get Mode and Type Description value.
                var strDataXmlCapModeDesc = ajaxGetCapturingModeDesc(strBatchCapMode);
                var strDataXMlChqTypeDesc = ajaxGetCheckTypeDesc(strChequeTypeId);

                //check current batch capturing mode with selected capturing mode
                if ((jsTrim(strBatchCapMode) != jsTrim(strSelectedCapMode)) || (jsTrim(strChequeTypeId) != jsTrim(strSelectedChequeType))) {

                    var strQuery;
                    var blnConfirm;

                    strQuery = "Kindly end batch or remove the scanned items before changing the capturing mode.";
                    //(strQuery);
                    $("#sErrorMsg").text("Kindly end batch or remove the scanned items before changing the capturing mode.");
                    $("#divErrorMsg").show();

                    if (blnConfirm) {  //if "OK", complete current batch and start new batch with new capturing mode. 
                        jsCompleteBatch(1); // Blocking changing of capturing mode

                    }
                    else {
                        var intGetSeqNo;
                        var intGetBankType;
                        var strUIC;

                        xmlNode = objXml.getElementsByTagName("data");

                        strUIC = objXml.getElementsByTagName("uic")[xmlNode.length - 1].textContent;

                        arrReturnUIC = jsSplitUIC(strUIC);
                        intGetSeqNo = arrReturnUIC[0];
                        intGetBankType = arrReturnUIC[1];

                        //Continue with current batch, the screen setting will follow xml file
                        document.getElementById("sProcessingDate").innerText = jsFormatProcessDate(intCapturingDate.substring(6, 8), intCapturingDate.substring(4, 6), intCapturingDate.substring(0, 4), false);
                        document.getElementById("sCapMode").innerText = strDataXmlCapModeDesc;
                        document.getElementById("sChequeType").innerText = strDataXMlChqTypeDesc;
                        document.getElementById("sScannerId").innerText = intScannerId;
                        document.getElementById("sClrBranchId").innerText = intClearingBranch;
                        document.getElementById("sBatchNo").innerText = intBatchId;
                        document.getElementById("sSeqNo").innerText = leftPad(intGetSeqNo, 4);
                        //document.getElementById("txtTellerId").value = intflag5;

                        if (String(document.all.optEncashment) != "undefined") {
                            var objEncashment = document.all.optEncashment;
                            var strEncashment = "";
                            for (i = 0; i < objEncashment.length; i++) {
                                if (objEncashment[i].value == intflag3) {
                                    //  alert("check7");
                                    objEncashment[i].checked = true;
                                    break;
                                }
                            }
                        }

                        //Reset value into hidden field in DCC.aspx
                        document.getElementById("hSelectedCapMode").value = strBatchCapMode;
                        document.getElementById("hSelectedCapModeDesc").value = strDataXmlCapModeDesc;
                        document.getElementById("hChqType").value = strChequeTypeId;
                        document.getElementById("hChqTypeDesc").value = strDataXMlChqTypeDesc;
                        document.getElementById("hCapBranch").value = intBranchId;
                        document.getElementById("hPriority").value = intPriority;
                        document.getElementById("hCurrencyCode").value = intCurrency;
                        document.getElementById("hBankType").value = intGetBankType;
                        document.getElementById("hTotalCounter").value = intTotalRecord;
                        document.getElementById("hChequeType").value = intChequeType;
                        $("#btnEndBatch").prop("disabled", false);
                        $("#btnRemove").prop("disabled", false);
                        $("#btnRemoveAll").prop("disabled", false);
                    }
                }
                else {

                    var intGetSeqNo;
                    var intGetBankType;
                    var strUIC;
                    xmlNode = objXml.getElementsByTagName("data");

                    strUIC = objXml.getElementsByTagName("uic")[xmlNode.length - 1].textContent;
                    arrReturnUIC = jsSplitUIC(strUIC);
                    intGetSeqNo = arrReturnUIC[0];
                    intGetBankType = arrReturnUIC[1];

                    //Continue with current batch, the screen setting will follow xml file
                    $("#sProcessingDate").text(jsFormatProcessDate(intCapturingDate.substring(6, 8), intCapturingDate.substring(4, 6), intCapturingDate.substring(0, 4), false));
                    $("#sCapMode").text(strDataXmlCapModeDesc);
                    $("#sChequeType").text(strSelectedChqTypeDesc);
                    $("#sScannerId").text(intScannerId);
                    $("#sClrBranchId").text(intClearingBranch);
                    $("#sBatchNo").text(intBatchId);
                    $("#sSeqNo").text(leftPad(intGetSeqNo, 4));
                    $("#txtTellerId").text(intflag5);

                    if (String(document.all.optEncashment) != "undefined") {
                        var objEncashment = document.all.optEncashment;
                        var strEncashment = "";
                        for (i = 0; i < objEncashment.length; i++) {
                            if (objEncashment[i].value == intflag3) {
                                //   alert("check8");
                                objEncashment[i].checked = true;
                                break;
                            }
                        }

                    }

                    //Reset value into hidden field
                    document.getElementById("hSelectedCapMode").value = strBatchCapMode;
                    document.getElementById("hSelectedCapModeDesc").value = strDataXmlCapModeDesc;
                    document.getElementById("hChqType").value = strSelectedChequeType;
                    document.getElementById("hChqTypeDesc").value = strSelectedChqTypeDesc;
                    document.getElementById("hCapBranch").value = intBranchId;
                    document.getElementById("hPriority").value = intPriority;
                    document.getElementById("hCurrencyCode").value = intCurrency;
                    document.getElementById("hBankType").value = intGetBankType;
                    document.getElementById("hTotalCounter").value = intTotalRecord;

                    //For ChequeType
                    document.getElementById("hChequeType").value = intChequeType;

                    //get last PDC Id
                    //if (strPDCId != '') {
                    //    if (document.getElementById("lblPdcId")) {
                    //        document.getElementById("lblPdcId").innerText = strPDCId;
                    //    }
                    //}
                    if (strPaymentMode != '') {
                        if (document.getElementById("hPaymentMode")) {
                            document.getElementById("hPaymentMode").value = strPaymentMode;
                        }
                    }
                    if (strPDCChequeType != '') {
                        if (document.getElementById("hPDCChequeType")) {
                            document.getElementById("hPDCChequeType").value = strPDCChequeType;
                        }
                    }
                    if (strNumOfPayment != '') {
                        if (document.getElementById("txtNumOfPymt")) {
                            document.getElementById("txtNumOfPymt").value = strNumOfPayment;
                        }
                    }
                    if (strPNN != '') {
                        if (document.getElementById("txtPNN")) {
                            document.getElementById("txtPNN").value = strPNN;
                        }
                    }
                    if (intFirstPaymentDate != '') {
                        if (document.getElementById("txtChequeDate")) {
                            document.getElementById("txtChequeDate").value = jsFormatProcessDate(intFirstPaymentDate.substring(6, 8), intFirstPaymentDate.substring(4, 6), intFirstPaymentDate.substring(0, 4), false);
                        }
                    }
                    if (dblPaymentAmt != '') {
                        if (document.getElementById("txtFirstAmount")) {
                            document.getElementById("txtFirstAmount").value = dblPaymentAmt;
                        }
                    }
                    if (dblLastPaymentAmt != '') {
                        if (document.getElementById("txtLastAmount")) {
                            document.getElementById("txtLastAmount").value = dblLastPaymentAmt;
                        }
                    }
                    if (dblTotalPymentAmt != '') {
                        if (document.getElementById("txtTotalBatchAmount")) {
                            document.getElementById("txtTotalBatchAmount").value = dblTotalPymentAmt;
                        }
                    }

                    //get last PDC Id            
                    $("#btnEndBatch").prop("disabled", false);
                    $("#btnRemove").prop("disabled", false);
                    $("#btnRemoveAll").prop("disabled", false);

                }

                ////disable late item field
                //if (intLate != '') {
                //    var objLateItems = document.all.optLateItems
                //    objLateItems.disabled = true

                //    if (intLate == 0) {
                //        objLateItems[0].checked = true     //normal
                //    }
                //    else if (intLate == 1) {
                //        objLateItems[1].checked = true     //late1
                //    }
                //    else if (intLate == 2) {
                //        objLateItems[2].checked = true     //late2
                //    }
                //    else {
                //        objLateItems.disabled = false
                //    }
                //}

                ////disable priority field 
                //if (intPriority != '') {
                //    var objPriority = document.all.optPriority
                //    for (i = 0; i < objPriority.length; i++) {
                //        objPriority[i].disabled = true
                //    }

                //    if (intPriority >= 1) {                            //priority = high
                //        objPriority[1].checked = true
                //    }
                //    else if (intPriority == 0 || intPriority == 1) {   //priority = normal 
                //        objPriority[0].checked = true
                //    }
                //    else {
                //        for (i = 0; i < objPriority.length; i++) {
                //            objPriority[i].disabled = false
                //        }
                //    }
                //}

                ////disabled bank type field
                //if (intGetBankType != '') {
                //    var objBankType = document.all.optBankType
                //    for (i = 0; i < objBankType.length; i++) {
                //        objBankType[i].disabled = true
                //    }

                //    if (intGetBankType == 2) {
                //        objBankType[0].checked = true
                //    }
                //    else if (intGetBankType == 3) {
                //        objBankType[1].checked = true
                //    }
                //    else if (intGetBankType == 33) {
                //        objBankType[2].checked = true
                //    }
                //    else {
                //        for (i = 0; i < objBankType.length; i++) {
                //            objBankType[i].disabled = false
                //        }
                //    }
                //}

                ////disabled currency field
                //if (intCurrency != '') {
                //    var blnSetSelectedCurrency; //Currency options are dynamically populated from db
                //    blnSetSelectedCurrency = false;
                //    var objCurrency = document.all.optCurrencyCode
                //    for (i = 0; i < objCurrency.length; i++) {
                //        objCurrency[i].disabled = true;
                //        if (intCurrency == objCurrency[i].value) {
                //            objCurrency[i].checked = true;
                //            blnSetSelectedCurrency = true;
                //        }
                //    }
                //    if (!blnSetSelectedCurrency) {
                //        for (i = 0; i < objCurrency.length; i++) {
                //            objCurrency[i].disabled = false
                //        }
                //    }
                //}

                ////NC flag
                ////default the NC flag
                //if (intNC != '') {
                //    var objAmount = document.all.optAmount
                //    for (i = 0; i < objAmount.length; i++) {
                //        objAmount[i].disabled = true;
                //    }
                //    objAmount[1].checked = true;
                //}              

                ////disable immediate entry field
                //var objImmediateEntry = document.all.optImmediateEntry;
                //for (i = 0; i < objImmediateEntry.length; i++) {
                //    objImmediateEntry[i].disabled = true
                //}

                //if (document.getElementById("hSelectedCapModeDesc").value != "CO") {
                //    for (i = 0; i < objImmediateEntry.length; i++) {
                //        objImmediateEntry[i].disabled = true
                //    }
                //}

                xmlNode = objXml.getElementsByTagName("data");


                if (xmlNode.length > 0) {

                    var intXmlNodeLen = xmlNode.length - 1
                    var intRowCount = 0

                    //for (intCount = intXmlNodeLen; intCount >= 0; intCount--) {
                    for (intCount = 0; intCount <= intXmlNodeLen; intCount++) {
                        jsAddRow(tblTemplate, tblBody);

                        tblBody.rows[intRowCount].cells[0].innerText = intCount + 1;

                        tblBody.rows[intRowCount].cells[2].innerHTML = '<a title="' + objXml.getElementsByTagName("iqa")[intCount].textContent.substring(1, objXml.getElementsByTagName("iqa")[intCount].textContent.length) + '" ><input type="hidden" id="iqa_' + objXml.getElementsByTagName("uic")[intCount].textContent + '" value="' + objXml.getElementsByTagName("iqa")[intCount].textContent.substring(0, 1) + '"/>' + objXml.getElementsByTagName("iqa")[intCount].textContent.substring(0, 1) + '</a>';

                        //Conditon for IQA to Change the 1, 2 ,0 to a value from tblsystemprofile
                        if (objXml.getElementsByTagName("iqa")[intCount].textContent.substring(0, 1) == "2") {
                            tblBody.rows[intRowCount].cells[2].innerHTML = '<a title="' + objXml.getElementsByTagName("iqa")[intCount].textContent.substring(1, objXml.getElementsByTagName("iqa")[intCount].textContent.length) + '" ><input type="hidden" id="iqa_' + objXml.getElementsByTagName("uic")[intCount].textContent + '" value="' + objXml.getElementsByTagName("iqa")[intCount].textContent.substring(0, 1) + '"/>' + iqa2 + '</a>';

                            if (objXml.getElementsByTagName("iqa")[intCount].textContent = iqa2) {

                                //tblBody.rows[intRowCount].cells[1].style.Color = "red";
                                tblBody.rows[intRowCount].cells[0].style.color = "white";
                                tblBody.rows[intRowCount].cells[1].style.color = "white";
                                tblBody.rows[intRowCount].cells[2].style.color = "white";
                                tblBody.rows[intRowCount].cells[3].style.color = "white";
                                tblBody.rows[intRowCount].cells[4].style.color = "white";
                                tblBody.rows[intRowCount].cells[5].style.color = "white";
                                tblBody.rows[intRowCount].cells[6].style.color = "white";
                                tblBody.rows[intRowCount].cells[7].style.color = "white";

                                tblBody.rows[intRowCount].cells[0].style.backgroundColor = "red";
                                tblBody.rows[intRowCount].cells[1].style.backgroundColor = "red";
                                tblBody.rows[intRowCount].cells[2].style.backgroundColor = "red";
                                tblBody.rows[intRowCount].cells[3].style.backgroundColor = "red";
                                tblBody.rows[intRowCount].cells[4].style.backgroundColor = "red";
                                tblBody.rows[intRowCount].cells[5].style.backgroundColor = "red";
                                tblBody.rows[intRowCount].cells[6].style.backgroundColor = "red";
                                tblBody.rows[intRowCount].cells[7].style.backgroundColor = "red";
                            }
                        }

                        if (objXml.getElementsByTagName("iqa")[intCount].textContent.substring(0, 1) == "1") {
                            tblBody.rows[intRowCount].cells[2].innerText = iqa1;
                        }

                        if (objXml.getElementsByTagName("iqa")[intCount].textContent.substring(0, 1) == "0") {
                            tblBody.rows[intRowCount].cells[2].innerText = iqa0;
                        }

                        if (objXml.getElementsByTagName("item_type")[intCount].textContent == 'C') {
                            totalChecks = totalChecks + 1;
                        }

                        //Condition to change the background-color if the item is deposit slip
                        if (objXml.getElementsByTagName("item_type")[intCount].textContent == 'P') {

                            tblBody.rows[intRowCount].cells[0].style.color = "white";
                            tblBody.rows[intRowCount].cells[1].style.color = "white";
                            tblBody.rows[intRowCount].cells[2].style.color = "white";
                            tblBody.rows[intRowCount].cells[3].style.color = "white";
                            tblBody.rows[intRowCount].cells[4].style.color = "white";
                            tblBody.rows[intRowCount].cells[5].style.color = "white";
                            tblBody.rows[intRowCount].cells[6].style.color = "white";
                            tblBody.rows[intRowCount].cells[7].style.color = "white";
                            tblBody.rows[intRowCount].cells[0].style.backgroundColor = "orange";
                            tblBody.rows[intRowCount].cells[1].style.backgroundColor = "orange";
                            tblBody.rows[intRowCount].cells[2].style.backgroundColor = "orange";
                            tblBody.rows[intRowCount].cells[3].style.backgroundColor = "orange";
                            tblBody.rows[intRowCount].cells[4].style.backgroundColor = "orange";
                            tblBody.rows[intRowCount].cells[5].style.backgroundColor = "orange";
                            tblBody.rows[intRowCount].cells[6].style.backgroundColor = "orange";
                            tblBody.rows[intRowCount].cells[7].style.backgroundColor = "orange";

                            totalDp = totalDp + 1;
                        }


                        //if (objXml.getElementsByTagName("valid_micr")[intCount].textContent == "D" || objXml.getElementsByTagName("valid_micr")[intCount].textContent == "F") {
                        if (objXml.getElementsByTagName("valid_micr")[intCount].textContent != "P" && objXml.getElementsByTagName("valid_micr")[intCount].textContent != "N/A") {
                            //tblBody.rows[intRowCount].cells[1].style.Color = "red";
                            tblBody.rows[intRowCount].cells[0].style.color = "white";
                            tblBody.rows[intRowCount].cells[1].style.color = "white";
                            tblBody.rows[intRowCount].cells[2].style.color = "white";
                            tblBody.rows[intRowCount].cells[3].style.color = "white";
                            tblBody.rows[intRowCount].cells[4].style.color = "white";
                            tblBody.rows[intRowCount].cells[5].style.color = "white";
                            tblBody.rows[intRowCount].cells[6].style.color = "white";
                            tblBody.rows[intRowCount].cells[7].style.color = "white";

                            tblBody.rows[intRowCount].cells[0].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[1].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[2].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[3].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[4].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[5].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[6].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[7].style.backgroundColor = "red";
                            totalChecks = totalChecks + 1;
                        }

						if(objXml.getElementsByTagName("valid_micr")[intCount].textContent == "D")
						{
							$("#btnForceMICRPass").prop("disabled", true);
						}	
						else	
						{
							$("#btnForceMICRPass").prop("disabled", false);
						}

                        if (objXml.getElementsByTagName("checktype")[intCount].textContent == "F") {


                            //tblBody.rows[intRowCount].cells[1].style.Color = "red";
                            tblBody.rows[intRowCount].cells[0].style.color = "white";
                            tblBody.rows[intRowCount].cells[1].style.color = "white";
                            tblBody.rows[intRowCount].cells[2].style.color = "white";
                            tblBody.rows[intRowCount].cells[3].style.color = "white";
                            tblBody.rows[intRowCount].cells[4].style.color = "white";
                            tblBody.rows[intRowCount].cells[5].style.color = "white";
                            tblBody.rows[intRowCount].cells[6].style.color = "white";
                            tblBody.rows[intRowCount].cells[7].style.color = "white";

                            tblBody.rows[intRowCount].cells[0].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[1].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[2].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[3].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[4].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[5].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[6].style.backgroundColor = "red";
                            tblBody.rows[intRowCount].cells[7].style.backgroundColor = "red";
                            totalChecks = totalChecks + 1;
                        }

                        //Add Display ChequeType
                        tblBody.rows[intRowCount].cells[1].innerText = objXml.getElementsByTagName("item_type")[intCount].textContent;
                        //alert(item_type);
                        tblBody.rows[intRowCount].cells[3].innerText = objXml.getElementsByTagName("valid_micr")[intCount].textContent;
                        //alert(valid_MICR);
                        tblBody.rows[intRowCount].cells[4].innerText = objXml.getElementsByTagName("micr")[intCount].textContent;
                        tblBody.rows[intRowCount].cells[5].innerText = objXml.getElementsByTagName("uic")[intCount].textContent;
                        tblBody.rows[intRowCount].cells[6].innerText = objXml.getElementsByTagName("checktype")[intCount].textContent;

                        tblBody.rows[intRowCount].cells[7].innerHTML = '<Input type="checkbox" name="cb_' + objXml.getElementsByTagName("uic")[intCount].textContent + '" id="cb_' + objXml.getElementsByTagName("uic")[intCount].textContent + '" onclick="jsAddToUIC(name.substring(3, name.length), this.checked);"/>';

                        intRowCount = intRowCount + 1
                    }

                    document.getElementById("hiddenNoItemperBatch").value = intRowCount;
                    $("#btnEndBatch").prop("disabled", false);
                    $("#btnRemove").prop("disabled", false);
                    $("#btnRemoveAll").prop("disabled", false);
                    $("#btnIQA").prop("disabled", false);
                    $("#btnRescan").prop("disabled", true);
                }
            }
            jsMICRCount();
            // alert("00");
            jsLoadcheckControl();
        }

        //Configure scanner setting
        function jsConfigScanner() {//Function vbConfigScanner()
            //alert("2");
            //scnControl.strIrfanViewEXE = "C:\INCHEQS-OCS\i_view32.exe";//'//scnControl.strIrfanViewEXE = "C:\INCHEQS-OCS\i_view32.exe"

            //Set endorsement
            var bEndorsement;
            bEndorsement = jsGetScannerTuning("bEndorsement").toString();

            if (bEndorsement.trim() != "") {
                scnControl.blnEndorsement = bEndorsement;
            }
            else {
                scnControl.blEndorsement = 0;
            }

            //Set jpeg quality    
            var lngJPEGQuality;
            lngJPEGQuality = jsGetScannerTuning("intJPEGQuality").toString();
            if (lngJPEGQuality.trim() != "") {
                scnControl.lngJPEGQuality = lngJPEGQuality;
            }
            else {
                scnControl.lngJPEGQuality = 35;//default
            }

            //Set scan image side
            var mlngSides;
            mlngSides = jsGetScannerTuning("mlngSides").toString();
            if (mlngSides.trim() != "") {
                scnControl.intScanSide = mlngSides;
            }
            else {
                scnControl.intScanSide = 2;//default
            }

            //Set scan type           
            var strScanType;
            strScanType = jsGetScannerTuning("strScanType").toString();
            if (strScanType.trim() != "") {
                scnControl.strScanType = strScanType;
            }
            else {
                scnControl.strScanType = "JFIF";//default
            }

            //Set Jpeg dpi
            var intJPEGDPI;
            intJPEGDPI = jsGetScannerTuning("intJPEGDPI").toString();
            if (intJPEGDPI.trim() != "") {
                scnControl.intJPEGDPI = intJPEGDPI;
            }
            else {
                scnControl.intJPEGDPI = 9999;//default
            }

            //Set Bitonal dpi           
            var intBitonalDPI;
            intBitonalDPI = jsGetScannerTuning("intBitonalDPI").toString();
            if (intBitonalDPI.trim() != "") {
                scnControl.intBinaryDPI = intBitonalDPI;
            }
            else {
                scnControl.intBinaryDPI = 200;//default
            }

            //Set Brightness           
            var intBrightness;
            intBrightness = jsGetScannerTuning("intBrightness").toString();
            if (intBrightness.trim() != "") {
                scnControl.intBrightness = intBrightness;
            }
            else {
                scnControl.intBrightness = 128;//default
            }

            //Set skew correction           
            var intSkewCorrect;
            intSkewCorrect = jsGetScannerTuning("intSkewCorrect").toString();
            if (intSkewCorrect.trim() != "") {
                scnControl.intSkewCorrect = intSkewCorrect;
            }
            else {
                scnControl.intSkewCorrect = 1;//default
            }

            //Set endorsement font size           
            var intEndorseFontSize;
            intEndorseFontSize = jsGetScannerTuning("intEndorseFontSize").toString();
            if (intEndorseFontSize.trim() != "") {
                scnControl.intEndorFontSize = intEndorseFontSize;
            }
            else {
                scnControl.intEdorFontSize = 4;//default
            }

            //Set Bitonal file extension      
            var strBitonalExtension;
            strBitonalExtension = jsGetScannerTuning("strBitonalExtension").toString();
            if (strBitonalExtension.trim() != "") {
                scnControl.strBitonalExtension = strBitonalExtension;
            }
            else {
                scnControl.strBitonalExtension = "TIF";//default
            }

            //Save Front Cheque Page as image in Bi-tonal Mode   
            var blnFronBitonalGen;
            blnFrontBitonalGen = jsGetScannerTuning("blnFrontBitonalGen").toString();
            if (blnFrontBitonalGen.trim() != "") {
                scnControl.blnFrontBitonalGen = blnFrontBitonalGen;
            }
            else {
                scnControl.blnFrontBitonalGen = true;//default
            }

            //Save Back Cheque Page as image in Bi-tonal Mode
            var blnBackBitonalGen;
            blnBackBitonalGen = jsGetScannerTuning("blnBackBitonalGen").toString();
            if (blnBackBitonalGen.trim() != "") {
                scnControl.blnBackBitonalGen = blnBackBitonalGen;
            }
            else {
                scnControl.blnBackBitonalGen = true;//default
            }
        }

        //Get scanner setting
        function jsGetScannerTuning(intScannerCode) {

            var strScannerTuningXML;
            var strScannerTuningvalue;

            strScannerTuningXML = $("#xmlScannerTuning").text();

            try {
                var xmlDoc;
                if (window.DOMParser) {
                    // code for modern browsers
                    parser = new DOMParser();
                    xmlDoc = parser.parseFromString(strScannerTuningXML, "text/xml");
                } else {
                    // code for old IE browsers
                    xmlDoc = new ActiveXObject("Microsoft.FreeThreadedXMLDOM");
                    xmlDoc.async = false;
                    xmlDoc.loadXML(strScannerTuningXML);
                }
            }
            catch (e) {
                //("Fail to load xml file: " + e);
                $("#sErrorMsg").text("Fail to load xml file: " + e);
                $("#divErrorMsg").show();
            }

            var strOutput = strScannerTuningXML;

            if (strOutput.indexOf("<ScannerTuning>") != -1) {
                var nodeList = xmlDoc.getElementsByTagName("ScannerTuningRecord");
                if (nodeList.length > 0) {
                    for (i = 0; i < nodeList.length; i++) {
                        if (xmlDoc.getElementsByTagName("ScannerTuningCode")[i].childNodes[0].nodeValue == intScannerCode) {
                            strScannerTuningvalue = xmlDoc.getElementsByTagName("ScannerTuningvalue")[i].childNodes[0].nodeValue;
                        }

                    }
                }
            }

            return strScannerTuningvalue
        }

        //Total Checks | Deposit Slips
        function jsMICRCount() {
            var sTotalChecks;
            var sTotalDp;
            sTotalChecks = $("#sTotalChecks");
            sTotalDp = $("#sTotalDp");

            $("#sTotalChecks").html(totalChecks);
            $("#sTotalDp").html(totalDp);
        }

        //Get all the values and build UIC

        function jsBuildUIC() {
            var strScannerId
            var strBatchNo
            var strSeqNo
            var strBankBranchCode
            var strBankCode
            var strBankType
            var strCurrencyCode
            var strChqType
            var strClrDate
            var strUIC

            //Check -1 value in sSeqNo
            if (parseInt($("#sSeqNo").text()) == "-1" || parseInt($("#sSeqNo").text()) == -1) {
                $("#sSeqNo").text(leftPad(1, 4));
            }

            strScannerId = jsRight("000" + jsTrim($("#sScannerId").text()), 3);
            //alert(strScannerId);
            strBatchNo = jsRight("0000" + jsTrim($("#sBatchNo").text()), 4);
            strSeqNo = jsRight("0000" + jsTrim($("#sSeqNo").text()), 4);
            //alert(strSeqNo);

            //strBankCode = jsTrim($("#sClrBranchId").text()).substring(1, 4)
            //strBankBranchCode = jsTrim($("#sClrBranchId").text()).substring(4, 8)
            //Change sClrBranchId to hCapBranch in UIC Printing
            strBankCode = jsTrim($("#hCapBranch").val()).substring(1, 4)
            strBankBranchCode = jsTrim($("#hCapBranch").val()).substring(4, 8)
            

            var i;
            //var oOptBankType = document.all.optBankType
            //for (i = 0; i < oOptBankType.length; i++) {
            //    if (oOptBankType(i).checked) {
            //        strBankType = jsTrim(oOptBankType(i).value);
            //    }
            //}

            var oOptCurrCode = document.all.optCurrencyCode
            if (oOptCurrCode.length == null) {
                strCurrencyCode = jsTrim(oOptCurrCode.value);
            }
            else {
                for (i = 0; i < oOptCurrCode.length; i++) {
                    // alert("check9");
                    if (oOptCurrCode(i).checked) {
                        strCurrencyCode = jsTrim(oOptCurrCode(i).value);
                    }
                }
            }

            strChqType = jsTrim($("#hChqType").val());
            strClrDate = jsTrim($("#hClrDate").val());

            //If strChqType is empty, it is running bank negara mode. So, it will assign batch ticket number into UIC
            //Only happen when first cheque scan from bank negara mode
            if (strChqType == "") {
                strChqType = "88";
            }

            //Build UIC - Bangladesh format
            //9 - routing number
            //8 - running sequence number
            //6 - presetment date - 'DDMMYY'                   
            //strUIC = strClrDate + strBankCode + strScannerId + strBankBranchCode + strSeqNo;

            strUIC = strClrDate + strBankCode + strBankBranchCode + strScannerId + strBatchNo + strSeqNo;

            //alert(strClrDate);
            //alert(strBankCode);
            //alert(strBankBranchCode);
            //alert(strScannerId);
            //alert(strBatchNo);
            //alert(strSeqNo);
            //alert(strUIC);

            if (typeof (strUIC) == 'undefined' || strUIC.length > 26) {
                return "";
            }
            // alert(strUIC.length);

            return strUIC;

        }

        //Left
        function jsLeft(str, n) {
            if (n <= 0)
                return "";
            else if (n > String(str).length)
                return str;
            else
                return String(str).substring(0, n);
        }

        //Right

        function jsRight(str, n) {
            if (n <= 0)
                return "";
            else if (n > String(str).length)
                return str;
            else {
                var iLen = String(str).length;
                return String(str).substring(iLen, iLen - n);

            }
        }

        //Trim
        function jsTrim(str) {

            if (!str || typeof str != 'string')
                return '';

            return str.replace(/^\s+|\s+$/g, "");
        }

        //Configure UIC and endorsement (Continuously mode)
        function jsScanningItemConfig2(strUIC, intEnableEndorsement) {
            if (intEnableEndorsement != "") {
                scnControl.blnEndorsement = intEnableEndorsement;

                //OCX endorsement setting  2 = 1st presentment, 3 = 2nd presentment
                if ($("#optPre1st").prop("checked")) {
                    // alert("check10");
                    scnControl.intEndorseLine = 2;
                }
                else if ($("#optPre2nd").prop("checked")) {
                    // alert("check11");
                    scnControl.intEndorseLine = 3;
                }

                if (intEnableEndorsement == 1) {
                    var strProcessDateUIC;
                    var strBRSTNUIC;
                    var strUICUIC;
                    var strUICUIC2;

                    var strYY;
                    var strMM;
                    var strDD;

                    strYY = strUIC.substr(2, 2);
                    strMM = strUIC.substr(4, 2);
                    strDD = strUIC.substr(6, 2);

                    strProcessDateUIC = strMM + strDD + strYY;
                    //alert("dd");
                    strBRSTNUIC = $("#hCapBranch").val().substr(0, 6) + $("#hCapBranch").val().substr($("#hCapBranch").val().length - 1);
                    // alert("ee");
                    strUICUIC = strUIC.substr(8, 9);
                    //alert("ff");
                    strUICUIC2 = strUIC.substr(strUIC.length - 6);

                    //Add variable to check the Check Type
                    var CheckType;

                    //Change the Endorsement Based on the Check Type
                    if ($("#optNormal").prop("checked")) {
                        CheckType = "REG";
                    }
                    else if ($("#optBOC").prop("checked")) {
                        CheckType = "BOC";
                    }
                    else if ($("#optBIR").prop("checked")) {
                        CheckType = "BIR";
                    }

                    if ($("#optPre1st").prop("checked")) {
                        //scnControl.strEndorData = strProcessDateUIC + " " & strBRSTNUIC + " " + CheckType + " " + strUICUIC + "%I" + strUICUIC2 + "%E" + " NON-NEGOTIABLE";
                        scnControl.strEndorData = strUIC;
                        // alert(scnControl.strEndorData);
                    }
                    else if ($("#optPre2nd").prop("checked")) {
                        //scnControl.strEndorData = strProcessDateUIC & " " & strBRSTNUIC & " " & CheckType & " " & strUICUIC & "%I" & strUICUIC2 & "%E" & " RE-CLEARING";
                        scnControl.strEndorData = strUIC;
                        //  alert(scnControl.strEndorData);
                    }
                }
            }

            scnControl.strFolderName = strFolderName;
            scnControl.strFront1 = "GF";
            scnControl.strBack2 = "GB";
            scnControl.strFront3 = "BF";
            scnControl.strBack4 = "BB";

            //alert("ff");

            scnControl.strUIC = strUIC;
            //alert("gg");
            scnControl.strUIC2 = strUIC;
            // alert("hh");
            //debugger
            //scnControl.strScanImageNameParam = strUIC.substr(0, 22) + "%" + strUIC.substr(strUIC.length - 4) + "%";
            scnControl.strScanImageNameParam = strUIC;
            //alert("ii");
            scnControl.intMaximumCheque = MaxItemPerBatch - parseInt($("#hiddenNoItemperBatch").val());
        }

        //Configure UIC and endorsement (One by one mode)
        function jsScanningItemConfig(strUIC, strUICNext, intEnableEndorsement) {
            //alert(intEnableEndorsement);
            // debugger;
            if (intEnableEndorsement != null) {


                scnControl.blnEndorsement = intEnableEndorsement;

                if ($("#optPre1st").prop("checked")) {
                    scnControl.intEndorseLine = 0;
                }
                else if ($("#optPre2nd").prop("checked")) {
                    scnControl.intEndorseLine = 2;
                }
                //alert("uu");
                if (intEnableEndorsement == 1) {
                    var strProcessDateUIC;
                    var strBRSTNUIC;
                    var strUICUIC;

                    var strYY;
                    var strMM;
                    var strDD;
                    // alert("YY");
                    strYY = strUIC.substr(2, 2);
                    //alert("MM");
                    strMM = strUIC.substr(4, 2);
                    //alert("DD");
                    strDD = strUIC.substr(6, 2);

                    //alert("jj");

                    strProcessDateUIC = strMM + strDD + strYY;
                    strBRSTNUIC = $("#hCapBranch").val().substr(0, 8) + $("#hCapBranch").val().substr($("#hCapBranch").val().length - 1);
                    //alert("kk");

                    strUICUIC = strUIC.substr(8, 15)

                    //alert("ll");
                    if ($("#optPre1st").prop("checked")) {
                        scnControl.strEndorData = strProcessDateUIC & " " & strBRSTNUIC & " REG PHP " & strUICUIC & " NON-NEGOTIABLE";
                    }
                    else if ($("#optPre2nd").prop("checked")) {
                        scnControl.strEndorData = strProcessDateUIC & " " & strBRSTNUIC & " REG PHP " & strUICUIC & " RE-CLEARING";
                    }
                }
            }

            scnControl.strFolderName = strFolderName;
            scnControl.strFront1 = "GF";
            scnControl.strBack2 = "GB";
            scnControl.strFront3 = "BF";
            scnControl.strBack4 = "BB";

            //alert("mm");

            scnControl.strUIC = strUIC;
            //alert("nn");
            scnControl.strScanImageNameParam = strUIC;

            if (intEnableEndorsement != null) {

                scnControl.blnEndorsement = intEnableEndorsement;

                if (intEnableEndorsement == 1) {
                    var strNextProcessDateUIC;
                    var strNextBRSTNUIC;
                    var strNextUICUIC;

                    var strYY2;
                    var strMM2;
                    var strDD2;
                    strYY2 = strUIC.substr(2, 2);
                    strMM2 = strUIC.substr(4, 2);
                    strDD2 = strUIC.substr(6, 2);

                    //alert("oo");

                    strNextProcessDateUIC = strMM2 + strDD2 + strYY2;

                    //alert("pp");

                    strNextBRSTNUIC = $("#hCapBranch").val().substr(0, 8) + $("#hCapBranch").val().substr($("#hCapBranch").val().length - 1);
                    // alert("qq");
                    strNextUICUIC = strUIC.substr(8, 14);
                    //alert("rr");
                    scnControl.strEndorDataNext = strNextProcessDateUIC + " " + strNextBRSTNUIC + " REG PHP " + strNextUICUIC + " NON-NEGOTIABLE";
                    //alert("ss");
                }
            }
            scnControl.lngQuickTransport = 0;
        }

        //Get scanner error
        function jsRetrieveScannerErr(intErrorCode) {
            var strXML;
            var strValue;
            var xmlDoc;
            var blnErrorStatus = false;

            strXML = $("#hScannerError").val();
            //alert("tt");
            try {
                if (window.DOMParser) {
                    // code for modern browsers
                    parser = new DOMParser();
                    xmlDoc = parser.parseFromString(strXML, "text/xml");
                } else {
                    // code for old IE browsers
                    xmlDoc = new ActiveXObject("Microsoft.FreeThreadedXMLDOM");
                    xmlDoc.async = false;
                    xmlDoc.loadXML(strXML);
                }
            }
            catch (e) {
                //("Fail to load xml file: " + e);
                $("#sErrorMsg").text("Fail to load xml file: " + e);
                $("#divErrorMsg").show();
            }

            var strOutput = strXML;
            //alert("uu");
            if (strOutput.indexOf("<ScannerErrorRecord>") != -1) {
                var nodeList = xmlDoc.getElementsByTagName("ScannerErrorRecord");
                if (nodeList.length > 0) {
                    for (i = 0; i < nodeList.length; i++) {
                        if (xmlDoc.getElementsByTagName("ScannerErrorCode")[i].childNodes[0].nodeValue == intErrorCode) {
                            strValue = xmlDoc.getElementsByTagName("ScannerErrorDesc")[i].childNodes[0].nodeValue;
                            blnErrorStatus = true;
                            return strValue;
                            break;
                        }
                    }
                }
            }
            //alert("vv");
            if (blnErrorStatus == false) {
                return 'Unknown Error';
            }
        }

        //Create data.xml
        function jsCreateDataXMLFile() {
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
            }
            catch (e) {
                //("Fail to load local file");
                $("#sErrorMsg").text("Fail to load local file");
                $("#divErrorMsg").show();
            }
            var arrayFolder = strFolderName.split("\\")
            var blnFolderExists
            var blnDriveExists
            var blnFileExists

            for (var i = 0; i < arrayFolder.length; i++) {

                if (i != 0) {
                    if (arrayFolder[i] != '') {
                        strFolderName = strFolderName + arrayFolder[i] + "\\"
                        blnFolderExists = objFS.folderExists(strFolderName)

                        if (blnFolderExists == false) {
                            objFS.CreateFolder(strFolderName)
                        }
                    }
                }
                else {
                    strFolderName = arrayFolder[0] + "\\"
                    blnDriveExists = objFS.DriveExists(strFolderName)

                    if (blnDriveExists == false) {
                        //(arrayFolder[0] + 'does not exists in workstation.');
                        $("#sErrorMsg").text(arrayFolder[0] + 'does not exists in workstation.');
                        $("#divErrorMsg").show();
                        break;
                    }
                }
            }

            blnFileExists = objFS.FileExists(strFilePath)
            if (blnFileExists == false) {
                objFS.CreateTextFile(strFilePath)
                jsAddDefaultDataXml()
            }
        }

        //Show generate xml status
        function jsShowLoadingMsg(msg, type) {
            if (msg == "") {
                document.getElementById("loadingMessage").innerHTML = "";
                $("#loadingMessage").html("");
            } else {
                $("#loadingMessage").html(msg);
                $("#loadingMessage").css('color', 'red');
            }
        }

        //Check duplicate MICR
        function jsXMLDuplicatedMICR() {
            var exceptionMsg = '';
            // alert("aa");
            try {
                var objCaptureMode = document.getElementById("hSelectedCapMode").value;
                if (objCaptureMode != 'CS') {
                    var objFS = new ActiveXObject("Scripting.FileSystemObject")
                    var i
                    if (objFS.FileExists(strFilePath)) {
                        var f = objFS.GetFile(strFilePath);
                        var size = f.size;
                        if (size) {
                            var objFile = objFS.OpenTextFile(strFilePath)
                            var outputDoc = objFile.readAll();
                            objFile.close()

                            //var xmlDoc = new ActiveXObject("MICROSOFT.FreeThreadedXMLDOM");
                            //xmlDoc.async = "false";
                            //xmlDoc.loadXML(outputDoc);

                            var parser = new DOMParser();
                            var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                            var nodeList = xmlDoc.getElementsByTagName("data");
                            var strMICR = "";
                            var strNewMICR = "";

                            if (nodeList.length > 0) {
                                //  debugger;
                                for (i = 0; i < nodeList.length; i++) {
                                    strNewMICR = jsTrim(xmlDoc.getElementsByTagName("micr")[i].textContent);
                                    strNewMICR = strNewMICR.replace("?", "Z");
                                    if (jsTrim(xmlDoc.getElementsByTagName("valid_micr")[i].textContent) != "P") {
                                        //if (strMICR.search(strNewMICR) > -1 && jsTrim(xmlDoc.getElementsByTagName("item_type")[i].textContent) == 'C') {

                                        //    if (strNewMICR.length > 9) {
                                        //        xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "D";
                                        //    }
                                        //} else {
                                        //    strMICR = strMICR + ', ' + strNewMICR;
                                        //}

                                        if (jsTrim(xmlDoc.getElementsByTagName("micr")[i].textContent).length != 46 && jsTrim(xmlDoc.getElementsByTagName("valid_micr")[i].textContent) != "N/A") {
                                            xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "F";
                                        }
                                        else {
                                            if (strMICR.search(strNewMICR) > -1 && jsTrim(xmlDoc.getElementsByTagName("item_type")[i].textContent) == 'C') {

                                                if (strNewMICR.length > 9) {
                                                    xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "D";
                                                }
                                            }
                                            else {
                                                strMICR = strMICR + ', ' + strNewMICR;
                                            }
                                        }
                                    }
                                    //else {
                                    //    if (jsTrim(xmlDoc.getElementsByTagName("micr")[i].textContent).length != 46) {
                                    //        xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "F";
                                    //    }
                                    //}
                                    //else {
                                    //    if (strMICR.search(strNewMICR) > -1 && jsTrim(xmlDoc.getElementsByTagName("item_type")[i].textContent) == 'C') {

                                    //        if (strNewMICR.length > 9) {
                                    //            xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "D";
                                    //        }
                                    //    }
                                    //    else {
                                    //        strMICR = strMICR + ', ' + strNewMICR;
                                    //    }
                                    //}

                                }
                            }
                            var serializer = new XMLSerializer();
                            var xmlstring = serializer.serializeToString(xmlDoc);

                            var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                            objFileForWrite.write(xmlstring);
                            objFileForWrite.close();
                        }
                    }
                }

            }
            catch (e) {
                exceptionMsg = "(jsXMLDuplicatedMICR) Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //alert(exceptionMsg);
            }
        }

        //Check IQA
        function jsModifyIQA() {
            //debugger
            var exceptionMsg = '';
            try {
                var cbs = document.getElementsByTagName('input');

                for (var i = 0; i < cbs.length; i++) {

                    //if (xmlDoc.getElementsByTagName("iqa")[i].textContent != "1")
                    //{
                    if (cbs[i].type == 'checkbox' && cbs[i].name.substring(0, 3) == 'cb_' && cbs[i].checked == true) {

                        var logFullFilePath = document.getElementById("hCapturingPath").value + "\log\\mylog.log";
                        var settingPath = document.getElementById("hCapturingPath").value + "\\";

                        var sChequePath = document.getElementById("hCapturingPath").value + jsFormatProcessDateToDigit(document.getElementById("sProcessingDate").innerText) + "\\spool";
                        var sFrontIMG = cbs[i].name.substring(3, 33) + "BF.TIF";
                        var sBackIMG = cbs[i].name.substring(3, 33) + "BB.TIF";
                        var sGFrontIMG = cbs[i].name.substring(3, 33) + "GF.JPG";
                        var sGBackIMG = cbs[i].name.substring(3, 33) + "GB.JPG";


                        //var x = new ActiveXObject("FujiIQA_activeXdll.FujiIQA_activeXdll");

                        //var res = ajaxFBIQA(logFullFilePath, settingPath, sChequePath, sFrontIMG, sBackIMG, sGFrontIMG, sGBackIMG);

                        var iqa = new ActiveXObject("FujiIQA_activeXdll.FujiIQA_activeXdll");

                        iqa.logFullFilePath = logFullFilePath;

                        iqa.settingPath = settingPath;

                        iqa.callSetting();

                        var objFS = new ActiveXObject("Scripting.FileSystemObject")
                        var c
                        if (objFS.FileExists(strFilePath)) {
                            var f = objFS.GetFile(strFilePath);
                            var size = f.size;
                            if (size) {
                                var objFile = objFS.OpenTextFile(strFilePath)
                                var outputDoc = objFile.readAll();
                                objFile.close()

                                //var xmlDoc = new ActiveXObject("MICROSOFT.FreeThreadedXMLDOM");
                                //xmlDoc.async = "false";
                                //xmlDoc.loadXML(outputDoc);

                                var parser = new DOMParser();
                                var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                                var nodeList = xmlDoc.getElementsByTagName("data");
                                var strDataXmlUIC;


                                if (nodeList.length > 0) {

                                    for (c = 0; c < nodeList.length; c++) {
                                        if (jsTrim(xmlDoc.getElementsByTagName("uic")[c].textContent) == cbs[i].name.substring(3, 29)) {
                                            if (xmlDoc.getElementsByTagName("iqa")[c].textContent.substring(0, 1) != '1') {
                                                var res = iqa.callScan(sChequePath, sFrontIMG, sBackIMG, sGFrontIMG, sGBackIMG);

                                                if (res == "") {
                                                    var IQARes = "Error";
                                                } else {
                                                    var IQARes = res.replace("|BB@", "").replace("BF@", "").replace("|GB@", "").replace("|GF@", "");

                                                }

                                                if (jsTrim(IQARes) == "") {
                                                    jsXMLInsertIQA(cbs[i].name.substring(3, 33), '1');
                                                } else {
                                                    var FailDesc = "";
                                                    var arrIQA = res.split("|");
                                                    for (var k = 0; k < arrIQA.length; k++) {
                                                        var result = arrIQA[k].split("@");
                                                        if (jsTrim(result[1]) != "") {
                                                            FailDesc = FailDesc + result[0] + " - " + result[1];
                                                        }
                                                    }
                                                    jsXMLInsertIQA(cbs[i].name.substring(3, 33), '2' + FailDesc);
                                                }
                                            }

                                        }
                                    }
                                }

                                //var serializer = new XMLSerializer();
                                //var xmlstring = serializer.serializeToString(xmlDoc);

                                //var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                                //objFileForWrite.write(xmlstring);
                                //objFileForWrite.close();


                            }
                        }




                        //myControl1.logFullFilePath = logFullFilePath;

                        //myControl1.settingPath = settingPath;
                        //myControl1.callSetting();

                        //var res = myControl1.callScan(sChequePath, sFrontIMG, sBackIMG, sGFrontIMG, sGBackIMG);

                        //if (res == "") {
                        //    var IQARes = "Error";
                        //} else {
                        //    var IQARes = res.replace("|BB@", "").replace("BF@", "").replace("|GB@", "").replace("|GF@", "");

                        //}

                        //if (jsTrim(IQARes) == "") {
                        //    jsXMLInsertIQA(cbs[i].name.substring(3, 33), '1');
                        //} else {
                        //    var FailDesc = "";
                        //    var arrIQA = res.split("|");
                        //    for (var k = 0; k < arrIQA.length; k++) {
                        //        var result = arrIQA[k].split("@");
                        //        if (jsTrim(result[1]) != "") {
                        //            FailDesc = FailDesc + result[0] + " - " + result[1];
                        //        }
                        //    }
                        //    jsXMLInsertIQA(cbs[i].name.substring(3, 33), '2' + FailDesc);
                        //}
                    }
                    //}

                }

                var objXMLField = $("#xmlFields");
                var objTblFooter = document.getElementById('tblFooter')
                var objTblBody = document.getElementById('tblBody')

                //Load XML Document   
                jsLoadXMLDoc();

                //read data xml document at client side
                //Reset table into empty table
                jsResetRowTemplate();

                //Insert data into row depends on the Mode BN: Bank Negara Malaysia Mode
                if (document.getElementById("hSelectedCapMode").value == "BN") {
                    jsBindLineItems_BNM(objXMLField, objTblFooter, objTblBody);
                }
                else {
                    jsBindLineItems(objXMLField, objTblFooter, objTblBody);
                }
                //alert("IQA testing is done.");
            }
            catch (e) {
                exceptionMsg = "Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                $("#sErrorMsg").text(exceptionMsg);
                $("#divErrorMsg").show();
            }
        }

        //Count IQA
        function jsIQACount() {
            var exceptionMsg = '';
            var PassIQA = 0;
            var FailIQA = 0;
            var PassMICR = 0;
            var FailMICR = 0;
            var foundDepositSlip = 0;
            var foundDuplicatedMICR = 0;
            var firstDepositSlip = 0;
            var invalidCheck = 0;
            // alert("aaa");
            //alert(strFilePath);
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
                var i
                if (objFS.FileExists(strFilePath)) {
                    var f = objFS.GetFile(strFilePath);
                    var size = f.size;
                    if (size) {
                        var objFile = objFS.OpenTextFile(strFilePath)
                        var outputDoc = objFile.readAll();
                        objFile.close()

                        //var xmlDoc = new ActiveXObject("MICROSOFT.FreeThreadedXMLDOM");
                        //xmlDoc.async = "false";
                        //xmlDoc.loadXML(outputDoc);

                        var parser = new DOMParser();
                        var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                        var nodeList = xmlDoc.getElementsByTagName("data");
                        var strDataXmlUIC;

                        if (nodeList.length > 0) {

                            for (i = 0; i < nodeList.length; i++) {

                                if (xmlDoc.getElementsByTagName("iqa")[i].textContent.substring(0, 1) == '1') {
                                    PassIQA = PassIQA + 1;
                                }

                                if (xmlDoc.getElementsByTagName("iqa")[i].textContent.substring(0, 1) == '2') {
                                    FailIQA = FailIQA + 1;
                                }
                            }
                        }
                        var serializer = new XMLSerializer();
                        var xmlstring = serializer.serializeToString(xmlDoc);

                        var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                        objFileForWrite.write(xmlstring);
                        objFileForWrite.close();

                        document.getElementById("sIQAList").value = "Passed:" + PassIQA + " Failed: " + FailIQA
                    }
                }

            } catch (e) {
                //alert("jsError2");
                exceptionMsg = "(jsXMLEndBatchValidation) Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                $("#sErrorMsg").text(exceptionMsg);
                $("#divErrorMsg").show();
                return "Error";
            }
        }

        //Count MICR
        function jsMICCount() {
            //debugger;
            var exceptionMsg = '';
            var PassMICR = 0;
            var FailMICR = 0;
            var DuplicateMICR = 0;
            var foundDepositSlip = 0;
            var foundDuplicatedMICR = 0;
            var firstDepositSlip = 0;
            var invalidCheck = 0;
            //alert("countMi");
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
                var i
                if (objFS.FileExists(strFilePath)) {
                    var f = objFS.GetFile(strFilePath);
                    var size = f.size;
                    if (size) {
                        var objFile = objFS.OpenTextFile(strFilePath)
                        var outputDoc = objFile.readAll();
                        objFile.close()

                        //var xmlDoc = new ActiveXObject("MICROSOFT.FreeThreadedXMLDOM");
                        //xmlDoc.async = "false";
                        //xmlDoc.loadXML(outputDoc);

                        var parser = new DOMParser();
                        var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                        var nodeList = xmlDoc.getElementsByTagName("data");
                        var strDataXmlUIC;

                        if (nodeList.length > 0) {

                            for (i = 0; i < nodeList.length; i++) {

                                if (xmlDoc.getElementsByTagName("valid_micr")[i].textContent == "P") {
                                    PassMICR = PassMICR + 1;
                                }

                                if (xmlDoc.getElementsByTagName("valid_micr")[i].textContent == "F") {
                                    FailMICR = FailMICR + 1;
                                }

                                if (xmlDoc.getElementsByTagName("valid_micr")[i].textContent == "D") {
                                    DuplicateMICR = DuplicateMICR + 1;
									$("#btnForceMICRPass").prop("disabled", true);
                                }
								else	
								{
									$("#btnForceMICRPass").prop("disabled", false);
                                }
                            }
                        }
                        var serializer = new XMLSerializer();
                        var xmlstring = serializer.serializeToString(xmlDoc);

                        var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                        objFileForWrite.write(xmlstring);
                        objFileForWrite.close();

                        document.getElementById("sMICRList").value = "Passed:" + PassMICR + " Failed: " + FailMICR + " Duplicate: " + DuplicateMICR
                    }
                }
            } catch (e) {
                //alert("jsError3");
                exceptionMsg = "(jsXMLEndBatchValidation) Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                $("#sErrorMsg").text(exceptionMsg);
                $("#divErrorMsg").show();
                return "Error";
            }
        }
        //jsHouseChq
        function jsHouseChq() {
            var exceptionMsg = '';
            var HouseChq = 0;
            var FailMICR = 0;
            var DuplicateMICR = 0;
            var foundDepositSlip = 0;
            var foundDuplicatedMICR = 0;
            var firstDepositSlip = 0;
            var invalidCheck = 0;
            //alert("countMi");
            //alert(strFilePath);
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
                var i
                if (objFS.FileExists(strFilePath)) {
                    var f = objFS.GetFile(strFilePath);
                    var size = f.size;
                    if (size) {
                        var objFile = objFS.OpenTextFile(strFilePath)
                        var outputDoc = objFile.readAll();
                        objFile.close()
                        //var xmlDoc = new ActiveXObject("MICROSOFT.FreeThreadedXMLDOM");
                        //xmlDoc.async = "false";
                        //xmlDoc.loadXML(outputDoc);
                        var parser = new DOMParser();
                        var xmlDoc = parser.parseFromString(outputDoc, "text/xml");
                        var nodeList = xmlDoc.getElementsByTagName("data");
                        //var valid_MICRNode = xmlDoc.createElement("valid_MICR");
                        var strDataXmlUIC;
                        if (nodeList.length > 0) {
                            for (i = 0; i < nodeList.length; i++) {
                                if (xmlDoc.getElementsByTagName("checktype")[i].textContent == 'H') {
                                    HouseChq = HouseChq + 1;
                                }
                            }
                        }
                        var serializer = new XMLSerializer();
                        var xmlstring = serializer.serializeToString(xmlDoc);
                        var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                        objFileForWrite.write(xmlstring);
                        objFileForWrite.close();
                        //document.getElementById("sHouseChq").value = "House Cheque:" + HouseChq
                    }
                }

            } catch (e) {
                //alert("jsError3");
                exceptionMsg = "(jsXMLEndBatchValidation) Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                $("#sErrorMsg").text(exceptionMsg);
                $("#divErrorMsg").show();
                return "Error";
            }
        }

        //ImmediateEntry
        function jsShowImmediateEntry(strUIC) {
            //var strReturnResult;

            //strReturnResult = window.showModalDialog("ImmediateEntry.aspx?taskid=" + g_strTaskId + "&uic=" + strUIC, "", "dialogHeight:" + screen.height + "; dialogWidth:" + screen.width);
            //return strReturnResult;
        }

        //Check batch
        function jsCheckBatch() {
            var filesys, objfileSize, createdate;
            filesys = CreateObject("Scripting.FileSystemObject");

            //If Err.number <> 0 Then
            //Call jsAlert(jsRetrieveMessage("msgStrActiveXControlError1"))
            //'//Call jsAlert("Fail to link to text file. Please click 'Ok' to reload ActiveX Control.")
            //Call jsReloadPage()
            //Exit Function
            //End If

            filesys.GetFile(strFilePath);
            if (objfileSize.Size > 100) {
                // debugger;
                $("#btnScan").prop("disabled", false);
                $("#btnEject").prop("disabled", false);
                $("#btnEndBatch").prop("disabled", false);
                $("#btnRemove").prop("disabled", false);
                $("#btnRemoveAll").prop("disabled", false);
                $("#btnIQA").prop("disabled", false);
                $("#btnRescan").prop("disabled", true);
            }
            else {
                $("#btnScan").prop("disabled", false);
                $("#btnEject").prop("disabled", false);
                $("#btnIQA").prop("disabled", true);
                $("#btnRescan").prop("disabled", true);
                $("#btnEndBatch").prop("disabled", true);
                $("#btnRemove").prop("disabled", true);
                $("#btnRemoveAll").prop("disabled", true);
            }//End If

            filesys = null;//Set filesys = Nothing
        }

        //Add default value into data.xml
        function jsAddDefaultDataXml() {
            //debugger;
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject");
            }
            catch (e) {
                //("Cannot create text file.");
                $("#sErrorMsg").text("Cannot create text file.");
                $("#divErrorMsg").show();
            }

            var xmlData = $("#xmlData").html();
            xmlData = '<?xml version=\"1.0\"?>\n' + xmlData;
            var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
            objFileForWrite.write(xmlData);
            objFileForWrite.close();
        }

        //Reset the row into empty row
        function jsResetRowTemplate() {
            var objTblBody = document.getElementById('tblBody')
            var objTblBodyLen = objTblBody.rows.length

            for (var i = objTblBodyLen - 1; i >= 0; i--) {
                objTblBody.deleteRow(i)
            }
        }

        //data xml
        function jsEditDataXmlNCF(intUIC, intMICR, intNCF, strBranchSource, strFloatDays, strCaptureMode, strIQA) {

            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
            }
            catch (e) {
                //("Fail to load local file");
                $("#sErrorMsg").text("Fail to load local file");
                $("#divErrorMsg").show();
                jsRefreshPage();
            }
            var i
            if (objFS.FileExists(strFilePath)) {

                var f = objFS.GetFile(strFilePath);
                var size = f.size;

                if (size) {

                    var objFile = objFS.OpenTextFile(strFilePath)
                    var outputDoc = objFile.readAll();
                    objFile.close()

                    try {
                        var xmlDoc;
                        if (window.DOMParser) {
                            // code for modern browsers
                            parser = new DOMParser();
                            xmlDoc = parser.parseFromString(outputDoc, "text/xml");
                        } else {
                            // code for old IE browsers
                            xmlDoc = new ActiveXObject("Microsoft.FreeThreadedXMLDOM");
                            xmlDoc.async = false;
                            xmlDoc.loadXML(outputDoc);
                        }
                    }
                    catch (e) {
                        //("Fail to load xml file: " + e);
                        $("#sErrorMsg").text("Fail to load xml file: " + e);
                        $("#divErrorMsg").show();
                    }

                    //=========================================

                    var scannerId = xmlDoc.getElementsByTagName("scanner_id")[0].textContent;
                    if (jsTrim(scannerId) == '') {
                        xmlDoc.getElementsByTagName("scanner_id")[0].textContent = $("#sScannerId").text();
                    }

                    var intRecordCount; //Make counter for document scan
                    intRecordCount = parseInt(xmlDoc.getElementsByTagName("total_rec")[0].textContent);
                    intRecordCount = intRecordCount + 1;
                    //Set total record value into xml field call total_rec
                    xmlDoc.getElementsByTagName("total_rec")[0].textContent = intRecordCount;
                    //Set batch no value into xml field Batch_id
                    xmlDoc.getElementsByTagName("batch_id")[0].textContent = $("#sBatchNo").text();

                    //Priority
                    xmlDoc.getElementsByTagName("priority")[0].textContent = $(':radio[name=optPriority]:checked').val();

                    //Late Items?
                    xmlDoc.getElementsByTagName("late")[0].textContent = "0";
                    //var objOptLateItems = document.all.optLateItems
                    //for (i = 0; i < objOptLateItems.length; i++) {
                    //    if (objOptLateItems[i].checked == true) {
                    //        xmlDoc.selectSingleNode("//root/scanner/late").text = objOptLateItems[i].value;
                    //        break;
                    //    }
                    //}

                    //Currency
                    xmlDoc.getElementsByTagName("currency")[0].textContent = $(':radio[name=optCurrencyCode]:checked').val();

                    //Bank Type?
                    strBankType = "02";
                    //var objOptBankType = document.all.optBankType
                    //var strBankType;
                    //for (i = 0; i < objOptBankType.length; i++) {
                    //    if (objOptBankType[i].checked == true) {
                    //        strBankType = objOptBankType[i].value;
                    //        break;
                    //    }
                    //}

                    ////Set non-conforming flag into nc_flag Node Field
                    //var strNCFlag = xmlDoc.selectSingleNode("//root/scanner/nc_flag").text
                    //// Begin Mod: 27-DEC-2007 Liew Yu Joe Change get method on nc_flag
                    //if (jsTrim(strNCFlag) == '') {
                    //    var objOptAmount = document.all.optAmount;
                    //    for (i = 0; i < objOptAmount.length; i++) {
                    //        if (objOptAmount[i].checked == true) {
                    //            xmlDoc.selectSingleNode("//root/scanner/nc_flag").text = objOptAmount[i].value;
                    //            break;
                    //        }
                    //    }
                    //    //xmlDoc.selectSingleNode("//root/scanner/nc_flag").text = jsRight(arrMICR[4], 1);
                    //}

                    ////LockBox
                    //if (document.getElementById("hLockBoxKey").value != "undefined") {
                    //    xmlDoc.selectSingleNode("//root/scanner/flag1").text = document.getElementById("hLockBoxKey").value;
                    //}
                    //else {
                    //    xmlDoc.selectSingleNode("//root/scanner/flag1").text = "";
                    //}

                    ////Teller Id
                    //if (document.getElementById("txtTellerId").value != "undefined") {
                    //    xmlDoc.selectSingleNode("//root/scanner/flag5").text = document.getElementById("txtTellerId").value;
                    //}
                    //else {
                    //    xmlDoc.selectSingleNode("//root/scanner/flag5").text = "";
                    //}

                    ////Encashment
                    //if (String(document.all.optEncashment) != "undefined") {
                    //    var objEncashment = document.all.optEncashment;
                    //    var strEncashment = "";
                    //    for (i = 0; i < objEncashment.length; i++) {
                    //        if (objEncashment[i].checked == true) {
                    //            strEncashment = objEncashment[i].value;
                    //            break;
                    //        }
                    //    }
                    //    xmlDoc.selectSingleNode("//root/scanner/flag3").text = strEncashment;
                    //}
                    //else {
                    //    xmlDoc.selectSingleNode("//root/scanner/flag3").text = "";
                    //}

                    var arrMICR = "";
                    //alert("1");
                    //alert(intMICR);
                    arrMICR = jsArrGetSplitMICR(intMICR);


                    //arrMICR = intMICR;
                    //alert(arrMICR);
                    //Split the MICR value into array list
                    //0: Check Digit, 1: Serial Number,
                    //2: Bank Code, 3: Branch Code,
                    //4: Account Number, 5: Transaction code
                    //6: Amount, 7: Bank Type

                    //arrMICR[0] = strCheckDigit;                         //Get Check Digit
                    //arrMICR[1] = strType;                       //Get Serial Number
                    //arrMICR[2] = strLocAndBankCode;                           //Get Bank Code
                    //arrMICR[3] = strBranchCode;                         //Get Branch Code
                    //arrMICR[4] = strAccount;                            //Get Account Number
                    //arrMICR[5] = strSerialNumber;                    //Get Transcation Code
                    //arrMICR[6] = strAmount;                             //Get Amount
                    //arrMICR[7] = strBankType;                           //Get Branch Type or Bank Type
                    //arrMICR[8] = strBranchId;                           //Get clearing branch id
                    //arrMICR[6] = strBankType;                           //Get Branch Type or Bank Type
                    //arrMICR[7] = strBranchId;                           //Get clearing branch id


                    if ((arrMICR != "") && (arrMICR != null)) {
                        //Check on MICR check digit, serial number and account number
                        //intMICR = "na" + arrMICR[0] + "s" + arrMICR[1] + "s" + arrMICR[2] + "e" + arrMICR[3] + "a" + arrMICR[4] + "s" + arrMICR[5] + "x" + arrMICR[6] + "xm";
                        //intMICR = "na" + arrMICR[0] + "s" + arrMICR[1] + "s" + arrMICR[2] + "e" + arrMICR[3] + "a" + arrMICR[5] + "s" + arrMICR[4] + "x" + arrMICR[6] + "xm";
                        intMICR = "na" + arrMICR[0] + "s" + arrMICR[1] + "s" + arrMICR[2] + "e" + arrMICR[3] + "a" + arrMICR[5] + "s" + arrMICR[4] + "xm";
                        var d = new Date();
                        var strHours = d.getHours();
                        var strMinutes = d.getMinutes();

                        //Change on process date
                        var strDateCapture = $("#hClrDate").val();

                        var strTimeCapture = jsRight("00" + strHours, 2) + jsRight("00" + strMinutes, 2);


                        //Set date into capturing_date Node Field
                        if (jsTrim(xmlDoc.getElementsByTagName("capturing_date")[0].textContent) == '') {
                            xmlDoc.getElementsByTagName("capturing_date")[0].textContent = strDateCapture;
                        }

                        xmlDoc.getElementsByTagName("capturing_time")[0].textContent = strTimeCapture;

                        //Set lblClrBranchId into clearing_branch Node Field
                        var strClrBranch = xmlDoc.getElementsByTagName("clearing_branch")[0].textContent;
                        if (jsTrim(strClrBranch) == '') {
                            xmlDoc.getElementsByTagName("clearing_branch")[0].textContent = $("#sClrBranchId").text();
                        }

                        //Set hCapBranch into capturing_branch Node Field
                        var strCapBranch = xmlDoc.getElementsByTagName("capturing_branch")[0].textContent;
                        if (jsTrim(strCapBranch) == '') {
                            xmlDoc.getElementsByTagName("capturing_branch")[0].textContent = $("#hCapBranch").val();
                        }

                        //Set hSelectedCapMode into capturing_mode Node Field   
                        var strCapMode = xmlDoc.getElementsByTagName("capturing_mode")[0].textContent;
                        if (jsTrim(strCapMode) == '') {
                            xmlDoc.getElementsByTagName("capturing_mode")[0].textContent = $("#hSelectedCapMode").val();
                        }

                        //Set hSelectedPostingMode into posting_mode Node Field
                        var strCapMode = xmlDoc.getElementsByTagName("posting_mode")[0].textContent;
                        if (jsTrim(strCapMode) == '') {
                            xmlDoc.getElementsByTagName("posting_mode")[0].textContent = $("#hSelectedPostingMode").val();
                        }

                        //Set hChqType into cheque_type Node Field   
                        var strChqType = xmlDoc.getElementsByTagName("cheque_type")[0].textContent;
                        if (jsTrim(strChqType) == '') {
                            xmlDoc.getElementsByTagName("cheque_type")[0].textContent = $("#hChqType").val();
                        }

                        //Set g_intUserId into capturing_by field
                        var strCapturingBy = xmlDoc.getElementsByTagName("capturing_by")[0].textContent;
                        if (jsTrim(strCapturingBy) == '') {
                            xmlDoc.getElementsByTagName("capturing_by")[0].textContent = g_intUserId;
                        }

                        //Set Post Dated Cheque Status & CICS Mode
                        xmlDoc.getElementsByTagName("cheque_status")[0].textContent = $("#hChequeStatus").val();

                        //Set bank type into bank_type Node Field
                        //var strBankType = xmlDoc.getElementsByTagName("bank_type")[0].childNodes[0].nodeValue;
                        //if (jsTrim(strBankType) == '') {
                        //    xmlDoc.getElementsByTagName("bank_type")[0].textContent = strBankType;
                        //}
                        xmlDoc.getElementsByTagName("bank_type")[0].textContent = strBankType;

                        //Check if user select on Bank Acceptance - Document to Follow
                        if ($("#hStrChqType").val() != "") {
                            if ($("#hStrChqType").val() == "BD") {
                                if (jsTrim(strCapMode) == '') {
                                    xmlDoc.getElementsByTagName("dtf_flag")[0].textContent = 1; //dtf_flag
                                }
                            }
                        }

                        var nodeList = xmlDoc.getElementsByTagName("datas");

                        if (nodeList.length > 0) {

                            var parentNode = nodeList(0);
                            var dataNode = xmlDoc.createElement("data");
                            var uicNode = xmlDoc.createElement("uic");

                            //IQA Required
                            var iqaNode = xmlDoc.createElement("iqa");
                            //alert(strIQA);
                            //Micr required
                            var valid_MICRNode = xmlDoc.createElement("valid_micr");
                            //alert(valid_MICRNode);
                            //NCF Required
                            var ncfNode = xmlDoc.createElement("ncf");

                            //SourceBranch and FloatDays
                            var sourcebranchNode = xmlDoc.createElement("sourcebranch");
                            var floatdaysNode = xmlDoc.createElement("floatdays");

                            var item_typeNode = xmlDoc.createElement("item_type");
                            //alert(item_typeNode);
                            //Add Type if its House Check or Regular Check
                            //House check - Owned By the Bank
                            //Regular Check - From other Bank
                            var checktypenode = xmlDoc.createElement("checktype");

                            //Tag if its NON CICS or CICS mode under datas
                            var cicsmode = xmlDoc.createElement("cicsmode");

                            //Trans type and Presentment
                            var transtypenode = xmlDoc.createElement("trans_type");
                            var presentmentnode = xmlDoc.createElement("presentment");
                            var typeNode = xmlDoc.createElement("type");
                            //alert("typeNode");
                            var micrNode = xmlDoc.createElement("micr");
                            var ref1Node = xmlDoc.createElement("ref1");
                            var ref2Node = xmlDoc.createElement("ref2");
                            var ref3Node = xmlDoc.createElement("ref3");
                            var ref4Node = xmlDoc.createElement("ref4");
                            var ref5Node = xmlDoc.createElement("ref5");
                            var b_front_imgNode = xmlDoc.createElement("b_front_img");
                            var b_front_img_chksumNode = xmlDoc.createElement("b_front_img_chksum");
                            var b_back_imgNode = xmlDoc.createElement("b_back_img");
                            var b_back_img_chksumNode = xmlDoc.createElement("b_back_img_chksum");
                            var g_front_imgNode = xmlDoc.createElement("g_front_img");
                            var g_front_img_chksumNode = xmlDoc.createElement("g_front_img_chksum");
                            var g_back_imgNode = xmlDoc.createElement("g_back_img");
                            var g_back_img_chksumNode = xmlDoc.createElement("g_back_img_chksum");

                            // Append Payee Account's value into data.xml
                            var payeeNode = xmlDoc.createElement("payee_accounts");
                            var totalAmountNode = xmlDoc.createElement("total_amount");
                            var chequeDateNode = xmlDoc.createElement("cheque_date");
                            var accountNode = xmlDoc.createElement("account");

                            document.getElementById("hLastUIC").value = ""

                            uicNode.textContent = intUIC;
                            document.getElementById("hLastUIC").value = intUIC;


                            intMICR = jsTrim(intMICR)

                            //Add Flag For Cheque Type
                            //0 - Regular Check
                            //1 - On us Check

                            var strChequeType = intMICR;
                            var BackCode = strChequeType.indexOf("e");
                            var ChequeFlag
                            var GetBRSTNPosition
                            var GetBankCode

                            //GetBankCode = jsGetBk(strChequeType).substring(1,4)
                            //alert(GetBankCode);
                            ////ChequeFlag = '001' //
                            //GetBankCode = strLocAndBankCode
                            //alert(GetBankCode);
                            ChequeFlag = jsGetBk(strChequeType).substring(1, 4) //strChequeType.substring(GetBankCode, GetBankCode + 3)
                            //alert(ChequeFlag);
                            var sUserBankCode = $("#hUserBankCode").val();

                            //specify the bank code under chequeflag
                            //if (ChequeFlag == sUserBankCode) {
                            //    checktypenode.textContent = "F"
                            //} else {
                            //    checktypenode.textContent = "P"
                            //}

                            if (ChequeFlag != "") {
                                if (ChequeFlag == sUserBankCode) {
                                    checktypenode.textContent = "F"
                                }

                                else if (ajaxCheckBank(ChequeFlag) == true) {
                                    checktypenode.textContent = "F"
                                }

                                else {
                                    checktypenode.textContent = "P"
                                }

                            }
                            else {
                                checktypenode.textContent = "P"

                                //if (strCaptureMode == "CP") {
                                //    checktypenode.textContent = "P"
                                //} else {
                                //    checktypenode.textContent = "F"
                                //}
                            }
                            //CICS
                            cicsmode.textContent = $(':radio[name=optCICSModeType]:checked').val();

                            //Transtype
                            transtypenode.textContent = $(':radio[name=optChequeTranType]:checked').val();

                            //Presentment
                            presentmentnode.textContent = $(':radio[name=optPresentment]:checked').val();

                            if (strCaptureMode == "CO") {
                                if (arrMICR.length < 14 && arrMICR[1] != "" && arrMICR[2] != "" && arrMICR[3] != "" && arrMICR[4] != "" && arrMICR[8] != "") {

                                    item_typeNode.textContent = "C"

                                }
                                else {
                                    //alert("yy1111bbb");
                                    item_typeNode.textContent = "C"
                                    //alert(item_typeNode.textContent);

                                }
                                //For Check with PaySlip And Image Replacement 
                            } else if (strCaptureMode == "CP" || strCaptureMode == "IR") {

                                if (arrMICR.length < 14 && arrMICR[1] != "" && arrMICR[2] != "" && arrMICR[3] != "" && arrMICR[4] != "" && arrMICR[8] != "") {
                                    item_typeNode.textContent = "C"
                                }
                                else {
                                    item_typeNode.textContent = "P"
                                }
                            }

                            else if (strCaptureMode == "CS") {
                                if (intMICR.length > 14) {
                                    item_typeNode.textContent = "F"
                                }
                                else {
                                    item_typeNode.textContent = "C"
                                }
                            }

                            //Reinsert back to hidden micr value
                            document.getElementById("hMICR").value = intMICR
                            typeNode.textContent = arrMICR[1];
                            micrNode.textContent = intMICR
                            //IQA Required
                            if (item_typeNode.textContent == "P" || strCaptureMode == "CS") {
                                iqaNode.textContent = "1";
                            } else {
                                iqaNode.textContent = strIQA;
                            }
                            //2b. ForceMICR
                            // debugger;
                            if (item_typeNode.textContent == "C") {
                                var strMICR = "";
                                //alert("if C");
                                if (intMICR.length <= 9) {
                                    //alert("length less than 4");
                                    valid_MICRNode.textContent = "F";
                                }
                                else if (intMICR.length != 46) {
                                    //if (intMICR.search(strSearchMICRPattern) > -1) {
                                    //alert("got ?");
                                    valid_MICRNode.textContent = "F";
                                }
                                else if (intMICR.indexOf('?') > -1) {
                                    //if (intMICR.search(strSearchMICRPattern) > -1) {
                                    //alert("got ?");
                                    valid_MICRNode.textContent = "F";
                                }
                                //else if (strMICR.search(intMICR) > -1 ) {

                                //    if (intMICR.length > 9) {
                                //        valid_MICRNode.textContent = "D";
                                //    }
                                //}
                                else {
                                    var objFS2 = new ActiveXObject("Scripting.FileSystemObject")
                                    var c
                                    if (objFS2.FileExists(strFilePath)) {
                                        var f = objFS2.GetFile(strFilePath);
                                        var size = f.size;
                                        if (size) {
                                            var objFile2 = objFS2.OpenTextFile(strFilePath)
                                            var outputDoc2 = objFile2.readAll();
                                            objFile2.close()

                                            var parser2 = new DOMParser();
                                            var xmlDoc2 = parser2.parseFromString(outputDoc2, "text/xml");

                                            var nodeList2 = xmlDoc2.getElementsByTagName("data");
                                            var strDataXmlUIC;


                                            if (nodeList2.length > 0) {

                                                for (c = 0; c < nodeList2.length; c++) {
                                                    if (jsTrim(xmlDoc2.getElementsByTagName("micr")[c].textContent) == intMICR) {
                                                        valid_MICRNode.textContent = "D";
                                                        break;
                                                    }
                                                    else {
                                                        valid_MICRNode.textContent = "P";
                                                    }
                                                }
                                            }
                                            else {
                                                valid_MICRNode.textContent = "P";
                                            }

                                        }
                                    }
                                    //valid_MICRNode.textContent = "P";
                                }
                            }
                            else if (item_typeNode.textContent == "P") {
                                valid_MICRNode.textContent = "N/A";
                            }
                            else {
                                //alert("NOt C");
                                valid_MICRNode.textContent = "F";
                            }

                            //NCF Required
                            ncfNode.textContent = intNCF

                            //SourceBranch and FloatDays
                            strFloatDays = $("#hFloatDays").val();
                            sourcebranchNode.textContent = strBranchSource
                            floatdaysNode.textContent = strFloatDays

                            ref1Node.textContent = ""
                            ref2Node.textContent = ""
                            ref3Node.textContent = ""
                            ref4Node.textContent = "SCN"
                            ref5Node.textContent = ""
                            b_front_imgNode.textContent = intUIC + "BF.TIF"
                            b_front_img_chksumNode.textContent = ""
                            b_back_imgNode.textContent = intUIC + "BB.TIF"
                            b_back_img_chksumNode.textContent = ""
                            g_front_imgNode.textContent = intUIC + "GF.JPG"
                            g_front_img_chksumNode.textContent = ""
                            g_back_imgNode.textContent = intUIC + "GB.JPG"
                            g_back_img_chksumNode.textContent = ""

                            totalAmountNode.textContent = arrMICR[6];
                            chequeDateNode.textContent = "";

                            parentNode.appendChild(dataNode);
                            dataNode.appendChild(uicNode);

                            //IQA Required
                            dataNode.appendChild(iqaNode);

                            //2b. Valid MICR
                            dataNode.appendChild(valid_MICRNode);
                            //alert(valid_MICRNode);
                            //NCF Required
                            dataNode.appendChild(ncfNode);

                            //SourceBranch and FloatDays
                            dataNode.appendChild(sourcebranchNode);
                            dataNode.appendChild(floatdaysNode);

                            dataNode.appendChild(item_typeNode);

                            //Checktypenode and CICSmode
                            dataNode.appendChild(checktypenode);
                            dataNode.appendChild(cicsmode);

                            //Transtype and Presentment
                            dataNode.appendChild(presentmentnode);
                            dataNode.appendChild(transtypenode);
                            dataNode.appendChild(typeNode);
                            dataNode.appendChild(micrNode);
                            dataNode.appendChild(ref1Node);
                            dataNode.appendChild(ref2Node);
                            dataNode.appendChild(ref3Node);
                            dataNode.appendChild(ref4Node);
                            dataNode.appendChild(ref5Node);
                            dataNode.appendChild(b_front_imgNode);
                            dataNode.appendChild(b_front_img_chksumNode);
                            dataNode.appendChild(b_back_imgNode);
                            dataNode.appendChild(b_back_img_chksumNode);
                            dataNode.appendChild(g_front_imgNode);
                            dataNode.appendChild(g_front_img_chksumNode);
                            dataNode.appendChild(g_back_imgNode);
                            dataNode.appendChild(g_back_img_chksumNode);
                            dataNode.appendChild(payeeNode);

                            payeeNode.appendChild(totalAmountNode);
                            payeeNode.appendChild(chequeDateNode);
                            payeeNode.appendChild(accountNode);

                            var payeeNodeList = payeeNode.getElementsByTagName("account");
                            var accountNumberNode = xmlDoc.createElement("account_number");
                            var amountNode = xmlDoc.createElement("amount");

                            if (document.getElementById("hLockBoxKey").value != "undefined") {
                                if (document.getElementById("hOCRRequire").value != 'Y') {
                                    accountNumberNode.text = 0;
                                }
                                else {
                                    accountNumberNode.text = document.getElementById("hLockBoxAccNo").value;
                                }
                            }
                            else {
                                accountNumberNode.text = "";
                            }
                            amountNode.text = "";

                            payeeNodeList = payeeNodeList(0);
                            payeeNodeList.appendChild(accountNumberNode);
                            payeeNodeList.appendChild(amountNode);
                        }
                        else {
                            //("Fail to update scanning items information in the data.xml")
                            $("#sErrorMsg").text("Fail to update scanning items information in the data.xml");
                            $("#divErrorMsg").show();
                            return false;
                        }



                        var s = new XMLSerializer();
                        var newXmlStr = s.serializeToString(xmlDoc);
                        newXmlStr = '<?xml version=\"1.0\"?>\n' + newXmlStr;

                        //Write into data.xml
                        var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                        objFileForWrite.write(newXmlStr);
                        objFileForWrite.close();

                        var objXMLField = $("#xmlFields");
                        var objTblFooter = document.getElementById('tblFooter');
                        var objTblBody = document.getElementById('tblBody');

                        jsLoadXMLDoc();                                          //read data xml document at client side
                        jsResetRowTemplate();
                        jsBindLineItems(objXMLField, objTblFooter, objTblBody);
                        return true;
                    }
                    else {
                        //// Begin Mod: Liew Yu Joe 2007/10/21
                        //var blnCheckBFFileExist = false;
                        //var blnCheckBBFileExist = false;
                        //var blnCheckGFFileExist = false;
                        //var blnCheckGBFileExist = false;

                        //// Delete image
                        //try {
                        //    var objFS = new ActiveXObject("Scripting.FileSystemObject")
                        //}
                        //catch (e) {
                        //    Alert("Fail to load local file");
                        //    jsRefreshPage();
                        //}

                        //blnCheckBFFileExist = objFS.FileExists(strFolderName + "\\" + intUIC + "BF.JPG")
                        //blnCheckBBFileExist = objFS.FileExists(strFolderName + "\\" + intUIC + "BB.JPG")
                        //blnCheckGFFileExist = objFS.FileExists(strFolderName + "\\" + intUIC + "GF.JPG")
                        //blnCheckGBFileExist = objFS.FileExists(strFolderName + "\\" + intUIC + "GB.JPG")
                        //if ((blnCheckGBFileExist == true) || (blnCheckGFFileExist == true) ||
                        //    (blnCheckBBFileExist == true) || (blnCheckBFFileExist == true)) {

                        //    alert("Invalid MICR, please check your cheque. It will not perform update in data.xml");
                        //}

                        //objFS.DeleteFile(strFolderName + "\\" + intUIC + "BF.TIF", true)
                        //objFS.DeleteFile(strFolderName + "\\" + intUIC + "BB.TIF", true)
                        //objFS.DeleteFile(strFolderName + "\\" + intUIC + "GF.JPG", true)
                        //objFS.DeleteFile(strFolderName + "\\" + intUIC + "GB.JPG", true)

                        //// Set total count
                        //if (frmCapturing.hTotalCounter.value != 0) {
                        //    frmCapturing.hTotalCounter.value = frmCapturing.hTotalCounter.value - 1
                        //}

                        //jsDeleteUICInfoSeqNo(g_intScannerId, g_intUserId, g_strTaskId);

                        //return false;
                    }
                }
            }
        }

        //Check MICR value whether is valid
        //true: it is not valid cheque, false: it is valid cheque
        function jsCheckMICR(blnActivation, strMICR, intCounter) {
            var strReturnResult = '';
            var arrMICR;
            //Split the MICR value into array list
            //0: Check Digit, 1: Serial Number,
            //2: Bank Code, 3: Branch Code,
            //4: Account Number, 5: Transaction code
            //6: Amount 7: Bank Type 8: Clearing Branch Id
            arrMICR = jsArrGetSplitMICR(strMICR);

            //Add function check on current path address and re-open modal dialog
            var strLocation = window.location.pathname;
            var arrLocation = strLocation.split("/");
            var strNewLocation = "";

            for (var i = 0; i < arrLocation.length - 1; i++) {
                //Reset path
                strNewLocation = strNewLocation + arrLocation[i] + "\\";
            }

            strNewLocation = window.location.protocol + "//" + window.location.host + strNewLocation

            strNewLocation = strNewLocation.replace(/\\/g, "/")

            var blnCheckMessage = false;
            var blnValidateMICR = false;
            blnValidateMICR = jsValidateMICR(strMICR);

            if (blnActivation == true) { //For Cheque Only and BNM Mode purpose

                // if bank code, branch code and transaction code is not valid length, it need to rescan the cheque
                if (document.getElementById("hSelectedCapMode").value != 'BN') {

                    //Validate MICR
                    if (blnValidateMICR == true) {

                        if ((g_intEjectErrorMICR != null) && (g_intEjectErrorMICR == 1)) {
                            //jsEject();
                        }
                        //strReturnResult = window.showModalDialog(strNewLocation + "CaptureOption.aspx?TaskId=" + g_strTaskId, "", "dialogHeight:340px; dialogWidth:400px");

                        if (strReturnResult != null && strReturnResult != "") {
                            switch (strReturnResult) {
                                case 1:
                                    { //Recapture the image and use back the old UIC and does not print endorsement
                                        //Always get the first row of UIC value
                                        var objTableBody = document.getElementById("tblBody");

                                        document.getElementById("hRowClickUIC").value = objTableBody.rows(0).children(5).innerText;

                                        if (document.getElementById("hRowClickUIC").value != '') {
                                            jsReplaceImage();
                                            return true;
                                        }
                                    }
                                case 2:
                                    { //Scan next cheque and use new UIC and print endorsement
                                        var blnConfirm = false;

                                        //("You will scan next cheque. Please check your cheque whether is placed correctly.");
                                        $("#sErrorMsg").text("You will scan next cheque. Please check your cheque whether is placed correctly.");
                                        $("#divErrorMsg").show();

                                        //Get the new UIC
                                        var blnGetUICInfo;
                                        var strUIC;

                                        if (blnConfirm == true) {
                                            //Delete last value in the xml node
                                            jsDeleteXMLNode();

                                            //Minus one in total counter
                                            frmCapturing.hTotalCounter.value = frmCapturing.hTotalCounter.value - 1

                                            if ((g_intEjectErrorMICR != null) && (g_intEjectErrorMICR == 1)) {
                                                vbDecreaseUIC();
                                            }

                                            //Scan the next item
                                            vbScan();

                                            // Reserver for Immediate entry only
                                            // optEntryYes.checked = true
                                            if (document.getElementById("optEntryYes").checked == true) {
                                                document.getElementById("hRefreshPage").value = "Y"
                                            }

                                            return true;
                                        }
                                        else {
                                            return true; // Do not continue to scan
                                        }
                                    }
                                case 3:
                                    { //if user click on end batch, the current batch will be ended
                                        //(jsRetrieveMessage("msgStrBatchWillEnd"))
                                        //("Batch ended");
                                        $("#sNotice").text("Batch ended");
                                        $("#divNotice").show();

                                        //Delete last value in the xml node
                                        jsDeleteXMLNode();

                                        frmCapturing.hTotalCounter.value = frmCapturing.hTotalCounter.value - 1

                                        jsLoadXMLDoc();
                                        jsCompleteBatch(1);
                                        document.getElementById("hPromptMaxMessage").value = 2;
                                        return true; // Do not continue to scan
                                    }
                                default: { return false; } // Do not continue to scan
                            }
                        }
                    }
                    else {
                        return false; //return false to continue
                    }
                }
                else { //Repair MICR for Bank Negara mode only
                    //...
                }
            }
        }


        //ns84s1a10001e0001a000001s1000020000134567880m


        //Get splited MICR information
        function jsArrGetSplitMICR(strMICR) {
            //alert("micr");
            //alert("2");
            if (strMICR != "") {
                var arrMICR = new Array;

                var strCheckDigit = jsGetCDCTS(strMICR);               //'Get Check Digit - 0 digit
                //alert(strCheckDigit);
                var strType = jsGetTypeCTS(strMICR);            //'Get Serial Number - 10 digit
                // alert(strType);
                var strLocAndBankCode = jsGetBk(strMICR);                 //'Get statecode(2)+Bank Code(3) - 5 digit
                // alert(strLocAndBankCode);
                var strBranchCode = jsGetBr(strMICR);               //'Get Routing Number - 4 digit
                //alert(strBranchCode);
                var strSerialNumber = jsGetSerCTS(strMICR);            //'Get Serial Number - 10 digit
                // alert(strSerialNumber);
                var strAccount = jsGetAccCTS(strMICR);                 //'Get Account Number - 13 digit
                // alert(strAccount);
                var strTransactionCode = jsGetTC(strMICR);          //'Get Transaction Code - 3 digit
                //  alert(strTransactionCode);
                var strAmount = jsGetAmtCTS(strMICR);                  //'Get Amount - 13 digit
                // alert(strAmount);
                var strBranchId = "" + strLocAndBankCode.substr(1, 3) + strBranchCode; //'Get Branch Id - 9 digit

                var strBankType = "02"//?                           //'Get Branch Type or Bank Type - 2 digit


                arrMICR[0] = strCheckDigit;                         //Get Check Digit
                arrMICR[1] = strType;                       //Get Serial Number
                arrMICR[2] = strLocAndBankCode;                           //Get Bank Code
                arrMICR[3] = strBranchCode;                         //Get Branch Code
                arrMICR[4] = strAccount;                            //Get Account Number
                arrMICR[5] = strSerialNumber;                    //Get Transcation Code
                arrMICR[6] = strAmount;                             //Get Amount
                arrMICR[7] = strBankType;                           //Get Branch Type or Bank Type
                arrMICR[8] = strBranchId;                           //Get clearing branch id
                arrMICR[6] = strBankType;                           //Get Branch Type or Bank Type
                arrMICR[7] = strBranchId;                           //Get clearing branch id
                //alert(arrMICR);
                return arrMICR;

            }
        }

        //Append new row
        function jsAddRow(tblTemplate, tblBody) {
            var newRow;
            newRow = tblTemplate.rows[0].cloneNode(true);
            tblBody.appendChild(newRow);
        }

        //For Checking client and server batch no and sequence no.
        function jsSplitUIC(intSplitUIC) {
            var arrSplitUIC = new Array(); //UIC Array
            var intNodeLength;

            if (intSplitUIC.length > 0) {
                //0 - get sequence value
                //arrSplitUIC[0] = parseInt(intSplitUIC.substring(22, 30), 10); // Convert to octal value
                arrSplitUIC[0] = parseInt(intSplitUIC.substring(22, 30), 10); // Convert to octal value
                //1 - get bank type value
                arrSplitUIC[1] = parseInt(intSplitUIC.substring(10, 8), 10); // Convert to octal value
                //2 - get cheque type
                arrSplitUIC[2] = parseInt(intSplitUIC.substring(20, 18), 10); // Convert to octal value
            }
            return arrSplitUIC;
        }

        //Format process date
        function jsFormatProcessDate(intDay, intMonth, intYear, blnWriteHtml) {
            var month = new Array(12)
            month[0] = "Jan"
            month[1] = "Feb"
            month[2] = "Mar"
            month[3] = "Apr"
            month[4] = "May"
            month[5] = "Jun"
            month[6] = "Jul"
            month[7] = "Aug"
            month[8] = "Sep"
            month[9] = "Oct"
            month[10] = "Nov"
            month[11] = "Dec"

            if (!blnWriteHtml) {
                //return processDate.toString();
                return intDay + ' ' + month[intMonth - 1] + ' ' + intYear;
            } else {
                document.write(intDay + ' ' + month[intMonth - 1] + ' ' + intYear);
            }
        }

        //Able the option buttons
        function jsCheckOptionButton(optUncheckField) {

            if (optUncheckField != undefined) {
                for (i = 0; i < optUncheckField.length; i++) {
                    if (optUncheckField[i].disabled == true) { // if a button in group is checked,
                        optUncheckField[i].disabled = false;   // uncheck it or able it
                    }
                }
            }
            //Enabled NCF Required Flag when capturing mode equal to image replacement.
            //var optUncheckField1 = document.frmCapturingMode.optNCFRequired;
            //if (optUncheckField1 != undefined) {
            //    for (i = 0; i < optUncheckField1.length; i++) {
            //        if (optUncheckField1[i].disabled == true) { // if a button in group is checked,
            //            optUncheckField1[i].disabled = false;   // check it or disable it
            //        }
            //    }
            //}  
        }

        //Dynamically change the checktype based on the capture mode
        function jsGetCheckType() {
            var objTdCapType = document.getElementById('tdChequeType');

            var intRetModeResult;

            var captureMode = document.getElementsByName('optCapturingMode');
            var selectedMode = '';
            for (var x = 0; x < captureMode.length; x++) {
                if (captureMode[x].checked) {
                    selectedMode = captureMode[x].value;
                }
            }

            // post dated cheque option
            var optNonPostDated = document.getElementById('radChequeStatus_0');
            var optPostDated = document.getElementById('radChequeStatus_1');

            if ((selectedMode == 'CO' && '<%=strPdcCaptureMode%>' != '2') || (selectedMode == 'CP' && '<%=strPdcCaptureMode%>' == '2') || selectedMode == 'IR') {
                optPostDated.disabled = false;
            } else {
                optNonPostDated.checked = true;
                optPostDated.checked = false;
                optPostDated.disabled = true;
            }

            jsInsertOptCell2(objTdCapType, 'ChequeType', 'document.getElementById("sLockBoxSelect")', selectedMode);
        }

        //Delete item per UIC
        function jsDeleteDataXMLFilePerUIC() {
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject");

                var hSelectUIC;
                var arrUIC = [];
                var _deleteAll = false;

                hSelectUIC = document.getElementById("hSelectedUIC");
                arrUIC = hSelectUIC.Value.split("|");

                if (arrUIC.length >= 1) {

                    if (objFS.FileExists(strFilePath)) {

                        var f = objFS.GetFile(strFilePath);
                        var size = f.size;

                        if (size) {

                            var objFile = objFS.OpenTextFile(strFilePath)
                            var outputDoc = objFile.readAll();
                            objFile.close()

                            var parser = new DOMParser();
                            var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                            parentNode = xmlDoc.getElementsByTagName("datas");
                            var xmlNode = xmlDoc.getElementsByTagName("data");
                            var totalRec = xmlDoc.getElementsByTagName("total_rec")[0].textContent;

                            if (totalRec == arrUIC.length) {
                                jsDeleteDataXMLFile();
                                totalChecks = 0;
                                totalDp = 0;
                                //("All cheques has been removed");
                                //$("#sNotice").text("All cheques has been removed");
                                //$("#divNotice").show();
                                removeCtr = 1;
                                jsRefreshPage();
                                return;
                            }
                            //else {
                            //    alert("Please select cheque to remove");
                            //}

                            for (var x = 0; x < arrUIC.length; x++) {
                                for (var i = 0; i < parentNode[0].childNodes.length; i++) {
                                    if (parentNode[0].childNodes[i].childNodes[0].textContent == arrUIC[x]) {

                                        var oldnode = xmlDoc.getElementsByTagName("data")[i];

                                        xmlDoc.getElementsByTagName("datas")[0].removeChild(oldnode);


                                        totalRec = xmlDoc.getElementsByTagName("total_rec")[0].textContent;
                                        var finalCount = 0;
                                        if (jsTrim(totalRec) != '') {
                                            finalCount = parseInt(totalRec) - 1;

                                            xmlDoc.getElementsByTagName("total_rec")[0].textContent = finalCount;
                                        }

                                        // delete relate check image
                                        var strImageName = strFolderName + "\\" + arrUIC[x];

                                        if (objFS.FileExists(strImageName + "GF.JPG")) {
                                            objFS.DeleteFile(strImageName + "GF.JPG", true);
                                        }
                                        if (objFS.FileExists(strImageName + "GB.JPG")) {
                                            objFS.DeleteFile(strImageName + "GB.JPG", true);
                                        }
                                        if (objFS.FileExists(strImageName + "GU.JPG")) {
                                            objFS.DeleteFile(strImageName + "GU.JPG", true);

                                        }
                                        if (objFS.FileExists(strImageName + "BF.TIF")) {
                                            objFS.DeleteFile(strImageName + "BF.TIF", true);
                                        }
                                        if (objFS.FileExists(strImageName + "BB.TIF")) {
                                            objFS.DeleteFile(strImageName + "BB.TIF", true);
                                        }
                                        if (objFS.FileExists(strImageName + "GU.TIF")) {
                                            objFS.DeleteFile(strImageName + "GU.TIF", true);
                                        }

                                        removeCtr = 1;
                                        //$("#sNotice").text("The selected cheque(s) has been removed");
                                        //$("#divNotice").show();
                                    }
                                }

                            }
                        }
                    }

                    if (_deleteAll) {
                        if (objFS.FileExists("data.xml")) {
                            objFS.DeleteFile("data.xml", true);
                            jsRefreshPage();
                        }
                    }
                    else {
                        var serializer = new XMLSerializer();
                        var xmlstring = serializer.serializeToString(xmlDoc);

                        var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                        objFileForWrite.write(xmlstring);
                        objFileForWrite.close();

                        var objXMLField = $("#xmlFields");
                        var objTblFooter = document.getElementById("tblFooter");
                        var objTblBody = document.getElementById("tblBody");

                        jsLoadXMLDoc()                                          //read data xml document at client side
                        jsResetRowTemplate()
                        jsBindLineItems(objXMLField, objTblFooter, objTblBody)
                        jsRefreshPage();
                    }


                    return true;
                }
                else {
                    //("No cheque selected.");
                    $("#sErrorMsg").text("No cheque selected.");
                    $("#divErrorMsg").show();
                    return false;
                }
            }
            catch (e) {
                //("Fail to load local file : " + e);
                $("#sErrorMsg").text("Fail to load local file : " + e);
                $("#divErrorMsg").show();
                jsRefreshPage();
            }
        }

        //Delete the data.xml file into path
        function jsDeleteDataXMLFile() {

            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
            }
            catch (e) {
                //("msgStrActiveXControlError" + e);
                $("#sErrorMsg").text("msgStrActiveXControlError" + e);
                $("#divErrorMsg").show();
                jsRefreshPage();
            }
            var arrayFolder = strFolderName.split("\\")
            var blnFolderExists
            var blnDriveExists
            var blnFileExists
            var strFolderName1
            var strFolderName2
            strFolderName2 = strFolderName.substring(strFolderName.length - 5)
            if (strFolderName2 == "spool") {
                strFolderName1 = strFolderName
            } else {
                strFolderName1 = strFolderName.substring(0, strFolderName.length - 1)
            }
            blnFileExists = objFS.folderExists(strFolderName1);
            if (blnFileExists == true) {
                objFS.DeleteFolder(strFolderName1, true);

            }

            //jsRefreshPage(); //20180921
        }

        //Get the count of second s of micr to check the bankcode for CICS and NON CICS
        function jsGetPosition(str, m, i) {
            return str.split(m, i).join(m).length;
        }

        //Complete or end the batch
        function jsCompleteBatch(intQuestionType) {
            var strQuery
            var blnConfirm

            // check IQA 
            var checkRes = jsXMLEndBatchValidation();

            if (jsTrim(checkRes) != '') {
                //(checkRes);
                $("#sErrorMsg").text(checkRes);
                $("#divErrorMsg").show();
                return false;
            }
            switch (intQuestionType) {
                case 1:
                    {
                        blnConfirm = true;

                        if (document.getElementById("hSelectedCapMode").value != 'BN') {
                            document.getElementById("hPromptMaxMessage").value = 1;
                        }
                        document.getElementById("hRefreshPage").value = "N";
                        break;
                    }
                case 2:
                    {
                        blnConfirm = true;
                        document.getElementById("hRefreshPage").value = "Y";
                        break;
                    }
                case 3:
                    {
                        blnConfirm = true;

                        if (document.getElementById("hSelectedCapMode").value != 'BN') {
                            document.getElementById("hPromptMaxMessage").value = 2;
                        }
                        document.getElementById("hRefreshPage").value = "N";
                        break;
                    }
                case 4:
                    {
                        blnConfirm = true;
                        document.getElementById("hRefreshPage").value = "Y";
                        //jsRefreshPage();
                        break;
                    }
                default:
                    {
                        blnConfirm = false;
                        break;
                    }
            }

            if (blnConfirm) {  //if "OK", complete current batch 
                var strGetResultvalue;
                // Update Direct Image Replacement Page
                //if (document.getElementById("tblDIRBody") != undefined) {

                //    jsLoadXMLDoc();
                //    var blnUpdateResult;
                //    blnUpdateResult = false;
                //    blnUpdateResult = jsUpdateExceptionItem(document.getElementById("tblDIRBody"));

                //    if (blnUpdateResult == true) {
                //        document.getElementById("hRefreshPage").value = "N";
                //    }
                //    else {
                //        // If fail to update,
                //        jsAlert(jsRetrieveMessage("msgStrDataXMLMissing"));
                //        document.getElementById("hRefreshPage").value = "Y";
                //    }
                //}

                strGetResultvalue = jsCheckEndBatchFolder();

                //if (document.getElementById("hRefreshPage").value == "N") {
                    //If Complete Batch Successful, refresh the page and table
                    if (strGetResultvalue == true) {
                        jsRefreshPage();
                   //}
                }
                else {
                    document.getElementById("btnEndBatch").disabled = false;
                    document.getElementById("btnRemove").disabled = false;
                    document.getElementById("btnRemoveAll").disabled = false;
                    document.getElementById("btnIQA").disabled = false;
                    document.getElementById("btnRescan").disabled = true;
                    //else will return to main page
                }
            }
            else {
                document.getElementById("btnEndBatch").disabled = false;
                document.getElementById("btnRemove").disabled = false;
                document.fgetElementById("btnRemoveAll").disabled = false;
                document.getElementById("btnIQA").disabled = false;
                document.getElementById("btnRescan").disabled = true;
                //else will return to main page
            }
        }

        //End batch validation
        function jsXMLEndBatchValidation() {

            var exceptionMsg = '';
            var checkResult = 0;
            var foundDepositSlip = 0;
            var foundDuplicatedMICR = 0;
            var DuplicateMICR = 0;
            var FailMICR = 0;
            var firstDepositSlip = 0;
            var invalidCheck = 0;
            var strSelectedChequeType = 0;
            var OTCtype = 0;
            var validMicr = 0;
            var multiDepositSlip = 0;
            var counter;
            var SingleSlip = 0;
            var SlipCounter = 0;
            var RegularChqOTC = 0;
            var HouseChq = 0;
            try {

                SingleSlip = document.getElementById("hSingleSlip").value;
                strSelectedChequeType = document.getElementById("hChqType").value;
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
                var i
                if (objFS.FileExists(strFilePath)) {
                    var f = objFS.GetFile(strFilePath);
                    var size = f.size;
                    if (size) {
                        var objFile = objFS.OpenTextFile(strFilePath)
                        var outputDoc = objFile.readAll();
                        objFile.close()

                        var parser = new DOMParser();
                        var xmlDoc = parser.parseFromString(outputDoc, "text/xml");
                        var nodeList = xmlDoc.getElementsByTagName("data");

                        var strDataXmlUIC;

                        var CheckType
                        var CheckLastItem

                        CheckLastItem = ""
                        CheckType = ""
                        counter = ""
                        OTCtype = ""
                        DuplicateMICR = ""
                        HouseChq = ""

                        if (nodeList.length > 0) {

                            for (i = 0; i < nodeList.length; i++) {

                                counter = counter + xmlDoc.getElementsByTagName("item_type")[i].textContent;
                                CheckType = CheckType + xmlDoc.getElementsByTagName("item_type")[i].textContent;
                                OTCtype = OTCtype + xmlDoc.getElementsByTagName("checktype")[i].textContent;
                                DuplicateMICR = DuplicateMICR + xmlDoc.getElementsByTagName("valid_micr")[i].textContent;
                                FailMICR = FailMICR + xmlDoc.getElementsByTagName("valid_micr")[i].textContent;
                                HouseChq = HouseChq + xmlDoc.getElementsByTagName("checktype")[i].textContent;

                                if (xmlDoc.getElementsByTagName("iqa")[i].textContent.substring(0, 1) != '1') {
                                    checkResult = 1;
                                }
                                if (xmlDoc.getElementsByTagName("item_type")[i].textContent == 'P') {
                                    foundDepositSlip = 1;
                                    SlipCounter = SlipCounter + 1;

                                    if (i == 0) {
                                        firstDepositSlip = 1;
                                    }
                                }

                                if (xmlDoc.getElementsByTagName("item_type")[i].textContent == 'F') {
                                    invalidCheck = 1;
                                }
                                if (xmlDoc.getElementsByTagName("item_type")[i].textContent == 'D') {
                                    foundDuplicatedMICR = 1;
                                }
                                if (xmlDoc.getElementsByTagName("valid_micr")[i].textContent == "D") {
                                    DuplicateMICR = 1;
                                }
                                if (xmlDoc.getElementsByTagName("valid_micr")[i].textContent == "F") {
                                    invalidCheck = 1;
                                }

                                //alert(xmlDoc.getElementsByTagName("valid_micr")[i].textContent);
                                //if ((strSelectedChequeType == '13') && (xmlDoc.getElementsByTagName("item_type")[i].textContent != 'P')) {
                                //    if (xmlDoc.getElementsByTagName("checktype")[i].textContent == 'R') {
                                //        RegularChqOTC = 1;
                                //    }
                                //}
                                //if ((strSelectedChequeType == '13') && (xmlDoc.getElementsByTagName("item_type")[i].textContent == "Pass")) {
                                //    if (xmlDoc.getElementsByTagName("checktype")[i].textContent == 'H') {
                                //        HouseChq = 1;
                                //    }
                                //}
                                //alert(HouseChq);
                                if (xmlDoc.getElementsByTagName("checktype")[i].textContent == 'F') {
                                    RegularChqOTC = 1;
                                }
                                //alert("aa");
                                //alert(document.getElementById("hSelectedCapMode").value);
                            }
                        }


                        if (counter.indexOf('PP') >= 0) {
                            multiDepositSlip = 1;
                        }

                        // debugger;
                        var serializer = new XMLSerializer();
                        var xmlstring = serializer.serializeToString(xmlDoc);

                        CheckLastItem = CheckType.substring(CheckType.length - 1, CheckType.length);
                        var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                        objFileForWrite.write(xmlstring);
                        objFileForWrite.close();

                        if (checkResult == 0 && document.getElementById("hSelectedCapMode").value == "CP" && foundDepositSlip == 0) {
                            checkResult = 2;
                        }

                        if (checkResult == 0 && document.getElementById("hSelectedCapMode").value == "CP" && foundDepositSlip == 1 && firstDepositSlip == 0) {
                            checkResult = 4;
                        }
                        else if (document.getElementById("hSelectedCapMode").value == "CP" && CheckLastItem == 'P') {
                            checkResult = 7;
                        }


                        if (checkResult == 0 && document.getElementById("hSelectedCapMode").value == "CP" && multiDepositSlip == 1) {
                            checkResult = 8;
                        }


                        if (checkResult == 0 && document.getElementById("hSelectedCapMode").value == "CO" && invalidCheck == 1) {
                            checkResult = 3;
                        }

                        if (checkResult == 0 && foundDuplicatedMICR == 1) {
                            checkResult = 5;
                        }

                        if (checkResult == 0 && DuplicateMICR == 1) {
                            checkResult = 5;
                        }
                        if (document.getElementById("hSelectedCapMode").value == "CS" && invalidCheck == 1) {
                            checkResult = 6;
                        }


                        if (document.getElementById("hSelectedCapMode").value == "CP" && SlipCounter > 1 && SingleSlip == 1) {
                            checkResult = 9;
                        }
                        if ((document.getElementById("hSelectedCapMode").value == "CP" || document.getElementById("hSelectedCapMode").value == "CO") && RegularChqOTC == 1) {
                            checkResult = 10;
                        }
                        if ((document.getElementById("hSelectedCapMode").value == "CO") && RegularChqOTC == 1) {
                            checkResult = 11;
                        }

                        if ((parseInt($("#hTotalCounter").val()) > parseInt($("#hCompletedSeqNo").val()))) {

                            checkResult = 12;
                        }

                        //alert(document.getElementById("hSelectedCapMode").value);
                        //alert(HouseChq);

                        switch (checkResult) {
                            case 1:
                                return "Fail IQA found. System will not allow to End Batch.";
                                break;
                            case 2:
                                return "Deposit Slip not found. System will not allow to End Batch.";
                                break;
                            case 3:
                                return "No MICR found in some of the cheques. Kindly remove those items to End Batch.";
                                break;
                            case 4:
                                return "Deposit Slip should be the first item of the batch. System will not allow to End Batch.";
                                break;
                            case 5:
                                return "Duplicate MICR found. System will not allow to End Batch.";
                                break;
                            case 6:
                                return "MICR detected. Kindly scan correct SCR / Charge Slip.";
                                break;
                            case 7:
                                return "Deposit slip without check is found. System will not allow to End Batch.";
                                break;

                            case 8:
                                return "Two consecutive payslip has been detected. End Batch not allowed.";
                                break;
                            case 9:
                                return "More then 1 payslip has been detected. End Batch not allowed.";
                                break;

                            case 10:
                                return "Regular cheque has been detected. Kindly remove cheque with R type.";
                                break;
                            case 11:
                                //return "House cheque has been detected. Kindly remove cheque with H type.";
                                return "Failed Bank Test has been detected. Kindly remove cheque with F Bank Test.";
                                break;

                            case 12:
                                //return "House cheque has been detected. Kindly remove cheque with H type.";
                                return "Maximum cheque per batch has been exceeded. Kindly remove some cheque(s).";
                                break;

                            default:
                                return "";
                        }
                    }
                }

            }
            catch (e) {
                //alert("jsError5");
                exceptionMsg = "(jsXMLEndBatchValidation) Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                $("#sErrorMsg").text(exceptionMsg);
                $("#divErrorMsg").show();
                return "Error";
            }
        }

        //Exit scanner
        function jsExitScanner() {
            scnControl.ExitScanner();
        }

        //Rename tiff image and data.xml into folder called "clearingbranchid(7digit)_scannerid(3digit)batchno(4digit) E.g. 0014011_0010001"
        function jsCheckEndBatchFolder() {
            //debugger
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
            }
            catch (e) {
                //("Fail to load local file");
                $("#sErrorMsg").text("Fail to load local file");
                $("#divErrorMsg").show();
                jsRefreshPage();
            }
            var blnFolderExists
            var blnDriveExists
            var blnFileExists
            var blnReturnvalue
            var strCheckStatusType

            blnReturnvalue = "false"

            blnFileExists = objFS.FileExists(strFilePath)
            if (blnFileExists == false) {
                //("There are no batch file!");
                $("#sErrorMsg").text("There are no batch file.");
                $("#divErrorMsg").show();
            }

            var outputDoc = $("#xmlFields").html();
            var parser = new DOMParser();
            var objXml = parser.parseFromString(outputDoc, "text/xml");


            strOutput = outputDoc;
            if (strOutput.indexOf("<scanner>") != -1) {

                var intScannerId = objXml.getElementsByTagName("scanner_id")[0].textContent;

                var intBatchId;
                if ((document.getElementById("sBatchNo").value != null) && (document.getElementById("sBatchNo").value != '')
                    && (document.getElementById("sBatchNo").value != undefined)) {
                    intBatchId = document.getElementById("sBatchNo").innerText;
                }
                else {
                    intBatchId = objXml.getElementsByTagName("batch_id")[0].textContent;
                }

                //Add _0 or _1 in the folder name to check the PDC and Normal Check.
                if (document.getElementById("hChequeStatus").value == 'N') {
                    strCheckStatusType = "_0" //Normal Check
                } else {
                    strCheckStatusType = "_1" //PDC Check
                }

                var intClearingBranch = jsTrim(objXml.getElementsByTagName("clearing_branch")[0].textContent);

                //var strNewFolderName = jsRight("000000" + strClientProcessDate, 8) + "_" +
                //    jsRight("0000" + intClearingBranch, 9) + "_" +
                //    jsRight("0000" + intScannerId, 5) + jsRight("0000" + intBatchId, 4) + strCheckStatusType;

                var strNewFolderName = jsRight("000000" + strClientProcessDate, 8) + "_" +
                    jsRight("0000" + intClearingBranch, 9) + "_" +
                    jsRight("0000" + intScannerId, 5) + jsRight("0000" + intBatchId, 4);

                //alert(jsRight("000000" + strClientProcessDate, 8));
                //alert(jsRight("0000" + intClearingBranch, 9));
                //alert(jsRight("0000" + intScannerId, 5));
                //alert(jsRight("0000" + intBatchId, 4));
                //alert(strNewFolderName);

                var intSeqNo;
                var intScannerId;
                var intTaskId;
                var intUserId;
                var objFolderName;
                var blnUpdateUICInfo;
                var arrReturnUIC;
                var xmlNode;

                intUserId = g_intUserId;
                //Update UIC Batch No + 1 and Reset Sequence No to 1 in database
                intBatchId = parseInt(intBatchId, 10) + 1;
                intSeqNo = parseInt(document.getElementById("sSeqNo").innerText);//+ 1;
                //alert(intSeqNo);
                intScannerId = parseInt(document.getElementById('sScannerId').innerText);
                intTaskId = document.getElementById('hTaskId').value;
                // alert("1");
                var WSClearingBranch = $("#hWorkstationBranchId").val();
                blnUpdateUICInfo = ajaxUpdateUICInfo(intScannerId, intBatchId, intSeqNo, intUserId, WSClearingBranch);

                // Update the total counter value
                $("#hTotalCounter").val(0);


                if (blnUpdateUICInfo == true) {
                    blnFolderExists = objFS.FolderExists(strFolderName);
                    if (blnFolderExists == true) {
                        objFolderName = objFS.GetFolder(strFolderName);
                        //Check new folder whether exist
                        if (objFolderName.Name != strNewFolderName) {
                            //Change the data.xml to "foldername".xml
                            var objChangeFilename = objFS.GetFile(strFilePath);
                            objChangeFilename.Name = strNewFolderName + ".xml";


                            objFolderName.Name = strNewFolderName;
                            jsCreateReadyFile(objFS, strNewFolderName);

                            if (document.getElementById("hAutoCapture").value != 'Y') {
                            }

                            alert("Batch has ended successfully");
                            //$("#sNotice").text("Batch has ended successfully");
                            //$("#divNotice").show();
                            objFS = "";
                            blnReturnvalue = true;
                        }
                    }
                }
                else {
                    //("Batch Item Ended Fail. Please check with your administrator.");
                    $("#sErrorMsg").text("Batch Item Ended Fail. Please check with your administrator.");
                    $("#divErrorMsg").show();
                    blnReturnvalue = false;
                }
            }
            return blnReturnvalue;
        }

        //Create Rdy file when the image and data.xml have been moved
        function jsCreateReadyFile(objFileSystem, strReadyFileName) {

            var strStructureString = jsFolderRestructure(strFolderName);
            var arrFolder = strStructureString.split("\\");
            var strNewFolderName = "";
            var blnFolderExists;

            for (var i = 0; i < arrFolder.length - 2; i++) {
                //Reset path
                strNewFolderName = strNewFolderName + arrFolder[i] + "\\";
            }
            blnFolderExists = objFileSystem.FolderExists(strNewFolderName)
            if (blnFolderExists == true) {
                objFileSystem.CreateTextFile(strNewFolderName + strReadyFileName + ".RDY", true);
            }
        }

        //Folder restructure
        function jsFolderRestructure(strFolder) {
            var arrFolder = strFolder.split("\\")
            var strRestructureString = ""

            for (var i = 0; i < arrFolder.length; i++) {
                if (arrFolder[i] == "") {
                    break;
                }
                else {
                    //Reset path
                    strRestructureString = strRestructureString + arrFolder[i] + "\\";
                }
            }
            return strRestructureString
        }

        //Refresh page
        function jsRefreshPage() {
            jsExitScanner();
            document.getElementById("btnRefresh").click();

        }

        //Reload xml
        function jsReinsertXML(intCounter) {
            //update data.xml
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
            }
            catch (e) {
                //("Fail to load local file");
                $("#sErrorMsg").text("Fail to load local file");
                $("#divErrorMsg").show();
            }
            var i
            var strCapMode
            strCapMode = document.getElementById("hSelectedCapMode").value;
            if (objFS.FileExists(strFilePath)) {

                var f = objFS.GetFile(strFilePath);
                var size = f.size;

                if (size) {

                    var objFile = objFS.OpenTextFile(strFilePath)
                    var outputDoc = objFile.readAll();
                    objFile.close()

                    var parser = new DOMParser();
                    var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                    var nodeList = xmlDoc.getElementsByTagName("data");
                    var strSelectedUIC = jsTrim(document.getElementById("hRowClickUIC").value);
                    var strDataXmlUIC;
                    var strMICR = jsTrim(document.getElementById("hMICR").value);
                    var arrMICR;
                    var blnCheckMICR;

                    //Check MICR whether is batch ticket
                    arrMICR = jsArrGetSplitMICR(strMICR);

                    if (nodeList.length > 0) {
                        for (i = 0; i < nodeList.length; i++) {
                            strDataXmlUIC = jsTrim(xmlDoc.getElementsByTagName("uic")[i].textContent);

                            if (strSelectedUIC == strDataXmlUIC) {
                                //if the MICR remain error, ask user to repair the MICR.
                                var arrMICR;
                                var strChangedMICR;
                                //Split the MICR value into array list
                                //0: Check Digit, 1: Serial Number,
                                //2: Bank Code, 3: Branch Code,
                                //4: Account Number, 5: Transaction code
                                //6: Amount 7:Bank Type
                                arrMICR = jsArrGetSplitMICR(strMICR);

                                //if the MICR is invalid, it will appear repair message until MICR is correct
                                if ((strMICR == null) || (strMICR == '')) {
                                    //strChangedMICR = "na" + arrMICR[0] + "s" + arrMICR[1] + "s" + arrMICR[2] + "e" + arrMICR[3] + "a" + arrMICR[4] + "s" + arrMICR[5] + "x" + arrMICR[6] + "xm";
                                    strChangedMICR = "na" + arrMICR[0] + "s" + arrMICR[1] + "s" + arrMICR[2] + "e" + arrMICR[3] + "a" + arrMICR[5] + "s" + arrMICR[4] + "xm";
                                }
                                else {
                                    // strChangedMICR = "na" + arrMICR[0] + "s" + arrMICR[1] + "s" + arrMICR[2] + "e" + arrMICR[3] + "a" + arrMICR[4] + "s" + arrMICR[5] + "x" + arrMICR[6] + "xm";
                                    strChangedMICR = "na" + arrMICR[0] + "s" + arrMICR[1] + "s" + arrMICR[2] + "e" + arrMICR[3] + "a" + arrMICR[5] + "s" + arrMICR[4] + "xm";
                                }

                                //Check on valid MICR
                                if ((arrMICR != '') && (arrMICR != null)) {
                                    xmlDoc.getElementsByTagName("micr")[i].textContent = strChangedMICR;

                                    //Check correct MICR and assign as C or P or B
                                    if (arrMICR.length == 9 && arrMICR[1] != "" && arrMICR[2] != "" && arrMICR[3] != "" && arrMICR[4] != "" && arrMICR[8] != "") {
                                        if (parseInt(arrMICR[5], 10) >= 80) {
                                            // Re-insert for Batch Ticket
                                            //xmlDoc.getElementsByTagName("item_type")[i].textContent = "B";//nodeList[i].selectSingleNode('item_type').text = 'B';
                                            //var strConvertChqType = jsConvertTCChqTypeId(arrMICR[5]);
                                            //var strChqType = xmlDoc.selectSingleNode("//root/scanner/cheque_type").text

                                            //if (intCounter == 1) {
                                            //    // Begin Mod: Liew Yu Joe 2007/11/17
                                            //    xmlDoc.selectSingleNode("//root/scanner/capturing_branch").text = arrMICR[2] + arrMICR[3];
                                            //    xmlDoc.selectSingleNode("//root/scanner/clearing_branch").text = arrMICR[8];
                                            //    xmlDoc.selectSingleNode("//root/scanner/bank_type").text = arrMICR[7];
                                            //    // End Mod: Liew Yu Joe 2007/11/17
                                            //}

                                            //if (jsTrim(strChqType) == '') {
                                            //    xmlDoc.selectSingleNode("//root/scanner/cheque_type").text = strConvertChqType;
                                            //    document.getElementById("hChqType").value = xmlDoc.selectSingleNode("//root/scanner/cheque_type").text;
                                            //}
                                            //nodeList[i].selectSingleNode('payee_accounts/total_amount').text = arrMICR[6];
                                            //nodeList[i].selectSingleNode('payee_accounts/account/amount').text = "";
                                        }
                                        else {

                                            // Re-insert for Cheque
                                            xmlDoc.getElementsByTagName("item_type")[i].textContent = "C";//nodeList[i].selectSingleNode('item_type').text = 'C';
                                            if (document.getElementById("hSelectedCapMode").value == "BN") {
                                                xmlDoc.getElementsByTagName("total_amount")[i].textContent = "";//nodeList[i].selectSingleNode('payee_accounts/total_amount').text = "";
                                                xmlDoc.getElementsByTagName("amount")[i].textContent = arrMICR[6];//nodeList[i].selectSingleNode('payee_accounts/account/amount').text = arrMICR[6];
                                            }
                                            else {
                                                xmlDoc.getElementsByTagName("total_amount")[i].textContent = arrMICR[6];//nodeList[i].selectSingleNode('payee_accounts/total_amount').text = arrMICR[6];
                                                xmlDoc.getElementsByTagName("amount")[i].textContent = arrMICR[6];//nodeList[i].selectSingleNode('payee_accounts/account/amount').text = arrMICR[6];
                                            }
                                        }
                                    }
                                    else {
                                        //alert("b");
                                        // Re-insert for Payslip
                                        if (strCapMode == "CO") {
                                            xmlDoc.getElementsByTagName("item_type")[i].textContent = 'F';

                                        }

                                        //Re-insert for Charge Slip
                                        else if (strCapMode == "CS") {
                                            xmlDoc.getElementsByTagName("item_type")[i].textContent = 'C';
                                        }

                                        else {
                                            xmlDoc.getElementsByTagName("item_type")[i].textContent = 'P';
                                        }

                                    }

                                    // alert(strChangedMICR.length);
                                    //alert(xmlDoc.getElementsByTagName("item_type")[i].textContent = 'C');
                                    // if (strChangedMICR.search(strChangedMICR) > -1 && jsTrim(xmlDoc.getElementsByTagName("item_type")[i].textContent) == 'C') {
                                    //     //xmlDoc.getElementsByTagName("item_type")[i].textContent = 'D';
                                    //     alert("1");
                                    //     xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "D";
                                    // }
                                    // else {
                                    // alert("2");

                                    if (xmlDoc.getElementsByTagName("item_type")[i].textContent = 'C') {
                                        if (strChangedMICR.length <= 9) {
                                            //alert("length less than 4");
                                            xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "F";
                                        }
                                        else if (strChangedMICR.indexOf('?') > -1) {
                                            //if (intMICR.search(strSearchMICRPattern) > -1) {
                                            //alert("got ?");
                                            xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "F";
                                        }
                                        else {
                                            //alert("valid MICR");
                                            xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "P";
                                        }
                                        //}
                                    }


                                    //check if Bank Test is F then set F when rescan
                                    var ChequeFlag = jsGetBk(strMICR).substring(1, 4);
                                    var sUserBankCode = $("#hUserBankCode").val();



                                    if (ChequeFlag != "") {
                                        if (ChequeFlag == sUserBankCode) {
                                            xmlDoc.getElementsByTagName("checktype")[i].textContent = "F";
                                        }

                                        else if (ajaxCheckBank(ChequeFlag) == true) {
                                            xmlDoc.getElementsByTagName("checktype")[i].textContent = "F";
                                        }

                                        else {
                                            xmlDoc.getElementsByTagName("checktype")[i].textContent = "P";
                                        }
                                    }

                                    var serializer = new XMLSerializer();
                                    var xmlstring = serializer.serializeToString(xmlDoc);

                                    var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                                    objFileForWrite.write(xmlstring);
                                    objFileForWrite.close();


                                    if (document.getElementById("hSelectedCapMode").value == "BN") {
                                        if (i >= 1) {
                                            var blnCheckNewBatch = true;
                                            document.getElementById("scnControl").pstrMICR = 0;
                                            blnCheckNewBatch = jsCheckNewBatch(strChangedMICR, strSelectedUIC);
                                            if (blnCheckNewBatch == true) {
                                                return true;
                                            }
                                        }
                                    }

                                    var objXMLField = $("#xmlFields");
                                    var objTblFooter = document.getElementById('tblFooter')
                                    var objTblBody = document.getElementById('tblBody')

                                    //Load XML Document   
                                    jsLoadXMLDoc();    //read data xml document at client side
                                    //Reset table into empty table
                                    jsResetRowTemplate();

                                    //Insert data into row depends on the Mode BN: Bank Negara Malaysia Mode
                                    if (document.getElementById("hSelectedCapMode").value == "BN") {
                                        //jsBindLineItems_BNM(objXMLField, objTblFooter, objTblBody);
                                    }
                                    else {
                                        jsBindLineItems(objXMLField, objTblFooter, objTblBody);
                                    }
                                    return true;
                                }
                                else {
                                    return false;
                                }
                            }
                        }
                    }
                    else {
                        return false;
                    }
                }
            }
            return false;
        }

        //Delete sequence number by 1??
        function jsDeleteUICInfoSeqNo(intScannerId, intUserId, intTaskId) {
            //var url
            //var strResponse
            ////BeginMod: Joe 8 Aug 2007    
            //url = "xmlhttpDeleteUICInfoSequenceNo.aspx?scannerId=" + intScannerId + "&userId=" + intUserId + "&taskId=" + intTaskId
            ////EndMod: Joe 8 Aug 2007
            //try {
            //    var ohttp = new ActiveXObject("Microsoft.XmlHttp")
            //}
            //catch (e) {
            //    jsAlert(jsRetrieveMessage("msgStrActiveXControlError"))
            //    //jsAlert("Your browser is not IE 5 or higher");
            //    jsReloadPage();
            //}
            //ohttp.open("POST", url, false)
            //ohttp.send(null)

            //strResponse = ohttp.responseText
            //try {
            //    var objXML = new ActiveXObject("Microsoft.XMLDOM")
            //}
            //catch (e) {
            //    jsAlert(jsRetrieveMessage("msgStrActiveXControlError"))
            //    //jsAlert("Fail to load xml file");
            //    jsReloadPage();
            //}
            //objXML.async = "false"
            //objXML.loadXML(strResponse)

            //var xmlNode = objXML.selectNodes("//root/records/record");
            //if (xmlNode.length == 0) {
            //    return false;
            //}
            //else {
            //    document.getElementById("lblBatchNo").innerText = xmlNode[0].selectSingleNode("batchNo").text;
            //    document.getElementById("lblSeqNo").innerText = xmlNode[0].selectSingleNode("seqNo").text;
            //    return true;
            //}
        }

        // Function convert between Transaction code and cheque type
        function jsConvertTCChqTypeId(strChequeTypeId) {
            var arrGetChqType;
            var strReturnConvertCode;
            arrGetChqType = jsGetCaptureData('ChequeType');

            for (var i = 0; i < arrGetChqType.length; i++) {
                if (jsTrim(arrGetChqType[i][3]) == jsTrim(strChequeTypeId)) {
                    strReturnConvertCode = arrGetChqType[i][2];
                }
            }
            return strReturnConvertCode;
        }

        //Reset the row into empty row
        function jsResetRowTemplate() {

            var objTblBody = document.getElementById('tblBody');
            var objTblBodyLen = objTblBody.rows.length;

            for (var i = objTblBodyLen - 1; i >= 0; i--) {
                objTblBody.deleteRow(i);
            }
        }

        //Force MICR validation
        function jsForceMICRPass() {

            var exceptionMsg = '';
            var MICRRes;
            try {
                var hSelectUIC;
                var arrUIC = [];
                hSelectUIC = document.getElementById("hSelectedUIC");

                if (hSelectUIC.Value != "") {
                    arrUIC = hSelectUIC.Value.split("|");
                    if (arrUIC.length > 0) {
                        for (var x = 0; x < arrUIC.length; x++) {
                            jsXMLInsertMICR(arrUIC[x]);
                        }
                    }
                }
                else {
                    //("Please select item");
                    //$("#sErrorMsg").text("Please select item");
                    //$("#divErrorMsg").show();
                    return;
                }

                var objXMLField = $("#xmlFields");
                var objTblFooter = document.getElementById('tblFooter')
                var objTblBody = document.getElementById('tblBody')

                //Load XML Document   
                jsLoadXMLDoc();
                //read data xml document at client side
                //Reset table into empty table
                jsResetRowTemplate();
                //Insert data into row depends on the Mode BN: Bank Negara Malaysia Mode
                if (document.getElementById("hSelectedCapMode").value == "BN") {
                    jsBindLineItems_BNM(objXMLField, objTblFooter, objTblBody);
                }
                else {
                    jsBindLineItems(objXMLField, objTblFooter, objTblBody);
                }
                jsMICRCount();
                //jsIQACount();
            }



            catch (e) {
                exceptionMsg = "Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                //$("#sErrorMsg").text(exceptionMsg);
                //$("#divErrorMsg").show();
            }
        }

        //Force IQA
        function jsForceIQAPass() {

            var exceptionMsg = '';
            try {
                var cbs = document.getElementsByTagName('input');
                var cbx = [];
                //var cbs = document.getElementsByName('CheckBoxControl') 


                //var cbs = document.getElementsByTagName('input').type == 'checkbox';
                for (var i = 0; i < cbs.length; i++) {


                    if (cbs[i].type == 'checkbox' && cbs[i].name.substring(0, 3) == 'cb_' && cbs[i].checked == true) {
                        cbx.push(cbs[i]);
                    }
                }
                if (cbx.length > 0) {
                    for (var i = 0; i < cbx.length; i++) {
                        var logFullFilePath = document.getElementById("hCapturingPath").value + "\log\\mylog.log";
                        var settingPath = document.getElementById("hCapturingPath").value + "\\";

                        var sChequePath = document.getElementById("hCapturingPath").value + jsFormatProcessDateToDigit(document.getElementById("sProcessingDate").innerText) + "\\spool";
                        var sFrontIMG = cbx[i].name.substring(3, 33) + "BF.TIF";
                        var sBackIMG = cbx[i].name.substring(3, 33) + "BB.TIF";
                        var sGFrontIMG = cbx[i].name.substring(3, 33) + "GF.JPG";
                        var sGBackIMG = cbx[i].name.substring(3, 33) + "GB.JPG";

                        var res = ajaxFBIQA(logFullFilePath, settingPath, sChequePath, sFrontIMG, sBackIMG, sGFrontIMG, sGBackIMG);

                        if (res == "") {
                            var IQARes = "Error";
                        } else {
                            var IQARes = res.replace("|BB@", "").replace("BF@", "").replace("|GB@", "").replace("|GF@", "");
                        }

                        if (jsTrim(IQARes) == "") {
                            jsXMLInsertIQA(cbx[i].name.substring(3, 33), '1');
                            jsEditDataXMLFileForceIQA();
                        } else {
                            var FailDesc = "";
                            var arrIQA = res.split("|");
                            for (var k = 0; k < arrIQA.length; k++) {
                                var result = arrIQA[k].split("@");
                                if (jsTrim(result[1]) != "") {
                                    FailDesc = FailDesc + result[0] + " - " + result[1];
                                }
                            }
                            //alert(FailDesc);
                            jsXMLInsertForceIQA(cbx[i].name.substring(3, 33), FailDesc);
                            jsXMLInsertIQA(cbx[i].name.substring(3, 33), '1' + FailDesc);
                            jsEditDataXMLFileForceIQA();
                        }




                        var objXMLField = $("#xmlFields");
                        var objTblFooter = document.getElementById('tblFooter')
                        var objTblBody = document.getElementById('tblBody')

                        //Load XML Document   
                        jsLoadXMLDoc();
                        //read data xml document at client side
                        //Reset table into empty table
                        jsResetRowTemplate();
                        //Insert data into row depends on the Mode BN: Bank Negara Malaysia Mode
                        if (document.getElementById("hSelectedCapMode").value == "BN") {
                            jsBindLineItems_BNM(objXMLField, objTblFooter, objTblBody);
                        }
                        else {
                            jsBindLineItems(objXMLField, objTblFooter, objTblBody);
                        }


                        //$("#sNotice").text("Failed IQA cheque has successfully forced to pass");
                        //$("#divNotice").show();
                        jsIQACount();
                    }

                }

                //else {
                //    //alert("Please select item");
                //    $("#sErrorMsg").text("Please select item");
                //    $("#divErrorMsg").show();
                //}


            }
            catch (e) {
                exceptionMsg = "Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                // $("#sErrorMsg").text(exceptionMsg);
                // $("#divErrorMsg").show();
            }
        }

        //Insert MICR
        function jsXMLInsertMICR(selectedUIC) {
            // debugger;
            var exceptionMsg = '';
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
                var i;
                if (objFS.FileExists(strFilePath)) {
                    var f = objFS.GetFile(strFilePath);
                    var size = f.size;
                    if (size) {
                        var objFile = objFS.OpenTextFile(strFilePath)
                        var outputDoc = objFile.readAll();
                        objFile.close()


                        //var xmlDoc = new ActiveXObject("MICROSOFT.FreeThreadedXMLDOM");
                        //xmlDoc.async = "false";
                        //xmlDoc.loadXML(outputDoc);

                        var parser = new DOMParser();
                        var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                        var nodeparent = xmlDoc.getElementsByTagName("scanner");
                        var nodeList = xmlDoc.getElementsByTagName("data");

                        var strSelectedUIC = selectedUIC;
                        var strDataXmlUIC;

                        if (nodeList.length > 0) {
                            for (i = 0; i < nodeList.length; i++) {
                                strDataXmlUIC = jsTrim(xmlDoc.getElementsByTagName("uic")[i].textContent);
                                if (strSelectedUIC == strDataXmlUIC) {

                                    //** if the capturing mode is Check Only

                                    if (xmlDoc.getElementsByTagName("capturing_mode")[0].textContent == "CO") {
                                        //** if MICR is FAILED 
                                        //if (xmlDoc.getElementsByTagName("item_type")[i].textContent == "F") {
                                        //    xmlDoc.getElementsByTagName("item_type")[i].textContent = "C";
                                        //    totalChecks = totalChecks + 1;
                                        //    alert("MICR has been forced.");
                                        //} 
                                        if (xmlDoc.getElementsByTagName("valid_micr")[i].textContent == "F" || xmlDoc.getElementsByTagName("valid_micr")[i].textContent == "D") {
                                            xmlDoc.getElementsByTagName("valid_micr")[i].textContent = "P";
                                            totalChecks = totalChecks + 1;
                                            micrForceCtr = 1;


                                            //$("#sNotice").text("MICR has been forced to pass");
                                            //$("#divNotice").show();
                                        }
                                        else {
                                            //("Force MICR for this specimen is not allowed"); // don't change into C;
                                            //$("#sErrorMsg").text("Force MICR for this specimen is not allowed");
                                            //$("#divErrorMsg").show();
                                        }
                                    }

                                    //** if the capturing mode is Check with Deposit Slip
                                    if (xmlDoc.getElementsByTagName("capturing_mode")[0].textContent == "CP") {
                                        //** if MICR is FAILED 
                                        if (xmlDoc.getElementsByTagName("item_type")[i].textContent == "F") {
                                            xmlDoc.getElementsByTagName("item_type")[i].textContent = "C";
                                            totalChecks = totalChecks + 1;
                                            //("MICR has been forced.");
                                            $("#sNotice").text("MICR has been forced.");
                                            $("#divNotice").show();
                                        }
                                        //** if MICR is FAILED and the check has been read as deposit slip
                                        else if (xmlDoc.getElementsByTagName("item_type")[i].textContent == "P") {
                                            // if there is only one P left;
                                            if (((totalDp - 1) < 1) || (i == 0)) {
                                                //("Force MICR for this specimen is not allowed\nThere must be at least 1 deposit slip in this batch");
                                                $("#sErrorMsg").text("Force MICR for this specimen is not allowed\nThere must be at least 1 deposit slip in this batch");
                                                $("#divErrorMsg").show();
                                            }
                                            else {
                                                xmlDoc.getElementsByTagName("item_type")[i].textContent = "C";
                                                totalDp = totalDp - 1;
                                                totalChecks = totalChecks + 1;
                                                //("MICR has been forced.");
                                                $("#sNotice").text("MICR has been forced.");
                                                $("#divNotice").show();
                                            }
                                        }
                                        else {
                                            //("Force MICR for this specimen is not allowed");
                                            $("#sErrorMsg").text("Force MICR for this specimen is not allowed");
                                            $("#divErrorMsg").show();
                                        }
                                    }
                                }

                            }

                        }

                        var serializer = new XMLSerializer();
                        var xmlstring = serializer.serializeToString(xmlDoc);


                        var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                        objFileForWrite.write(xmlstring);
                        objFileForWrite.close();
                    }
                }

            }
            catch (e) {
                exceptionMsg = "(jsXMLInsertItem_Type) Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                //$("#sErrorMsg").text(exceptionMsg);
                //$("#divErrorMsg").show();
            }
        }

        //Replace new image to current old image
        function jsReplaceImage() {
            //var strReplaceImageUIC = jsTrim(document.getElementById("hRowClickUIC").value)
            //var strQuery
            //var blnConfirm
            //var strMICR
            //var blnWriteXml

            ////Ask question when perform image re-capture
            //if (strReplaceImageUIC != '') {
            //    //strQuery = jsRetrieveMessage("msgStrRecapureConfirm")
            //    //strQuery = strQuery + " You will re-scan the cheque. Click 'Ok' to continue.";
            //    //blnConfirm = jsConfirm(strQuery);
            //}
            //else {
            //    //jsAlert(jsRetrieveMessage("msgStrRecapture"))
            //    alert('Please select scanning item in scanning result list for re-capture.');
            //}

            //if (blnConfirm) {

            //    if (vbReCapture()) {
            //        try {
            //            var objFS = new ActiveXObject("Scripting.FileSystemObject")
            //        }
            //        catch (e) {
            //            jsAlert(jsRetrieveMessage("msgStrActiveXControlError"))
            //            //jsAlert("Fail to load local file");
            //        }

            //        if (objFS.FileExists(strFilePath)) {
            //            if ((g_intEjectErrorMICR != null) && (g_intEjectErrorMICR == 1)) {
            //                vbDecreaseUIC();
            //            }

            //            vbScan();
            //        }

            //    }
            //    else {
            //        return false;
            //        //No need to prompt message
            //        //jsAlert(jsRetrieveMessage("msgStrErrorRecapture"))
            //        //jsAlert('Error occurs when re-capture selected item. Please make sure the item is probably placed in scanner feeder.')
            //    }
            //}
            //else {
            //    return true;
            //}
        }

        //Validate MICR to detect if Cheque is wrongly captured
        function jsValidateMICR(strMICR) {
            var arrMICR;

            arrMICR = jsArrGetSplitMICR(strMICR);

            if ((strMICR == null) || (strMICR == '')) {
                return true;
            }
            else if ((strMICR.length < 15) && (strMICR.length >= 1)) {
                return true;
            }

            else if (strMICR.substr(0, 5) == 'n????') {
                return true;
            }
            else if (strMICR.length != 46) {
                return true;
            }
            else {
                return false;
            }
        }

        //Insert IQA 
        function jsXMLInsertIQA(selectedUIC, ResultIQA) {
            var exceptionMsg = '';
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
                var i;
                if (objFS.FileExists(strFilePath)) {
                    var f = objFS.GetFile(strFilePath);
                    var size = f.size;
                    if (size) {
                        var objFile = objFS.OpenTextFile(strFilePath)
                        var outputDoc = objFile.readAll();
                        objFile.close()

                        //var xmlDoc = new ActiveXObject("MICROSOFT.FreeThreadedXMLDOM");
                        //xmlDoc.async = "false";
                        //xmlDoc.loadXML(outputDoc);

                        var parser = new DOMParser();
                        var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                        var nodeList = xmlDoc.getElementsByTagName("data");
                        var strSelectedUIC = selectedUIC;
                        var strDataXmlUIC;

                        //if (nodeList.length > 0) {
                        //    for (i = 0; i < nodeList.length; i++) {
                        //        strDataXmlUIC = jsTrim(xmlDoc.getElementsByTagName("uic")[i].textContent);
                        //        if (strSelectedUIC == strDataXmlUIC) {
                        //            debugger
                        //            xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                        //        }
                        //    }
                        //}

                        if (nodeList.length > 0) {
                            //debugger
                            for (i = 0; i < nodeList.length; i++) {
                                strDataXmlUIC = jsTrim(xmlDoc.getElementsByTagName("uic")[i].textContent);
                                if (strSelectedUIC == strDataXmlUIC) {


                                    //if (xmlDoc.getElementsByTagName("iqa")[i].textContent == "1") {
                                    //    xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                                    //}
                                    //else if (xmlDoc.getElementsByTagName("iqa")[i].textContent == "0") {
                                    //    xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                                    //}
                                    //else {
                                    //    if (xmlDoc.getElementsByTagName("iqa")[i].textContent.length == 1)
                                    //    {
                                    //        xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                                    //    }
                                    //    else
                                    //    {
                                    //        xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                                    //        alert("Failed IQA cheque has successfully forced to pass");
                                    //    }


                                    //}
                                    if (ResultIQA.length == 1) {
                                        if (ResultIQA == "0") {
                                            if (xmlDoc.getElementsByTagName("iqa")[i].textContent == "1" || xmlDoc.getElementsByTagName("iqa")[i].textContent == "0") {
                                                xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                                            }

                                            else {
                                                xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                                                //alert("Failed IQA cheque has successfully forced to pass");
                                                iqaForceCtr = 1;
                                            }

                                        }
                                        else if (ResultIQA == "1") {
                                            if (xmlDoc.getElementsByTagName("iqa")[i].textContent == "1" || xmlDoc.getElementsByTagName("iqa")[i].textContent == "0") {
                                                xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                                            }

                                            else {
                                                // debugger;
                                                xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;


                                                //  alert("Failed IQA cheque has successfully forced to pass");
                                                iqaForceCtr = 1;

                                            }
                                        }
                                        else {
                                            xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                                        }
                                    }
                                    else {

                                        if (xmlDoc.getElementsByTagName("iqa")[i].textContent == "1" || xmlDoc.getElementsByTagName("iqa")[i].textContent == "0") {
                                            xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;
                                            //alert("Failed IQA cheque has successfully forced to pass");
                                            iqaForceCtr = 1;
                                        }

                                        else {
                                            xmlDoc.getElementsByTagName("iqa")[i].textContent = ResultIQA;

                                        }
                                    }


                                }
                            }
                        }
                    }




                    var serializer = new XMLSerializer();
                    var xmlstring = serializer.serializeToString(xmlDoc);

                    var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                    objFileForWrite.write(xmlstring);
                    objFileForWrite.close();
                }
            }


            catch (e) {
                exceptionMsg = "(jsXMLInsertIQA) Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                $("#sErrorMsg").text(exceptionMsg);
                $("#divErrorMsg").show();
            }
        }

        //Eject
        function jsEject() {
            var strScannerErrDesc;
            //debugger
            $("#btnScan").prop("disabled", true);//frmCapturing.btnScan.disabled = true
            $("#btnEject").prop("disabled", true);//frmCapturing.btnEject.disabled = true
            $("#btnEndBatch").prop("disabled", true);//frmCapturing.btnEndBatch.disabled = true
            $("#btnRemove").prop("disabled", true);//frmCapturing.btnRemove.disabled = true
            $("#btnRemoveAll").prop("disabled", true)//frmCapturing.btnRemoveAll.disabled = true
            $("#btnIQA").prop("disabled", true);//frmCapturing.btnIQA.disabled= true
            $("#btnRescan").prop("disabled", true);//frmCapturing.btnRescan.disabled = true

            scnControl.Eject();


            //var filesys, objfilesize, createdate;//Dim filesys, objfileSize, createdate
            //filesys = CreateObject("Scripting.FileSystemObject");//Set filesys = CreateObject("Scripting.FileSystemObject")

            if (scnControl.mLngRet != 0) {//If scnControl.mLngRet <> 0 Then
                strScannerErrDesc = jsRetrieveScannerErr(scnControl.mLngRet);//strScannerErrDesc = jsRetrieveScannerErr(scnControl.mLngRet)
                //(strScannerErrDesc);//Call (strScannerErrDesc)
                $("#sErrorMsg").text(strScannerErrDesc);
                $("#divErrorMsg").show();
                $("#txtStatus").val(strScannerErrDesc);//frmCapturing.txtStatus.value = strScannerErrDesc	             
            }
            else {//Else
                $("#txtStatus").val("Scanner Connected");//frmCapturing.txtStatus.value = jsRetrieveMessage("msgStrScannerConnect")
            }//End If


            //Call jsReloadPage()
            jsInitScanner = true;

            $("#btnScan").prop("disabled", false);
            jsRefreshPage();
        }

        //Update XML on force iqa pass
        function jsEditDataXMLFileForceIQA() {
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
            }
            catch (e) {
                //(jsRetrieveMessage("msgStrActiveXControlError"))
                //("Fail to load local file");
                $("#sErrorMsg").text("Fail to load local file");
                $("#divErrorMsg").show();
                jsRefreshPage();
            }
            var i
            if (objFS.FileExists(strFilePath)) {

                var f = objFS.GetFile(strFilePath);
                var size = f.size;

                if (size) {

                    var objFile = objFS.OpenTextFile(strFilePath)
                    var outputDoc = objFile.readAll();
                    objFile.close()

                    var parser = new DOMParser();
                    var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                    var flag4 = xmlDoc.getElementsByTagName("flag4")[0].textContent;
                    if (jsTrim(flag4) == '') {
                        xmlDoc.getElementsByTagName("flag4")[0].textContent = "1";
                    }

                    //Set hChqType into cheque_type Node Field   
                    var strChqType = xmlDoc.getElementsByTagName("cheque_type")[0].textContent;
                    if (jsTrim(strChqType) == '') {
                        xmlDoc.getElementsByTagName("cheque_type")[0].textContent = document.getElementById("hChqType").value;
                    }
                }

                else {
                    //(jsRetrieveMessage("msgStrDataXMLMissing"));
                    //("Fail to update scanning items information in data.xml")
                    $("#sErrorMsg").text("Fail to update scanning items information in data.xml");
                    $("#divErrorMsg").show();
                    return false;
                }

                //Write into data.xml
                var serializer = new XMLSerializer();
                var xmlstring = serializer.serializeToString(xmlDoc);

                var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                objFileForWrite.write(xmlstring);
                objFileForWrite.close();

                var objXMLField = $("#objXMLField");
                var objTblFooter = document.getElementById('tblFooter')
                var objTblBody = document.getElementById('tblBody')

                jsLoadXMLDoc();                            //read data xml document at client side
                jsResetRowTemplate();
                jsBindLineItems(objXMLField, objTblFooter, objTblBody);
                return true;
            }
        }

        //Move IQA Fail Reason to Filter1
        function jsXMLInsertForceIQA(selectedUIC, ResultIQA) {
            //debugger
            var exceptionMsg = '';
            try {
                var objFS = new ActiveXObject("Scripting.FileSystemObject")
                var i
                if (objFS.FileExists(strFilePath)) {
                    var f = objFS.GetFile(strFilePath);
                    var size = f.size;
                    if (size) {
                        var objFile = objFS.OpenTextFile(strFilePath)
                        var outputDoc = objFile.readAll();
                        objFile.close()

                        //var xmlDoc = new ActiveXObject("MICROSOFT.FreeThreadedXMLDOM");
                        //xmlDoc.async = "false";
                        //xmlDoc.loadXML(outputDoc);

                        var parser = new DOMParser();
                        var xmlDoc = parser.parseFromString(outputDoc, "text/xml");

                        var nodeList = xmlDoc.getElementsByTagName("data");
                        var strSelectedUIC = selectedUIC;
                        var strDataXmlUIC;

                        if (nodeList.length > 0) {
                            for (i = 0; i < nodeList.length; i++) {
                                strDataXmlUIC = jsTrim(xmlDoc.getElementsByTagName("uic")[i].textContent);
                                if (strSelectedUIC == strDataXmlUIC) {
                                    xmlDoc.getElementsByTagName("ref1")[i].textContent = ResultIQA;
                                }
                            }
                        }
                        var serializer = new XMLSerializer();
                        var xmlstring = serializer.serializeToString(xmlDoc);

                        var objFileForWrite = objFS.OpenTextFile(strFilePath, 2, false);
                        objFileForWrite.write(xmlstring);
                        objFileForWrite.close();
                    }
                }

            }
            catch (e) {
                exceptionMsg = "(jsXMLInsertForceIQA) Error Message: " + e.message;
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Code: ";
                exceptionMsg = exceptionMsg + (e.number);
                exceptionMsg = exceptionMsg + "\n";
                exceptionMsg = exceptionMsg + "Error Name: " + e.name;
                //(exceptionMsg);
                $("#sErrorMsg").text(exceptionMsg);
                $("#divErrorMsg").show();
            }
        }



        //----------------------------------------------------------------------------------------------------------
        //-------MICR PARSER
        //-----------------

        //MICRParser - identify 2 digits Check Digit field from the MICR string pass in

        //ns84s1a10001e0001a000001s1000020000134567880m


        function jsGetCD(strInText) {
            var int1;
            var int2;
            var intBOD;
            var intEOD;
            var strResult;
            //alert("2");
            intBOD = strInText.indexOf("m") + 1;
            intEOD = strInText.indexOf("n") + 1;
            int1 = strInText.indexOf("a") + 1;
            int2 = strInText.indexOf("s") + 1;

            if (int1 > 0 && int1 < 10 && int2 > 0 && int2 < 8 && int2 - int1 - 1 > 0) {
                // alert("3");
                strResult = strInText.substr((int1 + 1) - 1, int2 - int1 - 1);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;

            }
            else if (int1 > 0 && int1 < 10 && int2 > 0 && int2 > 8) {
                //  alert("4");
                strResult = strInText.substr((int1 + 1) - 1, 2);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;

            }
            else if (int1 > 10 && intEOD > 0 && int2 > 0 && int2 - intEOD - 1 < 5 && int2 - intEOD - 1 > 0 && int2 - 2 > 0) {
                // alert("5");
                strResult = strInText.substr((int2 - 2) - 1, 2);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;//Return strResult

            }
            else {//Else
                //alert("6");
                return "";//Return ""

            }//End If
            // alert("7");
            return "";//Return ""
        }

        function ReplaceSymbolToAlphabet(strActualMICR) {
            //debugger;

            if (strActualMICR.length > 0) {
                //< 00 < 1: 2004 = 0011: 999986 < 12345678901234567890
                //To remove first 12 unwanted chars from MICR line
                strActualMICR = strActualMICR.substr(12, strActualMICR.length - 12);
                //To remove unwanted space
                strActualMICR = jsTrim(strActualMICR);

                //To replace any symbols.
                //String.replace(/<TERM>/g, '')
                strActualMICR = strActualMICR.replace(/:/g, "a");
                strActualMICR = strActualMICR.replace(/</g, "s");
                strActualMICR = strActualMICR.replace(/=/g, "e");
                strActualMICR = strActualMICR.replace(/;/g, "x");
                strActualMICR = strActualMICR.replace(/E/g, "?");
                strActualMICR = strActualMICR.replace(" ", "");

                //To add characters in the beginning and end
                strActualMICR = "n" + strActualMICR + "m";


            }
            return strActualMICR;
        }

        function jsGetCDCTS(strInText) {
            var int1;
            var int2;
            var intBOD;
            var intEOD;
            var strResult;
            //alert("2");
            //intBOD = strInText.indexOf("m") + 1;
            //intEOD = strInText.indexOf("n") + 1;
            //int1 = strInText.indexOf("a") + 1;
            //int2 = strInText.indexOf("s") + 1;
            //intEOD = strInText.indexOf("n") + 1;
            int1 = strInText.indexOf("s") + 1;
            int2 = strInText.indexOf("s", int1) + 1;

            if (int1 > 0 && int1 < 10 && int2 > 0 && int2 < 8 && int2 - int1 - 1 > 0) {
                // alert("3");
                strResult = strInText.substr((int1 + 1) - 1, int2 - int1 - 1);
                //alert(strResult);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;

            }
            else if (int1 > 0 && int1 < 10 && int2 > 0 && int2 > 8) {
                //  alert("4");
                strResult = strInText.substr((int1 + 1) - 1, 2);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;

            }
            else if (int1 > 10 && intEOD > 0 && int2 > 0 && int2 - intEOD - 1 < 5 && int2 - intEOD - 1 > 0 && int2 - 2 > 0) {
                //     alert("5");
                strResult = strInText.substr((int2 - 2) - 1, 2);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;//Return strResult

            }
            else {//Else
                //    alert("6");
                return "";//Return ""

            }//End If
            // alert("7");
            return "";//Return ""
        }

        //MICRParser - identify 10 digits Serial Number field from the MICR string pass in
        function jsGetSer(strInText) {
            var int1;
            var int2;
            var int3;
            var strResult;

            int1 = strInText.indexOf("s") + 1;
            int2 = strInText.indexOf("s", int1) + 1;
            int3 = strInText.indexOf("s", int2) + 1;

            if (int1 > 0 && int1 < 6 && int2 > 0 && int2 - 1 < 14 && int2 - int1 - 1 > 0) {
                strResult = strInText.substr((int1 + 1) - 1, int2 - int1 - 1);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int1 > 0 && int1 < 6 && int2 > 0 && int2 > 14) {
                strResult = strInText.substr((int1 + 1) - 1, 6);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int1 > 6 && int1 < 28 && int1 - 6 > 0) {
                strResult = strInText.substr((int1 - 6) - 1, 6)
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else {
                return "";
            }

            return "";
        }

        //MICRParser - identify 10 digits Serial Number field from the MICR string pass in
        function jsGetSerCTS(strInText) {
            var int1;
            var int2;
            var int3;
            var strResult;

            int1 = strInText.indexOf("a") + 1;
            int2 = strInText.indexOf("a", int1) + 1;
            int3 = strInText.indexOf("s", int2) + 1;


            if (int2 > 6 && int2 < 28 && int2 - 6 > 0) {
                strResult = strInText.substr(int2, 6)
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else {
                return "";
            }

            return "";
        }

        //MICRParser - identify 1 digit Type
        function jsGetTypeCTS(strInText) {
            var int1;
            var int2;
            var int3;
            var strResult;

            int1 = strInText.indexOf("s", 3) + 1;
            int2 = strInText.indexOf("a", int1) + 1;
            int3 = strInText.indexOf("s", int2) + 1;

            if (int1 > 0 && int1 < 6 && int2 > 0 && int2 - 1 < 14 && int2 - int1 - 1 > 0) {
                strResult = strInText.substr((int1 + 1) - 1, int2 - int1 - 1);

                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int1 > 0 && int1 < 6 && int2 > 0 && int2 > 14) {
                strResult = strInText.substr((int1 + 1) - 1, 6);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int1 > 6 && int1 < 28 && int1 - 6 > 0) {
                strResult = strInText.substr((int1 - 6) - 1, 6)
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else {
                return "";
            }

            return "";
        }

        //MICRParser - identify 5 digits Bank Code field from the MICR string pass in
        function jsGetBk(strInText) {
            var int1;
            var int2;
            var int3;
            var strResult;

            int1 = strInText.indexOf("e") + 1;
            int2 = strInText.indexOf("s") + 1;
            int3 = strInText.indexOf("s", int2) + 1;

            if (int1 > 3 && int1 - 2 > 0) {
                strResult = strInText.substr((int1 - 5) - 1, 5)
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else if (int2 > 0 && int2 < 6 && int3 > int2 && int3 - 1 < 14) {
                strResult = strInText.substr((int3 + 1) - 1, 3);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else {
                return "";
            }

            return "";
        }

        //MICRParser - identify 4 digits Branch Code field from the MICR string pass in
        function jsGetBr(strInText) {
            var int1;
            var int2;
            var int3;
            var strResult;

            int1 = strInText.indexOf("e") + 1;
            int2 = strInText.indexOf("a", int1) + 1;
            int3 = strInText.indexOf("a", int2) + 1;

            if (int1 > 0) {
                strResult = strInText.substr((int1 + 1) - 1, 9);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int3 > 3 && int3 - 9 > 0) {
                strResult = strInText.substr((int3 - 9) - 1, 9);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else if (int2 > 0 && int3 == 0 && int2 - 9 > 0) {
                strResult = strInText.substr((int2 - 9) - 1, 9);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else {
                return "";
            }

            return "";
        }

        //MICRParser - identify 10 digits Account Number field from the MICR string pass in
        function jsGetAcc(strInText) {
            var int1;
            var int2;
            var int3;
            var int4;
            var int5;
            var int6e;
            var intBOD;
            var intEOD;
            var iCdFound;
            var iAmtFound;
            var strResult;

            intBOD = strInText.indexOf("m") + 1;
            intEOD = strInText.indexOf("n") + 1;
            int1 = strInText.indexOf("a") + 1;
            int2 = strInText.indexOf("s") + 1;
            int3 = strInText.indexOf("a", int1) + 1;
            int4 = strInText.indexOf("s", int2) + 1;
            int5 = strInText.indexOf("s", int4) + 1;
            int6e = strInText.indexOf("e") + 1;

            if (jsGetCD(strInText).length > 0) {
                iCdFound = true;
            }
            if (jsGetAmt(strInText).length > 0) {
                iAmtFound = true;
            }

            if (iCdFound == true && int3 > int1 && int5 > int4 && int4 > int2 && int5 - int3 > 0) {
                strResult = strInText.substr((int3 + 1) - 1, int5 - int3 - 1);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int3 == 0 && int1 > 6 && int4 > int2 && int5 > int4 && int5 - int2 > 0) {
                strResult = strInText.substr((int1 + 1) - 1, int5 - int1 - 1);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int1 > 0 && int3 > int1 && int2 > 0 && int4 > int2 && int5 == 0 && intBOD - intEOD > 35 && iAmtFound == true) {
                strResult = strInText.substr((int3 + 1) - 1, 13);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int1 > 0 && int3 > int1 && int2 > 0 && int4 > int2 && int5 == 0 && intBOD - intEOD > 24 && iAmtFound == false) {
                strResult = strInText.substr((int3 + 1), 13);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int3 == 0 && int2 > 0 && int4 > int2 && int5 > int4 && int5 - int3 > 0 && int6e > 0) {
                strResult = strInText.substr((int6e + 7) - 1, int5 - int6e - 7);
                strResult = jsVerify(strResult, "LR");
                return strResult;
            }
            else if (int3 == 0 && int2 > 0 && int4 > int2 && int5 > int4 && int5 - int3 > 0) {
                strResult = strInText.substr((int5 - 13) - 1, 13);
                strResult = jsVerifyField(str);
                return strResult;
            }
            else if (int2 < int1 && int1 > 0 && int3 == 0 && int5 == 0 && int4 > int2 && intBOD - intEOD > 34 && iAmtFound == true) {
                strResult = strInText.substr((int1 + 1) - 1, 13);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int2 < int1 && int1 > 0 && int3 == 0 && int5 == 0 && int4 > int2 && intBOD - intEOD > 22 && iAmtFound == false) {
                strResult = strInText.substr((int1 + 1) - 1, 13);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int2 > 0 && int4 > int2 && int5 > int4 && int5 - int3 > 0) {
                var CheckStr;

                CheckStr = strInText.substr((int5 - 13) - 1, int5 - int3 - 1);

                if (CheckStr.substr(0, 1) == "a") {
                    strResult = strInText.substr((int5 - 12) - 1, int5 - int3 - 1)
                }
                else {
                    strResult = strInText.substr((int5 - 13) - 1, int5 - int3 - 1);
                }
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else if (int4 == 0 && int2 > 25 && int2 - 13 > 0) {
                strResult = strInText.substr((int2 - 13) - 1, 13);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else {
                return "";
            }

            return "";
        }

        //MICRParser - identify 10 digits Account Number field from the MICR string pass in
        function jsGetAccCTS(strInText) {
            var int1;
            var int2;
            var int3;
            var int4;
            var int5;
            var int6e;
            var intBOD;
            var intEOD;
            var iCdFound;
            var iAmtFound;
            var strResult;

            intBOD = strInText.indexOf("m") + 1;
            intEOD = strInText.indexOf("n") + 1;
            int1 = strInText.indexOf("a") + 1;
            int2 = strInText.indexOf("s") + 1;
            int3 = strInText.indexOf("a", int1) + 1;
            int4 = strInText.indexOf("s", int2) + 1;
            int5 = strInText.indexOf("s", int4) + 1;
            int6e = strInText.indexOf("e") + 1;

            if (jsGetCDCTS(strInText).length > 0) {
                iCdFound = true;
            }
            if (jsGetAmtCTS(strInText).length > 0) {
                iAmtFound = true;
            }

            //if (int3 > int1 && int5 > int4 && int4 > int2 && int5 - int3 > 0) {
            //    strResult = strInText.substr(int4, 13);
            //    strResult = jsVerifyField(strResult, "LR");
            //    return strResult;
            //}         

            if (int1 > 0 && int5 > int4 && int4 > int2 && int5 - int3 > 0) {
                strResult = strInText.substr(int5, 20);

                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            return "";
        }

        //MICRParser - identify 2 digits Transaction Code field from the MICR string pass in
        function jsGetTC(strInText) {
            var int1;
            var int2;
            var int3;
            var int4;
            var int5;
            var int6a;
            var int7a;
            var iAccFound;
            var intBOD;
            var intEOD;
            var strResult;

            intBOD = strInText.indexOf("m") + 1;
            intEOD = strInText.indexOf("n") + 1;

            int1 = strInText.indexOf("s") + 1;
            int2 = strInText.indexOf("s", int1) + 1;
            int3 = strInText.indexOf("s", int2) + 1;
            int4 = strInText.indexOf("x") + 1;
            int5 = strInText.indexOf("x", int4) + 1;
            int6a = strInText.indexOf("a") + 1;
            int7a = strInText.indexOf("a", int6a) + 1;

            if (jsGetAcc(strInText).length > 0) {
                iAccFound = true;
            }

            if (iAccFound == true && int3 > 25 && int4 > 0 && int4 - int3 - 1 > 0) {
                strResult = strInText.substr((int3 + 1) - 1, int4 - int3 - 1);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (iAccFound == true && int3 < 25) {
                strResult = strInText.substr((int3 + 1) - 1, 2);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int4 > 0 && int5 == 0 && int4 < 36 && int3 == 0 && int2 > int1 && int4 - 2 > 0) {
                strResult = strInText.substr((int4 - 2) - 1, 2);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else if (int4 > 0 && int5 == 0 && int4 < 36 && int3 == 0 && int2 > int1 && int4 - 2 > 0) {
                strResult = strInText.substr((int2 + 1) - 1, 2);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int4 > 0 && int5 > int4 && int4 - 2 > 0) {
                strResult = strInText.substr((int4 - 2) - 1, 2);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else if (iAccFound == true && int4 > 0 && int5 > int4 && int6a > 0 && int4 - int3 - 1 > 0) {
                strResult = strInText.substr((int6a + 1) - 1, int4 - int6a - 1);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int4 > 0 && int5 > int4 && int4 - 2 > 0) {
                strResult = strInText.substr((int4 - 2) - 1, 2);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else if (int4 == 0 && int5 == 0 && intBOD > 0 && intBOD - 2 > 0) {
                strResult = strInText.substr((intBOD - 2) - 1, 2);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else {
                return "";
            }

            return "";
        }

        //MICRParser - identify 0 to 10 digits Amount field from the MICR string pass in
        function jsGetAmt(strInText) {
            var int1;
            var int2;
            var intBOD;
            var strResult;

            intBOD = strInText.indexOf("m") + 1;
            int1 = strInText.indexOf("x") + 1;
            int2 = strInText.indexOf("x", int1) + 1;

            if (int1 > 0 && intBOD - int1 > 11) {
                strResult = strInText.substr((int1 + 1) - 1, 11);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int1 > 0 && int1 < int2 && int2 - int1 - 1 > 0) {
                strResult = strInText.substr((int1 + 1) - 1, int2 - int1 - 1);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
            }
            else if (int1 > 0 && int2 == 0 && int1 < 37 && intBOD > 0 && intBOD - int1 - 1 > 0) {
                strResult = strInText.substr((int1 + 1) - 1, intBOD - int1 - 2);
                strResult = jsVerifyField((strResult));
                return strResult;
            }
            else if (int1 > 0 && int2 == 0 && int1 > 37) {
                strResult = strInText.substr((int1 - 12) - 1, 12);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
            }
            else {
                return "";
            }

            return "";
        }

        //MICRParser - identify 0 to 13 digits Amount field from the MICR string pass in
        function jsGetAmtCTS(strInText) {
            var int1;
            var int2;
            var intBOD;
            var strResult;

            intBOD = strInText.indexOf("m") + 1;
            int1 = strInText.indexOf("x") + 1;
            int2 = strInText.indexOf("x", int1) + 1;

            if (int1 > 0 && intBOD - int1 > 11) {
                strResult = strInText.substr((int1 + 1) - 1, 13);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
                //alert(strResult);
            }
            else if (int1 > 0 && int1 < int2 && int2 - int1 - 1 > 0) {
                strResult = strInText.substr((int1 + 1) - 1, int2 - int1 - 1);
                strResult = jsVerifyField(strResult, "LR");
                return strResult;
                //alert("jsGetAmtCTS1");
            }
            else if (int1 > 0 && int2 == 0 && int1 < 37 && intBOD > 0 && intBOD - int1 - 1 > 0) {
                strResult = strInText.substr((int1 + 1) - 1, intBOD - int1 - 2);
                strResult = jsVerifyField((strResult));
                return strResult;
            }
            else if (int1 > 0 && int2 == 0 && int1 > 37) {
                strResult = strInText.substr((int1 - 12) - 1, 12);
                strResult = jsVerifyField(strResult, "RL");
                return strResult;
                //alert("strResult1")
            }
            else {
                return "";
            }

            return "";
        }

        //MICRParser - to filter out overlaped field (if any)
        function jsVerifyField(strText, strStartPosition) {
            var intCnt;
            var intLen;

            intLen = strText.length;
            // alert(strText);
            if (strStartPosition.toUpperCase() == "LR") {//check from Left to Right
                for (intCnt = 1; intCnt <= intLen; intCnt++) {
                    if (strText.substr(intCnt - 1, 1) == "a" ||
                        strText.substr(intCnt - 1, 1) == "s" ||
                        strText.substr(intCnt - 1, 1) == "e" ||
                        strText.substr(intCnt - 1, 1) == "x" ||
                        strText.substr(intCnt - 1, 1) == "m" ||
                        strText.substr(intCnt - 1, 1) == "n") {
                        return strText.substr(0, intCnt - 1);
                    }
                }
            }
            else if (strStartPosition.toUpperCase() == "RL") { //check from Right to left
                for (intCnt = intLen; intCnt >= 1; intCnt--) {
                    if (strText.substr(intCnt - 1, 1) == "a" ||
                        strText.substr(intCnt - 1, 1) == "s" ||
                        strText.substr(intCnt - 1, 1) == "e" ||
                        strText.substr(intCnt - 1, 1) == "x" ||
                        strText.substr(intCnt - 1, 1) == "m" ||
                        strText.substr(intCnt - 1, 1) == "n") {
                        return strText.substr(intCnt, intLen - intCnt);
                    }
                }
            }
            return strText.substr(0, intLen);
        }


        //----------------------------------------------------------------------------------------------------------
        //-------AJAX
        //-----------------

        function ajaxUpdateUICInfo(intScannerId, intBatchNo, intSeqNo, intUserId, intClearingBranch) {
            var blnSuccess;
            //alert(intScannerId);
            //alert(intBatchNo);
            //alert(intSeqNo);
            // alert(intUserId);
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/UpdateUICInfo",
                method: "POST",
                data: "intScannerId=" + intScannerId + "&intBatchNo=" + intBatchNo + "&intSeqNo=" + intSeqNo + "&intUserId=" + intUserId + "&intClearingBranch=" + intClearingBranch,
                success: function (data) {
                    blnSuccess = true;

                }
            });

            return blnSuccess;
        }

        function ajaxCheckBank(bankCode) {
            //  debugger;
            var blnSuccess;

            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/CheckBank",
                method: "POST",
                data: "bankCode=" + bankCode,
                success: function (data) {

                    if (data == true) {
                        blnSuccess = true;
                    }

                }
            });

            return blnSuccess;
        }


        function ajaxGetUICInfoDataTableWithUpdate(intScannerId, intBranchId) {
            if (parseInt($("#sSeqNo").text()) == 0 || parseInt($("#sSeqNo").text()) == "") {

                $("#sSeqNo").text(leftPad(1, 4));

                //$.ajax({
                //    async: false,
                //    cache: false,
                //    url: App.ContextPath + "CommonApi/GetAndUpdateUICInfoIncSequence",
                //    method: "POST",
                //    data: "intScannerId=" + intScannerId + "&intBranchId=" + intBranchId,
                //    success: function (data) {
                //        $.each(data, function (i, item) {
                //            if (item.fldseqno == "-1") {
                //                $("#sSeqNo").text(0);
                //            }
                //            else {

                //                if (item.fldseqno == 0 || item.fldseqno == "0") {
                //                    $("#sSeqNo").text(item.fldseqno + 1);
                //                }
                //                else {
                //                    $("#sSeqNo").text(item.fldseqno);
                //                }

                //            }
                //            //$("#sBatchNo").text(jsTrim(item.fldbatchno));
                //            $("#sBatchNo").text(jsRight("0000" + jsTrim(item.fldbatchno), 4));
                //            //alert("hi");
                //            //alert(item.fldbatchno);
                //            //jsRight("0000" + jsTrim(item.fldbatchno), 4);
                //        });
                //        return true;
                //    }
                //});
            }
            else {
                $("#sSeqNo").text(leftPad(parseInt($("#sSeqNo").text()) + 1, 4));//document.getElementById("lblSeqNo").innerText = parseInt(document.getElementById("lblSeqNo").innerText) + 1;   
            }
        }

        function ajaxGetCapturingModeDesc(strModeId) {
            var strDesc = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/GetCapturingModeDesc",
                method: "POST",
                data: "strModeId=" + strModeId,
                success: function (data) {
                    $.each(data, function (i, item) {
                        strDesc = item.fldDescription;
                    });
                }
            });
            return strDesc;
        }

        function ajaxGetCheckTypeDesc(strTypeId) {
            var strReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/GetCheckTypeDetails",
                method: "POST",
                data: "strTypeId=" + strTypeId,
                success: function (data) {
                    $.each(data, function (i, item) {
                        strReturn = item.fldDescription;
                    });
                }
            });
            return strReturn;
        }

        function ajaxFBIQA(logFullFilePath, settingPath, sChequePath, sFrontIMG, sBackIMG, sGFrontIMG, sGBackIMG) {
            var strReturn = "";
            $.ajax({
                async: false,
                cache: false,
                url: App.ContextPath + "CommonApi/FBIQA",
                method: "POST",
                data: "logFullFilePath=" + logFullFilePath + "&settingPath=" + settingPath + "&sChequePath=" + sChequePath + "&sFrontIMG=" + sFrontIMG + "&sBackIMG=" + sBackIMG + "&sGFrontIMG=" + sGFrontIMG + "&sGBackIMG=" + sGBackIMG,
                success: function (data) {
                    strReturn = data;
                }
            });
            return strReturn;
        }
    });

})();

//Display scanned item in the row
//alert("bb");
function jsDisplayScanItem(row, intPageId) {
    var strUIC;
    //Display item when click on table field
    if (intPageId == 1) {
        strUIC = jsTrim(row.children(5).innerText);
        //alert(strUIC);
        //Hire UIC string into value
        document.getElementById("hRowClickUIC").value = strUIC;

        //Display UIC string into textbox
        document.getElementById("txtUIC").value = strUIC;
    }
    //Use for immediate entry page: display item when page loaded and it will display when user click on page
    if (intPageId == 2) {
        strUIC = jsTrim(document.getElementById("hPreviousUIC").value);
    }

    //var strFrontImageLocation = strFolderName + "\\" + strUIC + "BF.TIF"
    //var strBackImageLocation = strFolderName + "\\" + strUIC + "BB.TIF"
    var strFrontImageLocation = strFolderName + "\\" + strUIC + "GF.JPG"
    var strBackImageLocation = strFolderName + "\\" + strUIC + "GU.tif"
    var strBackGBImageLocation = strFolderName + "\\" + strUIC + "BB.TIF"
    //var strBackImageLocation = strFolderName + "\\" + strUIC + "GB.JPG"

    //Display scanned images into OCX
    jsDisplayScanItemToOCX(1, strFrontImageLocation)
    jsDisplayScanItemToOCX(2, strBackImageLocation)
    jsDisplayScanItemToOCX(3, strBackGBImageLocation)

    //Set value into table body
    var objTableBody = document.getElementById("tblBody")
    var intSelectedIndex = document.getElementById("hSelectedRowIndex").value


    if (intSelectedIndex != '' && intSelectedIndex != row.rowIndex) {
        intSelectedIndex = parseInt(intSelectedIndex) - 1
        if (objTableBody.rows[intSelectedIndex] != undefined)
            objTableBody.rows[intSelectedIndex].className = "cssListingRowMouseLeaveColor"
    }

    row.className = "cssListingRowMouseClickColor"
    document.getElementById("hSelectedRowIndex").value = row.rowIndex
    $("#btnRescan").prop("disabled", false);
}

//Trim
function jsTrim(str) {

    if (!str || typeof str != 'string')
        return '';

    return str.replace(/^\s+|\s+$/g, "");
}

//Display all the item field in table
function jsDisplayScanItemToOCX(intPicBoxId, strItemLocation) {
    var filesys, getname, path;

    //On Error Resume Next
    filesys = new ActiveXObject("Scripting.FileSystemObject");

    //'//BeginMod: Joe 01/08/2007
    //If Err.number <> 0 Then
    //Call jsAlert(jsRetrieveMessage("msgStrActiveXControlError2"))
    //Call jsReloadPage()
    //Exit Function
    //End If
    //'//EndMod: Joe 01/08/2007

    path = filesys.GetAbsolutePathName(strItemLocation);
    //alert(path);
    getname = filesys.GetFileName(path);
    if (filesys.FileExists(path)) {//If filesys.FileExists(path) Then
        scnControl.DisplayScanningItem(intPicBoxId, strItemLocation);
    }
}

//Add and remove selected/unselected UIC
function jsAddToUIC(s, r) {
    var hSelectedUIC;
    hSelectedUIC = document.getElementById("hSelectedUIC");
    if (!r) {
        if (g_uic.length >= 1) {
            for (var i = 0; i < g_uic.length; i++) {
                if (g_uic[i] == s.toString()) {
                    g_uic.splice(i, 1);
                }
            }
        }
    }
    else {
        if (g_uic.length >= 1) {
            for (var i = 0; i < g_uic.length; i++) {
                if (g_uic[i] == s.toString()) {
                    g_uic.splice(i, 1);
                }
            }
        }
        g_uic.push(s);
    }

    hSelectedUIC.Value = "";
    for (var x = 0; x < g_uic.length; x++) {
        if (x == 0) {
            hSelectedUIC.Value = g_uic[x];
        }
        else {
            hSelectedUIC.Value = hSelectedUIC.Value + "|" + g_uic[x];
        }
    }
}

//Check all
function jsCheckAll(bx) {
    var cbs = document.getElementsByTagName('input');
    for (var i = 0; i < cbs.length; i++) {
        if (cbs[i].type == 'checkbox' && cbs[i].name.substring(0, 3) == 'cb_') {
            cbs[i].checked = bx.checked;
            jsAddToUIC(cbs[i].name.substring(3, cbs[i].name.length), cbs[i].checked);
        }
    }
}

//Uncheck
function jsUncheckOptionButton(optUncheckField) {
    if (optUncheckField != undefined) {
        for (i = 0; i < optUncheckField.length; i++) {
            if (optUncheckField[i].disabled == false) { // if a button in group is checked,
                optUncheckField[i].disabled = true;   // check it or disable it
            }
        }
    }
}

//When mouse over the row, row will highlighted in CSS cssListingRowMouseOverColor
function jsRowMouseOver(row) {
    var intSelectedIndex = jsTrim(document.getElementById("hSelectedRowIndex").value)
    if (intSelectedIndex != row.rowIndex)
        row.className = "cssListingRowMouseOverColor"
}

//When mouse leave from the row, row will highlighted in CSS cssListingRowMouseOverColor
function jsRowMouseLeave(row) {
    var intSelectedIndex = jsTrim(document.getElementById("hSelectedRowIndex").value)
    if (intSelectedIndex != row.rowIndex)
        row.className = "cssListingRowMouseLeaveColor"
}

function leftPad(n, length) {
    var str = (n > 0 ? n : -n) + "";
    var zeros = "";
    for (var i = length - str.length; i > 0; i--)
        zeros += "0";
    zeros += str;
    return n >= 0 ? zeros : "-" + zeros;
}