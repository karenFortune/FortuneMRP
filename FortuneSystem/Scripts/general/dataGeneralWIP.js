
$(function () {
    $("#popover2").dxPopover({
        target: "#link2",
        showEvent: "mouseenter",
        hideEvent: "mouseleave",
        position: "right",
        closeOnBackButton: false,
        closeOnOutsideClick: false,
        width: 300,
        showTitle: true,
        title: "Details:"
    });


    //TimeOut
    var timeOut = null,
        updateTasks = [];
    var timerCallback = function () {
        $.each(updateTasks, function (index, task) {
            task.deferred.resolve();
        });
        updateTasks = [];
        timeOut = null;
    };

});

function FilterFecha() {
    var d = new Date();

    var month = d.getMonth() + 1;
    var day = d.getDate();

    var output = (('' + day).length < 2 ? '0' : '') + day + '/' +
        (('' + month).length < 2 ? '0' : '') + month + '/' +
        d.getFullYear();

    return output;
}
