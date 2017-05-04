using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Globalization;


namespace registro
{
    public partial class Form1 : Form
    {
        
        int   documento_tipo;
        int   pagina;
        int   contador;
        bool  pagina_fin;
        String   documento_fecha;
        int codigo;
        

        String documento1;
        String documento2;
        String documento3;

        int[] tipo1;
        int[] tipo2;
        int[] tipo3;

        int[] fecha1;
        int[] fecha2;
        int[] fecha3;

        int[] hoja1;
        int[] hoja2;
        int[] hoja3;

        int[][] columnas1;
        int[][] columnas2;
        int[][] columnas3;

        String fin_texto1;
        String fin_texto2;
        String fin_texto3;

        int[] fin_lugar1;
        int[] fin_lugar2;
        int[] fin_lugar3;

        String documento_nombre;

        public Form1()
        {
            InitializeComponent();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private String [] texto_partir(int [] corte, String texto)
        {
            int lugar=0;
            String [] partes=new String [corte.Length];
            for(int i=0;i<corte.Length;i++)
            {
                if (texto.Length >= lugar+corte[i])
                {
                    partes[i] = texto.Substring(lugar, corte[i]);
                }
                else
                {
                    partes[i] = String.Empty;
                }
                lugar += corte[i];                
            }
            return partes;
        }


        private void texto_captura(String texto)
        {
            int fila;
            switch(documento_tipo)
            { 
                case 1:
                    if(contador>hoja1[1] && !pagina_fin)
                    {
                        
                        fila = (contador - hoja1[1]) % columnas1.Length;
                        String [] partes = texto_partir(columnas1[fila], texto);
                        if (fin_lugar1[0] == fila && partes[fin_lugar1[1]].Equals(fin_texto1)) pagina_fin = true;
                        else
                            switch (fila)
                            {
                                case 0:
                                Datos.cotizacion_guardar(documento_fecha,partes[5],partes[0],partes[2],partes[3]);
                                break;
                            }
                    }
                    contador++;
                    if (contador > hoja1[0]) { contador = 1; pagina++; pagina_fin = false; }
                break;
            }
        }

        private void archivo_leer(String nombre)
        {
            System.IO.StreamReader archivo;
            archivo = new System.IO.StreamReader(nombre);
            pagina = 1;
            contador = 1;
            pagina_fin = false;
            String linea = archivo.ReadLine();
            while (linea != null)
            {
                texto_captura(linea);
                linea = archivo.ReadLine();
            }
            archivo.Close();
        }

        private void tipo_encontrar(String nombre)
        {
            System.IO.StreamReader archivo;
            archivo = new System.IO.StreamReader(nombre);
            int contador = 1;
            String linea = archivo.ReadLine();
            while (linea != null && contador<20) 
            {
                if (tipo1[0] == contador && linea.Length >= tipo1[1]+tipo1[2])
                    if (documento1.Equals(linea.Substring(tipo1[1], tipo1[2])))
                    { documento_tipo = 1; documento_nombre=documento1; break; }
                if (tipo2[0] == contador && linea.Length >= tipo2[1]+tipo2[2])
                    if (documento2.Equals(linea.Substring(tipo2[1], tipo2[2])))
                    { documento_tipo = 2; documento_nombre=documento2; break; }
                if (tipo3[0] == contador && linea.Length >= tipo3[1]+tipo3[2])
                    if (documento3.Equals(linea.Substring(tipo3[1], tipo3[2])))
                    { documento_tipo = 3; documento_nombre=documento3; break; }
                contador++;
                linea = archivo.ReadLine();
            }
            archivo.Close();
        }

        private void fecha_encontrar(String nombre)
        {
            System.IO.StreamReader archivo;
            archivo = new System.IO.StreamReader(nombre);
            int contador = 1;
            String linea = archivo.ReadLine();
            while (linea != null && contador < 20)
            {
                if (documento_tipo == 1 && fecha1[0] == contador && linea.Length >= fecha1[1] + fecha1[2])
                { documento_fecha = linea.Substring(fecha1[1], fecha1[2]); break; }
                if (documento_tipo==2 && fecha2[0] == contador && linea.Length >= fecha2[1] + fecha2[2])
                { documento_fecha = linea.Substring(fecha2[1], fecha2[2]); break; }
                if (documento_tipo == 3 && fecha3[0] == contador && linea.Length >= fecha3[1] + fecha3[2])
                { documento_fecha = linea.Substring(fecha3[1], fecha3[2]); break; }
                contador++;
                linea = archivo.ReadLine();
            }
            archivo.Close();
        }



        private int[] lista_leer(String cadena)
        {
            int[] lista;
            String[] columnas = cadena.Split(',');
            lista = new int[columnas.Length];
            for (int j = 0; j < columnas.Length; j++)
                {
                    lista[j] = int.Parse(columnas[j]);
                }
            return lista;
        }


        private int[][] arreglo_leer(String cadena)
        {
            int[][] arreglo;
            String[] filas = cadena.Split(';');
            arreglo = new int[filas.Length][];
            for (int i = 0; i < filas.Length; i++)
            {
                String[] columnas = filas[i].Split(',');
                arreglo[i] = new int[columnas.Length];
                for (int j = 0; j < columnas.Length; j++)
                {
                    arreglo[i][j] = int.Parse(columnas[j]);
                }
            }
            return arreglo;
        }

        private void configurar()
        {
            try
            {

                documento1       = ConfigurationManager.AppSettings["documento1"];
                documento2       = ConfigurationManager.AppSettings["documento2"];
                documento3       = ConfigurationManager.AppSettings["documento3"];

                tipo1            = lista_leer(ConfigurationManager.AppSettings["tipo1"]);
                tipo2            = lista_leer(ConfigurationManager.AppSettings["tipo2"]);
                tipo3            = lista_leer(ConfigurationManager.AppSettings["tipo3"]);

                fecha1 = lista_leer(ConfigurationManager.AppSettings["fecha1"]);
                fecha2 = lista_leer(ConfigurationManager.AppSettings["fecha2"]);
                fecha3 = lista_leer(ConfigurationManager.AppSettings["fecha3"]);

                hoja1            = lista_leer(ConfigurationManager.AppSettings["hoja1"]);
                hoja2            = lista_leer(ConfigurationManager.AppSettings["hoja2"]);
                hoja3            = lista_leer(ConfigurationManager.AppSettings["hoja3"]);

                columnas1        = arreglo_leer(ConfigurationManager.AppSettings["columnas1"]);
                columnas2        = arreglo_leer(ConfigurationManager.AppSettings["columnas2"]);
                columnas3        = arreglo_leer(ConfigurationManager.AppSettings["columnas3"]);

                fin_lugar1       = lista_leer(ConfigurationManager.AppSettings["fin_lugar1"]);
                fin_lugar2       = lista_leer(ConfigurationManager.AppSettings["fin_lugar2"]);
                fin_lugar3       = lista_leer(ConfigurationManager.AppSettings["fin_lugar3"]);

                fin_texto1 = ConfigurationManager.AppSettings["fin_texto1"];
                fin_texto2 = ConfigurationManager.AppSettings["fin_texto2"];
                fin_texto3 = ConfigurationManager.AppSettings["fin_texto3"];
            }
            catch
            {
                MessageBox.Show("Error en el archivo de configuración");
                Close();
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            configurar();
            
            Datos.abrir();
            String [] argumento = Environment.GetCommandLineArgs();
            if (argumento.Length > 1)
            {
                tipo_encontrar(argumento[1]);
                fecha_encontrar(argumento[1]);
                if (MessageBox.Show("Capturara Datos\n"+"Docuento: "+documento_nombre+"\nFecha:"+documento_fecha,
                    "Importar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    archivo_leer(argumento[1]);
                }
                Close();
            }
        }

        private void cotizacionesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 formulario = new Form2();
            if (formulario.ShowDialog() == DialogResult.OK)
            {
            }
        }
    }
}
