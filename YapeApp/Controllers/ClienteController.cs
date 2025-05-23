﻿using System;
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
using System.IO;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Web.UI;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace YapeApp.Controllers
{
    public class ClienteController : Controller
    {
        public static string Cadena = ConfigurationManager.ConnectionStrings["Cadena"].ConnectionString;
        SqlConnection cnx = new SqlConnection(Cadena);

        public List<Yape> listarYapes()
        {
            List<Yape> lista = new List<Yape>();
            if (sesionActiva())
            {
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
            }
            else
            {
                ActionCerrarSesion();
            }
            return lista;
        }

        public bool sesionActiva()
        {
            if (Session["Numero"] != null)
            {
                return true;
            }
            return false;
        }

        public string cerrarSesion()
        {
            int id = obtenerId();
            string respuesta = "Error >> Sesion Invalida";
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
            if (sesionActiva())
            {
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
            }
            else
            {
                ActionCerrarSesion();
            }
            return lista;
        }

        private string Yapear(Yape yape)
        {
            string mensaje = string.Empty;
            if (sesionActiva())
            {
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
            }
            else
            {
                ActionCerrarSesion();
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
            if (sesionActiva())
            {
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
            }
            else
            {
                ActionCerrarSesion();
            }
            return detalle;
        }
        double obtenerMontoTotalRecibido(List<Yape> lista)
        {
            double resultado = 0.0;
            for (int i = 0; i < lista.Count(); i++)
            {
                if (lista[i].NRC_YAP.Equals(Session["Numero"]))
                {
                    resultado += lista[i].MON_YAP;
                }
            }
            return resultado;
        }
        double obtenerMontoTotalDepositado(List<Yape> lista)
        {
            double resultado = 0.0;
            for (int i = 0; i < lista.Count(); i++)
            {
                if (lista[i].NRZ_YAP.Equals(Session["Numero"]))
                {
                    resultado += lista[i].MON_YAP;
                }
            }
            return resultado;
        }

        // GET: Cliente
        public ActionResult Index(string mensaje)
        {
            if (sesionActiva())
            {
                List<Yape> lista = listarYapes();
                if(lista.Count() == 0)
                {
                    ViewBag.aviso = "Realize o Reciba por lo menos un Yape para comenzar a ver su historial";
                }
                if (!mensaje.IsEmpty())
                {
                    ViewBag.mensaje = mensaje;
                }
                return View(listarYapes());
            }
            else
            {
                return ActionCerrarSesion();
            }
        }

        public ActionResult Details(int id)
        {
            Detalles detalle = ObtenerDetallesYape(id);
            return View(detalle);
        }

        public ActionResult ActionCerrarSesion()
        {
            string mensaje = cerrarSesion();
            if (mensaje.Equals("Error >> Sesion Invalida"))
            {
                mensaje = "Ocurrió un problema, vuelva a iniciar Sesión";
                ViewBag.mensaje = mensaje;
            }
            else
            {
                ViewBag.MensajeBueno = mensaje;
            }
            return View("~/Views/Login/ActionLogin.cshtml");
        }

        public ActionResult ActionFiltrarYapesXFecha(string fecha)
        {
            if (sesionActiva())
            {
                Session["fecha"] = fecha;
                List<Yape> listarYapes = filtrarYapesXFecha(fecha);
                if (listarYapes.Count() == 0)
                {
                    ViewBag.error = "No se encontraron yapes, verifique la fecha seleccionada";
                }
                return View(listarYapes);
            }
            else
            {
                return ActionCerrarSesion();
            }
        }

        public ActionResult realizarYapeo(string mensaje)
        {
            if (sesionActiva())
            {
                if (!mensaje.IsEmpty() && mensaje.Contains("Error"))
                {
                    ViewBag.error = mensaje;
                    return View();
                }
                ViewBag.mensaje = mensaje;
                return View();
            }
            else
            {
                return ActionCerrarSesion();
            }
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
        public ActionResult ActionPDFxFecha()
        {
            if (sesionActiva())
            {
                List<Yape> lista = filtrarYapesXFecha(Session["fecha"].ToString());
                var placeholder = GenerarPDF(lista);
                var pdf = placeholder.GeneratePdf();
                return File(pdf, "application/pdf", "ReporteYapes.pdf");
            }
            else
            {
                return ActionCerrarSesion();
            }
        }

        [HttpGet]
        public ActionResult ActionPDF(string fecha)
        {
            if (sesionActiva())
            {
                List<Yape> lista = listarYapes();
                var placeholder = GenerarPDF(lista);
                var pdf = placeholder.GeneratePdf();
                return File(pdf, "application/pdf", "ReporteYapes.pdf");
            }
            else
            {
                return ActionCerrarSesion();
            }
        }

        IDocument GenerarPDF(List<Yape> lista)
        {
            var stream = new FileStream(Server.MapPath("~/Content/Images/LogoYape.png"), FileMode.Open);
            double montoTotalRecibido = obtenerMontoTotalRecibido(lista);
            double montoTotalDepositado = obtenerMontoTotalDepositado(lista);
            montoTotalRecibido = Math.Round(montoTotalRecibido, 2);
            montoTotalDepositado = Math.Round(montoTotalDepositado, 2);
            double montoTotal = montoTotalRecibido - montoTotalDepositado;
            montoTotal = Math.Round(montoTotal, 2);

            return Document.Create(contenedor =>
            {
                contenedor.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4);
                    pagina.Margin(15, QuestPDF.Infrastructure.Unit.Millimetre);
                    pagina.PageColor(QuestPDF.Helpers.Colors.White);
                    pagina.DefaultTextStyle(x => x.FontSize(20));

                    pagina.Header().PaddingRight(25).PaddingLeft(25).ShowOnce().Row(fila =>
                    {
                        fila.ConstantItem(55).Image(stream).FitArea();
                        stream.Close();

                        fila.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("RUC : 100059474831").Bold().AlignCenter().FontSize(10);
                            col.Item().Text("Central Telefonica : (01)-049348").Bold().AlignCenter().FontSize(10);
                            col.Item().Text("E-Mail : atencionalcliente@yape.pe").Bold().AlignCenter().FontSize(10);
                        });
                    });

                    pagina.Content().PaddingVertical(35).PaddingLeft(30).PaddingRight(30).Column(col =>
                    {
                        col.Item().Background("#f6e1f9").BorderBottom(1).BorderColor("#f6e1f9").PaddingHorizontal(10).PaddingTop(6).AlignLeft().Row(fila =>
                        {
                            fila.ConstantItem(80).Text("Detalle").Bold().FontColor("#742384").FontSize(10).AlignStart();
                            fila.RelativeItem().AlignRight().Text(texto =>
                            {
                                texto.Span("N° de Recibo : ").Bold().FontSize(8);
                                texto.Span(new Random().Next(1, 1000000000).ToString()).Bold().FontSize(8);
                            });
                        });

                        col.Item().Background("#f6e1f9").BorderBottom(1).BorderColor("#f6e1f9").PaddingHorizontal(10).PaddingBottom(3).AlignRight().Text(texto =>
                        {
                            texto.Span("Fecha de Emisión : ").Bold().FontSize(8);
                            texto.Span(DateTime.Now.ToShortDateString()).Bold().FontSize(8);
                        });

                        col.Item().Background("#f6e1f9").BorderBottom(1).BorderColor("#f6e1f9").PaddingHorizontal(10).PaddingBottom(3).AlignRight().Text(texto =>
                        {
                            texto.Span("Teléfono del Cliente : ").Bold().FontSize(8);
                            texto.Span(Session["Numero"].ToString()).Bold().FontSize(8);
                        });

                        col.Item().Background("#f6e1f9").PaddingHorizontal(10).PaddingBottom(6).AlignRight().Text(texto =>
                        {
                            texto.Span("Cliente : ").Bold().FontSize(8);
                            texto.Span(Session["NombreCompleto"].ToString()).Bold().FontSize(8);
                        });

                        col.Item().PaddingVertical(11).Text("DETALLE DE YAPES").Bold().FontSize(12).AlignStart();

                        col.Item().BorderRight(1).BorderLeft(1).BorderBottom(1).BorderColor("#e0e0e0").Table(tabla =>
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
                                fila.Cell().Background("#742384").Padding(4).Text("Recibidor").FontColor("#ffffff").Bold().FontSize(10).AlignCenter();
                                fila.Cell().Background("#742384").Padding(4).Text("Realizador").FontColor("#ffffff").Bold().FontSize(10).AlignCenter();
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
                                    tabla.Cell().Row(e).Column(4).Background("#f0f0f0").Padding(5).Text(Math.Round(yape.MON_YAP, 2).ToString()).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(5).Background("#f0f0f0").Padding(5).Text(yape.Fecha).FontSize(10).AlignCenter();
                                }
                                else
                                {
                                    tabla.Cell().Row(e).Column(1).Padding(5).Text(yape.IDE_YAP.ToString()).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(2).Padding(5).Text(yape.NRC_YAP).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(3).Padding(5).Text(yape.NRZ_YAP).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(4).Padding(5).Text(Math.Round(yape.MON_YAP, 2).ToString()).FontSize(10).AlignCenter();
                                    tabla.Cell().Row(e).Column(5).Padding(5).Text(yape.Fecha).FontSize(10).AlignCenter();
                                }
                            }
                        });

                        col.Item().PaddingBottom(15);

                        col.Item().Background("#f0f0f0").PaddingTop(8).PaddingHorizontal(15).Row(fila =>
                        {
                            fila.ConstantItem(110).Text("Resumen").FontSize(10).Bold().AlignStart();
                            fila.RelativeItem().AlignRight().Text(texto =>
                            {
                                texto.Span("Total de Yapes : ").FontSize(10);
                                texto.Span(lista.Count().ToString()).FontSize(10);
                            });
                        });

                        col.Item().Background("#f0f0f0").AlignRight().PaddingHorizontal(15).Column(interior =>
                        {
                            interior.Item().Text(textoG =>
                            {
                                textoG.Span("Monto Total Recibido : S/ ").FontSize(10);
                                textoG.Span(montoTotalRecibido.ToString()).FontSize(10);
                            });
                        });

                        col.Item().Background("#f0f0f0").AlignRight().PaddingBottom(8).PaddingHorizontal(15).Column(interior =>
                        {
                            interior.Item().Text(textoG =>
                            {
                                textoG.Span("Monto Total Depositado : S/ ").FontSize(10);
                                textoG.Span(montoTotalDepositado.ToString()).FontSize(10);
                            });
                        });

                        col.Item().PaddingVertical(4);

                        col.Item().Background("#742386").PaddingVertical(1).AlignRight().PaddingHorizontal(15).Column(subTotal =>
                        {
                            subTotal.Item().Text(textoG =>
                            {
                                textoG.Span("SUB TOTAL : S/ ").FontSize(12).FontColor("#ffffff").Bold();
                                textoG.Span(montoTotal.ToString()).FontSize(12).FontColor("#ffffff").Bold();
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

        public string[] filtrarMeses(List<Yape> listaYapes)
        {
            string[] listaReferencia = new string[12] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            List<string> lista = new List<string>();

            for (int i = 0; i < 12; i++)
            {
                if (listaYapes.Any(a => a.Fecha.Contains(listaReferencia[i])))
                {
                    lista.Add(listaReferencia[i]);
                }
            }

            return lista.ToArray();
        }

        public string[] filtrarYears(List<Yape> listaYapes)
        {
            List<string> years = new List<string>();

            SqlCommand cmd = new SqlCommand("SP_ObtenerAnosYapeados", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", Session["Numero"]);
            cnx.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                years.Add(dr.GetInt32(0).ToString());
            }
            cnx.Close();
            dr.Close();

            return years.ToArray();
        }

        [HttpGet]
        public ActionResult ActionExportarExcel()
        {
            if (!sesionActiva())
            {
                return ActionCerrarSesion();
            }
            List<Yape> listaYapes = listarYapes();
            string[] listaMeses = filtrarMeses(listaYapes);
            string[] listaYears = filtrarYears(listaYapes);
            return ExportarExcel(listaYapes, listaMeses, listaYears);
        }

        public ActionResult ExportarExcel(List<Yape> lista, string[] meses, string[] years)
        {
            XLWorkbook xls = new XLWorkbook();

            for (int y = 0; years.Length > y; y++)
            {
                for (int i = 0; meses.Length > i; i++)
                {
                    double totalYapeado = 0.0, totalRecibido = 0.0;
                    bool hayDatos = false;

                    DataTable dataTable = new DataTable();
                    dataTable.TableName = meses[i];
                    dataTable.Clear();

                    dataTable.Columns.Add("ID");
                    dataTable.Columns.Add("Numero Recibidor");
                    dataTable.Columns.Add("Numero Realizador");
                    dataTable.Columns.Add("Monto");
                    dataTable.Columns.Add("Fecha");

                    for (int a = 0; lista.Count() > a; a++)
                    {
                        if (lista[a].Fecha.Contains(meses[i]) && lista[a].Fecha.Contains(years[y]))
                        {
                            DataRow dr = dataTable.NewRow();
                            dr["ID"] = lista[a].IDE_YAP;
                            dr["Numero Recibidor"] = int.Parse(lista[a].NRC_YAP);
                            dr["Numero Realizador"] = int.Parse(lista[a].NRZ_YAP);
                            dr["Monto"] = lista[a].MON_YAP;
                            dr["Fecha"] = lista[a].Fecha;
                            dataTable.Rows.Add(dr);

                            if (lista[a].NRC_YAP.Equals(Session["Numero"])) totalRecibido += lista[a].MON_YAP;
                            if (lista[a].NRZ_YAP.Equals(Session["Numero"])) totalYapeado += lista[a].MON_YAP;

                            hayDatos = true;
                        }
                    }

                    if (hayDatos)
                    {
                        var ws = xls.AddWorksheet();

                        ws.Name = meses[i] + " - " + years[y];
                        ws.ColumnWidth = 36;
                        ws.Cell("A2").Style.Font.SetFontSize(22);
                        ws.Cell("A2").Style.Font.SetBold(true);
                        ws.Cell("A2").Style.Font.SetItalic(true);
                        ws.Cell("A2").Style.Font.SetUnderline(XLFontUnderlineValues.Single);
                        ws.Cell("A2").Value = "Reporte de " + meses[i];

                        ws.Cell("A4").Value = "TOTALES";
                        ws.Cell("A4").Style.Fill.SetBackgroundColor(XLColor.Gray);
                        ws.Cell("A4").Style.Font.SetBold(true);
                        ws.Cell("A5").Value = "Total Yapeado : S/" + totalYapeado;
                        ws.Cell("A5").Style.Fill.SetBackgroundColor(XLColor.DarkGray);
                        ws.Cell("A6").Value = "Total Recibido : S/" + totalRecibido;
                        ws.Cell("A6").Style.Fill.SetBackgroundColor(XLColor.DarkGray);

                        var tabla = ws.Cell("B4").InsertTable(dataTable);

                        tabla.Theme = XLTableTheme.TableStyleMedium5;
                        tabla.SetShowHeaderRow(true);
                        tabla.SetShowAutoFilter(false);
                    }
                }
            }

            using (MemoryStream st = new MemoryStream())
            {
                xls.SaveAs(st);
                return File(st.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
        }
    }
}