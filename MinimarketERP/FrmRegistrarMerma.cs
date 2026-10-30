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
    public partial class FrmRegistrarMerma : Form
    {
        public FrmRegistrarMerma()
        {
            InitializeComponent();
        }

        private void FrmRegistrarMerma_Load(object sender, EventArgs e)
        {
            CargarMotivos();
        }
        private void CargarMotivos()
        {
            cmbMotivo.Items.Clear();
            cmbMotivo.Items.Add("Vencimiento");
            cmbMotivo.Items.Add("Producto Dañado");
            cmbMotivo.Items.Add("Empaque Roto");
            cmbMotivo.Items.Add("Pérdida / Extravío");
            cmbMotivo.SelectedIndex = 0;
        }

        private void btnGuardarMerma_Click(object sender, EventArgs e)
        {
            string codigoProducto = txtCodigoProducto.Text.Trim();
            if (string.IsNullOrWhiteSpace(codigoProducto))
            {
                MessageBox.Show("Por favor ingrese el código del producto.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigoProducto.Focus();
                return;
            }

            int cantidadPerdida = 0;
            if (!int.TryParse(txtCantidadPerdida.Text, out cantidadPerdida) || cantidadPerdida <= 0)
            {
                MessageBox.Show("Ingrese una cantidad perdida válida mayor a cero.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidadPerdida.Focus();
                return;
            }

            string motivo = cmbMotivo.SelectedItem.ToString();
            string observaciones = txtObservaciones.Text.Trim();

            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();

                    // Buscar el producto por código y obtener su Id y Stock actual
                    int idProducto = 0;
                    int stockActual = 0;
                    string queryBuscar = "SELECT IdProducto, Stock FROM Productos WHERE Codigo = @codigo AND Activo = 1";

                    using (SqlCommand cmdB = new SqlCommand(queryBuscar, con))
                    {
                        cmdB.Parameters.AddWithValue("@codigo", codigoProducto);
                        using (SqlDataReader dr = cmdB.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                idProducto = Convert.ToInt32(dr["IdProducto"]);
                                stockActual = Convert.ToInt32(dr["Stock"]);
                            }
                        }
                    }

                    if (idProducto == 0)
                    {
                        MessageBox.Show("No se encontró ningún producto activo con ese código.", "No Encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (cantidadPerdida > stockActual)
                    {
                        MessageBox.Show($"La cantidad perdida ({cantidadPerdida}) supera el stock disponible en inventario ({stockActual}).",
                                        "Stock Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Registrar la Merma en la Base de Datos
                    string queryMerma = @"INSERT INTO Mermas (IdProducto, IdEmpleado, Cantidad, Motivo, Observaciones, FechaHora) 
                                          VALUES (@idP, @idEmp, @cant, @motivo, @obs, GETDATE())";

                    using (SqlCommand cmdM = new SqlCommand(queryMerma, con))
                    {
                        cmdM.Parameters.AddWithValue("@idP", idProducto);
                        cmdM.Parameters.AddWithValue("@idEmp", EmpleadoSesion.IdEmpleado);
                        cmdM.Parameters.AddWithValue("@cant", cantidadPerdida);
                        cmdM.Parameters.AddWithValue("@motivo", motivo);
                        cmdM.Parameters.AddWithValue("@obs", string.IsNullOrWhiteSpace(observaciones) ? "S/N" : observaciones);
                        cmdM.ExecuteNonQuery();
                    }

                    // Descontar Stock del producto
                    string queryStock = "UPDATE Productos SET Stock = Stock - @cant WHERE IdProducto = @idP";
                    using (SqlCommand cmdS = new SqlCommand(queryStock, con))
                    {
                        cmdS.Parameters.AddWithValue("@cant", cantidadPerdida);
                        cmdS.Parameters.AddWithValue("@idP", idProducto);
                        cmdS.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("¡Merma registrada exitosamente y stock descontado del inventario!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la merma: " + ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }
        private void LimpiarCampos()
        {
            txtCodigoProducto.Clear();
            txtCantidadPerdida.Clear();
            txtObservaciones.Clear();
            if (cmbMotivo.Items.Count > 0) cmbMotivo.SelectedIndex = 0;
            txtCodigoProducto.Focus();
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
