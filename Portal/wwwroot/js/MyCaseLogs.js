var mycaselogstbl;
$(document).ready(function () {
    loaddata();
});


function loaddata() {
    mycaselogstbl = $("#mycaselogstbl").DataTable({

        "ajax": {
            "url": "/Ticket/GetMyCaseLogs"
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
