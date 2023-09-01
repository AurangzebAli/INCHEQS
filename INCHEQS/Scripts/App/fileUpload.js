(function () {

    function bindUploadFile() {
        $(document).off('change', ".fileUploader").on('change', ".fileUploader", function (e) {
            var fileName;
            var fileType;
            var fileTypeExt = $("#fileTypeExt").val()
            $.each(e.target.files, function (key, value) {
                fileName = value.name;
                fileType = fileName.slice(-3);
                if ("."+fileType != fileTypeExt) {
                    $(".fileUploader").val("");
                    alert("Wrong File Format. Only " + fileTypeExt + " File Allowed");
                }
            })
            //Add clear date value inside hidden input from search form
            $("#clearDate").val($("[name='fldClearDate']").val())
        })
    }

    function toggleUploadContainer() {
        $(".manualUploadButton").off('click').on('click', function () {
            $(".manualUploadContainer").toggleClass("hidden");
        })
    }

    function ajaxUploadFile() {
        $(document).off("click", ".normal-submit-file-upload").on("click", ".normal-submit-file-upload", function (e) {
            e.preventDefault();
            var $form = $(this).closest("form");
            if ($(this).data("action") == null) {
                alert("No Action properties specified for the button. Eg data-action='Controller/Action'");
                return;
            }
            var action = $(this).attr("action");
            var files = $(".fileUploader")[0].files;
            if (files.length > 0) {
                if (window.FormData !== undefined) {
                    var form = $('form')[0];
                    var data = new FormData(form);
                    for (var x = 0; x < files.length; x++) {
                        data.append("file", files[x]);
                    }
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: $(this).data("action"),
                        contentType: false,
                        processData: false,
                        data: data,
                        beforeSend: function () {
                            $(".upload-notice").text("Uploading...");
                        },
                        success: function (result) {
                            $(".init-search").trigger("click");
                            $(".search").trigger("click");
                            $(".upload-notice").text("File(s) Successfully Uploaded");
                        }
                    });
                } else {
                    alert("This browser doesn't support HTML5 file uploads!");
                }
            }

        });
    }

    $(document).ready(function () {
        bindUploadFile();
        toggleUploadContainer();
        ajaxUploadFile();
    })
})();