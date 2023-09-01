var ez;
(function () {
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////COMMON BINDING
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    // XX START

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////CHEQUE IMAGE ACTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    function bindCloseButton() {
        $("#closeBtn").off("click.cheque").on("click.cheque", function (e) {
            e.preventDefault();
            var $form = $(this).closest("form");
            $.ajax({
                type: "POST",
                url: $(this).data("action"),
                data: $form.serialize(),
                success: function (data) {
                    $("#resultContainer").html(data);
                    $("#searchForm").trigger("submit");
                    $(".right-panel").css("background-color", "#ffffff");
                    $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").show();
                }
            })
            //$("#resultContainer").html(data);
            //Remove Original Record Holder
            if ($("#OriTotalRecordHolderHeader").length > 0) {
                $("#OriTotalRecordHolderHeader").remove();
            }
        })
    }

    function bindAndDrawImageController() {
        $(".modify-cheque-btn-back").off("click.cheque")
            .on("click.cheque", function () {
                var actionClicked = $(this).data("btnfor");
                var imgFolder = $("#imageFolderBack").val();
                var imgId = $("#imageIdBack").val();
                var imgState = $("#imageStateBack").val();
                if (actionClicked === "reset") {
                    //Reset Button Clicked.
                    //Remove All State
                    imgState = "bw,";
                } else if (actionClicked === "front") {
                    imgState = imgState.replace("back", "");
                } else if (actionClicked === "back") {
                    imgState = imgState.replace("front", "");
                } else if (actionClicked === "bw") {
                    imgState += "," + "front";
                    imgState = imgState.replace("greyscale", "");
                    imgState = imgState.replace("uv", "");
                    imgState = imgState.replace("back", "");
                } else if (actionClicked === "greyscale") {
                    imgState += "," + "front";
                    imgState = imgState.replace("bw", "");
                    imgState = imgState.replace("uv", "");
                    imgState = imgState.replace("back", "");
                } else if (actionClicked === "uv") {
                    imgState = imgState.replace("bw", "");
                    imgState = imgState.replace("greyscale", "");
                    imgState = imgState.replace("back", "");
                }

                //If state clicked found in previous state, remove it.
                //This make the button act as Toggle
                if (imgState.indexOf(actionClicked) >= 0) {
                    imgState = imgState.replace("," + actionClicked, "");
                } else {
                    //Add the state 
                    imgState += "," + actionClicked;
                }

                //Replace ,, with ,
                imgState = imgState.replace(",,", ",");

                //Bind Error image again
                //bindErrorImage()

                //Remove image to display preload unless for big image
                if (actionClicked !== "large") {
                    $('#chequeImageBack').attr("src", "")
                }

                //Get only unique states for the server
                var arrayState = _.uniq(imgState.split(","));
                //If large state found. Draw in modal instead
                if ($.inArray("large", arrayState) > -1) {
                    //alert("d");
                    var imageHolder = "<img id='bigImageHolderBack'></img>";
                    bootbox.alert({
                        size: "large",
                        message: imageHolder,
                    });
                    $("#bigImageHolderBack").parents(".bootbox-body").addClass("image-loader");
                    $('#bigImageHolderBack').attr("src", App.ContextPath + "Image/LargeCheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    $("#bigImageHolderBack").parents('.modal-dialog').css({
                        width: 'auto',
                        height: 'auto',
                        'max-height': '80%'
                    });
                } else {
                    //Draw the image accordingly
                    //alert("c");
                    $("#imageStateBack").val(arrayState.join());
                    $('#chequeImageBack').attr("src", App.ContextPath + "Image/Cheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    //Bind zoomable image 
                    $("#chequeImageBack").off("click.cheque").on("click.cheque", function (e) {
                        extendEzPlusClickFunction("chequeImageBack", initializeEzPlus)
                    });
                }
            });
        //Trigger first draw of image
        $("#backBtn2").trigger('click');
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////EZ PLUS ZOOM PLUGINS BINDER
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    function extendEzPlusClickFunction(selector, callback) {
        callback("#" + selector);
        var ez = $("#" + selector).data('ezPlus');
        //alert(selector);
        $("#" + selector).off("click.cheque").on("click.cheque", function (e) {
            //alert("a");
            var ez = $("#" + selector).data('ezPlus');
            if (ez) {
                //alert("b");
                var zoomId = ez.options.zoomId;
                if ($("#" + selector + "-zoomLens").length > 0) $("#" + selector + "-zoomLens").remove();
                var $zoomLens = $(".zoomContainer[uuid='" + zoomId + "']")
                    .clone()
                    .attr("id", selector + "-zoomLens")
                    .appendTo("#resultContainer").click(function () {
                        $(this).remove();
                        callback("#" + selector);
                    });
                ez.destroy();
                $("#" + selector + "-zoomLens").remove();
                $("#" + selector).off("click.cheque").on("click.cheque", function (e) {
                    extendEzPlusClickFunction(selector, callback)
                });
            }
        });
    }

    function initEzPlus(selector, options) {
        var ez = $(selector).data('ezPlus');
        //alert("b");
        if (ez) ez.destroy();
        if ($(selector + "-zoomLens").length > 0) $(selector + "-zoomLens").remove();
        _.merge(options, {
            zoomType: 'lens',
            zoomContainerAppendTo: "#resultContainer",
            cursor: 'pointer',
            scrollZoom: true
        });
        $(selector).ezPlus(options);
    }

    function initializeEzPlus(selector) {
            //alert("b");
            initEzPlus(selector, {
                constrainSize: 500
            });
    }



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////INITIATE ALL FUNCTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    $(document).ready(function () {
        bindAndDrawImageController();
        bindCloseButton();
        



    });

})();
