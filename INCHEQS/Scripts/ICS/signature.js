var ez;
(function () {
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
            //console.log("trigger click")
            //alert("123");
            var hostNo;

            hostNo = $("#searchAccountNumber").val().trim();

            if (hostNo.length == 10) {
                hostNo = "10" + hostNo;
            }
            /*else
            {
                hostNo
            }*/

            $("#signatureImage-zoomContainer, #signatureImage").remove();
            $("#signatureGallery,#signatureRulesTable tbody,#signatureSelectedRulesTable tbody, #signatureResult").empty();//.delay(500);

            drawSignatureInformationTable(hostNo, $holder);
            drawSignatureRulesTable(hostNo, $holderRules);
            drawSignatureInformation(hostNo);
            drawSignatureRules(hostNo);
            
        })

        if ($holder.length > 0) {
            isSDSServerAvailable(function (result) {
                //solozzzheelooooo
                if (result) {
                //  $("#searchAccountNumber").val($("input[name='current_fldAccountNumber']").val())
                    $("#signatureGallery span").last().remove();

                //$("#searchAccountNumber").val($("input[name='current_fldHostAccountNo']").val())
                //drawSignatureInformationTable($("#searchAccountNumber").val(), $("#searchimageNo").val(), $holder);
                //drawSignatureInformationTable($("#searchAccountNumber").val(), $holder);
                    //drawSignatureRulesTable($("#searchAccountNumber").val(), $holderRules);
                    //drawSignatureRules($("#searchAccountNumber").val());
                //$('#searchSignature').trigger("click");
                }
                else {
                     $holder.append("<span class='bold red'>Signature Database is Offline</span>");
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
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>ImageNo : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.imageNo +"</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Group : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.sigGroup +"</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Name : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.imageDesc +"</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>ID : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.imageId +"</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Nationality : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.Nationality +"</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Status : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.imageStatus +"</span> </p> ";
                    strHtml += "<p style='FONT-WEIGHT: bold; COLOR: #4682b4; margin: 0; padding: 0;'>Relationship : <span style='FONT-WEIGHT: bold; COLOR: #000000;'>" + item.Relation +"</span> </p> ";
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
                    $holder.html("<span class='bold txtBlack'>Signature Not Found</span>");
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
                        /*$.urlParam = function (name) {
                            var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(largeImage);
                            return results[1] || 0;
                        }*/

                        var imageNo = $(this).attr("value").trim();
                        //alert("soloz");
                        //alert($.urlParam('imageNo'));
                        var imageHTML = " <div style='width: 100%; height: 100%;'> " +
                                        "<img id='signatureImage' class='signature - actual img - responsive' src='" + largeImage + "' />" +
                                        LoadImageInfo(accountNo, imageNo);
                                        "<div/>";
                        $("#signatureInfo").html(imageHTML);

                        /*$("#signatureImage").off("click.cheque").on("click.cheque", function (e) {
                            extendEzPlusClickFunction("signatureImage", initializeEzPlus)
                        });*/
                           
                    })

                    //setTimeout(function () {
                        if ($.trim($('#signature1').attr('value')) != '' && $.trim($('#signature1').attr('value')) != null) {
                            $('#signature1').trigger('click');
                        } 

                    //}, 100)

                }
            }
        });
    }

    var signatureTableTemplate = [
        "<% var counter = 1; %> ",
        "<% var item = 1; %> ",
        "<div id='signatureGallery'>",
        "<% _.forEach(data, function(i) { %>",

        "<div id='signatureItem' class='signatureItem'>",
        "<div>",

        "<% if (i.accountStatus == 'C' ) { %>",
        "<span style='color: red; font-weight: bold; font-size: 16px'>*</span>",
        "<% } %>",
        //"<div class='pull-left signature-number'>",
        //"<%= counter++ %> .<br/>",
        //"<input type='checkbox' name='signatureBox' value='<%= i.imageNo %>'>",
        //"</div>",
        "<div class='pull-left signature-thumb'>",
        "<a id='signature<%= item %>' class='signature-gallery-nav' value='<%=i.imageNo%>' ",
        //"data-zoom-image='<%= App.ContextPath %>Signature/Large?imageId=<%=i.imageId%>&imageNo=<%=i.imageNo%> ' >",
        "data-zoom-image='<%=i.ImageCode%>' >",
        //"&data-zoom-imageNo='imageNo=<%=i.imageNo%>'  >",
        //"<img class='gallery-thumb' src='<%= App.ContextPath %>Signature/Thumb?imageId=<%=i.imageId%>&imageNo=<%=i.imageNo%>' width='140' height='75'/>",
        "<img class='gallery-thumb' src='<%=i.ImageCode%>' width='140' height='75'/>",
        "</a>",
        "</div>",
        "<div style='clear:both'></div>",
        "</div>",
        //"<span style='margin-left:25px'>Group : </span> ",
        /*"<span id = 'signatureGroup_<%= counter-1 %>' class= 'signatureGroup' ><%= i.sigGroup %></span > ",
        "<input type='hidden' id='signatureDesc_<%= counter-1 %>' class='signatureDesc' value='<%= i.imageDesc %>'>",
        "<input type='hidden' id='signatureCounter_<%= counter-1 %>' class='signatureCounter' value='<%= counter-1 %>'>",*/
        "</div>",
        "<% item = item + 1; %> ",
        "<% }) %>",
        "</div>",
        "<div style='clear:both'></div>"

    ].join("\n");



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
        "<tr style='background-color:#f8fcfd'><th class='col-sm-4'>Name of Account</th><td colspan='3'> <input style='font-weight: bold; width: 100%; border: 1px solid blue;' maxlength='250' onfocus='this.blur' value='<%= i.accName %>' readonly> </td></tr>",
        "<tr style='background-color:#f8fcfd'><th class='col-sm-4'>Account No</th><td> <input type='text' value='<%= i.accNo %>' style='font-weight: bold;color: black;border: 1px solid blue;' readonly> </td>",
        "<th class='col-sm-2'>Status</th><td><input type='text' style='width:100%;font-weight: bold;color: black;border: 1px solid blue;' value='<%= i.signatureStatus %>' readonly></td>",
        "</tr>",
        "<% }) %>"
    ].join("\n");

    //signature Rules result table
    var signatureRulesTableTemplate = [
        "<% if (data != 0 && data != null && data != '') { %>",
        "<thead><tr style='background-color:#f8fcfd'><th colspan='2' style='font-weight: bold; padding: 2px; border-top: none; border-bottom: none;'>SIG CONDITIONS</th><td style='border-top: 1px solid #f8fcfd;border-bottom: 1px solid #f8fcfd'></td><th style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'>MIN (RM)</th><th style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'>MAX (RM)</th></tr>",
        "<% } %>",
        "<% var TmpRuleNo = 0; %>",
        "<% var SumReqSig = 0; %>",
        "<% _.forEach(data, function(i) { %>",
        "<% if ((TmpRuleNo != i.realRuleNo) && (TmpRuleNo != '') && (TmpRuleNo != 0)) { %>",
        "<tr style='background-color:#f8fcfd'><td style='border-top: 1px solid #f8fcfd'></td>",
        "<td style='border-top: 1px solid #f8fcfd'></td>",
        "<td colspan='2' style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'><%=SumReqSig%> OF REQ SIG</td>",
        "<td style='border-top: 1px solid #f8fcfd'></td>",
        "</tr> ",
        "<% } %>",
        "<% if (TmpRuleNo != i.realRuleNo) { %>",
        "<tr style='background-color:#f8fcfd'><td style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'> RULE <%=i.RuleNo%>,</td>",
        "<td style='border-top: 1px solid #f8fcfd'></td>",
        "<td colspan='2' style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'>EFFECTIVE DATE</td><td style = 'font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'><%=i.AccEffective%></td>",
        "</tr> ",
        "<% } %>",
        "<tr style='background-color:#f8fcfd'>",
        "<td style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'>",
        "<% if (TmpRuleNo != i.realRuleNo) { %>",
        "RULE <%=i.RuleNo %>,",
        "<% } %>",
        "</td> ",
        "<td style='border-top: 1px solid #f8fcfd'></td>",
        "<td colspan='1' style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'><%=i.totalReq%> OF <%=i.groupName%></td>",
        "<td style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'>",
        "<% if (TmpRuleNo != i.realRuleNo) { %>",
        "<%=i.minAmount%>",
        "<% } %>",
        "</td>",
        "<td style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'>",
        "<% if (TmpRuleNo != i.realRuleNo) { %>",
        "<%=i.amount%>",
        "<% } %>",
        "</td>",
        "</tr> ",
        "<% if ((TmpRuleNo != i.realRuleNo) && (TmpRuleNo != '')) { %>",
        "<% SumReqSig = 0; %>",
        "<% } %>",
        "<% SumReqSig = parseInt(SumReqSig) + parseInt(i.totalReq); %>",
        "<% TmpRuleNo = i.realRuleNo; }) %>",
        "<% if (SumReqSig != 0) { %>",
        "<tr style='background-color:#f8fcfd'><td style='border-top: 1px solid #f8fcfd'></td>",
        "<td style='border-top: 1px solid #f8fcfd'></td>",
        "<td colspan='2' style='font-weight: bold;padding: 2px; border-top: none; border-bottom: none;'><%=SumReqSig%> OF REQ SIG</td>",
        "<td style='border-top: 1px solid #f8fcfd'></td>",
        "</tr> ",
        "<% } %>",
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

                },
                success: function (data) {

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
    /////INITIATE ALL FUNCTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    $(document).ready(function () {


        //Draw signature halt other process while connecting to db. 
        //Delay the process to give way to cheque drawing


        //setTimeout(function () {
            drawMainSignatureInformation();
        //}, 800);



    });

})();
