$(document).ready(function () {

    $(document).on("click", ".classAddQ", function () {
        var rowCount = $('.data-Talla').length + 1;
        var tallasdiv = '<tr class="data-Talla">' +
            '<td width="250"><input type="text"  name="f-talla" id="f-talla" class="form-control talla" autocomplete="off" /></td>' +
            '<td width="250"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric numCantTall"  value="' + 0 + '" /></td>' +
            '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>' +
            '</tr>';
        $('#tablaTallas').append(tallasdiv);
    });

    $(document).on("click", ".classAddPack", function () {
        var rowCount = $('.data-Pack').length + 1;
        var packdiv = '<tr class="data-Pack">' +
            '<td class="mover" width="10%"><span class="glyphicon glyphicon-fullscreen" aria-hidden="true"></span></td>' +
            '<td class="datoPack" width="20%"><input type="text"  name="f-pack" id="f-pack" style="text-transform:uppercase" class="form-control pack" style="width: 80%;" /></td>' +
            '<td><button type="button" id="btnDelete" class="packDelete btn btn btn-danger btn-xs" value="4">Delete</button></td>' +
            '</tr>';
        $('#tablaPack').append(packdiv);
    });

    $(document).on("click", ".deleteTalla", function () {
        $(this).closest("tr").remove();
    });

    $(document).on("click", ".packDelete", function () {
        $(this).closest("tr").remove();
    });


});