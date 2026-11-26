using System.Text.Json.Serialization;
using EventosApi.Models.Enuns;

namespace EventosApi.Models
{
    // Tabela de ligação Evento <-> Participante, com dados próprios (status e presença)
    public class Inscricao
    {
        public int Id { get; set; }

        public int EventoId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Evento? Evento { get; set; }

        public int ParticipanteId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Participante? Participante { get; set; }

        public DateTime DataInscricao { get; set; }
        public StatusInscricaoEnum Status { get; set; } = StatusInscricaoEnum.Confirmada;

        // Check-in no dia do evento
        public bool Presente { get; set; }
    }
}
