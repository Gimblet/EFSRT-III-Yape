using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace YapeApp.Models
{
    public class Yape
    {
        [Required]
        [Display(Name = "ID", Order = 0)]
        public int IDE_YAP {  get; set; }

        [Required]
        [Display(Name = "Numero Recibidor", Order = 1)]
        public string NRC_YAP { get; set; }

        [Required]
        [Display(Name ="Numero Realizador",  Order = 2)]
        public string NRZ_YAP { get; set; }

        [Required]
        [Display(Name ="Monto", Order = 3)]
        public double MON_YAP { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha", Order = 4)]
        public DateTime FEC_YAP { get; set; }
    }
}