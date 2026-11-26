using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosApi.Data;
using EventosApi.Models;
using EventosApi.Models.Enuns;

namespace EventosApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventosController : ControllerBase
    {
        private readonly DataContext _context;

        public EventosController(DataContext context)
        {
            _context = context;
        }

        // GET /Eventos?status=Publicado&organizadorId=1&de=2027-01-01&ate=2027-12-31&busca=workshop
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] StatusEventoEnum? status,
            [FromQuery] int? organizadorId,
            [FromQuery] DateTime? de,
            [FromQuery] DateTime? ate,
            [FromQuery] string? busca)
        {
            var query = _context.TB_EVENTOS.AsNoTracking()
                .Include(e => e.Organizador)
                .Include(e => e.Local)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            if (organizadorId.HasValue)
                query = query.Where(e => e.OrganizadorId == organizadorId.Value);

            if (de.HasValue)
                query = query.Where(e => e.DataInicio >= de.Value);

            if (ate.HasValue)
                query = query.Where(e => e.DataInicio <= ate.Value);

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(e => e.Titulo.ToLower().Contains(busca.ToLower()));

            return Ok(await query.OrderBy(e => e.DataInicio).ToListAsync());
        }

        // GET /Eventos/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var evento = await _context.TB_EVENTOS.AsNoTracking()
                .Include(e => e.Organizador)
                .Include(e => e.Local)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
                return NotFound(new { mensagem = $"Evento {id} não encontrado." });

            return Ok(evento);
        }

        // GET /Eventos/1/Inscricoes  -> lista de inscritos do evento
        [HttpGet("{id:int}/Inscricoes")]
        public async Task<IActionResult> GetInscricoes(int id)
        {
            if (!await _context.TB_EVENTOS.AnyAsync(e => e.Id == id))
                return NotFound(new { mensagem = $"Evento {id} não encontrado." });

            var inscricoes = await _context.TB_INSCRICOES.AsNoTracking()
                .Include(i => i.Participante)
                .Where(i => i.EventoId == id)
                .OrderBy(i => i.DataInscricao)
                .ToListAsync();

            return Ok(inscricoes);
        }

        // GET /Eventos/1/Resumo  -> vagas, presença e receita prevista
        [HttpGet("{id:int}/Resumo")]
        public async Task<IActionResult> GetResumo(int id)
        {
            var evento = await _context.TB_EVENTOS.AsNoTracking()
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
                return NotFound(new { mensagem = $"Evento {id} não encontrado." });

            var confirmadas = evento.Inscricoes
                .Where(i => i.Status == StatusInscricaoEnum.Confirmada)
                .ToList();

            return Ok(new
            {
                eventoId = evento.Id,
                titulo = evento.Titulo,
                status = evento.Status,
                capacidadeMaxima = evento.CapacidadeMaxima,
                inscricoesConfirmadas = confirmadas.Count,
                vagasRestantes = Math.Max(0, evento.CapacidadeMaxima - confirmadas.Count),
                presentes = confirmadas.Count(i => i.Presente),
                receitaPrevista = confirmadas.Count * evento.Preco
            });
        }

        // POST /Eventos
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Evento novo)
        {
            var erro = await Validar(novo);
            if (erro != null)
                return BadRequest(new { mensagem = erro });

            novo.Id = 0;
            novo.DataCriacao = DateTime.UtcNow;
            novo.Organizador = null;     // vínculo apenas por OrganizadorId
            novo.Local = null;           // vínculo apenas por LocalId
            novo.Inscricoes = new();

            _context.TB_EVENTOS.Add(novo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = novo.Id }, novo);
        }

        // PUT /Eventos/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Evento dados)
        {
            var evento = await _context.TB_EVENTOS
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
                return NotFound(new { mensagem = $"Evento {id} não encontrado." });

            var erro = await Validar(dados);
            if (erro != null)
                return BadRequest(new { mensagem = erro });

            var confirmadas = evento.Inscricoes.Count(i => i.Status == StatusInscricaoEnum.Confirmada);
            if (dados.CapacidadeMaxima < confirmadas)
                return BadRequest(new { mensagem = $"O evento já tem {confirmadas} inscrições confirmadas; a capacidade não pode ser menor." });

            evento.Titulo = dados.Titulo;
            evento.Descricao = dados.Descricao;
            evento.DataInicio = dados.DataInicio;
            evento.DataFim = dados.DataFim;
            evento.Preco = dados.Preco;
            evento.CapacidadeMaxima = dados.CapacidadeMaxima;
            evento.Status = dados.Status;
            evento.OrganizadorId = dados.OrganizadorId;
            evento.LocalId = dados.LocalId;

            await _context.SaveChangesAsync();
            return Ok(evento);
        }

        // POST /Eventos/1/Publicar  -> Rascunho -> Publicado
        [HttpPost("{id:int}/Publicar")]
        public async Task<IActionResult> Publicar(int id)
        {
            var evento = await _context.TB_EVENTOS.FirstOrDefaultAsync(e => e.Id == id);
            if (evento == null)
                return NotFound(new { mensagem = $"Evento {id} não encontrado." });

            if (evento.Status != StatusEventoEnum.Rascunho)
                return BadRequest(new { mensagem = $"Só é possível publicar um evento em Rascunho (status atual: {evento.Status})." });

            evento.Status = StatusEventoEnum.Publicado;
            await _context.SaveChangesAsync();
            return Ok(evento);
        }

        // POST /Eventos/1/Cancelar  -> cancela o evento e as inscrições confirmadas
        [HttpPost("{id:int}/Cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var evento = await _context.TB_EVENTOS
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
                return NotFound(new { mensagem = $"Evento {id} não encontrado." });

            if (evento.Status == StatusEventoEnum.Cancelado || evento.Status == StatusEventoEnum.Encerrado)
                return BadRequest(new { mensagem = $"O evento já está {evento.Status}." });

            evento.Status = StatusEventoEnum.Cancelado;
            foreach (var inscricao in evento.Inscricoes.Where(i => i.Status == StatusInscricaoEnum.Confirmada))
                inscricao.Status = StatusInscricaoEnum.Cancelada;

            await _context.SaveChangesAsync();
            return Ok(evento);
        }

        // DELETE /Eventos/1  (apaga também as inscrições)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var evento = await _context.TB_EVENTOS
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
                return NotFound(new { mensagem = $"Evento {id} não encontrado." });

            _context.TB_EVENTOS.Remove(evento);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Regras de negócio comuns ao POST e ao PUT
        private async Task<string?> Validar(Evento e)
        {
            if (e.DataFim <= e.DataInicio)
                return "A data de término deve ser posterior à data de início.";

            if (!Enum.IsDefined(typeof(StatusEventoEnum), e.Status))
                return "Status inválido. Use Rascunho, Publicado, Cancelado ou Encerrado.";

            // O banco em memória não valida chaves estrangeiras, então conferimos aqui
            if (!await _context.TB_ORGANIZADORES.AnyAsync(o => o.Id == e.OrganizadorId))
                return $"Organizador {e.OrganizadorId} não existe.";

            var local = await _context.TB_LOCAIS.AsNoTracking().FirstOrDefaultAsync(l => l.Id == e.LocalId);
            if (local == null)
                return $"Local {e.LocalId} não existe.";

            if (e.CapacidadeMaxima > local.Capacidade)
                return $"A capacidade do evento ({e.CapacidadeMaxima}) excede a do local \"{local.Nome}\" ({local.Capacidade}).";

            return null;
        }
    }
}
