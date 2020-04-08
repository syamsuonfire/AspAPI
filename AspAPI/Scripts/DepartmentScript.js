$(document).ready(function () {
    $('#Department').dataTable({
        "ajax": loadDataDepartment(),
        "responsive": true,
    });
    $('[data-toggle="tooltip"]').tooltip();
});



function loadDataDepartment() { //naming bebas
    $.ajax({
        url: "/Department/LoadDepartment", //manggil ke controller. /Controller/NamaClassuntukmenampilkandata
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            debugger;
            var html = '';
            $.each(result, function (key, Department) { //attribut function setelah key bebas ("Department"). Sesuaikan dengan nama controller lebih baik
                html += '<tr>';
                html += '<td>' + Department.DepartmentName + '</td>';
                html += '<td>' + moment(Department.CreateDate).format('DD-MM-YYYY') + '</td>';
                if (Department.UpdateDate == null) {
                    html += '<td> Not Updated yet </td>';
                }
                else {
                    html += '<td>' + moment(Department.UpdateDate).format('DD-MM-YYYY') + '</td>';
                }
                html += '<td><button type="button" class="btn btn-warning" id="Update" onclick="return GetById(' + Department.Id + ')">Edit</button> ';
                html += '<button type="button" class="btn btn-danger" id="Delete" onclick="return Delete(' + Department.Id + ')" >Delete</button ></td > ';
                html += '</tr>';
            });
            $('.departmentbody').html(html);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}


function Delete(Id) {
    Swal.fire({
        title: "Do you want to delete it?",
        text: "You won't be able to restore this!",
        showCancelButton: true,
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.value) {
            $.ajax({
                url: "/Department/Delete/",
                type: "POST",
                data: { id: Id }
            }).then((result) => {
                if (result.StatusCode == 200) {
                    Swal.fire({
                        icon: 'success',
                        position: 'center',
                        title: 'Department Delete Successfully',
                    }).then((result) => {
                        if (result.value) {
                            location.reload()
                        }
                    });
                }
                else {
                    Swal.fire({
                        icon: 'error',
                        title: 'error',
                        text: 'Failed to Delete Department ',
                    })
                    ClearScreen();
                }
            })
        }
    });
}

function Save() {
    if ($('#DepartmentName').val() == 0) {
        Swal.fire({
            position: 'center',
            type: 'error',
            title: 'Please Full Fill The Department Name',
            showConfirmButton: false,
            timer: 1500
        });
        location.reload();
    }
    else {
        var Department = new Object();
        Department.Id = $('#Id').val();
        Department.DepartmentName = $('#DepartmentName').val();
        $.ajax({
            url: "/Department/InsertOrUpdate/",
            type: "POST",
            data: Department
        }).then((result) => {
            //debugger;
            if (result.StatusCode == 200) {
                Swal.fire({
                    position: 'center',
                    type: 'success',
                    title: 'Department Added Successfully',
                }).then((result) => {
                    if (result.value) {
                        location.reload()
                    }
                });
            }
            else {
                Swal.fire('Error', 'Failed to Add Department', 'error');
                ClearScreen();
            }
        });
    }
}





function ClearScreen() {
    $('#Id').val('');
    $('#DepartmentName').val('');
    $('#EditBtn').hide();
    $('#SaveBtn').show();
}




function GetById(Id) {
    $.ajax({
        url: "/Department/GetById/" + Id,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        async: false,
        success: function (result) {
            const obj = JSON.parse(result);
            $('#Id').val(obj.Id);
            $('#DepartmentName').val(obj.Name);
            $('#myModal').modal('show');
            $('#UpdateBtn').show();
            $('#SaveBtn').hide();
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
    return false;
}


function Edit() {
    if ($('#DepartmentName').val() == 0) {
        Swal.fire({
            position: 'center',
            type: 'error',
            title: 'Please Full Fill The Department Name',
            showConfirmButton: false,
            timer: 1500
        });
        location.reload();
    }
    else {
        var Department = new Object();
        Department.Id = $('#Id').val();
        Department.DepartmentName = $('#DepartmentName').val();
        $.ajax({
            url: "/Department/InsertOrUpdate/",
            type: "POST",
            data: Department
        }).then((result) => {
            if (result.StatusCode == 200) {
                Swal.fire({
                    position: 'center',
                    type: 'success',
                    title: 'Department Updated Successfully',
                }).then((result) => {
                    if (result.value) {
                        location.reload()
                    }
                });
            }
            else {
                Swal.fire('Error', 'Failed to Update Department', 'error');
                $('#myModal').modal('show');
                $('#SaveBtn').hide();
            }
        });
    }
}
