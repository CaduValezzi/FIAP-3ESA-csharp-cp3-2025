using cp3_csharp.Data;
using cp3_csharp.Models.Database;
using cp3_csharp.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cp3_csharp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class OperacoesController : ControllerBase
{

    private readonly LojaContext _db;
    public OperacoesController(LojaContext db)
    {
        _db = db;
    }


    [HttpGet("GetInfo")]
    public async Task<ActionResult<IEnumerable<InfoEstoqueDTO>>> GetInfo([FromQuery] int? bandaId)
    {
         try
        {
            IQueryable<InfoEstoqueDTO> query;

            
            if (bandaId.HasValue)
            {
                query = _db.Camisas
                    .Where(c => c.BandaId == bandaId.Value)
                    .Select(c => new InfoEstoqueDTO
                    {
                        CamisaId = c.CamisaId,
                        NomeCamisa = c.NomeCamisa,
                        Tamanho = c.Tamanho,
                        Cor = c.Cor,
                        QuantidadeEmEstoque = c.QuantidadeEmEstoque,
                        BandaId = c.BandaId
                    });
            }
            else
            {
                query = _db.Camisas
                    .Select(c => new InfoEstoqueDTO
                    {
                        CamisaId = c.CamisaId,
                        NomeCamisa = c.NomeCamisa,
                        Tamanho = c.Tamanho,
                        Cor = c.Cor,
                        QuantidadeEmEstoque = c.QuantidadeEmEstoque,
                        BandaId = c.BandaId
                    });
            }

            
            var result = await query.ToListAsync();

           
            return Ok(result);
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [HttpPost("GerarEstoqueInicial")]
    public async Task<ActionResult<string>> EstoqueInicial()
    {
        try
        {
            
            string dataAtual = DateTime.Now.ToString("ddMMyy");
            string nomeArquivo = $"{dataAtual}_estoque_inicial.txt";
            string caminhoArquivo = Path.Combine("files", nomeArquivo);

            
            var camisasEstoque = await _db.Camisas
                .Include(c => c.Banda) 
                .ToListAsync();

            
            var sb = new StringBuilder();
            foreach (var camisa in camisasEstoque)
            {
                sb.AppendLine($"{camisa.CamisaId};{camisa.Banda.Nome};{camisa.QuantidadeEmEstoque}");
            }

            
            if (!Directory.Exists("files"))
            {
                Directory.CreateDirectory("files");
            }

            
            await System.IO.File.WriteAllTextAsync(caminhoArquivo, sb.ToString());

            
            return Ok(sb.ToString());
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        throw new NotImplementedException();
    }

    [HttpPost("GerarEstoqueFinal")]
    public async Task<ActionResult<string>> EstoqueFinal()
    {
        try
        {
            
            string dataAtual = DateTime.Now.ToString("ddMMyy");
            string nomeComprasArquivo = $"{dataAtual}_compras.txt";
            string caminhoComprasArquivo = Path.Combine("files", nomeComprasArquivo);

            string nomeEstoqueFinalArquivo = $"{dataAtual}_estoque_final.txt";
            string caminhoEstoqueFinalArquivo = Path.Combine("files", nomeEstoqueFinalArquivo);

            
            if (!System.IO.File.Exists(caminhoComprasArquivo))
            {
                return NotFound("Arquivo de compras não encontrado.");
            }


            var comprasTexto = await System.IO.File.ReadAllLinesAsync(caminhoComprasArquivo);

            
            var compras = new Dictionary<int, int>(); 
            foreach (var linha in comprasTexto)
            {
                var partes = linha.Split(';');
                if (partes.Length == 2 && int.TryParse(partes[0], out int camisaId) && int.TryParse(partes[1], out int quantidadeComprada))
                {
                    if (compras.ContainsKey(camisaId))
                    {
                        
                    }
                    else
                    {
                        compras[camisaId] = quantidadeComprada;
                    }
                }
            }

            
            var camisasEstoque = await _db.Camisas
                .Include(c => c.Banda) 
                .ToListAsync();

           
            var sb = new StringBuilder();
            foreach (var camisa in camisasEstoque)
            {
                
                int quantidadeFinal = camisa.QuantidadeEmEstoque;
                if (compras.ContainsKey(camisa.CamisaId))
                {
                    quantidadeFinal -= compras[camisa.CamisaId];
                }


                quantidadeFinal = Math.Max(quantidadeFinal, 0 );        
                sb.AppendLine($"{camisa.CamisaId};{camisa.Banda.Nome};{quantidadeFinal}");
                camisa.QuantidadeEmEstoque = quantidadeFinal;
            }

            
            if (!Directory.Exists("files"))
            {
                Directory.CreateDirectory("files");
            }

            
            await System.IO.File.WriteAllTextAsync(caminhoEstoqueFinalArquivo, sb.ToString());

            
            return Ok(sb.ToString());
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    
    [HttpPost("AtualizarEstoque")]
    public async Task<ActionResult> AtualizarEstoque()
    {
        try
        {
           
            string dataAtual = DateTime.Now.ToString("ddMMyy");
            string nomeEstoqueFinalArquivo = $"{dataAtual}_estoque_final.txt";
            string caminhoEstoqueFinalArquivo = Path.Combine("files", nomeEstoqueFinalArquivo);

            
            if (!System.IO.File.Exists(caminhoEstoqueFinalArquivo))
            {
                return NotFound("Arquivo de estoque final não encontrado.");
            }

            
            var estoqueTexto = await System.IO.File.ReadAllLinesAsync(caminhoEstoqueFinalArquivo);

            // Processar as linhas do arquivo - cada linha deve ter o formato <CamisaId>;<NomeBanda>;<QuantidadeEmEstoque>
            foreach (var linha in estoqueTexto)
            {
                var partes = linha.Split(';');
                if (partes.Length == 3 && int.TryParse(partes[0], out int camisaId) && int.TryParse(partes[2], out int quantidadeEmEstoque))
                {
                    
                    var camisa = await _db.Camisas.FirstOrDefaultAsync(c => c.CamisaId == camisaId);
                    
                    if (camisa != null)
                    {
                       
                        camisa.QuantidadeEmEstoque = quantidadeEmEstoque;

                        
                        _db.Camisas.Update(camisa);
                    }
                    else
                    {
                        
                        Console.WriteLine($"Camisa com Id {camisaId} não encontrada no banco de dados.");
                    }
                }
                else
                {
                    
                    Console.WriteLine($"Linha inválida no arquivo: {linha}");
                }
            }

            
            await _db.SaveChangesAsync();

           
            return Ok("Estoque atualizado com sucesso.");
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }


    [HttpPost("SalvarPedidos")]
    public async Task<ActionResult> SalvarPedidos()
    {
        try
        {
            string dataAtual = DateTime.Now.ToString("ddMMyy");
            string nomeComprasArquivo = $"{dataAtual}_compras.txt";
            string caminhoComprasArquivo = Path.Combine("files", nomeComprasArquivo);

            
            if (!System.IO.File.Exists(caminhoComprasArquivo))
            {
                return NotFound("Arquivo de compras não encontrado.");
            }

            
            var comprasTexto = await System.IO.File.ReadAllLinesAsync(caminhoComprasArquivo);

            
            var pedido = new Pedido
            {
                DataPedido = DateTime.Now,
                
                ItensPedido = new List<ItemPedido>()
            };

            
            foreach (var linha in comprasTexto)
            {
                var partes = linha.Split(';');
                if (partes.Length == 2 && int.TryParse(partes[0], out int camisaId) && int.TryParse(partes[1], out int quantidadeComprada))
                {
                    
                    var camisa = await _db.Camisas.FirstOrDefaultAsync(c => c.CamisaId == camisaId);
                    if (camisa != null)
                    {
                        var itemPedido = new ItemPedido
                        {
                            CamisaId = camisa.CamisaId,
                            Quantidade = quantidadeComprada,
                             PrecoUnitario = camisa.Preco, 
                            Pedido = pedido
                        };

                    
                    }
                    else
                    {
                        
                        Console.WriteLine($"Camisa com Id {camisaId} não encontrada.");
                    }
                }
                else
                {
                    
                    Console.WriteLine($"Linha inválida no arquivo: {linha}");
                }
            }

            
            _db.Pedidos.Add(pedido);
            await _db.SaveChangesAsync();

            
            return Ok($"Pedido #{pedido.PedidoId} salvo com sucesso.");
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
}



}



