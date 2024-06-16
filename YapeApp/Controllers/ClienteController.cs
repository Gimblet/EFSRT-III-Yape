using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.Sql;
using System.Data;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System.Data.SqlClient;
using YapeApp.Models;

namespace YapeApp.Controllers
{
    public class ClienteController : Controller
    {
        public static string Cadena = ConfigurationManager.ConnectionStrings["Cadena"].ConnectionString;
        SqlConnection cnx = new SqlConnection(Cadena);

        public List<Yape> listarYapes()
        {
            List<Yape> lista = new List<Yape>();
            SqlCommand cmd = new SqlCommand("SP_ListarYapes", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", Session["Numero"]);
            cnx.Open();
            IDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Yape yape = new Yape();
                {
                    yape.IDE_YAP = dr.GetInt32(0);
                    yape.NRC_YAP = dr.GetString(1);
                    yape.NRZ_YAP = dr.GetString(2);
                    yape.MON_YAP = dr.GetDouble(3);
                    yape.FEC_YAP = dr.GetDateTime(4);
                };
                lista.Add(yape);
            };
            cnx.Close();
            dr.Close();
            return lista;
        }

        // GET: Cliente
        public ActionResult Index()
        {
            return View(listarYapes());
        }

        [HttpGet]
        public ActionResult ActionPDF()
        {
            List<Yape> lista = listarYapes();
            var placeholder = GenerarPDF(lista);
            var pdf = placeholder.GeneratePdf();
            return File(pdf, "application/pdf", "ReporteYapes.pdf");
        }

        IDocument GenerarPDF(List<Yape> lista)
        {
            return Document.Create(contenedor =>
            {
                contenedor.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4);
                    pagina.Margin(2, Unit.Centimetre);
                    pagina.PageColor(Colors.White);
                    pagina.DefaultTextStyle(x => x.FontSize(20));

                    pagina.Header().Text("Reporte de Yapes").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                });
            });
        }
    }
}