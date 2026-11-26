using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosApi.Data;
using EventosApi.Models;

namespace EventosApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LocaisController : ControllerBase
    {
        private readonly DataContext _context;

        public LocaisController(DataContext context)
        {
            _context = context;
        }

        // GET /Locais?cidade=São Paulo
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? cidade)
        {
            var query = _context.TB_LOCAIS.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(cidade))
                query = query.Where(l => l.Cidade.ToLower() == cidade.ToLower());

            return Ok(await query.OrderBy(l => l.Nome).ToListAsync());
        }

        // GET /Locais/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var local = await _context.TB_LOCAIS.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            if (local == null)
                return NotFound(new { mensagem = $"Local {id} não encontrado." });

            return Ok(local);
        }

        // POST /Locais
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Local novo)
        {
            novo.Id = 0;
            novo.Eventos = new();

            _context.TB_LOCAIS.Add(novo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = novo.Id }, novo);
        }

        // PUT /Locais/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Local dados)
        {
            var local = await _context.TB_LOCAIS.FirstOrDefaultAsync(l => l.Id == id);
            if (local == null)
                return NotFound(new { mensagem = $"Local {id} não encontrado." });

            // Não pode reduzir a capacidade abaixo da de um evento já cadastrado
            var maiorCapacidade = await _context.TB_EVENTOS
                .Where(e => e.LocalId == id)
                .Select(e => (int?)e.CapacidadeMaxima)
                .MaxAsync();

            if (maiorCapacidade.HasValue && dados.Capacidade < maiorCapacidade.Value)
                return BadRequest(new { mensagem = $"Existe evento com capacidade {maiorCapacidade} neste local; a nova capacidade não pode ser menor." });

            local.Nome = dados.Nome;
            local.Endereco = dados.Endereco;
            local.Cidade = dados.Cidade;
            local.Capacidade = dados.Capacidade;

            await _context.SaveChangesAsync();
            return Ok(local);
        }

        // DELETE /Locais/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var local = await _context.TB_LOCAIS.FirstOrDefaultAsync(l => l.Id == id);
            if (local == null)
                return NotFound(new { mensagem = $"Local {id} não encontrado." });

            if (await _context.TB_EVENTOS.AnyAsync(e => e.LocalId == id))
                return BadRequest(new { mensagem = "Não é possível remover um local que possui eventos." });

            _context.TB_LOCAIS.Remove(local);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
