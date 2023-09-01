(function () {
    $(document).ready(function () {
        var str = $('.secure-form').attr('action');
        var n = str.lastIndexOf('/');
        var result = str.substring(n + 1);

        if (result == "SaveCreate") {
                $("#banktype").change(function () {
                    $.ajax({
                        type: 'POST',
                        url: App.ContextPath + 'BankCharges/GetBankChargesDesc',
                        dataType: 'json',
                        data: { type: $("#banktype").val() },
                        success: function (desc) {
                            $("#fldBankChargesDesc").html(desc);
                        }
                    })
                })

                $(".amount").change(function () {
                    $("#fldChequeMinAmtAR").attr("disabled", "disabled");
                    $("#fldChequeMaxAmtAR").attr("disabled", "disabled");
                    $("#fldChequeMinAmt").attr("disabled", "disabled");
                    $("#fldChequeMaxAmt").attr("disabled", "disabled");

                    if ($(this).val() == "Amount") {
                        $("#fldChequeMinAmt").removeAttr("disabled");
                        $("#fldChequeMaxAmt").removeAttr("disabled");
                    } else if ($(this).val() == "AmountRange") {
                        $("#fldChequeMinAmtAR").removeAttr("disabled");
                        $("#fldChequeMaxAmtAR").removeAttr("disabled");
                    }
                })

                $(".bankcharges").change(function () {
                    $("#fldBankChargesAmount").attr("disabled","disabled");
                    $("#fldBankChargesRate").attr("disabled","disabled");
                    if ($(this).val() == "ChargesAmount") {
                        $("#fldBankChargesAmount").removeAttr("disabled");
                    } else if ($(this).val() == "ChargesRate") {
                        $("#fldBankChargesRate").removeAttr("disabled");
                    }
                })
        }

        if (result == "Update") {
            $.ajax({
                type: 'POST',
                url: App.ContextPath + 'BankCharges/GetBankChargesDesc',
                dataType: 'json',
                data: { type: $("#fldBankChargesType").val() },
                success: function (desc) {
                    $("#fldBankChargesDesc").html(desc);
                }
            })

            

            $(".amount").change(function () { 
                if ($(this).val() == "Amount") {
                    $("#fldChequeMinAmtAR").attr("disabled", "disabled");
                    $("#fldChequeMaxAmtAR").attr("disabled", "disabled");
                    $("#fldChequeMinAmt").removeAttr("disabled");
                    $("#fldChequeMaxAmt").removeAttr("disabled");
                } else if ($(this).val() == "AmountRange") {
                    $("#fldChequeMinAmtAR").removeAttr("disabled");
                    $("#fldChequeMaxAmtAR").removeAttr("disabled");
                    $("#fldChequeMinAmt").attr("disabled", "disabled");
                    $("#fldChequeMaxAmt").attr("disabled", "disabled");
                }
                
            })

            $(".bankcharges").change(function () {
               
                if ($(this).val() == "ChargesAmount") {
                    $("#fldBankChargesAmount").removeAttr("disabled");
                    $("#fldBankChargesRate").attr("disabled", "disabled");
                } else if ($(this).val() == "ChargesRate") {
                    $("#fldBankChargesRate").removeAttr("disabled");
                    $("#fldBankChargesAmount").attr("disabled", "disabled");
                }
            })


        }
        /*$("#fldBankChargesDesc").html("Test");
        if (result == "BankCharges") {
        
           
            /*$.ajax({
                type: 'POST',
                url: App.ContextPath + 'BankCharges/GetBankChargesDesc',
                dataType: 'json',
                data: { type: $("#fldBankChargesType").val() },
                success: function (desc) {
                    $("#fldBankChargesDesc").html(desc);
                }
            })

        }*/
        /*$("#fldBankChargesRate").change(function () {
            var num = parseFloat(this.value),
                min = 0,
                max = 100;

            if (isNan(num)) {
                num= "";
                return num;
            } else if (num < min) {
                return min;
            } else if (num > max) {
                return max;
            } else {
                return num;
            }
        })*/
    });
})();