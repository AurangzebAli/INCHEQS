(function () {

    function bindInputAsMoney() {
        $(".currency").maskMoney();
    }

    $(document).ready(function () {
        bindInputAsMoney();
    })
})();