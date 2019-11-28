using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.QCReport
{
	public class QCPruebaLavado
	{
		public int IdQCPruebasLavados { get; set; }
		[Display(Name = "HOUR")]
		public DateTime HoraLavado { get; set; }
		public int IdTalla { get; set; }
		public string Talla { get; set; }
		public int Results { get; set; }
		public int IdQCReport { get; set; }
        public string Fecha { get; set; }
    }
}