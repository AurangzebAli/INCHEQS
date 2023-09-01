var Pagination = new function () {

    var context = document;

    var preparePagination = function () {

        var orderBy = $(".orderByHolder", context).val();
        var orderType = $(".orderTypeHolder", context).val();
        $("." + orderBy, context).addClass(orderType);

        $(".pagination a", context).off('click').on('click', function (e) {
            e.preventDefault();
            var page = $(this).data("page")
            var $form = $(this).closest("form");// $("#searchForm", context)
            if ($form.length == 0) {
                bootbox.alert("No Form found for pagination process")
            }
            $(".pageHolder", $form).remove()
            $("<input type='hidden' class='pageHolder' />", $form).attr("name", "page").attr("value", page).appendTo($form);

            submitForm($form)
        });

        //Draw sorting
        $(".sort", context).off("click").on("click", function (e) {
            if ($(this).data('sort')) {
                var $form = $(this).closest("form"); //$("#searchForm", context)
                var sortField = $(this).data('sort')

                var sortType = $(".orderTypeHolder", context).val()
                if (sortType === "asc") {
                    sortType = "desc"
                } else {
                    sortType = "asc"
                }

                if (sortField !== $(".orderByHolder", context).val()) {
                    sortType = "asc"
                }

                $(".orderByHolder", context).remove()
                $(".orderTypeHolder", context).remove()


                $("<input/>").attr("type", "hidden").attr("name", "orderBy")
                    .attr("value", sortField).addClass("orderByHolder")
                    .appendTo($form);
                $("<input/>").attr("type", "hidden").attr("name", "orderType")
                    .attr("value", sortType).addClass("orderTypeHolder")
                    .appendTo($form);


                submitForm($form)
            }
        });

        //Draw Pagination
        var currentPage = parseInt($(".currentPage", context).text())
        var totalPage = parseInt($(".totalPage", context).text())

        //pagination add active class and next/prev button
        $(".pagination a." + currentPage, context).parent("li").addClass("active")
        if ($(".pagination a[pageAct='prev']", context).data("page") == 0) { $(".pagination a[pageAct='prev']", context).hide(); }
        if ($(".pagination a[pageAct='next']", context).data("page") == totalPage + 1) { $(".pagination a[pageAct='next']", context).hide(); }


    };

    var submitForm = function ($form) {
        $form.submit();
    };

    //Calculate page amount based on column and row below it if table in database match the name of 'amount' (e.g 'fldTotalAmount' or 'fldAmount')
    var calculateAmount = function() {

        //convert from double format to currency format
        $.fn.digits = function () {
            return this.each(function () {
                $(this).text($(this).text().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,"));
            })
        }

        var result = 0;
        $('td.currency', context).each(function () {
            result += Number($.trim($(this).text()).replace(/[^0-9\.]+/g, ""));
        });
        $("span.totalAmount" , context).text(result.toFixed(2)).digits();
        $("span.record", context).text($("#search-result-table tbody tr", context).length);
    }

    this.paginate = function (context) {
        context = context;
        preparePagination();
        calculateAmount();
    };

}