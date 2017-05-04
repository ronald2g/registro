using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace registro
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            MySqlDataReader puntero = Datos.cotizacion_listar(dateTimePicker1.Value);
            if (puntero != null)
            {
                 while (puntero.Read())
                    {
                        ListViewItem item = new ListViewItem(puntero.GetString("reg_etiqueta"));
                        item.SubItems.Add(puntero.GetString("reg_concepto"));
                        item.SubItems.Add(puntero.GetString("reg_monto"));
                        listView1.Items.Add(item);
                    }
                puntero.Close();
            }
            else
            {
                MessageBox.Show("Erro al listar cotizaciones");
            }
        }
    }
}
