using FortuneSystem.Models.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.Packing
{
    public class PackingTypeSize
    {
        public int IdPackingTypeSize { get; set; }
        public int IdTalla { get; set; }
        [Display(Name = "PIECES ")]
        public int Pieces { get; set; }
        public int Ratio { get; set; }
        public int IdSummary { get; set; }
        public int IdTipoEmpaque { get; set; }
        public string NombreTipoPak { get; set; }
        public string Talla { get; set; }
        public string TallasGrl { get; set; }
        public string Ratios { get; set; }
        [Display(Name = "TYPE OF PACKAGING ")]
        public TipoEmpaque TipoEmpaque { get; set; }
        [Display(Name = "TYPE OF PACKAGING ")]
        public TipoEmpaque TipoEmpaqueBP { get; set; }
        [Display(Name = "TYPE OF PACKAGING ")]
        public TipoEmpaquePPKs TipoEmpaquePPK { get; set; }
        [Display(Name = "TYPE OF PACKAGING ")]
        public TipoEmpaqueBulks TipoEmpaqueBulk { get; set; }
        [Display(Name = "PACKAGING FORM")]
        public FormaEmpaque FormaEmpaque { get; set; }
        public int IdFormaEmpaque { get; set; }
        [Display(Name = "PO#")]
        public int NumberPO { get; set; }
        [Display(Name = "QTY")]
        public int Cantidad { get; set; }
        [Display(Name = "CARTONS")]
        public int Cartones { get; set; }
        public int PartialNumber { get; set; }
        public int TotalRatio { get; set; }
        public int TotalBulk { get; set; }
        [Display(Name = "TOTAL PIECES")]
        public int TotalPieces { get; set; }
        [Display(Name = "TOTAL UNITS")]
        public int TotalUnits { get; set; }
        [Display(Name = "TOTAL CARTONS")]
        public int TotalCartones { get; set; }
        public virtual PackingM PackingM { get; set; }
        public List<PackingTypeSize> ListaEmpaque { get; set; }
        [Display(Name = "PACKING NAME")]
        public string PackingName { get; set; }
        [Display(Name = "NAME PACKING")]
        public string PackingNameBulk { get; set; }
        [Display(Name = "ASSORT NAME")]
        public string AssortName { get; set; }
        [Display(Name = "PACKING")]
        public string PackingRegistrado { get; set; }
        [Display(Name = "PACKING")]
        public string PackingRegistradoAssort { get; set; }        
        [Display(Name = "PACKING")]
        public string PackingRegistradoPPK { get; set; }
        [Display(Name = "PACKING")]
        public string PackingRegistradoVariosPPK { get; set; }
        [Display(Name = "PACKING")]
        public string PackingRegistradoVariosBULKS { get; set; }
        [Display(Name = "STYLES")]
        public string PackingRegistradoEstilosAssort { get; set; }
        public int IdBlockPack { get; set; }
        public virtual ItemDescripcion ItemDescripcion { get; set; }
        public int NumRegistros { get; set; }
        public int TotalPiezas { get; set; }
        public int SumaTotal { get; set; }
        [Display(Name = "TOTAL UNITS")]
        public int TotalUnitsPPKHT { get; set; }
        [Display(Name = "TOTAL UNITS")]
        public int TotalUnitsPPKActHT { get; set; }
        [Display(Name = "NO. PIECES")]
        public int NumberPKK { get; set; }
        [Display(Name = "NAME PACKING")]
        public string NombrePacking { get; set; }
        [Display(Name = "NO. PIECES")]
        public int NumberPPKs { get; set; }
        [Display(Name = "NO. PIECES")]
        public int NumberAddPPKs { get; set; }
        [Display(Name = "NAME PACKING")]
        public string NombrePackingPPKs { get; set; }
        [Display(Name = "NAME PACKING")]
        public string NombrePackingAddPPKs { get; set; }
        [Display(Name = "NAME PACKING")]
        public string NombrePackingAddBulks { get; set; }
        [Display(Name = "NAME PACKING")]
        public string NombrePackingBulks { get; set; }
        public List<PackingTypeSize> ListaPackingName { get; set; }
        public int TotalCajas { get; set; }
        public int NumUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public int PiecesEstilo { get; set; }
        public int NumTotalPiezasEstilo { get; set; }
        public int NumTotalCartonesEstilo { get; set; }
        [Display(Name = "REMAINING CARTONS")]
        public int TotalCartonesTerminados { get; set; }
        [Display(Name = "REMAINING PIECES")]
        public int TotalPiezasTerminadas { get; set; }
    }

    public enum TipoEmpaque
    {
        BULK = 1,
        PPK = 2,
        PPKS = 4,
        BULKS = 5
        //ASSORTMENT = 3
    }

    public enum TipoEmpaquePPKs
    {
        PPKS = 4
    }

    public enum TipoEmpaqueBulks
    {
        BULKS = 5
    }

    public enum FormaEmpaque
    {
        STORE = 1,
        ECOM = 2,
        RETAIL = 4,
        DIRECT = 5,
        [Display(Name = "RETAIL-GAP")]
        RETAIL_GAP = 6,
        [Display(Name = "RETAIL-ECHO")]
        RETAIL_ECHO = 7,
        [Display(Name = "CANADA-STORE")]
        CANADA_STORE = 8,
        BACKSTOCK = 9,
        [Display(Name = "N/A")]
        NO_APLICA = 10
        //ASSORTMENT = 3
    }
}