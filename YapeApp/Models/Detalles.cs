using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace YapeApp.Models
{
    public class Detalles
    {
        [Display(Name = "ID", Order = 0)]
        public int IDE_YAP {  get; set; }

        [Required]
        [Display(Name = "Numero Recibidor", Order = 1)]
        public string NRC_YAP { get; set; }

        [Required]
        [Display(Name ="Nombre Recibidor",  Order = 2)]
        public string NOM_REC { get; set; }

        [Required]
        [Display(Name = "Numero Realizador", Order = 3)]
        public string NRZ_YAP { get; set; }

        [Required]
        [Display(Name ="Nombre Realizador",  Order = 4)]
        public string NOM_REA { get; set; }

        [Required]
        [Display(Name ="Monto", Order = 5)]
        public double MON_YAP { get; set; }

        [Required]
        [Display(Name = "Fecha", Order = 6)]
        public string Fecha { get; set; }
    }
}