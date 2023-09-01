
$(document).ready(function () {
    //Add function for add-from to add-to in multiple select form
    $('.addAll').click(function () {
        $('.select-from option').each(function () {
            $('.select-to').append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option>");
            $(this).remove();
        });
    });
    $('.addOne').click(function () {
        $('.select-from option:selected').each(function () {
            $('.select-to').append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option>");
            $(this).remove();
        });
    });
    $('.rmOne').click(function () {
        $('.select-to option:selected').each(function () {
            $('.select-from').append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option>");
            $(this).remove();
        });
    });
    $('.rmAll').click(function () {
        $('.select-to option').each(function () {
            $('.select-from').append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option>");
            $(this).remove();
        });
    });

    $("form").off('submit').on('submit', function (e) {
        $(".select-to option").prop("selected", "selected");
    })
});