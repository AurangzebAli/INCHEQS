


////Make the DIV element draggagle:
//dragElement(document.getElementById("BoxChequeAmountEntrydiv"));

//function dragElement(elmnt) {
//	var pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
//	if (document.getElementById(elmnt.id + "header")) {
//		/* if present, the header is where you move the DIV from:*/
//		document.getElementById(elmnt.id + "header").onmousedown = dragMouseDown;
//	} else {
//		/* otherwise, move the DIV from anywhere inside the DIV:*/
//		elmnt.onmousedown = dragMouseDown;
//	}

//	function dragMouseDown(e) {
//		e = e || window.event;
//		e.preventDefault();
//		// get the mouse cursor position at startup:
//		pos3 = e.clientX;
//		pos4 = e.clientY;
//		document.onmouseup = closeDragElement;
//		// call a function whenever the cursor moves:
//		document.onmousemove = elementDrag;
//	}

//	function elementDrag(e) {
//		e = e || window.event;
//		e.preventDefault();
//		// calculate the new cursor position:
//		pos1 = pos3 - e.clientX;
//		pos2 = pos4 - e.clientY;
//		pos3 = e.clientX;
//		pos4 = e.clientY;
//		// set the element's new position:
//		elmnt.style.top = (elmnt.offsetTop - pos2) + "px";
//		elmnt.style.left = (elmnt.offsetLeft - pos1) + "px";
//	}

//	function closeDragElement() {
//		/* stop moving when mouse button is released:*/
//		document.onmouseup = null;
//		document.onmousemove = null;
//	}
//}

$(function () {

    $('.BoxChequeAmountEntry').draggable({
        containment: "window",
        handle: '.titlebar',
        stack: "div.BoxChequeAmountEntry"
    });

    $('.container').resizable({
        handles: 'se',
        resize: function (e, ui) {
            $(this).find('.BoxDiv').height(ui.size.height - $('.BoxChequeAmountEntryheader').height() - $('.inputarea').height() - 12);
            $(this).find('.inputarea').width(ui.size.width - 10);
        }
    });

});
$(function () {

    $("h2")
		.wrapInner("<span>")

    $("h2 br")
		.before("<span class='spacer'>")
		.after("<span class='spacer'>");

});