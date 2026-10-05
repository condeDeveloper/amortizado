namespace Conde.Amortizado;

/// <summary>
/// Uma FILA feita com DUAS PILHAS: o exemplo mais limpo de "amortizado baixo,
/// pior caso alto".
///
/// Entrar e empilhar na pilha de entrada, e custa um. Sair e desempilhar da de
/// saida; quando ela esta vazia, tudo da entrada e virado para dentro dela de uma
/// vez, o que custa o tamanho da fila.
///
/// Entao uma saida pode custar n. E o custo amortizado e DOIS, porque cada item
/// e movido no maximo duas vezes na vida: uma ao virar e outra ao sair.
///
/// A distancia entre esses dois numeros e o assunto. Num sistema com prazo, o
/// pior caso e o que importa, e saber que a media e dois nao ajuda quando a
/// operacao que estourou o prazo foi a que custou n.
/// </summary>
public sealed class FilaComPilhas
{
    private readonly Stack<int> entrada = new();
    private readonly Stack<int> saida = new();

    public Contador Contador { get; } = new();

    public int Tamanho => entrada.Count + saida.Count;

    public bool Vazia => Tamanho == 0;

    public void Entrar(int valor)
    {
        entrada.Push(valor);
        Contador.Gastar();
        Contador.Fechar();
    }

    public int Sair()
    {
        if (saida.Count == 0)
        {
            if (entrada.Count == 0) throw new InvalidOperationException("fila vazia");

            // A VIRADA: aqui mora todo o custo da estrutura, e ela acontece em
            // uma saida de cada lote. As saidas seguintes custam um ate a pilha
            // de saida esvaziar de novo.
            while (entrada.Count > 0)
            {
                saida.Push(entrada.Pop());
                Contador.Gastar();
            }
        }

        var valor = saida.Pop();
        Contador.Gastar();
        Contador.Fechar();
        return valor;
    }

    /// <summary>
    /// O POTENCIAL da fila: o tamanho da pilha de entrada.
    ///
    /// Ele e exatamente o trabalho que ainda vai ser preciso para virar os itens
    /// que ja entraram. Entrar sobe o potencial em um, e a virada derruba ele a
    /// zero pagando o proprio custo. Com ele, o custo amortizado de entrar e
    /// dois e o de sair e dois.
    /// </summary>
    public double Potencial() => entrada.Count;

    /// <summary>
    /// Roda uma sequencia alternada e devolve o contador e a verificacao do
    /// potencial.
    ///
    /// A sequencia e em LOTES de propósito: entrar com muitos e sair com muitos
    /// e o que produz a virada cara. Alternar um a um nunca junta item na
    /// entrada e esconde o fenomeno.
    /// </summary>
    public static (Contador Contador, Potencial Potencial, long PiorSaida) EmLotes(int lotes, int porLote)
    {
        var fila = new FilaComPilhas();
        var potencial = new Potencial(0);
        long piorSaida = 0;

        for (var lote = 0; lote < lotes; lote++)
        {
            for (var i = 0; i < porLote; i++)
            {
                var antes = fila.Contador.Total;
                fila.Entrar(i);
                potencial.Registrar(fila.Contador.Total - antes, fila.Potencial());
            }

            for (var i = 0; i < porLote; i++)
            {
                var antes = fila.Contador.Total;
                fila.Sair();
                var custo = fila.Contador.Total - antes;
                piorSaida = Math.Max(piorSaida, custo);
                potencial.Registrar(custo, fila.Potencial());
            }
        }

        return (fila.Contador, potencial, piorSaida);
    }
}

/// <summary>
/// Um CONTADOR BINARIO: incrementar custa de um a todos os bits, e a media e
/// menos de dois.
///
/// Ele e o exemplo mais antigo de analise amortizada e o mais fácil de conferir
/// na mao: ao somar um, os bits um do fim viram zero e o primeiro zero vira um.
/// Um incremento pode custar trinta e dois; a soma de n incrementos custa menos
/// de 2n.
///
/// A conta e geometrica: o bit zero muda em TODO incremento, o bit um em metade
/// deles, o bit dois em um quarto, e assim por diante. A soma dessa serie e dois.
/// </summary>
public sealed class ContadorBinario
{
    private readonly bool[] bits;

    public ContadorBinario(int quantosBits) => bits = new bool[quantosBits];

    public Contador Contador { get; } = new();

    public long Valor
    {
        get
        {
            long saida = 0;
            for (var i = bits.Length - 1; i >= 0; i--) saida = saida * 2 + (bits[i] ? 1 : 0);
            return saida;
        }
    }

    /// <summary>Quantos bits estao em um, que e o potencial natural.</summary>
    public int Uns => bits.Count(b => b);

    public void Somar()
    {
        var i = 0;
        while (i < bits.Length && bits[i])
        {
            bits[i] = false;
            Contador.Gastar();
            i++;
        }

        if (i < bits.Length)
        {
            bits[i] = true;
            Contador.Gastar();
        }

        Contador.Fechar();
    }

    /// <summary>
    /// Roda n incrementos e devolve o contador e a verificacao do potencial,
    /// que aqui e simplesmente a quantidade de bits em um.
    /// </summary>
    public static (Contador Contador, Potencial Potencial) Contar(int quantos, int bits = 32)
    {
        var contador = new ContadorBinario(bits);
        var potencial = new Potencial(0);

        for (var i = 0; i < quantos; i++)
        {
            var antes = contador.Contador.Total;
            contador.Somar();
            potencial.Registrar(contador.Contador.Total - antes, contador.Uns);
        }

        return (contador.Contador, potencial);
    }
}
