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
    public partial class FrmRegistrarCompra : Form
    {
        private DataTable dtLotesCompra = new DataTable();
        public FrmRegistrarCompra()
        {
            InitializeComponent();
        }

        private void FrmRegistrarCompra_Load(object sender, EventArgs e)
        {
            CargarProveedores();
            ConfigurarGrillaLotes();
            dtpFechaLlegada.Value = DateTime.Today;
        }
        // 1. Cargar lista de proveedores disponibles
        private void CargarProveedores()
        {
            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();
                    string query = "SELECT IdProveedor, NombreEmpresa FROM Proveedores WHERE Activo = 1";

                    // Si aún no tienes la tabla Proveedores, se puede usar este bloque fallback:
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbProveedor.DataSource = dt;
                    cmbProveedor.DisplayMember = "NombreEmpresa";
                    cmbProveedor.ValueMember = "IdProveedor";
                }
            }
            catch
            {
                // En caso de que no exista la tabla Proveedores en SQL, cargamos ítems por defecto
                cmbProveedor.Items.Clear();
                cmbProveedor.Items.Add("Distribuidora Central");
                cmbProveedor.Items.Add("Proveedor General");
                cmbProveedor.SelectedIndex = 0;
            }
        }

        // 2. Estructurar la grilla de lotes de compra
        private void ConfigurarGrillaLotes()
        {
            if (dtLotesCompra.Columns.Count == 0)
            {
                dtLotesCompra.Columns.Add("IdProducto", typeof(int));
                dtLotesCompra.Columns.Add("Producto", typeof(string));
                dtLotesCompra.Columns.Add("NroLote", typeof(string));
                dtLotesCompra.Columns.Add("Cantidad", typeof(int));
                dtLotesCompra.Columns.Add("CostoUnitario", typeof(decimal));
                dtLotesCompra.Columns.Add("Subtotal", typeof(decimal), "Cantidad * CostoUnitario");
                dtLotesCompra.Columns.Add("FechaVencimiento", typeof(DateTime));
            }
            dgvDetalleCompra.DataSource = dtLotesCompra;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            string factura = string.IsNullOrWhiteSpace(txtNroFactura.Text) ? "S/N" : txtNroFactura.Text.Trim();
            DateTime fecha = dtpFechaLlegada.Value;

            // Agrega el registro directamente a la grilla
            dtLotesCompra.Rows.Add(1, "Lote Ingresado - " + factura, factura, fecha);

            MessageBox.Show("Lote agregado temporalmente a la lista.", "Lote Agregado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (dtLotesCompra.Rows.Count == 0)
            {
                MessageBox.Show("No se ha agregado ningún lote a la compra.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal totalGeneral = 0;
                foreach (DataRow row in dtLotesCompra.Rows)
                {
                    totalGeneral += Convert.ToDecimal(row["Subtotal"]);
                }

                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();

                    // Registrar Cabecera
                    string queryCompra = @"INSERT INTO Compras (IdEmpleado, FechaHora, NroFactura, Total) 
                                           OUTPUT INSERTED.IdCompra 
                                           VALUES (@idEmp, @fecha, @factura, @total)";

                    int idCompraGenerada = 0;
                    using (SqlCommand cmd = new SqlCommand(queryCompra, con))
                    {
                        cmd.Parameters.AddWithValue("@idEmp", EmpleadoSesion.IdEmpleado);
                        cmd.Parameters.AddWithValue("@fecha", dtpFechaLlegada.Value);
                        cmd.Parameters.AddWithValue("@factura", string.IsNullOrWhiteSpace(txtNroFactura.Text) ? "S/N" : txtNroFactura.Text.Trim());
                        cmd.Parameters.AddWithValue("@total", totalGeneral);

                        idCompraGenerada = (int)cmd.ExecuteScalar();
                    }

                    // Registrar Detalles y Actualizar Stock
                    foreach (DataRow row in dtLotesCompra.Rows)
                    {
                        int idProd = Convert.ToInt32(row["IdProducto"]);
                        int cantidad = Convert.ToInt32(row["Cantidad"]);
                        decimal costo = Convert.ToDecimal(row["CostoUnitario"]);
                        decimal subtotal = Convert.ToDecimal(row["Subtotal"]);

                        string queryDet = @"INSERT INTO DetalleCompras (IdCompra, IdProducto, Cantidad, CostoUnitario, Subtotal) 
                                            VALUES (@idC, @idP, @cant, @costo, @sub)";
                        using (SqlCommand cmdD = new SqlCommand(queryDet, con))
                        {
                            cmdD.Parameters.AddWithValue("@idC", idCompraGenerada);
                            cmdD.Parameters.AddWithValue("@idP", idProd);
                            cmdD.Parameters.AddWithValue("@cant", cantidad);
                            cmdD.Parameters.AddWithValue("@costo", costo);
                            cmdD.Parameters.AddWithValue("@sub", subtotal);
                            cmdD.ExecuteNonQuery();
                        }

                        // Aumentar Stock del Producto
                        string queryStock = "UPDATE Productos SET Stock = Stock + @cant WHERE IdProducto = @idP";
                        using (SqlCommand cmdS = new SqlCommand(queryStock, con))
                        {
                            cmdS.Parameters.AddWithValue("@cant", cantidad);
                            cmdS.Parameters.AddWithValue("@idP", idProd);
                            cmdS.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("¡Compra registrada y stock de productos actualizado con éxito!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                dtLotesCompra.Clear();
                txtNroFactura.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la compra: " + ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
