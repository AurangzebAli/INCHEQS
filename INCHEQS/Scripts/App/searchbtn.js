(function () {
    $(".init-search").click(function () {
        var $form = $(this).closest("form");
        $(".pageHolder", $form).remove();
        $("<input type='hidden' class='pageHolder' />", $form).attr("name", "searchbtnvalue").attr("value", "1").appendTo($form);

    })
});