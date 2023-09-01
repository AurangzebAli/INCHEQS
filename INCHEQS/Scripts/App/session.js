//Convert minute to milisecond
var sessionPeriod = $("#sessionPeriod").val();

if (sessionPeriod == 0) { //default 1 if value is 0
    sessionPeriod = 1 * 60 - 35 //35 sec for modal count
} else {
    sessionPeriod = sessionPeriod * 60 - 35 //35 sec for modal count
}
var c = 0;
var max_count = 30;
var logout = true;

function startIdleTimer() {
    idleTime = 0;


    //Increment the idle time counter every second.
    var idleInterval = setInterval(timerIncrement, 1000);

    function timerIncrement() {
       // console.log('sessionPeriod: ', sessionPeriod);
       // console.log('idleTimer', idleTime);
        idleTime++;
        if (idleTime > sessionPeriod) {
            startTimer();
            clearTimeout(idleInterval);
        }
    }

    //Zero the idle timer event.
    $(document).on('click', 'a, button, td', function (e) {
        idleTime = 0;
        
    });
}


function startTimer() {
        logout = true;
        c = 0;
        max_count = 30;
        $('#timer').html(max_count);
        $('#logout_popup').modal({
            backdrop: 'static',
            keyboard: false
        });
        startCount();
}

function resetTimer() {
    logout = false;
    
    $('#logout_popup').modal('hide');
    clearTimeout(t);
    keepUserActive();
    startIdleTimer();
}

function signOut() {
    
    logout = true;
    $('#logout_popup').modal('hide');
    clearTimeout(t);
    removeSession();
    catchPageUnloadFlag = false;
}

function timedCount() {
    c = c + 1;
    
    remaining_time = max_count - c;
    if (remaining_time == 0 && logout) {
        signOut()
    } else {
        $('#timer').html(remaining_time);
        t = setTimeout(function () { timedCount() }, 1000);
    }
}
function startCount() {
    timedCount();
}

function keepUserActive() {
    
    $.ajax({
        cache: false,
        type: "POST",
        url: $("#contextPath").html() + "CommonApi/KeepCurrentUserSession",
        beforeSend: function () {
            //remove refresh jason
            1+1
        }
    });
}


function bindBrowserClose() {
    catchPageUnloadFlag = true;
    console.log('bindBrowserClose' + new Date())
    $(".home-dashboard").on("click", function (e) {
        catchPageUnloadFlag = false;
    });

    $("#appSwitcher").on("click", function (e) {
        catchPageUnloadFlag = false;
    });

    $("#logout").on("click", function (e) {
        catchPageUnloadFlag = false;
    });


    $(window).one("beforeunload", function (e) {
       
        if (catchPageUnloadFlag) {
            e.preventDefault();
            e.stopImmediatePropagation();
            e.stopPropagation();
            removeSession();
            //return "Do you really want to close?";
        }
    });

}

function removeSession() {
    console.log("removeSession", new Date())

    $.ajax({
        cache: false,
        type: "POST",
        url: $("#contextPath").html() + "CommonApi/RemoveCurrentUserSession",
        success: function (data) {
            clearTimeout(t);
            clearTimeout(m);
            bootbox.alert(data.notice);
        }
    });
}


// XX Edit
function getMessage(e) {
    console.log("start getMessage" + new Date())
    
    $.ajax({
        cache: false,
        type: "POST",
        url: $("#contextPath").html() + "CommonApi/GetMessage",
        beforeSend: function (data) {
            //remove refresh jason
            1 + 1
        },
        success: function (data) {
            if (data) {
                $('.messageMarquee').removeClass("hidden")
                $('.messageMarquee').html(data);
            }
        }
    });
}
// XX End

function bindTransferSession() {
    //app swither
    console.log('bindTransferSession' + new Date())
    $(document).on('click', "#appSwitcher", function () {
        $.ajax({
            cache: false,
            type: "POST",
            url: $("#contextPath").html() + "CommonApi/TransferSessionToOcs",
            success: function (data) {
                window.open("http://localhost/INCHEQS_OCS_KFH/WebContent/Main.aspx", "_self");
            }
        });
    })

}

// refresh every 15 sec
m = setTimeout(function () { 
    // XX Edit
    getMessage()
    // XX End
}, 15000);

//Initialize
$(document).ready(function () {
    startIdleTimer();
    // XX Edit
    getMessage();
    // XX End
    //bindBrowserClose();
    //bindTransferSession();
})