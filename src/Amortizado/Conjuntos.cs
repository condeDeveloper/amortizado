namespace Conde.Amortizado;

/// <summary>
/// O sorteio com semente, escrito por extenso: o Random do .NET nao promete a
/// mesma sequencia entre versoes nem entre sistemas, e a CI roda em tres.
/// </summary>
public sealed class Aleatorio
{
    private ulong estado;

    public Aleatorio(ulong semente) => estado = semente * 2 + 1;

    public ulong Proximo()
    {
        estado = unchecked(estado * 6364136223846793005UL + 1442695040888963407UL);
        return estado >> 17;
    }

    public int Entre(int baixo, int alto) => baixo + (int)(Proximo() % (ulong)(alto - baixo + 1));
}

/// <summary>
/// CONJUNTOS DISJUNTOS, ou union-find: duas otimizacoes que parecem detalhes e
/// mudam a classe de complexidade.
///
/// A estrutura e trivial: cada item aponta para um pai, e a raiz identifica o
/// conjunto. Juntar dois conjuntos e fazer uma raiz apontar para a outra.
///
/// As duas otimizacoes sao chaves aqui, e a medida mostra que elas nao somam:
/// elas se MULTIPLICAM. Cada uma sozinha da custo logaritmico; as duas juntas dao
/// um custo que cresce tao devagar que, para qualquer n que caiba no universo,
/// ele e menor que cinco. A funcao exata e o inverso da de Ackermann, e ela nao
/// e constante, so parece.
/// </summary>
public sealed class Conjuntos
{
    private readonly int[] pai;
    private readonly int[] altura;
    private readonly int[] tamanho;

    public Conjuntos(int itens, bool comCompressao = true, bool porAltura = true)
    {
        ComCompressao = comCompressao;
        PorAltura = porAltura;

        pai = Enumerable.Range(0, itens).ToArray();
        altura = new int[itens];
        tamanho = Enumerable.Repeat(1, itens).ToArray();
    }

    /// <summary>
    /// COMPRESSAO DE CAMINHO: ao buscar a raiz, todo mundo do caminho passa a
    /// apontar direto para ela.
    ///
    /// E uma otimizacao que acontece na LEITURA, o que e incomum: buscar muda a
    /// estrutura. E por isso que a analise amortizada e a unica que faz sentido
    /// aqui, porque uma busca cara deixa as proximas baratas.
    /// </summary>
    public bool ComCompressao { get; }

    /// <summary>
    /// UNIAO POR ALTURA: a arvore mais baixa passa a apontar para a mais alta.
    ///
    /// Sem ela, uma sequencia de unioes na ordem errada constroi uma corrente
    /// de n itens, e buscar a raiz custa n.
    /// </summary>
    public bool PorAltura { get; }

    public Contador Contador { get; } = new();

    public int Raiz(int item)
    {
        if (!ComCompressao)
        {
            while (pai[item] != item)
            {
                Contador.Gastar();
                item = pai[item];
            }
            return item;
        }

        var raiz = item;
        while (pai[raiz] != raiz)
        {
            Contador.Gastar();
            raiz = pai[raiz];
        }

        while (pai[item] != raiz)
        {
            var proximo = pai[item];
            pai[item] = raiz;
            Contador.Gastar();
            item = proximo;
        }

        return raiz;
    }

    public bool Juntar(int a, int b)
    {
        var raizA = Raiz(a);
        var raizB = Raiz(b);
        if (raizA == raizB) return false;

        if (PorAltura)
        {
            if (altura[raizA] < altura[raizB]) (raizA, raizB) = (raizB, raizA);
            pai[raizB] = raizA;
            tamanho[raizA] += tamanho[raizB];
            if (altura[raizA] == altura[raizB]) altura[raizA]++;
        }
        else
        {
            // Sem a uniao por altura, a raiz do segundo sempre vira filha da do
            // primeiro. E a escolha mais ingenua, e e ela que constroi correntes.
            pai[raizB] = raizA;
            tamanho[raizA] += tamanho[raizB];
        }

        Contador.Gastar();
        return true;
    }

    public bool Juntos(int a, int b) => Raiz(a) == Raiz(b);

    /// <summary>A altura real da arvore do item, contada passo a passo.</summary>
    public int Profundidade(int item)
    {
        var passos = 0;
        while (pai[item] != item)
        {
            passos++;
            item = pai[item];
        }
        return passos;
    }

    /// <summary>A maior profundidade do conjunto inteiro.</summary>
    public int MaiorProfundidade() => Enumerable.Range(0, pai.Length).Max(Profundidade);

    /// <summary>Quantos conjuntos existem agora.</summary>
    public int Quantos() => Enumerable.Range(0, pai.Length).Count(i => pai[i] == i);

    /// <summary>
    /// A CORRENTE: a sequencia de unioes que constroi a arvore mais funda
    /// possivel sem uniao por altura.
    ///
    /// Ela nao sai de sorteio. Juntar sempre o item novo com a raiz atual, na
    /// ordem certa, empilha um atras do outro, e e isso que faz a busca custar n.
    /// </summary>
    public static Conjuntos Corrente(int itens, bool comCompressao, bool porAltura)
    {
        var conjuntos = new Conjuntos(itens, comCompressao, porAltura);

        // Sem uniao por altura, Juntar(a, b) poe a raiz de b embaixo da de a.
        // Chamando na ordem (1,0), (2,1), (3,2)... cada item novo vira a raiz e
        // os antigos ficam pendurados numa corrente.
        for (var i = 1; i < itens; i++) conjuntos.Juntar(i, i - 1);
        return conjuntos;
    }

    /// <summary>
    /// Roda a mesma carga nas quatro combinacoes e devolve o total de passos de
    /// ponteiro de cada uma.
    /// </summary>
    public static long Carga(int itens, bool comCompressao, bool porAltura, int buscas)
    {
        var conjuntos = Corrente(itens, comCompressao, porAltura);
        conjuntos.Contador.Zerar();

        for (var i = 0; i < buscas; i++) conjuntos.Raiz(i % itens);
        return conjuntos.Contador.Total;
    }

    /// <summary>
    /// Uma carga com unioes em ordem SORTEADA, que e a que separa a compressao
    /// da uniao por altura.
    ///
    /// Ela existe porque a carga da corrente NAO separa as duas, e isso me
    /// corrigiu: com uniao por altura, a corrente vira uma arvore de altura um,
    /// nao sobra caminho nenhum para comprimir, e as duas ultimas linhas da
    /// tabela ficam identicas. Nao e que a compressao seja inutil ali: e que
    /// aquela carga ja esta resolvida pela outra otimizacao.
    ///
    /// Com unioes sorteadas a arvore fica mais funda, e ai da para ver a
    /// compressao trabalhando.
    /// </summary>
    public static (long Passos, int Profundidade) CargaSorteada(int itens, bool comCompressao, bool porAltura,
                                                               int buscas, ulong semente)
    {
        var sorte = new Aleatorio(semente);
        var conjuntos = new Conjuntos(itens, comCompressao, porAltura);

        for (var i = 0; i < itens - 1; i++)
            conjuntos.Juntar(sorte.Entre(0, itens - 1), sorte.Entre(0, itens - 1));

        var profundidade = conjuntos.MaiorProfundidade();
        conjuntos.Contador.Zerar();

        for (var i = 0; i < buscas; i++) conjuntos.Raiz(sorte.Entre(0, itens - 1));
        return (conjuntos.Contador.Total, profundidade);
    }

    /// <summary>As quatro combinacoes, com nome, para a tabela.</summary>
    public static IEnumerable<(string Nome, bool Compressao, bool Altura)> Todas()
    {
        yield return ("nenhuma das duas", false, false);
        yield return ("so compressao", true, false);
        yield return ("so uniao por altura", false, true);
        yield return ("as duas", true, true);
    }
}
