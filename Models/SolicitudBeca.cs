using System.ComponentModel.DataAnnotations;

namespace Becas.Models
{
    public class SolicitudBeca
    {
        [Key]
        [Display(Name = "Nombre del estudiante")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        public string NombreEstudiante { get; set; } = string.Empty;

        [Display(Name = "Cédula")]
        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "La cédula debe tener exactamente 9 dígitos.")]
        public string Cedula { get; set; } = string.Empty;

        [Display(Name = "Cantón")]
        [Required(ErrorMessage = "El cantón es obligatorio.")]
        public string Canton { get; set; } = string.Empty;

        [Display(Name = "¿Tuvo beca anterior?")]
        public bool TuvoBecaAnterior { get; set; }

        [Display(Name = "Detalle de la situación socioeconómica")]
        [Required(ErrorMessage = "El detalle es obligatorio.")]
        [MinLength(20, ErrorMessage = "El detalle debe tener al menos 20 caracteres.")]
        public string Detalle { get; set; } = string.Empty;
    }
}

