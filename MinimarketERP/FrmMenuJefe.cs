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
    public partial class FrmMenuJefe : Form
    {
        public FrmMenuJefe()
        {
            InitializeComponent();
        }

        private void FrmMenuJefe_Load(object sender, EventArgs e)
        {
            // Cargar automáticamente los datos del Jefe desde la sesión activa
            lblUsuario.Text = "Usuario: " + EmpleadoSesion.NombreCompleto;
            lblRol.Text = "Rol: " + EmpleadoSesion.Rol;
        }

        private void btnVerInventario_Click(object sender, EventArgs e)
        {
            FormProductos frmProd = new FormProductos();
            frmProd.ShowDialog(); // Abre la ventana de inventario/productos
        }

        private void btnGestionEmpleados_Click(object sender, EventArgs e)
        {
            FrmGestionEmpleados frmEmp = new FrmGestionEmpleados();
            frmEmp.ShowDialog(); // Abre la ventana de gestión de empleados
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            FrmReportes frmRep = new FrmReportes();
            frmRep.ShowDialog();
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea cerrar la sesión actual?", "Cerrar Sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FrmLogin login = new FrmLogin();
                login.Show();
                this.Close();
            }
        }
    }
}
