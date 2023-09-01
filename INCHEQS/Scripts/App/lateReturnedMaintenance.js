var ez;
(function () {

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////INITIATE ALL FUNCTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    $(document).ready(function () {

        $('#btnFindLateReturnMaintenance').click(function () {
            var e = document.getElementById("fldClearDate");
            var strUser = e.value;
            if (strUser == "") {
                alert("No Late Returned Items");
            }
            else {
                drawLateReturnMaintenanceTable();
            }
        });
        
        
    });
    
    function drawLateReturnMaintenanceTable() {
        //Call ajax and draw table
        //Return JSON
        var $form = $("#LateReturnedMaintenanceForm");
        var $holder = $("#resultLateReturnMaintenance tbody");
        $.ajax({
            cache: false,
            type: "POST",
            url: App.ContextPath + "LateReturnedMaintenance/getLateReturnMaintenance",
            data: $form.serialize(),

            beforeSend: function () {
                $('.rules-loader').removeClass("hidden")
            },
            success: function (data) {
                //Draw Rules using template

                $holder.empty().append(_.template(LateReturnedMaintenanceTemplate)({ data: data }));

            }
        });
    }

    var LateReturnedMaintenanceTemplate = [

        "<% _.forEach(data, function(i) { %>",
        "<tr>",
        "<td><%=i.fldClearDate %></td>",
        "<td><%=i.fldAccountNo %></td>",
        "<td><%=i.fldChequeNo %></td>",
        "<td><%=i.fldUIC %></td>",
        "<td><%=i.fldAmount %></td>",
        "<td><%=i.fldtranscode %></td>",
        "<td><%=i.fldApprovalUserId %></td>",
        "<td><%=i.fldApprovalStatus %></td>",
        "<td><%=i.fldApprovalTimestamp %></td>",
        "<td><%=i.upiGenerated %></td>",
        "</tr>",
        "<% }) %>"
    ].join("\n");

})();