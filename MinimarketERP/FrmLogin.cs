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
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            // Cargar la lista desplegable de roles al iniciar la pantalla
            cmbRol.Items.Clear();
            cmbRol.Items.Add("Jefe");
            cmbRol.Items.Add("Almacenero");
            cmbRol.Items.Add("Cajero");
            cmbRol.SelectedIndex = 0; // Selecciona 'Jefe' por defecto
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            // 1. Validar que no existan campos vacíos
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                MessageBox.Show("Por favor ingrese su usuario y contraseña.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string rolSeleccionado = cmbRol.SelectedItem.ToString();

            try
            {
                using (SqlConnection con = Conexion.ObtenerConexion())
                {
                    con.Open();

                    // Consulta SQL que valida Usuario, Contraseña y Rol activo
                    string query = @"SELECT IdEmpleado, NombreCompleto, Cargo, Rol 
                                     FROM Empleados 
                                     WHERE Usuario = @user 
                                       AND Contrasena = @pass 
                                       AND Rol = @rol 
                                       AND Activo = 1";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@user", txtUsuario.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtContrasena.Text.Trim());
                        cmd.Parameters.AddWithValue("@rol", rolSeleccionado);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                // 2. Guardar la sesión global del empleado autenticado
                                EmpleadoSesion.IdEmpleado = Convert.ToInt32(dr["IdEmpleado"]);
                                EmpleadoSesion.NombreCompleto = dr["NombreCompleto"].ToString();
                                EmpleadoSesion.Cargo = dr["Cargo"].ToString();
                                EmpleadoSesion.Rol = dr["Rol"].ToString();

                                MessageBox.Show("¡Bienvenido al sistema, " + EmpleadoSesion.NombreCompleto + "!\nRol activo: " + EmpleadoSesion.Rol,
                                                "Acceso Concedido", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                this.Hide();

                                // 3. REDIRECCIÓN DIRECTA A LA VENTANA EXCLUSIVA DEL ROL
                                if (EmpleadoSesion.Rol == "Jefe")
                                {
                                    FrmMenuJefe menuJefe = new FrmMenuJefe();
                                    menuJefe.Show();
                                }
                                else if (EmpleadoSesion.Rol == "Almacenero")
                                {
                                    FrmMenuAlmacenero menuAlmacen = new FrmMenuAlmacenero();
                                    menuAlmacen.Show();
                                }
                                else if (EmpleadoSesion.Rol == "Cajero")
                                {
                                    FrmMenuCajero menuCajero = new FrmMenuCajero();
                                    menuCajero.Show();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Credenciales incorrectas o el usuario no corresponde al rol de '" + rolSeleccionado + "'.",
                                                "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtContrasena.Clear();
                                txtContrasena.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con SQL Server: " + ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
