

var DashboardTempates =  new function () {

    this.TableTemplate = [
       "<table class='table table-bordered'>",
           "<thead>",
               "<tr>",
                   "<% _.forEach(data[0], function(val, key) { %>",
                       "<td><%= key %></td>",
                   "<% }) %>",
               "</tr>",
           "</thead>",
           "<tbody>",
               "<% _.forEach(data, function(row) { %>",
                   "<tr>",
                   "<% _.forEach(row, function(val, key) { %>",
                       "<td><%= val %></td>",
                   "<% }) %>",
                   "</tr>",
               "<% }) %>",
           "</tbody>",
       "</table>"
    ].join("\n");

    this.QueueTableTemplate = [
        "<table class='table table-bordered'>",
            "<thead>",
                "<tr>",
                    "<td>Branch </td>",
                    "<td>Branch Approve </td>",
                    "<td>Branch Return </td>",
                    "<td>Branch Maker  </td>",
                    "<td>Branch Checker </td>",
                    "<td>Total</td>",
                "</tr>",
                "<tr class='hidden'>",
                   "<% _.forEach(data[0], function(val, key) { %>",
                       "<td><%= key %></td>",
                   "<% }) %>",
               "</tr>",
            "</thead>",
            "<tbody>",
                "<% _.forEach(data, function(row) { %>",
                    "<tr>",
                    "<% _.forEach(row, function(val, key) { %>",
                        "<td class='<%= key %>'><%= val %></td>",
                    "<% }) %>",
                    "</tr>",
                "<% }) %>",
            "</tbody>",
            "<tfoot>",
                "<tr>",
                    "<td>Total</td>",
                    "<td><span id='BranchCheckerApprove'></span></td>",
                    "<td><span id='BranchCheckerReject'></span></td>",
                    "<td><span id='BranchMakerOutstanding'></span></td>",
                    "<td><span id='BranchCheckerApproveOutstanding'></span></td>",
                    "<td><span id='TotalItem'></span></td>",
                "</tr>",
            "</tfoot>",
        "</table>"
    ].join("\n");

    this.PaginatedTableTemplate = [
       "<table class='table data-table'>",
           "<thead>",
               "<tr>",
                   "<% _.forEach(data[0], function(val, key) { %>",
                       "<td><%= key %></td>",
                   "<% }) %>",
               "</tr>",
           "</thead>",
           "<tbody>",
               "<% _.forEach(data, function(row) { %>",
                   "<tr>",
                   "<% _.forEach(row, function(val, key) { %>",
                       "<td><%= val %></td>",
                   "<% }) %>",
                   "</tr>",
               "<% }) %>",
           "</tbody>",
       "</table>"
    ].join("\n");

}
