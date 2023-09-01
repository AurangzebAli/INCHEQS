(function () {

    function bindCloseButton() {
        $("#closeBtn").off("click.cheque").on("click.cheque", function (e) {
            //e.preventDefault();
            //var $form = $(this).closest("form");
            //alert($form);
            //alert($(this).data("action"));
            //$.ajax({
            //    type: "POST",
            //    url: $(this).data("action"),
            //    data: $form.serialize(),
            //    success: function (data) {
            //        //alert(data);
            //        $("#resultContainer").html(data);
            //        //$("#searchForm").trigger("submit");
            //        //$(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").show();
            //    }
            //})
            $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").show();
            //$.ajax({
            //    cache: false,
            //    type: "POST",
            //    url: $(this).data("action"),
            //    //data: $("input", this).serialize(),
            //    success: function (data) {
            //        //alert("success");
            //        //$("#resultContainer").empty();
            //        //$("#resultContainer").html(data);
                    

            //    }
            //})
        })
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////INITIATE ALL FUNCTION
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    $(document).ready(function () {
        bindCloseButton();
    });

})();