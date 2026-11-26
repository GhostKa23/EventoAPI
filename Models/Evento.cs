using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EventosApi.Models.Enuns;

namespace EventosApi.Models
{
    public class Evento
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        [Range(0, 1000000)]
        public decimal Preco { get; set; }

        [Range(1, 1000000)]
        public int CapacidadeMaxima { get; set; }

        public StatusEventoEnum Status { get; set; } = StatusEventoEnum.Rascunho;
        public DateTime DataCriacao { get; set; }

        // Todo evento pertence a um Organizador e acontece em um Local
        public int OrganizadorId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Organizador? Organizador { get; set; }

        public int LocalId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Local? Local { get; set; }

        [JsonIgnore]
        public List<Inscricao> Inscricoes { get; set; } = new();
    }
}
