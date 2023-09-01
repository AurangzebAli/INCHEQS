(function () {
    $(document).ready(function () {

        $("#").off("click").on("click", function () {
            $(".overlay,.popup").fadeIn();
            StartProgrssBar();
            CallAPI();
        })
    })

    //progress bar function
    function StartProgrssBar() {
        var currentDate = new Date();
        var second = currentDate.getSeconds();
        if (second < 10) {
            second = "0" + second;
        }

        $('.progress-bar').css('width', second + '%');

        setTimeout(function () { StartProgrssBar(), 500 });
    }

    //close Progress Bar
    function CloseProgressBar() {
        $('#Fade_area').removeAttr("style");
        $('#mymodal').removeAttr("style");
    }
    function CallAPI() {
        $.ajax({
            url: "",
            type: 'post',
            contentType: "application/json",
            success: function () {
                setTimeout(function () { CloseProgressBar() },10000)
            },
            error: function () {
                setTimeout(function () { CloseProgressBar() }, 10000)
            }


        });

    }
})