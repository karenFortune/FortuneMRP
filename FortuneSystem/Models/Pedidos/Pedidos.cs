using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Catalogos;
using FortuneSystem.Models.Item;
using FortuneSystem.Models.Items;
using FortuneSystem.Models.Packing;
using FortuneSystem.Models.PNL;
using FortuneSystem.Models.POSummary;
using FortuneSystem.Models.PrintShop;
using FortuneSystem.Models.QCReport;
using FortuneSystem.Models.Revisiones;
using FortuneSystem.Models.Trims;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace FortuneSystem.Models.Pedidos
{

    public class OrdenesCompra
    {

        [Display(Name = "#")]
        public int IdPedido { get; set; }

        //[Required(ErrorMessage = "Ingrese el número de referencia de la orden.")]
        //[RegularExpression("/[^A-Z\u00f1\u00d1\u0020\0-9]/g", ErrorMessage = "El Orden Ref. debe contener sólo números y letras.")]
        [Display(Name = "ORDEN REF")]
        public string PO { get; set; }
        public string NombrePO { get; set; }


        [Display(Name = "PO")]
        public string VPO { get; set; }

        [Required]
        [Display(Name = "CUSTOMER")]
        [ForeignKey("CUSTOMER")]
        [Column("CUSTOMER")]
        public int Cliente { get; set; }

        public virtual CatCliente CatCliente { get; set; }
        public List<CatCliente> LCliente { get; set; }

        [Required]
        [Display(Name = "ORDEN CUSTOMER")]
        [ForeignKey("CUSTOMER_FINAL")]
        [Column("CUSTOMER_FINAL")]
        public int ClienteFinal { get; set; }

        public virtual CatClienteFinal CatClienteFinal { get; set; }
        public List<CatClienteFinal> LClienteFinal { get; set; }

        [Display(Name = "TYPE ORDEN")]
        public int IdTipoOrden { get; set; }
        public virtual CatTipoOrden CatTipoOrden { get; set; }
        public List<CatTipoOrden> ListadoTipoOrden { get; set; }

        [Required]
        [Display(Name = "STATUS")]
        [ForeignKey("ID_STATUS")]
        [Column("ID_STATUS")]
        public int IdStatus { get; set; }
        public virtual CatStatus CatStatus { get; set; }


        [Required]
        [RegularExpression("^[0-1][0-9][- /.][0-3][0-9][- /.][0-9]{4}$", ErrorMessage = "Incorrect date format.")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        [Display(Name = "CANCEL DATE")]
        public DateTime FechaCancel { get; set; }
        public string FechaCancelada { get; set; }

        [RegularExpression("^[0-1][0-9][- /.][0-3][0-9][- /.][0-9]{4}$", ErrorMessage = "Incorrect date format.")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        [Display(Name = "DATE")]
        public DateTime FechaFinalOrden { get; set; }
        public string FechaOrdenFinal { get; set; }

        [Required]
        // [RegularExpression("^[0-9]{4}-[0-1][0-9]-[0-3][0-9]$", ErrorMessage = "Formato de fecha incorrecta.")]
        [RegularExpression("^[0-1][0-9][- /.][0-3][0-9][- /.][0-9]{4}$", ErrorMessage = "Incorrect date format.")]
        [Display(Name = "DATE RECEIVED")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime FechaOrden { get; set; }
        public string FechaRecOrden { get; set; }

        //[Required(ErrorMessage = "Ingrese el total de unidades.")]
        [Required]
        [Display(Name = "TOTAL UNITS")]
        public int TotalUnidades { get; set; }

        [Display(Name = "TOTAL UNITS REGISTERED")]
        public int TotalUnidadesReg { get; set; }
        public List<CatCliente> ListaClientes { get; set; }

        public List<CatClienteFinal> ListaClientesFinal { get; set; }

        public List<CatStatus> ListaCatStatus { get; set; }

        public List<CatGenero> ListarTallasPorGenero { get; set; }

        public int Historial { get; set; }

        public virtual Revision Revision { get; set; }

        public virtual PrintShopC PrintShopC { get; set; }

        public virtual Pnl PNL { get; set; }

        public virtual PackingM Packing { get; set; }

        public int Usuario { get; set; }
        public string NombreUsr { get; set; }

        public string NombreClienteFinal { get; set; }

        public int HistorialPacking { get; set; }

        public List<PackingTypeSize> ListPack { get; set; }

        public List<ItemDescripcion> ListItems { get; set; }
        [Display(Name = "STYLE")]
        public int IdEstilo { get; set; }

        //posummary
        public int IdSummaryOrden { get; set; }
        public virtual InfoSummary InfoSummary { get; set; }
        //comentarios
        public virtual CatComentarios CatComentarios { get; set; }
        public List<CatComentarios> ListaComentarios { get; set; }
        public string FechaActComent { get; set; }
        //Tipo Brand
        public virtual CatTypeBrand CatTipoBrand { get; set; }
        //Imagen
        public virtual IMAGEN_ARTE ImagenArte { get; set; }
		public virtual IMAGEN_ARTE_PNL ImagenArtePnl { get; set; }
		//Recibo
		public virtual recibo Recibo { get; set; }
        public string MillPO { get; set; }
        public List<recibo> ListadoRecibosBlanks { get; set; }
        public List<recibo> ListadoRecibos { get; set; }
        public int TotalRecibo { get; set; }
        public string TipoPartial { get; set; }
        public int TotalRestante { get; set; }
        public string TallaRecibo { get; set; }
        //Printshop
        public int TotalPrinted { get; set; }
        public int RestaPrintshop { get; set; }
        //SALIDAS ESTILOS 
        public int DestinoSalida { get; set; }
        //Trims
        public virtual Trim_requests Trims { get; set; }
        public virtual InfoPriceTickets InfoPriceTickets { get; set; }
        //Shipped
        public virtual Shipped Shipped { get; set; }
        //Pack Instruction
        public virtual InfoPackInstruction InfoPackInstruction { get; set; }
		//Lista MillPO
		public List<InfoMillPO> ListaMillPO { get; set; }
		//Lista Tipo de Packing Estilo
		public List<CatTypePackItem> ListaTypePack { get; set; }
		//Lista periodo
		public List<Periodo> ListaPeriodos { get; set; }
		[Display(Name = "SEARCH YEAR:")]
		public int IdPeriodo { get; set; }
        public string EstatusPack { get; set; }
        public string EstatusPackAssort { get; set; }
        public string EstatusArtePnl { get; set; }
        public int CargoUser { get; set; }
        public int NumeroPedido { get; set; }
        public virtual QCReportGeneral QCReport { get; set; }
       

    }

    public class InfoSummary
    {
        public int IdItems { get; set; }
        public int CantidadEstilo { get; set; }
        public int IdTalla { get; set; }
        public string Talla { get; set; }
        public int CantidadTalla { get; set; }
        public int TotalEstilo { get; set; }
        public virtual ItemDescripcion ItemDesc { get; set; }
        public virtual CatColores CatColores { get; set; }
        public virtual CatGenero CatGenero { get; set; }
        public List<InfoSummary> ListSummary { get; set; }
        public string FechaUCC { get; set; }
		public int IdSucursal { get; set; }
       


    }

    public class InfoPriceTickets
    {
        public int Id_request_pt { get; set; }
        public string Estado { get; set; }
        public int Restante { get; set; }
        public string Descripcion { get; set; }
        public int Id_talla { get; set; }
        public int Total { get; set; }
        public string Tipo_item { get; set; }
        public string Talla { get; set; }
        public string Fecha_recibo { get; set; }
        public int Recibo { get; set; }
    }

    public class Shipped
    {
        public int Id_shipping { get; set; }
        public int Cantidad { get; set; }
        public int Id_summary { get; set; }
        public int IdTalla { get; set; }
        public List<Shipped> ListaShipped { get; set; }
    }

    public class InfoPackInstruction
    {
        public int IdInstructionPack { get; set; }
        public int EstadoPack { get; set; }
        public string Fecha_Pack { get; set; }
        public DateTime Fecha_Rec_Pack { get; set; }
    }

	public class InfoMillPO
	{
		public int IdMillPO { get; set; }
		public string MillPO { get; set; }
		public int IdPedido { get; set; }
	}

	public class Periodo
	{
	   public int NumPeriodo { get; set; }
	}



}