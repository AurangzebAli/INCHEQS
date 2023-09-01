
var input = ""; //holds current input as a string (amount)
var input2 = ""; //holds current input2 as string (date)

(function () {
    $(document).ready(function () {
        $("#chqdate").val("");
        $("#chqdate").focus();
        //$("#chqdate").select();

        $("#chqdate").keydown(function (event) {
            var keycode = event.keyCode || event.which;
            if (keycode == 13 || keycode == 9) {
                if ($("#chqdate").val() === 'undefined' || $("#chqdate").val() === null || $("#chqdate").val() === "") {
                    alert("Please Add Cheque Date.");
                    $("#atm").focus();
                    return;
                }
                else {
                    var datecheque = $("#chqdate").val();
                    $("#atm").focus();
                }
            }
        });

        $("#atm").keydown(function (event) {
            var keycode = event.keyCode || event.which;
            if (keycode == 13 || keycode == 9) {
                if ($("#atm").val() === 'undefined' || $("#atm").val() === null || $("#atm").val() === "") {
                    alert("Please Add Cheque Amount.");
                    $("#chqdate").focus();
                    return;
                }
                else {
                    var datecheque = $("#chqdate").val();
                    $("#chqdate").focus();
                }
            }
        });

        $("#chqdate").keydown(function (e) {
            if (e.keyCode == 8 && input.length > 0) {
                input = input.slice(0, input.length - 1); //remove last digit
                $(this).val(addDash(formatNumber(input)));
            }
            else if (input.length == 7) {
                input = input.slice(0, input.length - 1); //remove last digit
                $(this).val(addDash(formatNumber(input)));
            }
            else if (e.keyCode == 107) {
                $("#Confirmbtn").trigger('click');
            }
            else {
                var key = getKeyValue(e.keyCode);
                if (key) {
                    input += key; //add actual digit to the input string
                    $(this).val(addDash(formatNumber(input))); //format input string and set the input box value to it
                }
            }
            return false;

            function addDash(num) {
                return input.toString().replace(/\B(?=(\d{2})+(?!\d))/g, "-");
            }

        });

        $("#atm").val("");
        $("#atm").keydown(function (e) {
            //handle backspace key
            if (e.keyCode == 8 && input2.length > 0) {
                input2 = input2.slice(0, input2.length - 1); //remove last digit
                $(this).val(addCommas(formatNumber(input2)));
            }
            else if (e.keyCode == 107) {
                $("#Confirmbtn").trigger('click');
            }
            else {
                var key = getKeyValue(e.keyCode);
                if (key) {
                    input2 += key; //add actual digit to the input string
                    $(this).val(addCommas(formatNumber(input2))); //format input string and set the input box value to it
                }
            }
            return false;
        });
        function getKeyValue(keyCode) {
            if (keyCode > 57) { //also check for numpad keys
                keyCode -= 48;
            }
            if (keyCode >= 48 && keyCode <= 57) {
                return String.fromCharCode(keyCode);
            }
        }
        function formatNumber(input2) {
            if (isNaN(parseFloat(input2))) {
                return "0.00"; //if the input is invalid just set the value to 0.00
            }
            var num = parseFloat(input2);
            return (num / 100).toFixed(2); //move the decimal up to places return a X.00 format
        }
        function addCommas(num) {
            return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        }
    });
})();