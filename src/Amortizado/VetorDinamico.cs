namespace Conde.Amortizado;

/// <summary>
/// O VETOR QUE CRESCE: o exemplo canonico de analise amortizada.
///
/// Acrescentar um item custa uma operacao, exceto quando o vetor enche: ai ele
/// aloca um maior e COPIA tudo. Essa copia e cara e rara, e a pergunta amortizada
/// e quanto ela custa POR INSERCAO quando se olha a sequencia inteira.
///
/// O fator de crescimento e uma chave aqui, e nao e detalhe de implementacao:
/// dobrar da custo constante por insercao, e crescer de um em um da custo linear
/// por insercao, ou seja, quadratico no total. A medida mostra os dois.
/// </summary>
public sealed class VetorDinamico
{
    private int[] dados;
    private readonly double fator;
    private readonly int passo;

    /// <summary>
    /// Cria o vetor. Com <paramref name="fatorDeCrescimento"/> maior que um, a
    /// capacidade e multiplicada; com ele em um, ela cresce de
    /// <paramref name="passoFixo"/> em <paramref name="passoFixo"/>.
    /// </summary>
    public VetorDinamico(double fatorDeCrescimento = 2, int capacidadeInicial = 1, int passoFixo = 1)
    {
        if (fatorDeCrescimento < 1) throw new ArgumentOutOfRangeException(nameof(fatorDeCrescimento));
        if (capacidadeInicial < 1) throw new ArgumentOutOfRangeException(nameof(capacidadeInicial));

        fator = fatorDeCrescimento;
        passo = passoFixo;
        dados = new int[capacidadeInicial];
    }

    public Contador Contador { get; } = new();

    public int Tamanho { get; private set; }

    public int Capacidade => dados.Length;

    /// <summary>Quantas vezes o vetor precisou crescer.</summary>
    public int Crescimentos { get; private set; }

    /// <summary>Quantos itens foram copiados no total.</summary>
    public long Copias { get; private set; }

    public int this[int indice] => dados[indice];

    public void Acrescentar(int valor)
    {
        if (Tamanho == dados.Length)
        {
            var nova = fator > 1
                ? Math.Max(dados.Length + 1, (int)(dados.Length * fator))
                : dados.Length + passo;

            var maior = new int[nova];
            Array.Copy(dados, maior, Tamanho);

            // A copia e o custo de verdade, e e ela que a analise amortizada
            // distribui pelas insercoes baratas que vieram antes.
            Contador.Gastar(Tamanho);
            Copias += Tamanho;
            Crescimentos++;

            dados = maior;
        }

        dados[Tamanho++] = valor;
        Contador.Gastar();
        Contador.Fechar();
    }

    /// <summary>
    /// O POTENCIAL do vetor, para a verificacao do metodo.
    ///
    /// Para o fator dois, a escolha classica e duas vezes o tamanho menos a
    /// capacidade. Ela e zero logo depois de crescer e sobe ate igualar a
    /// capacidade quando o vetor enche de novo, que e exatamente a conta que a
    /// proxima copia vai cobrar.
    /// </summary>
    public double PotencialClassico() => 2.0 * Tamanho - Capacidade;

    /// <summary>
    /// Roda uma sequencia de insercoes e devolve o contador e a verificacao do
    /// potencial.
    /// </summary>
    public static (Contador Contador, Potencial Potencial) Inserir(int quantos, double fator,
                                                                   int capacidadeInicial = 1, int passoFixo = 1)
    {
        var vetor = new VetorDinamico(fator, capacidadeInicial, passoFixo);
        var potencial = new Potencial(vetor.PotencialClassico());

        for (var i = 0; i < quantos; i++)
        {
            var antes = vetor.Contador.Total;
            vetor.Acrescentar(i);
            potencial.Registrar(vetor.Contador.Total - antes, vetor.PotencialClassico());
        }

        return (vetor.Contador, potencial);
    }

    /// <summary>
    /// Quanto ESPACO o vetor desperdica: a capacidade menos o tamanho.
    ///
    /// E a outra ponta da troca. Dobrar deixa ate metade do vetor vazio logo
    /// depois de crescer; crescer de pouco em pouco quase nao desperdica e paga
    /// em copia. Nao existe escolha que ganhe nas duas.
    /// </summary>
    public int Desperdicio => Capacidade - Tamanho;
}
