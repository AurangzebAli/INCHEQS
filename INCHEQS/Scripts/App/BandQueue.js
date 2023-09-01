$(document).ready(function () {
    $(".checkbox-band-queue").on('change', function () {
        var rowId = $(this).attr("id"); //chkActive_3        
        var rowIndex = parseInt(rowId.substr(rowId.length - 1, rowId.length)); //3
        var row = $(this).closest('tr');
        var prevLowerBoundRowId = "txtLBound_" + (rowIndex - 1);//txtLBound_2        
        var prevUpperBoundRowId = "txtUBound_" + (rowIndex - 1);//txtLBound_2
        var canChange = false;
   
        if (rowIndex < 10) {

            var chkbox = '#chkActive_' + rowIndex;
            var nextChkbox = '#chkActive_' + (parseInt(rowIndex) + 1);

            if ($(nextChkbox).is(':checked')) {
                alert('You must disable Band ' + (parseInt(rowIndex) + 1) + ' before disable Band ' + rowIndex + '.');
                $(chkbox).prop('checked', 'checked');
                return false;
            }
        }
        if (rowIndex > 1) {

            var chkbox = '#chkActive_' + rowIndex;
            var nextChkbox = '#chkActive_' + (rowIndex - 1);

            if ($(nextChkbox).is(':checked')) {
                if (rowIndex > 2) {
                    var PrevInputID = '#txtLBound_' + (parseInt(rowIndex) - 1);
                    if ($(PrevInputID).val() < 1) {
                        alert('You must set Band ' + (rowIndex - 1) + ' limit before active Band ' + rowIndex + '.');
                        $(chkbox).attr('Checked', false);
                        return false;
                    }
                }
            } else {
                alert('You must active Band ' + (rowIndex - 1) + ' before active band ' + rowIndex + '.');
                $(chkbox).attr('Checked', false);
                return false;
            }
        }

        if (canChange=true) {
        if ($(this).is(':checked')) {
            $("#txtLBound_" + rowIndex).prop("readonly", false); // Current Row
            $('#' + prevUpperBoundRowId).prop("readonly", false).prop("value", "0.00")
            $('#txtUBound_' + rowIndex).prop("value", "And Above");
        } else {
            $("#txtLBound_" + rowIndex + ",#txtUBound_" + rowIndex).prop("readonly", true); // Current Row
            $('#' + prevUpperBoundRowId).prop("readonly", true).prop("value", "And Above");
            $('#txtLBound_' + rowIndex).prop("value", "0.00");
            $('#txtUBound_' + rowIndex).prop("value", "0.00");

        }
    }
    });
    $(".txtUBound").on('change', function () {
        var rowId = $(this).attr("id");
        var no = parseInt(rowId.substr(rowId.length - 1, rowId.length));
        var LBound = '#txtLBound_' + no;
        var UBound = '#txtUBound_' + no;

        var nextLBound = '#txtLBound_' + (no + 1);
        var currentUValue = parseFloat($(UBound).val());
        if (parseInt($(LBound).val()) > 0) {
            if (parseFloat($(LBound).val()) >= parseFloat($(UBound).val())) {
                alert('Band upper bound limit must greater than lower bound.');
                $(UBound).focus();
            } else {
                $(nextLBound).val((currentUValue + parseFloat(0.01)).toFixed(2));
                $(UBound).val((currentUValue + parseFloat(0.00)).toFixed(2));
            }

            
        }
    });

    $(".txtLBound").on('change', function () {
        var rowId = $(this).attr("id");
        var no = parseInt(rowId.substr(rowId.length - 1, rowId.length));
        var InputID = '#txtLBound_' + no;
        var InputID2 = '#txtUBound_' + no;

        var tmp = (parseFloat($(InputID).val()).toFixed(2));
        $(InputID).val(tmp);

        if (no > 1 && no < 10) {
            var PrevInputID2 = '#txtLBound_' + (parseInt(no) - 1);
            var PrevInputID = '#txtUBound_' + (parseInt(no) - 1);
            if (parseInt($(PrevInputID).val()) > parseInt($(PrevInputID2).val())) {
                if (parseInt($(InputID).val()) > parseInt($(PrevInputID2).val())) {

                    var PrevCheckID = '#chkActive_' + (parseInt(no) - 1);

                    if ($(PrevCheckID).is(':checked')) {

                        $(PrevInputID).val((parseFloat($(InputID).val()) - parseFloat(0.01)).toFixed(2));
                    }
                } else {
                    alert('Band Lower bound limit must greater than Band ' + (parseInt(no) - 1) + ' lower bound.');
                    $(InputID).focus();
                }
            }
        }
    });

});