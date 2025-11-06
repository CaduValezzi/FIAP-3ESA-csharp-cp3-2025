# CP3 – C#  ·  “Sangue & Algodão”
=======================================================

Este repositório contém a Web API da loja de camisas de banda (mesma temática da prova escrita).
Alguns endpoints estão **não implementados** — essas são as **questões da prova**.

A prova escrita em PDF está no repositório para referência de conteúdo/modelos.

-------------------------------------------------------
COMO ENTREGAR
-------------------------------------------------------
Faça **fork** deste repositório, resolva as questões e faça **commit/push** no seu fork.
Ao fim, **envie o link do seu fork** no Assignment **“CP3 - C#”** no Teams.

-------------------------------------------------------
ONDE ESTÁ CADA QUESTÃO NO CÓDIGO
-------------------------------------------------------
Q1  → Controllers/CamisasController.cs      (GET  /api/camisas/estoque-critico)

Q2  → Controllers/OperacoesController.cs    (GET  /api/operacoes/GetInfo?bandaId=)

Q3  → Controllers/OperacoesController.cs    (POST /api/operacoes/GerarEstoqueInicial)

Q4  → Controllers/OperacoesController.cs    (POST /api/operacoes/GerarEstoqueFinal)

Q5  → Controllers/OperacoesController.cs    (POST /api/operacoes/AtualizarEstoque)

Q6  → Controllers/OperacoesController.cs    (POST /api/operacoes/SalvarPedidos)

OBS.: Existem **CRUDs prontos** em `BandasController`, `CamisasController`, `PedidosController` e `ItensPedidoController`.
Eles servem para **inspecionar/consultar** o banco e como **“cola”** de EF (GET/POST/UPDATE/DELETE).
**Você não precisa usar** esses CRUDs para resolver as questões.

-------------------------------------------------------
FORMATO DOS ARQUIVOS
-------------------------------------------------------
Diretório: `files/` (na raiz do projeto)  
Data usada no nome: **DDMMAA** (ex.: `061125`)

1) Compras (já incluso) — `files/DDMMAA_compras.txt`
   - Blocos separados por `#`
   - Cabeçalho do bloco:  `<PedidoId>;<CpfCliente>;<HH:mm>`
   - Linhas de item:      `<Banda>;<Tamanho>;<Modelo>;<Preco>`
   Exemplo:
     #
     1;01234567890;09:12
     Iron Maiden;M;The Trooper;119.90
     Metallica;G;Master of Puppets;129.90

2) Estoques que **você gera**:
   - Uma linha por **camisa**:  `<IdCamisa>;<NomeBanda>;<Quantidade>`
   - Exemplo: `1;Iron Maiden;12`

  Observação: Internamente, para localizar a **camisa correta** ao processar as compras,
     use a tripla **(BandaId, Modelo, Tamanho)**, que **identifica unicamente** uma camisa.

-------------------------------------------------------
ENUNCIADO
-------------------------------------------------------

Q1 — GET /api/camisas/estoque-critico  (CamisasController.cs)

Retorne um `EstoqueCriticoDTO` com:
  • `Camisas`: todas as camisas onde `QuantidadeEmEstoque < QuantidadeMinimaAlerta`  
  • `QuantidadeTotal`: soma das `QuantidadeEmEstoque` dessas camisas  
  • `QuantidadeTipos`: número de registros de camisa em crítico

Q2 — GET /api/operacoes/GetInfo?bandaId=  (OperacoesController.cs)

Retorne `List<InfoEstoqueDTO>`.
  • Se `bandaId` vier na query, retorne **apenas** as camisas da banda indicada.  
  • Se **não** vier, retorne **todas** as camisas.  

Q3 — POST /api/operacoes/GerarEstoqueInicial  (OperacoesController.cs)

Gere o arquivo **`files/DDMMAA_estoque_inicial.txt`** no formato:
  `<IdCamisa>;<NomeBanda>;<QuantidadeEmEstoque>`  
Retorne **a string** escrita no arquivo na resposta.

Q4 — POST /api/operacoes/GerarEstoqueFinal  (OperacoesController.cs)

É garantido que a tripla **(BandaId, Modelo, Tamanho)** identifica unicamente uma camisa.

A partir de **`files/DDMMAA_compras.txt`**:
  • Calcule o **estoque final por CamisaId** (subtraindo as compras do estoque inicial, ou no banco ou no arquivo)  
  • Gere **`files/DDMMAA_estoque_final.txt`** no formato:
    `<IdCamisa>;<NomeBanda>;<QuantidadeFinal>`  
  • Retorne **a string** do arquivo gerado

Q5 — POST /api/operacoes/AtualizarEstoque  (OperacoesController.cs)

Leia `files/DDMMAA_estoque_final.txt` e atualize no banco:
  `Camisa.QuantidadeEmEstoque = <QuantidadeFinal>`  

Q6 — POST /api/operacoes/SalvarPedidos  (OperacoesController.cs)

A partir de `files/DDMMAA_compras.txt`:
  • Para cada bloco `#`, criar **Pedido** com:
      `DataPedido =` data de hoje + HH:mm do bloco  
      `CpfCliente =` cabeçalho do bloco  
      `Status =` **Entregue**  
  • Para as demais linha do bloco, criar **ItemPedido**, resolvendo `CamisaId` por **(Banda + Modelo + Tamanho)**  
  • Salvar no banco  

Cada questão vale 2 pontos.
(SIM! A prova vale mais que dez, mas dez é a nota máxima)

-------------------------------------------------------
DÚVIDAS
-------------------------------------------------------
Sintaxe, interpretação do enunciado, achou um erro, está confuso com algo, detalhes do formato, quer dizer oi?
Chame o professor durante a prova — ajudarei no que for possível.

Boa prova!
