using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElTrueque.Api.Models
{
    [Table("MensajesChat")]
    public class MensajeChat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ChatId { get; set; } = string.Empty; // Identificador del grupo (ej. "trueque_15")

        [Required]
        public int EmisorId { get; set; }

        [Required]
        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; } = DateTime.Now;

        public bool Leido { get; set; } = false;
    }
}
