var closedtbl;
$(document).ready(function () {
    loaddata();
});


function loaddata() {
    closedtbl = $("#closedtbl").DataTable({

        "ajax": {
            "url": "/Ticket/GetClosedCases"
        },

        "columns": [
            { "data": "id" },
            { "data": "title" },
            { "data": "authorEmail" },
            { "data": "authorMobile" },
            { "data": "authorCompany" },
            { "data": "created" },
            { "data": "status" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <button onclick="openCustomSizeWindow('/Ticket/ManageAttach/${data}', 900, 900)" class="btn btn-info mb-1" style="width:auto">Attachments</button>
                    `
                }

            }
        ],

        layout: {
            topStart: {
                buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
            }
        },


    });
}


function openCustomSizeWindow(url, width, height) {
    var leftPosition = (screen.width) ? (screen.width - width) / 2 : 0;
    var topPosition = (screen.height) ? (screen.height - height) / 2 : 0;
    var settings = 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=' + width + ', height=' + height + ', top=' + topPosition + ', left=' + leftPosition;
    window.open(url, '', settings);
}
