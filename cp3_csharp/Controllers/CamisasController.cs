using cp3_csharp.Data;
using cp3_csharp.Models.Database;
using cp3_csharp.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cp3_csharp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CamisasController : ControllerBase
{
    private readonly LojaContext _db;

    public CamisasController(LojaContext db)
    {
        _db = db;
    }

    [HttpGet("todas")]
    public async Task<ActionResult<IEnumerable<Camisa>>> GetAll()
    {
        var lista = await _db.Camisas.AsNoTracking()
                                     .ToListAsync();
        return Ok(lista);
    }


    [HttpPost("nova")]
    public async Task<ActionResult> Create([FromBody] CamisaCreateDTO dto)
    {
        if (dto.Preco < 0) return BadRequest("Preço não pode ser negativo.");
        if (dto.QuantidadeEmEstoque < 0 || dto.QuantidadeMinimaAlerta < 0)
            return BadRequest("Quantidade não pode ser negativa.");

        var bandaExiste = await _db.Bandas.AsNoTracking().AnyAsync(b => b.Id == dto.BandaId);
        if (!bandaExiste) return BadRequest($"Banda {dto.BandaId} não encontrada.");

        var entity = new Camisa
        {
            BandaId = dto.BandaId,
            Tamanho = dto.Tamanho,
            Modelo = dto.Modelo,
            Cor = dto.Cor,
            Tecido = dto.Tecido,
            Preco = dto.Preco,
            QuantidadeEmEstoque = dto.QuantidadeEmEstoque,
            QuantidadeMinimaAlerta = dto.QuantidadeMinimaAlerta
        };

        _db.Camisas.Add(entity);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            return Conflict("Já existe uma camisa para essa Banda/Modelo/Tamanho.");
        }

        return Created();
    }


    [HttpPost("atualiza/{id:int}")]
    public async Task<ActionResult> Atualiza(int id, [FromBody] CamisaUpdateDTO dto)
    {
        var entity = await _db.Camisas.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return NotFound();

        if (dto.BandaId.HasValue)
        {
            var bandaExiste = await _db.Bandas.AsNoTracking().AnyAsync(b => b.Id == dto.BandaId.Value);
            if (!bandaExiste) return BadRequest($"Banda {dto.BandaId} não encontrada.");
            entity.BandaId = dto.BandaId.Value;
        }

        if (dto.Tamanho.HasValue) entity.Tamanho = dto.Tamanho.Value;
        if (!string.IsNullOrWhiteSpace(dto.Modelo)) entity.Modelo = dto.Modelo.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Cor)) entity.Cor = dto.Cor.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Tecido)) entity.Tecido = dto.Tecido.Trim();

        if (dto.Preco.HasValue)
        {
            if (dto.Preco.Value < 0) return BadRequest("Preço não pode ser negativo.");
            entity.Preco = dto.Preco.Value;
        }

        if (dto.QuantidadeEmEstoque.HasValue)
        {
            if (dto.QuantidadeEmEstoque.Value < 0) return BadRequest("Quantidade não pode ser negativa.");
            entity.QuantidadeEmEstoque = dto.QuantidadeEmEstoque.Value;
        }

        if (dto.QuantidadeMinimaAlerta.HasValue)
        {
            if (dto.QuantidadeMinimaAlerta.Value < 0) return BadRequest("Quantidade não pode ser negativa.");
            entity.QuantidadeMinimaAlerta = dto.QuantidadeMinimaAlerta.Value;
        }

        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            return Conflict("Já existe uma camisa para essa Banda/Modelo/Tamanho.");
        }

        return Ok(entity);
    }


    [HttpPost("remove/{id:int}")]
    public async Task<ActionResult> Remove(int id)
    {
        var entity = await _db.Camisas.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return NotFound();

        _db.Camisas.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }


    [HttpGet("estoque-critico")]
    public async Task<ActionResult<EstoqueCriticoDTO>> EstoqueCritico()
    {
        try
        {
            // Buscar camisas com estoque abaixo do mínimo
            var camisasCriticas = await _db.Camisas
                .Where(c => c.QuantidadeEmEstoque < c.QuantidadeMinimaAlerta)
                .AsNoTracking()
                .ToListAsync();

            // Calcular quantidade total de camisas em estoque crítico
            int quantidadeTotal = camisasCriticas.Sum(c => c.QuantidadeEmEstoque);

            // Calcular quantidade de tipos de camisas em estoque crítico
            int quantidadeTipos = camisasCriticas.Count;

            // Montar DTO
            var dto = new EstoqueCriticoDTO
            {
                Camisas = camisasCriticas,
                QuantidadeTotal = quantidadeTotal,
                QuantidadeTipos = quantidadeTipos
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }


}
