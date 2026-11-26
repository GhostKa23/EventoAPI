using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosApi.Data;
using EventosApi.Models;
using EventosApi.Models.Enuns;

namespace EventosApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrganizadoresController : ControllerBase
    {
        private readonly DataContext _context;

        public OrganizadoresController(DataContext context)
        {
            _context = context;
        }

        // GET /Organizadores?busca=maria
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? busca)
        {
            var query = _context.TB_ORGANIZADORES.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(o => o.Nome.ToLower().Contains(busca.ToLower()));

            return Ok(await query.OrderBy(o => o.Nome).ToListAsync());
        }

        // GET /Organizadores/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var organizador = await _context.TB_ORGANIZADORES.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organizador == null)
                return NotFound(new { mensagem = $"Organizador {id} não encontrado." });

            return Ok(organizador);
        }

        // GET /Organizadores/1/Eventos  -> todos os eventos do organizador
        [HttpGet("{id:int}/Eventos")]
        public async Task<IActionResult> GetEventos(int id)
        {
            if (!await _context.TB_ORGANIZADORES.AnyAsync(o => o.Id == id))
                return NotFound(new { mensagem = $"Organizador {id} não encontrado." });

            var eventos = await _context.TB_EVENTOS.AsNoTracking()
                .Include(e => e.Local)
                .Where(e => e.OrganizadorId == id)
                .OrderBy(e => e.DataInicio)
                .ToListAsync();

            return Ok(eventos);
        }

        // GET /Organizadores/1/Resumo  -> painel do organizador
        [HttpGet("{id:int}/Resumo")]
        public async Task<IActionResult> GetResumo(int id)
        {
            var organizador = await _context.TB_ORGANIZADORES.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organizador == null)
                return NotFound(new { mensagem = $"Organizador {id} não encontrado." });

            var eventos = await _context.TB_EVENTOS.AsNoTracking()
                .Include(e => e.Inscricoes)
                .Where(e => e.OrganizadorId == id)
                .ToListAsync();

            var confirmadas = eventos
                .SelectMany(e => e.Inscricoes)
                .Where(i => i.Status == StatusInscricaoEnum.Confirmada)
                .ToList();

            var proximo = eventos
                .Where(e => e.Status == StatusEventoEnum.Publicado && e.DataInicio > DateTime.UtcNow)
                .OrderBy(e => e.DataInicio)
                .Select(e => new { e.Id, e.Titulo, e.DataInicio })
                .FirstOrDefault();

            return Ok(new
            {
                organizadorId = organizador.Id,
                nome = organizador.Nome,
                totalEventos = eventos.Count,
                eventosPorStatus = eventos
                    .GroupBy(e => e.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                inscricoesConfirmadas = confirmadas.Count,
                presentes = confirmadas.Count(i => i.Presente),
                proximoEvento = proximo
            });
        }

        // POST /Organizadores
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Organizador novo)
        {
            if (await _context.TB_ORGANIZADORES.AnyAsync(o => o.Email.ToLower() == novo.Email.ToLower()))
                return BadRequest(new { mensagem = "Já existe um organizador com este e-mail." });

            novo.Id = 0;
            novo.DataCadastro = DateTime.UtcNow;
            novo.Eventos = new();

            _context.TB_ORGANIZADORES.Add(novo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = novo.Id }, novo);
        }

        // PUT /Organizadores/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Organizador dados)
        {
            var organizador = await _context.TB_ORGANIZADORES.FirstOrDefaultAsync(o => o.Id == id);
            if (organizador == null)
                return NotFound(new { mensagem = $"Organizador {id} não encontrado." });

            if (await _context.TB_ORGANIZADORES.AnyAsync(o =>
                    o.Id != id && o.Email.ToLower() == dados.Email.ToLower()))
                return BadRequest(new { mensagem = "Já existe um organizador com este e-mail." });

            organizador.Nome = dados.Nome;
            organizador.Email = dados.Email;
            organizador.Telefone = dados.Telefone;
            organizador.Empresa = dados.Empresa;

            await _context.SaveChangesAsync();
            return Ok(organizador);
        }

        // DELETE /Organizadores/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var organizador = await _context.TB_ORGANIZADORES.FirstOrDefaultAsync(o => o.Id == id);
            if (organizador == null)
                return NotFound(new { mensagem = $"Organizador {id} não encontrado." });

            if (await _context.TB_EVENTOS.AnyAsync(e => e.OrganizadorId == id))
                return BadRequest(new { mensagem = "Não é possível remover um organizador que possui eventos." });

            _context.TB_ORGANIZADORES.Remove(organizador);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
