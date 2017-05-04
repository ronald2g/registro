using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Windows.Forms;

namespace afiliaciones
{
    public class Datos
    {
        public static Enlace enlace1;
        public static Enlace enlace2;
        public static Enlace enlace3;

        public static String [] grupo  = {"O","A","B","AB" };
        public static String [] factor = {"-","+"};

        public static bool abrir()
        {
            try
            {
                enlace1 = new Enlace(
                      ConfigurationManager.AppSettings["servidor"]
                    , ConfigurationManager.AppSettings["almacen"]
                    , ConfigurationManager.AppSettings["usuario"]
                    , ConfigurationManager.AppSettings["clave"]);
                enlace2 = new Enlace(
                      ConfigurationManager.AppSettings["servidor2"]
                    , ConfigurationManager.AppSettings["almacen2"]
                    , ConfigurationManager.AppSettings["usuario2"]
                    , ConfigurationManager.AppSettings["clave2"]);
                enlace3 = new Enlace(
                      ConfigurationManager.AppSettings["servidor3"]
                    , ConfigurationManager.AppSettings["almacen3"]
                    , ConfigurationManager.AppSettings["usuario3"]
                    , ConfigurationManager.AppSettings["clave3"]);
                return true;
            }
            catch
            {
                MessageBox.Show("Problemas en la conexión");
            }
            return false;
        }

        





        

        /* ASEGURADO */

        public static MySqlDataReader asegurado2_buscar(String cadena)
        {
            MySqlDataReader puntero = null;
            char[] espacio = new char[] {' '};
            String[] lista = cadena.Split(espacio, StringSplitOptions.RemoveEmptyEntries);
            if (lista.Length == 2)
            {
                puntero = enlace1.consulta(String.Format("select * from afiliado where ((paterno like '{0}%' or materno like '{0}%') and (nombre like '{1}%' or nombre like '% {1}%')) or (paterno like '{0}%' and materno like '{1}%') limit 30", lista[0], lista[1]));
            } 
            else if(lista.Length>2)
            {
                puntero = enlace1.consulta(String.Format("select * from afiliado where paterno like '{0}%' and materno like '{1}%' and (nombre like '{2}%' or nombre like '% {2}%') limit 30", lista[0], lista[1],lista[2]));
            }
            return puntero;
        }

        public static String[] asegurado_datos(String matricula,String carpeta,String ci)
        {
            String[] matricula2 = matricula_traducir(matricula);
            String nace = matricula2[0];
            String sexo = matricula2[1];

            String[] lista = carpeta_traducir(carpeta.ToString());
            String regional = lista[0];
            String carpeta2 = lista[1];

            String numero=codigo_cadena(ci);
            
            return new String[] {nace,sexo,carpeta2,regional,numero};

        }

        public static String asegurado2_guardar(String matricula, String carpeta, String ci, int expedido, String paterno, String materno, String nombre, String empresa, int sangre, int distrito)
        {
            String[] datos = asegurado_datos(matricula, carpeta, ci);
            String persona = enlace2.indice(String.Format("insert into persona (nom_paterno,nom_materno,nom_nombre,nac_fecha,nac_sexo,per_tipo,per_empresa,per_sangre,per_distrito) values ('{0}','{1}','{2}','{3}',{4},1,{5},{6})", paterno, materno, nombre, datos[0], datos[1], empresa, sangre,distrito));
            enlace2.ejecutar(String.Format("insert into codigo (cod_persona,cod_historia,cod_regional,cod_familia,cod_matricula,cid_numero,cid_lugar) values ({0},{1},{2},'A','{3}',{4},{5})", persona, datos[2], datos[3], matricula, datos[4],expedido));
            return persona;
        }

        public static int asegurado2_modificar(String persona,String matricula, String carpeta, String ci, String paterno, String materno, String nombre, String empresa, int sangre)
        {
            String[] datos = asegurado_datos(matricula, carpeta, ci);
            enlace2.ejecutar(String.Format("update persona set nom_paterno='{1}',nom_materno='{2}',nom_nombre='{3}',nac_fecha='{4}',nac_sexo={5},per_tipo=1,per_empresa={6},per_sangre={7} where per_codigo={0}", persona,paterno, materno, nombre, datos[0], datos[1], empresa, sangre));
            String codigo = enlace2.valor(String.Format("select cod_persona from codigo where cod_persona={0}", persona));
            if (String.IsNullOrEmpty(codigo))
            {
                return enlace2.ejecutar(String.Format("insert into codigo (cod_persona,cod_historia,cod_regional,cod_familia,cod_matricula,cid_numero) values ({0},{1},{2},'A','{3}',{4})", persona, datos[2], datos[3], matricula, datos[4]));
            }
            else
            {
                return enlace2.ejecutar(String.Format("update codigo set cod_historia={1},cod_regional={2},cod_familia='A',cod_matricula='{3}',cid_numero={4} where cod_persona={0}", persona, datos[2], datos[3], matricula, datos[4]));
            }
        }

        /******** guardar datos del asegurado en cochabamba **********/

        public static String asegurado2_guardar(DataRow persona)
        {
            String empresa = empresa_guardar(persona["empresa"].ToString());
            String codigo = enlace2.indice(String.Format("insert into persona (nom_paterno,nom_materno,nom_nombre,nac_fecha,nac_sexo,per_tipo,per_empresa,per_sangre,per_distrito) values ('{0}','{1}','{2}','{3}',{4},1,{5},0,4)", persona["paterno"].ToString(), persona["materno"].ToString(), persona["nombre"].ToString(), persona["nace"].ToString(), persona["sexo"].ToString(), empresa));
            enlace2.ejecutar(String.Format("insert into codigo (cod_persona,cod_historia,cod_regional,cod_familia,cod_matricula,cid_numero) values ({0},{1},{2},'A','{3}',0)", codigo, persona["carpeta"].ToString(), persona["regional"].ToString(), persona["matricula"].ToString()));
            return codigo;
        }

        public static int beneficiario2_actualizar(String codigo,int sangre,String familia,int pariente,String ci,int expedido)
        {
            ci = codigo_numero(ci);
            enlace2.ejecutar(String.Format("update codigo set cod_familia='{1}',cid_numero={2},cid_lugar={3} where cod_persona={0}", codigo, familia,ci,expedido));
            return enlace2.ejecutar(String.Format("update persona set per_pariente={1},per_sangre={2} where per_codigo={0}", codigo, pariente, sangre));
        }

        public static String beneficiario2_guardar(DataRow persona)
        {
            String empresa = empresa_guardar(persona["empresa"].ToString());
            String titular = enlace2.valor(String.Format("select cod_persona from codigo where cod_matricula='{0}'", persona["tafiliado"].ToString()));
            String codigo = enlace2.indice(String.Format("insert into persona (nom_paterno,nom_materno,nom_nombre,nac_fecha,nac_sexo,per_tipo,per_empresa,per_sangre,per_titular,per_distrito,per_pariente) values ('{0}','{1}','{2}','{3}',{4},2,{5},0,{6},4,0)", persona["paterno"].ToString(), persona["materno"].ToString(), persona["nombre"].ToString(), persona["nace"].ToString(), persona["sexo"].ToString(), empresa,titular));
            enlace2.ejecutar(String.Format("insert into codigo (cod_persona,cod_historia,cod_regional,cod_familia,cod_matricula,cid_numero) values ({0},{1},{2},'','{3}',0)", codigo, persona["carpeta"].ToString(), persona["regional"].ToString(), persona["matricula"].ToString()));
            return codigo;
        }


        /****** recupera asegurado base: la paz *****/

        public static DataRow asegurado1_recuperar(String codigo)
        {
            return enlace1.primero(String.Format("select id,matricula,paterno,materno,nombre,hclinica,empresa,tafiliado from afiliado where id={0}", codigo));
        }

        /* asegurado completar codigo de persona,ci,sangre distrito base : cochabamba */

        public static void beneficiario2_completar(DataRow asegurado)
        {
            // asegurado_traducir(asegurado);

            asegurado.Table.Columns.Add("codigo");
            asegurado.Table.Columns.Add("ci");
            asegurado.Table.Columns.Add("expedido");
            asegurado.Table.Columns.Add("sangre");
            asegurado.Table.Columns.Add("distrito");
            asegurado.Table.Columns.Add("pariente");
            asegurado.Table.Columns.Add("familia");

            DataRow datos;
            datos = enlace2.primero(String.Format("select * from persona,codigo where per_codigo=cod_persona and cod_matricula='{0}'", asegurado["matricula"].ToString()));

            if (datos != null)
            {
                asegurado["codigo"] = datos["per_codigo"].ToString();
                asegurado["ci"] = codigo_cadena(datos["cid_numero"].ToString());
                asegurado["expedido"] = codigo_numero(datos["cid_lugar"].ToString());
                asegurado["sangre"] = codigo_numero(datos["per_sangre"].ToString());
                asegurado["distrito"] = codigo_numero(datos["per_distrito"].ToString());
                asegurado["pariente"] = codigo_numero(datos["per_pariente"].ToString());
                asegurado["familia"] = datos["cod_familia"].ToString();
            }
            else
            {
                asegurado["codigo"] = beneficiario2_guardar(asegurado);
                asegurado["ci"] = "";
                asegurado["expedido"] = "0";
                asegurado["sangre"] = "0";
                asegurado["distrito"] = "0";
                asegurado["pariente"] = "0";
                asegurado["familia"] = "";
            }
        }




        public static void asegurado2_completar(DataRow asegurado)
        {
           // asegurado_traducir(asegurado);

            asegurado.Table.Columns.Add("codigo");
            asegurado.Table.Columns.Add("ci");
            asegurado.Table.Columns.Add("sangre");
            asegurado.Table.Columns.Add("distrito");

            DataRow datos= enlace2.primero(String.Format("select * from persona,codigo where per_codigo=cod_persona and cod_matricula='{0}'", asegurado["matricula"].ToString()));

            if (datos != null)
            {
                asegurado["codigo"  ] = datos["per_codigo"  ].ToString();
                asegurado["ci"      ] = codigo_cadena(datos["cid_numero"  ].ToString());
                asegurado["sangre"  ] = codigo_numero(datos["per_sangre"  ].ToString());
                asegurado["distrito"] = codigo_numero(datos["per_distrito"].ToString());
            }
            else
            {
                asegurado["codigo"  ] = asegurado2_guardar(asegurado);
                asegurado["ci"      ] = "";
                asegurado["sangre"  ] = "0";
                asegurado["distrito"] = "0";
            }
        }

        public static MySqlDataReader beneficiario3_recuperar(String carpeta, String ci, String matricula)
        {
            String carnet = "";
            String carpeta2 = "";
            String mat = matricula.Substring(2);
            if (!String.IsNullOrEmpty(ci)) carnet = String.Format(" or CIPac={0}", ci);
            if (!String.IsNullOrEmpty(carpeta)) carpeta2 = String.Format(" or NroSobre={0}", carpeta);
            return enlace3.consulta(String.Format("select MatPac,NomPac,CIPac from bene where MatPac like '{0}%'{1}{2}", mat, carpeta2, carnet));
        }

        public static MySqlDataReader asegurado3_recuperar(String carpeta,String ci,String matricula)
        {
            String carnet = "";
            String carpeta2 = "";
            String mat = matricula.Substring(2);
            if(!String.IsNullOrEmpty(ci)) carnet=String.Format(" or CIAseg={0}",ci);
            if(!String.IsNullOrEmpty(carpeta)) carpeta2 = String.Format(" or NroSobre={0}", carpeta);
            return enlace3.consulta(String.Format("select MatAseg,NomAseg,CIAseg from aseg where MatAseg like '{0}%'{1}{2}",mat,carpeta2,carnet)); 
        }

        public static int sangre_anterior(String sangre){
            int indice = 0;
            if (sangre.Contains("O")) indice = 1;
            if (sangre.Contains("A")) indice = 3;
            if (sangre.Contains("B")) indice = 5;
            if (sangre.Contains("A") && sangre.Contains("B")) indice = 7;
            if (sangre.Contains("+")) indice += 1;
            return indice;
        }


        public static int pariente_anterior(String codigo,String sexo)
        {
            int[] femenino ={0,3,1,7,13,5,11,0,0,0,0,0,0,0,0,0,0,0};
            int[] masculino={0,4,2,8,16,6,12,0,0,0,0,0,0,0,0,0,0,0};

            int numero = 0;
            Int32.TryParse(codigo, out numero);

            if(sexo.Equals("S"))
                return masculino[numero];
            else
                return femenino[numero];
        }

        public static String departamento_anterior(String codigo)
        {
            int[] depto = {0,4,3,8,6,1,5,2,7,9,0,0,0};
            int numero = 0;
            Int32.TryParse(codigo, out numero);
            return depto[numero].ToString();
        }

        public static void beneficiario3_rescatar(String persona, String matricula)
        {
            DataRow asegurado = enlace3.primero(String.Format("select * from bene where MatPac='{0}'", matricula));
            int numero = 0;
            Int32.TryParse(asegurado["CIPac"].ToString(), out numero);
            int pariente=pariente_anterior(asegurado["TipPar"].ToString(),asegurado["Sexo"].ToString());
            enlace2.ejecutar(String.Format("update codigo set cid_numero={1},cod_familia='{2}' where cod_persona={0}", persona, numero, asegurado["His"].ToString()));
            enlace2.ejecutar(String.Format("update persona set per_pariente={1} where per_codigo={0}", persona, pariente));
        }

        public static String departamento_codigo(String nombre,String depto)
        {
            return enlace2.valor(String.Format("select pro_codigo from _provincia where pro_nombre='{0}' and pro_padre={1}", nombre,depto),"0");
        }
        public static DataRow complementario_recuperar(String persona)
        {
            return enlace2.primero(String.Format("select * from datos where dat_persona={0}", persona));
        }


        public static int complemento_guardar(String persona, int depto, int provincia, String localidad, String padre, String madre, String esposo, String profesion, String ocupacion, String calle, String numero, String zona, String telefono, String telefono2, DateTime fecha)
        {
            String codigo = enlace2.valor(String.Format("select dat_persona from datos where dat_persona={0}", persona));
            if (String.IsNullOrEmpty(codigo))
            {
                return enlace2.ejecutar(String.Format("insert into datos (dat_persona,nac_depto,nac_provincia,nac_localidad,nac_padre,nac_madre,nom_esposo,per_profesion,per_ocupacion,dom_calle,dom_numero,dom_zona,dom_telefono,per_telefono,car_expedido) values ({0},{1},{2},'{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')", persona, depto, provincia, localidad, padre, madre, esposo, profesion, ocupacion, calle, numero, zona, telefono, telefono2, fecha.ToString("yyyy-MM-dd")));
            }
            else
            {
                return enlace2.ejecutar(String.Format("update datos set nac_depto={1},nac_provincia={2},nac_localidad='{3}',nac_padre='{4}',nac_madre='{5}',nom_esposo='{6}',per_profesion='{7}',per_ocupacion='{8}',dom_calle='{9}',dom_numero='{10}',dom_zona='{11}',dom_telefono='{12}',per_telefono='{13}',car_expedido='{14}' where dat_persona={0}", persona, depto, provincia, localidad, padre, madre, esposo, profesion, ocupacion, calle, numero, zona, telefono, telefono2, fecha.ToString("yyyy-MM-dd")));
            }
        }

        public static void asegurado3_rescatar(String persona,String matricula)
        {
            DataRow asegurado = enlace3.primero(String.Format("select * from aseg where MatAseg='{0}'", matricula));
            enlace2.ejecutar(String.Format("update codigo set cid_numero={0} where cod_persona={1}", asegurado["CIAseg"].ToString(),persona));
            DataRow complemento = enlace3.primero(String.Format("select * from dataseg where MatAseg='{0}'", matricula));
            int indice = sangre_anterior(complemento["GS"].ToString());
            enlace2.ejecutar(String.Format("update persona set per_sangre={0},per_distrito={1} where per_codigo={2}",indice,4,persona));
            
            String codigo = enlace2.valor(String.Format("select dat_persona from datos where dat_persona={0}",persona));
            String depto = departamento_anterior(complemento["Depto"].ToString());
            String provincia = enlace2.valor(String.Format("select pro_codigo from _provincia where pro_nombre='{0}' and pro_padre={1}", complemento["Provi"].ToString(),depto),"0");
            DateTime registro=(DateTime)asegurado["FecAfil"];
            if (String.IsNullOrEmpty(codigo))
            {
                enlace2.ejecutar(String.Format("insert into datos (dat_persona,nac_depto,nac_provincia,nac_localidad,nac_padre,nac_madre,nom_esposo,per_profesion,per_ocupacion,dom_calle,dom_numero,dom_zona,dom_telefono,per_telefono,car_expedido) values ({0},{1},{2},'{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')", persona, depto, provincia, complemento["Local"].ToString(), complemento["Padre"].ToString(), complemento["Madre"].ToString(),"", complemento["Profes"].ToString(), complemento["Ocupa"].ToString(), complemento["Direcc"].ToString(), "", "", complemento["Telef"].ToString(), complemento["Telfof"].ToString(), registro.ToString("yyyy-MM-dd")));
            }
            else
            {
                enlace2.ejecutar(String.Format("update datos set nac_depto={1},nac_provincia={2},nac_localidad='{3}',nac_padre='{4}',nac_madre='{5}',nom_esposo='{6}',per_profesion='{7}',per_ocupacion='{8}',dom_calle='{9}',dom_numero='{10}',dom_zona='{11}',dom_telefono='{12}',per_telefono='{13}',car_expedido='{14}' where dat_persona={0}", persona, depto, provincia, complemento["Local"].ToString(), complemento["Padre"].ToString(), complemento["Madre"].ToString(), "",complemento["Profes"].ToString(), complemento["Ocupa"].ToString(), complemento["Direcc"].ToString(), "", "", complemento["Telef"].ToString(), complemento["Telfof"].ToString(), registro.ToString("yyyy-MM-dd")));
            }
        }

        public static String[] matricula_traducir(String matricula){
            int mes = Convert.ToInt32(matricula.Substring(4, 2));
            String sexo = "1";
            if (mes >12) 
            {
                sexo = "2";
                mes -= 50;
            }

            String nace = String.Format("{0:D4}-{1:D2}-{2:D2}", matricula.ToString().Substring(0, 4), mes, matricula.ToString().Substring(6, 2));
            return new String[] {nace,sexo};
        }

        public static String[] carpeta_traducir(String carpeta)
        {
            String[] lista = carpeta.Split('-');
            if (lista.Length == 2) return new String[] {codigo_numero(lista[0]), codigo_numero(lista[1])};
            else return new String[] { "3", "0" };
        }

        public static void asegurado_traducir(DataRow asegurado)
        {
            asegurado.Table.Columns.Add("sexo");
            asegurado.Table.Columns.Add("carpeta");
            asegurado.Table.Columns.Add("regional");
            asegurado.Table.Columns.Add("nace");
            asegurado.Table.Columns.Add("nace2",typeof(DateTime));

            String[] matricula2 = matricula_traducir(asegurado["matricula"].ToString());
            asegurado["nace"] = matricula2[0];
            asegurado["nace2"] = DateTime.ParseExact(matricula2[0], "yyyy-MM-dd", null);
            asegurado["sexo"] = matricula2[1];

            String[] lista = carpeta_traducir(asegurado["hclinica"].ToString());
            asegurado["regional"] = lista[0];
            asegurado["carpeta"] = lista[1];
        }

        public static DataRow asegurado1_matricula(String matricula)
        {
            /* no se usa select * porque no se controla la excepcion de lectura de fechas */
            return enlace1.primero(String.Format("select id,matricula,paterno,materno,nombre,hclinica,empresa,tafiliado from afiliado where matricula='{0}'", matricula));
        }

        public static String asegurado_busqueda(String matricula)
        {
            return enlace1.valor(String.Format("select id from afiliado where matricula='{0}'", matricula));
        }


        public static MySqlDataReader beneficiario_buscar(String cadena)
        {
            return enlace1.consulta(String.Format("select * from afiliado where tafiliado='{0}' and matricula<>'{0}'", cadena));
        }

        public static String empresa_guardar(String nombre)
        {
            String codigo = enlace2.valor(String.Format("select emp_codigo from empresa where emp_nombre='{0}'", nombre));
            if (String.IsNullOrEmpty(codigo))
            {
                return enlace2.indice(String.Format("insert into empresa (emp_nombre) values ('{0}')",nombre));
            }
            else
            {
                return codigo;
            }
        }

        public static String asegurado2_encontrar(String matricula)
        {
            return enlace2.valor(String.Format("select cod_persona from codigo where cod_matricula='{0}'", matricula));
        }

        public static MySqlDataReader asegurados_similares(String paterno, String materno, String nombre,String ci)
        {
            String numero = codigo_cadena(ci);
            if (String.IsNullOrEmpty(numero))
            {
                return enlace2.consulta(String.Format("select * from persona left join codigo on per_codigo=cod_persona where nom_paterno='{0}' and (nom_materno='{1}' or nom_nombre='{2}') and (cod_matricula is null or cod_matricula='')", paterno, materno, nombre));
            }
            else
            {
                return enlace2.consulta(String.Format("select * from persona left join codigo on per_codigo=cod_persona where nom_paterno='{0}' and (nom_materno='{1}' or nom_nombre='{2}') and (cod_matricula is null or cod_matricula='') or cid_numero={3}", paterno, materno, nombre, numero));
            }
        }

        public static String codigo_cadena(String ci)
        {
            int numero = 0;
            Int32.TryParse(ci, out numero);
            if (numero > 0) return numero.ToString();
            else return String.Empty;
        }

        public static String codigo_numero(String ci)
        {
            int numero = 0;
            Int32.TryParse(ci, out numero);
            return numero.ToString();
        }



    }







    public class Enlace
    {
        public MySqlConnection enlace;

        public Enlace(String servidor, String almacen, String usuario, String clave)
        {
            try
            {
                enlace = new MySqlConnection(String.Format("server={0};database={1};uid={2};password={3}", servidor, almacen, usuario, clave));
                enlace.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("server={0};database={1};uid={2}", servidor, almacen, usuario));
                MessageBox.Show(ex.Message);
            }
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


        public String valor(String consulta,String defecto = "")
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



    public class Conexion
    {
        public MySqlConnection enlace;
        public MySqlConnection enlace2;
        public bool abrir()
        {
            try
            {
                enlace = new MySqlConnection(String.Format("server={0};database={1};uid={2};password={3}"
                    , ConfigurationManager.AppSettings["servidor"]
                    , ConfigurationManager.AppSettings["almacen"]
                    , ConfigurationManager.AppSettings["usuario"]
                    , ConfigurationManager.AppSettings["clave"]));

                enlace2 = new MySqlConnection(String.Format("server={0};database={1};uid={2};password={3}"
                    , ConfigurationManager.AppSettings["servidor2"]
                    , ConfigurationManager.AppSettings["almacen2"]
                    , ConfigurationManager.AppSettings["usuario2"]
                    , ConfigurationManager.AppSettings["clave2"]));
                //enlace2.Open();
                enlace.Open();
                return true;
            }
            catch
            {
                MessageBox.Show("Problemas en la conexión");
            }
            return false;
        }

     
        List<List<String>> permutar(List<String> lista)
        {
            List<List<String>> resultado = new List<List<string>>();
            if (lista.Count == 1)
            {
                resultado.Add(lista);
                return resultado;
            }
            else
            {
                String primero = lista[0];
                lista.RemoveAt(0);
                List<List<String>> permutado = permutar(lista);
                foreach (List<String> cadena in permutado)
                {
                    for (int i = 0; i <= cadena.Count; i++)
                    {
                        List<String> copia = new List<string>(cadena);
                        copia.Insert(i, primero);
                        resultado.Add(copia);
                    }
                }
            }
            return resultado;
        }


        public String[] busqueda(String cadena)
        {
            List<String> partido = new List<String>();
            List<String> consulta = new List<String>();
            partido.AddRange(cadena.Split(' '));
            List<List<String>> p = permutar(partido);
            foreach (List<String> lista in p)
            {
                consulta.Add(String.Join("% ", lista.ToArray()));
            }
            return consulta.ToArray();
        }

    }
}
