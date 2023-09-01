
(function () {
    $(document).ready(function () {

        var $holderCapturingMode = $("#ajaxCapturingModeList");
        var $holderCapturingType = $("#ajaxCapturingTypeList");
        var $holderCapturingInfo = $("#ajaxCapturingInfoList");
        var $holderPostingInfo = $("#ajaxPostingModeList");
        //  var $holderMacAddress = showMacAddress();

        if ($holderCapturingMode.length > 0) {
            
            var a1 = $.ajax({
                cache: false,
                url: App.ContextPath + "CommonApi/CapturingModeList",
                method: "POST",

            }),


                a2 = a1.then(function (data) {

                    return $.ajax({
                        cache: false,
                        url: App.ContextPath + "CommonApi/CapturingTypeList",
                        method: "POST",
                        data: "id=" + $(':radio[name=optCapturingMode]:checked').val(),
                    });
                });

            a3 = a1.then(function (data) {

                return $.ajax({
                    cache: false,
                    url: App.ContextPath + "CommonApi/PostingInfoList",
                    method: "POST",

                });
            });

            a1.done(function (data) {
                $.each(data, function (i, item) {
                    if (i == 0) {
                        
                        $holderCapturingMode.append("<input type='radio' name='opt" + "CapturingMode" +
                            "' id= 'opt" + item.fldModeId + "' value='" + item.fldModeId + "' " +
                            " onclick='jsCheckCapMode();' checked /> " + item.fldDescription + "<br />");
                    } else {
                        
                        $holderCapturingMode.append("<input type='radio' name='opt" + "CapturingMode" +
                            "' id= 'opt" + item.fldModeId + "' value='" + item.fldModeId + "' " +
                            " onclick='jsCheckCapMode();' /> " + item.fldDescription + "<br />");
                    }
                });
                jsCheckCapMode();
            });

            a1.fail(function () {

            });

            a3.done(function (data) {
                $.each(data, function (i, item) {
                    if (i == 0) {
                        $holderPostingInfo.append("<input type='radio' name='opt" + "PostingMode" +
                            "' id= 'opt" + item.fldTypeId + "' value='" + item.fldTypeId + "' " +
                            " checked /> " + item.fldDescription + "<br />");
                    } else {
                        $holderPostingInfo.append("<input type='radio' name='opt" + "PostingMode" +
                            "' id= 'opt" + item.fldTypeId + "' value='" + item.fldTypeId + "' " +
                            " /> " + item.fldDescription + "<br />");
                    }
                });
            });

            a2.done(function (data) {
                $.each(data, function (i, item) {
                    if (i == 0) {
                        $holderCapturingType.append("<input type='radio' name='opt" + "ChequeType" +
                            "' id= 'opt" + item.fldTypeId + "' value='" + item.fldTypeId + "' " +
                            " checked /> " + item.fldDescription + "<br />");
                    } else {
                        $holderCapturingType.append("<input type='radio' name='opt" + "ChequeType" +
                            "' id= 'opt" + item.fldTypeId + "' value='" + item.fldTypeId + "' " +
                            " /> " + item.fldDescription + "<br />");
                    }
                });

                //$(':radio[name=optCapturingMode]').click(function () {
                //    $.ajax({
                //        cache: false,
                //        url: App.ContextPath + "CommonApi/CapturingTypeList",
                //        method: "POST",
                //        data: "id=" + $(':radio[name=optCapturingMode]:checked').val(),
                //        success: function (data) {
                //            $holderCapturingType.empty();
                //            $.each(data, function (i, item) {
                //                if (i == 0) {
                //                    $holderCapturingType.append("<input type='radio' name='opt" + "ChequeType" +
                //                        "' id= 'opt" + item.fldTypeId + "' value='" + item.fldTypeId + "' " +
                //                        " checked /> " + item.fldDescription + "<br />");
                //                } else {
                //                    $holderCapturingType.append("<input type='radio' name='opt" + "ChequeType" +
                //                        "' id= 'opt" + item.fldTypeId + "' value='" + item.fldTypeId + "' " +
                //                        " /> " + item.fldDescription + "<br />");
                //                }
                //            });
                //        },
                //    });
                //})
            });


        
        }
    });
})();

//----------------------------------------------------------------------------------------------------------
//-------FUNCTIONS
//-----------------

function jsCheckCapMode() {
    var selectedMode = $(':radio[name=optCapturingMode]:checked').val();
    if (selectedMode == "CO" || selectedMode == "IR") {
        $("#radChequeStatus_1").prop("disabled", false);
    }
    else {
        $("#radChequeStatus_0").prop("checked", true);
        $("#radChequeStatus_1").prop("checked", false);
        $("#radChequeStatus_1").prop("disabled", true);
    }
}