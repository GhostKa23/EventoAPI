using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventosApi.Models
{
    public class Local
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Endereco { get; set; } = string.Empty;

        [Required]
        public string Cidade { get; set; } = string.Empty;

        [Range(1, 1000000)]
        public int Capacidade { get; set; }

        [JsonIgnore]
        public List<Evento> Eventos { get; set; } = new();
    }
}
