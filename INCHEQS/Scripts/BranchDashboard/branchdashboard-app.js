
(function () {
    DashboardApp = {
        init: function () {
            $(".dashboard-element").each(function (i, element) {
                var id = $(this).attr("id");
                var taskId = $(this).data("taskid");
                var widgetType = $(this).data("widget-type");
                var divWidth = $(this).data("div-width");
                var $that = $(".widget-body",this);
                var $form = $(this).closest("form");
                var contextPath = $("#contextPath").html();
                $.ajax({
                    type: "POST",
                    url: contextPath + "DashboardApi/GetWidgetDetails",
                    data: $form.serialize() + "&divId=" + id + "&taskId=" + taskId,
                    success: function (data) {
                        $that.css("background", "none")
                        if (typeof DashboardFunctions[widgetType] === "function") {
                            DashboardFunctions[widgetType]($that, data);                            
                        } else {
                            alert("Dear Developer, please define function '" + widgetType + "' in Javascript Template code");
                        }
                    }
                });
                
            });
        },
        wrap: function () {
            var fullWidth = 0;
            var divCounter = 0;
            $('[data-div-width]').each(function () {
                fullWidth = fullWidth + $(this).data("div-width");
                divCounter = divCounter + 1;
                if (fullWidth == 12) {
                    fullWidth = 0;
                    $('[data-div-width]:nth-child(' + divCounter + ')').after('<div class="clearfix"></div>');
                    divCounter = divCounter + 1;
                } else if (fullWidth > 12) {
                    fullWidth = 0;
                    $('[data-div-width]:nth-child(' + divCounter + ')').before('<div class="clearfix"></div>');
                    divCounter = divCounter + 1;
                }
            })
        }
    }

    $(document).ready(function () {
        DashboardApp.init();
        DashboardApp.wrap();
    });

})();