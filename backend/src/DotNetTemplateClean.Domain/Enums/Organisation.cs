
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DotNetTemplateClean.Domain;

public enum FonctionDomaine
{
    Qualité,
    Métrologie,
    Management,

    [Display(Name = "Analyse chimique")]
    AnalyseChimique,
    RadioLogie,
    Informatique,

    [Display(Name = "Management laboratoire")]
    ManagementLaboratoire,
}

public enum TypeFonction
{
    Management,
    Support,
    Technique,
}

public enum  StatutPersonnel
{
    Stagiaire,
    Titulaire,
    [Display(Name = "En disponibilité")]
    EnDisponibilité,

    [Display(Name = "En longue maladie")]
    LongueMaladie,
}

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()
                        ?.Name ?? enumValue.ToString();
    }
}
