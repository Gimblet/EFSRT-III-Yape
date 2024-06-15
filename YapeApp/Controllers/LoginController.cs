using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.Sql;
using System.Data;
using YapeApp.Models;
using System.Data.SqlClient;

namespace YapeApp.Controllers
{
    public class LoginController : Controller
    {
        public static string Cadena = ConfigurationManager.ConnectionStrings["Cadena"].ConnectionString;
        SqlConnection cnx = new SqlConnection(Cadena);

        public string Login(Cliente datosRecibidos)
        {
            Cliente usuarioValido = validarLogin(datosRecibidos);
            if (usuarioValido != null)
            {
                SqlCommand cmd = new SqlCommand("SP_AgregarNuevaSesion", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", usuarioValido.IDE_CLI);
                cmd.Parameters.AddWithValue("@numero", usuarioValido.NUM_CLI);
                cnx.Open();
                cmd.ExecuteNonQuery();
                cnx.Close();
                return "Valido";
            }
            else
            {
                return "Usuario o clave incorrecta";
            }
        }

        public Cliente validarLogin(Cliente aValidar)
        {
            Cliente cliente = null;
            SqlCommand cmd = new SqlCommand("SP_ValidarInicioSesion", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", aValidar.NUM_CLI);
            cmd.Parameters.AddWithValue("@clave", aValidar.CLA_CLI);
            cnx.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                cliente = new Cliente();
                {
                    cliente.IDE_CLI = dr.GetInt32(0);
                    cliente.NUM_CLI = dr.GetString(1);
                };
            }
            cnx.Close();
            dr.Close();
            return cliente;
        }

        // GET: Login
        public ActionResult ActionLogin()
        {
            return View();
        }

        [HttpPost]public ActionResult ActionLogin(Cliente datosRecibidos)
        {
            string respuesta = Login(datosRecibidos);
            if (respuesta.Equals("Valido"))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.UsuarioInvalido = respuesta;
                return View();
            }
        }
    }
}