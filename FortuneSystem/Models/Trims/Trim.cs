using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FortuneSystem.Models.Shipping;
using FortuneSystem.Models.Almacen;

namespace FortuneSystem.Models.Trims
{
    public class Trim_item
    {
        public int id_item { get; set; }
        public string item { get; set; }
        public string descripcion { get; set; }
        public string family { get; set; }
        public string unit { get; set; }
        public int minimo { get; set; }
        public int total { get; set; }
    }
    public class Pedido_t
    {
        public int id_pedido { get; set; }
        public string pedido { get; set; }
        public List<Estilos_t> lista_estilos { get; set; }
        public int total { get; set; }
        public int cantidad { get; set; }
        public int estado { get; set; }
        public List<Trim_requests> lista_requests { get; set; }
    }
    public class Estilos_t
    {
        public int id_estilo { get; set; }
        public int id_po_summary { get; set; }
        public string estilo { get; set; }
        public string descripcion { get; set; }
        public string genero { get; set; }
        public List<Talla_t> lista_tallas { get; set; }
        public List<Trim_requests> lista_requests { get; set; }
    }
    public class Talla_t
    {
        public int id_talla { get; set; }
        public int total { get; set; }
        public int total_orden { get; set; }
        public int total_entregado { get; set; }
        public string talla { get; set; }
        public int impreso { get; set; }
        public int id { get; set; }
        public int id_trim_estilos { get; set; }
    }
    public class Trim_requests
    {
        public int id_request { get; set; }
        public int id_entrega_item { get; set; }
        public int id_entrega { get; set; }
        public int id_talla { get; set; }
        public int id_summary { get; set; }
        public int id_item { get; set; }
        public int total { get; set; }
        public int cantidad { get; set; }
        public int blanks { get; set; }
        public int restante { get; set; }
        public int revision { get; set; }
        public int id_usuario { get; set; }
        public int id_estilo { get; set; }
        public string talla { get; set; }
        public string item { get; set; }
        public string usuario { get; set; }
        public string fecha { get; set; }
        public string estilo { get; set; }
        public string fecha_recibo { get; set; }
        public string tipo_item { get; set; }
        public string comentarios { get; set; }
        public string mill_po { get; set; }
        public int recibo { get; set; }
        public int id_pedido { get; set; }
        public int entregado { get; set; }
        public string pedido { get; set; }
        public int auditado { get; set; }
        public int id_estado { get; set; }
        public int id_inventario { get; set; }
        public string estado { get; set; }
        public string Descripcion { get; set; }
        public string estilo_descripcion { get; set; }
        public recibo recibo_item { get; set; }
        public int location { get; set; }
        public string lugar_location { get; set; }
        public int entrega { get; set; }
        public int impreso { get; set; }
        public int familia { get; set; }
        public int stock { get; set; }
        public int templates_pt { get; set; }
        public int id_sucursal { get; set; }
        public int id_location { get; set; }
        public List<Trim_location> lista_locations { get; set; }
        public List<Trim_estilo> lista_estilos { get; set; }
        public List<Trim_entregas> lista_entrega { get; set; }
    }
    public class Item_t
    {
        public int id_item { get; set; }
        public int categoria { get; set; }
        public int total { get; set; }
        public int total_estilo { get; set; }
        public string descripcion { get; set; }
        public string componente { get; set; }
        public string fecha { get; set; }
        public string familia { get; set; }

    }
    public class registro_price_tickets
    {
        public int id_price_ticket { get; set; }
        public int estado { get; set; }
        public int id_summary { get; set; }
        public int id_pedido { get; set; }
        public string pedido { get; set; }
        public int id_estilo { get; set; }
        public string estilo { get; set; }
        public string total { get; set; }
        public string upc { get; set; }
        public string descripcion_estilo { get; set; }
        public int id_color { get; set; }
        public string color { get; set; }
        public int id_talla { get; set; }
        public int impreso { get; set; }
        public string talla { get; set; }
        public string tickets { get; set; }
        public string dept { get; set; }
        public string clas { get; set; }
        public string sub { get; set; }
        public string retail { get; set; }
        public string cl { get; set; }
        public string usuario { get; set; }
        public string fecha { get; set; }
    }
    public class Trim_location
    {
        public int id_location { get; set; }
        public int id_usuario { get; set; }
        public string fecha { get; set; }
        public string location { get; set; }
        public string usuario { get; set; }
        public int total { get; set; }
        public List<Trim_location_item> lista_items { get; set; }
    }
    public class Trim_location_item
    {
        public int id_location_item { get; set; }
        public int id_location { get; set; }
        public int id_trim_location { get; set; }
        public int id_ubicacion{ get; set; }
        public string ubicacion { get; set; }
        public string comentarios { get; set; }
        public int total { get; set; }
        public int id_request { get; set; }
    }
    public class Trim_estilo
    {
        public int id { get; set; }
        public int id_request { get; set; }
        public int id_summary { get; set; }
        public int id_estilo { get; set; }
        public string estilo { get; set; }
        public int impreso { get; set; }
        public int total_orden { get; set; }
        public int total_entregado { get; set; }
        public string descripcion { get; set; }
        public List<Talla_t> lista_tallas { get; set; }

    }
    public class Trim_entregas
    {
        public int id_entrega { get; set; }
        public int id_pedido { get; set; }
        public string entrega { get; set; }
        public string recibe { get; set; }
        public string fecha { get; set; }
        public string pedido { get; set; }
        public string comentarios { get; set; }
        public int total { get; set; }
        public int id_request { get; set; }
        public List<Trim_requests> lista_request { get; set; }       
        public List<Trim_entregas_items> lista_entregas { get; set; }
    }
    public class Trim_entregas_items
    {
        public int id_entrega_item { get; set; }
        public int id_entrega { get; set; }
        public int id_item { get; set; }
        public int id_trim_orden { get; set; }
        public int id_trim_request { get; set; }
        public int id_inventario { get; set; }
        public int total_orden { get; set; }
        public int total { get; set; }
        public int total_anterior { get; set; }
        public string comentarios { get; set; }
        public string item { get; set; }
        public string estilo { get; set; }
        public string talla { get; set; }
        public int restante { get; set; }

    }
    public class Impresion
    {
        public int id_impresion { get; set; }
        public int id_pedido { get; set; }
        public int id_summary { get; set; }
        public int id_talla { get; set; }
        public string tipo { get; set; }

    }
    public class Pedidos_trim
    {
        public int id_pedido { get; set; }
        public string pedido { get; set; }
        public int id_customer { get; set; }
        public int id_usuario { get; set; }
        public string customer { get; set; }
        public string usuario { get; set; }
        public string ship_date { get; set; }
        public int id_gender { get; set; }
        public string gender { get; set; }
        public string fold_size { get; set; }
        public string estado { get; set; }
        public List<Empaque> lista_empaque { get; set; }
        public List<Assortment> lista_assort { get; set; }
        public List<Family_trim> lista_families { get; set; }
        public List<Estilos_trims> lista_estilos { get; set; }
        public List<Trim_requests> lista_trims { get; set; }

    }
    public class Family_trim
    {
        public int id_family_trim { get; set; }
        public string family_trim { get; set; }
        public List<Trim_requests> lista_requests { get; set; }
    }
    public class clientes
    {
        public int id_customer { get; set; }
        public string customer { get; set; }
    }
    public class generos
    {
        public int id_genero { get; set; }
        public string genero { get; set; }
    }
    public class imagenes_trim
    {
        public int id_imagen { get; set; }
        public string imagen { get; set; }
        public string fecha { get; set; }
        public int id_usuario { get; set; }
        public string usuario { get; set; }
        public int id_familia { get; set; }
        public string familia { get; set; }
        public List<imagen_datos> lista_datos { get; set; }
    }
    public class imagen_datos
    {
        public int id_genero { get; set; }
        public string genero { get; set; }
        public int id_customer { get; set; }
        public string customer { get; set; }
    }
    public class Pedidos_trim_card
    {
        public int id_trim_card { get; set; }
        public int id_pedido { get; set; }
        public string pedido { get; set; }
        public int id_customer { get; set; }
        public string customer { get; set; }
        public string fecha { get; set; }
        public int id_gender { get; set; }
        public string gender { get; set; }
        public string fold_size { get; set; }
        public string ratio { get; set; }
        public int tipo_empaque { get; set; }
        public int id_usuario { get; set; }
        public int estado { get; set; }
        public string usuario { get; set; }
        public string comentarios { get; set; }
        public string entrega { get; set; }
        public string recibe{ get; set; }
        public string fecha_entrega{ get; set; }
        public List<Familias_trim_card> lista_familias { get; set; }
        public List<Estilos_t> lista_estilos { get; set; }
        public List<generos> lista_generos { get; set; }
    }
    public class Familias_trim_card
    {
        public int id_family_trim { get; set; }
        public int id_item { get; set; }
        public string family_trim { get; set; }
        public string item { get; set; }
        public int id_imagen { get; set; }
        public string imagen { get; set; }
        public int id_especial { get; set; }
        public string especial { get; set; }
        public string notas { get; set; }
        public List<imagenes_trim> lista_imagenes { get; set; }
        public List<generos> lista_generos { get; set; }
    }
    public class Transferencia_trim
    {
        public int id_transferencia { get; set; }
        public int id_usuario { get; set; }
        public string fecha { get; set; }
        public string usuario { get; set; }
        public List<Transferencia_item> lista_items { get; set; }
    }
    public class Transferencia_item{
        public int id_transferencia { get; set; }
        public int id_transferencia_item { get; set; }
        public int id_item { get; set; }
        public int id_pedido { get; set; }
        public string item { get; set; }
        public int total { get; set; }
        public int id_ubicacion { get; set; }
        public int id_sucursal { get; set; }
        public string ubicacion { get; set; }
        public string sucursal { get; set; }
        public string pedido { get; set; }
        public string tipo { get; set; }


    }
    public class Estilos_trims
    {
        public int id_estilo { get; set; }
        public int id_summary { get; set; }
        public string estilo { get; set; }
        public string descripcion { get; set; }
        public List<Trim_requests> lista_trims { get; set; }
    }

    public class Trim_orden {
        public int id_trim_order { get; set; }
        public int id_request { get; set; }
        public int id_item { get; set; }
        public string item { get; set; }
        public int id_pedido { get; set; }
        public string pedido { get; set; }
        public int id_summary { get; set; }
        public string estilo { get; set; }
        public int id_talla { get; set; }
        public string talla { get; set; }
        public int id_sucursal { get; set; }
        public string sucursal { get; set; }
        public int id_locacion { get; set; }
        public string locacion { get; set; }
        public int id_usuario { get; set; }
        public string usuario { get; set; }
        public int total { get; set; }
        public int anterior { get; set; }
        public int restante { get; set; }
        public string fecha { get; set; }
        public string fecha_completa { get; set; }
        public string po { get; set; }
        public string familia { get; set; }

    }

    public class Sucursal_trims
    {
        public int id_sucursal { get; set; }
        public string sucursal { get; set; }
    }






















}
