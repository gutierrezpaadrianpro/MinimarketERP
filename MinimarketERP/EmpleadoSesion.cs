namespace MinimarketERP
{
    public static class EmpleadoSesion
    {
        public static int IdEmpleado { get; set; }
        public static string NombreCompleto { get; set; }
        public static string Cargo { get; set; }
        public static string Rol { get; set; } // 'Jefe', 'Almacenero', 'Cajero'
    }
}
