using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.Almacen
{
    public class recibos_item
    {
        public int id_recibo { get; set; }
        public int id_recibo_item { get; set; }
        public int id_inventario { get; set; }
        public int total { get; set; }
        public int total_orden { get; set; }
        public int total_inventario { get; set; }

        public int id_talla { get; set; }
        public int id_pedido { get; set; }
        public int id_summary { get; set; }
        public string talla { get; set; }
        public Inventario item { get; set; }

        public virtual caja_inventario ci { get; set; }
        public List<caja_inventario> lista_cajas { get; set; }

        public int id_ubicacion{ get; set; }
        public int id_porcentaje { get; set; }
        public string porcentaje { get; set; }
        public int id_pais{ get; set; }
        public int id_item{ get; set; }
        public string pais { get; set; }
        public string item_description { get; set; }
        public string pedido { get; set; }
        
        
    }
}