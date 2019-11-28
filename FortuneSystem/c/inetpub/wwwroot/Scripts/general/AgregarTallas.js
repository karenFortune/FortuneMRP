$(document).ready(function () {

    $(document).on("click", ".classAdd", function () {
        var rowCount = $('.data-Talla').length + 1;
        var tallasdiv = '<tr class="data-Talla">' +
            '<td><input type="text"  name="f-talla" id="f-talla" class="form-control talla"  /></td>' +
            '<td><input type="text" id="l-cantidad" name="l-cantidad" class="form-control l-name01"  /></td>' +
            '<td><input type="text" id="e-extras"  name="e-extras" class="form-control e-name01" value="' + 0 + '"/></td>' +
            '<td><input type="text" id="s-ejemplo" name="s-ejemplo" class="form-control s-name01" value="' + 0 + '"/></td>' +
            '<td><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>' +
            '</tr>';
        $('#tablaTallas').append(tallasdiv);
    });

    $(document).on("click", ".deleteTalla", function () {
        $(this).closest("tr").remove();
    });

});