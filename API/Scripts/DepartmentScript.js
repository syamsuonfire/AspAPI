(document).ready(function () {
    $('#Department').dataTable({
        "ajax": loadDataDepartment(),
        "responsive": true,
    });

    $('[data-toggle="tooltip"]').tooltip();

});

function loadDataDepartment() { //naming bebas
    $.ajax({
        url: "/Department/LoadDepartment", //memanggil/controller
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            debugger;
            var html = '';
            $.each(result, function (key, Department) { //atribut function, sesuaikan dengan nama controller lebih baik
                html += '<tr>';
                html += '<td>' + Department.NameDepartment + '</td>';
                html += '<td>' + Department.CreateDate + '</td>';
                html += '<td>' + Department.UpdateDate + '</td>';
                html += '<td><button type="button" class="btn btn-warning" id="Update" onclick="return GetById(' + Department.Id + ')">Edit</button> ';
                html += '<button type="button" class = "btn btn-danger" id="Delete" onclick="return Delete(' + Department.Id + ')" >Delete</button></td>';
                html += '</tr>';
            });
            $('departmentbody').html(html);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    });
}