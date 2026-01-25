$(function () {

    $('#tblLocation').DataTable({
        dom: 'lBfrtip',
        buttons: [
            'copy', 'csv', 'excel', 'pdf', 'print'
        ],

        lengthMenu: [5, 10, 20, 50, 100, 200, 500],
        lengthChange: true,
        searching: true,
        autoWidth: false,
        responsive: true,
        paging: true,


    });
});