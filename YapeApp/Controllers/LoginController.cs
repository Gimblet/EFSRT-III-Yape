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
using System.Diagnostics;

namespace YapeApp.Controllers
{
    public class LoginController : Controller
    {
        public static string Cadena = ConfigurationManager.ConnectionStrings["Cadena"].ConnectionString;
        SqlConnection cnx = new SqlConnection(Cadena);

        public string registrarCliente(Cliente datos)
        {
            string mensaje = "";
            if (esDNIRegistrado(datos))
            {
                mensaje = "Error >> El DNI ya se encuentra registrado";
            }
            else if (esNumeroRegistrado(datos))
            {
                mensaje = "Error >> El numero ya se encuentra registrado";
            }
            else
            {
                SqlCommand cmd = new SqlCommand("SP_RegistrarCliente", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@dni", datos.DNI_CLI);
                cmd.Parameters.AddWithValue("@nombre", datos.NOM_CLI);
                cmd.Parameters.AddWithValue("@apellido", datos.APE_CLI);
                cmd.Parameters.AddWithValue("@numero", datos.NUM_CLI);
                cmd.Parameters.AddWithValue("@clave", datos.CLA_CLI);
                cnx.Open();
                cmd.ExecuteNonQuery();
                cnx.Close();

                mensaje = "Registrado exitosamente, ingrese sus credenciales";
            }
            return mensaje;
        }

        public string Login(Cliente datosRecibidos)
        {
            Cliente usuarioValido = validarLogin(datosRecibidos);
            if (usuarioValido != null)
            {
                // Si verificarSesionAnterior es igual a 1, significa que ya habia un usuario logeado con las credenciales dadas
                if (verificarSesionAnterior(usuarioValido) == 1) { eliminarSesion(usuarioValido); }

                agregarNuevaSesion(usuarioValido);
                AlmacenarDatos(datosRecibidos);

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
        public void agregarNuevaSesion(Cliente data)
        {
            SqlCommand cmd = new SqlCommand("SP_AgregarNuevaSesion", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", data.IDE_CLI);
            cmd.Parameters.AddWithValue("@numero", data.NUM_CLI);
            cnx.Open();
            cmd.ExecuteNonQuery();
            cnx.Close();
        }

        public Boolean esDNIRegistrado(Cliente datos)
        {
            Boolean respuesta = true;
            SqlCommand cmd = new SqlCommand("SP_BuscarDNI", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@dni", datos.DNI_CLI);
            cnx.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                respuesta = true;
            }
            else
            {
                respuesta = false;
            }
            rd.Close();
            cnx.Close();
            return respuesta;
        }

        public Boolean esNumeroRegistrado(Cliente datos)
        {
            Boolean respuesta = true;
            SqlCommand cmd = new SqlCommand("SP_BuscarNumero", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", datos.NUM_CLI);
            cnx.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                respuesta = true;
            }
            else
            {
                respuesta = false;
            }
            rd.Close();
            cnx.Close();
            return respuesta;
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
            SqlCommand cmd = new SqlCommand("Sp_ObtenerNombre", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", llave);
            cnx.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                nombre = rd.GetString(0);
            }
            cnx.Close();
            rd.Close();
            return nombre;
        }

        private string obtenerApellido(string llave)
        {
            string apellido = string.Empty;
            SqlCommand cmd = new SqlCommand("Sp_ObtenerApellido", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", llave);
            cnx.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                apellido = rd.GetString(0);
            }
            cnx.Close();
            rd.Close();
            return apellido;
        }

        private string obtenerNombreCompleto(string llave)
        {
            string nombreCompleto = string.Empty;
            SqlCommand cmd = new SqlCommand("Sp_ObtenerNombreCompleto", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", llave);
            cnx.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                nombreCompleto = rd.GetString(0);
            }
            cnx.Close();
            rd.Close();
            return nombreCompleto;
        }

        private double obtenerSaldo(string llave)
        {
            double saldo = 0.0;
            SqlCommand cmd = new SqlCommand("Sp_ObtenerSaldo", cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@numero", llave);
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
        #endregion

        // GET: Login
        public ActionResult ActionLogin()
        {
            return View();
        }

        public ActionResult ActionRegistrar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ActionRegistrar(Cliente datosRecibidos)
        {
            string resultado = registrarCliente(datosRecibidos);
            if (resultado.Contains("Error"))
            {
                ViewBag.Mensaje = resultado;
                return View();
            }
            else
            {
                ViewBag.MensajeBueno = resultado;
                return View("~/Views/Login/ActionLogin.cshtml");
            }
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