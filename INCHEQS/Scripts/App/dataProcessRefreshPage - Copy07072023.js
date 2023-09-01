var myVar;

(function () {
    $(document).ready(function () {
        //debugger;
        getLatestProcess();
        //bindRefreshPage();

    });


    function bindRefreshPage() {

        if (!(typeof myVar === 'undefined' || myVar === null)) {
            stopRefreshPage(myVar);
        }

        myVar = setTimeout(function () {
            refreshPage(myVar);
        }, 20000);



    }

    function getLatestProcess() {
        //debugger;
        var dataParam;
        var dataUrl;
        var dataProcessType = $("#dataProcessType").val();


        if (dataProcessType == "Import") {
            dataParam = "&processname=" + $("#dataProcessContainer").data("processname");
            dataUrl = App.ContextPath + "DataProcessApi/AsJsonMicrProcess";

            if ($("#dataProcessContainer").length > 0) {
                $.ajax({
                    cache: false,
                    url: dataUrl,
                    method: "POST",
                    data: dataParam,
                    beforeSend: function () {
                        $(".resultLoader").toggleClass("hidden");
                    },
                    success: function (data) {
                        $(".resultLoader").toggleClass("hidden");
                        var $ul = $("<ul />");
                        _.each(data, function (d) {
                            $ul.append("<li>" + d + "</li>")
                        });
                        $("#dataProcessContainer").html($ul);
                    }
                })
            }
        }

     
    }

    function refreshPage(myVar) {
        //debugger;
        //empty container to give way other script
        $('#resultContainer').empty();

        if ($('.init-search').length > 0) {
            var $form = $(".init-search").closest('form');
            $form.submit();
        }
        else {
            stopRefreshPage(myVar);
            submitOnload();
        }

     
     

     
    }

    function stopRefreshPage(myVar) {
        clearTimeout(myVar);
    }


    function submitOnload() {

        var $submitOnload = $(".submit-onload");
        if ($submitOnload.length > 0) {
            //console.log("submit-onload")
            $submitOnload.submit();
        }
    }
})()
