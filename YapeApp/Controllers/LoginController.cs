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
                // Si verificarSesionAnterior es igual a 1, significa que ya habia un usuario logeado con las credenciales dadas
                if (verificarSesionAnterior(usuarioValido) == 1) { eliminarSesion(usuarioValido); }
                SqlCommand cmd = new SqlCommand("SP_AgregarNuevaSesion", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", usuarioValido.IDE_CLI);
                cmd.Parameters.AddWithValue("@numero", usuarioValido.NUM_CLI);
                cnx.Open();
                cmd.ExecuteNonQuery();
                cnx.Close();

                AlmacenarDatos(usuarioValido);

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
        public int verificarSesionAnterior(Cliente id)
        {
            int resultado = 0;
            SqlCommand cmd = new SqlCommand("SP_BuscarSesion", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", id.IDE_CLI);
            cnx.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                resultado = 1;
            }
            rd.Close();
            cnx.Close();
            return resultado;
        }

        public void eliminarSesion(Cliente id)
        {
            SqlCommand cmd = new SqlCommand("SP_EliminarSesion", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", id.IDE_CLI);
            cnx.Open();
            cmd.ExecuteNonQuery();
            cnx.Close();
        }

        #region Almacenamiento de Datos

        public void AlmacenarDatos(Cliente datosUsuario)
        {
            string llave = datosUsuario.NUM_CLI;

            Session["Numero"] = datosUsuario.NUM_CLI;
            Session["Nombre"] = obtenerNombre(llave);
            Session["Apellido"] = obtenerApellido(llave);
            Session["NombreCompleto"] = obtenerNombreCompleto(llave);
            Session["Saldo"] = obtenerSaldo(llave);
        }

        private string obtenerNombre(string llave)
        {
            string nombre = string.Empty;
            return "";
        }

        private string obtenerApellido(string llave)
        {
            return "";
        }
        private string obtenerNombreCompleto(string llave)
        {
            return "";
        }
        private double obtenerSaldo(string llave)
        {
            return 0.0;
        }
        #endregion

        // GET: Login
        public ActionResult ActionLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ActionLogin(Cliente datosRecibidos)
        {
            string respuesta = Login(datosRecibidos);
            if (respuesta.Equals("Valido"))
            {
                return RedirectToAction("Index", "Cliente");
            }
            else
            {
                ViewBag.UsuarioInvalido = respuesta;
                return View();
            }
        }
    }
}