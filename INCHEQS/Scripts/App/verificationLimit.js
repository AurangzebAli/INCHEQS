﻿(function () {
    $(document).ready(function () {

        $(".generateButton").click(function () { 
            var type1 = document.getElementById('firstType').value;
            var amount1 = document.getElementById('firstAmount').value;
            var concat = document.getElementById('concat').value;
            var type2 = document.getElementById('secondType').value;
            var amount2 = document.getElementById('secondAmount').value;
            var generate;

            if ($("#concat option:selected").val() == "") {
                generate = "Amount " + type1 + amount1;
            } else {
                if ($("#secondType").val() == "") {
                    alert("Please select second amount")
                    document.getElementById("secondType").focus();
                    return false;
                }
                else if ($("#secondAmount").val() == "") {
                    alert("Please insert second amount value")
                    document.getElementById("secondAmount").focus();
                    return false;
                } else {
                    generate = "Amount " + type1 + amount1 + concat + " Amount " + type2 + amount2;
                }
            }
            $("#VerifyLimitDesc").val(generate);

        });

    });

    function disableField() {
        $("#secondType").prop("disabled", true)
        $("#secondAmount").prop("readonly", true)
        $("#secondType").prop("value", "")
        $("#secondAmount").prop("value", "0.00")
    };
    function enableField() {
        $("#secondType").prop("disabled", false)
        $("#secondAmount").prop("readonly", false)
    };


    if ($("#concat option:selected").val() == "") {
        disableField()
    } else {
        enableField()
        if ($("#secondType").val() == "") {
            alert("Please fill in second amount value")
            document.getElementById("secondType").focus();
        }
    }

    $("#concat").change(function () { //change dropdown
        if (this.value == "") {
            disableField();
        } else {
            enableField();
        }
    });

})();