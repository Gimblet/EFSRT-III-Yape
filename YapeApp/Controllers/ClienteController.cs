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
using QuestPDF.Previewer;
using System.Web.WebPages;

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
                    yape.Fecha = dr.GetString(4);
                };
                lista.Add(yape);
            };
            cnx.Close();
            dr.Close();
            return lista;
        }

        public string cerrarSesion()
        {
            int id = obtenerId();
            string respuesta = "Sesion Invalida";
            if (id != -1)
            {
                SqlCommand cmd = new SqlCommand("SP_EliminarSesion", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);
                cnx.Open();
                int value = cmd.ExecuteNonQuery();
                if (value == 1)
                {
                    Session.Clear();
                    respuesta = "Sesion Cerrada Correctamente";
                    return respuesta;
                }
                else
                {
                    Session.Clear();
                    return respuesta;
                }
            }
            return respuesta;
        }

        private int obtenerId()
        {
            int id = -1;
            if (Session["Numero"] != null)
            {
                SqlCommand cmd = new SqlCommand("Sp_ObtenerIDClientexNumero", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@numero", Session["Numero"]);
                cnx.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    id = dr.GetInt32(0);
                }
                cnx.Close();
                dr.Close();
            }
            return id;
        }

        private List<Yape> filtrarYapesXFecha(string fecha)
        {
            List<Yape> lista = new List<Yape>();
            SqlCommand cmd = new SqlCommand("SP_BuscarYapeXFecha", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", Session["Numero"]);
            cmd.Parameters.AddWithValue("@fecha", fecha);
            cnx.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Yape yape = new Yape();
                {
                    yape.IDE_YAP = dr.GetInt32(0);
                    yape.NRC_YAP = dr.GetString(1);
                    yape.NRZ_YAP = dr.GetString(2);
                    yape.MON_YAP = dr.GetDouble(3);
                    yape.Fecha = dr.GetString(4);
                };
                lista.Add(yape);
            };
            dr.Close();
            cnx.Close();
            return lista;
        }

        private string Yapear(Yape yape)
        {
            string mensaje = string.Empty;
            try
            {
                SqlCommand cmd = new SqlCommand("Sp_RealizarYapeo", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@numeroRecibiente", yape.NRC_YAP);
                cmd.Parameters.AddWithValue("@numeroRealizante", Session["Numero"]);
                cmd.Parameters.AddWithValue("@monto", yape.MON_YAP);
                cnx.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    mensaje = dr[0].ToString();
                }

            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
            }
            finally
            {
                cnx.Close();
            }
            return mensaje;
        }

        private double actualizarSaldo()
        {
            double saldo = 0.0;
            SqlCommand cmd = new SqlCommand("Sp_ObtenerSaldo", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", Session["Numero"]);
            cnx.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                saldo = rd.GetDouble(0);
            }
            cnx.Close();
            rd.Close();
            return saldo;
        }

        public Detalles ObtenerDetallesYape(int id)
        {
            Detalles detalle = null;
            try
            {
                SqlCommand cmd = new SqlCommand("SP_ObtenerDetallesYape", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);
                cnx.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    detalle = new Detalles();
                    detalle.IDE_YAP = dr.GetInt32(0);
                    detalle.NRC_YAP = dr.GetString(1);
                    detalle.NOM_REC = dr.GetString(2);
                    detalle.NRZ_YAP = dr.GetString(3);
                    detalle.NOM_REA = dr.GetString(4);
                    detalle.MON_YAP = dr.GetDouble(5);
                    detalle.Fecha = dr.GetString(6);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cnx.Close();
            }
            return detalle;
        }

        // GET: Cliente
        public ActionResult Index(string mensaje)
        {
            if (!mensaje.IsEmpty())
            {
                ViewBag.mensaje = mensaje;
            }
            return View(listarYapes());
        }

        public ActionResult Details(int id)
        {
            Detalles detalle = ObtenerDetallesYape(id);
            return View(detalle);
        }

        public ActionResult ActionCerrarSesion()
        {
            string mensaje = cerrarSesion();
            if (mensaje.Equals("Sesion Invalida"))
            {
                mensaje = "Ocurrió un problema, vuelva a iniciar Sesión";
            }
            ViewBag.mensaje = mensaje;
            return View("~/Views/Login/ActionLogin.cshtml");
        }
        public ActionResult ActionFiltrarYapesXFecha(string fecha)
        {
            return View(filtrarYapesXFecha(fecha));
        }

        public ActionResult realizarYapeo(string mensaje)
        {
            if (!mensaje.IsEmpty() && mensaje.Contains("Error"))
            {
                ViewBag.error = mensaje;
                return View();
            }
            ViewBag.mensaje = mensaje;
            return View();
        }

        [HttpPost]
        public ActionResult ActionYapear(Yape datos)
        {
            string mensaje = Yapear(datos);
            if (mensaje.Equals("Ocurrió un problema, vuelva a iniciar sesión"))
            {
                return RedirectToAction("ActionCerrarSesion");
            }
            else if (mensaje.Contains("Error"))
            {
                return RedirectToAction("RealizarYapeo", new { mensaje = mensaje });
            }
            Session["Saldo"] = actualizarSaldo();
            return RedirectToAction("Index", new { mensaje = mensaje });
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