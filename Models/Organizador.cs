using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventosApi.Models
{
    // Principal usuário do sistema: quem cria e gerencia os eventos.
    public class Organizador
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Telefone { get; set; }
        public string? Empresa { get; set; }
        public DateTime DataCadastro { get; set; }

        [JsonIgnore]
        public List<Evento> Eventos { get; set; } = new();
    }
}
