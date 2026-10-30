using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinimarketERP
{
    public partial class FrmMenuCajero : Form
    {
        public FrmMenuCajero()
        {
            InitializeComponent();
        }

        private void FrmMenuCajero_Load(object sender, EventArgs e)
        {
            // Mostrar dinámicamente los datos del cajero activo en pantalla
            lblUsuario.Text = "Usuario: " + EmpleadoSesion.NombreCompleto;
            lblRol.Text = "Rol: " + EmpleadoSesion.Rol;
        }

        private void btnRegistrarVenta_Click(object sender, EventArgs e)
        {
            FrmVentas frmVentas = new FrmVentas();
            frmVentas.ShowDialog();
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Módulo de historial de ventas del cajero en desarrollo.", "Mis Ventas", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnReportesVenta_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Módulo de reportes de venta y cierre de caja en desarrollo.", "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSesion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea cerrar la sesión actual?", "Cerrar Sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FrmLogin login = new FrmLogin();
                login.Show();
                this.Close();
            }
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
