namespace Conde.Amortizado;

/// <summary>
/// O JUIZ: a contagem exata de operacoes elementares.
///
/// Analise amortizada e sobre CUSTO, e custo medido com relogio nao serve para
/// nada aqui. O relogio mede o cache, o coletor de lixo, o compilador e a
/// temperatura da maquina, e a diferenca entre dobrar e crescer de um em um
/// aparece como ruido no meio disso tudo.
///
/// O que este repositorio conta e OPERACAO: cada copia de item, cada
/// comparacao, cada passo de ponteiro. O numero que sai e o mesmo em qualquer
/// maquina, e e sobre ele que a conta amortizada fala.
/// </summary>
public sealed class Contador
{
    private readonly List<long> porOperacao = [];

    /// <summary>O total de operacoes elementares desde o comeco.</summary>
    public long Total { get; private set; }

    /// <summary>Quantas chamadas do cliente ja houve.</summary>
    public int Chamadas => porOperacao.Count;

    /// <summary>O custo de cada chamada, na ordem.</summary>
    public IReadOnlyList<long> PorOperacao => porOperacao;

    /// <summary>O custo da chamada mais cara. E o PIOR CASO medido.</summary>
    public long Pico => porOperacao.Count == 0 ? 0 : porOperacao.Max();

    /// <summary>O custo medio por chamada. E o AMORTIZADO medido.</summary>
    public double Medio => Chamadas == 0 ? 0 : Total / (double)Chamadas;

    private long naChamada;

    /// <summary>Conta operacoes dentro da chamada em andamento.</summary>
    public void Gastar(long quantas = 1)
    {
        naChamada += quantas;
        Total += quantas;
    }

    /// <summary>Fecha a chamada atual e guarda o custo dela.</summary>
    public void Fechar()
    {
        porOperacao.Add(naChamada);
        naChamada = 0;
    }

    public void Zerar()
    {
        porOperacao.Clear();
        Total = 0;
        naChamada = 0;
    }

    public override string ToString() =>
        $"{Chamadas} chamadas, {Total} operacoes, media {Medio:N2}, pico {Pico}";
}

/// <summary>
/// A verificacao do METODO DO POTENCIAL, que e uma tecnica de PROVA e aqui vira
/// uma conferencia numerica.
///
/// A ideia e inventar uma funcao do estado da estrutura, o potencial, e definir
/// o custo amortizado de uma operacao como o custo real mais a variacao do
/// potencial. Quando a conta fecha, a soma dos amortizados limita a soma dos
/// reais, e cada operacao tem um custo amortizado pequeno mesmo que algumas
/// sejam caras.
///
/// Normalmente isso e verificado no papel. Aqui as duas condicoes sao conferidas
/// a cada passo, em cima dos numeros de verdade:
///
///   1. o potencial nunca fica abaixo do inicial (senao a conta nao limita nada)
///   2. a soma dos amortizados e pelo menos a soma dos reais
///
/// Uma funcao de potencial mal escolhida quebra a primeira, e o erro e silencioso
/// no papel: a prova parece funcionar e nao prova.
/// </summary>
public sealed class Potencial
{
    private readonly List<long> reais = [];
    private readonly List<double> amortizados = [];
    private double anterior;
    private readonly double inicial;

    public Potencial(double potencialInicial = 0)
    {
        inicial = potencialInicial;
        anterior = potencialInicial;
    }

    public double Menor { get; private set; } = double.MaxValue;

    public long SomaReal => reais.Sum();

    public double SomaAmortizada => amortizados.Sum();

    public double MaiorAmortizado => amortizados.Count == 0 ? 0 : amortizados.Max();

    /// <summary>Registra uma operacao: o custo real e o potencial DEPOIS dela.</summary>
    public void Registrar(long custoReal, double potencialDepois)
    {
        reais.Add(custoReal);
        amortizados.Add(custoReal + potencialDepois - anterior);
        anterior = potencialDepois;
        Menor = Math.Min(Menor, potencialDepois);
    }

    /// <summary>O potencial nunca ficou abaixo do inicial.</summary>
    public bool NuncaFicouNegativo => Menor >= inicial - 1e-9;

    /// <summary>A soma dos amortizados limita a soma dos reais.</summary>
    public bool Limita => SomaAmortizada >= SomaReal - 1e-9;

    /// <summary>As duas condicoes juntas, que e o que a prova precisa.</summary>
    public bool Vale => NuncaFicouNegativo && Limita;

    public override string ToString() =>
        $"real {SomaReal}, amortizado {SomaAmortizada:N1}, maior amortizado {MaiorAmortizado:N2}, " +
        $"potencial minimo {Menor:N1}";
}
