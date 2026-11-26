using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventosApi.Models
{
    public class Participante
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Telefone { get; set; }

        [JsonIgnore]
        public List<Inscricao> Inscricoes { get; set; } = new();
    }
}
