
(function () {
    $(document).ready(function () {
        $("#btnEOD").on("click", function (e) {
            if (confirm('Are you sure you want to perform End Of Day?')) {
                e.preventDefault();
                var $parentLi = $(this).parents("li");
                var isMainMenu = $(this).attr("isMainMenu");
                var dataAction = $(this).attr("data-action");
                if (isMainMenu) {
                    $parentLi.siblings().removeClass("active");
                    var mainMenuTitle = $("span", this).html();

                    $("#left-menu").removeClass("hidden");
                    $("#mainMenuTitle").html(mainMenuTitle);
                    $(".left-panel .sub-menu h5").html(mainMenuTitle);

                }

                $(".menu-holder li").removeClass("active");
                $parentLi.addClass("active")


                //Trigger event to let the rest of the listener know next page has been clicked
                $(document).trigger("menu-click");

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: dataAction,
                    success: function (data) {
                        $("#right-panel").html(data);
                        if (isMainMenu) {
                            renderMenus(false, mainMenuTitle);
                        }
                    }
                });
            } else {
                return;
            }
        })
    });

})();