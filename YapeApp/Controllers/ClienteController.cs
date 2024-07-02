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
using System.Web.Razor.Parser.SyntaxTree;

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
                    pagina.Margin(15, Unit.Millimetre);
                    pagina.PageColor(Colors.White);
                    pagina.DefaultTextStyle(x => x.FontSize(20));

                    pagina.Header().Row(fila =>
                    {
                        fila.ConstantItem(120).Border(1).Height(85).Placeholder();

                        fila.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("YAPE").AlignCenter().Bold().FontSize(20);
                            col.Item().Text("RUC : 100059474831").Bold().AlignCenter().FontSize(10);
                            col.Item().Text("Central Telefonica : (01)-049348").Bold().AlignStart().FontSize(10);
                            col.Item().Text("E-Mail : atencionalcliente@yape.pe").Bold().AlignStart().FontSize(10);
                        });
                    });

                    pagina.Content().PaddingVertical(35).PaddingLeft(25).PaddingRight(25).Column(col =>
                    {
                        col.Item().AlignLeft().Row(fila =>
                        {
                            fila.ConstantItem(80).Text("Detalle").Bold().FontSize(12).AlignStart();
                            fila.RelativeItem().AlignRight().Text(texto =>
                            {
                                texto.Span("Número de Recibo : ").Bold().FontSize(12);
                                texto.Span(new Random().Next(1, 1000000000).ToString()).Bold().FontSize(12);
                            });
                        });

                        col.Item().AlignRight().Height(20).Text(texto =>
                        {
                            texto.Span("Fecha de Emisión : ").Bold().FontSize(12);
                            texto.Span(DateTime.Now.ToShortDateString()).Bold().FontSize(12);
                        });

                        col.Item().PaddingTop(11).Text("DETALLE DE YAPES").Bold().FontSize(12).AlignCenter();

                        col.Item().PaddingTop(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(formato =>
                            {
                                formato.RelativeColumn();
                                formato.RelativeColumn(2);
                                formato.RelativeColumn(2);
                                formato.RelativeColumn(2);
                                formato.RelativeColumn(5);
                            });

                            tabla.Header(fila =>
                            {
                                fila.Cell().Background("#742384").Padding(4).Text("ID").FontColor("#ffffff").Bold().FontSize(10).AlignCenter();
                                fila.Cell().Background("#742384").Padding(4).Text("Número Recibidor").FontColor("#ffffff").Bold().FontSize(10).AlignCenter();
                                fila.Cell().Background("#742384").Padding(4).Text("Número Realizador").FontColor("#ffffff").Bold().FontSize(10).AlignCenter();
                                fila.Cell().Background("#742384").Padding(4).Text("Monto").FontColor("#ffffff").Bold().FontSize(10).AlignCenter();
                                fila.Cell().Background("#742384").Padding(4).Text("Fecha").FontColor("#ffffff").Bold().FontSize(10).AlignCenter();
                            });

                            for (int i = 0; i < lista.Count(); i++)
                            {
                                Yape yape = lista[i];
                                uint e = (uint)i + 1;
                                if (e % 2 == 0)
                                {
                                    tabla.Cell().Row(e).Column(1).Background("#f0f0f0").Padding(5).Text(yape.IDE_YAP.ToString()).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(2).Background("#f0f0f0").Padding(5).Text(yape.NRC_YAP).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(3).Background("#f0f0f0").Padding(5).Text(yape.NRZ_YAP).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(4).Background("#f0f0f0").Padding(5).Text(yape.MON_YAP.ToString()).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(5).Background("#f0f0f0").Padding(5).Text(yape.Fecha).FontSize(10).AlignCenter();
                                }
                                else
                                {
                                    tabla.Cell().Row(e).Column(1).Padding(5).Text(yape.IDE_YAP.ToString()).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(2).Padding(5).Text(yape.NRC_YAP).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(3).Padding(5).Text(yape.NRZ_YAP).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(4).Padding(5).Text(yape.MON_YAP.ToString()).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(5).Padding(5).Text(yape.Fecha).FontSize(10).AlignCenter();
                                }
                            }
                        });

                        col.Item().PaddingTop(15).PaddingHorizontal(15).Row(fila =>
                        {
                            fila.ConstantItem(110).Text("Resumen").FontSize(10).Bold().AlignStart();
                            fila.RelativeItem().AlignRight().Text(texto =>
                            {
                                texto.Span("Total de Yapes : ").FontSize(10);
                                texto.Span(lista.Count().ToString()).FontSize(10);
                            });
                        });

                        col.Item().AlignRight().PaddingHorizontal(15).Column(interior =>
                        {
                            interior.Item().Text(textoG =>
                            {
                                textoG.Span("Monto Total Recibido : S/ ").FontSize(10);
                                textoG.Span("").FontSize(10);
                            });
                        });

                        col.Item().AlignRight().PaddingHorizontal(15).Column(interior =>
                        {
                            interior.Item().Text(textoG =>
                            {
                                textoG.Span("Monto Total Realizado : S/ ").FontSize(10);
                                textoG.Span("").FontSize(10);
                            });
                        });

                        col.Item().PaddingTop(3).AlignRight().PaddingHorizontal(15).Column(subTotal =>
                        {
                            subTotal.Item().Text(textoG =>
                            {
                                textoG.Span("SUB TOTAL : S/ ").FontSize(12).Bold();
                                textoG.Span("").FontSize(12).Bold();
                            });
                        });

                    });

                    pagina.Footer().AlignRight().Text(texto =>
                    {
                        texto.Span("Página ").Bold().FontSize(9);
                        texto.CurrentPageNumber().FontSize(9).Bold();
                        texto.Span(" de ").Bold().FontSize(9).Bold();
                        texto.TotalPages().FontSize(9).Bold();
                    });
                });
            });
        }
    }
}