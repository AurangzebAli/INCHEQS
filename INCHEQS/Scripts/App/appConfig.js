
//Setup ajax error thrown
jQuery.ajaxSetup({
    //async: false,
    //timeout: 300000, //300000, // sets timeout to 300 seconds for ajax

    beforeSend: function (xhr, settings) {
        //console.log('before Send app config');
        $('#loading-indicator').show();
        $('#sub-menu, #cssmenu a:not(#logout)').addClass("disable-click");
        $('#right-panel').addClass("disable-click");
        $('#resultContainer').addClass("disable-nav");

       // console.log('Request URL:', settings.url);
        // You can also print other information about the request
        //console.log('Request Type:', settings.type);
        //onsole.log('Data:', settings.data);
    },
    complete: function () {
        //console.log('before complete app config');

        $('#loading-indicator').hide();
        $('#sub-menu, #cssmenu a:not(#logout)').removeClass("disable-click");
        $('#right-panel').removeClass("disable-click");
        $('#resultContainer').removeClass("disable-nav");
    },
    success: function(jqXhr, textStatus){
        //console.log('before success app config');

        App.initBind(jqXhr) 
     
    },
    error: function (jqXHR, textStatus, errorThrown) {
        
        var contextPath = $("#contextPath").html();
        //console.log(contextPath);
       // console.log(jqXHR);
        if (jqXHR.status == 401) {
            window.location.href = contextPath + "?error=Timeout";
        }
        console.log('error: ', errorThrown);
        $("#myModal .modal-title").html(errorThrown);
        var $html = $(jqXHR.responseText);
        $("#myModal .modal-body").html($html);
        $("style", "#myModal .modal-body").remove();
        $(".modal-body h1, .modal-body h2").css("font-size", "18px");
        $("code").css("white-space", "normal");
        $("#myModal").modal();
    }
});