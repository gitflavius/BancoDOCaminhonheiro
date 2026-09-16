namespace PXbankAPI.Application.DTOs
{
    /// <summary>
    /// ENTRADA: so o que o cliente da API pode informar ao criar um motorista.
    /// Id, datas e saldo ficam de fora de proposito - sao definidos pelo sistema.
    /// </summary>
    public class CriarMotoristaDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
    }

    /// <summary>
    /// ENTRADA: o que pode ser alterado depois. O CPF nao entra aqui:
    /// documento nao se corrige por PUT, se corrige por processo administrativo.
    /// </summary>
    public class AtualizarMotoristaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    /// <summary>
    /// SAIDA: o que a API devolve. Pode conter dados calculados,
    /// porque aqui o sistema informa, nao recebe.
    /// </summary>
    public class MotoristaResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public decimal Saldo { get; set; }
        public int TotalTransacoes { get; set; }
    }
}