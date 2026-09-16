using PXbankAPI.Application.DTOs;
using PXbankAPI.Domain.Entities;
using PXbankAPI.Domain.Enums;
using PXbankAPI.Domain.Interfaces;

namespace PXbankAPI.Application.Services
{
    public interface IFreteService
    {
        Task<ApiResponse<FreteResponseDto>> Criar(CriarFreteDto dto);
        Task<ApiResponse<FreteResponseDto>> Obter(int id);
        Task<ApiResponse<List<FreteResponseDto>>> ObterPorMotorista(int motoristaId);
        Task<ApiResponse<FreteResponseDto>> Confirmar(int id);
        Task<ApiResponse<FreteResponseDto>> Recusar(RecusarFreteDto dto);
        Task<ApiResponse<ExtratoResponseDto>> ObterExtrato(int motoristaId, DateTime inicio, DateTime fim);
        Task<ApiResponse<RelatorioFretesDto>> GerarRelatorio(DateTime inicio, DateTime fim);
    }

    public class FreteService : IFreteService
    {
        private readonly IRepositorioTransacao _fretes;
        private readonly IRepositorioMotorista _motoristas;
        private readonly IContaRepository _contas;

        public FreteService(
            IRepositorioTransacao fretes,
            IRepositorioMotorista motoristas,
            IContaRepository contas)
        {
            _fretes = fretes;
            _motoristas = motoristas;
            _contas = contas;
        }

        public async Task<ApiResponse<FreteResponseDto>> Criar(CriarFreteDto dto)
        {
            try
            {
                var motorista = await _motoristas.ObterPorId(dto.MotoristaId);

                if (!motorista.Ativo)
                    return ApiResponse<FreteResponseDto>.Erro("Motorista inativo nao pode receber frete");

                var frete = new Transacao
                {
                    MotoristaId = dto.MotoristaId,
                    Valor = dto.Valor,
                    Tipo = dto.Tipo,
                    Descricao = dto.Descricao,
                    Origem = dto.Origem,
                    Destino = dto.Destino,
                    ReferenciaExterna = dto.ReferenciaExterna
                };

                if (!frete.Validar(out var erro))
                    return ApiResponse<FreteResponseDto>.Erro(erro);

                // A comissao e calculada pelo dominio, nao vem do cliente.
                frete.CalcularComissao();

                await _fretes.Adicionar(frete);
                await _fretes.Salvar();

                return ApiResponse<FreteResponseDto>.Ok(
                    Mapear(frete, motorista.Nome),
                    "Frete registrado com sucesso");
            }
            catch (KeyNotFoundException)
            {
                return ApiResponse<FreteResponseDto>.Erro("Motorista nao encontrado");
            }
        }

        /// <summary>
        /// Confirma o frete e credita o valor liquido na conta do motorista.
        /// O dominio faz as validacoes: a Transacao recusa mudar de estado se
        /// ja foi processada, e a Conta recusa frete pendente ou de outro dono.
        /// </summary>
        public async Task<ApiResponse<FreteResponseDto>> Confirmar(int id)
        {
            try
            {
                var frete = await _fretes.ObterPorId(id);
                var conta = await _contas.ObterPorMotorista(frete.MotoristaId);

                if (conta is null)
                    return ApiResponse<FreteResponseDto>.Erro("Motorista nao possui conta");

                frete.MarcarComoProcessada();
                conta.ReceberFrete(frete);

                await _fretes.Atualizar(frete);
                await _contas.Salvar();

                var motorista = await _motoristas.ObterPorId(frete.MotoristaId);

                return ApiResponse<FreteResponseDto>.Ok(
                    Mapear(frete, motorista.Nome),
                    $"Frete confirmado. Creditado R$ {frete.Valor - frete.ComissaoCalculada:N2} na conta.");
            }
            catch (KeyNotFoundException)
            {
                return ApiResponse<FreteResponseDto>.Erro("Frete nao encontrado");
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<FreteResponseDto>.Erro(ex.Message);
            }
        }

        public async Task<ApiResponse<FreteResponseDto>> Recusar(RecusarFreteDto dto)
        {
            try
            {
                var frete = await _fretes.ObterPorId(dto.Id);

                frete.MarcarComoRecusada(dto.Motivo);

                await _fretes.Atualizar(frete);
                await _fretes.Salvar();

                var motorista = await _motoristas.ObterPorId(frete.MotoristaId);

                return ApiResponse<FreteResponseDto>.Ok(
                    Mapear(frete, motorista.Nome),
                    "Frete recusado");
            }
            catch (KeyNotFoundException)
            {
                return ApiResponse<FreteResponseDto>.Erro("Frete nao encontrado");
            }
            catch (ArgumentException ex)
            {
                return ApiResponse<FreteResponseDto>.Erro(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<FreteResponseDto>.Erro(ex.Message);
            }
        }

        public async Task<ApiResponse<FreteResponseDto>> Obter(int id)
        {
            try
            {
                var frete = await _fretes.ObterPorId(id);
                var motorista = await _motoristas.ObterPorId(frete.MotoristaId);

                return ApiResponse<FreteResponseDto>.Ok(Mapear(frete, motorista.Nome));
            }
            catch (KeyNotFoundException)
            {
                return ApiResponse<FreteResponseDto>.Erro("Frete nao encontrado");
            }
        }

        public async Task<ApiResponse<List<FreteResponseDto>>> ObterPorMotorista(int motoristaId)
        {
            var fretes = await _fretes.ObterPorMotorista(motoristaId);
            var lista = fretes.Select(f => Mapear(f, f.Motorista?.Nome ?? string.Empty)).ToList();

            return ApiResponse<List<FreteResponseDto>>.Ok(lista);
        }

        public async Task<ApiResponse<ExtratoResponseDto>> ObterExtrato(
            int motoristaId, DateTime inicio, DateTime fim)
        {
            var conta = await _contas.ObterPorMotorista(motoristaId);

            if (conta is null)
                return ApiResponse<ExtratoResponseDto>.Erro("Conta nao encontrada");

            var movimentos = await _contas.ObterExtrato(conta.Id, inicio, fim);

            var extrato = new ExtratoResponseDto
            {
                ContaId = conta.Id,
                Titular = conta.Titular,
                SaldoAtual = conta.Saldo,
                PeriodoInicio = inicio,
                PeriodoFim = fim,
                Movimentos = movimentos.Select(m => new MovimentoResponseDto
                {
                    Id = m.Id,
                    Tipo = m.Tipo,
                    Valor = m.Valor,
                    Descricao = m.Descricao,
                    OcorridoEm = m.OcorridoEm,
                    TransacaoId = m.TransacaoId
                }).ToList()
            };

            return ApiResponse<ExtratoResponseDto>.Ok(extrato);
        }

        public async Task<ApiResponse<RelatorioFretesDto>> GerarRelatorio(DateTime inicio, DateTime fim)
        {
            var fretes = await _fretes.ObterPorPeriodo(inicio, fim);

            var relatorio = new RelatorioFretesDto
            {
                TotalFretes = fretes.Count,
                ValorTotal = fretes.Sum(f => f.Valor),
                ComissaoTotal = fretes.Sum(f => f.ComissaoCalculada),
                FretesPorTipo = fretes.GroupBy(f => f.Tipo.ToString())
                                      .ToDictionary(g => g.Key, g => g.Count()),
                FretesPorStatus = fretes.GroupBy(f => f.Status.ToString())
                                        .ToDictionary(g => g.Key, g => g.Count()),
                DataGeracao = DateTime.UtcNow
            };

            return ApiResponse<RelatorioFretesDto>.Ok(relatorio, "Relatorio gerado");
        }

        private static FreteResponseDto Mapear(Transacao frete, string nomeMotorista) => new()
        {
            Id = frete.Id,
            MotoristaId = frete.MotoristaId,
            NomeMotorista = nomeMotorista,
            Valor = frete.Valor,
            ComissaoCalculada = frete.ComissaoCalculada,
            ValorLiquido = frete.Valor - frete.ComissaoCalculada,
            Tipo = frete.Tipo,
            Status = frete.Status,
            Descricao = frete.Descricao,
            Origem = frete.Origem,
            Destino = frete.Destino,
            DataCriacao = frete.DataCriacao,
            DataProcessamento = frete.DataProcessamento
        };
    }
}