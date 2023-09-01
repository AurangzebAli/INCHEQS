var ez;
(function () {

    function drawRejectCodesSelect(selector) {

        var $selectList = $(selector);
        if ($selectList.length > 0) {

            $.ajax({
                type: "POST",
                url: App.ContextPath + "RejectCodesApi/getRejectCodeBranch",
                success: function (data) {

                    var rejectType = data.rejectType;
                    var rejectCode = data.rejectCode;

                    $.each(rejectType, function (i, rejectType) {
                        $selectList.append("<optgroup label='" + rejectType.fldRejectTypeDesc + "'>");
                        $.each(rejectCode, function (j, rejectCode) {
                            if (rejectType.fldRejectType == rejectCode.fldRejectType) {
                                $selectList.append("<option value=" + rejectCode.fldRejectCode + ">" + rejectCode.fldRejectCode + " - " + rejectCode.fldRejectDesc + " </option>");
                            }
                        });
                        $selectList.append("<option></option>");
                        $selectList.append("</optgroup>");
                    });
                    //Initiate selected select list
                    $("#rejectCodeSelectListBranch").val($.trim($("#rejectCodeTextBranch").val()))
                }
            })
        }
        bindRejectCodeInput();
        // xx start
        bindCharges();
        // xx end
    }

    function bindRejectCodeInput() {

        //on keyup
        $("#rejectCodeTextBranch").off('keyup').on('keyup', function () {
            //alert($(this).val().length);
            var rejectcode = $(this).val()
            if ($(this).val().length == 1) {
                //alert("a");
                rejectcode = ['00', $(this).val()].join('');
            }
            else if ($(this).val().length == 2) {
                //alert("b");
                rejectcode = ['0', $(this).val()].join('');
            }
            //alert(rejectcode);
            $("#rejectCodeSelectListBranch").val(rejectcode);
        });

        //on change
        $("#rejectCodeSelectListBranch").off('change').on('change', function () {
            //alert($(this).val().length);
            var rejectcode = $(this).val()
            if ($(this).val().length == 1) {
                //alert("a");
                rejectcode = ['00', $(this).val()].join('');
            }
            else if ($(this).val().length == 2) {
                //alert("b");
                rejectcode = ['0', $(this).val()].join('');
            }
            //alert(rejectcode);
            $("#rejectCodeTextBranch").val(rejectcode);
        });
    }

    // xx start
    function bindCharges() {
        //$("#rejectCodeSelectList").change(function () {
        $("#rejectCodeSelectListBranch").change(function () {
            $.ajax({
                cache: false,
                type: "POST",
                url: App.ContextPath + "RejectCodesApi/getCharges",
                data: "rejectCode=" + $('#rejectCodeSelectListBranch').val(),
                success: function (result) {
                    $("#serviceCharge").val(result);
                }
            });
        });
    }
    // xx end

    $(document).ready(function () {
        drawRejectCodesSelect("#rejectCodeSelectListBranch");
    });

})();