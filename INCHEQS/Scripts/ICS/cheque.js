﻿var ez;
(function () {
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////COMMON BINDING
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    // XX START
    function bindShortCutKey() {
        $(document).off("keypress").on("keypress", function (e) {
            //alert(e.keyCode);
            //alert("b");
            //alert($("#rejectCodeText").is(":focus"));

            //console.log(e.keyCode +"+"+ e.shiftKey); // True = Shift key pressed
            if ($(".remarks").is(":focus") || $("#rejectCodeText").is(":focus") || $("#serviceCharge").is(":focus")) {
            }
            else {
                //switch (e.keyCode + "+" + e.shiftKey) {
                //    case 39 + "+true": $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                //    case 37 + "+true": $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                //    case 65 + "+true": $("#approveBtn").trigger('click'); break; // Keyboard-A
                //    case 82 + "+true": $("#returnBtn").trigger('click'); break; // Keyboard-R
                //    case 66 + "+true": $("#routeBtn").trigger('click'); break; // Keyboard-B
                //    case 27 + "+true": $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                //}
                switch (e.keyCode) {
                    //case 39: $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                    //case 37: $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                    //case 113: $("#approveBtn").trigger('click'); break; // Keyboard-F2
                    //case 82: $("#returnBtn").trigger('click'); break; // Keyboard-R
                    //case 66: $("#routeBtn").trigger('click'); break; // Keyboard-B
                    //case 80: $("#pulloutReasonBtn").trigger('click'); break; // Keyboard-P
                    //case 114: $("#returnBtn").trigger('click'); break; // Keyboard-r
                    //case 98: $("#routeBtn").trigger('click'); break; // Keyboard-b
                    //case 112: $("#pulloutReasonBtn").trigger('click'); break; // Keyboard-p
                    case 27: $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                    case 13: $("#confirmBtn").trigger('click'); break; // Keyboard-Enter

                }
            }
        })
        $(document).off("keyup").on("keyup", function (e) {
            //alert(e.keyCode);
            //alert($("#rejectCodeText").is(":focus"));
            //alert("a");
            //console.log(e.keyCode +"+"+ e.shiftKey); // True = Shift key pressed
            if ($(".remarks").is(":focus") || $("#rejectCodeText").is(":focus") || $("#serviceCharge").is(":focus")) {
            }
            else {
                //switch (e.keyCode + "+" + e.shiftKey) {
                //    case 39 + "+true": $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                //    case 37 + "+true": $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                //    case 65 + "+true": $("#approveBtn").trigger('click'); break; // Keyboard-A
                //    case 82 + "+true": $("#returnBtn").trigger('click'); break; // Keyboard-R
                //    case 66 + "+true": $("#routeBtn").trigger('click'); break; // Keyboard-B
                //    case 27 + "+true": $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                //}
                switch (e.keyCode) {
                    //case 39: $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                    //case 37: $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                    //case 113: $("#approveBtn").trigger('click'); break; // Keyboard-F2
                    case 13: $("#confirmBtn").trigger('click'); break; // Keyboard-Enter
                    case 27: $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                }
            }
        })

    }
    // XX END


    function bindOriginalTotalRecord() {
        if ($("#OriTotalRecordHolder").length > 0) {
            $(".header").append('<input type="hidden" id="OriTotalRecordHolderHeader" value="' + $("#OriTotalRecordHolder").val() + '">')
            $("#OriTotalRecord").text($("#OriTotalRecordHolderHeader").val());
        }
        if ($("#OriTotalRecordHolderHeader").length > 0) {
            $("#OriTotalRecord").text($("#OriTotalRecordHolderHeader").val());
        }
    }

    function bindStaleDate() {
        var clearDate = $(".clear-date").text();
        var delimiter = clearDate.substring(clearDate.length - 4, clearDate.length - 5);
        var staleDate = moment(clearDate, "DD" + delimiter + "MM" + delimiter + "YYYY").add(6, 'M').format("DD" + delimiter + "MM" + delimiter + "YYYY");
        $(".stale-date").text(staleDate);
    }

    function bindErrorImage() {
        //$("img#chequeImage").on('error', function () {
        //    $(this).unbind("error").attr("src", $("#contextPath").html() + 'Content/images/no-cheque.jpg');
        //});
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////CHEQUE IMAGE ACTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    function bindCloseButton() {
        $("#closeBtn").off("click.cheque").on("click.cheque", function (e) {
            e.preventDefault();
            var $form = $(this).closest("form");
            $.ajax({
                type: "POST",
                url: $(this).data("action"),
                data: $form.serialize(),
                success: function (data) {
                    $("#resultContainer").html(data);
                    $("#searchForm").trigger("submit");
                    $(".right-panel").css("background-color", "#ffffff");
                    $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").show();
                }
            })
            //$("#resultContainer").html(data);
            //Remove Original Record Holder
            if ($("#OriTotalRecordHolderHeader").length > 0) {
                $("#OriTotalRecordHolderHeader").remove();
            }
        })
    }

    function bindAndDrawImageController() {
        $(".modify-cheque-btn").off("click.cheque")
            .on("click.cheque", function () {
                var actionClicked = $(this).data("btnfor");
                var imgFolder = $("#imageFolder").val();
                var imgId = $("#imageId").val();
                var imgState = $("#imageState").val();
                if (actionClicked === "reset") {
                    //Reset Button Clicked.
                    //Remove All State
                    imgState = "bw,";
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
                bindErrorImage()

                //Remove image to display preload unless for big image
                if (actionClicked !== "large") {
                    $('#chequeImage').attr("src", "")
                }

                //Get only unique states for the server
                var arrayState = _.uniq(imgState.split(","));
                //If large state found. Draw in modal instead
                if ($.inArray("large", arrayState) > -1) {
                    //alert("d");
                    var imageHolder = "<img id='bigImageHolder'></img>";
                    bootbox.alert({
                        size: "large",
                        message: imageHolder,
                    });
                    $("#bigImageHolder").parents(".bootbox-body").addClass("image-loader");
                    $('#bigImageHolder').attr("src", App.ContextPath + "Image/LargeCheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    $("#bigImageHolder").parents('.modal-dialog').css({
                        width: 'auto',
                        height: 'auto',
                        'max-height': '80%'
                    });
                } else {
                    //Draw the image accordingly
                    //alert("c");
                    $("#imageState").val(arrayState.join());
                    
                    $('#chequeImage').attr("src", App.ContextPath + "Image/Cheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    
                    //Bind zoomable image
                    $("#chequeImage").off("click.cheque").on("click.cheque", function (e) {
                        extendEzPlusClickFunction("chequeImage", initializeEzPlus)
                    });
                }
            });
        //Trigger first draw of image
        $("#frontBtn").trigger('click');
    }
    function bindSSCardZoom() {

        //Bind zoomable image
        $("#SSCard").off("click.cheque").on("click.cheque", function (e) {
            extendEzPlusClickFunction("SSCard", initializeEzPlus)
        });
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


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////CHEQUE DRAWING & SDS : Function for Signature drawing mechanism AND SDS Connection
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    function isSDSServerAvailable(callback) {
        var SDSAjaxRequest = $.ajax({
            type: "POST",
            url: App.ContextPath + "Signature/IsSDSServerConnected",
            async: true,
            beforeSend: function () {
                //Override global setup in AppConfig.js
                $("a,button").off("click.sds").on("click.sds", function () {
                    //console.log(SDSAjaxRequest);
                    if (SDSAjaxRequest != null) {
                        SDSAjaxRequest.abort();
                        SDSAjaxRequest = null;
                    }
                })
            },
            timeout: 3000,
            error: function () {
                callback(false);
            },
            complete: function () {
                //Override global setup in AppConfig.js
                //$('#loading-indicator').hide();
                //$('#content').removeClass("disable-nav");
                //$('#cssmenu ul').removeClass("disable-click");
            },
            success: function (result) {
                callback(result);
            }
        });
    }

    function drawMainSignatureInformation() {
        // debugger

        var $holder = $("#signatureGallery");
        var $holderRules = $("#signatureRulesTable tbody");


        $("#searchSignature", document).off("click.g").on("click.g", function (e) {
            console.log("trigger click")
            //    alert("123");
            $("#signatureImage-zoomContainer, #signatureImage").remove();
            $("#signatureGallery,#signatureRulesTable tbody,#signatureSelectedRulesTable tbody, #signatureResult").empty().delay(500);
            drawSignatureInformationTable($("#searchAccountNumber").val(), $("#searchIssueBankBranch").val(), $holder);
            drawSignatureRulesTable($("#searchAccountNumber").val(), $("#searchIssueBankBranch").val(), $holderRules);
        })

        if ($holder.length > 0) {
            isSDSServerAvailable(function (result) {
                //solozzzheelooooo
                if (result) {
                    //  $("#searchAccountNumber").val($("input[name='current_fldAccountNumber']").val())

                    $("#searchAccountNumber").val($("input[name='current_fldHostAccountNo']").val())
                    $("#searchIssueBankBranch").val($("input[name='current_fldIssueBankBranch']").val())
                    //drawSignatureInformationTable($("#searchAccountNumber").val(), $("#searchimageNo").val(), $holder);
                    //drawSignatureInformationTable($("#searchAccountNumber").val(), $("#searchIssueBankBranch").val(), $holder);
                    //drawSignatureRulesTable($("#searchAccountNumber").val(), $("#searchIssueBankBranch").val(), $holderRules);
                    $('#searchSignature').trigger("click");


                } else {
                    $holder.append("Signature Database is Offline??");
                    $("#remarkField").val("Signature Database is Offline")
                }
            });
        }
    }

    //whelloworldz
    function LoadImageInfo(AccNo, issuingBankBranch, ImageNo) {
        //debugger
        var strHtml = "";
        var accNo = AccNo;
        var ImageNo = ImageNo;
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "Signature/GetImageInfoList",
            method: "POST",
            data: "AccNo=" + accNo + "&issuingBankBranch=" + issuingBankBranch + "&ImageNo=" + ImageNo,
            success: function (data) {
                $.each(data, function (i, item) {
                    //strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; '>Account No : " + item.imageId +" </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; '>Name : " + item.imageDesc + " </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; '>Status : " + item.imageStatus + " </p> ";
                    //strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; '>Effective Date : " + item.AccEffective + " </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; '>Group : " + item.groupName + " </p> ";
                    //strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; '>ID : " + item.imageId +" </p> ";
                    //strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; '>Nationality : " + item.Nationality +" </p> ";
                    //strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; '>Status : " + item.imageStatus +" </p> ";
                    //strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; '>Relationship : " + item.Relation + " </p> ";

                });
            }
        });
        return strHtml;
    }
    function drawSignatureInformationTable(accountNo, issuingBankBranch, $holder) {
        //alert('bellaaa');
        //alert(accountNo);
        //debugger;
        $.ajax({
            cache: false,
            type: "POST",
            url: App.ContextPath + "Signature/List",
            data: "accountNo=" + accountNo + "&issuingBankBranch=" + issuingBankBranch,
            beforeSend: function () {
                $('.signature-loader').removeClass("hidden")
            },

            success: function (data) {
                //$('.signature-loader').addClass("hidden")
                if (data.length == 0) {
                    $holder.html("<span>Signature Not Found</span>");
                    $("#remarkField").val("Signature Not Found")
                    return;
                } else {
                    $holder.append(_.template(signatureTableTemplate)({ data: data }));

                    //Mark checkbox of ticked before based on inwardItemId
                    //markCarryForwardSignature($("#fldInwardItemId").val());

                    //When signature is onlick, swap image in mainSignature with the clicked one.
                    $(".signature-gallery-nav").on('click', function (e) {
                        $("#signatureImage-zoomContainer").remove();
                        var largeImage = $(this).data("zoom-image");
                        //let searchParams = new URLSearchParams(largeImage);
                        //var ImageNo1 = searchParams.get('imageNo');
                        $.urlParam = function (name) {
                            var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(largeImage);
                            return results[1] || 0;
                        }
                        var imageNo = $.urlParam('imageNo');
                        //alert("soloz");
                        //alert($.urlParam('imageNo'));
                        //debugger;
                        var imageHTML = " <table>" +
                            "<td><img id='signatureImage' class='signature-actual img-signature-responsive' style='width:250px;height:150px;' src='" + largeImage + "' /></td>" +
                            //"<td>" + LoadImageInfo(accountNo, issuingBankBranch, imageNo) + "</td>" +
                            "</table > ";
                        var signatureInfoDetails = " <table>" +
                            "<td>" + LoadImageInfo(accountNo, issuingBankBranch, imageNo) + "</td>" +
                            "</table> ";
                        $("#signatureInfo").html(imageHTML);
                        $("#signatureInfoDetail").html(signatureInfoDetails);
                        //extendEzPlusClickFunction("signatureImage", initializeEzPlus);
                    })
                }
            }
        });
    }

    var signatureTableTemplate = [
        "<% var counter = 1; %> ",
        "<div id='signatureGallery'>",
        "<% _.forEach(data, function(i) { %>",
        "<div id='signatureItem' class='signatureItem'>",
        "<div>",
        //"<div class='pull-left signature-number'>",
        //"<%= counter++ %> .<br/>",
        "<input type='checkbox' name='signatureBox' value='<%= i.imageNo %>'>",
        //"</div>",
        "<div class='pull-left signature-thumb'>",
        "<a class='signature-gallery-nav' ",
        "data-zoom-image='<%= App.ContextPath %>Signature/Large?imageId=<%=i.imageId%>&imageNo=<%=i.imageNo%> ' >",
        //"&data-zoom-imageNo='imageNo=<%=i.imageNo%>'  >",
        "<img class='gallery-thumb' src='<%= App.ContextPath %>Signature/Thumb?imageId=<%=i.imageId%>&imageNo=<%=i.imageNo%>' />",
        "</a>",
        "</div>",
        "<div style='clear:both'></div>",
        "</div>",
        "<span style='margin-left:25px'>Group : </span> ",
        " <span id = 'signatureGroup_<%= counter-1 %>' class= 'signatureGroup' ><%= i.sigGroup %></span > ",
        "<input type='hidden' id='signatureDesc_<%= counter-1 %>' class='signatureDesc' value='<%= i.imageDesc %>'>",
        "<input type='hidden' id='signatureCounter_<%= counter-1 %>' class='signatureCounter' value='<%= counter-1 %>'>",
        "</div>",
        "<% }) %>",
        "</div>",
        "<div style='clear:both'></div>"

    ].join("\n");

    function markCarryForwardSignature(itemId) {
        var $form = $("#searchForm");

        //Call stored procs for validate
        $.ajax({
            cache: false,
            type: "POST",
            url: App.ContextPath + "Signature/GetCheckedSignature",
            data: "itemId=" + itemId,
            success: function (data) {
                $.each(data, function (i, item) {
                    //console.log(item.checkedSignature);
                    $("input[value=" + item.checkedSignature + "]").prop("checked", "checked")
                })
                //Trigger validation on success load
                signatureValidateSubmit();
            }
        });

        return false;
    }

    function drawSignatureRules(accountNo) {
        //Call ajax and draw table
        //Return JSON
        $.ajax({
            cache: false,
            type: "POST",
            url: App.ContextPath + "Signature/GetSignatureRulesInfo",
            data: "AccNo=" + accountNo,
            success: function (data) {
                //Draw Rules using template
                $('#signatureRules2').empty().append(_.template(signatureRulesTableTemplate)({ data: data }));
                //Trigger validation on change
            }
        });
    }

    function drawSignatureInformation(accountNo) {
        //Call ajax and draw table
        //Return JSON
        $.ajax({
            cache: false,
            type: "POST",
            url: App.ContextPath + "Signature/GetSignatureInformation",
            data: "AccNo=" + accountNo,
            success: function (data) {
                //Draw Rules using template
                $('#signatureInfo2').empty().append(_.template(signatureRulesInfoTemplate)({ data: data }));
                //Trigger validation on change
            }
        });
    }

    function drawSignatureRulesTable(accountNo, issuingBankBranch, $holder) {
        //debugger;
        //Call ajax and draw table
        //Return JSON
        $.ajax({
            cache: false,
            type: "POST",
            url: App.ContextPath + "Signature/RulesList",
            data: "accountNo=" + accountNo + "&issuingBankBranch=" + issuingBankBranch,
            beforeSend: function () {
                $('.rules-loader').removeClass("hidden")
            },
            success: function (data) {
                $('.rules-loader').addClass("hidden")
                //Draw Rules using template
                $holder.append(_.template(signatureRulesTemplate)({ data: data }));
                //Trigger validation on change
                $("#signatureGallery input[type=checkbox]").on("change", function (e) {
                    signatureValidateSubmit();
                })
            }
        });
    }

    //signature Rules result
    var signatureRulesTemplate = [
        "<% _.forEach(data, function(i) { %>",
        "<tr>",
        "<td><%=i.condition%></td>",
        //"<td><%=i.fldRequireSigNo%></td>",
        "<td><%=i.sigGroup%></td>",
        "<td><%=i.fldSigAmountFrom%> to <%=i.fldSigAmountLimit%></td>", ,
        "<td><%=i.fldSigValidTo%></td>",
        "</tr>",
        "<% }) %>"
    ].join("\n");

    function signatureValidateSubmit() {
        $("#signatureSelectedRulesTable tbody tr").remove()
        var $form = $("#searchForm");
        $checkedCheckBoxes = $("#signatureGallery input[type=checkbox]:checked");
        if ($checkedCheckBoxes.length > 0) {
            //Call stored procs for validate
            $.ajax({
                cache: false,
                type: "POST",
                url: App.ContextPath + "Signature/ValidateSignature",
                data: $form.serialize(),
                beforeSend: function () {
                    //  $("#signatureResult").text("Validating...");
                },
                success: function (data) {
                    //$('.rules-loader').addClass("hidden")
                    if (data.redirect == undefined) {
                        if (data == "UNMATCH") {
                            $("#signatureResult").html("<span class='txtRed'>UNMATCH</span>");
                        } else if (data == "MATCH") {
                            $("#signatureResult").html("<span class='txtGreen'>MATCH</span>");
                        } else {
                            $("#signatureResult").html("");
                        }

                        $checkedCheckBoxes.each(function (e) {
                            $sigGroup = $(this).parents(".signatureItem").children(".signatureGroup").text();
                            $sigDesc = $(this).parents(".signatureItem").children(".signatureDesc").val();
                            $sigNo = $(this).parents(".signatureItem").children(".signatureCounter").val();
                            $("#signatureSelectedRulesTable tbody").append("<tr> <td>" + $sigNo + "</td> <td>" + $sigGroup + "</td> <td>" + $sigDesc + "</td> </tr>")
                        })
                    }
                    return false;
                }
            });
        } else {
            $checkedCheckBoxes = $("#signatureGallery input[type=checkbox]:checked");
            if ($checkedCheckBoxes.length > 0) {
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: App.ContextPath + "Signature/DeleteAllSignatureHistory",
                    data: $form.serialize(),
                    beforeSend: function () {
                        //    $("#signatureResult").text("Validating...");
                    },
                    success: function (data) {
                        if (data == "UNMATCH") {
                            $("#signatureResult").html("<span class='txtRed'>UNMATCH</span>");
                        } else if (data == "MATCH") {
                            $("#signatureResult").html("<span class='txtGreen'>MATCH</span>");
                        } else {
                            $("#signatureResult").html("");
                        }
                    }
                })
            }
            else {
                $("#signatureResult").html("");
            }
        }
        return false;
    };

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////EZ PLUS ZOOM PLUGINS BINDER
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    function extendEzPlusClickFunction(selector, callback) {
        callback("#" + selector);
        var ez = $("#" + selector).data('ezPlus');
        //alert(selector);
        $("#" + selector).off("click.cheque").on("click.cheque", function (e) {
            //alert("a");
            var ez = $("#" + selector).data('ezPlus');
            if (ez) {
                //alert("b");
                var zoomId = ez.options.zoomId;
                if ($("#" + selector + "-zoomLens").length > 0) $("#" + selector + "-zoomLens").remove();
                var $zoomLens = $(".zoomContainer[uuid='" + zoomId + "']")
                    .clone()
                    .attr("id", selector + "-zoomLens")
                    .appendTo("#resultContainer").click(function () {
                        $(this).remove();
                        callback("#" + selector);
                    });
                ez.destroy();
                $("#" + selector + "-zoomLens").remove();
                $("#" + selector).off("click.cheque").on("click.cheque", function (e) {
                    extendEzPlusClickFunction(selector, callback)
                });
            }
        });
    }

    function initEzPlus(selector, options) {
        var ez = $(selector).data('ezPlus');
        //alert("b");
        if (ez) ez.destroy();
        if ($(selector + "-zoomLens").length > 0) $(selector + "-zoomLens").remove();
        _.merge(options, {
            zoomType: 'lens',
            zoomContainerAppendTo: "#resultContainer",
            cursor: 'pointer',
            scrollZoom: true
        });
        $(selector).ezPlus(options);
    }

    function initializeEzPlus(selector) {
            //alert("b");
            initEzPlus(selector, {
                constrainSize: 500
            });
    }



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////INITIATE ALL FUNCTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    $(document).ready(function () {
        bindAndDrawImageController();

        bindAndDrawImageController();
        bindCloseButton();
        bindShortCutKey();
        bindOriginalTotalRecord();
        bindStaleDate();
        bindErrorImage();
        bindSSCardZoom();
        //Draw signature halt other process while connecting to db. 
        //Delay the process to give way to cheque drawing


        //setTimeout(function () {
            drawMainSignatureInformation();
        //}, 800);

        //Flash host Status
        setInterval(function () {
            $(".blink").toggleClass("txtBlue");
        }, 500);


    });

})();
