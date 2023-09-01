(function () {
    $(document).ready(function () {

        $(".capturing-fullview").off('click').on('click', function (e) {
            $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer,#capturingtitleindex").hide();
        });

        $(".capturing-fullview_off").off('click').on('click', function (e) {
            $(".header,.left-panel,#cssmenu,#search-fields-section,.switcher,footer").show();
        });
    });

})();