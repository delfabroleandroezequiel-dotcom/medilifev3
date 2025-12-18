using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models;

public enum CondicionAnteIVA
{
    [Display(Name = "Inscripto")]
    Inscripto = 0,

    [Display(Name = "Excento")]
    Excento = 1,

    [Display(Name = "Consumidor final")]
    ConsumidorFinal = 2,

    [Display(Name = "Particular")]
    Particular = 3,

    [Display(Name = "Monotributista")]
    Monotributista = 4
}
