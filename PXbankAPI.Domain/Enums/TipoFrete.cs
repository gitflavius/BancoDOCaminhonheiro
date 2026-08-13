using System;
using System.Collections.Generic;
using System.Text;

namespace PXbankAPI.Domain.Enums
{
    public enum TipoFrete
    {
        Rodoviario = 1,
        Aereo = 2,
        Maritimo = 3,
        Ferroviario = 4
    }

    public enum StatusTransacao
    {
        Pendente = 1,
        Processando = 2,
        Confirmada = 3,
        Recusada = 4,
        Cancelada = 5
    }
}
