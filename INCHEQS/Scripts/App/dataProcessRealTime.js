
(function () {
    $(document).ready(function () {

        bindRefreshPageRealtime();
        bindSearchButton();


        if ($("#dataProcessContainer ul li:last #processStatus").text() != "4") {
            console.log("!=")
            getLatestProcess();
        }

    });

    function bindSearchButton() {
        $(".search").on('click', function (e) {
            //console.log("bindSearchButton")
            getLatestProcess()
        })
    }

    function bindRefreshPageRealtime() {

        $(".init-refresh-page-realtime").on('click', function (e) {
            
            var $this = $(this);
            var $form = $this.closest('form');
            //debugger;
            //alert($(this).data("action"));
            var $actionRefresh = $(this).data("action");
            
            if ($actionRefresh.includes('/ICS/DayEndProcess/SaveCreate') === true) {
                $actionRefresh = $actionRefresh.replace("SaveCreate", "ProgressBar");
            }
            else if ($actionRefresh.includes('/ICS/HostValidation/Generate') === true) {
                $actionRefresh = $actionRefresh.replace("Generate", "ProgressBar");

            }
            else if ($actionRefresh == '/ICS/GenerateUPI/Generate') {
                $actionRefresh = '/ICS/GenerateUPI/ProgressBar';// $actionRefresh.replace("/Generate", "/ProgressBar");

            }

            else {
                $actionRefresh = $actionRefresh.replace("SaveCreate", "ChequeVerification");

            }
            
            //debugger;
            $.ajax({
                cache: false,
                type: "POST",
                url: $(this).data("action"),
                data: $form.serialize(),
                success: function (data) {

                    if (data.ErrorMsg != "" && data.ErrorMsg != null) {
                        $("#ErrorMsg .notice-body").html(data.ErrorMsg)
                        $("#ErrorMsg").removeClass("ErrorMsg");
                    }
                    else { popup($actionRefresh); }
                    //var getchequesvalue = $('#totalcheques').val();
                    //var rcheques = confirm(data.notice);

                    //if (rcheques == true) {
                        //bindChequeVerification($this, $form, $actionRefresh);

                    //}
                    //if (data.notice != "" && data.notice != null) {
                    //    $("#notice .notice-body").html(data.notice)
                    //    $("#notice").removeClass("hidden");
                    //}
              


                }
            });
        })



    }

    function bindChequeVerification(thiss, form, $action) {

        $(".init-refresh-page-realtime").on('click', function (e) {
        var $this = thiss;//$(this);
        var $form = form;//$this.closest('form');
            
        var getchequesvalue = $('#totalcheques').val();
        //var rcheques = confirm('not yet processed?');
        //debugger;
        //if (rcheques == true) {
        var r = confirm("Execute the process. Are you sure?")
        if (r == true) {

            //1. Change url to the button url
            //2. submit
            //3. revert back to original action

            var originalAction = $form.attr("action");
            $form.attr("action", $this.data("action"))

            var $actionRefresh1 = $action;
            $actionRefresh1 = $actionRefresh1.replace("ChequeVerification", "SaveCreate");

            //'/ICS/DayEndProcess/SaveCreate',
            $.ajax({
                cache: false,
                type: "POST",
                url: $actionRefresh1,
                data: $form.serialize(),
                success: function (data) {
                    if (inwardProcessType == "ICL") {
                        //if ($("#notice .notice-body").html("Process Complete Successfully") > -1) {
                        //    //$("#notice").removeClass("alert-success");
                        //    //$("#notice").addClass("alert-danger");


                        //$("#notice .notice-body").html(data.notice);
                        //$("#notice").removeClass("hidden");
                    }
                    else {
                        if (data.notice != "" && data.notice != null) {
                            $("#notice .notice-body").html(data.notice)
                            $("#notice").removeClass("hidden");
                        }

                        if (data.ErrorMsg != "" && data.ErrorMsg != null) {
                            $("#ErrorMsg .notice-body").html(data.ErrorMsg)
                            $("#ErrorMsg").removeClass("ErrorMsg");
                        }
                    }
                }
            });
            $form.attr("action", originalAction);
            if ($(".notice-body").html().indexOf("ERROR") <= 0) {
                //Delay the process
                setTimeout(function () {
                    submitLatestProcess();
                }, 1000);
            }
        }

        //}



        })
    }


    function getLatestProcess() {

        //console.log("getLatestProcess")
        //debugger;
        var dataParam;
        var dataUrl;
        var dataProcessType = $("#dataProcessType").val();
        var inwardProcessType = $("#inwardProcessType").val();
        //alert(dataProcessType);
        //alert(inwardProcessType);
        console.log("cleardate=" + $("[name=fldProcessDate]").val() + "&processname=" + $("#dataProcessContainer").data("processname") + "")

        if (dataProcessType == "ICL") {
            
            //dataParam = "cleardate=" + $("[name=fldClearDate]").val() + "&pospaytype=" + $("#dataProcessContainer").data("pospaytype") + "&processname=" + $("#dataProcessContainer").data("processname");
            dataParam = "cleardate=" + $("[name=fldProcessDate]").val() + "&pospaytype=" + 'Import' + "&processname=" + $("#dataProcessContainer").data("processname");

            dataUrl = App.ContextPath + "DataProcessApi/AsJsonICL";
        } else if (dataProcessType == "ECCS") {
            dataParam = "cleardate=" + $("[name=fldClearDate]").val() + "&processname=" + $("#dataProcessContainer").data("processname") + "&filetype=" + $("[name=fileType]").val();
            dataUrl = App.ContextPath + "DataProcessApi/AsJsonECCS";
        } else if ($("#dataProcessContainer").data("processname") == "ICSEOD") {
            dataParam = "cleardate=" + $("[name=fldClearDate]").val() + "&processname=" + $("#dataProcessContainer").data("processname");
            dataUrl = App.ContextPath + "DataProcessApi/AsJsonEOD";
        }
         else if ($("#dataProcessContainer").data("processname") == "ICSEOD") {
            dataParam = "cleardate=" + $("[name=fldClearDate]").val() + "&pospaytype=" + $("#dataProcessContainer").data("pospaytype") + "&processname=" + $("#dataProcessContainer").data("processname");
            dataUrl = App.ContextPath + "DataProcessApi/AsJsonICL";
        }
        //else if (inwardProcessType == "ICL") {
        //    dataParam = "cleardate=" + $("[name=fldProcessDate]").val() + "&systemtype=" + $("#dataProcessContainer").data("systemtype") + "&processname=" + $("#dataProcessContainer").data("processname");
        //    dataUrl = App.ContextPath + "DataProcessApi/AsJsonInwardICL";
        //} 
        else if (dataProcessType == "LoadDailyFile") {
            dataParam = "cleardate=" + $("[name=fldClearingDate]").val() + "&processname=" + $("#dataProcessContainer").data("processname");
            dataUrl = App.ContextPath + "DataProcessApi/AsJsonLoadDailyFile";
        }
        else if ($("#dataProcessContainer").data("processname") == "Download & Import Host Status File"
            || $("#dataProcessContainer").data("processname") == "NonConforman"
            ||  dataUrl == App.ContextPath + "DataProcessApi/AsJson" ||
                $("#dataProcessContainer").data("processname") == "Download & Import Final Bank Host Status File"
                || $("#dataProcessContainer").data("processname") == "Download & Import 2nd Host Status File") {
            dataParam = "cleardate=" + $("[name=fldProcessDate]").val() + "&processname=" + encodeURIComponent($("#dataProcessContainer").data("processname"));
        }
        else {
            dataParam = "cleardate=" + $("[name=fldClearDate]").val() + "&processname=" + encodeURIComponent($("#dataProcessContainer").data("processname"));
            dataUrl = App.ContextPath + "DataProcessApi/AsJson";

        }
        if ($("#dataProcessContainer").length > 0) {
           
            //debugger;
            $.ajax({
                cache: false,
                url: dataUrl,
                method: "POST",
                data: dataParam,
                beforeSend: function () {
                    $(".resultLoader").toggleClass("hidden");
                },
                success: function (data) {
                    //alert(data);
                    $(".resultLoader").toggleClass("hidden");
                    var $ul = $("<ul />");
                    _.each(data, function (d) {
                        $ul.append("<li>" + d + "</li>")
                    });
                    $("#dataProcessContainer").html($ul);
                    if (data.length > 0) {
                        //if (dataProcessType == "ICL") {
                        //   // Check if last data or last 2 data contain "Complete"
                        //    if (data[data.length - 1].indexOf("Data was Completed") >= 0 ||
                        //    data[data.length - 2].indexOf("Data was Completed") >= 0 ||
                        //    data[data.length - 3].indexOf("Data was Completed") >= 0
                        //    ) {
                        //    $("#notice .notice-body").html("Process Complete")
                        //    clearTimeoutIfExist();
                        //    } else if (data[data.length - 1].indexOf("Error") >= 0) {
                        //    $("#notice .notice-body").html("Process Error")
                        //    clearTimeoutIfExist();
                        //    }
                        //}
                        if (inwardProcessType == "ICL") {
                            if ($("#dataProcessContainer ul li:last #processStatus").text() == "4") {
                                //$("#notice .notice-body").html("ICL File uploaded successfully.")
                                //clearTimeoutIfExist();
                                $("#notice").remove();
                                $(".alert").remove();
                                $(".notice-body").text("");
                                var html = ' <div id=notice" class="alert alert-success alert-dismissible" role="alert"> ';
                                html = html + ' <button type = "button" class="close" data-dismiss="alert" > <span aria-hidden="true">×</span> <span class="sr-only">Close</span></button > ';
                                html = html + ' <div class="notice-body">Process Complete Successfully</div>'
                                html = html + ' </div > ';
                                $(html).insertBefore("#searchForm");
                                //$form.attr("action", originalAction);
                                $("#notice").addClass("hidden");
                                clearTimeoutIfExist();
                            }
                            else if ($("#dataProcessContainer ul li:last #processStatus").text() == "5") {
                                //$("#notice").removeClass("alert-success");
                                //$("#notice").addClass("alert-danger");
                                //$("#notice .notice-body").html("ICL File uploaded fail.")
                                //$("#notice").removeClass("hidden");
                                ////$form.attr("action", originalAction);
                                //clearTimeoutIfExist();
                                $("#notice").remove();
                                $(".alert").remove();
                                $(".notice-body").text("");
                                var html = ' <div id=notice" class="alert alert-danger alert-dismissible" role="alert"> ';
                                html = html + ' <button type = "button" class="close" data-dismiss="alert" > <span aria-hidden="true">×</span> <span class="sr-only">Close</span></button > ';
                                html = html + ' <div class="notice-body">Download & Import fail.</div>'
                                html = html + ' </div > ';
                                $(html).insertBefore("#searchForm");
                                //$form.attr("action", originalAction);
                                clearTimeoutIfExist();
                            }
                        }
                        else {
                            if ($("#dataProcessContainer ul li:last #processStatus").text() == "4") {
                                //debugger;
                                console.log("==")
                                $("#notice .notice-body").html("Process Complete Successfully");
                                clearTimeoutIfExist();
                            }
                        }

                    } else {
                        //console.log("TEST")
                        //clearTimeoutIfExist();
                    }
                }
            })
        }

    }


    function submitLatestData() {
        //empty container to give way other script
        //$('#resultContainer').empty();
        var $form = $(".init-refresh-page-realtime").closest('form');
        $form.submit();
    }
    function submitLatestProcess() {
        //console.log("submitLatestProcess")
        debugger;
        getLatestProcess()
        timeout = setTimeout(submitLatestProcess, 1000);
        $(document).on('click', '.menu', function (e) {
            clearTimeoutIfExist();
        });
    }


    function clearTimeoutIfExist() {
        if (!(typeof timeout === 'undefined' || timeout === null)) {
            clearTimeout(timeout);
        }


        submitLatestData();

        //clearTimeout(timeout);
    }




    function popup($actionRefresh) {


        $('.progress-bar').css('width', "0" + '%');
        $(".percentage").html("0%");
        $(".result-bar").html("ICSImport");
        CallApi($actionRefresh);

        $(".overlay,.popup").fadeIn();
        StartProgrssBar(1);

    }


    //progress bar function
    function StartProgrssBar(second) {
        //var currentDate = new Date();
        //var second = currentDate.getSeconds();
        var value = "";
        if (second < 10) {
            value = 10 * second;
            if (second == 2) {
                $(".result-bar").html("")
                $(".percentage").html(value + "%")

            }
            if (second == 4) {
                $(".result-bar").html("")
                $(".percentage").html(value + "%")
            }
            if (second == 6) {
                $(".result-bar").html("")
                $(".percentage").html(value + "%")
            }
            if (second == 8) {
                $(".result-bar").html("")
                $(".percentage").html(value + "%")
            }
        }
        if (second == 10) {
            value = 10 * second;
            $(".result-bar").html("")
            $(".percentage").html(value + "%")


        }

        if (second < 9) { second = second + 1; }


        $('.progress-bar').css('width', value + '%');



        setTimeout(function () { StartProgrssBar(second) }, 500);
        value = "";




    }

    //close Progress Bar
    function CloseProgressBar() {
        $('#Fade_area').removeAttr("style");
        $('#mymodal').removeAttr("style");

        $('.progress-bar').css('width', "0" + '%');
        $(".percentage").html("0%");
        $(".result-bar").html("ICSImport");


    }




    function CallApi($actionRefresh) {

        $.ajax({
            cache: false,
            type: "POST",
            url: $actionRefresh,//'/ICS/MICRImage/ProgressBar',//data: $form,
            success: function (data) {

                if (data == "4") {

                    StartProgrssBar(10);




                }

                setTimeout(function () { CloseProgressBar() }, 500)




            },
            error: function () {
                setTimeout(function () { CloseProgressBar() }, 10)
            }


        });
        //}

    }


})()