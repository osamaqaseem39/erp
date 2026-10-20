$(function () {
    ChangePrintScreenwidth();
    $(window).resize(function () {
        // This will execute whenever the window is resized
        ChangePrintScreenwidth();
    });
})

function ChangePrintScreenwidth() {
    // This function will be used for make responsive app name in header
    //if ($(window).width() <= 1120) {
    //    $('#printable-content').css(
    //        {
    //            "width": "100%",
    //            "overflow-x":"auto"
    //        });
    //}
    //else {
    //    $('#printable-content').css(
    //        {
    //            "width": "800px",
    //            "overflow-x": "hidden"
    //        });
    //}

    $('#printable-content').css(
    {
        "width": "800px",
        "overflow-x": "hidden"
    });

}