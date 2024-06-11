var logtbl;
$(document).ready(function () {
    loaddata();
});


function loaddata() {
    logtbl = $("#logtbl").DataTable({

        "ajax": {
            "url": "/Ticket/GetLogs"
        },

        "columns": [


            { "data": "addedBy" },
            { "data": "action" },
            { "data": "ticketNumber" },
            { "data": "message" },
            { "data": "time" },

        ],

        layout: {
            topStart: {
                buttons: ['copy', 'csv', 'excel']
            }
        },


    });
}
