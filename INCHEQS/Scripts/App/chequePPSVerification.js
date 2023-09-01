var ez;
(function () {
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////COMMON BINDING
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    function bindShortCutKey() {
        $(document).off("keypress").on("keypress", function (e) {
            //alert(e.keyCode);
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
      //              case 39 : $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
      //              case 37 : $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
      //              case 113 : $("#approveBtn").trigger('click'); break; // Keyboard-F2
      //              case 82 : $("#returnBtn").trigger('click'); break; // Keyboard-R
      //              case 66 : $("#routeBtn").trigger('click'); break; // Keyboard-B
      //              case 80: $("#pulloutReasonBtn").trigger('click'); break; // Keyboard-P
		    //case 114: $("#returnBtn").trigger('click'); break; // Keyboard-r
      //              case 98: $("#routeBtn").trigger('click'); break; // Keyboard-b
      //              case 112: $("#pulloutReasonBtn").trigger('click'); break; // Keyboard-p
                    case 27 : $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                }
            }
        })
$(document).off("keyup").on("keyup", function (e) {
            //alert(e.keyCode);
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
                    //case 39 : $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                    //case 37 : $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                    //case 113 : $("#approveBtn").trigger('click'); break; // Keyboard-F2

                    case 27 : $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                }
            }
        })
        
    }


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
        var delimiter = clearDate.substring(clearDate.length - 4,clearDate.length - 5);
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
                    $("#imageState").val(arrayState.join());
                    $('#chequeImage').attr("src", App.ContextPath + "Image/Cheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    //Bind zoomable image 
                    $("#chequeImage").off("click.cheque").on("click.cheque", function (e) {
                        extendEzPlusClickFunction("chequeImage", initializeEzPlus)
                    });                }
            });
        //Trigger first draw of image
        $("#frontBtn").trigger('click');
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
        //debugger

        var $holder = $("#signatureGallery");
        var $holderRules = $("#signatureRulesTable tbody");


        $("#searchSignature", document).off("click.g").on("click.g", function (e) {
            console.log("trigger click")
            //    alert("123");
            $("#signatureImage-zoomContainer, #signatureImage").remove();
            $("#signatureGallery,#signatureRulesTable tbody,#signatureSelectedRulesTable tbody, #signatureResult").empty().delay(500);

            drawSignatureInformationTable($("#searchAccountNumber").val(), $holder);
            drawSignatureRulesTable($("#searchAccountNumber").val(), $holderRules);
            drawSignatureInformation($("#searchAccountNumber").val());
            drawSignatureRules($("#searchAccountNumber").val());
        })

        if ($holder.length > 0) {
            isSDSServerAvailable(function (result) {
                //solozzzheelooooo
                if (result) {
                    //  $("#searchAccountNumber").val($("input[name='current_fldAccountNumber']").val())

                    $("#searchAccountNumber").val($("input[name='current_fldHostAccountNo']").val())
                    //drawSignatureInformationTable($("#searchAccountNumber").val(), $("#searchimageNo").val(), $holder);
                    //drawSignatureInformationTable($("#searchAccountNumber").val(), $holder);
                    //drawSignatureRulesTable($("#searchAccountNumber").val(), $holderRules);
                    //drawSignatureRules($("#searchAccountNumber").val());
                    $('#searchSignature').trigger("click");


                } else {
                    $holder.append("Signature Database is Offline??");
                    $("#remarkField").val("Signature Database is Offline")
                }
            });
        }
    }

    //whelloworldz
    function LoadImageInfo(AccNo, ImageNo) {
        //debugger
        var strHtml = "";
        var accNo = AccNo;
        var ImageNo = ImageNo;
        $.ajax({
            async: false,
            cache: false,
            url: App.ContextPath + "Signature/GetImageInfoList",
            method: "POST",
            data: "AccNo=" + accNo + "&ImageNo=" + ImageNo,
            success: function (data) {
                $.each(data, function (i, item) {
                    //img_accNo = data[9].accountNo.trim();
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>ImageNo : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.imageNo + "</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Group : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.sigGroup + "</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Name : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.imageDesc + "</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>ID : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.imageId + "</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Nationality : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.Nationality + "</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Status : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.imageStatus + "</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Relationship : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.Relation + "</span> </p> ";
                });
            }
        });
        return strHtml;
    }
    function drawSignatureInformationTable(accountNo, $holder) {
        //alert('bellaaa');
        //alert(accountNo);
        $.ajax({
            cache: false,
            type: "POST",
            url: App.ContextPath + "Signature/List",
            data: "accountNo=" + accountNo,
            beforeSend: function () {
                $('.signature-loader').removeClass("hidden")
            },

            success: function (data) {
                $('.signature-loader').addClass("hidden")
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
                        var imageHTML = " <div style='width: 100%; height: 100%;'> " +
                            "<img id='signatureImage' class='signature - actual img - responsive' src='" + largeImage + "' />" +
                            LoadImageInfo(accountNo, imageNo);
                        "<div/>";
                        $("#signatureInfo").html(imageHTML);

                        $("#signatureImage").off("click.cheque").on("click.cheque", function (e) {
                            extendEzPlusClickFunction("signatureImage", initializeEzPlus)
                        });

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
        //"<input type='checkbox' name='signatureBox' value='<%= i.imageNo %>'>",
        //"</div>",
        "<div class='pull-left signature-thumb'>",
        "<a class='signature-gallery-nav' ",
        "data-zoom-image='<%= App.ContextPath %>Signature/Large?imageId=<%=i.imageId%>&imageNo=<%=i.imageNo%> ' >",
        //"&data-zoom-imageNo='imageNo=<%=i.imageNo%>'  >",
        "<img class='gallery-thumb' src='<%= App.ContextPath %>Signature/Thumb?imageId=<%=i.imageId%>&imageNo=<%=i.imageNo%>' width='140' height='75'/>",
        "</a>",
        "</div>",
        "<div style='clear:both'></div>",
        "</div>",
        //"<span style='margin-left:25px'>Group : </span> ",
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

    function drawSignatureRulesTable(accountNo, $holder) {
        //Call ajax and draw table
        //Return JSON
        $.ajax({
            cache: false,
            type: "POST",
            url: App.ContextPath + "Signature/RulesList",
            data: "accountNo=" + accountNo,
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

    //signature Rules result table
    var signatureRulesInfoTemplate = [
        "<% _.forEach(data, function(i) { %>",
        //"<% var status = i.status.trim().toString(); %>",
        //"<% var signatureStatus = ''; %>",
        //"<% if (status == 'S'){ %>",
        //"<% signatureStatus = 'Suspended'; %>",
        //"<% } %>",
        //"<% else if (status == 'U'){ %>",
        //"<% signatureStatus = 'Unvalidated'; %>",
        //"<% } %>",
        //"<% else if (status == 'V'){ %>",
        //"<% signatureStatus = 'Validated'; %>",
        //"<% } %>",
        //"<%  else if (status == 'C'){ %>",
        //"<% signatureStatus = 'Closed/Cancel'; %>",
        //"<% } %>",
        //"<%  else if (status == 'VC'){ %>",
        //"<% signatureStatus = 'Validated-Pending for Closure'; %>",
        //"<% } %>",
        //"<%  else if (status == 'UC'){ %>",
        //"<% signatureStatus = 'Unvalidated-Pending for Closure'; %>",
        //"<% } %>",
        //"<%  else if (status == 'VS'){ %>",
        //"<% signatureStatus = 'Validated-Pending for Suspended'; %>",
        //"<% } %>",
        //"<%  else if (status == 'US'){ %>",
        //"<% signatureStatus = 'Unvalidated-Pending for Suspended'; %>",
        //"<% } %>",
        "<tr><th class='col-sm-4'>Name of Account</th><td colspan='3'> <textarea style='height: 45px; width: 488px' maxlength='250' onfocus='this.blur' value='<%= i.accName %>' readonly> <%= i.accName %> </textarea></td></tr>",
        "<tr><th class='col-sm-4'>Account No</th><td> <input type='text' value='<%= i.accNo %>' readonly> </td>",
        "<th class='col-sm-2'>Status</th><td> <input type='text' style='width:auto' value='<%= i.signatureStatus %>' readonly></td>",
        "</tr>",
        "<% }) %>"
    ].join("\n");

    //signature Rules result table
    var signatureRulesTableTemplate = [
        "<thead><tr><th colspan='2' style = 'font-weight: bold;'>SIG CONDITIONS</th><th style = 'font-weight: bold;'>MIN (RM)</th><th style = 'font-weight: bold;'>MAX (RM)</th></tr>",
        "<% var TmpRuleNo = 0; %>",
        "<% _.forEach(data, function(i) { %>",
        "<% if (TmpRuleNo != i.realRuleNo) { %>",
        "<tr><td style = 'font-weight: bold;'> RULE <%=i.RuleNo%>,</td>",
        "<td colspan='2' style = 'font-weight: bold;'>EFFECTIVE DATE</td><td style = 'font-weight: bold;'><%=i.AccEffective%></td>",
        "</tr> ",
        "<% } %>",
        "<tr><td colspan = '2' style = 'font-weight: bold;'><%=i.totalReq%> OF <%=i.groupName%></td><td style = 'font-weight: bold;'><%=i.minAmount%></td><td style = 'font-weight: bold;'><%=i.amount%></td></tr>",
        "<% TmpRuleNo = i.realRuleNo; }) %>",
        "</thead>"
    ].join("\n");

    //signature Rules result
    var signatureRulesTemplate = [
        "<% _.forEach(data, function(i) { %>",
        "<tr>",
        "<td><%=i.condition%></td>",
        "<td><%=i.fldRequireSigNo%></td>",
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
        initEzPlus(selector, {
            constrainSize: 500
        });
    }

    function FindPayee() {

        var $form = $("#searchForm");
        var $holder = $("#tblFindPayee tbody");

        $.ajax({
            cache: false,
            type: "POST",
            url: App.ContextPath + "PPS/Verification/Find",
            data: $form.serialize(),

            success: function (data) {

                $holder.empty().append(_.template(findPayeeTemplate)({ data: data }));
                //compareCellValues();
            }
        });
    }

    var findPayeeTemplate = [

        "<% _.forEach(data, function(i) { %>",
        "<tr>",
        "<td><%=i.fldAccountNo %></td>",
        "<td><%=i.fldChequeNo %></td>",
        "<td id='fldPayeeName'><%=i.fldPayeeName %></td>",
        "<td><%=i.fldAmount%></td>",
        "<td id='fldIssueDate'><%=i.fldIssueDate %></td>",
        "<td><%=i.fldTransCode %></td>",
        "<td id='Status'><%=i.fldStatus %></td>",
        "<td class='txtRed'><%=i.fldHostStatus %></td>",
        "<td><%=i.fldValid %></td>",
        "</tr>",
        "<% }) %>"
    ].join("\n");

    function compareCellValues() {
        var row = $("#tblFindPayee tr").length;
        if (row > 1) {
            
            var ocrPayeeInfo = document.getElementById("chequeInfo").rows[1].cells.item(2).innerHTML;
            var chequePayeeInfo = document.getElementById("tblFindPayee").rows[1].cells.item(2).innerHTML;
            chequePayeeInfo = chequePayeeInfo.replace(" ", "");
            chequePayeeInfo = chequePayeeInfo.replace(" ", "");
            var ocrDateInfo = document.getElementById("chequeInfo").rows[1].cells.item(4).innerHTML;
            var chequeDateInfo = document.getElementById("tblFindPayee").rows[1].cells.item(4).innerHTML;

            var chequeStatusInfo = document.getElementById("tblFindPayee").rows[1].cells.item(6).innerHTML;
            //alert(ocrPayeeInfo);
            //alert(chequePayeeInfo);
            //alert(ocrDateInfo);
            //alert(chequeDateInfo);
            //alert(chequeStatusInfo);
            if (ocrPayeeInfo != chequePayeeInfo) {
                $('#fldPayeeName').css({
                    'background-color': '#FFB6C1'
                });
            }

            if (ocrDateInfo != chequeDateInfo) {
                $('#fldIssueDate').css({
                    'background-color': '#FFB6C1'
                });
            }

            if (chequeStatusInfo != "U") {
                $('#Status').css({
                    'background-color': '#FFB6C1'
                });
            }
        }

    }

    function checkChequeStatus() {
        var chequeStatusInfo = document.getElementById("tblFindPayee").rows[1].cells.item(6).innerHTML;
        //alert(chequeStatusInfo);
        if (chequeStatusInfo == "P") {
            //alert("test");
            if (confirm("This Cheque is already paid.\n Do you still want to be proceed?") == true) {

            } else {
                $("#approveBtn").prop("disabled", true).css({'color': 'Silver'});
                $("#returnBtn").prop("disabled", true).css({ 'color': 'Silver' });
                $("#routeBtn").prop("disabled", true).css({ 'color': 'Silver' });
            }

        }
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////INITIATE ALL FUNCTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    $(document).ready(function () {
        bindAndDrawImageController();
        bindCloseButton();
        bindShortCutKey();
        bindOriginalTotalRecord();
        bindStaleDate();
        bindErrorImage();

        FindPayee();
        
        //Draw signature halt other process while connecting to db. 
        //Delay the process to give way to cheque drawing
        setTimeout(function () {
            drawMainSignatureInformation();
        }, 800);

        $("#findBtn").click(function () {
            FindPayee();
        });

        //Flash host Status
        setInterval(function () {
            $(".blink").toggleClass("txtBlue");
        }, 1000);

        $("#approveBtn").click(function () {
            var rowCount = $("#tblFindPayee tr").length;
            if (rowCount > 1) {
                checkChequeStatus();
            }
        });

    });

})();
