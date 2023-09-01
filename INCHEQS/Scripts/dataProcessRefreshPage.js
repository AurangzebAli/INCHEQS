
(function () {
    $(document).ready(function () {
        bindRefreshPage();
    
    });

    function bindRefreshPage() {
        
        setTimeout(function () {
            refreshPage();
        }, 20000);
    }
    function refreshPage() {
        //empty container to give way other script
        $('#resultContainer').empty();
        var $form = $(".init-search").closest('form');
        $form.submit();

    }

})()