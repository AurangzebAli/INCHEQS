(function () {
    $(document).ready(function () {

        $(".clickable-row").off('click').on('click', function (e) {
            if (e.target.nodeName != "INPUT") {
                if ($(this).data('action')) {
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: $(this).data('action'),
                        data: $("input", this).serialize(),
                        success: function (data) {
                            $("#resultContainer").html(data);
                        }
                    });
                }
            }
        })


        $(".clickable-row-without-search").off('click').on('click', function (e) {
            if (e.target.nodeName != "INPUT") {
                if ($(this).data('action')) {
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: $(this).data('action'),
                        data: $("input", this).serialize(),
                        success: function (data) {
                            $("#right-panel").html(data);
                        }
                    });
                }
            }
        })

        $(".clickable-row-without-search-fullview").off('click').on('click', function (e) {
            if (e.target.nodeName != "INPUT") {
                if ($(this).data('action')) {
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: $(this).data('action'),
                        data: $("input", this).serialize(),
                        success: function (data) {
                            $("#resultContainer").html(data);
                            $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer,#resultFilter").hide();
                        }
                    });
                }
            }
        })

        $(".inward-item-fullview").off('click').on('click', function (e) {
            if ($(this).data('action')) {
                //alert($("input", this).attr('name') + $("input", this).attr('value'));

                var $form = $(".isFilter").serializeArray();
                $("input", this).each(function () {
                    var $this = $(this);
                    //$form.push({ name: $("input", this).attr('name'), value: $("input", this).attr('value') });
                    $form.push({ name: $this.attr('name'), value: $this.attr('value') });
                    //images.push({ name: $this.attr('name'), value: $this.val() });
                });

                
                
                //$form.push($("input", this).serialize());
                //alert(JSON.stringify($form));
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data('action'),
                    data: $form,
                    success: function (data) {
                        $("#resultContainer").html(data);
                        $(".right-panel").css("background-color", "#ffffff");
                        $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").hide();                    
                    }
                });
            }
        });

        $(".verification-inward-item-fullview").off('click').on('click', function (e) {
            if ($(this).data('action')) {
                var $form = $(this).closest("form");
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: $(this).data('action'),
                    data: $form.serializeArray(),
                    success: function (data) {
                        $("#resultContainer").html(data);
                        $(".right-panel").css("background-color", "#ffffff");
                        $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").hide();
                    }
                });
            }
        });

        //Make inner text colored base on innertext
        //$("#search-result-table").find('td:contains("Approved")').addClass("green");
        //$("#search-result-table").find('td:contains("Returned")').addClass("red");

    });

})();