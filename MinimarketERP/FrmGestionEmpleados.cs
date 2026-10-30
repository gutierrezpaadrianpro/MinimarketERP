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
    public partial class FrmGestionEmpleados : Form
    {
        private int idEmpleadoSeleccionado = 0; // Para saber qué empleado se está editando
        public FrmGestionEmpleados()
        {
            InitializeComponent();
        }

        private void FrmGestionEmpleados_Load(object sender, EventArgs e)
        {
            CargarRoles();
            CargarEmpleados();
            chkActivo.Checked = porDefectoActivo(); // Opcional por defecto
        }
        private bool porDefectoActivo() => true;

        private void CargarRoles()
        {
            // Si 'cmbRol' es un ComboBox, cargamos los roles fijos
            if (cmbRol is ComboBox combo)
            {
                combo.Items.Clear();
                combo.Items.Add("Jefe");
                combo.Items.Add("Almacenero");
                combo.Items.Add("Cajero");
                combo.SelectedIndex = 0;
            }
        }

        // Cargar todos los empleados en la grilla (DataGridView)
        private void CargarEmpleados()
        {
            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();
                    string query = @"SELECT IdEmpleado, CI, NombreCompleto, Cargo, Rol, Usuario, Contrasena, Activo 
                                     FROM Empleados 
                                     ORDER BY IdEmpleado DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvEmpleados.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la lista de empleados: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCI.Text) ||
                string.IsNullOrWhiteSpace(txtNombreCompleto.Text) ||
                string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                MessageBox.Show("Por favor complete los campos obligatorios (CI, Nombre Completo, Usuario y Contraseña).", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();
                    string query = @"INSERT INTO Empleados (CI, NombreCompleto, Cargo, Rol, Usuario, Contrasena, Activo) 
                                     VALUES (@ci, @nombre, @cargo, @rol, @user, @pass, @activo)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ci", txtCI.Text.Trim());
                        cmd.Parameters.AddWithValue("@nombre", txtNombreCompleto.Text.Trim());
                        cmd.Parameters.AddWithValue("@cargo", string.IsNullOrWhiteSpace(txtCargo.Text) ? "General" : txtCargo.Text.Trim());
                        cmd.Parameters.AddWithValue("@rol", cmbRol.Text.Trim());
                        cmd.Parameters.AddWithValue("@user", txtUsuario.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtContrasena.Text.Trim());
                        cmd.Parameters.AddWithValue("@activo", chkActivo.Checked ? 1 : 0);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("¡Empleado registrado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LimpiarCampos();
                        CargarEmpleados();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar el empleado (¿El usuario o CI ya existen?): " + ex.Message, "Error de SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (idEmpleadoSeleccionado == 0)
            {
                MessageBox.Show("Por favor seleccione un empleado de la tabla para modificar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();
                    string query = @"UPDATE Empleados 
                                     SET CI = @ci, NombreCompleto = @nombre, Cargo = @cargo, 
                                         Rol = @rol, Usuario = @user, Contrasena = @pass, Activo = @activo 
                                     WHERE IdEmpleado = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", idEmpleadoSeleccionado);
                        cmd.Parameters.AddWithValue("@ci", txtCI.Text.Trim());
                        cmd.Parameters.AddWithValue("@nombre", txtNombreCompleto.Text.Trim());
                        cmd.Parameters.AddWithValue("@cargo", txtCargo.Text.Trim());
                        cmd.Parameters.AddWithValue("@rol", cmbRol.Text.Trim());
                        cmd.Parameters.AddWithValue("@user", txtUsuario.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtContrasena.Text.Trim());
                        cmd.Parameters.AddWithValue("@activo", chkActivo.Checked ? 1 : 0);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("¡Datos del empleado actualizados correctamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LimpiarCampos();
                        CargarEmpleados();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al modificar el empleado: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (idEmpleadoSeleccionado == 0)
            {
                MessageBox.Show("Por favor seleccione un empleado de la tabla para eliminar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("¿Está seguro de eliminar/dar de baja a este empleado?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection con = Conexion.ObtenerConexion())
                    {
                        con.Open();
                        string query = "UPDATE Empleados SET Activo = 0 WHERE IdEmpleado = @id";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@id", idEmpleadoSeleccionado);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Empleado dado de baja correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LimpiarCampos();
                            CargarEmpleados();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvEmpleados_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvEmpleados.Rows[e.RowIndex];
                idEmpleadoSeleccionado = Convert.ToInt32(row.Cells["IdEmpleado"].Value);
                txtCI.Text = row.Cells["CI"].Value.ToString();
                txtNombreCompleto.Text = row.Cells["NombreCompleto"].Value.ToString();
                txtCargo.Text = row.Cells["Cargo"].Value.ToString();
                cmbRol.Text = row.Cells["Rol"].Value.ToString();
                txtUsuario.Text = row.Cells["Usuario"].Value.ToString();
                txtContrasena.Text = row.Cells["Contrasena"].Value.ToString();
                chkActivo.Checked = Convert.ToBoolean(row.Cells["Activo"].Value);
            }
        }
        private void LimpiarCampos()
        {
            idEmpleadoSeleccionado = 0;
            txtCI.Clear();
            txtNombreCompleto.Clear();
            txtCargo.Clear();
            txtUsuario.Clear();
            txtContrasena.Clear();
            chkActivo.Checked = true;
            if (cmbRol is ComboBox combo && combo.Items.Count > 0) combo.SelectedIndex = 0;
            txtNombreCompleto.Focus();
        }

        // Botones del menú lateral
        private void btnRegresar_Click(object sender, EventArgs e)
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
