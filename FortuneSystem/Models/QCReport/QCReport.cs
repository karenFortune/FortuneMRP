using FortuneSystem.Models.Item;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.QCReport
{
    public class QCReportGeneral
    {
        public int IdQCReport { get; set; }
        [Display(Name = "GENERAL REPORT")]
        public string ReporteG { get; set; }
        [Display(Name = "GENERAL REPORT")]
        public string ReporteG2 { get; set; }
        [Display(Name = "SACADOR")]
        public string Sacador { get; set; }
        [Display(Name = "SACADOR")]
        public string Sacador2 { get; set; }
        [Display(Name = "CACHADOR")]
        public string Cachador { get; set; }
        [Display(Name = "CACHADOR")]
        public string Cachador2 { get; set; }
        [Display(Name = "METEDOR")]
        public string Metedor { get; set; }
        [Display(Name = "METEDOR")]
        public string Metedor2 { get; set; }
        [Display(Name = "QC INSPECTOR")]
        public string QCInspector { get; set; }
        [Display(Name = "QC INSPECTOR")]
        public string QCInspector2 { get; set; }
        [Display(Name = "AQL RESULTS")]
        public bool AQL { get; set; }
        public string DatoAQL { get; set; }
        [Display(Name = "SHIFT")]
        public Turno Turnos { get; set; }
        [Display(Name = "SHIFT")]
        public Turno TurnosQC { get; set; }
        public int Turno { get; set; }
        [Display(Name = "DATE")]
        public DateTime Fecha { get; set; }
        [Display(Name = "DATE")]
        public DateTime Fecha2 { get; set; }
        [Display(Name = "DATE")]
        public DateTime FechaRM { get; set; }
        public string Mes { get; set; }
        public int IdUsuario { get; set; }
        public int IdMaquina { get; set; }
        public int IdSummary { get; set; }
        [Display(Name = "TURN")]
        public string DescTurno { get; set; }
        [Display(Name = "MACHINE")]
        public Maquina Maquinas { get; set; }
        public virtual QCPruebaLavado QCPruebaLavado { get; set; }
        public List<ItemTalla> ListaTallas { get; set; }

    }


    public enum Turno
    {
        First = 1,
        Second = 2
    }

    public enum Maquina
    {
        Machine1 = 1,
        Machine2 = 2,
        Machine3 = 3,
        Machine4 = 4,
        Machine5 = 5,
        Machine6 = 6,
        Machine7 = 7,
        Machine8 = 8,
        Machine9 = 9,
        Machine10 = 10,
        Machine11 = 11,
        Machine12 = 12
    }
}