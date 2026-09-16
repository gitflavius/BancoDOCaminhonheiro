using PXbankAPI.Application.DTOs;
using PXbankAPI.Domain.Entities;
using PXbankAPI.Domain.Interfaces;

namespace PXbankAPI.Application.Services
{
    public interface IMotoristaService
    {
        Task<ApiResponse<MotoristaResponseDto>> Criar(CriarMotoristaDto dto);
        Task<ApiResponse<MotoristaResponseDto>> Obter(int id);
        Task<ApiResponse<List<MotoristaResponseDto>>> ObterAtivos();
        Task<ApiResponse<MotoristaResponseDto>> Atualizar(AtualizarMotoristaDto dto);
        Task<ApiResponse<decimal>> ObterSaldo(int id);
    }

    public class MotoristaService : IMotoristaService
    {
        private readonly IRepositorioMotorista _motoristas;
        private readonly IContaRepository _contas;

        public MotoristaService(IRepositorioMotorista motoristas, IContaRepository contas)
        {
            _motoristas = motoristas;
            _contas = contas;
        }

        public async Task<ApiResponse<MotoristaResponseDto>> Criar(CriarMotoristaDto dto)
        {
            var motorista = new Motorista
            {
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Placa = dto.Placa,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            if (!motorista.Validar(out var erro))
                return ApiResponse<MotoristaResponseDto>.Erro(erro);

            await _motoristas.Adicionar(motorista);
            await _motoristas.Salvar();

            // Todo motorista nasce com uma conta. Sem ela nao ha onde
            // creditar o frete depois.
            var conta = new Conta(motorista.Id, motorista.Nome, motorista.Cpf);
            await _contas.Adicionar(conta);
            await _contas.Salvar();

            return ApiResponse<MotoristaResponseDto>.Ok(
                Mapear(motorista, conta),
                "Motorista criado com sucesso");
        }

        public async Task<ApiResponse<MotoristaResponseDto>> Obter(int id)
        {
            try
            {
                var motorista = await _motoristas.ObterPorId(id);
                var conta = await _contas.ObterPorMotorista(id);

                return ApiResponse<MotoristaResponseDto>.Ok(Mapear(motorista, conta));
            }
            catch (KeyNotFoundException)
            {
                return ApiResponse<MotoristaResponseDto>.Erro("Motorista nao encontrado");
            }
        }

        public async Task<ApiResponse<List<MotoristaResponseDto>>> ObterAtivos()
        {
            var motoristas = await _motoristas.ObterAtivos();
            var lista = motoristas.Select(m => Mapear(m, null)).ToList();

            return ApiResponse<List<MotoristaResponseDto>>.Ok(lista);
        }

        public async Task<ApiResponse<MotoristaResponseDto>> Atualizar(AtualizarMotoristaDto dto)
        {
            try
            {
                var motorista = await _motoristas.ObterPorId(dto.Id);

                motorista.Nome = dto.Nome;
                motorista.Email = dto.Email;
                motorista.Telefone = dto.Telefone;
                motorista.Placa = dto.Placa;
                motorista.Ativo = dto.Ativo;
                motorista.DataAtualizacao = DateTime.UtcNow;

                if (!motorista.Validar(out var erro))
                    return ApiResponse<MotoristaResponseDto>.Erro(erro);

                await _motoristas.Atualizar(motorista);
                await _motoristas.Salvar();

                var conta = await _contas.ObterPorMotorista(motorista.Id);

                return ApiResponse<MotoristaResponseDto>.Ok(
                    Mapear(motorista, conta),
                    "Motorista atualizado com sucesso");
            }
            catch (KeyNotFoundException)
            {
                return ApiResponse<MotoristaResponseDto>.Erro("Motorista nao encontrado");
            }
        }

        public async Task<ApiResponse<decimal>> ObterSaldo(int id)
        {
            var conta = await _contas.ObterPorMotorista(id);

            if (conta is null)
                return ApiResponse<decimal>.Erro("Conta nao encontrada para este motorista");

            return ApiResponse<decimal>.Ok(conta.Saldo);
        }

        private static MotoristaResponseDto Mapear(Motorista motorista, Conta? conta) => new()
        {
            Id = motorista.Id,
            Nome = motorista.Nome,
            Cpf = motorista.Cpf,
            Email = motorista.Email,
            Telefone = motorista.Telefone,
            Placa = motorista.Placa,
            Ativo = motorista.Ativo,
            DataCriacao = motorista.DataCriacao,
            Saldo = conta?.Saldo ?? 0m,
            TotalTransacoes = motorista.Transacoes?.Count ?? 0
        };
    }
}