namespace Lawllit.Model.Finance.Contratos;

public sealed class CotacaoModel
{
    public string Rotulo { get; set; } = string.Empty;
    public string Bandeira { get; set; } = string.Empty;
    public decimal ValorCompra { get; set; }
    public decimal MaximaDia { get; set; }
    public decimal MinimaDia { get; set; }
    public decimal VariacaoPercentual { get; set; }
    public int CasasDecimais { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
