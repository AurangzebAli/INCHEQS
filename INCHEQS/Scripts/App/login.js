//Initialize
$(document).ready(function () {
    bindPasswordMask();
    //allowoneclickuserbutton();
    displayErrorOnQueryString();
    getMacAddress();
})


function bindPasswordMask() {
    //remove autocomplete for password
    setTimeout(function () {
        $("#UserPassword, #OldPassword, #NewPassword, #ConfirmNewPassword").attr('type', 'password')
    }, 1);
}

function getParameterByName(name) {
    var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
    return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
}

function displayErrorOnQueryString() {
    var err = getParameterByName("error")
    if (err !== null) {
        $("#notice .notice-body").html("User Session Ended");
        $("#notice").toggleClass("hidden");
    }
}

function getMacAddress() {
    //debugger
    var obj = new ActiveXObject("WbemScripting.SWbemLocator");
    var s = obj.ConnectServer(".");
     var properties = s.ExecQuery("Select MACAddress From Win32_NetworkAdapterConfiguration Where IPEnabled = 'True' AND MACAddress <> '' and MACAddress <> null");
    var e = new Enumerator(properties);
    var output = [];
    while (!e.atEnd()) {
        var p = e.item();
        if (!p) continue;
        output.push(p.MACAddress);
        e.moveNext();
    }
    $("#macAddress").val(output);
}

//function allowoneclickuserbutton()
//{
//    $(".login-btn").off("click.g").on("click.g", function () {
//        $(".login-btn").attr("disabled", true);
//    });
//}