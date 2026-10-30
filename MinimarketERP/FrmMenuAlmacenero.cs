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
    public partial class FrmMenuAlmacenero : Form
    {
        public FrmMenuAlmacenero()
        {
            InitializeComponent();
        }

        private void FrmMenuAlmacenero_Load(object sender, EventArgs e)
        {
            // Cargar automáticamente los datos del Almacenero activo en pantalla
            lblUsuario.Text = "Usuario: " + EmpleadoSesion.NombreCompleto;
            lblRol.Text = "Rol: " + EmpleadoSesion.Rol;
        }

        private void btnProductos_Click(object sender, EventArgs e)
        {
            FrmRegistrarCompra frmCompra = new FrmRegistrarCompra();
            frmCompra.ShowDialog();
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            FormProductos frmProd = new FormProductos();
            frmProd.ShowDialog();
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            FrmRegistrarMerma frmMerma = new FrmRegistrarMerma();
            frmMerma.ShowDialog();
        }

        private void btnSesion_Click(object sender, EventArgs e)
        {
            FrmVerStockYLotes frmStock = new FrmVerStockYLotes();
            frmStock.ShowDialog();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea cerrar la sesión actual?", "Cerrar Sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FrmLogin login = new FrmLogin();
                login.Show();
                this.Close();
            }
        }

        private void btngerente_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Está seguro de salir de la aplicación?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
