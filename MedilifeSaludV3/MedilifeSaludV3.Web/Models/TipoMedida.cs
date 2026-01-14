using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models;

public enum TipoMedida
{
    [Display(Name = "ml")]
    ml = 0,
    [Display(Name = "ml2")]
    ml2 = 1,
    [Display(Name = "mm")]
    mm = 2,
    [Display(Name = "mm2")]
    mm2 = 3,
    [Display(Name = "cm")]
    cm = 4,
    [Display(Name = "cm2")]
    cm2 = 5,
    [Display(Name = "fr")]
    fr = 6
}
