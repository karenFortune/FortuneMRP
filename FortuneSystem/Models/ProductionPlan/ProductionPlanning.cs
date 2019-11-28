using FortuneSystem.Models.Items;
using FortuneSystem.Models.Pedidos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.ProductionPlan
{
	public class ProductionPlanning
	{
		[Display(Name = "#")]
		public int IdProductionPlan { get; set; }
		[Display(Name = "STYLE")]
		public int IdSummary { get; set; }
		[RegularExpression("^[0-1][0-9][- /.][0-3][0-9][- /.][0-9]{4}$", ErrorMessage = "Incorrect date format.")]
		[DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
		[Display(Name = "DATE")]
		public DateTime Fecha { get; set; }
		[RegularExpression("^[0-1][0-9][- /.][0-3][0-9][- /.][0-9]{4}$", ErrorMessage = "Incorrect date format.")]
		[DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
		[Display(Name = "DATE")]
		public DateTime FechaAct { get; set; }
		public string DiaFecha { get; set; }
		public string FechaPlan { get; set; }
		[Display(Name = "SHIFT")]
		public Turno Turnos { get; set; }
		[Display(Name = "SHIFT")]
		public Turno TurnosAct { get; set; }
		public int IdTurno { get; set; }
		[Display(Name = "PRIORITY")]
		public Prioridad Prioridades { get; set; }
		[Display(Name = "PRIORITY")]
		public Prioridad PrioridadesAct { get; set; }
		[Display(Name = "STATUS")]
		public Estatus Estatus { get; set; }
		public int IdPrioridad { get; set; }
		public int IdUser { get; set; }
		[Display(Name = "OVER")]
		public int IdHorno { get; set; }
		public virtual ProdOverMachine ProdOverMachine { get; set; }
		[Display(Name = "MACHINE")]
		public int IdProdOverMachine { get; set; }
		[Display(Name = "PO")]
		public int IdPedido { get; set; }
		public string PO { get; set; }
		public List<OrdenesCompra> ListaPOs { get; set; }
		public List<CatProdOver> ListHornos { get; set; }
		[Display(Name = "METEDOR")]
		public string Metedor { get; set; }
		[Display(Name = "SACADOR")]
		public string Sacador { get; set; }
		[Display(Name = "CACHADOR")]
		public string Cachador { get; set; }
		[Display(Name = "METEDOR")]
		public string MetedorAct { get; set; }
		[Display(Name = "SACADOR")]
		public string SacadorAct { get; set; }
		[Display(Name = "CACHADOR")]
		public string CachadorAct { get; set; }
		public string Status { get; set; }
		public string StatusAct { get; set; }
		public virtual ItemDescripcion ItemDescripcion { get; set; }
		public string Pedido { get; set; }
		public int IdEstilo { get; set; }
		
	

	}

	public class CatProdOver
	{
		[Display(Name = "#")]
		public int IdProdCatOver { get; set; }
		[Display(Name = "OVER")]
		public string Horno { get; set; }
	}

	public class ProdOverMachine
	{
		[Display(Name = "#")]
		public int IdProdOverMachine { get; set; }
		public virtual CatProdOver CatProdOver { get; set; }
		public int IdProdCatOver { get; set; }
		[Display(Name = "# MACHINE")]
		public int NoMaquina { get; set; }
		public List<ProdOverMachine> ListaMaquinas { get; set; }
	}
	
	public enum Turno
	{
		First = 1,
		Second = 2
	}

	public enum Prioridad
	{
		Low = 1,
		High = 2
	}

	public enum Estatus
	{
		ABIERTO = 1,
		CERRADO = 2
	}
}