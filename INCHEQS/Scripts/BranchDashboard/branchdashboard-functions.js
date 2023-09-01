
DashboardFunctions = {
    //QueuePivotWithUI: function (selector, data) {
    //    var sortAs = $.pivotUtilities.sortAs;
    //    $(selector).pivotUI(data, {
    //        sorters: function (attr) {
    //            if (attr == "QueueLocationIndividual") {
    //                return sortAs(["", "CCU Maker", "2nd Checker", "Pending on Branch Admin"]);
    //            }
    //        },
    //        rows: ["BAND"],
    //        cols: ["QueueDescription", "QueueLocationIndividual", "QueueLocation"],
    //        aggregatorName: "Count",
    //        rendererName: "Heatmap"
    //    });
    //},


    Table: function (selector, data) {
        var $holder = $(selector);
        $holder.html(_.template(DashboardTempates.TableTemplate)({ data: data }));
    },

    QueueTableTemplate: function (selector, data) {
        var $holder = $(selector);
        $holder.html(_.template(DashboardTempates.QueueTableTemplate)({ data: data }));

        //Draw total for each column after success load
        $("#bprogresStatusWidget table tbody tr").first().children().each(function () {
            sumTotalColumn($(this).prop("class"));
        })
        function sumTotalColumn(className) {
            var result = 0;
            $("." + className).each(function () {
                result += Number($(this).text());
            });
            $("span#" + className).text(result);
        }
    },

    PaginatedTable: function (selector, data) {
        var $holder = $(selector);
        $holder.html(_.template(DashboardTempates.PaginatedTableTemplate)({ data: data }));
        //initialize javascript after success load
        $(".data-table").dataTable();
    }
}