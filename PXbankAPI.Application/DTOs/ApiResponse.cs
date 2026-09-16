namespace PXbankAPI.Application.DTOs
{
    /// <summary>
    /// Envelope padrao de resposta da API. Todo endpoint devolve esta estrutura,
    /// entao quem consome sempre sabe onde olhar: Sucesso diz se deu certo,
    /// Dados traz o resultado e Erros traz o que falhou.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public T? Dados { get; set; }
        public List<string> Erros { get; set; } = new();

        public static ApiResponse<T> Ok(T dados, string mensagem = "Sucesso")
            => new()
            {
                Sucesso = true,
                Mensagem = mensagem,
                Dados = dados
            };

        public static ApiResponse<T> Erro(string mensagem, List<string>? erros = null)
            => new()
            {
                Sucesso = false,
                Mensagem = mensagem,
                Erros = erros ?? new List<string> { mensagem }
            };
    }
}