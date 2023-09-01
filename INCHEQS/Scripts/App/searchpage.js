(function () {
    $(document).ready(function () { 

        $('#resultContainer').empty();
      
        submitOnload();
        submitRealTimeForm();
        bindBranchCode();
        $("#searchForm").ready(function () {

            $(".init-search").trigger('click');
            $(".init-search").click(function () {

                var $form = $(this).closest("form");
                $(".pageHolder", $form).remove();
                $("<input type='hidden' class='pageHolder' />", $form).attr("name", "page").attr("value", "1").appendTo($form);

            })
            
        })

    });

    function submitOnload() {
        var $submitOnload = $(".submit-onload");
        if ($submitOnload.length > 0) {
            //console.log("submit-onload")
            $submitOnload.submit();
        }
    }

    // xx start
    function bindBranchCode() {
        //$("#rejectCodeSelectList").change(function () {
        $("#fldIssueBankCode").change(function () {
            $("#fldIssueBankBranch").empty();
            //alert($('#fldIssueBankCode').val());
            $.ajax({
                cache: false,
                type: "POST",
                url: App.ContextPath + "CommonApi/IssueBankBranch",
                data: "bankCode=" + $('#fldIssueBankCode').val(),
                success: function (data) {
                    $("#fldIssueBankBranch").append("<option value= ''>ALL</option>");
                    //var branchCode = data.IssueBankBranch;
                    //console.log(branchCode);
                    $.each(data, function (j, branchCode) {
                        //alert(branchCode.value);
                        $("#fldIssueBankBranch").append("<option value=" + branchCode.fldBranchCode + ">" + branchCode.fldBranchDesc + " </option>");
                        
                       
                    });
                }
            });
        });
    }
    // xx end
    function submitRealTimeForm() {
        var $realTimeFormOnLoad = $(".realtime-form-onload");        
        if ($realTimeFormOnLoad.length > 0) {
            console.log("realtim submit-onload")
            $realTimeFormOnLoad.submit();
            timeoutVariable = setTimeout(submitRealTimeForm, 1000);

            $(document).on("menu-click", function () {
                if (timeoutVariable) {
                    clearTimeout(timeoutVariable);
                }
            });
        }
    }
    

})();