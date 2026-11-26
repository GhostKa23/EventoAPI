using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosApi.Data;
using EventosApi.Models;
using EventosApi.Models.Enuns;

namespace EventosApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InscricoesController : ControllerBase
    {
        private readonly DataContext _context;

        public InscricoesController(DataContext context)
        {
            _context = context;
        }

        // GET /Inscricoes?eventoId=1&participanteId=2&status=Confirmada
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? eventoId,
            [FromQuery] int? participanteId,
            [FromQuery] StatusInscricaoEnum? status)
        {
            var query = _context.TB_INSCRICOES.AsNoTracking()
                .Include(i => i.Evento)
                .Include(i => i.Participante)
                .AsQueryable();

            if (eventoId.HasValue)
                query = query.Where(i => i.EventoId == eventoId.Value);

            if (participanteId.HasValue)
                query = query.Where(i => i.ParticipanteId == participanteId.Value);

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            return Ok(await query.OrderBy(i => i.Id).ToListAsync());
        }

        // GET /Inscricoes/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var inscricao = await _context.TB_INSCRICOES.AsNoTracking()
                .Include(i => i.Evento)
                .Include(i => i.Participante)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inscricao == null)
                return NotFound(new { mensagem = $"Inscrição {id} não encontrada." });

            return Ok(inscricao);
        }

        // POST /Inscricoes  { "eventoId": 1, "participanteId": 4 }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Inscricao nova)
        {
            var evento = await _context.TB_EVENTOS.FirstOrDefaultAsync(e => e.Id == nova.EventoId);
            if (evento == null)
                return BadRequest(new { mensagem = $"Evento {nova.EventoId} não existe." });

            if (evento.Status != StatusEventoEnum.Publicado)
                return BadRequest(new { mensagem = $"O evento não aceita inscrições (status: {evento.Status})." });

            if (!await _context.TB_PARTICIPANTES.AnyAsync(p => p.Id == nova.ParticipanteId))
                return BadRequest(new { mensagem = $"Participante {nova.ParticipanteId} não existe." });

            if (await _context.TB_INSCRICOES.AnyAsync(i =>
                    i.EventoId == nova.EventoId && i.ParticipanteId == nova.ParticipanteId))
                return BadRequest(new { mensagem = "Este participante já está inscrito neste evento." });

            var confirmadas = await _context.TB_INSCRICOES.CountAsync(i =>
                i.EventoId == nova.EventoId && i.Status == StatusInscricaoEnum.Confirmada);

            if (confirmadas >= evento.CapacidadeMaxima)
                return BadRequest(new { mensagem = "Evento lotado: não há vagas disponíveis." });

            nova.Id = 0;
            nova.DataInscricao = DateTime.UtcNow;
            nova.Status = StatusInscricaoEnum.Confirmada;
            nova.Presente = false;
            nova.Evento = null;
            nova.Participante = null;

            _context.TB_INSCRICOES.Add(nova);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = nova.Id }, nova);
        }

        // PUT /Inscricoes/1  { "status": "Confirmada", "presente": true }
        // Só status e presença podem mudar; evento e participante são fixos.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Inscricao dados)
        {
            var inscricao = await _context.TB_INSCRICOES
                .Include(i => i.Evento)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inscricao == null)
                return NotFound(new { mensagem = $"Inscrição {id} não encontrada." });

            if (!Enum.IsDefined(typeof(StatusInscricaoEnum), dados.Status))
                return BadRequest(new { mensagem = "Status inválido. Use Confirmada ou Cancelada." });

            if (dados.Status == StatusInscricaoEnum.Cancelada && dados.Presente)
                return BadRequest(new { mensagem = "Uma inscrição cancelada não pode ter presença registrada." });

            // Reativar uma inscrição cancelada exige vaga
            if (inscricao.Status == StatusInscricaoEnum.Cancelada && dados.Status == StatusInscricaoEnum.Confirmada)
            {
                var confirmadas = await _context.TB_INSCRICOES.CountAsync(i =>
                    i.EventoId == inscricao.EventoId && i.Status == StatusInscricaoEnum.Confirmada);

                if (confirmadas >= inscricao.Evento!.CapacidadeMaxima)
                    return BadRequest(new { mensagem = "Evento lotado: não há vagas para reativar a inscrição." });
            }

            inscricao.Status = dados.Status;
            inscricao.Presente = dados.Presente;

            await _context.SaveChangesAsync();
            return Ok(inscricao);
        }

        // DELETE /Inscricoes/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var inscricao = await _context.TB_INSCRICOES.FirstOrDefaultAsync(i => i.Id == id);
            if (inscricao == null)
                return NotFound(new { mensagem = $"Inscrição {id} não encontrada." });

            _context.TB_INSCRICOES.Remove(inscricao);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
