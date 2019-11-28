
function RegistrarComentario(key, d, values, idSummary, tipoArchivo) {
    var idComentario = key.CatComentarios.IdComentario;
    var comentario = values.CatComentarios.Comentario;

    $.ajax({
        type: 'POST',
        url: "/WIP/RegistrarCometarioWIP",
        data: { Comentario: comentario, IdSummary: parseInt(idSummary), TipoArchivo: tipoArchivo },
        success: function (data) {
            d.resolve(data);
            /*if (!timeOut) {
                timeOut = setTimeout(timerCallback, 100);
            }*/
        },
        error: function (e) {
            alert("error: " + e.responseText);
        }
    });
}

function RegistrarFechaUCC(d, values, idSummary) {
    var fechaUCC = values.InfoSummary.FechaUCC;
    $.ajax({
        type: 'POST',
        url: "/WIP/RegistrarFechaUCC",
        data: { FechaUCC: fechaUCC, IdSummary: parseInt(idSummary) },
        success: function (data) {
            d.resolve(data);
            /*if (!timeOut) {
                timeOut = setTimeout(timerCallback, 100);
            }*/
        },
        error: function (e) {
            alert("error: " + e.responseText);
        }
    });
}


