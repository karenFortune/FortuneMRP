using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using FortuneSystem.Models.Pedidos;
using Rotativa.AspNetCore;
using Rotativa;
using ClosedXML.Excel;
using System.IO;
using FortuneSystem.Models.POSummary;
using System.Configuration;

namespace FortuneSystem.Controllers
{
    public class ReportController : Controller
    {
		readonly PedidosData objPedido = new PedidosData();
		readonly DescripcionItemData de = new DescripcionItemData();

        public ActionResult GetListaPO(bool pdf = false)
        {
            int id = Convert.ToInt32(Session["idPed"]);
            ViewBag.PDF = pdf; 
         
            return View(de.ListadoInfEstilo(id));
        }
        [AllowAnonymous]
        public ActionResult Imprimir_Reporte_PO()
        {
            int id = Convert.ToInt32(Session["idPed"]);
            string po = Convert.ToString(Session["nombrePO"]);
            string filename = "Reporte_" + po + ".pdf";
            Dictionary<string, string> cookieCollection = new Dictionary<string, string>();
            foreach (var key in Request.Cookies.AllKeys)
            {
                cookieCollection.Add(key, Request.Cookies.Get(key).Value);
            }
            return new Rotativa.ViewAsPdf("Imprimir_Reporte_PO", de.ListadoInfEstilo(id))
            {
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                Cookies = cookieCollection,
                PageSize = Rotativa.Options.Size.Letter,
                PageMargins = new Rotativa.Options.Margins(4, 4, 5, 5),
                FileName = filename,
                FormsAuthenticationCookieName = System.Web.Security.FormsAuthentication.FormsCookieName
            };

        }

        public ActionResult ObtenerReporteWIP()
		{
			FileContentResult robj;
			//string year = Convert.ToString(Session["year_reporte"]);
			List<OrdenesCompra> listaPedidos = objPedido.ListaOrdenCompraWIP(1).ToList();
			List<OrdenesCompra> listaShipped = objPedido.ListaOrdenCompraWIP(2).ToList();
			List<OrdenesCompra> listaCancelled = objPedido.ListaOrdenCompraWIP(3).ToList();


			int row = 1;
			using (XLWorkbook libro_trabajo = new XLWorkbook())
			{ //Regex.Replace(pedido, @"\s+", " "); 
				var wp = libro_trabajo.Worksheets.Add("WIP");
				var ws = libro_trabajo.Worksheets.Add("SHIPPED");
				var wc = libro_trabajo.Worksheets.Add("CANCELLED");
				wp.TabColor = XLColor.Green;
				ws.TabColor = XLColor.Yellow;
				wc.TabColor = XLColor.Red;

				//CABECERAS TABLA
				var headers = new List<String[]>();
				List<String> titulos = new List<string>
				{
					"CUSTOMER",
					"RETAILER",
					"P.O. RECVD DATA",
					"PO NO.",
					"BRAND NAME",
					"AMT PO",
					"REG/BULK",
					"BALANCE QTY",
					"EXPECTED SHIP DATE",
					"ORIGINAL CUST DUE DATE",
					"DESIGN NAME",
					"STYLE",
					"MillPO",
					"COLOR",
					"GENDER",
					"BLANKS RECEIVED",
					"PARTIAL/COMPLETE BLANKS",
					"ART RECEIVED",
					"TRIM RECEIVED",
					"PACK INST.RCVD",
					"PRICE TICKET RECEIVED",
					"UCC RECEIVED",
					"COMMENTS UPDATE",
					"COMMENTS"
				};
				headers.Add(titulos.ToArray());
				ws.Cell(row, 1).Value = headers;
				ws.Range(row, 1, row, 24).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 0, 0);
				ws.Range(row, 1, row, 24).Style.Font.FontColor = XLColor.FromArgb(255, 255, 255);
				wp.Cell(row, 1).Value = headers;
				wp.Range(row, 1, row, 24).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 0, 0);
				wp.Range(row, 1, row, 24).Style.Font.FontColor = XLColor.FromArgb(255, 255, 255);
				wc.Cell(row, 1).Value = headers;
				wc.Range(row, 1, row, 24).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 0, 0);
				wc.Range(row, 1, row, 24).Style.Font.FontColor = XLColor.FromArgb(255, 255, 255);
				wp.RangeUsed().SetAutoFilter().Column(1);
				ws.RangeUsed().SetAutoFilter().Column(1);
				wc.RangeUsed().SetAutoFilter().Column(1);
				row++; //AGREGAR DATOS LISTAS
				SheetWIP(listaPedidos, wp, row);
				SheetShipped(listaShipped, ws, row);
				SheetCancelled(listaCancelled, wc, row);

				wp.Rows().AdjustToContents();
				wp.Columns().AdjustToContents();
				ws.Rows().AdjustToContents();
				ws.Columns().AdjustToContents();
				wc.Rows().AdjustToContents();
				wc.Columns().AdjustToContents();
				//ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				/***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
				/* HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
				 httpResponse.Clear();
				 httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				 httpResponse.AddHeader("content-disposition", "attachment;filename=\"WIP.xlsx\"");*/
				// Flush the workbook to the Response.OutputStream
				using (MemoryStream memoryStream = new MemoryStream())
				{
					libro_trabajo.SaveAs(memoryStream);
					var bytesdata = File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WIP.xlsx");
					robj = bytesdata;
				}

				// return File(httpResponse.OutputStream);
			}
			return Json(robj, JsonRequestBehavior.AllowGet);
		}

		public void SheetWIP(List<OrdenesCompra> listaPedidos, IXLWorksheet wp, int row)
		{
			foreach (OrdenesCompra e in listaPedidos)
			{
				var celdas = new List<String[]>();
				List<String> datos = new List<string>();
				DateTime fechaActual = DateTime.Today;
				string fechaHoy = String.Format("{0:dd/MM/yyyy}", fechaActual);
				if (e.CatComentarios.FechaComents == fechaHoy)
				{
					wp.Range(row, 1, row, 24).Style.Fill.BackgroundColor = XLColor.FromArgb(254, 232, 200);
				}
				datos.Add(e.CatCliente.Nombre);

				datos.Add(e.CatClienteFinal.NombreCliente);
				datos.Add(e.FechaRecOrden);
				//PO NO
				if (e.RestaPrintshop <= 10)
				{
					datos.Add(e.PO);
					wp.Range(row, 4, row, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(95, 157, 205);
					wp.Range(row, 4, row, 4).Style.Font.Bold = true;
					wp.Range(row, 4, row, 4).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.PO);
					wp.Range(row, 4, row, 4).Style.Font.Bold = true;
					wp.Range(row, 4, row, 4).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				datos.Add(e.CatTipoBrand.TipoBrandName);
				wp.Range(row, 5, row, 5).Style.Font.Bold = true;
				datos.Add(e.VPO);
				datos.Add(e.CatTipoOrden.TipoOrden);
				wp.Range(row, 7, row, 7).Style.Font.Bold = true;
				datos.Add((e.InfoSummary.CantidadEstilo).ToString());
				if (e.FechaCancel < fechaActual)
				{
					e.FechaOrdenFinal = "TBD";
					datos.Add(e.FechaOrdenFinal);
					wp.Range(row, 4, row, 4).Style.Font.Bold = true;
				}
				else
				{
					datos.Add(e.FechaOrdenFinal);
				}

				datos.Add(e.FechaCancelada);
				//DESIGN NAME
				//e.DestinoSalida
				if (e.InfoSummary.IdSucursal == 2)
				{
					datos.Add(e.InfoSummary.ItemDesc.Descripcion);
					wp.Range(row, 11, row, 11).Style.Fill.BackgroundColor = XLColor.FromArgb(190, 174, 241);
					wp.Range(row, 11, row, 11).Style.Font.Bold = true;
					wp.Range(row, 11, row, 11).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoSummary.ItemDesc.Descripcion);
					wp.Range(row, 11, row, 11).Style.Font.Bold = true;
					wp.Range(row, 11, row, 11).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				datos.Add(e.InfoSummary.ItemDesc.ItemEstilo);
				datos.Add(e.MillPO);
				datos.Add(e.InfoSummary.CatColores.DescripcionColor);
				datos.Add(e.InfoSummary.CatGenero.Genero);
				// BLANKS RECEIVED 
				if (e.TotalRestante == 0)
				{
					datos.Add((e.TotalRestante).ToString());
					wp.Range(row, 16, row, 16).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.TotalRestante != e.InfoSummary.TotalEstilo)
				{
					datos.Add((e.TotalRestante).ToString());
					wp.Range(row, 16, row, 16).Style.Font.FontColor = XLColor.FromArgb(246, 57, 57);
				}
				//PARTIAL/COMPLETE BLANKS
				if (e.TipoPartial == "PARTIAL")
				{

					datos.Add(e.TipoPartial);
					wp.Range(row, 17, row, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(249, 136, 29);
					wp.Range(row, 17, row, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);

				}
				else if (e.TipoPartial == "COMPLETE")
				{
					datos.Add(e.TipoPartial);
					wp.Range(row, 17, row, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(64, 191, 128);
					wp.Range(row, 17, row, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.TipoPartial == null)
				{
					e.TipoPartial = "";
					datos.Add(e.TipoPartial);
					wp.Range(row, 17, row, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				//ART RECEIVED
				if (e.ImagenArte.StatusArteInf == "IN HOUSE")
				{
					datos.Add((e.ImagenArte.StatusArteInf).ToString());
					wp.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wp.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);

				}
				else if (e.ImagenArte.StatusArteInf == "REVIEWED")
				{
					datos.Add((e.ImagenArte.StatusArteInf).ToString());
					wp.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wp.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.ImagenArte.StatusArteInf == "PENDING")
				{
					datos.Add((e.ImagenArte.StatusArteInf).ToString());
					wp.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(236, 95, 95);
					wp.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.ImagenArte.StatusArteInf == "APPROVED")
				{
					string infoArte = e.ImagenArte.StatusArteInf + e.ImagenArte.FechaArte;
					datos.Add(infoArte);
					wp.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(246, 129, 51);
					wp.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				//TRIM RECEIVED
				if (e.Trims.restante <= 0 && e.Trims.estado == "1")
				{
					datos.Add(e.Trims.fecha_recibo);
					wp.Range(row, 19, row, 19).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wp.Range(row, 19, row, 19).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.Trims.restante >= 1 && e.Trims.estado == "1")
				{
					datos.Add(e.Trims.fecha_recibo);
					wp.Range(row, 19, row, 19).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 213, 67);
					wp.Range(row, 19, row, 19).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.Trims.fecha_recibo);
					wp.Range(row, 19, row, 19).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				//PACK INSTRUCTION
				if (e.InfoPackInstruction.Fecha_Pack != "" && e.InfoPackInstruction.EstadoPack == 1)
				{
					datos.Add(e.InfoPackInstruction.Fecha_Pack);
					wp.Range(row, 20, row, 20).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wp.Range(row, 20, row, 20).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoPackInstruction.Fecha_Pack);
					wp.Range(row, 20, row, 20).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				//PRICE TICKET RECEIVED
				if (e.InfoPriceTickets.Restante <= 0 && e.InfoPriceTickets.Estado == "1")
				{
					datos.Add(e.InfoPriceTickets.Fecha_recibo);
					wp.Range(row, 21, row, 21).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wp.Range(row, 21, row, 21).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.InfoPriceTickets.Restante >= 1 && e.InfoPriceTickets.Estado == "1")
				{
					datos.Add(e.InfoPriceTickets.Fecha_recibo);
					wp.Range(row, 21, row, 21).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 213, 67);
					wp.Range(row, 21, row, 21).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoPriceTickets.Fecha_recibo);
					wp.Range(row, 21, row, 21).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				//UCC RECEIVED
				if (e.InfoSummary.FechaUCC != "")
				{
					datos.Add(e.InfoSummary.FechaUCC);
					wp.Range(row, 22, row, 22).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wp.Range(row, 22, row, 22).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoSummary.FechaUCC);
					wp.Range(row, 22, row, 22).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				datos.Add(e.CatComentarios.FechaComents);
				datos.Add(e.CatComentarios.Comentario);
				celdas.Add(datos.ToArray());
				wp.Cell(row, 1).Value = celdas;


				row++;
			}
		}

		public void SheetShipped(List<OrdenesCompra> listaShipped, IXLWorksheet ws, int row)
		{
			foreach (OrdenesCompra e in listaShipped)
			{
				var celdas = new List<String[]>();
				List<String> datos = new List<string>();
				DateTime fechaActual = DateTime.Today;
				string fechaHoy = String.Format("{0:dd/MM/yyyy}", fechaActual);
				if (e.CatComentarios.FechaComents == fechaHoy)
				{
					ws.Range(row, 1, row, 24).Style.Fill.BackgroundColor = XLColor.FromArgb(254, 232, 200);
				}
				datos.Add(e.CatCliente.Nombre);

				datos.Add(e.CatClienteFinal.NombreCliente);
				datos.Add(e.FechaRecOrden);
				//PO NO
				if (e.RestaPrintshop <= 10)
				{
					datos.Add(e.PO);
					ws.Range(row, 4, row, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(95, 157, 205);
					ws.Range(row, 4, row, 4).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.PO);
					ws.Range(row, 4, row, 4).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				datos.Add(e.CatTipoBrand.TipoBrandName);
				datos.Add(e.VPO);
				datos.Add(e.CatTipoOrden.TipoOrden);
				datos.Add((e.Shipped.Cantidad).ToString());
				if (e.FechaCancel < fechaActual)
				{
					e.FechaOrdenFinal = "TBD";
					datos.Add(e.FechaOrdenFinal);
					ws.Range(row, 4, row, 4).Style.Font.Bold = true;
				}
				else
				{
					datos.Add(e.FechaOrdenFinal);
				}
				datos.Add(e.FechaCancelada);
				//DESIGN NAME
				if (e.InfoSummary.IdSucursal == 2)
				{
					datos.Add(e.InfoSummary.ItemDesc.Descripcion);
					ws.Range(row, 11, row, 11).Style.Fill.BackgroundColor = XLColor.FromArgb(190, 174, 241);
					ws.Range(row, 11, row, 11).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoSummary.ItemDesc.Descripcion);
					ws.Range(row, 11, row, 11).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				datos.Add(e.InfoSummary.ItemDesc.ItemEstilo);
				datos.Add(e.MillPO);
				datos.Add(e.InfoSummary.CatColores.DescripcionColor);
				datos.Add(e.InfoSummary.CatGenero.Genero);
				// BLANKS RECEIVED 
				if (e.TotalRestante == 0)
				{
					datos.Add((e.TotalRestante).ToString());
					ws.Range(row, 16, row, 16).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.TotalRestante != e.InfoSummary.TotalEstilo)
				{
					datos.Add((e.TotalRestante).ToString());
					ws.Range(row, 16, row, 16).Style.Font.FontColor = XLColor.FromArgb(246, 57, 57);
				}
				//PARTIAL/COMPLETE BLANKS
				if (e.TipoPartial == "PARTIAL")
				{

					datos.Add(e.TipoPartial);
					ws.Range(row, 17, row, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(249, 136, 29);
					ws.Range(row, 17, row, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);

				}
				else if (e.TipoPartial == "COMPLETE")
				{
					datos.Add(e.TipoPartial);
					ws.Range(row, 17, row, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(64, 191, 128);
					ws.Range(row, 17, row, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.TipoPartial == null)
				{
					e.TipoPartial = "";
					datos.Add(e.TipoPartial);
					ws.Range(row, 17, row, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				//ART RECEIVED
				if (e.ImagenArte.StatusArteInf == "IN HOUSE")
				{
					datos.Add((e.ImagenArte.StatusArteInf).ToString());
					ws.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					ws.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);

				}
				else if (e.ImagenArte.StatusArteInf == "REVIEWED")
				{
					datos.Add((e.ImagenArte.StatusArteInf).ToString());
					ws.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					ws.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.ImagenArte.StatusArteInf == "PENDING")
				{
					datos.Add((e.ImagenArte.StatusArteInf).ToString());
					ws.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(236, 95, 95);
					ws.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.ImagenArte.StatusArteInf == "APPROVED")
				{
					string infoArte = e.ImagenArte.StatusArteInf + "-" + e.ImagenArte.FechaArte;
					datos.Add(infoArte);
					ws.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(246, 129, 51);
					ws.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				//TRIM RECEIVED
				if (e.Trims.restante <= 0 && e.Trims.estado == "1")
				{
					datos.Add(e.Trims.fecha_recibo);
					ws.Range(row, 19, row, 19).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					ws.Range(row, 19, row, 19).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.Trims.restante >= 1 && e.Trims.estado == "1")
				{
					datos.Add(e.Trims.fecha_recibo);
					ws.Range(row, 19, row, 19).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 213, 67);
					ws.Range(row, 19, row, 19).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.Trims.fecha_recibo);
					ws.Range(row, 19, row, 19).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				//PACK INSTRUCTION
				if (e.InfoPackInstruction.Fecha_Pack != "" && e.InfoPackInstruction.EstadoPack == 1)
				{
					datos.Add(e.InfoPackInstruction.Fecha_Pack);
					ws.Range(row, 20, row, 20).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					ws.Range(row, 20, row, 20).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoPackInstruction.Fecha_Pack);
					ws.Range(row, 20, row, 20).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				//PRICE TICKET RECEIVED
				if (e.InfoPriceTickets.Restante <= 0 && e.InfoPriceTickets.Estado == "1")
				{
					datos.Add(e.InfoPriceTickets.Fecha_recibo);
					ws.Range(row, 21, row, 21).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					ws.Range(row, 21, row, 21).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.InfoPriceTickets.Restante >= 1 && e.InfoPriceTickets.Estado == "1")
				{
					datos.Add(e.InfoPriceTickets.Fecha_recibo);
					ws.Range(row, 21, row, 21).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 213, 67);
					ws.Range(row, 21, row, 21).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoPriceTickets.Fecha_recibo);
					ws.Range(row, 21, row, 21).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				//UCC RECEIVED
				if (e.InfoSummary.FechaUCC != "")
				{
					datos.Add(e.InfoSummary.FechaUCC);
					ws.Range(row, 22, row, 22).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					ws.Range(row, 22, row, 22).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoSummary.FechaUCC);
					ws.Range(row, 22, row, 22).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				datos.Add(e.CatComentarios.FechaComents);
				datos.Add(e.CatComentarios.Comentario);
				celdas.Add(datos.ToArray());
				ws.Cell(row, 1).Value = celdas;


				row++;
			}

		}

		public void SheetCancelled(List<OrdenesCompra> listaCancelled, IXLWorksheet wc, int row)
		{
			foreach (OrdenesCompra e in listaCancelled)
			{
				var celdas = new List<String[]>();
				List<String> datos = new List<string>();
				DateTime fechaActual = DateTime.Today;
				string fechaHoy = String.Format("{0:dd/MM/yyyy}", fechaActual);
				if (e.CatComentarios.FechaComents == fechaHoy)
				{
					wc.Range(row, 1, row, 24).Style.Fill.BackgroundColor = XLColor.FromArgb(254, 232, 200);
				}
				datos.Add(e.CatCliente.Nombre);

				datos.Add(e.CatClienteFinal.NombreCliente);
				datos.Add(e.FechaRecOrden);
				//PO NO
				if (e.RestaPrintshop <= 10)
				{
					datos.Add(e.PO);
					wc.Range(row, 4, row, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(95, 157, 205);
					wc.Range(row, 4, row, 4).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.PO);
					wc.Range(row, 4, row, 4).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				datos.Add(e.CatTipoBrand.TipoBrandName);
				datos.Add(e.VPO);
				datos.Add(e.CatTipoOrden.TipoOrden);
				datos.Add((e.InfoSummary.CantidadEstilo).ToString());
				if (e.FechaCancel < fechaActual)
				{
					e.FechaOrdenFinal = "TBD";
					datos.Add(e.FechaOrdenFinal);
					wc.Range(row, 4, row, 4).Style.Font.Bold = true;
				}
				else
				{
					datos.Add(e.FechaOrdenFinal);
				}
				datos.Add(e.FechaCancelada);
				//DESIGN NAME
				if (e.InfoSummary.IdSucursal == 2)
				{
					datos.Add(e.InfoSummary.ItemDesc.Descripcion);
					wc.Range(row, 11, row, 11).Style.Fill.BackgroundColor = XLColor.FromArgb(190, 174, 241);
					wc.Range(row, 11, row, 11).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoSummary.ItemDesc.Descripcion);
					wc.Range(row, 11, row, 11).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				datos.Add(e.InfoSummary.ItemDesc.ItemEstilo);
				datos.Add(e.MillPO);
				datos.Add(e.InfoSummary.CatColores.DescripcionColor);
				datos.Add(e.InfoSummary.CatGenero.Genero);
				// BLANKS RECEIVED 
				if (e.TotalRestante == 0)
				{
					datos.Add((e.TotalRestante).ToString());
					wc.Range(row, 16, row, 16).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.TotalRestante != e.InfoSummary.TotalEstilo)
				{
					datos.Add((e.TotalRestante).ToString());
					wc.Range(row, 16, row, 16).Style.Font.FontColor = XLColor.FromArgb(246, 57, 57);
				}
				//PARTIAL/COMPLETE BLANKS
				if (e.TipoPartial == "PARTIAL")
				{

					datos.Add(e.TipoPartial);
					wc.Range(row, 17, row, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(249, 136, 29);
					wc.Range(row, 17, row, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);

				}
				else if (e.TipoPartial == "COMPLETE")
				{
					datos.Add(e.TipoPartial);
					wc.Range(row, 17, row, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(64, 191, 128);
					wc.Range(row, 17, row, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.TipoPartial == null)
				{
					e.TipoPartial = "";
					datos.Add(e.TipoPartial);
					wc.Range(row, 17, row, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				//ART RECEIVED
				if (e.ImagenArte.StatusArteInf == "IN HOUSE")
				{
					datos.Add((e.ImagenArte.StatusArteInf).ToString());
					wc.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wc.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);

				}
				else if (e.ImagenArte.StatusArteInf == "REVIEWED")
				{
					datos.Add((e.ImagenArte.StatusArteInf).ToString());
					wc.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wc.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.ImagenArte.StatusArteInf == "PENDING")
				{
					datos.Add((e.ImagenArte.StatusArteInf).ToString());
					wc.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(236, 95, 95);
					wc.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.ImagenArte.StatusArteInf == "APPROVED")
				{
					string infoArte = e.ImagenArte.StatusArteInf + "-" + e.ImagenArte.FechaArte;
					datos.Add(infoArte);
					wc.Range(row, 18, row, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(246, 129, 51);
					wc.Range(row, 18, row, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				//TRIM RECEIVED
				if (e.Trims.restante <= 0 && e.Trims.estado == "1")
				{
					datos.Add(e.Trims.fecha_recibo);
					wc.Range(row, 19, row, 19).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wc.Range(row, 19, row, 19).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.Trims.restante >= 1 && e.Trims.estado == "1")
				{
					datos.Add(e.Trims.fecha_recibo);
					wc.Range(row, 19, row, 19).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 213, 67);
					wc.Range(row, 19, row, 19).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.Trims.fecha_recibo);
					wc.Range(row, 19, row, 19).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				//PACK INSTRUCTION
				if (e.InfoPackInstruction.Fecha_Pack != "" && e.InfoPackInstruction.EstadoPack == 1)
				{
					datos.Add(e.InfoPackInstruction.Fecha_Pack);
					wc.Range(row, 20, row, 20).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wc.Range(row, 20, row, 20).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoPackInstruction.Fecha_Pack);
					wc.Range(row, 20, row, 20).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}

				//PRICE TICKET RECEIVED
				if (e.InfoPriceTickets.Restante <= 0 && e.InfoPriceTickets.Estado == "1")
				{
					datos.Add(e.InfoPriceTickets.Fecha_recibo);
					wc.Range(row, 21, row, 21).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wc.Range(row, 21, row, 21).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else if (e.InfoPriceTickets.Restante >= 1 && e.InfoPriceTickets.Estado == "1")
				{
					datos.Add(e.InfoPriceTickets.Fecha_recibo);
					wc.Range(row, 21, row, 21).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 213, 67);
					wc.Range(row, 21, row, 21).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoPriceTickets.Fecha_recibo);
					wc.Range(row, 21, row, 21).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				//UCC RECEIVED
				if (e.InfoSummary.FechaUCC != "")
				{
					datos.Add(e.InfoSummary.FechaUCC);
					wc.Range(row, 22, row, 22).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 193, 116);
					wc.Range(row, 22, row, 22).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				else
				{
					datos.Add(e.InfoSummary.FechaUCC);
					wc.Range(row, 22, row, 22).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
				}
				datos.Add(e.CatComentarios.FechaComents);
				datos.Add(e.CatComentarios.Comentario);
				celdas.Add(datos.ToArray());
				wc.Cell(row, 1).Value = celdas;


				row++;
			}

		}



	}
}