var ez;
(function () {
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////COMMON BINDING
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    // XX START
    function bindShortCutKey() {
        $(document).off("keypress").on("keypress", function (e) {
            //alert(e.keyCode);
            //alert("b");
            //alert($("#rejectCodeText").is(":focus"));

            //console.log(e.keyCode +"+"+ e.shiftKey); // True = Shift key pressed
            if ($(".remarks").is(":focus") || $("#rejectCodeText").is(":focus") || $("#serviceCharge").is(":focus")) {
            }
            else {
                //switch (e.keyCode + "+" + e.shiftKey) {
                //    case 39 + "+true": $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                //    case 37 + "+true": $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                //    case 65 + "+true": $("#approveBtn").trigger('click'); break; // Keyboard-A
                //    case 82 + "+true": $("#returnBtn").trigger('click'); break; // Keyboard-R
                //    case 66 + "+true": $("#routeBtn").trigger('click'); break; // Keyboard-B
                //    case 27 + "+true": $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                //}
                switch (e.keyCode) {
                    //case 39: $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                    //case 37: $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                    //case 113: $("#approveBtn").trigger('click'); break; // Keyboard-F2
                    //case 82: $("#returnBtn").trigger('click'); break; // Keyboard-R
                    //case 66: $("#routeBtn").trigger('click'); break; // Keyboard-B
                    //case 80: $("#pulloutReasonBtn").trigger('click'); break; // Keyboard-P
                    //case 114: $("#returnBtn").trigger('click'); break; // Keyboard-r
                    //case 98: $("#routeBtn").trigger('click'); break; // Keyboard-b
                    //case 112: $("#pulloutReasonBtn").trigger('click'); break; // Keyboard-p
                    case 27: $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                    case 13: $("#confirmBtn").trigger('click'); break; // Keyboard-Enter

                }
            }
        })
        $(document).off("keyup").on("keyup", function (e) {
            //alert(e.keyCode);
            //alert($("#rejectCodeText").is(":focus"));
            //alert("a");
            //console.log(e.keyCode +"+"+ e.shiftKey); // True = Shift key pressed
            if ($(".remarks").is(":focus") || $("#rejectCodeText").is(":focus") || $("#serviceCharge").is(":focus")) {
            }
            else {
                //switch (e.keyCode + "+" + e.shiftKey) {
                //    case 39 + "+true": $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                //    case 37 + "+true": $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                //    case 65 + "+true": $("#approveBtn").trigger('click'); break; // Keyboard-A
                //    case 82 + "+true": $("#returnBtn").trigger('click'); break; // Keyboard-R
                //    case 66 + "+true": $("#routeBtn").trigger('click'); break; // Keyboard-B
                //    case 27 + "+true": $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                //}
                switch (e.keyCode) {
                    //case 39: $("#nextBtn").trigger('click'); break; // Keyboard-Right Arrow
                    //case 37: $("#previousBtn").trigger('click'); break; // Keyboard-Left Arrow
                    //case 113: $("#approveBtn").trigger('click'); break; // Keyboard-F2
                    case 13: $("#confirmBtn").trigger('click'); break; // Keyboard-Enter
                    case 27: $("#closeBtn").trigger('click'); break; // Keyboard-ESC
                }
            }
        })

    }
    // XX END


    function bindOriginalTotalRecord() {
        if ($("#OriTotalRecordHolder").length > 0) {
            $(".header").append('<input type="hidden" id="OriTotalRecordHolderHeader" value="' + $("#OriTotalRecordHolder").val() + '">')
            $("#OriTotalRecord").text($("#OriTotalRecordHolderHeader").val());
        }
        if ($("#OriTotalRecordHolderHeader").length > 0) {
            $("#OriTotalRecord").text($("#OriTotalRecordHolderHeader").val());
        }
    }


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
        $(".modify-cheque-btn").off("click.cheque")
            .on("click.cheque", function () {
                var actionClicked = $(this).data("btnfor");
                var imgFolder = $("#imageFolder").val();
                var imgId = $("#imageId").val();
                var imgState = $("#imageState").val();
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
                    $('#chequeImage').attr("src", "")
                }

                //Get only unique states for the server
                var arrayState = _.uniq(imgState.split(","));
                //If large state found. Draw in modal instead
                if ($.inArray("large", arrayState) > -1) {
                    //alert("d");
                    var imageHolder = "<img id='bigImageHolder'></img>";
                    bootbox.alert({
                        size: "large",
                        message: imageHolder,
                    });
                    $("#bigImageHolder").parents(".bootbox-body").addClass("image-loader");
                    $('#bigImageHolder').attr("src", App.ContextPath + "Image/LargeCheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    $("#bigImageHolder").parents('.modal-dialog').css({
                        width: 'auto',
                        height: 'auto',
                        'max-height': '80%'
                    });
                } else {
                    //Draw the image accordingly
                    //alert("c");
                    $("#imageState").val(arrayState.join());
                    $('#chequeImage').attr("src", App.ContextPath + "Image/Cheque?imageFolder=" + imgFolder + "&imageId=" + imgId + "&imageState=" + arrayState.join());
                    //Bind zoomable image 
                    $("#chequeImage").off("click.cheque").on("click.cheque", function (e) {
                        extendEzPlusClickFunction("chequeImage", initializeEzPlus)
                    });
                }
            });
        //Trigger first draw of image
        $("#frontBtn").trigger('click');
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
        bindShortCutKey();
        bindOriginalTotalRecord();

        //Flash host Status
        setInterval(function () {
            $(".blink").toggleClass("txtBlue");
        }, 500);


    });

})();
