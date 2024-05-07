using MVCVentas.Controllers;

namespace MVCVentas.Interfaces
{
    public interface IVentasControllerFactory
    {
        VentasController Create();
    }
}
