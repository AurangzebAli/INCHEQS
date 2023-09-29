var App = new function () {

    var context = document;
    var intervals;
    window.catchPageUnloadFlag = true;

    var initBootstrap = function () {
        //Remove href form a.disabled
        $("a.disabled", context).prop("href", "#");
        //make tooltip enable
        $('[data-toggle="tooltip"]', context).tooltip();
        //make popover enable
        $('[data-toggle="popover"]', context).popover();
        //Add jQuery alert for fade out when close button clicked
        $('.close', context).off("click.g").on("click.g", function () {
            $(this).parent().fadeOut();
        })
        //prevent to input char in .numeric input
        $(document).on("input", ".number-only", function () {
            this.value = this.value.replace(/[^0-9\.]/g, '');
        });

        $('.number-only').keypress(function (event) {
            if ((event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which < 48 || event.which > 57)) {
                event.preventDefault();
            }
        });


    }

    var bindInputAsMoney = function () {
        $(".currency").maskMoney();
    }

    var bindPreventEnterKey = function () {
        $('form input').on('keypress', function (e) {
            return e.which !== 13;
        });
    }

    var removeAutoComplete = function () {
        $(document).on('focus', ':input', function () {
            $(this).attr('autocomplete', 'off');
        });

        $("#UserPassword").off('focus').on('focus', function () {
            $("#UserPassword").val("");
            $("#UserPassword").attr('type', 'password')
        });
    }

    var addInterval = function (callback, duration) {

        intervals = setInterval(callback, duration);
    }

    var clearAllIntervals = function () {
        window.clearInterval(intervals);
    }


    var initBootbox = function () {
        //Add confirm button before delete
        $(".confirm", context).off("click.g.confirm").on("click.g.confirm", function (e) {
            e.preventDefault();
            var $form = $(this).closest("form");
            var msg = $(this).attr("confirm-msg");
            if (msg == undefined) {
                msg = "Confirm Delete?"
            }
            bootbox.confirm(msg, function (result) {
                if (result) {
                    $form.submit();
                }
            });
        });
    };

    var bindDateForCalendar = function () {
        var dateFormatConfig = $("span#jQueryDateFormat", context).text();
        //Add jQuery Datepicker for input class .form-date
        $('.form-date', context).datepicker({
            dateFormat: dateFormatConfig,
            changeMonth: true,
            changeYear: true,
            showOn: "button",
            buttonText: '',
            buttonImageOnly: true,
            buttonImage: $("#contextPath").html() + 'Content/images/cal.jpg'
        });
        //Add default date if no value
        if ($(".form-date", context).val() == "") {
            $(".form-date", context).datepicker("setDate", new Date());
        }

        //Add jQuery Datepicker date to and date from limit date
        $(".date-start", context).datepicker({
            dateFormat: dateFormatConfig,
            changeMonth: true,
            changeYear: true,
            showOn: "button",
            buttonText: '',
            buttonImageOnly: true,
            buttonImage: $("#contextPath").html() + 'Content/images/cal.jpg',
            onSelect: function (selected) {
                $(".date-end").datepicker("option", "minDate", selected)
            }
        });
        $(".date-end", context).datepicker({
            dateFormat: dateFormatConfig,
            changeMonth: true,
            changeYear: true,
            showOn: "button",
            buttonText: '',
            buttonImageOnly: true,
            buttonImage: $("#contextPath").html() + 'Content/images/cal.jpg',
            onSelect: function (selected) {
                $(".date-start", context).datepicker("option", "maxDate", selected)
            }
        });

        if ($(".date-start", context).val() === '') {
            $(".date-start", context).datepicker("setDate", new Date());
        }
        if ($(".date-end", context).val() === '') {
            $(".date-end", context).datepicker("setDate", new Date());
        }
    }

    var prepareDataTable = function () {
        //Add data table plugins to table class .data-table
        $('.data-table', context).dataTable();
        //replace label with span for ie8 compatibility
        $('.dataTables_length label', context).contents().unwrap().wrap('<span/>');
        $('.dataTables_filter label', context).contents().unwrap().wrap('<span/>');

        //Perform data table after page load and fix the width
        if ($(".data-table", context).length) {
            $('.data-table', context).dataTable().fnSettings().aoDrawCallback.push({
                "fn": bindFormAction
            });
            $(".data-table", context).removeClass("hidden").width("");
        }

    }

    var bindSecureForm = function () {

        $(".secure-form", context).off('submit.g').on('submit.g', function (e) {
            e.preventDefault();
            //Add selected box in multi select option. Put here because prevent multi sumbit function
            $(".select-to option", context).prop("selected", "selected");
            $.ajax({
                cache: false,
                type: $(this).attr('method'),
                url: $(this).attr('action'),
                data: $(this).serializeArray(),
                success: function (data) {
                    if ($("#resultContainer").length > 0) {
                        $("#resultContainer").html(data);
                    } else {
                        $("#right-panel").html(data);
                    }
                }
            });

        });

    }



    var bindRowSubmit = function () {

        $(".row-action-submit", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            var url = "";
            // if ($form.attr("action")) { url = $form.attr("action"); }
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $(this).parents("tr").find(".row-result-params input").serialize(),
                success: function (data) {
                    $("#resultContainer", context).html(data);
                }
            });

        });

    }

    var bindNormalSubmit = function () {

        $(".normal-submit", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }

            $(".normal-submit-Verification").attr("disabled", true);
            $(".normal-submit-pending-branch").attr("disabled", true);
            $(".btn-print").attr("disabled", true);

            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $form.serialize(),
                success: function (data) {
                    $("#resultContainer").html(data);
                }
            });

        });


        $(".normal-submit-Verification-branchremarks", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");
            if ($.trim($('#BranchActivation').val()) == '0') {
                alert("BranchActivation not activated");
                return;
            }




            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                $(".normal-submit-Verification").attr("disabled", true);
                $(".btn-action-verification").attr("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {

                        $('#myModal').modal('hide');
                        $("#resultContainer").html(data);

                    }
                });

            }

        });




        $(".normal-submit-Verification", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");
            if ($.trim($('#CCUActivation').val()) == '0') {
                alert("CCUActivation not activated");
                return;
            }


            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                $(".normal-submit-Verification").attr("disabled", true);
                $(".btn-action-verification").attr("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {

                        $("#resultContainer").html(data);

                    }
                });

            }

        });

        $(".normal-submit-pending-branch", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                var r = confirm("You are not a branch user, do you want to proceed?")

                if (r == true) {
                    $(".normal-submit-pending-branch").attr("disabled", true);
                    $(".btn-action-verification").attr("disabled", true);
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: $(this).data("action"),
                        data: $form.serialize(),
                        success: function (data) {
                            $("#resultContainer").html(data);
                        }
                    });
                }
            }

        });

        $(".normal-submit-Manual", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");

            if ($.trim($('#CCUActivation').val()) == '0') {
                alert("CCUActivation not activated");
                return;
            }
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                $(".normal-submit-Verification").attr("disabled", true);
                $(".btn-action-verification").attr("disabled", true);



                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {

                        // alert("Successfully Manual Marked.");
                        //$("#closeBtn").trigger('click')
                        //$('#myModal').modal('hide')
                        $("#resultContainer").html(data);


                    }
                });

            }

        });


        $(".normal-submit-Autorizer", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");

            if ($.trim($('#CCUActivation').val()) == '0') {
                alert("CCUActivation not activated");
                return;
            }
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                $(".normal-submit-Verification").attr("disabled", true);
                $(".btn-action-verification").attr("disabled", true);
                $(".normal-submit-Autorizer").attr("disabled", true);



                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {

                        // alert("Successfully Manual Marked.");
                        //$("#closeBtn").trigger('click')
                        //$('#myModal').modal('hide')
                        $("#resultContainer").html(data);


                    }
                });

            }

        });






        $(".normal-submit-Manual-Returned", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");
            if ($.trim($('#CCUActivation').val()) == '0') {
                alert("CCUActivation not activated");
                return;
            }

            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                $(".normal-submit-Verification").attr("disabled", true);
                $(".btn-action-verification").attr("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        //$("#resultContainer").html(data);
                        alert("Successfully Returned.");
                    }
                });

            }

        });
        $(".normal-submit-CreditPosting", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            if ($.trim($('#CCUActivation').val()) == '0') {
                alert("CCUActivation not activated");
                return;
            }

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");

            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                $(".normal-submit-Verification").attr("disabled", true);
                $(".btn-action-verification").attr("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        if ($('.alert-danger').children(0).text() == '') {
                            alert("Successfully Posted.");
                        }

                        $("#resultContainer").html(data);

                    }
                });

            }

        });



        $(".normal-submit-late-maintenance", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                var r = confirm("Are you sure want to generate late return for this item?")

                if (r == true) {
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: $(this).data("action"),
                        data: $form.serialize(),
                        success: function (data) {
                            $("#right-panel").html(data);
                        }
                    });
                }
            }

        });


        $(".normal-submit-without-search", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                //alert("Scripts\App\App.js");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $form.serialize(),
                success: function (data) {
                    $("#right-panel").html(data);
                }
            });

        });

        $(".normal-submit-branchActivation", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {


                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        $("#right-panel").html(data);
                    }
                });

            }

        });
        $(".normal-submit-confirm-action", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {


                input = "";
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        $("#resultContainer").html(data);
                    }
                });


            }

        });

        $(".normal-submit-confirm-action-DataEntry", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                var objHfdChqAmt = $("#hfdChqAmt").val();
                if (objHfdChqAmt == "") {
                    alert("Account Listing Cannot be Empty.");
                }
                else {
                    var objhfAccountNameStatus = $("#hfAccountNameStatus").val();
                    var objhfZeroAmount = $("#hfZeroAmount").val();
                    if (objhfAccountNameStatus == "true") {
                        alert("Invalid GL Code OR Account Number Found in the Listing.");
                        return;
                    }
                    else if (objhfZeroAmount == "true") {
                        alert("Invalid Amount Found in the Listing. Please Enter Correct Amount.");
                        return;
                    }
                    else {

                        input = "";
                        $.ajax({
                            cache: false,
                            type: "POST",
                            url: $(this).data("action"),
                            data: $form.serialize(),
                            success: function (data) {
                                $("#resultContainer").html(data);
                            }
                        });
                    }

                }
            }

        });

        $(".normal-submit-confirm-action-Balancing", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                //debugger
                var DiffAmount = $("#txtDiffAmount").val();
                if (DiffAmount != "0.00" && DiffAmount != "0") {
                    alert("Amount is not Balanced.");
                    return;
                }
                else {

                    input = "";
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: $(this).data("action"),
                        data: $form.serialize(),
                        success: function (data) {
                            $("#resultContainer").html(data);
                        }
                    });

                }
            }

        });

        $(".normal-submit-confirm-action-button", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {


                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        $("#right-panel").html(data);
                    }
                });


            }

        });

        $(".normal-submit-confirm-action-button-refresh", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            var $actionRefresh = $(this).data("action");
            $actionRefresh = $actionRefresh.replace("Import", "ProgressBar");

            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {

                        $("#right-panel").html(data);

                        popup($actionRefresh);

                        //$("#resultContainer").html(data);

                    }
                });

            }
        });

        $(".normal-submit-Verification-routereason", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");
            if ($.trim($('#CCUActivation').val()) == '0') {
                alert("CCUActivation not activated");
                return;
            }

            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                $(".normal-").attr("disabled", true);
                $(".btn-action-verification").attr("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {

                        alert('Reasons Save Successfully');
                        $('#myModal').modal('hide')
                        $("#resultContainer").html(data);

                        //$("#closeBtn").trigger('click')
                    }
                });

            }

        });

        $(".normal-submit-Verification-returnreason", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");
            if ($.trim($('#CCUActivation').val()) == '0') {
                alert("CCUActivation not activated");
                return;
            }
            $("#myModal").modal();
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {

                $(".normal-").attr("disabled", true);
                //    $(".btn-action-verification").attr("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {

                        alert('Reasons Save Successfully');
                        $('#myModal').modal('hide')
                        $("#resultContainer").html(data);
                        //$("#closeBtn").trigger('click')
                    }
                });

            }

        });






        $(".btn-Generate", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");

            var $actionRefresh = $(this).data("action");
            $actionRefresh = $actionRefresh.replace("Generate", "ProgressBar");

            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                $(".normal-").attr("disabled", true);
                $(".btn-action-verification").attr("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        //$("#right-panel").html(data);
                        //$("#resultContainer").html(data);
                        //$("#myModal .modal-body").html(data)
                        //$("#myModal").modal();
                        //alert("Data Save Successfully");
                        popup($actionRefresh);
                    }
                });

            }

        });


        $(".btn-DayEndSettlement", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            //setTimeout(function () {
            //    $('#btnTest').removeAttr("disabled");
            //}, 30000);
            var $form = $(this).closest("form");

            var $actionRefresh = $(this).data("action");
            $actionRefresh = $actionRefresh.replace("Generate", "ProgressBar");

            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                $(".btn-DayEndSettlement").attr("disabled", true);
                $(".btn-action-verification").attr("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        //$("#right-panel").html(data);
                        //$("#resultContainer").html(data);
                        //$("#myModal .modal-body").html(data)
                        //$("#myModal").modal();
                        //alert("Data Save Successfully");

                         setTimeout(function () {
                        $("#notice .notice-body").html(data.notice)
                        $("#notice").removeClass("hidden");
                    }, 500);
                        
                    }
                });

            }

        });



        /*** Upload and Progress Bar code  
         * Author : Ali */
        $(".normal-submit-confirm-action-button-browse", context).off("change").on("change", function (e) {
            e.preventDefault()

            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {

                        setTimeout(function () {

                            refreshPage();
                        }, 10000);
                    }
                });

            }
        });
        $(".normal-submit-confirm-action-button-upload", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            var $form = $(this).closest("form");

            var $actionRefresh = $(this).data("action");
            $actionRefresh = $actionRefresh.replace("Upload", "");

            var $fileUpload = $("#file").get(0);
            var $fileData = $form;
            var $formData = new FormData();

            var $files = $fileUpload.files;

            for (var i = 0; i < $files.length; i++) {
                if ($files[i].name.indexOf('.zip') != -1) {
                    $formData.append($files[i].name, $files[i]);
                }
                else {
                    alert('Please Select Zip file');
                    return;
                }


            }
            $formData.append('fldProcessDate', $files[0].name);

            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                /*$form.serialize()*/
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $formData,//'/ICS/Import'
                    success: function (data) {
                        $("#myModal .modal-body").html(data)
                        $("#myModal").modal();

                        refreshPageAfterUpload($form, $actionRefresh);
                        setTimeout(function () {

                            refreshPage();
                        }, 10000);
                    },
                    contentType: false,
                    processData: false

                });

            }
        });


        function refreshPageAfterUpload($form, $actionRefresh) {

            $.ajax({
                cache: false,
                type: "POST",
                url: $actionRefresh,//'/ICS/MICRImage',//data: $form,
                data: $form,
                success: function (data) {

                    //console.log(data);
                    $("#right-panel").html(data);

                }


            });
        }


        function popup($actionRefresh) {


            $('.progress-bar').css('width', "0" + '%');
            $(".percentage").html("0%");
            $(".result-bar").html("ICSImport");
            let dataParam = "Import=" + "Import"
            CallApi($actionRefresh, dataParam, true);
            //$(".overlay,.popup").fadeIn();
            //StartProgrssBar(1);

        }


        //progress bar function
        function StartProgrssBar(second1) {
            //var currentDate = new Date();
            //var second = currentDate.getSeconds();
            let value = "";
            //alert(second1);
            let second = second1;
            if (second < 10) {
                value = 10 * second;
                if (second === 2) {

                    $(".result-bar").html("ICSImport")
                    $(".percentage").html(value + "%")
                    $('.progress-bar').css('width', value + '%');

                    //setTimeout(function () { StartProgrssBar(second) }, 500);

                }
                if (second === 4) {
                    $(".result-bar").html("Transferring ICL  Image Files to Inward Clearing System(BytesofZip)")
                    $(".percentage").html(value + "%")
                    $('.progress-bar').css('width', value + '%');
                    //setTimeout(function () { StartProgrssBar(second) }, 500);

                }
                if (second === 6) {
                    $(".result-bar").html("Transferring ICL Image Files was Completed.")
                    $(".percentage").html(value + "%")
                    $('.progress-bar').css('width', value + '%');
                    //setTimeout(function () { StartProgrssBar(second) }, 500);

                }
                if (second === 8) {
                    $(".result-bar").html("Extracting ICL Image Files(extractedfilesize).")
                    $(".percentage").html(value + "%")
                    $('.progress-bar').css('width', value + '%');
                    //setTimeout(function () { StartProgrssBar(second) }, 500);

                }
            }
            if (second === 10) {
                value = 10 * second;
                $(".result-bar").html("Extracting ICL Image Files was Completed.")
                $(".percentage").html(value + "%")
                $('.progress-bar').css('width', value + '%');
                //setTimeout(function () { StartProgrssBar(second) }, 5000);



            }



            value = "";
            second = "";


        }

        //function StartProgrssBar(second) {
        //    //var currentDate = new Date();
        //    //var second = currentDate.getSeconds();
        //    var value = "";
        //    if (second < 10) {
        //        value = 10 * second;
        //        if (second == 2) {
        //            $(".result-bar").html("ICSImport")
        //            $(".percentage").html(value + "%")

        //        }
        //        if (second == 4) {
        //            $(".result-bar").html("Transferring ICL  Image Files to Inward Clearing System(BytesofZip)")
        //            $(".percentage").html(value + "%")
        //        }
        //        if (second == 6) {
        //            $(".result-bar").html("Transferring ICL Image Files was Completed.")
        //            $(".percentage").html(value + "%")
        //        }
        //        if (second == 8) {
        //            $(".result-bar").html("Extracting ICL Image Files(extractedfilesize).")
        //            $(".percentage").html(value + "%")
        //        }               
        //    }
        //    if (second == 10) {
        //        value = 10 * second;
        //        $(".result-bar").html("Extracting ICL Image Files was Completed.")
        //        $(".percentage").html(value + "%")


        //    }

        //    if (second < 9) { second = second + 1; } 


        //     $('.progress-bar').css('width', value + '%');



        //    setTimeout(function () { StartProgrssBar(second) },5000);
        //    value = "";




        //}

        //close Progress Bar
        function CloseProgressBar() {
            $('#Fade_area').removeAttr("style");
            $('#mymodal').removeAttr("style");

            $('.progress-bar').css('width', "0" + '%');
            $(".percentage").html("0%");
            $(".result-bar").html("ICSImport");


        }


        function CallApi($actionRefresh, $formData, showPopup) {
            //alert($actionRefresh);

            $.ajax({
                cache: false,
                type: "POST",
                url: $actionRefresh,//'/ICS/MICRImage/ProgressBar',//data: $form,
                data: $formData,
                beforeSend: function () {
                    if (showPopup) {
                        $(".overlay,.popup").fadeIn();
                    }
                },
                success: function (data) {
                    console.log(data);
                    if (data[0] == "" && data[1] === "Import") {
                        console.log('in empty import');
                        StartProgrssBar(1)
                        CallApi($actionRefresh, $formData)
                    }

                    if (data[0] === "1" && data[1] === "Import") {
                        console.log('in 1');
                        StartProgrssBar(2)
                        CallApi($actionRefresh, $formData)

                    }
                    if (data[0] === "2" && data[1] === "Import") {
                        console.log('in 2');
                        StartProgrssBar(4)
                        CallApi($actionRefresh, $formData)

                    }
                    if (data[0] === "4" && data[1] === "Import") {
                        console.log('in 4');

                        StartProgrssBar(6)
                        let chequeconversion = "Import=" + "ChequeConversion";

                        CallApi($actionRefresh, chequeconversion)

                    }
                    if (data[0] == "" && data[1] === "ChequeConversion") {
                        console.log('in empty ChequeConversion');
                        CallApi($actionRefresh, $formData)
                    }
                    if (data[0] == "1" && data[1] === "ChequeConversion") {
                        console.log('data is 1 and cheque conversion');
                        CallApi($actionRefresh, $formData)
                    }
                    if (data[0] === "2" && data[1] === "ChequeConversion") {
                        console.log('data is 2 and cheque conversion')
                        StartProgrssBar(8)

                        CallApi($actionRefresh, $formData);
                    }
                    if (data[0] === "4" && data[1] == "ChequeConversion") {
                        console.log('data is 4 and cheque conversion')
                        StartProgrssBar(10);
                        setTimeout(function () { CloseProgressBar() }, 500);

                    }
                },
                error: function (e) {
                    console.log('Error');
                    console.log(e);
                    setTimeout(function () { CloseProgressBar() }, 10)
                }
            });
        }


        $(".normal-submit-checked-action-button", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            let dataSentToServer = [];
            $("input[type=checkbox][name^=chk_genuineCheckvalue_]").each(function () {
                if ($(this).prop('value') != '') {
                    dataSentToServer.push({
                        Cheque: $(this).prop('value'),
                        IsGenuine: $(this).prop('checked')

                    });
                }
                
            });
            $('#chk_genuineCheckvalue_').val(JSON.stringify(dataSentToServer));
            
            var $form = $(this).closest("form");


            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {



                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(), //{  list: dataSentToServer }    ,
                    //dataType: "json",
                    success: function (data) {
                     //   alert('Marked Cheques Genuine')
                   
                        //$("#right-panel").html(data);
                    }
                });


            }

        });





        $(".normal-submit-save-action-button", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {



                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        $("#right-panel").html(data);
                    }
                });
            }



        });

        $(".normal-submit-verifysave-action-button", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {



                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        $("#right-panel").html(data);
                    }
                });
            }



        });




        $(".normal-submit-delete-action-button", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {



                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        $("#right-panel").html(data);
                    }
                });


            }

        });






        $(".normal-submit-verifydelete-action-button", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {



                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        $("#right-panel").html(data);
                    }
                });


            }

        });

        $(".normal-submit-CustomConfirm-action-button", context).off("click.g").on("click.g", function (e) {
            e.preventDefault();
            var $form = $(this).closest("form");
            var msg = $(this).attr("confirm-msg");
            if (msg == undefined) {
                msg = "Confirm Delete?"
            }
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            else {
                var r = confirm(msg)
                if (r == true) {
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: $(this).data("action"),
                        data: $form.serialize(),
                        success: function (data) {
                            $("#right-panel").html(data);
                        }
                    });
                }
            }
        });

    }

    var bindSecureNav = function () {
        $(".secure-nav", context).off('click.g').on('click.g', function (e) {
            e.preventDefault();
            $.ajax({
                cache: false,
                type: "POST",
                url: this.href,
                success: function (data) {
                    $("#right-panel").html(data);
                }
            });
        });
    }

    var bindModalPrint = function () {

        $(".modal-print", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            var title = $(this).data('title');
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                success: function (data) {
                    if (title != undefined) { $("#myModal .modal-title").html(title) }
                    else { $("#myModal .modal-header").remove() }
                    $("#myModal .modal-body").html(data)
                    $("#myModal").modal();

                    // xx start
                    //alert("b");
                    //Remove modal input
                    //$("#myModal .modal-body #search-fields-section div.row:first").remove();

                    //clone input and append to modal to user form collection
                    //var dataArray = $("input, select").not("[name^='row_'],[name='reportType']").serializeArray();
                    //$(dataArray).each(function (i, field) {
                    //    $(".modal-body #searchForm").append('<input type="hidden" name="' + field.name + '" value = "' + field.value + '" />');
                    //});
                    // xx end

                }
            });
        });
    }

    var bindModalSubmit = function () {
        -
            $(".modal-submit", context).off("click.g").on("click.g", function (e) {
                e.preventDefault()
                var $form = $(this).closest("form");
                var title = $(this).data('title');

                if ($(this).data("action") == null) {
                    alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                    return;
                }
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $form.serialize(),
                    success: function (data) {
                        if (title != undefined) { $("#myModal .modal-title").html(title) }
                        else { $("#myModal .modal-header").remove() }
                        $("#myModal .modal-body").html(data)
                        $("#myModal").modal();
                    }
                });
            });
    }

    var bindRowModalSubmit = function () {
        $(".row-modal-submit", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            var title = $(this).data('title');
            alert('this');
            // if ($form.attr("action")) { url = $form.attr("action"); }
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $(this).parents("tr").find(".row-result-params input").serialize(),
                success: function (data) {
                    if (title != undefined) { $("#myModal .modal-title").html(title) }
                    else { $("#myModal .modal-header").remove() }
                    $("#myModal .modal-body").html(data)
                    $("#myModal").modal();
                }
            });
        })
    }
    var bindRowModalPostedItemDetails = function () {
        $(".row-modal-submit-Posted-detail", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            var title = $(this).data('title');
            // if ($form.attr("action")) { url = $form.attr("action"); }
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $(this).parents("tr").find(".row-result-params input").serialize(),
                success: function (data) {
                    if (title != undefined) { $("#myModal .modal-title").html(title) }
                    else { $("#myModal .modal-header").remove() }
                    $("#myModal .modal-body").html(data)
                    $("#myModal").modal();
                }
            });
        })
    }
    var bindRowModalSubmitOutwardDetail = function () {
        $(".row-modal-submit-outward-detail", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            // if ($form.attr("action")) { url = $form.attr("action"); }
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $(this).parents("tr").find(".row-result-params input").serialize(),
                success: function (data) {
                    $("#outwardDetailModal .modal-body").html(data)
                    $("#outwardDetailModal").modal();
                }
            });
        })
    }
    var bindRowActionSendFTP = function () {
        $(".row-action-sendFTP", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $(this).parents("tr").find(".row-result-params input").serialize(),
                success: function (data) {
                    //console.log(data.notice)
                    $(".search").trigger('click')
                    setTimeout(function () {
                        $("#notice .notice-body").html(data.notice)
                        $("#notice").removeClass("hidden");
                    }, 500);
                }
            });
        })
    }

    var BindRowActionRegenerateCPFile = function () {
        $(".row-action-RegenerateCP", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $(this).parents("tr").find(".row-result-params input").serialize(),
                success: function (data) {
                    //console.log(data.notice)
                    //$(".search").trigger('click')
                    $("#notice .notice-body").html(data.notice)
                    $("#notice").removeClass("hidden");
                    setTimeout(function () {
                        $("#notice .notice-body").html("")
                        $("#notice").addClass("hidden");
                    }, 5000);
                }
            });
        })
    }

    var bindRowActionSendLFTP = function () {
        $(".row-action-sendLFTP", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $(this).parents("tr").find(".row-result-params input").serialize(),
                success: function (data) {
                    //console.log(data.notice)
                    $(".search").trigger('click')
                    //setTimeout(function () {
                    //    $("#notice .notice-body").html(data.notice)
                    //    $("#notice").removeClass("hidden");
                    //}, 500);
                }
            });
        })
    }

    var bindRowActionDownload = function () {

        $(".row-action-download", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            var inputfileName = $(this).parents("tr").find(".row-result-params input[name=row_fldfilename]")
            var inputfilePath = $(this).parents("tr").find(".row-result-params input[name=row_fldfilepath]")

            var $form = $(this).closest("form");

            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");

            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }

            inputfileName.attr("name", "this_fldfilename")
            inputfilePath.attr("name", "this_fldfilepath")

            $form.attr("target", "_blank");
            $form.attr("action", targetAction);

            $form.off("submit");
            $form.submit();

            bindSecureForm();

            inputfileName.attr("name", "row_fldfilename")
            inputfilePath.attr("name", "row_fldfilepath")
            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });
        $(".row-action-downloadUPI", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var inputfileName = $(this).parents("tr").find(".row-result-params input[name=row_fldFileName]")
            var inputfilePath = $(this).parents("tr").find(".row-result-params input[name=row_fldFilePath]")
            var $form = $(this).closest("form");
            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");
            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            inputfileName.attr("name", "this_fldFileName")
            inputfilePath.attr("name", "this_fldFilePath")
            $form.attr("target", "_blank");
            $form.attr("action", targetAction);
            $form.off("submit");
            $form.submit();
            bindSecureForm();
            inputfileName.attr("name", "row_fldFileName")
            inputfilePath.attr("name", "row_fldFilePath")
            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });
    }
    var bindRowActionDownloadPostingFile = function () {
        $(".row-action-download-postingfile", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var inputfileName = $(this).parents("tr").find(".row-result-params input[name=row_fldPostingFileName]")
            var inputfilePath = $(this).parents("tr").find(".row-result-params input[name=row_fldPostingFilePath]")
            var $form = $(this).closest("form");
            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");
            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            inputfileName.attr("name", "this_fldfilename")
            inputfilePath.attr("name", "this_fldfilepath")
            $form.attr("target", "_blank");
            $form.attr("action", targetAction);
            $form.off("submit");
            $form.submit();
            bindSecureForm();
            inputfileName.attr("name", "row_fldfilename")
            inputfilePath.attr("name", "row_fldfilepath")
            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });
    }
    // debugger;
    var bindRowActionDownloadInwardFile = function () {
        //debugger;
        $(".row-action-download-inwardfile", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            //debugger;
            var inputfileName = $(this).parents("tr").find(".row-result-params input[name=row_fldFileName]")
            var inputfileBatch = $(this).parents("tr").find(".row-result-params input[name=row_fldIRDBatch]")
            var inputbankcode = $(this).parents("tr").find(".row-result-params input[name=row_PresentingBankCode]")
            var $form = $(this).closest("form");
            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");
            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            inputfileName.attr("name", "this_fldFileName")
            inputfileBatch.attr("name", "this_fldIRDBatch")
            inputbankcode.attr("name", "this_PresentingBankCode")
            $form.attr("target", "_blank");
            $form.attr("action", targetAction);
            $form.off("submit");
            $form.submit();
            bindSecureForm();
            inputfileName.attr("name", "row_fldFileName")
            inputfileBatch.attr("name", "row_fldIRDBatch")
            inputbankcode.attr("name", "row_PresentingBankCode")
            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });
    }
    var bindRowActionDownloadInwardDoc = function () {
        $(".row-action-download-inward", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            //debugger;
            var inputfileName = $(this).parents("tr").find(".row-result-params input[name=row_fldfilename]")
            var inputfilePath = $(this).parents("tr").find(".row-result-params input[name=row_fldfilepath]")
            var inputItemID = $(this).parents("tr").find(".row-result-params input[name=row_fldiriteminitialid]")
            var $form = $(this).closest("form");
            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");
            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            inputfileName.attr("name", "this_fldfilename")
            inputfilePath.attr("name", "this_fldfilepath")
            inputItemID.attr("name", "this_fldiriteminitialid")
            $form.attr("target", "_blank");
            $form.attr("action", targetAction);
            $form.off("submit");
            $form.submit();
            bindSecureForm();
            inputfileName.attr("name", "row_fldfilename")
            inputfilePath.attr("name", "row_fldfilepath")
            inputItemID.attr("name", "row_fldiriteminitialid")
            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });
    }

    var bindRowActionRelease = function () {

        $(".row-action-release", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            bindSecureForm();

            if (confirm('Are you sure you want to perform End Of Day?')) {
                $(this).attr("disabled", "disabled");
                var inputBranchCode = $(this).parents("tr").find(".row-result-params input[name=row_fldBranchCode]")
                var $form = $(this).closest("form");

                var tempHTML = $(this).parents("tr").html();
                tempHTML = tempHTML.toString();
                tempHTML = tempHTML.replace("Done", "Not Done");
                $(this).parents("tr").html(tempHTML);

                var strBranchId = inputBranchCode.val();
                var strAmount = "0";
                var strDifference = "0";
                var strEODStatus = "0";

                //$.ajax({
                //    async: false,
                //    cache: false,
                //    url: App.ContextPath + "CommonApi/OCSInsertBranchEndOfDay",
                //    method: "POST",
                //    data: "strBranchId=" + strBranchId + "&strAmount=" + strAmount + "&strDifference=" + strDifference + "&strEODStatus=" + strEODStatus,
                //    success: function (data) {
                //        blnSuccess = true;
                //    }
                //});
                $.ajax({
                    async: false,
                    cache: false,
                    url: App.ContextPath + "CommonApi/OCSHubInsertBranchEndOfDay",
                    method: "POST",
                    data: "strBranchId=" + strBranchId + "&strEODStatus=" + strEODStatus,
                    success: function (data) {
                        //blnSuccess = true;
                        alert("End of Day performed successfully");
                        submitLatestData();
                    }
                });
            } else {
                return;
            }
        });

        $(".row-action-EODRelease", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            bindSecureForm();

            if (confirm('Are you sure you want to release End Of Day?')) {
                $(this).attr("disabled", "disabled");
                var inputBranchCode = $(this).parents("tr").find(".row-result-params input[name=row_fldBranchCode]")
                var $form = $(this).closest("form");

                var tempHTML = $(this).parents("tr").html();
                tempHTML = tempHTML.toString();
                tempHTML = tempHTML.replace("Not Done", "Done");
                $(this).parents("tr").html(tempHTML);

                var strBranchId = inputBranchCode.val();
                var strAmount = "0";
                var strDifference = "0";
                var strEODStatus = "0";

                $.ajax({
                    async: false,
                    cache: false,
                    url: App.ContextPath + "CommonApi/OCSDeleteBranchEndOfDay",
                    method: "POST",
                    data: "strBranchId=" + strBranchId + "&strEODStatus=" + strEODStatus,
                    success: function (data) {
                        //blnSuccess = true;
                        alert("End of Day released successfully");
                        submitLatestData();

                    }
                });
            } else {
                return;
            }
        });
    }

    function submitLatestData() {
        //empty container to give way other script
        $('#resultContainer').empty();
        var $form = $(".init-search").closest('form');
        $form.submit();
        $('#resultContainer').reload();
    }

    var bindActionDownloadPrint = function () {

        $(".action-download-print", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            var $form = $(this).closest("form");

            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");

            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            if ($("#" + $form.attr("id") + " input:checkbox:checked").length > 1) {
                alert("Please select only 1 record to print")
                return;
            }
            else if ($("#" + $form.attr("id") + " input:checkbox:checked").length == 0) {
                alert("Please select 1 record to print")
                return;
            }


            $form.attr("target", "_blank");
            $form.attr("action", targetAction);

            $form.off("submit");
            $form.submit();

            bindSecureForm();

            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });

        $(".action-download-print-radio", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            var $form = $(this).closest("form");

            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");

            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            if ($("#" + $form.attr("id") + " input:radio:checked").length > 1) {
                alert("Please select only 1 record to print")
                return;
            }
            else if ($("#" + $form.attr("id") + " input:radio:checked").length == 0) {
                alert("Please select 1 record to print")
                return;
            }


            $form.attr("target", "_blank");
            $form.attr("action", targetAction);

            $form.off("submit");
            $form.submit();

            bindSecureForm();

            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });
    }

    var bindActionDownload = function () {

        $(".action-download", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            var $form = $(this).closest("form");

            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");

            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }

            $form.attr("target", "_blank");
            $form.attr("action", targetAction);

            $form.off("submit");
            $form.submit();

            bindSecureForm();

            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });
    }

    $(".action-download-cts", context).off("click.g").on("click.g", function (e) {
        e.preventDefault()
        var $form = $(this).closest("form");

        //var prevTarget = "_blank";
        //var prevAction ="/CTS/TransactionEnquiry/TransactionEnquiryRetrieverPage";
        //var targetAction = $(this).data("action");
        var prevTarget = $form.attr("target");
        var prevAction = $form.attr("action");
        var targetAction = $(this).data("action");

        if (targetAction == null) {
            alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
            return;
        }

        $form.attr("target", "_blank");
        $form.attr("action", targetAction);

        $form.off("submit");
        $form.submit();

        bindSecureForm();

        $form.attr("target", prevTarget);
        $form.attr("action", prevAction);

    });

    var bindRowActionBranch = function () {

        $(".row-action-branch", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            var inputItemID = $(this).parents("tr").find(".row-result-params input[name=row_fldItemInitialID]")
            var inputBranchCode = $(this).parents("tr").find(".row-result-params input[name=row_fldCapturingBranch]")
            var inputCaptureDate = $(this).parents("tr").find(".row-result-params input[name=row_CapturingDate]")
            var inputBatchNo = $(this).parents("tr").find(".row-result-params input[name=row_fldBatchNumber]")
            var inputScannerID = $(this).parents("tr").find(".row-result-params input[name=row_fldScannerID]")
            var $form = $(this).closest("form");

            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");

            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }

            inputItemID.attr("name", "this_fldItemInitialID")
            inputBranchCode.attr("name", "this_fldCapturingBranch")
            inputCaptureDate.attr("name", "this_CapturingDate")
            inputBatchNo.attr("name", "this_fldBatchNumber")
            inputScannerID.attr("name", "this_fldScannerID")
            $form.attr("target", "_blank");
            $form.attr("action", targetAction);

            $form.off("submit");
            $form.submit();

            bindSecureForm();


            inputItemID.attr("name", "row_fldItemInitialID")
            inputBranchCode.attr("name", "row_fldCapturingBranch")
            inputCaptureDate.attr("name", "row_CapturingDate")
            inputBatchNo.attr("name", "row_fldBatchNumber")
            inputScannerID.attr("name", "row_fldScannerID")
            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });
    }

    var bindActionBranch = function () {

        $(".action-branch", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()

            var $form = $(this).closest("form");

            var prevTarget = $form.attr("target");
            var prevAction = $form.attr("action");
            var targetAction = $(this).data("action");

            if (targetAction == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }

            $form.attr("target", "_blank");
            $form.attr("action", targetAction);

            $form.off("submit");
            $form.submit();

            bindSecureForm();

            $form.attr("target", prevTarget);
            $form.attr("action", prevAction);
        });
    }
    var bindRowAjaxSubmit = function () {


        $(".row-action-submit-ajax", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            var url = "";
            // if ($form.attr("action")) { url = $form.attr("action"); }
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $(this).parents("tr").find(".row-result-params input").serialize(),
                success: function (data) {
                    $("#notice .notice-body").html(data.notice)
                    $("#notice").removeClass("hidden");
                }
            });

        });

    }

    var bindRowAjaxSubmitRefresh = function () {
        $(".row-action-submit-ajax-refresh", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            bindSecureForm();
            if (confirm('Are you sure you want to regenerate?')) {
                var $form = $(this).closest("form");
                var url = "";
                if ($(this).data("action") == null) {
                    alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                    return;
                }
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data("action"),
                    data: $(this).parents("tr").find(".row-result-params input").serialize(),
                    success: function (data) {
                        alert("Regenerating file has been triggered");
                        setTimeout(function () {
                            refreshPage();
                        }, 10000);
                    }
                });
            }
            else {
                return;
            }
        });
    }
    function refreshPage() {
        //empty container to give way other script
        $('#resultContainer').empty();
        var $form = $(".init-search").closest('form');
        $form.submit();
    }
    var bindModalPageLoad = function () {
        //initialize button with modal load from another page
        //use <button class="modal-page-load" data-modal-href="/href/bla" data-modal-title="modal Title">
        $(".modal-page-load", context).off('click.g').on('click.g', function (e) {
            e.preventDefault();
            var title = $(this).data('modal-title');
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data('modal-href'),
                success: function (data) {
                    $("#myModal .modal-title").html(title)
                    $("#myModal .modal-body").html(data)
                    $("#myModal").modal();
                }
            });
        })



    }

    var bindAjaxSubmit = function () {

        $(".ajax-submit", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $form.serialize(),
                success: function (data) {
                    $("#notice .notice-body").html(data.notice)
                    $("#notice").removeClass("hidden");

                }
            });

        });

    }

    var bindAjaxProceedPasswordSubmit = function () {
        $(".ajax-proceed-password-submit", context).off("click.g").on("click.g", function (e) {
            e.preventDefault()
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $form.serialize(),
                success: function (data) {
                    //console.log(data)
                    $("#myModal").modal('toggle');
                    $("#notice .notice-body").html(data.notice)
                    $("#notice").removeClass("hidden");
                    $("#searchForm").submit();
                }
            });

        });

    }

    var initMomentInterval = function () {
        //Add day and date to header
        function momentDayTime() {
            var currentDayAndTime = moment().format('ddd, MMM DD, YYYY h:mm:ss A');
            $(".currentDayAndTime").html(currentDayAndTime);
        }
        //initiate for first load
        momentDayTime()
        // refresh every 1 sec
        setInterval(function () {
            momentDayTime()
        }, 1000);
    }

    var initToggleSubMenu = function () {
        $(".switcher", context).off("click.g").on("click.g", function () {
            $(".left-panel").toggle();
            $(".switcher-icon").toggleClass("glyphicon-arrow-right");
        })
    }

    var preventRightClick = function () {
        $(document).off("contextmenu.g").on("contextmenu.g", function () {
            bootbox.alert("Right click is prohibited")
            return false;
        });
    }


    var bindkeyupevent = function () {
        $(document).off("contextmenu.g").on("contextmenu.g", function () {
            bootbox.alert("Right click is prohibited")
            return false;
        });
    }




    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    // INITIAL BINDING
    /////////////////////////////////////////////////////////////////////////////////////////////////////////


    var bindFormAction = function () {
        bindModalPageLoad();
        bindSecureForm();
        bindSecureNav();

        bindNormalSubmit();
        bindActionDownload();
        bindActionDownloadPrint
        bindActionBranch();
        bindModalSubmit();
        bindAjaxSubmit();
        bindAjaxProceedPasswordSubmit();
        bindModalPrint();

        bindRowSubmit();
        bindRowActionDownload();
        bindRowActionRelease();
        bindRowActionBranch();
        bindRowModalSubmit();
        bindRowAjaxSubmit();
        bindRowAjaxSubmitRefresh();
        bindRowModalSubmitOutwardDetail();
        bindRowModalPostedItemDetails();
        bindRowActionSendFTP();
        BindRowActionRegenerateCPFile();
        bindRowActionSendLFTP();
        bindRowActionDownloadInwardFile();
        bindRowActionDownloadInwardDoc();
        bindRowActionDownloadPostingFile();

    };


    this.initOnce = function () {
        initToggleSubMenu();
        initMomentInterval();
    }


    this.initBind = function (context) {
        context = context || document;
        initBootstrap();
        //preventRightClick();
        initBootbox();
        bindFormAction();
        prepareDataTable();
        bindDateForCalendar();
        Pagination.paginate();
        bindPreventEnterKey();
        removeAutoComplete();
        bindInputAsMoney();

    };

    this.ContextPath = $("#contextPath").html();



};

(function () {
    //$(document).keypress(function (event) {
    //    var keycode = (event.keyCode ? event.keyCode : event.which);
    //    if (keycode == '13') {
    //        alert('You pressed a "enter" key in somewhere');
    //        event.preventDefault();
    //    }
    //});

    $("#atm").keypress(function (e) {
        if (e.keyCode == 13) {
            alert("amir");
        }
    });

    $(document).ready(function () {
        App.initBind();
        App.initOnce();
    });
})();