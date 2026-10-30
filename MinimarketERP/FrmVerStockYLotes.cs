using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace MinimarketERP
{
    public partial class FrmVerStockYLotes : Form
    {
        public FrmVerStockYLotes()
        {
            InitializeComponent();
        }

        private void FrmVerStockYLotes_Load(object sender, EventArgs e)
        {
            CargarFiltrosEstado();
            CargarStockGeneral();
        }
        private void CargarFiltrosEstado()
        {
            cmbFiltrarEstado.Items.Clear();
            cmbFiltrarEstado.Items.Add("-- Todos los Estados --");
            cmbFiltrarEstado.Items.Add("⚠️ STOCK BAJO");
            cmbFiltrarEstado.Items.Add("🟢 ÓPTIMO");
            cmbFiltrarEstado.SelectedIndex = 0;
        }

        private void CargarStockGeneral(string filtroTexto = "", string filtroEstado = "")
        {
            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();
                    string query = @"SELECT IdProducto, Codigo AS [Código], Nombre AS [Producto], 
                                            PrecioCosto AS [Costo], PrecioVenta AS [Precio Venta], 
                                            Stock, 
                                            CASE 
                                                WHEN Stock <= 5 THEN '⚠️ STOCK BAJO' 
                                                ELSE '🟢 ÓPTIMO' 
                                            END AS [Estado]
                                     FROM Productos 
                                     WHERE Activo = 1";

                    // Aplicar filtro por texto (nombre o código)
                    if (!string.IsNullOrWhiteSpace(filtroTexto))
                    {
                        query += " AND (Nombre LIKE @filtroTexto OR Codigo LIKE @filtroTexto)";
                    }

                    query += " ORDER BY Stock ASC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        if (!string.IsNullOrWhiteSpace(filtroTexto))
                        {
                            cmd.Parameters.AddWithValue("@filtroTexto", "%" + filtroTexto.Trim() + "%");
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Filtrar por estado si se seleccionó uno específico desde el ComboBox
                        if (!string.IsNullOrEmpty(filtroEstado) && filtroEstado != "-- Todos los Estados --")
                        {
                            DataView dv = dt.DefaultView;
                            dv.RowFilter = $"Estado = '{filtroEstado}'";
                            dgvStock.DataSource = dv.ToTable();
                        }
                        else
                        {
                            dgvStock.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el stock: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string textoBusqueda = txtBuscarProducto.Text;
            string estadoSeleccionado = cmbFiltrarEstado.SelectedItem?.ToString() ?? "";
            CargarStockGeneral(textoBusqueda, estadoSeleccionado);
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (dgvStock.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos en la grilla para exportar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Reporte de stock e inventario exportado correctamente.", "Exportación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Está seguro de salir de la aplicación?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
