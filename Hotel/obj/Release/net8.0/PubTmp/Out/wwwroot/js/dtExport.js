$(document).ready(function () {
    if (!$.fn.DataTable.isDataTable('#myTable')) {
        $('#myTable').DataTable({
            paging: false,
            dom: 'Bfrtip',
            buttons: [
                {
                    extend: 'copy',
                    exportOptions: {
                        columns: ':visible:not(.no-export)'
                    }
                },
                {
                    extend: 'excel',
                    exportOptions: {
                        columns: ':visible:not(.no-export)'
                    }
                },
                {
                    extend: 'pdf',
                    exportOptions: {
                        columns: ':visible:not(.no-export)'
                    }
                }
            ]
        });
    }
});
