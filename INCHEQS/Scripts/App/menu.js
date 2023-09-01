(function () {
    $(document).ready(function () {

        //Add has-sub class in menu if li has ul inside
        $('li').has("ul").addClass('has-sub');

        //Add active class when menu-holder li click
        $(".menu-holder li").off("click").on("click", function () {
            $(".menu-holder li").removeClass("active")
            $(this).toggleClass("active")
        })
        $(".caret-red").click(function () {
                $('.header').slideToggle();
        });

        renderMenus(true);
    });


    function renderMenus(shouldRenderMainMenu, mainMenuTitle) {
    
        var ContextPath = $("#contextPath").html();
        $('.menu-holder').addClass("disable-nav");
        $('.submenu-loader').removeClass("hidden");

        $.ajax({
            cache: false,
            type: "POST",
            url: ContextPath + "TaskMenu/GetCurrentUserMenuJson",
            success: function (data) {
                //var mainMenuTitle = $("#mainMenuTitle").html();
                var result = _.uniqBy(data, function (e) {
                    return e.mainMenu;
                });
              

                var subMenu = _.filter(data, function (e) {
                    return _.trim(e.mainMenu) == _.trim(mainMenuTitle);
                });

                var subMenuCategory = _.uniqBy(subMenu, function (e) {
                 
                    return _.trim(e.subMenu);
                });

                var $subMenuCategoryHolder = $(".menu-holder");
                $subMenuCategoryHolder.html("");
                _.each(subMenuCategory, function (menu) {
                    $subMenuCategoryHolder.append('<div class="menu-title"> ' + menu.subMenu + '  <span id="togglebutton" class="toggle-btn glyphicon glyphicon-align-justify"></span></div>	' + '<ul id=' + menu.subMenuId + '>')
                });


                _.each(subMenu, function (menu) {
                    if (menu.subMenuId === "") {
                       
                        $('<li>').append(
                            $('<a>').attr('data-action', ContextPath + menu.url).addClass("menu-secure-nav").append(
                                $('<span>').append(menu.menuTitle))).appendTo($(".menu-holder"));

                    } else {
                        $('<li>').append(
                            $('<a>').attr('data-action', ContextPath + menu.url).addClass("menu-secure-nav").append(
                                $('<span>').append(menu.menuTitle))).appendTo($("#" + menu.subMenuId));
                    }
                });

                if (shouldRenderMainMenu) {
                    result = _.sortBy(result, ['mainSeq'], ['asc']);

                    var $menus = $("#cssmenu ul");
                    _.each(result, function (menu) {
                        $menus.append($('<li>').append(
                            $('<a>').attr('data-action', ContextPath + menu.url).addClass("menu-secure-nav").attr("isMainMenu", true).append(
                                $('<span>').addClass(replaceSpaces(menu.mainMenu))
                            .append(menu.mainMenu))))

                        //Give active class to active menu title
                        //if (mainMenuTitle) {
                        //    $("." + replaceSpaces(mainMenuTitle)).parents("li").addClass("active")
                        //}
                    })
                }


                //Toggle btn in left-menu created by: Ali
                $(".toggle-btn").off('click').on('click', function (e) {
                    e.preventDefault();
                    //$(this).parents().next().slideToggle();
                    if ($(this).parents().next('ul').hasClass('hidden')) {
                        $(this).parents().next('ul').removeClass('hidden')
                    }
                    else
                    {
                        $(this).parents().next('ul').addClass('hidden')
                    }
                });





                $(".menu-secure-nav").off('click').on('click', function (e) {
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

                        // XX Edit
                        if (mainMenuTitle == 'INWARD CLEARING' || mainMenuTitle == 'Branch') {
                            $.ajax({
                                cache: false,
                                type: "POST",
                                url: $("#contextPath").html() + "CommonApi/SubmissionTimes",
                                success: function (data) {
                                    $(".left-panel .sub-menu table").html('<table width="100%" border="0" cellpadding="2" cellspacing="1" style="border-top:1px solid #F4A460;border-bottom:1px solid #F4A460;border-left:1px solid #F4A460;border-right:1px solid #F4A460"> ' +
                                        '<tr style = "font-size:8pt;background-color:#F4A460 ;color:#ffffff" >' +
                                        ' <td colspan="3" align="center">Cutoff Time</td></tr>' +
                                        '<tr style="font-size:8pt;color:#000000;background-color:#FFF5EE">' +
                                        ' <td width="40%">&nbsp;</td>  <td width="30%">' + data.spickCode + '</td>' +
                                        ' </tr> <tr style="font-size:8pt;color:#000000;background-color:#FFF5EE">' +
                                        '   <td width="40%">Start</td><td width="60%"><font color="red">' + data.startTime + '</font></td>' +
                                        ' </tr> <tr style="font-size:8pt;color:#000000;background-color:#FFF5EE">      <td width="40%">End</td>' +
                                        '   <td width="60%"><font color="red">' + data.endTime + '</font></td>  </tr>  </table > ');
                                }
                            });
                        }
                        else {
                            $(".left-panel .sub-menu table").html('')
                        }
                        // XX End
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
                });

                $('.submenu-loader').addClass("hidden");
                $('.menu-holder').removeClass("disable-nav");
            }
        });




    }


    function replaceSpaces(input) {
        return input.split(' ').join('_');
    }

})();