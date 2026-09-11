using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PXBankAPI.Application.DTOs;
using PXBankAPI.Domain.Entities;
using PXBankAPI.Domain.Enums;
using PXBankAPI.Domain.Interfaces;

namespace PXBankAPI.Application.Services
{
    public interface IServicoMotorista
    {
        Task<ApiResponse<MotoristaPorMotoristaDto>> Criar(CriarMotoristaDto dto);
        Task<ApiResponse<MotoristaPorMotoristaDto>> Obter(int id);
        Task<ApiResponse<List<MotoristaPorMotoristaDto>>> ObterTodos();
        Task<ApiResponse<MotoristaPorMotoristaDto>> Atualizar(AtualizarMotoristaDto dto);
        Task<ApiResponse<bool>> Deletar(int id);
        Task<ApiResponse<decimal>> ObterSaldo(int id);
    }

    public class ServicoMotorista: IServicoMotorista
    {
        private readonly IServicoMotorista _repositorio;
        private readonly IRepositorioAuditoria _auditoria;
    }
    
    public servicoMotorista(IRepositorioMotorista repositorio, IRepositorioAuditoria auditoria)
        {
            _repositorio = repositorio;
            _auditoria = auditoria;
        }

        public async Task<ApiResponse<MotoristaPorMotoristaDto>> Criar(CriarMotoristaDto dto)
        {
            try
            {
                var existente = await _repositorio.ObterPorCpf(dto.cpf);
                if (existente != null)
                {
                    return ApiResponse<MotoristaPorMotoristaDto>.Erro("CPF ja cadastrado no sistema");
                }
                var motorista = new Motorista
                {
                    Nome = dto.Nome,
                    Cpf = dto.Cpf,
                    Email = dto.Email,
                    Telefone = dto.Telefone,
                    SaldoDisponivel = 0,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow
                };
                if (!motorista.Validar(out var erro))
                {
                    return ApiResponse<MotoristaPorMotoristaDto>.Erro(erro);
                }
                await _repositorio.Adicionar(motorista);
                await _repositorio.Salvar();

                await _auditoria.RegistrarAcao(
                    "MOTORISTA_CRIADO",
                    "SISTEMA",
                    $"Motorista {motorista.Nome} criado com sucesso"
                );

                return ApiResponse<MotoristaPorMotoristaDto>.Ok(
                    MapearParaDto(motorista),
                    "Motorista criado com sucesso"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<MotoristaPorMotoristaDto>.Erro(
                    "Erro ao criar motorista",
                    new List<string> { ex.Message }
                    );
            }

        }

        public async Task<ApiResponse<MotoristaPorMotoristaDto>>Obter(int id)
        {
            var motorista = await _repositorio.Obter(id);
            if (motorista == null)
            {
                return ApiResponse<MotoristaPorMotoristaDto>.Erro("Motorista não encontrado");
            }
            
            return ApiResponse<MotoristaPorMotoristaDto>.Ok(MapearParaDto(motorista));
        }

        public async Task<ApiResponse<List<MotoristaPorMotoristaDto>>> ObterTodos()
        {
            var motoristas = await _repositorio.ObterTodos();
            var dto = motoristas.Select(MapearParaDto).ToList();
            return ApiResponse<List<MotoristaPorMotoristaDto>>.Ok(dto);
        }

        public async Task<ApiResponse<MotoristaPorMotoristaDto>>Atualizar(AtualizarMotoristaDto dto)
        {
            var motorista = await _repositorio.ObterPorId(dto.Id);
            if(motorista == null)
            {
                return ApiResponse<MotoristaPorMotoristaDto>.Erro("Motorista não encontrado");
            }
            motorista.Nome = dto.Nome;
            motorista.Email = dto.Email;
            motorista.Telefone = dto.Telefone;
            motorista.Placa = dto.Placa;
            motorista.Ativo = dto.Ativo;
            motorista.DataAtualizacao = DateTime.UtcNow;
            if (!motorista.Validar(out var erro))
            {
                return ApiResponse<MotoristaPorMotoristaDto>.Erro(erro);
            }

            await _repositorio.Atualizar(motorista);
            await _repositorio.Salvar();

            return ApiResponse<MotoristaPorMotoristaDto>.ok(
                MapearParaDto(motorista),
                "Motorista Atualizado com sucesso"
                );

        }
        public async Task<ApiResponse<bool>> Deletar(int id)
        {
            var motorista = await _repositorio.ObterPorId(id);
            if (motorista == null)
            {
                return ApiResponse<bool>.Erro("Motorista não encontrado");
            }

            await _repositorio.Deletar(id);
            await _repositorio.Salvar();

            return ApiResponse<bool>.Ok(true, "Motorista deletado com sucesso");
        }
        public async Task<ApiResponse<decimal>> ObterSaldo(int id)
        {
            var motorista = await _repositorio.ObterPorId(id);
            if (motorista == null)
            {
                return ApiResponse<decimal>.Erro("Motorista não encontrado");
            }

            return ApiResponse<decimal>.Ok(motorista.SaldoDisponivel);
        }
        private MotoristaPorMotoristaDto MapearParaDto(Motorista motorista)
            {
                return new MotoristaPorMotoristaDto
                {
                    Id = motorista.Id,
                    Nome = motorista.Nome,
                    Cpf = motorista.Cpf,
                    Email = motorista.Email,
                    Telefone = motorista.Telefone,
                    Placa = motorista.Placa,
                    SaldoDisponivel = motorista.SaldoDisponivel,
                    Ativo = motorista.Ativo,
                    DataCriacao = motorista.DataCriacao,
                    TotalTransacoes = motorista.Transacoes?.Count ?? 0
                };
        }
    }
    public interface IServicoTransacao
    {
        Task<ApiResponse<TransacaoResponseDto>> Criar(CriarTransacaoDto dto);
        Task<ApiResponse<TransacaoResponseDto>> Obter(int id);
        Task<ApiResponse<List<TransacaoResponseDto>>> ObterPorMotorista(int motoristId);
        Task<ApiResponse<List<TransacaoResponseDto>>> ObterTodas();
        Task<ApiResponse<TransacaoResponseDto>> Atualizar(AtualizarTransacaoDto dto);
        Task<ApiResponse<bool>> Processar(int id);
        Task<ApiResponse<RelatorioTransacoesDto>> GerarRelatorio(DateTime dataInicio, DateTime dataFim);
    }

    public class ServicoTransacao : IServicoTransacao
    {
    private readonly IRepositorioTransacao _repositorio;
    private readonly IRepositorioMotorista _repositorioMotorista;
    private readonly IRepositorioAuditoria _auditoria;

    public ServicoTransacao(
        IRepositorioTransacao repositorio,
        IRepositorioMotorista repositorioMotorista,
        IRepositorioAuditoria auditoria)
    {
        _repositorio = repositorio;
        _repositorioMotorista = repositorioMotorista;
        _auditoria = auditoria;
    }

    public async Task<ApiResponse<TransacaoResponseDto>> Criar(CriarTransacaoDto dto)
    {
        try
        {
            var motorista = await _repositorioMotorista.ObterPorId(dto.MotoristId);
            if (motorista == null)
            {
                return ApiResponse<TransacaoResponseDto>.Erro("Motorista não encontrado");
            }

            var transacao = new Transacao
            {
                MotoristId = dto.MotoristId,
                Valor = dto.Valor,
                TipoFrete = dto.TipoFrete,
                Descricao = dto.Descricao,
                Origem = dto.Origem,
                Destino = dto.Destino,
                ReferenciaExterna = dto.ReferenciaExterna,
                Status = StatusTransacao.Pendente,
                DataCriacao = DateTime.UtcNow
            };

            if (!transacao.Validar(out var erro))
            {
                return ApiResponse<TransacaoResponseDto>.Erro(erro);
            }

            transacao.ComissaoCalculada = transacao.CalcularComissao();

            await _repositorio.Adicionar(transacao);
            await _repositorio.Salvar();

            await _auditoria.RegistrarAcao(
                "TRANSACAO_CRIADA",
                "SISTEMA",
                $"Transação de R$ {transacao.Valor} criada para motorista {motorista.Nome}"
            );

            return ApiResponse<TransacaoResponseDto>.Ok(
                MapearParaDto(transacao, motorista),
                "Transação criada com sucesso"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<TransacaoResponseDto>.Erro(
                "Erro ao criar transação",
                new List<string> { ex.Message }
            );
        }
    }
    public async Task<ApiResponse<TransacaoResponseDto>> Obter(int id)
    {
        var transacao = await _repositorio.ObterPorId(id);
        if (transacao == null)
        {
            return ApiResponse<TransacaoResponseDto>.Erro("Transação não encontrada");
        }

        return ApiResponse<TransacaoResponseDto>.Ok(MapearParaDto(transacao, transacao.Motorista));
    }
    public async Task<ApiResponse<List<TransacaoResponseDto>>> ObterPorMotorista(int motoristId)
    {
        var transacoes = await _repositorio.ObterPorMotorista(motoristId);
        var dto = transacoes.Select(t => MapearParaDto(t, t.Motorista)).ToList();
        return ApiResponse<List<TransacaoResponseDto>>.Ok(dto);
    }
    public async Task<ApiResponse<List<TransacaoResponseDto>>> ObterTodas()
    {
        var transacoes = await _repositorio.ObterTodos();
        var dto = transacoes.Select(t => MapearParaDto(t, t.Motorista)).ToList();
        return ApiResponse<List<TransacaoResponseDto>>.Ok(dto);
    }
    public async Task<ApiResponse<TransacaoResponseDto>> Atualizar(AtualizarTransacaoDto dto)
    {
        var transacao = await _repositorio.ObterPorId(dto.Id);
        if (transacao == null)
        {
            return ApiResponse<TransacaoResponseDto>.Erro("Transação não encontrada");
        }

        transacao.Status = dto.Status;
        transacao.Descricao = dto.Descricao;

        await _repositorio.Atualizar(transacao);
        await _repositorio.Salvar();

        return ApiResponse<TransacaoResponseDto>.Ok(
            MapearParaDto(transacao, transacao.Motorista),
            "Transação atualizada com sucesso"
        );
    }
    public async Task<ApiResponse<bool>> Processar(int id)
    {
        var transacao = await _repositorio.ObterPorId(id);
        if (transacao == null)
        {
            return ApiResponse<bool>.Erro("Transação não encontrada");
        }

        var motorista = await _repositorioMotorista.ObterPorId(transacao.MotoristId);

        if (!motorista.DebitarSaldo(transacao.ComissaoCalculada))
        {
            transacao.MarcarComoRecusada("Saldo insuficiente");
            await _repositorio.Atualizar(transacao);
            await _repositorio.Salvar();
            return ApiResponse<bool>.Erro("Saldo insuficiente para processar a transação");
        }

        transacao.MarcarComoProcessada();
        await _repositorio.Atualizar(transacao);
        await _repositorioMotorista.Atualizar(motorista);
        await _repositorio.Salvar();

        await _auditoria.RegistrarAcao(
            "TRANSACAO_PROCESSADA",
            "SISTEMA",
            $"Transação {id} processada com comissão de R$ {transacao.ComissaoCalculada}"
        );

        return ApiResponse<bool>.Ok(true, "Transação processada com sucesso");
    }
    public async Task<ApiResponse<RelatorioTransacoesDto>> GerarRelatorio(DateTime dataInicio, DateTime dataFim)
    {
        var transacoes = await _repositorio.ObterPorPeriodo(dataInicio, dataFim);

        var relatorio = new RelatorioTransacoesDto
        {
            TotalTransacoes = transacoes.Count,
            ValorTotal = transacoes.Sum(t => t.Valor),
            ComissaoTotal = transacoes.Sum(t => t.ComissaoCalculada),
            TransacoesPorTipo = transacoes
                .GroupBy(t => t.TipoFrete)
                .ToDictionary(g => g.Key, g => g.Count()),
            TransacoesPorStatus = transacoes
                .GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.Count()),
            DataGeracao = DateTime.UtcNow
        };

        return ApiResponse<RelatorioTransacoesDto>.Ok(relatorio, "Relatório gerado com sucesso");
    }
    private TransacaoResponseDto MapearParaDto(Transacao transacao, Motorista motorista)
    {
        return new TransacaoResponseDto
        {
            Id = transacao.Id,
            MotoristId = transacao.MotoristId,
            NomeMotorista = motorista?.Nome ?? "N/A",
            Valor = transacao.Valor,
            TipoFrete = transacao.TipoFrete,
            Status = transacao.Status,
            Descricao = transacao.Descricao,
            Origem = transacao.Origem,
            Destino = transacao.Destino,
            ComissaoCalculada = transacao.ComissaoCalculada,
            DataCriacao = transacao.DataCriacao,
            DataProcessamento = transacao.DataProcessamento
        };
    }
}
