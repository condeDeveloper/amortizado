namespace Conde.Amortizado;

/// <summary>
/// A ARVORE SPLAY, de Sleator e Tarjan, 1985: uma arvore de busca que nao se
/// mantem equilibrada e mesmo assim custa o logaritmo.
///
/// Ela nao guarda altura, nao guarda cor e nao guarda contador nenhum. A unica
/// coisa que ela faz e, a cada acesso, trazer o item acessado para a RAIZ por
/// uma sequencia de rotacoes. E so isso.
///
/// O pior caso de uma operacao e n: com as chaves inseridas em ordem crescente,
/// a arvore vira uma corrente e a primeira busca no fundo percorre tudo. O custo
/// AMORTIZADO e o logaritmo, e e ele que importa, porque aquela busca cara
/// deixa a arvore muito mais rasa para as proximas.
///
/// E a mesma ideia da compressao de caminho dos conjuntos disjuntos: acessar
/// MUDA a estrutura, e por isso analisar um acesso isolado nao diz nada sobre a
/// sequencia.
/// </summary>
public sealed class Splay
{
    private sealed class No
    {
        public No(int chave) => Chave = chave;

        public int Chave { get; }
        public No? Esquerda { get; set; }
        public No? Direita { get; set; }
        public No? Pai { get; set; }
    }

    private No? raiz;

    public Contador Contador { get; } = new();

    public int Tamanho { get; private set; }

    /// <summary>Quantas rotacoes foram feitas desde o comeco.</summary>
    public long Rotacoes { get; private set; }

    /// <summary>A altura da arvore, contada de verdade.</summary>
    public int Altura => Fundura(raiz);

    private static int Fundura(No? no) =>
        no is null ? 0 : 1 + Math.Max(Fundura(no.Esquerda), Fundura(no.Direita));

    public void Inserir(int chave)
    {
        if (raiz is null)
        {
            raiz = new No(chave);
            Tamanho++;
            Contador.Gastar();
            Contador.Fechar();
            return;
        }

        var atual = raiz;
        No pai;

        while (true)
        {
            Contador.Gastar();
            pai = atual;

            if (chave == atual.Chave)
            {
                Subir(atual);
                Contador.Fechar();
                return;
            }

            var proximo = chave < atual.Chave ? atual.Esquerda : atual.Direita;
            if (proximo is null) break;
            atual = proximo;
        }

        var novo = new No(chave) { Pai = pai };
        if (chave < pai.Chave) pai.Esquerda = novo;
        else pai.Direita = novo;

        Tamanho++;
        Subir(novo);
        Contador.Fechar();
    }

    public bool Buscar(int chave)
    {
        var atual = raiz;
        No? ultimo = null;

        while (atual is not null)
        {
            Contador.Gastar();
            ultimo = atual;
            if (chave == atual.Chave) break;
            atual = chave < atual.Chave ? atual.Esquerda : atual.Direita;
        }

        // O item NAO encontrado tambem sobe, na pessoa do ultimo no visitado.
        // Deixar de subir ali quebra a analise: a busca que falhou gastou o
        // mesmo caminho e precisa pagar a mesma melhoria na estrutura.
        if (ultimo is not null) Subir(ultimo);

        Contador.Fechar();
        return atual is not null;
    }

    /// <summary>
    /// Traz o no para a raiz, pelos tres casos de Sleator e Tarjan.
    ///
    /// O caso ZIG-ZIG, em que o no e o pai estao do mesmo lado, roda o AVO
    /// PRIMEIRO, e nao o pai. Essa ordem parece arbitraria e e o que faz a
    /// analise fechar: rodando o pai primeiro, a arvore sobe o no do mesmo
    /// jeito e NAO encurta o caminho dos outros, e o custo amortizado deixa de
    /// ser o logaritmo.
    /// </summary>
    private void Subir(No no)
    {
        while (no.Pai is not null)
        {
            var pai = no.Pai;
            var avo = pai.Pai;

            if (avo is null)
            {
                Rodar(no);
            }
            else if ((no == pai.Esquerda) == (pai == avo.Esquerda))
            {
                Rodar(pai);
                Rodar(no);
            }
            else
            {
                Rodar(no);
                Rodar(no);
            }
        }

        raiz = no;
    }

    private void Rodar(No no)
    {
        var pai = no.Pai!;
        var avo = pai.Pai;

        if (no == pai.Esquerda)
        {
            pai.Esquerda = no.Direita;
            if (no.Direita is not null) no.Direita.Pai = pai;
            no.Direita = pai;
        }
        else
        {
            pai.Direita = no.Esquerda;
            if (no.Esquerda is not null) no.Esquerda.Pai = pai;
            no.Esquerda = pai;
        }

        pai.Pai = no;
        no.Pai = avo;

        if (avo is null) raiz = no;
        else if (avo.Esquerda == pai) avo.Esquerda = no;
        else avo.Direita = no;

        Rotacoes++;
    }

    /// <summary>As chaves em ordem, para conferir que a arvore continua de busca.</summary>
    public List<int> EmOrdem()
    {
        var saida = new List<int>();
        Percorrer(raiz, saida);
        return saida;
    }

    private static void Percorrer(No? no, List<int> saida)
    {
        if (no is null) return;
        Percorrer(no.Esquerda, saida);
        saida.Add(no.Chave);
        Percorrer(no.Direita, saida);
    }

    /// <summary>
    /// A carga que constroi a CORRENTE: inserir em ordem crescente.
    ///
    /// Numa arvore de busca comum isso e o pior caso e fica assim para sempre.
    /// Aqui a corrente existe, e a primeira busca no fundo ja a desmonta.
    /// </summary>
    public static Splay EmOrdem(int chaves)
    {
        var arvore = new Splay();
        for (var i = 0; i < chaves; i++) arvore.Inserir(i);
        return arvore;
    }

    /// <summary>
    /// A carga que a arvore splay foi feita para: acessos CONCENTRADOS em poucas
    /// chaves.
    ///
    /// Uma arvore equilibrada custa o logaritmo em todo acesso, sempre. A splay
    /// se adapta: as chaves mais pedidas sobem e ficam perto da raiz, e o custo
    /// delas cai para quase nada. E a propriedade que nenhuma arvore de
    /// equilibrio fixo tem.
    /// </summary>
    public static (long Passos, int Altura) Concentrado(int chaves, int acessos, int quentes, ulong semente)
    {
        var sorte = new Aleatorio(semente);
        var arvore = EmOrdem(chaves);
        arvore.Contador.Zerar();

        for (var i = 0; i < acessos; i++)
            arvore.Buscar(sorte.Entre(0, quentes - 1));

        return (arvore.Contador.Total, arvore.Altura);
    }

    /// <summary>A mesma contagem com acessos espalhados por todas as chaves.</summary>
    public static (long Passos, int Altura) Espalhado(int chaves, int acessos, ulong semente)
    {
        var sorte = new Aleatorio(semente);
        var arvore = EmOrdem(chaves);
        arvore.Contador.Zerar();

        for (var i = 0; i < acessos; i++) arvore.Buscar(sorte.Entre(0, chaves - 1));

        return (arvore.Contador.Total, arvore.Altura);
    }
}
