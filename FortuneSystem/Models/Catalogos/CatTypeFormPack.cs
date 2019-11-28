using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.Catalogos
{
	public class CatTypeFormPack
	{
		[Display(Name = "#")]
		public int IdTipoFormPack{ get; set; }
		[Display(Name = "PACKING FORM")]
		public string TipoFormPack { get; set; }
	}
}