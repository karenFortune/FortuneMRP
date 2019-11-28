using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.QCReport
{
	public class QCMisPrints
	{
		public int IdQCMisprints { get; set; }
		public DateTime FechaRegistro { get; set; }
		public int IdTalla { get; set; }
		public int MisPrint1st { get; set; }
		public int MisPrint2nd { get; set; }
		public int Repairs1st { get; set; }
		public int Repairs2nd { get; set; }
		public int Sprayed1st { get; set; }
		public int Sprayed2nd { get; set; }
		public int Defects1st { get; set; }
		public int Defects2nd { get; set; }
		public int IdSummary { get; set; }
        public string Talla { get; set; }
    }
}