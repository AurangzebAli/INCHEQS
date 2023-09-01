var ez;
(function () {

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////INITIATE ALL FUNCTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    var test = "";
    var test2 = "";
    var test3 = "";
    var check = false;
    $("#StartBatchICR").ready(function () {
        upperStatus();
        drawProgressDetailsTable();
        percentage();
    });

    $('#StartBatchICR').click(function () { return false; });
    $(window).click(function () {
        clearInterval(test);
        clearInterval(test2);
        clearInterval(test3);
    });

    function percentage() {
        clearInterval(test);
        test = setInterval(function () {
            if (check == true) {
                clearInterval(test);
            }
            else {
                refresh();
            }
        }, 1000);

        function refresh() {
            $.ajax({
                cache: false,
                type: "POST",
                url: App.ContextPath + "StartBatchICR/percentage",
                data: "machineID=" + $('#txtMachineID').val(),
                success: function (result) {
                    animateProgressBar(result);
                    if (result >= 100) {
                        clearInterval(test);
                    }
                }
            });
        };
    };

    function animateProgressBar(percentageCompleted) {
        $('#innerDiv').animate({
            'width': (500 * percentageCompleted) / 100
        }, 1000);

        $({ counter: percentageCompleted }).animate({ counter: percentageCompleted }, {
            duration: 1000,
            step: function () {
                $('#innerDiv').text(this.counter + ' %');
            }
        })

    }

    function drawProgressDetailsTable() {
        //Call ajax and draw table
        //Return JSON
        var $form = $("#searchForm");
        var $holder = $("#progressDetails tbody");

        var progressDetailsTemplate = [

            "<% _.forEach(data, function(i) { %>",
            "<tr>",
            "<td><%=i.fldChequeNumber %></td>",
            "<td><%=i.fldAccountNumber %></td>",
            "<td><%=i.fldStatus %></td>",
            "<td><%=i.fldRemarks %></td>",
            "</tr>",

            "<% }) %>"
        ].join("\n");

        test2 = setInterval(function () {
            if (check == true) {
                clearInterval(test2);
            }
            else {
                refresh();
            }
        }, 2000);

        function refresh() {
            $.ajax({
                cache: false,
                type: "POST",
                url: App.ContextPath + "StartBatchICR/getProgressDetail",
                data: $form.serialize(),

                beforeSend: function () {
                    $('.rules-loader').removeClass("hidden")
                },
                success: function (data) {
                    $holder.empty();
                    //Draw Rules using template
                    if (data.length > 0) {
                        $holder.append(_.template(progressDetailsTemplate)({ data: data }));
                    } else {
                        $holder.append("<tr><td>No Data</td></tr>");
                    }
                }
            });
        }
    }

    function upperStatus() {

        var $form = $("#searchForm");
        test3 = setInterval(function () {
            if (check == true) {
                clearInterval(test3);
            }
            else {
                refresh();
            }
        }, 2000);
        function refresh() {
            $.ajax({
                cache: false,
                type: "POST",
                url: App.ContextPath + "StartBatchICR/upperStatus",
                data: $form.serialize(),
                success: function (result) {
                    $("#totalCount").text(result.totalCount);
                    $("#successCount").text(result.successCount);
                    $("#itemLeftCount").text(result.itemLeftCount);
                    $("#failCount").text(result.failCount);
                    $("#startTime").text(result.startTime);
                    $("#endTime").text(result.endTime);
                }
            });
        };
    };
})();