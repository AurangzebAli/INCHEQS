﻿var input = ""; //holds current input as a string
(function () {
    $(document).ready(function () {
        var ItemType;
        var ItemID;
        var P_Flag = false;
        var RecordSelected = false;

        var CheckPExist = $("#tblBalHistory tbody");
        CheckPExist.find('tr').each(function (i) {
            var $tds = $(this).find('td'),
                intItems = $tds.eq(2).text()
            if (intItems == 'p' || intItems == 'P') {
                P_Flag = true;
            }
        });
        //debugger
        $("#htoUpdateBalancing").val("");
        var IPCAmount = $("#txtIPCAmount").val();
        var CUSTAmount = $("#txtCUSTAmount").val();
        if (IPCAmount != "") {
            IPCAmount = IPCAmount.replace(/[^0-9]/g, '');
        }

        if (CUSTAmount != "") {
            CUSTAmount = CUSTAmount.replace(/[^0-9]/g, '');
        }
        var Difference = 0.00;
        if (IPCAmount > CUSTAmount) {
            Difference = IPCAmount - CUSTAmount
            Difference = Math.abs(Difference);
            Difference = (Difference / 100).toFixed(2)
        }
        else if (CUSTAmount > IPCAmount) {
            Difference = CUSTAmount - IPCAmount
            Difference = Math.abs(Difference);
            Difference = (Difference / 100).toFixed(2)
        }
        else if (CUSTAmount == IPCAmount) {
            Difference = IPCAmount - CUSTAmount
            Difference = (Difference / 100).toFixed(2)
        }

        if (Difference > 0) {
            $('#txtDiffAmount').css('background-color', 'Red');
            $("#txtDiffAmount").val(addCommas(Difference));
        }

        $("#txtChequeAmount").keydown(function (event) {
            var keycode = event.keyCode || event.which;
            if (keycode == 107) {
                //debugger;
                var DiffAmount = $("#txtDiffAmount").val();
                if (DiffAmount != "0.00" && DiffAmount != "0") {
                    alert("Amount is not Balanced.");
                    $("#txtChequeAmount").focus();
                    return;
                }
                else {
                    $("#Confirmbtn").trigger('click');
                }
            }
        });


        $(".BalanceHistory").on('click', function () {

            var BalancingDetail = $(this).closest('tr');
            ItemID = BalancingDetail.find("td:eq(0)").text();
            var MakerChecker = BalancingDetail.find("td:eq(1)").text();
            ItemType = BalancingDetail.find("td:eq(2)").text();
            var UIC = BalancingDetail.find("td:eq(3)").text();
            var Amount = BalancingDetail.find("td:eq(6)").text();
            $('.BalanceHistory').css('background-color', 'white');
            $('#BalHis_' + ItemID).css('background-color', '#B3B6B7');
            if (Amount != "") {
                $('#txtChequeAmount').val("");
                Amount = Amount.replace(/[^0-9]/g, '');
                Amount = (Amount / 100).toFixed(2);
                if (P_Flag == true) {
                    BindChequeInfo(ItemID);
                    bindAndDrawImageControllerinBalancing(UIC);
                }
                $('#txtChequeAmount').val(addCommas(Amount));
                $('#txtChequeAmount').focus();
                $('#txtChequeAmount').select();
                RecordSelected = true;
            }
        });

        $("#txtChequeAmount").keydown(function (event) {
            var keycode = event.keyCode || event.which;
            if (keycode == 13) {
                jsCalculateDifference();
                input = "";
            }
        });
        function addCommas(num) {
            return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        }

        function jsCalculateDifference() {
            var userKeyInAmt = $('#txtChequeAmount').val().replace(/[^0-9]/g, '');
            if (userKeyInAmt > 0 && RecordSelected == true) {
                userKeyInAmt = (userKeyInAmt / 100).toFixed(2);
                if (ItemType == "C" || ItemType == "P") {
                    $("#txtIPCAmount").val(toCurrency(userKeyInAmt));
                    $('#BalHisAmt_' + ItemID).text(toCurrency(userKeyInAmt));

                    var TotalVAmount = 0;
                    var TotalCAmount = 0;
                    var table = $("#tblBalHistory tbody");
                    table.find('tr').each(function (i) {
                        var $tds = $(this).find('td'),
                            intItems = $tds.eq(2).text(),
                            intAmount = $tds.eq(6).text().replace(/[^0-9]/g, '');
                        if (intItems == 'V' || intItems == 'P') {
                            TotalVAmount = parseFloat(TotalVAmount) + parseFloat(intAmount);
                            $("#txtCUSTAmount").val(toCurrency(TotalVAmount));
                        }
                        else if (intItems == 'C') {
                            TotalCAmount = parseFloat(TotalCAmount) + parseFloat(intAmount);
                            $("#txtIPCAmount").val(toCurrency(TotalCAmount));
                        }
                    });

                    var IPCUpdatedAmt = TotalCAmount;//$("#txtIPCAmount").val().replace(/[^0-9]/g, '');
                    var CustAmt = TotalVAmount;//$("#txtCUSTAmount").val().replace(/[^0-9]/g, '');

                    if (IPCUpdatedAmt > CustAmt) {
                        Difference = IPCUpdatedAmt - CustAmt
                        Difference = Math.abs(Difference);
                        Difference = (Difference / 100).toFixed(2)
                    }
                    else if (CustAmt > IPCUpdatedAmt) {
                        Difference = CustAmt - IPCUpdatedAmt
                        Difference = Math.abs(Difference);
                        Difference = (Difference / 100).toFixed(2)
                    }
                    else if (CustAmt == IPCUpdatedAmt) {
                        Difference = IPCUpdatedAmt - CustAmt
                        Difference = (Difference / 100).toFixed(2)
                    }

                    if (Difference == 0 || Difference == 0.00) {
                        $('#txtDiffAmount').css('background-color', 'Green');
                        $("#txtDiffAmount").val(Difference);
                        //

                        var Data = "";
                        var table = $("#tblBalHistory tbody");
                        table.find('tr').each(function (i) {
                            var $tds = $(this).find('td'),
                                intItemID = $tds.eq(0).text(),
                                intItemType = $tds.eq(2).text(),
                                intAmount = $tds.eq(6).text().replace(/[^0-9]/g, ''),
                                intOriginalAmount = $tds.eq(7).text().replace(/[^0-9]/g, '');
                            if (intOriginalAmount != intAmount) {
                                if (Data == "") {

                                    Data = intItemID + "," + intItemType + "," + intAmount + "|";
                                }
                                else {
                                    Data = Data + intItemID + "," + intItemType + "," + intAmount + "|";;
                                }
                            }

                        });
                        Data = Data.substring(0, Data.length - 1);
                        $("#htoUpdateBalancing").val(Data);
                    }
                    else {
                        $('#txtDiffAmount').css('background-color', 'Red');
                        $("#txtDiffAmount").val(addCommas(Difference));
                    }

                }
                else {
                    var userKeyInAmt = $('#txtChequeAmount').val().replace(/[^0-9]/g, '');
                    $("#txtCUSTAmount").val(toCurrency(userKeyInAmt));
                    $('#BalHisAmt_' + ItemID).text(toCurrency(userKeyInAmt));

                    var TotalVAmount = 0;
                    var TotalCAmount = 0;
                    var table = $("#tblBalHistory tbody");
                    table.find('tr').each(function (i) {
                        var $tds = $(this).find('td'),
                            intItems = $tds.eq(2).text(),
                            intAmount = $tds.eq(6).text().replace(/[^0-9]/g, '');
                        if (intItems == 'V' || intItems == 'P') {
                            TotalVAmount = parseFloat(TotalVAmount) + parseFloat(intAmount);
                            $("#txtCUSTAmount").val(toCurrency(TotalVAmount));
                        }
                        else if (intItems == 'C') {
                            TotalCAmount = parseFloat(TotalCAmount) + parseFloat(intAmount);
                            $("#txtIPCAmount").val(toCurrency(TotalCAmount));
                        }
                    });


                    var CustUpdatedAmt = TotalVAmount;//$("#txtCUSTAmount").val().replace(/[^0-9]/g, '');
                    var IPCAmut = TotalCAmount;//$("#txtIPCAmount").val().replace(/[^0-9]/g, '');

                    if (IPCAmut > CustUpdatedAmt) {
                        Difference = IPCAmut - CustUpdatedAmt
                        Difference = Math.abs(Difference);
                        Difference = (Difference / 100).toFixed(2)
                    }
                    else if (CustUpdatedAmt > IPCAmut) {
                        Difference = CustUpdatedAmt - IPCAmut
                        Difference = Math.abs(Difference);
                        Difference = (Difference / 100).toFixed(2)
                    }
                    else if (CustUpdatedAmt == IPCAmut) {
                        Difference = IPCAmut - CustUpdatedAmt
                        Difference = (Difference / 100).toFixed(2)
                    }

                    if (Difference == 0 || Difference == 0.00) {
                        $('#txtDiffAmount').css('background-color', 'Green');
                        $("#txtDiffAmount").val(Difference);

                        var Data = "";
                        var table = $("#tblBalHistory tbody");
                        table.find('tr').each(function (i) {
                            var $tds = $(this).find('td'),
                                intItemID = $tds.eq(0).text(),
                                intItemType = $tds.eq(2).text(),
                                intAmount = $tds.eq(6).text().replace(/[^0-9]/g, ''),
                                intOriginalAmount = $tds.eq(7).text().replace(/[^0-9]/g, '');
                            if (intOriginalAmount != intAmount) {
                                if (Data == "") {

                                    Data = intItemID + "," + intItemType + "," + intAmount + "|";
                                }
                                else {
                                    Data = Data + intItemID + "," + intItemType + "," + intAmount + "|";
                                }
                            }
                        });
                        Data = Data.substring(0, Data.length - 1);
                        $("#htoUpdateBalancing").val(Data);
                    }
                    else {
                        $('#txtDiffAmount').css('background-color', 'Red');
                        $("#txtDiffAmount").val(addCommas(Difference));
                    }
                }
            }
            else {

                alert("Please Select Record to make it Balanced.");
                return;

            }
        }
        $("#AddBalancingAmount").click(function () {
            jsCalculateDifference();
            input = "";
        });
        $("#txtChequeAmount").keydown(function (e) {
            //handle backspace key
            if (e.keyCode == 8 && input.length > 0) {
                input = input.slice(0, input.length - 1); //remove last digit
                $(this).val(addCommas(formatNumber(input)));
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


        function BindChequeInfo(ItemID)
        {
            $.ajax({
                cache: false,
                async: false,
                url: App.ContextPath + "CommonApi/GetBalancingIndividualItemDetail",
                method: "POST",
                data: "intItemID=" + ItemID,
                success: function (data) {
                    if (data.length > 0) {
                        $.each(data, function (i, item) {
                            $("#flduic").text(item.fldUIC);
                            $("input[name='new_fldcheckdigit']").val(item.fldCheckDigit);
                            $("input[name='new_fldtype']").val(item.fldType);
                            $("input[name='new_fldstatecode']").val(item.fldStateCode);
                            $("input[name='new_fldbankcode']").val(item.fldBankCode);
                            $("input[name='new_fldbranchcode']").val(item.fldBranchCode);
                            $("input[name='new_fldserial']").val(item.fldSerial);
                            $("input[name='new_fldissueraccNo']").val(item.fldIssuerAccNo);
                        });
                    }
                }
            });
        }

        function bindAndDrawImageControllerinBalancing(imgId) {
            $(".modify-cheque-btn").off("click.cheque")
                .on("click.cheque", function () {
                    // debugger
                    var actionClicked = $(this).data("btnfor");
                    var imgFolder = $("#imageFolder").val();
                    //var imgId = $("#imageId").val();
                    var imgState = $("#imageState").val();
                    var imgSys = $("#System").val();
                    if (actionClicked === "reset") {
                        //Reset Button Clicked.
                        //Remove All State
                        imgState = "greyscale,";
                        $('#chequeImage').css({ "transform": "" });
                    } else if (actionClicked === "front") {
                        imgState = imgState.replace("back", "");
                    } else if (actionClicked === "back") {
                        imgState = imgState.replace("front", "");
                    } else if (actionClicked === "bw") {
                        imgState += "," + "front";
                        imgState = imgState.replace("greyscale", "");
                        imgState = imgState.replace("uv", "");
                        imgState = imgState.replace("back", "");
                    } else if (actionClicked === "greyscale") {
                        imgState += "," + "front";
                        imgState = imgState.replace("bw", "");
                        imgState = imgState.replace("uv", "");
                        imgState = imgState.replace("back", "");
                    } else if (actionClicked === "uv") {
                        imgState = imgState.replace("bw", "");
                        imgState = imgState.replace("greyscale", "");
                        imgState = imgState.replace("back", "");
                    }

                    //If state clicked found in previous state, remove it.
                    //This make the button act as Toggle
                    if (imgState.indexOf(actionClicked) >= 0) {
                        imgState = imgState.replace("," + actionClicked, "");
                    } else {
                        //Add the state 
                        imgState += "," + actionClicked;
                    }

                    //Replace ,, with ,
                    imgState = imgState.replace(",,", ",");

                    //Bind Error image again
                    //bindErrorImage()

                    //Remove image to display preload unless for big image
                    if (actionClicked !== "large") {
                        $('#chequeImage').attr("src", "")
                    }

                    //Get only unique states for the server
                    var arrayState = _.uniq(imgState.split(","));
                    //If large state found. Draw in modal instead
                    if ($.inArray("large", arrayState) > -1) {
                        var imageHolder = "<img id='bigImageHolder'></img>";
                        bootbox.alert({
                            size: "large",
                            message: imageHolder,
                        });
                        $("#bigImageHolder").parents(".bootbox-body").addClass("image-loader");
                        //$('#bigImageHolder').attr("src", App.ContextPath + "Image/LargeChequeOCS?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());

                        $("#bigImageHolder").parents('.modal-dialog').css({
                            width: 'auto',
                            height: 'auto',
                            'max-height': '80%'
                        });

                        $.ajax({
                            cache: false,
                            url: App.ContextPath + "CommonApi/OCSGetImageByte",
                            async: false,
                            method: "POST",
                            data: "imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join(),
                            success: function (data) {
                                $.each(data, function (i, item) {
                                    $('#bigImageHolder').attr("src", $('#chequeImage').attr("src"));
                                });
                            },
                        });

                    }
                    else {
                        //Draw the image accordingly
                        $("#imageState").val(arrayState.join());

                        $.ajax({
                            cache: false,
                            url: App.ContextPath + "CommonApi/OCSGetImageByte",
                            method: "POST",
                            data: "imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join(),
                            success: function (data) {
                                $.each(data, function (i, item) {
                                    $('#chequeImage').attr("src", item.image);
                                    var imgObj = document.getElementById('chequeImage');
                                    if ($.inArray("invert", arrayState) > -1) {
                                        imgObj.src = invertimg(imgObj);
                                    }
                                    if ($.inArray("rotate", arrayState) > -1) {
                                        imgObj.src = rotateimg(imgObj);
                                    }
                                    //Bind zoomable image 
                                    // extendEzPlusClickFunction("chequeImage", initializeEzPlus);
                                });
                            },
                        });

                    }
                });
            //Trigger first draw of image
            $("#frontBtn").trigger('click');
        }

        function rotateimg(imgObj) {
            var canvas = document.createElement('canvas');
            var ctx = canvas.getContext('2d');

            canvas.width = imgObj.width;
            canvas.height = imgObj.height;

            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.translate(imgObj.width / 2, imgObj.height / 2);
            ctx.rotate(180 * Math.PI / 180);
            ctx.drawImage(imgObj, -imgObj.width / 2, -imgObj.height / 2, imgObj.width, imgObj.height);
            return canvas.toDataURL();
        }

        function invertimg(imgObj) {
            var canvas = document.createElement('canvas');
            var canvasContext = canvas.getContext('2d');

            var imgW = imgObj.width;
            var imgH = imgObj.height;
            canvas.width = imgW;
            canvas.height = imgH;

            canvasContext.drawImage(imgObj, 0, 0, imgW, imgH);
            var imgData = canvasContext.getImageData(0, 0, imgW, imgH);

            // invert colors
            var i;
            for (i = 0; i < imgData.data.length; i += 4) {
                imgData.data[i] = 255 - imgData.data[i];
                imgData.data[i + 1] = 255 - imgData.data[i + 1];
                imgData.data[i + 2] = 255 - imgData.data[i + 2];
                imgData.data[i + 3] = 255;
            }
            canvasContext.putImageData(imgData, 0, 0, 0, 0, imgData.width, imgData.height);
            return canvas.toDataURL();
        }

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
    });
})();