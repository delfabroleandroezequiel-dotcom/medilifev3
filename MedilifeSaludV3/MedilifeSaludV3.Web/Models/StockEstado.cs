using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models;

public enum StockEstado
{
    [Display(Name = "En depósito")]
    D = 0,

    [Display(Name = "Entregado")]
    EN = 1,

    [Display(Name = "Usado")]
    U = 2,

    [Display(Name = "Devuelto a proveedores")]
    DP = 3
}
