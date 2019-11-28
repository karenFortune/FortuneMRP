using FortuneSystem.Models;
using FortuneSystem.Models.Arte;
using FortuneSystem.Models.Catalogos;
using FortuneSystem.Models.Item;
using FortuneSystem.Models.Items;
using FortuneSystem.Models.Pedidos;
using FortuneSystem.Models.POSummary;
using FortuneSystem.Models.PrintShop;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace FortuneSystem.Controllers
{
    public class ArteController : Controller
    {
		readonly ArteData objArte = new ArteData();
		readonly CatTallaItemData objItem = new CatTallaItemData();
		readonly ItemDescripcionData objDesc = new ItemDescripcionData();
		readonly PedidosData objPedido = new PedidosData();
		readonly DescripcionItemData objItems = new DescripcionItemData();
		readonly CatEspecialidadesData objEspecialidad = new CatEspecialidadesData();
        private readonly MyDbContext db = new MyDbContext();
		

		public ActionResult Index()
        {
			List<IMAGEN_ARTE> listaArtes = objArte.ListaInvArtes().ToList();        
            return View(listaArtes);

        }

        public ActionResult IndexPNL()
        {
			List<IMAGEN_ARTE_PNL> listaArtes = objArte.ListaInvArtesPnl().ToList();
            return View(listaArtes);
        }

        public ActionResult CatalogoArte()
        {           
            return View();
        }

		public ActionResult CreateArteEstilo()
		{
			return View();
		}

		public ActionResult FileUpload(int idArte, string estilo, string descripcion)
        {          
            IMAGEN_ARTE IArte = db.ImagenArte.Find(idArte);
            ARTE art = db.Arte.Where(x => x.IdImgArte == idArte).FirstOrDefault();
			CatEspecialidades catEspecialidad = new CatEspecialidades();
			if (art != null)
			{

				catEspecialidad.IdEspecialidad = objItems.ObtenerEspecialidadPorIdSummary(art.IdSummary);
				
			}
			
			if (catEspecialidad.IdEspecialidad == 0)
            {
                catEspecialidad.IdEspecialidad = 13;
            }
            IArte.ListaTecnicas = objEspecialidad.ListaEspecialidades().ToList();
            ViewBag.listEspecialidad = new SelectList(IArte.ListaTecnicas, "IdEspecialidad", "Especialidad", catEspecialidad.IdEspecialidad);
            IArte.CATARTE = art;           
            IArte.CatEspecialidades = catEspecialidad;
            IArte.Estilo = estilo;
            IArte.DescripcionEstilo = descripcion;
            ObtenerEstados(IArte.StatusArte, IArte);
            
            return View(IArte);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FileUpload([Bind] IMAGEN_ARTE imagen_arte, HttpPostedFileBase fileArte)
        {
            if(imagen_arte.extensionArte == null)
            {
			 fileArte = imagen_arte.FileArte;
                if (fileArte != null)
                {
                    string ext = Path.GetFileName(fileArte.FileName);
                   // string path = Path.Combine(Server.MapPath("~/Content/imagenesArte"), ext);
                    string rutaBD = "C:/imagenesArte/" + ext;
                    if (System.IO.File.Exists(rutaBD))
                    {
                        imagen_arte.extensionArte = ext;
                    }
                    else
                    {
                        imagen_arte.extensionArte = ext;
                        fileArte.SaveAs(rutaBD);
                    }                       
                    
                    TempData["imagArteOK"] = "The Art image was registered correctly.";
                }
                else
                {
                    imagen_arte.extensionArte = "";
                }
            }
            else
            {
                imagen_arte.extensionArte = "";

            }

            ObtenerEstadosPorId(imagen_arte);
            imagen_arte.fecha = DateTime.Today;
            imagen_arte.CATARTE.IdEstilo =Convert.ToInt32(imagen_arte.IdEstilo);
            imagen_arte.ListaTecnicas = objEspecialidad.ListaEspecialidades().ToList();
            imagen_arte.idUsuario = Convert.ToInt32(Session["id_Empleado"]);
            ViewBag.listEspecialidad = new SelectList(imagen_arte.ListaTecnicas, "IdEspecialidad", "Especialidad", imagen_arte.CatEspecialidades.IdEspecialidad);
            //if (ModelState.IsValid)
            //{
                db.Entry(imagen_arte).State = EntityState.Modified;
                db.SaveChanges();
            TempData["imagArteOK"] = "The Art image was registered correctly.";
            return RedirectToAction("Index");
         //   }          
           // return View(imagen_arte);
        }
        
        public ActionResult FileUploadPNL(int id, int idEst)
        {
            IMAGEN_ARTE_PNL IArtes = new IMAGEN_ARTE_PNL();
            IMAGEN_ARTE_PNL IArte = db.ImagenArtePnl.Where(x => x.IdSummary == id).FirstOrDefault();
            int? idStatus = 0;
			
			if (IArte == null)
            {
                IArte = IArtes;
                IArte.StatusPNL = 4;
                idStatus = IArte.StatusPNL;
                IArte.IdSummary = id;
                IArte.EstadosPNL = 0;
                IArte.IdEstilo = idEst;				
				IArte.DescripcionEstilo = objDesc.ObtenerEstiloPorId(IArte.IdEstilo);   

            } else{
                idStatus = IArte.StatusPNL;
                IArte.DescripcionEstilo = objDesc.ObtenerEstiloPorId(IArte.IdEstilo);
			}
			IArte.Estilo = objDesc.ObtenerEstiloPorId(IArte.IdEstilo);
			IArte.Tienda = objArte.ObtenerclienteSummary(id);
            Regex kohl = new Regex("KOHL");
            Regex walmart = new Regex("WAL-");
            IArte.ResultadoK = kohl.Matches(IArte.Tienda);
            IArte.ResultadoW = walmart.Matches(IArte.Tienda); 
			IArte.fecha= DateTime.Today;
			ObtenerEstadosPNL(IArte.StatusPNL, IArte);    
            if(IArte.IdImgArtePNL == 0)
            {
                objArte.AgregarArtePnlImagen(IArte);
                IArte.IdImgArtePNL= objItems.Obtener_Utlimo_Id_Arte_Pnl();
            }          
                return PartialView(IArte);          
           
        }

        [HttpPost]
         public ActionResult FileUploadPNL(IMAGEN_ARTE_PNL artePNL)
        {
		
			if (artePNL.extensionPNL == null)
            {
				//filePNL = artePNL.FilePNL;
                if (artePNL.FilePNL != null)
                {
                    string ext = Path.GetFileName(artePNL.FilePNL.FileName);
                    string archivo = artePNL.FilePNL.FileName;
                   // string path = Path.Combine(Server.MapPath("~/Content/imagenesPNL"), ext);
                    string rutaBD = "C:/imagenesPNL/" + archivo;
                   // string ruta = Server.MapPath(rutaBD);
                    if (System.IO.File.Exists(rutaBD))
                    {
                        //System.IO.File.Delete(path);
                        artePNL.extensionPNL = ext;                       
                    }
                    else
                    {
                        artePNL.extensionPNL = ext;
						artePNL.FilePNL.SaveAs(rutaBD);
                    }
                    
                    TempData["imagPnlOK"] = "The PNL image was registered correctly.";
                }
            }

            if (artePNL.EstadosPNL == EstatusImgPNL.APPROVED)
            {
                artePNL.StatusPNL = 1;
            }
            else if (artePNL.EstadosPNL == EstatusImgPNL.INHOUSE)
            {
                artePNL.StatusPNL = 2;
            }
            else if (artePNL.EstadosPNL == EstatusImgPNL.REVIEWED)
            {
                artePNL.StatusPNL = 3;
            }
            else if (artePNL.EstadosPNL == EstatusImgPNL.PENDING)
            {
                artePNL.StatusPNL = 4;
            }
         
            int idPedido = objPedido.Obtener_Id_Pedido(artePNL.IdSummary);
			artePNL.fecha = DateTime.Today;
			 if (ModelState.IsValid)
			 {
				 db.Entry(artePNL).State = EntityState.Modified;
				 db.SaveChanges();

				 return RedirectToAction("Detalles", "Pedidos", new { id = idPedido });
			 }

			return View(artePNL);
        }
		public ActionResult ActualizarImagenArt(/*int? id,*/ int idArte, string status, int idEspecialidad, string combos, string comentarios)
		{
			IMAGEN_ARTE IArte = db.ImagenArte.Find(idArte);
			if (Request.Files.Count > 0)
			{
				try
				{
					HttpFileCollectionBase files = Request.Files;
					for (int i = 0; i < files.Count; i++)
					{
						HttpPostedFileBase file = files[i];
						string fname;

						if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER" || Request.Browser.Browser.ToUpper() == "FF")
						{
							string[] testfiles = file.FileName.Split(new char[] { '\\' });
							fname = testfiles[testfiles.Length - 1];
						}
						else
						{
							fname = file.FileName;
						}
						string ext = Path.GetFileName(file.FileName);
						//fname = Path.Combine(Server.MapPath("~/Content/imagenesArte"), ext);
                        string rutaBD = "C:/imagenesArte/" + ext;
                        if (System.IO.File.Exists(ext))
						{

							System.IO.File.Replace(IArte.extensionArte, ext, ext);
							IArte.extensionArte = ext;
							file.SaveAs(rutaBD);
						}
						else
						{
							IArte.extensionArte = ext;
                            //file.SaveAs(fname);
                            file.SaveAs(rutaBD);
						}


					}
                    ActualizarInfoImagenArte(idArte, status, idEspecialidad, IArte, combos, comentarios);
                    TempData["imgArteOK"] = "The Art was modified correctly.";
					return Json(new
					{
						redirectUrl = Url.Action("Index", "Arte"),
						isRedirect = true
					});
				}
				catch (Exception ex)
				{
					TempData["imgArteError"] = "The Art could not be modified, try it later." + ex.Message;
					return Json(new
					{
						redirectUrl = Url.Action("Index", "Arte"),
						isRedirect = true
					});
				}
			}
			else
			{
                ActualizarInfoImagenArte(idArte, status, idEspecialidad, IArte, combos, comentarios);
                TempData["imgArteOK"] = "The Art was modified correctly.";
				return Json(new
				{
					redirectUrl = Url.Action("Index", "Arte"),
					isRedirect = true
				});
			}


			//IArte.ExtensionL = objArte.BuscarExtensionPNLPorId(IArte.IdImgArtePNL);

			//return View(IArte);
		}

		[HttpPost]
         public ActionResult FileUploadEstilo(HttpPostedFileBase FileArte)
         {
             POSummary arte = new POSummary();
             if (arte.ExtensionArte == null)
             {
			    FileArte = arte.FileArte;
                 if (FileArte != null)
                 {
                     string ext = Path.GetFileName(FileArte.FileName);
                     string path = Path.Combine(Server.MapPath("~/Content/imagenesEstilos"), ext);
                     arte.ExtensionArte = ext;
                     FileArte.SaveAs(path);
                     TempData["imagArteOK"] = "The Art image was registered correctly.";
                 }
             }

             return View();
         }

        [HttpPost]
        public ActionResult UploadFiles()
        {
			if (Request.Files.Count > 0)
            {
                try
                {
					IMAGEN_ARTE_ESTILO arte = new IMAGEN_ARTE_ESTILO();
                    HttpFileCollectionBase files = Request.Files;
					HttpPostedFileBase file= Request.Files[0];
					string fname="";
					for (int i = 0; i < files.Count; i++)
                    {

                         file= files[i];
                       

                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER" || Request.Browser.Browser.ToUpper() == "FF")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }
						string ext = Path.GetFileName(file.FileName);
						fname = Path.Combine(Server.MapPath("~/Content/imagenesEstilos"), fname);
						if (System.IO.File.Exists(fname))
						{
							//System.IO.File.Delete(path);
							arte.extensionArt = ext;
						}
						else
						{
							arte.extensionArt = ext;
							file.SaveAs(fname);
						}

						arte.StatusArt = 3;
						arte.fecha = DateTime.Today;
						//arte.IdSummary = Convert.ToInt32(Session["IdItems"]);
						string nomEstilo = Convert.ToString(Session["nombreEstilo"]);
						arte.IdEstilo = objDesc.ObtenerIdEstilo(nomEstilo);
						objArte.AgregarArteEstilo(arte);
					}				

						return Json(file.FileName);
				}
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
		public ActionResult SaveDropzoneJsUploadedFiles()
		{			
			foreach (string fileName in Request.Files)
			{

				IMAGEN_ARTE_ESTILO arte = new IMAGEN_ARTE_ESTILO();
				HttpPostedFileBase file = Request.Files[fileName];
				HttpFileCollectionBase files = Request.Files;
				string fname = "";
				for (int i = 0; i < files.Count; i++)
				{

					file = files[i];


					if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER" || Request.Browser.Browser.ToUpper() == "FF")
					{
						string[] testfiles = file.FileName.Split(new char[] { '\\' });
						fname = testfiles[testfiles.Length - 1];
					}
					else
					{
						fname = file.FileName;
					}
					string ext = Path.GetFileName(file.FileName);
                    //fname = Path.Combine(Server.MapPath("~/Content/imagenesArte"), fname);
                    string rutaBD = "C:/imagenesArte/" + ext;
                    /*string extension = file.FileName;
					int tam_var = extension.Length;z
					string nomEstilo = extension.Substring(0, tam_var - 4);
					int idEstilo = objDesc.ObtenerIdEstilo(nomEstilo);
					var arteImg = db.ImagenArte.Where(x => x.IdEstilo == idEstilo).FirstOrDefault();*/
                    if (System.IO.File.Exists(rutaBD))
					{
						//file.SaveAs(rutaBD);
                    }
                    else
                    {
                        file.SaveAs(rutaBD);
                    }
                    
                    //You can Save the file content here
                }
					
			}

			return Json(new { Message = string.Empty });

		}

		[HttpPost]
		public ActionResult UploadFilesArtEstilo(int idSummary, int idEstilo, string color, string status)
		{
			if (Request.Files.Count > 0)
			{
				try
				{
					
					IMAGEN_ARTE_ESTILO arte = new IMAGEN_ARTE_ESTILO();
					HttpFileCollectionBase files = Request.Files;
					HttpPostedFileBase file = Request.Files[0];
					string fname = "";
					for (int i = 0; i < files.Count; i++)
					{

						file = files[i];


						if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER" || Request.Browser.Browser.ToUpper() == "FF")
						{
							string[] testfiles = file.FileName.Split(new char[] { '\\' });
							fname = testfiles[testfiles.Length - 1];
						}
						else
						{
							fname = file.FileName;
						}
						string ext = Path.GetFileName(file.FileName);
						//fname = Path.Combine(Server.MapPath("~/Content/imagenesArte"), fname);
                        string rutaBD = "C:/imagenesArte/" + ext;
                        if (System.IO.File.Exists(rutaBD))
						{
							
                        }
                        else
                        {
                            file.SaveAs(rutaBD);
                        }				
						var arteEstilo = db.ImagenArteEstilo.Where(x => x.IdEstilo == idEstilo && x.Color == color).FirstOrDefault();
						if(arteEstilo == null)
						{
							if (status == "APPROVED")
							{
								arte.StatusArt = 1;
							}
							else if (status == "REVIEWED")
							{
								arte.StatusArt = 2;
							}
							else if (status == "PENDING")
							{
								arte.StatusArt = 3;
							}
							else if (status == "INHOUSE")
							{
								arte.StatusArt = 4;
							}
							//arte.StatusArt = 3;
							arte.fecha = DateTime.Today;
							arte.extensionArt = ext;
							arte.IdSummary = idSummary;
							arte.Color = color;
							//arte.IdSummary = Convert.ToInt32(Session["IdItems"]);
							string nomEstilo = Convert.ToString(Session["nombreEstilo"]);
							arte.IdEstilo = idEstilo;//objDesc.ObtenerIdEstilo(nomEstilo);
							objArte.AgregarArteEstilo(arte);
						}
						
					}

					return Json(file.FileName);
				}
				catch (Exception ex)
				{
					return Json(new { Message = string.Empty });
				}
			}
			else
			{
				return Json(new { Message = string.Empty });
			}
		}

		// GET: Arte
		public ActionResult ListaImgArte(int id)
        {
			List<IMAGEN_ARTE> listaArtes = objArte.ListaArtes(id).ToList();
            return PartialView(listaArtes);
        }

        public ActionResult ListaImgArtePNL(int id)
        {
			List<IMAGEN_ARTE_PNL> listaArtes = objArte.ListaArtesPNL(id).ToList();
            return PartialView(listaArtes);
        }

        public ActionResult ConvertirImagen(int arteCodigo)
        {
            var arte = db.ImagenArte.Where(x => x.IdImgArte == arteCodigo).FirstOrDefault();
            if (arte != null)
            {
                if (arte.extensionArte != null && arte.extensionArte != "" )
                {
					return RutaImagenArte(arte);              
                
                }
                else
                {
                    return File("~/Content/img/noImagen.png", "image/png");
                }    
                         
            }
            else
            {
                return File("~/Content/img/noImagen.png", "image/png");
            }
            
        }


		public ActionResult BuscarConvertirImagenArte(int arteCodigo, string estilo, string color, int idSummary, int idEstilo)
		{
			IMAGEN_ARTE_ESTILO arteEstilo = new IMAGEN_ARTE_ESTILO();
			string descripcion = estilo.TrimEnd() + "_" + color.TrimEnd();
			var arte = db.ImagenArte.Where(x => x.IdImgArte == arteCodigo).FirstOrDefault();
            //var arteEstilo = db.ImagenArteEstilo.Where(x => x.IdSummary == idSummary).FirstOrDefault();
            //var arteEstilo = db.ImagenArteEstilo.Where(x => x.IdEstilo == idEstilo && x.Color == color).FirstOrDefault();
            if (arte != null)
            {
                int tam_var = arte.extensionArte.Length;
                string nomEstilo = "";
                if (tam_var != 0)
                {
                    nomEstilo = arte.extensionArte.Substring(0, tam_var - 4);
                }

                if (nomEstilo == descripcion && arte.extensionArte != null && arte.extensionArte != "")
                {
                    return RutaImagenArte(arte);
                }
                else
                {
                    BuscarRutaImagenEstilo(descripcion, arteEstilo);
                    if (/*arteEstilo != null &&*/ arteEstilo.extensionArt != "" && arteEstilo.extensionArt != null)
                    {
                        int tam_var2 = arteEstilo.extensionArt.Length;
                        string nomEsdesctiloArt = arteEstilo.extensionArt.Substring(0, tam_var2 - 4);
                        if (descripcion == nomEsdesctiloArt && arteEstilo.extensionArt != null)
                        {
                            return RutaImagenArteEstilo(arteEstilo);
                        }
                        else
                        {
                            return RutaImagenArte(arte);

                        }
                    }
                    else
                    {

                        return RutaImagenArte(arte);
                    }


                }
            }
            else
            {
                return File("~/Content/img/noImagen.png", "image/png");
            }
        }
		public ActionResult ConvertirImagenArte(string extensionArte)
		{
			IMAGEN_ARTE arte = new IMAGEN_ARTE() { extensionArte = extensionArte };

			if (arte.extensionArte != null && arte.extensionArte != "")
			{
				return RutaImagenArte(arte);

			}
			else
			{
				return File("~/Content/img/noImagen.png", "image/png");
			}

		}

		public ActionResult ConvertirImagenListaArteEstilo (string extensionArte, string color, string estilo)
        {
            IMAGEN_ARTE arte = new IMAGEN_ARTE() {extensionArte = extensionArte};
			IMAGEN_ARTE_ESTILO arteEstilo = new IMAGEN_ARTE_ESTILO();
			string descripcion = estilo.TrimEnd() + "_" + color.TrimEnd();
			if (arte != null && arte.extensionArte != "" && arte.extensionArte != null)
			{
				int tam_var = arte.extensionArte.Length;
				string nombreEstiloArt = arte.extensionArte.Substring(0, tam_var - 4);
				if (descripcion == nombreEstiloArt && arte.extensionArte != null && arte.extensionArte != "")
				{
					return RutaImagenArte(arte);
				}
				else
				{
					BuscarRutaImagenEstilo(descripcion, arteEstilo);
					if (/*arteEstilo != null &&*/ arteEstilo.extensionArt != "" && arteEstilo.extensionArt != null)
					{
						int tam_var2 = arteEstilo.extensionArt.Length;
						string nomEsdesctiloArt = arteEstilo.extensionArt.Substring(0, tam_var2 - 4);
						if (descripcion == nomEsdesctiloArt && arteEstilo.extensionArt != null)
						{
							return RutaImagenArteEstilo(arteEstilo);
						}
						else
						{
							return RutaImagenArte(arte);

						}
					}
					else
					{

						return RutaImagenArte(arte);
					}
				}
			}
			else
			{
				BuscarRutaImagenEstilo(descripcion, arteEstilo);
				return ObtenerImagenArteEstilo(descripcion, arteEstilo, arte);
			}

		}


        public ActionResult ConvertirImagenArtePNL(string extensionPnl)
        {
            IMAGEN_ARTE_PNL arte = new IMAGEN_ARTE_PNL() { extensionPNL = extensionPnl };
         
                if (arte.extensionPNL != null && arte.extensionPNL != "")
                {
					return RutaImagenPNL(arte);                                    
                }
                else
                {
                    return File("~/Content/img/noImagen.png", "image/png");
                }    
           
        }

        public ActionResult ConvertirImagenPNL(int pnlCodigo)
        {
            var arte = db.ImagenArtePnl.Where(x => x.IdImgArtePNL == pnlCodigo).FirstOrDefault();
            if (arte != null)
            {                              
                if (arte.extensionPNL != null && arte.extensionPNL != "" )
                {
					return RutaImagenPNL(arte);
                   /* switch (arte.extensionPNL.ToLower())
                    {
                        case "gif":
                            return new FilePathResult("~/Content/imagenesPNL/" + arte.extensionPNL, System.Net.Mime.MediaTypeNames.Image.Gif);
                        case "jpeg":
                            return new FilePathResult("~/Content/imagenesPNL/" + arte.extensionPNL, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                        default:
                            return new FilePathResult("~/Content/imagenesPNL/" + arte.extensionPNL, System.Net.Mime.MediaTypeNames.Application.Octet);
                    }  */                 
                }
                else
                {
                    return File("~/Content/img/noImagen.png", "image/png");
                }
            }
            else
            {
                return File("~/Content/img/noImagen.png", "image/png");
            }
        }

        public ActionResult ObtenerIdEstilo(int id)
        {
            Session["idSummary"] = id;
            return View();
        }


		public ActionResult BuscarImagenArte(string nombreEstilo)
		{
			int idEstilo = objDesc.ObtenerIdEstilo(nombreEstilo);
			var arte = db.ImagenArte.Where(x => x.IdEstilo == idEstilo).FirstOrDefault();
			if (arte != null)
			{
				if (arte.extensionArte != null && arte.extensionArte != "")
				{
					return new FilePathResult("C:/imagenesArte/" + arte.extensionArte, System.Net.Mime.MediaTypeNames.Application.Octet);
				}
				else
				{
					return File("~/Content/img/noImagen.png", "image/png");
				}

			}
			else
			{
				return File("~/Content/img/noImagen.png", "image/png");
			}

		}

        public ActionResult ConvertirImagenArteEstilo(string nombreEstilo, string color)
            {
			string descripcion = nombreEstilo.TrimEnd() + "_" + color.TrimEnd();
			IMAGEN_ARTE_ESTILO arteEstilo = new IMAGEN_ARTE_ESTILO();
			int idEstilo = objDesc.ObtenerIdEstilo(nombreEstilo);
                var arte = db.ImagenArte.Where(x => x.IdEstilo == idEstilo).FirstOrDefault();
			   //var arteEstilo = db.ImagenArteEstilo.Where(x => x.IdEstilo == idEstilo && x.Color == color).FirstOrDefault();
			/* if (arte != null || arteEstilo != null)
			 {*/
			if (arte != null && arte.extensionArte != "")
			{
				int tam_var = arte.extensionArte.Length;
				string nombreEstiloArt = arte.extensionArte.Substring(0, tam_var - 4);
				if (descripcion == nombreEstiloArt && arte.extensionArte != null && arte.extensionArte != "")	
				{
					return RutaImagenArte(arte);					
				}
				else
				{
					BuscarRutaImagenEstilo(descripcion, arteEstilo);
					if (/*arteEstilo != null &&*/ arteEstilo.extensionArt != "" && arteEstilo.extensionArt != null)
					{
						int tam_var2 = arteEstilo.extensionArt.Length;
						string nomEsdesctiloArt = arteEstilo.extensionArt.Substring(0, tam_var2 - 4);
						if (descripcion == nomEsdesctiloArt && arteEstilo.extensionArt != null)
						{
							return RutaImagenArteEstilo(arteEstilo);
						}
						else
						{
							return RutaImagenArte(arte);

						}
					}
					else
					{
						
						return RutaImagenArte(arte);
					}
				}
			}
			else
			{
				BuscarRutaImagenEstilo(descripcion, arteEstilo);
				return ObtenerImagenArteEstilo(descripcion, arteEstilo, arte);
			}
		}

        public void BuscarRutaImagenEstilo(string descripcion, IMAGEN_ARTE_ESTILO arteEstilo)
        {
            int i = 0;
            //string sourceDirectory2 = System.Web.HttpContext.Current.Server.MapPath("~/Content/imagenesArte/");
            //string sourceDirector32 = System.Web.HttpContext.Current.Server.MapPath("C:/imagenesArte/");
            string sourceDirectory2 = "C:/imagenesArte/";
                var files = Directory.EnumerateFiles(sourceDirectory2, descripcion + ".*");
                foreach (string currentFile in files)
                {
                    if (i == 0)
                    {
                        string fileName = currentFile.Substring(sourceDirectory2.Length);
                        arteEstilo.extensionArt = fileName;
                        i++;
                    }
                }
        }

        private ActionResult ObtenerImagenArteEstilo(string descripcion, IMAGEN_ARTE_ESTILO arteEstilo, IMAGEN_ARTE arte)
		{
			if (arteEstilo.extensionArt != "" && arteEstilo.extensionArt != null)
			{
				int tam_var2 = arteEstilo.extensionArt.Length;
				string nomEsdesctiloArt = arteEstilo.extensionArt.Substring(0, tam_var2 - 4);
				if (descripcion == nomEsdesctiloArt && arteEstilo.extensionArt != null)
				{
					return RutaImagenArteEstilo(arteEstilo);
				}
				else
				{
					return RutaImagenArte(arte);
				}
			}
			else
			{
				return File("~/Content/img/noImagen.png", "image/png");
			}
		}

		public ActionResult ConvertirImagenPNLEstilo(string nombreEstilo, string IdItem)
        {
			int idEstilo= objDesc.ObtenerIdEstilo(nombreEstilo);
            int IdItems = Convert.ToInt32(IdItem);
            var arte = db.ImagenArtePnl.Where(x => x.IdEstilo == idEstilo && x.IdSummary == IdItems).FirstOrDefault();
            if (arte != null)
            {
                if (arte.extensionPNL != null && arte.extensionPNL != "")
				{
					return RutaImagenPNL(arte);
				}
				else
                {
                    return File("~/Content/img/noImagen.png", "image/png");
                }
                
            } else
            {
                return File("~/Content/img/noImagen.png", "image/png");
            }
           
        }

		public ActionResult Create(int? id, int idArte, string estilo, string color)
        {
            IMAGEN_ARTE IArte = db.ImagenArte.Find(idArte);

           ARTE art = db.Arte.Where(x => x.IdImgArte == idArte).FirstOrDefault();
            Session["id"]= id;   
            int Summary = Convert.ToInt32(Session["id"]);
			IArte.Estilo = estilo.TrimEnd();
			IArte.Color = color.TrimEnd();
            art.IdEstilo = Summary;
            IArte.CATARTE = art;
            IArte.Tienda = objArte.ObtenerclienteEstilo(id, idArte);
            Regex kohl = new Regex("KOHL");
            Regex walmart = new Regex("WAL-");
            IArte.ResultadoK = kohl.Matches(IArte.Tienda);
            IArte.ResultadoW = walmart.Matches(IArte.Tienda);
            IArte.fecha = DateTime.Today;
            ObtenerEstados(IArte.StatusArte, IArte);     
        
            return View(IArte);
        }

        public ActionResult EditarArtePNL(int? id, int idArte)
        {
            IMAGEN_ARTE_PNL IArte = db.ImagenArtePnl.Find(idArte);

            Session["id"] = id;
            int Summary = Convert.ToInt32(Session["id"]);
            IArte.IdSummary = id;
            IArte.Tienda = objArte.ObtenerclienteSummary(IArte.IdSummary);       
            Regex kohl = new Regex("KOHL");
            Regex walmart = new Regex("WAL-");
            IArte.ResultadoK = kohl.Matches(IArte.Tienda);
            IArte.ResultadoW = walmart.Matches(IArte.Tienda);
            IArte.Estilo = objDesc.ObtenerEstiloPorId(IArte.IdEstilo);
			IArte.fecha = DateTime.Today;
			ObtenerEstadosPNL(IArte.StatusPNL, IArte);  

            return View(IArte);
        }

        public ActionResult ActualizarImagenPNL(int idArte)
        {
            IMAGEN_ARTE_PNL IArte = db.ImagenArtePnl.Find(idArte);

           IArte.extensionPNL= objArte.BuscarExtensionPNLPorId(IArte.IdImgArtePNL);

            return View(IArte);
        }

        public ActionResult ActualizarImagenArtePNL(/*int? id,*/ int idArte,string status, int idEspecialidad, string combos, string comentarios)
        {
            IMAGEN_ARTE IArte = db.ImagenArte.Find(idArte);
            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];
                        string fname;

                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER" || Request.Browser.Browser.ToUpper() == "FF")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }
                        string ext = Path.GetFileName(file.FileName);
                        //fname = Path.Combine(Server.MapPath("~/Content/imagenesArte"), ext);
                        string rutaBD = "C:/imagenesArte/" + ext;
                        if (System.IO.File.Exists(ext))
                         {
                            
                            System.IO.File.Replace(IArte.extensionArte, ext, ext);
                            IArte.extensionArte = ext;
                            file.SaveAs(rutaBD);
                        }
                         else
                         {
                            IArte.extensionArte = ext;
                            file.SaveAs(rutaBD);
                        }
                        

                    }
                    ActualizarInfoImagenArte(idArte, status, idEspecialidad, IArte, combos, comentarios);
                    TempData["imgArteOK"] = "The Art was modified correctly.";
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Arte"),
                        isRedirect = true
                    });
                }
                catch (Exception ex)
                {
                    TempData["imgArteError"] = "The Art could not be modified, try it later." + ex.Message;
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Arte"),
                        isRedirect = true
                    });
                }
            }
            else
            {
                ActualizarInfoImagenArte(idArte, status, idEspecialidad, IArte, combos, comentarios);
                TempData["imgArteOK"] = "The Art was modified correctly.";
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Arte"),
                    isRedirect = true
                });
            }
           

            //IArte.ExtensionL = objArte.BuscarExtensionPNLPorId(IArte.IdImgArtePNL);

           //return View(IArte);
        }

        public void ActualizarInfoImagenArte(int idArte, string status, int idEspecialidad, IMAGEN_ARTE IArte, string combos, string comentarios )
        {
            
            if (status == "APPROVED")
            {
                IArte.StatusArte = 1;

            }
            else if (status == "REVIEWED")
            {
                IArte.StatusArte = 2;

            }
            else if (status == "PENDING")
            {
                IArte.StatusArte = 3;
            }
            else if (status == "INHOUSE")
            {
                IArte.StatusArte = 4;
            }
            IArte.fecha = DateTime.Today;
            IArte.combos = combos;
            IArte.comentarios = comentarios;
            objArte.ActualizarImagen(IArte);
            List<int> listado = objArte.ListaEstilosPorImagenesArte(idArte).ToList();
            foreach (int id in listado)
            {
                objArte.ActualizarEspecialidadImagenArte(id, idEspecialidad);
            }
        }

        public void ObtenerEstadosPNL(int? idStatus, IMAGEN_ARTE_PNL IArte)
        {
            //Obtener el idEstado PNL
            if (idStatus == 1)
            {
                IArte.EstadosPNL = EstatusImgPNL.APPROVED;
            }
            else if (idStatus == 2)
            {
                IArte.EstadosPNL = EstatusImgPNL.INHOUSE;
            }
            else if (idStatus == 3)
            {
                IArte.EstadosPNL = EstatusImgPNL.REVIEWED;
            }
            else if (idStatus == 4)
            {
                IArte.EstadosPNL = EstatusImgPNL.PENDING;
            }
        }

        public void ObtenerEstados(int? idEstadoArte, IMAGEN_ARTE arte)
        {
            //Obtener el idEstado Arte 
            if (idEstadoArte == 1)
            {
                arte.EstadosArte = EstatusArte.APPROVED;
            }
            else if (idEstadoArte == 2)
            {
                arte.EstadosArte = EstatusArte.REVIEWED;
            }
            else if (idEstadoArte == 3)
            {
                arte.EstadosArte = EstatusArte.PENDING;
            }
            else if (idEstadoArte == 4)
            {
                arte.EstadosArte = EstatusArte.INHOUSE;
            }
        }  

        public void ObtenerEstadosPorId(IMAGEN_ARTE Arte)
        {
            if (Arte.EstadosArte == EstatusArte.APPROVED)
            {
                Arte.StatusArte = 1;
            }
            else if (Arte.EstadosArte == EstatusArte.REVIEWED)
            {
                Arte.StatusArte = 2;
            }
            else if (Arte.EstadosArte == EstatusArte.PENDING)
            {
                Arte.StatusArte = 3;
            }
            else if (Arte.EstadosArte == EstatusArte.INHOUSE)
            {
                Arte.StatusArte = 4;
            }

            if (Arte.EstadosPNL == EstatusPNL.APPROVED)
            {
                Arte.StatusPNL = 1;
            }
            else if (Arte.EstadosPNL == EstatusPNL.INHOUSE)
            {
                Arte.StatusPNL = 2;
            }
            else if (Arte.EstadosPNL == EstatusPNL.REVIEWED)
            {
                Arte.StatusPNL = 3;
            }
            else if (Arte.EstadosPNL == EstatusPNL.PENDING)
            {
                Arte.StatusPNL = 4;
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind] IMAGEN_ARTE Arte)
        {
                    
            return View(Arte);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarArtePNL([Bind] IMAGEN_ARTE_PNL artePNL, HttpPostedFileBase filePNL)
        {
            if (artePNL.extensionPNL == null)
            {
				filePNL = artePNL.FilePNL;
                if (filePNL != null)
                {
                    string ext = Path.GetFileName(filePNL.FileName);
                   // string path = Path.Combine(Server.MapPath("~/Content/imagenesPNL"), ext);
                    string rutaBD = "C:/imagenesPNL/" + ext;
                    if (System.IO.File.Exists(rutaBD))
                    {
                        //System.IO.File.Delete(path);
                        artePNL.extensionPNL = ext;
                    }
                    else
                    {
                        artePNL.extensionPNL = ext;
                        filePNL.SaveAs(rutaBD);
                    }

                    TempData["imagArtePNLOK"] = "The PNL image was modified correctly.";
                    objArte.ActualizarImagenPNL(artePNL);
                }
            }
                
            IMAGEN_ARTE_PNL IArte = db.ImagenArtePnl.Find(artePNL.IdImgArtePNL);

            //Session["id"] = id;
            //int Summary = Convert.ToInt32(Session["id"]);
            artePNL.IdSummary = IArte.IdSummary;
            artePNL.Tienda = objArte.ObtenerclienteSummary(IArte.IdSummary);
            Regex kohl = new Regex("KOHL");
            Regex walmart = new Regex("WAL-");
            artePNL.ResultadoK = kohl.Matches(artePNL.Tienda);
            artePNL.ResultadoW = walmart.Matches(artePNL.Tienda);
            artePNL.Estilo = objDesc.ObtenerEstiloPorId(IArte.IdEstilo);
            if (artePNL.EstadosPNL == EstatusImgPNL.APPROVED)
            {
                artePNL.StatusPNL = 1;
            }
            else if (artePNL.EstadosPNL == EstatusImgPNL.INHOUSE)
            {
                artePNL.StatusPNL = 2;
            }
            else if (artePNL.EstadosPNL == EstatusImgPNL.REVIEWED)
            {
                artePNL.StatusPNL = 3;
            }
            else if (artePNL.EstadosPNL == EstatusImgPNL.PENDING)
            {
                artePNL.StatusPNL = 4;
            }
            ObtenerEstadosPNL(artePNL.StatusPNL, artePNL);

            objArte.ActualizarEstadoImagenPNL(artePNL);
            TempData["imagArtePNLOK"] = "The PNL image was modified correctly.";
            return RedirectToAction("IndexPNL");
        }



        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IMAGEN_ARTE arte = db.ImagenArte.Find(id);
            arte.Tienda = objArte.ObtenerclienteEstilo(id, arte.IdImgArte);
            if (arte == null)
            {
                return HttpNotFound();
            }
        
            return View(arte);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
		public ActionResult Edit([Bind] IMAGEN_ARTE Arte, HttpPostedFileBase imgArte, HttpPostedFileBase imgPNL)
        {
			 imgArte = Arte.FileArte;

            if (imgArte != null)
            {
                string ext = Path.GetFileName(imgArte.FileName);
                //string path = Path.Combine(Server.MapPath("~/Content/imagenesArte"), ext);
                string rutaBD = "C:/imagenesArte/" + ext;
                Arte.extensionArte = ext;
                imgArte.SaveAs(rutaBD);
            }

	       imgPNL = Arte.FilePNL;
            if (imgPNL != null)
            {
                string ext = Path.GetFileName(imgPNL.FileName);
               // string path = Path.Combine(Server.MapPath("~/Content/imagenesArte"), ext);
                string rutaBD = "C:/imagenesArte/" + ext;
                Arte.extensionPNL = ext;
                imgPNL.SaveAs(rutaBD);
            }
            if (ModelState.IsValid)
            {
                //db.Entry(Arte).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
     
            return View(Arte);
        }
        

        public JsonResult Lista_Tallas_Estilo(int id)
        {
            IMAGEN_ARTE arte = new IMAGEN_ARTE();
            int idEstilo = Convert.ToInt32(Session["id"]);
            List<CatTallaItem> listaT = objItem.Lista_tallas_Estilo_Arte(idEstilo).ToList();
            arte.ListaTallas = listaT;

            List<UPC> listaU = objItem.Lista_tallas_upc(id).ToList();
            var result = Json(new { listaTalla = listaT, listaUPC = listaU});
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Lista_Tallas_Estilo_Arte_Pnl(int id, int idEst)
        {
            IMAGEN_ARTE_PNL arte = new IMAGEN_ARTE_PNL();
            List<CatTallaItem> listaT = objItem.Lista_tallas_Estilo_Arte(id).ToList();
            arte.ListaTallas = listaT;

            List<UPC> listaU = objItem.Lista_tallas_upc(idEst).ToList();
            var result = Json(new { listaTalla = listaT, listaUPC = listaU });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Capture()
        {
            string CapturedFilePath = "";
            try
            {
                Bitmap bitmap = new Bitmap
              (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);

                Graphics graphics = Graphics.FromImage(bitmap as System.Drawing.Image);
                graphics.CopyFromScreen(25, 25, 25, 25, bitmap.Size);

                bitmap.Save(CapturedFilePath, ImageFormat.Bmp);
            }
            catch (Exception)
            {
               
            }
            return View();
        }

		public static ActionResult RutaImagenArte(IMAGEN_ARTE arte)
        {
           //~/Content/imagenesArte/
            switch (arte.extensionArte.ToLower())
			{
				case "gif":
					return new FilePathResult("C:/imagenesArte/" + arte.extensionArte, System.Net.Mime.MediaTypeNames.Image.Gif);
				case "jpeg":
					return new FilePathResult("C:/imagenesArte/" + arte.extensionArte, System.Net.Mime.MediaTypeNames.Image.Jpeg);
				default:
					return new FilePathResult("C:/imagenesArte/" + arte.extensionArte, System.Net.Mime.MediaTypeNames.Application.Octet);
			}
		}


		private static ActionResult RutaImagenArteEstilo(IMAGEN_ARTE_ESTILO arteEstilo)
		{
			switch (arteEstilo.extensionArt.ToLower())
			{
				case "gif":
					return new FilePathResult("C:/imagenesArte/" + arteEstilo.extensionArt, System.Net.Mime.MediaTypeNames.Image.Gif);
				case "jpeg":
					return new FilePathResult("C:/imagenesArte/" + arteEstilo.extensionArt, System.Net.Mime.MediaTypeNames.Image.Jpeg);
				default:
					return new FilePathResult("C:/imagenesArte/" + arteEstilo.extensionArt, System.Net.Mime.MediaTypeNames.Application.Octet);
			}
		}

		private static ActionResult RutaImagenPNL(IMAGEN_ARTE_PNL arte)
		{
			switch (arte.extensionPNL.ToLower())
			{
				case "gif":
					return new FilePathResult("C:/imagenesPNL/" + arte.extensionPNL, System.Net.Mime.MediaTypeNames.Image.Gif);
				case "jpeg":
					return new FilePathResult("C:/imagenesPNL/" + arte.extensionPNL, System.Net.Mime.MediaTypeNames.Image.Jpeg);
				default:
					return new FilePathResult("C:/imagenesPNL/" + arte.extensionPNL, System.Net.Mime.MediaTypeNames.Application.Octet);
			}
		}


	}
    
}