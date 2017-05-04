using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Windows.Forms;

namespace registro
{
    public class Datos
    {
        public static Enlace enlace1;
        public static Enlace enlace2;
        public static Enlace enlace3;

        public static bool abrir()
        {
            try
            {
                enlace1 = new Enlace(
                      ConfigurationManager.AppSettings["servidor"]
                    , ConfigurationManager.AppSettings["almacen"]
                    , ConfigurationManager.AppSettings["usuario"]
                    , ConfigurationManager.AppSettings["clave"]);
                enlace1.abrir();
                return true;
            }
            catch
            {
                MessageBox.Show("Problemas en la conexión");
            }
            return false;
        }

        private static String fecha_mysql(String fecha)
        {
            String[] a = fecha.Split('/');
            if (a.Length == 3) return a[2] + "-" + a[1] + "-" + a[0];
            return String.Empty;
        }

        public static int cotizacion_guardar(String fecha, String monto, String etiqueta, String concepto, String detalle)
        {
            String fecha2 = fecha_mysql(fecha);
            String monto2 = monto.Replace(",", "");
            etiqueta = etiqueta.Trim();
            concepto = concepto.Trim();
            detalle = detalle.Trim();
            if (!String.IsNullOrEmpty(etiqueta))
            {
                String codigo = enlace1.valor(String.Format("select reg_codigo from registro where reg_etiqueta='{0}' and reg_fecha='{1}' and reg_tipo=1 and reg_estado=1", etiqueta, fecha2));
                if (String.IsNullOrEmpty(codigo))
                    return enlace1.ejecutar(String.Format("insert into registro (reg_fecha,reg_tipo,reg_monto,reg_etiqueta,reg_concepto,reg_detalle,reg_creado,reg_estado) values('{0}',1,{1},'{2}','{3}','{4}',curdate(),1)", fecha2, monto2, etiqueta, concepto, detalle));
            }
            return 0;
        }

        public static MySqlDataReader cotizacion_listar(DateTime fecha)
        {
            return enlace1.consulta(String.Format("select * from registro where reg_fecha='{0}' and reg_tipo=1 and reg_estado=1 order by reg_etiqueta+0", fecha.ToString("yyyy-MM-dd")));
        }

    }







    public class Enlace
    {
        public MySqlConnection enlace;
        String servidor;
        String almacen;
        String usuario;
        String clave;

        public Enlace(String servidor, String almacen, String usuario, String clave)
        {
            this.servidor = servidor;
            this.almacen  = almacen;
            this.usuario  = usuario;
            this.clave    = clave;
        }

        public bool abrir()
        {
            try
            {
                enlace = new MySqlConnection(String.Format("server={0};database={1};uid={2};password={3}", this.servidor, this.almacen, this.usuario, this.clave));
                enlace.Open();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Error al abrir la base de datos: server={0};database={1};uid={2}", servidor, almacen, usuario));
                MessageBox.Show(ex.Message);
            }
            return false;
        }

        public int ejecutar(String consulta)
        {
            MySqlCommand comando = new MySqlCommand(consulta, enlace);
            return comando.ExecuteNonQuery();
        }

        public String indice(String consulta)
        {
            MySqlCommand comando = new MySqlCommand(consulta, enlace);
            comando.ExecuteNonQuery();
            comando = new MySqlCommand("select last_insert_id()", enlace);
            return comando.ExecuteScalar().ToString();
        }


        public String ultimo(String consulta)
        {
            MySqlCommand comando = new MySqlCommand(consulta, enlace);
            comando.ExecuteNonQuery();
            return valor("select last_insert_id()");
        }


        public String valor(String consulta, String defecto = "")
        {
            MySqlCommand comando = new MySqlCommand(consulta, enlace);
            object respuesta = comando.ExecuteScalar();
            return respuesta == null ? defecto : respuesta.ToString();
        }

        public DataRow primero(String consulta)
        {
            DataTable tabla = new DataTable();
            MySqlDataAdapter a = new MySqlDataAdapter(consulta, enlace);
            a.Fill(tabla);
            if (tabla.Rows.Count > 0) return tabla.Rows[0]; else return null;
        }

        public DataTable lista(String consulta)
        {
            DataTable tabla2 = new DataTable();
            MySqlDataAdapter a = new MySqlDataAdapter(consulta, enlace);
            a.Fill(tabla2);
            return tabla2;
        }

        public MySqlDataReader consulta(String consulta)
        {
            MySqlCommand comando;
            comando = new MySqlCommand(consulta, enlace);
            return comando.ExecuteReader();
        }

    }





}
