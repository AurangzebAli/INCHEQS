var ez;
(function () {
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////COMMON BINDING
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    function bindShortCutKey() {
        $(document).off("keyup").on("keyup", function (e) {
            //console.log(e.keyCode +"+"+ e.shiftKey); // True = Shift key pressed
            if ($(".remarks").is(":focus") || $("#rejectCodeText").is(":focus")) {
            } else {
                switch (e.keyCode + "+" + e.shiftKey) {
                    case 39 + "+true": $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                    case 37 + "+true": $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                    case 65 + "+true": $("#approveBtn").trigger('click'); break; // Keyboard-A
                    case 82 + "+true": $("#returnBtn").trigger('click'); break; // Keyboard-R
                    case 66 + "+true": $("#routeBtn").trigger('click'); break; // Keyboard-B
                    case 27 + "+true": $("#closeBtn").trigger('click'); break; // Keyboard-ESC
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
        var staleDate = moment(clearDate, "DD" + delimiter + "MM" + delimiter + "YYYY").add(-179, 'days').format("DD" + delimiter + "MM" + delimiter + "YYYY");
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
                    $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").show();
                   
                 
                }
            })
            //$("#resultContainer").html(data);
            //Remove Original Record Holder
            if ($("#OriTotalRecordHolderHeader").length > 0) {
                $("#OriTotalRecordHolderHeader").remove();
            }
        })

        $("#closeBtnCTS").off("click.cheque").on("click.cheque", function (e) {

            e.preventDefault();
            var $form = $(this).closest("form");
            
            
            $.ajax({
                type: "POST",
                url: $(this).data("action"),
                data: $form.serialize(),
                success: function (data) {
                //    $("#formABC").attr("disabled", true);
                //    $(".formABC").attr("disabled", true);
                //    $("#formABC :input").prop("disabled", true);
                //    $(".formABC :input").prop("disabled", true);
                    $("#resultContainer").html(data);
                    $("#searchForm").trigger("submit");
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
    function bindPrintButton() {
        $("#printBtn").off("click.cheque").on("click.cheque", function (e) {
            e.preventDefault();
            var $form = $(this).closest("form");
           
            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            
            //var prevTarget = "_blank";
            //var prevAction = "/CTS/TransactionEnquiry/TransactionEnquiryRetrieverPage";
            var targetAction = $(this).data("action");


            //alert(prevTarget);
            //alert(prevAction);
            //alert(targetAction);


            $.ajax({
                type: "POST",
                url: targetAction,
                data: $form.serialize(),
                success: function (data) {
                    $form.attr("target", "_blank");
                    $form.attr("action", targetAction);
                    

                    $form.off("submit");
                    $form.submit();

                    //bindSecureForm();

                    //$form.attr("target", prevTarget);
                    //$form.attr("action", prevAction);
                    $form.attr("target", "_blank");
                    $form.attr("action", "/CTS/TransactionEnquiry/TransactionEnquiryRetrieverPage");


                    
                }
            })

        })

    }
    //function bindCTSPrintButton() {
    //    $("#printBtn").off("click.cheque").on("click.cheque", function (e) {
    //        e.preventDefault();
    //        var $form = $(this).closest("form");
    //        $.ajax({
    //            type: "POST",
    //            url: $(this).data("action"),
    //            data: $form.serialize(),
    //            success: function (data) {
    //                $("#resultContainer").html(data);
    //                $("#searchForm").trigger("submit");
    //                $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").show();
    //            }
    //        })
    //        //$("#resultContainer").html(data);
    //        //Remove Original Record Holder
    //        if ($("#OriTotalRecordHolderHeader").length > 0) {
    //            $("#OriTotalRecordHolderHeader").remove();
    //        }
    //    })
    //}

    function bindAndDrawImageController() {
      
        $(".modify-cheque-btn").off("click.cheque")
            .on("click.cheque", function () {
               
                var actionClicked = $(this).data("btnfor");
                var imgFolder = $("#imageFolder").val();
                var imgId = $("#imageId").val();
                //alert(imgId);
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
                    //alert("2");
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
                  
                    //$('#chequeImage').attr("src", App.ContextPath + "Image/Cheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    $('#chequeImage').attr("src", App.ContextPath + "Image/Cheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    //alert(App.ContextPath + "Image/Cheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    //Bind zoomable image 
                    extendEzPlusClickFunction("chequeImage", initializeEzPlus);
					}
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
        var $holder = $("#signatureGallery");
        var $holderRules = $("#signatureRulesTable tbody");

        //$("#searchSignature", document).off("click.g").on("click.g", function (e) {
        //    console.log("trigger click")
        //    alert("123");
        //    $("#signatureImage-zoomContainer, #signatureImage").remove();
        //    $("#signatureGallery,#signatureRulesTable tbody,#signatureSelectedRulesTable tbody, #signatureResult").empty().delay(500);
        //    drawSignatureInformationTable($("#searchAccountNumber").val(), $holder);
        //    drawSignatureRulesTable($("#searchAccountNumber").val(), $holderRules);
        //})

        if ($holder.length > 0) {
            isSDSServerAvailable(function (result) {
                //if (result) {
              //  $("#searchAccountNumber").val($("input[name='current_fldAccountNumber']").val())
                $("#searchAccountNumber").val($("input[name='current_fldHostAccountNo']").val())
                    drawSignatureInformationTable($("#searchAccountNumber").val(), $holder);
                    drawSignatureRulesTable($("#searchAccountNumber").val(), $holderRules);
                    $('#searchSignature').trigger("click");

                //} else {
               //     $holder.append("Signature Database is Offline");
             //       $("#remarkField").val("Signature Database is Offline")
           //     }
            });
        }
    }

    function drawSignatureInformationTable(accountNo, $holder) {
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
                        $("#signatureInfo").html("<img id='signatureImage' class='signature-actual img-responsive' src='" + largeImage + "' />");
                        extendEzPlusClickFunction("signatureImage", initializeEzPlus);
                    })
                }
            }
        });
    }

    var signatureTableTemplate = [
        "<% var counter = 1; %> ",
        "<div id='signatureGallery'>",
            "<% _.forEach(data, function(i) { %>",
                "<div id='signatureItem'>",
                    "<div>",
                        "<div class='pull-left signature-number'>",
                            "<%= counter++ %> .<br/>",
                            "<input type='checkbox' name='signatureBox' value='<%= i.imageNo %>'>",
                        "</div>",
                        "<div class='pull-left signature-thumb'>",
                            "<a class='signature-gallery-nav' ",
                            "data-zoom-image='<%= App.ContextPath %>Signature/Large?imageId=<%=i.imageId%>&imageNo=<%=i.imageNo%>' >",
                                "<img class='gallery-thumb' src='<%= App.ContextPath %>Signature/Thumb?imageId=<%=i.imageId%>&imageNo=<%=i.imageNo%>' />",
                            "</a>",
                        "</div>",
                        "<div style='clear:both'></div>",
                    "</div>",
                    "<span style='margin-left:25px'>Group : </span><span id='signatureGroup'><%= i.sigGroup %></span>",
                    "<input type='hidden' id='signatureDesc' value='<%= i.imageDesc %>'>",
                    "<input type='hidden' id='signatureCounter' value='<%= counter-1 %>'>",
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

    var signatureRulesTemplate = [
        "<% _.forEach(data, function(i) { %>",
            "<tr>",
                 "<td><%=i.accountName%></td>",
                "<td><%=i.accountType%></td>",
                "<td><%=i.accountStatus%></td>",
                "<td><%=i.condition%></td>",
                 "<td><%=i.ProductTypeCode%></td>",
                "<td><%=i.AccEffective%> to <%=i.AccExpiry%></td>",
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
                //        if (data == "UNMATCH") {
                //            $("#signatureResult").html("<span class='txtRed'>UNMATCH</span>");
                //        } else if (data == "MATCH") {
                //            $("#signatureResult").html("<span class='txtGreen'>MATCH</span>");
                //        } else {
                //            $("#signatureResult").html("");
                //        }
                        $checkedCheckBoxes.each(function (e) {
                            $sigGroup = $(this).parents("#signatureItem").children("#signatureGroup").text();
                            $sigDesc = $(this).parents("#signatureItem").children("#signatureDesc").val();
                            //$sigNo = $(this).parents("#signatureItem").children("#signatureCounter").val();
                            $("#signatureSelectedRulesTable tbody").append("<tr> <td>" + $sigGroup + "</td> <td>" + $sigDesc + "</td> </tr>")
                        })
                    }
                    return false;
                }
            });
        } else {
            $.ajax({
                cache: false,
                type: "POST",
                url: App.ContextPath + "Signature/DeleteAllSignatureHistory",
                data: $form.serialize(),
                beforeSend: function () {
                //    $("#signatureResult").text("Validating...");
                },
                success: function (data) {
                    //if (data == "UNMATCH") {
                    //    $("#signatureResult").html("<span class='txtRed'>UNMATCH</span>");
                    //} else if (data == "MATCH") {
                    //    $("#signatureResult").html("<span class='txtGreen'>MATCH</span>");
                    //} else {
                    //    $("#signatureResult").html("");
                    //}
                }
            })
        }
        return false;
    };

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////EZ PLUS ZOOM PLUGINS BINDER
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    function extendEzPlusClickFunction(selector, callback) {
        callback("#" + selector);
        $("#" + selector).off("click.cheque").on("click.cheque", function (e) {
            var ez = $("#" + selector).data('ezPlus');
            if (ez) {
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



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////INITIATE ALL FUNCTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    $(document).ready(function () {
        bindAndDrawImageController();
        bindCloseButton();
        bindPrintButton();
        bindShortCutKey();
        bindOriginalTotalRecord();
        bindStaleDate();
        bindErrorImage();

        //Draw signature halt other process while connecting to db. 
        //Delay the process to give way to cheque drawing
        setTimeout(function () {
            drawMainSignatureInformation();
        }, 800);

        //Flash host Status
        setInterval(function () {
            $(".blink").toggleClass("txtBlue");
        }, 500);


    });

})();