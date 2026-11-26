using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosApi.Data;
using EventosApi.Models;

namespace EventosApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParticipantesController : ControllerBase
    {
        private readonly DataContext _context;

        public ParticipantesController(DataContext context)
        {
            _context = context;
        }

        // GET /Participantes?busca=ana
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? busca)
        {
            var query = _context.TB_PARTICIPANTES.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(p => p.Nome.ToLower().Contains(busca.ToLower()));

            return Ok(await query.OrderBy(p => p.Nome).ToListAsync());
        }

        // GET /Participantes/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var participante = await _context.TB_PARTICIPANTES.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (participante == null)
                return NotFound(new { mensagem = $"Participante {id} não encontrado." });

            return Ok(participante);
        }

        // POST /Participantes
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Participante novo)
        {
            if (await _context.TB_PARTICIPANTES.AnyAsync(p => p.Email.ToLower() == novo.Email.ToLower()))
                return BadRequest(new { mensagem = "Já existe um participante com este e-mail." });

            novo.Id = 0;
            novo.Inscricoes = new();

            _context.TB_PARTICIPANTES.Add(novo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = novo.Id }, novo);
        }

        // PUT /Participantes/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Participante dados)
        {
            var participante = await _context.TB_PARTICIPANTES.FirstOrDefaultAsync(p => p.Id == id);
            if (participante == null)
                return NotFound(new { mensagem = $"Participante {id} não encontrado." });

            if (await _context.TB_PARTICIPANTES.AnyAsync(p =>
                    p.Id != id && p.Email.ToLower() == dados.Email.ToLower()))
                return BadRequest(new { mensagem = "Já existe um participante com este e-mail." });

            participante.Nome = dados.Nome;
            participante.Email = dados.Email;
            participante.Telefone = dados.Telefone;

            await _context.SaveChangesAsync();
            return Ok(participante);
        }

        // DELETE /Participantes/1  (apaga também as inscrições dele)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var participante = await _context.TB_PARTICIPANTES
                .Include(p => p.Inscricoes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (participante == null)
                return NotFound(new { mensagem = $"Participante {id} não encontrado." });

            _context.TB_PARTICIPANTES.Remove(participante);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
