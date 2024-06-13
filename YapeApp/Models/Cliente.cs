using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace YapeApp.Models
{
    public class Cliente
    {
        [Required]
        [Display(Name = "ID", Order = 0)]
        public string IDE_CLI {  get; set; }

        [Required]
        [Display(Name = "DNI", Order = 1)]
        public string DNI_CLI { get; set; }

        [Required]
        [Display(Name ="Nombres",  Order = 2)]
        public string NOM_CLI { get; set; }

        [Required]
        [Display(Name ="Apellidos", Order = 3)]
        public string APE_CLI { get; set; }

        [Required]
        [Phone]
        [Display(Name ="Telefono", Order = 4)]
        public string NUM_CLI { get; set; }

        [Required]
        [Display(Name = "Saldo", Order = 5)]
        public double SAL_CLI { get; set; }

        [Required]
        [Display(Name = "Clave", Order = 6)]
        public string CLA_CLI { get; set; }

        public Cliente()
        {
            IDE_CLI = "";
            DNI_CLI = "";
            NOM_CLI = "";
            APE_CLI = "";
            NUM_CLI = "";
            SAL_CLI = 0;
            CLA_CLI = "";
        }
    }
}