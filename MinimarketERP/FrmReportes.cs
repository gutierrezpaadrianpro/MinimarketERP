using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinimarketERP
{
    public partial class FrmReportes : Form
    {
        public FrmReportes()
        {
            InitializeComponent();
        }

        private void FrmReportes_Load(object sender, EventArgs e)
        {
            CargarVendedores();
            // Inicializar las fechas por defecto en el día actual
            dtpFechaInicial.Value = DateTime.Today;
            dtpFechaFinal.Value = DateTime.Today;
            lblTotalRecaudado.Text = "0.00";
        }
        private void CargarVendedores()
        {
            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();
                    string query = "SELECT IdEmpleado, NombreCompleto FROM Empleados WHERE Activo = 1";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Agregar opción general para consultar todos los vendedores
                    DataRow filaTodos = dt.NewRow();
                    filaTodos["IdEmpleado"] = 0;
                    filaTodos["NombreCompleto"] = "-- Todos los Vendedores --";
                    dt.Rows.InsertAt(filaTodos, 0);

                    cmbVendedor.DataSource = dt;
                    cmbVendedor.DisplayMember = "NombreCompleto";
                    cmbVendedor.ValueMember = "IdEmpleado";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la lista de vendedores: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Está seguro de salir de la aplicación?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
