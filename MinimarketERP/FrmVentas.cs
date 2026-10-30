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
    public partial class FrmVentas : Form
    {
        private DataTable dtCarrito = new DataTable();
        public FrmVentas()
        {
            InitializeComponent();
        }

        private void FrmVentas_Load(object sender, EventArgs e)
        {
            // Mostrar el nombre del vendedor activo en la etiqueta superior
            lblVendedor.Text = "Vendedor: " + EmpleadoSesion.NombreCompleto;
            CargarProductos();
            ConfigurarCarrito();
        }
        // Cargar los productos disponibles con stock en el ComboBox
        private void CargarProductos()
        {
            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();
                    string query = "SELECT IdProducto, Nombre, PrecioVenta, Stock FROM Productos WHERE Stock > 0";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbProducto.DataSource = dt;
                    cmbProducto.DisplayMember = "Nombre";
                    cmbProducto.ValueMember = "IdProducto";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Configurar las columnas de la grilla del carrito
        private void ConfigurarCarrito()
        {
            if (dtCarrito.Columns.Count == 0)
            {
                dtCarrito.Columns.Add("IdProducto", typeof(int));
                dtCarrito.Columns.Add("Producto", typeof(string));
                dtCarrito.Columns.Add("Precio", typeof(decimal));
                dtCarrito.Columns.Add("Cantidad", typeof(int));
                dtCarrito.Columns.Add("Subtotal", typeof(decimal), "Precio * Cantidad");
            }
            dgvCarrito.DataSource = dtCarrito;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (cmbProducto.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un producto válido.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idProd = Convert.ToInt32(cmbProducto.SelectedValue);
            string nombreProd = cmbProducto.Text;
            int cantidad = Convert.ToInt32(numCantidad.Value); // O si es textBox: Convert.ToInt32(txtCantidad.Text)

            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();
                    string query = "SELECT PrecioVenta, Stock FROM Productos WHERE IdProducto = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", idProd);
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                decimal precio = Convert.ToDecimal(dr["PrecioVenta"]);
                                int stock = Convert.ToInt32(dr["Stock"]);

                                if (cantidad > stock)
                                {
                                    MessageBox.Show("Stock insuficiente. Stock disponible: " + stock, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                // Verificar si ya existe en el carrito para sumar la cantidad
                                bool existe = false;
                                foreach (DataRow row in dtCarrito.Rows)
                                {
                                    if (Convert.ToInt32(row["IdProducto"]) == idProd)
                                    {
                                        int nuevaCant = Convert.ToInt32(row["Cantidad"]) + cantidad;
                                        if (nuevaCant > stock)
                                        {
                                            MessageBox.Show("La cantidad total supera el stock disponible.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }
                                        row["Cantidad"] = nuevaCant;
                                        existe = true;
                                        break;
                                    }
                                }

                                // Si no está, agregarlo como nueva fila
                                if (!existe)
                                {
                                    dtCarrito.Rows.Add(idProd, nombreProd, precio, cantidad);
                                }

                                CalcularTotal();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar producto: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CalcularTotal()
        {
            decimal total = 0;
            foreach (DataRow row in dtCarrito.Rows)
            {
                total += Convert.ToDecimal(row["Subtotal"]);
            }
            lblTotal.Text = total.ToString("N2");
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            if (dgvCarrito.CurrentRow != null && dgvCarrito.CurrentRow.Index < dtCarrito.Rows.Count)
            {
                dtCarrito.Rows.RemoveAt(dgvCarrito.CurrentRow.Index);
                CalcularTotal();
            }
            else
            {
                MessageBox.Show("Seleccione un producto del carrito para quitar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnProcesar_Click(object sender, EventArgs e)
        {
            if (dtCarrito.Rows.Count == 0)
            {
                MessageBox.Show("El carrito de ventas está vacío.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal totalVenta = 0;
                foreach (DataRow row in dtCarrito.Rows)
                {
                    totalVenta += Convert.ToDecimal(row["Subtotal"]);
                }

                decimal efectivoRecibido = 0;
                decimal.TryParse(txtEfectivoRecibido.Text, out efectivoRecibido);

                if (efectivoRecibido < totalVenta)
                {
                    MessageBox.Show("El efectivo recibido es menor al total de la venta.", "Pago Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal cambio = efectivoRecibido - totalVenta;

                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();

                    // 1. Insertar Cabecera de Venta
                    string queryVenta = @"INSERT INTO Ventas (IdEmpleado, FechaHora, Total, TipoPago) 
                                          OUTPUT INSERTED.IdVenta 
                                          VALUES (@idEmp, GETDATE(), @total, 'Efectivo')";

                    int idVenta = 0;
                    using (SqlCommand cmdV = new SqlCommand(queryVenta, con))
                    {
                        cmdV.Parameters.AddWithValue("@idEmp", EmpleadoSesion.IdEmpleado);
                        cmdV.Parameters.AddWithValue("@total", totalVenta);
                        idVenta = (int)cmdV.ExecuteScalar();
                    }

                    // 2. Registrar Detalle y Descontar Stock
                    foreach (DataRow row in dtCarrito.Rows)
                    {
                        int idProd = Convert.ToInt32(row["IdProducto"]);
                        int cantidad = Convert.ToInt32(row["Cantidad"]);
                        decimal precio = Convert.ToDecimal(row["Precio"]);
                        decimal subtotal = Convert.ToDecimal(row["Subtotal"]);

                        string queryDet = @"INSERT INTO DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal) 
                                            VALUES (@idV, @idP, @cant, @precio, @sub)";
                        using (SqlCommand cmdD = new SqlCommand(queryDet, con))
                        {
                            cmdD.Parameters.AddWithValue("@idV", idVenta);
                            cmdD.Parameters.AddWithValue("@idP", idProd);
                            cmdD.Parameters.AddWithValue("@cant", cantidad);
                            cmdD.Parameters.AddWithValue("@precio", precio);
                            cmdD.Parameters.AddWithValue("@sub", subtotal);
                            cmdD.ExecuteNonQuery();
                        }

                        string queryStock = "UPDATE Productos SET Stock = Stock - @cant WHERE IdProducto = @idP";
                        using (SqlCommand cmdS = new SqlCommand(queryStock, con))
                        {
                            cmdS.Parameters.AddWithValue("@cant", cantidad);
                            cmdS.Parameters.AddWithValue("@idP", idProd);
                            cmdS.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show($"¡Venta procesada con éxito!\nCambio a devolver al cliente: Bs {cambio:N2}", "Venta Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                dtCarrito.Clear();
                CalcularTotal();
                txtEfectivoRecibido.Clear();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al procesar el pago: " + ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Está seguro de salir de la aplicación?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
