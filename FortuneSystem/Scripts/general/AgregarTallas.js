$(document).ready(function () {

    $(document).on("click", ".classAdd", function () {
        var rowCount = $('.data-Talla').length + 1;
        var tallasdiv = '<tr class="data-Talla">' +
            '<td class="mover"><span class="glyphicon glyphicon-fullscreen" aria-hidden="true"></span></td>' +
            '<td class="datoTalla"><input type="text"  name="f-talla" id="f-talla" class="form-control talla" autocomplete="off" /></td>' +
            '<td class="datoTalla"><input type="text" id="l-cantidad" name="l-cantidad" class="form-control l-name01 numCantTall"  /></td>' +
            '<td class="datoTalla"><input type="text" id="e-extras"  name="e-extras" class="form-control e-name01 numExtTall" value="' + 0 + '"/></td>' +
            '<td class="datoTalla"><input type="text" id="s-ejemplo" name="s-ejemplo" class="form-control s-name01 numEjmTall" value="' + 0 + '"/></td>' +
            '<td><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>' +
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

	$(document).on("click", ".classAddMillPo", function () {
		var rowCount = $('.data-MPO').length + 1;
		var MPOdiv = '<tr class="data-MPO">' +
			'<td class="mover" width="10%"><span class="glyphicon glyphicon-fullscreen" aria-hidden="true"></span></td>' +
			'<td class="datoMPO" width="20%"><input type="text"  name="f-mpo" id="f-mpo" style="text-transform:uppercase" class="form-control mpo" style="width: 80%;" /></td>' +
			'<td><button type="button" id="btnDelete" class="mpoDelete btn btn btn-danger btn-xs" value="4">Delete</button></td>' +
			'</tr>';
		$('#tablaMPO').append(MPOdiv);
	});

	$(document).on("click", ".classAddArt", function () {
		var rowCount = $('.data-Art').length + 1;
		var artdiv = '<tr class="data-Art">' +
			'<td class="mover" width="5%"><span class="glyphicon glyphicon-fullscreen" aria-hidden="true"></span></td>' +
			'<td class="datoArt" width="20%"><div class="col-sm-5">' +
			'<div id="drop-container">' +
			'<div class="drop-area-text">Drag and Drop Image Here</div>' +
			'</div><img src="" class="drop-image" id="imgArteAdd" /></div></td>' +
			'<td class="datoArt" width="20%"><input class="form-control nuevo_item color"  id="CodigoColor" name="CodigoColor" style="text-transform:uppercase" type="text"  autocomplete="off" /></td>' +
			'<td><button type="button" id="btnDelete" class="artDelete btn btn btn-danger btn-xs" value="4">Delete</button></td>' +
			'</tr>';
		$('#tablaArtColor').append(artdiv);
	});
	
		
			
		
		
	

    $(document).on("click", ".deleteTalla", function () {
        $(this).closest("tr").remove();
	});

	$(document).on("click", ".packDelete", function () {
		$(this).closest("tr").remove();
	});

	$(document).on("click", ".mpoDelete", function () {
		$(this).closest("tr").remove();
	});

	$(document).on("click", ".artDelete", function () {
		$(this).closest("tr").remove();
	});

});