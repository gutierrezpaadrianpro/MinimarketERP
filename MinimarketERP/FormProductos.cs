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
    public partial class FormProductos : Form
    {
        public FormProductos()
        {
            InitializeComponent();
        }
        private void FormProductos_Load(object sender, EventArgs e)
        {
            CargarCategorias();
        }
        private void CargarCategorias()
        {
            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add("Bebidas");
            cmbCategoria.Items.Add("Abarrotes");
            cmbCategoria.Items.Add("Lácteos");
            cmbCategoria.Items.Add("Limpieza");
            cmbCategoria.Items.Add("Snacks y Golosinas");
            cmbCategoria.Items.Add("General");
            cmbCategoria.SelectedIndex = 5; // Selecciona "General" por defecto
        }

        private void btnRegistrarProducto_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtPrecioVenta.Text))
            {
                MessageBox.Show("Por favor complete los campos obligatorios: Código de barras, Nombre y Precio de Venta.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigo.Focus();
                return;
            }

            decimal precioVenta = 0;
            if (!decimal.TryParse(txtPrecioVenta.Text, out precioVenta) || precioVenta <= 0)
            {
                MessageBox.Show("Ingrese un precio de venta válido mayor a cero.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecioVenta.Focus();
                return;
            }

            int stockInicial = 0;
            int.TryParse(txtStockInicial.Text, out stockInicial);

            string codigoBarras = txtCodigo.Text.Trim();
            string nombreProducto = txtNombre.Text.Trim();
            string categoria = cmbCategoria.SelectedItem?.ToString() ?? "General";

            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();
                    string query = @"INSERT INTO Productos (Codigo, Nombre, Categoria, PrecioCosto, PrecioVenta, Stock, Activo) 
                                     VALUES (@codigo, @nombre, @categoria, 0, @venta, @stock, 1)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigoBarras);
                        cmd.Parameters.AddWithValue("@nombre", nombreProducto);
                        cmd.Parameters.AddWithValue("@categoria", categoria);
                        cmd.Parameters.AddWithValue("@venta", precioVenta);
                        cmd.Parameters.AddWithValue("@stock", stockInicial);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("¡Producto registrado exitosamente en el catálogo!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LimpiarCampos();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar el producto (¿El código de barras ya existe?): " + ex.Message, "Error de SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }
        private void LimpiarCampos()
        {
            txtCodigo.Clear();
            txtNombre.Clear();
            txtPrecioVenta.Clear();
            txtStockInicial.Clear();
            if (cmbCategoria.Items.Count > 0) cmbCategoria.SelectedIndex = 5;
            txtCodigo.Focus();
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
