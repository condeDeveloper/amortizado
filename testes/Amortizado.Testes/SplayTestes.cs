using Conde.Amortizado;
using Xunit;

namespace Conde.Amortizado.Testes;

public class SplayTestes
{
    [Fact]
    public void AArvoreContinuaSendoDeBusca()
    {
        var arvore = new Splay();
        foreach (var chave in new[] { 5, 2, 8, 1, 9, 3, 7, 4, 6 }) arvore.Inserir(chave);

        Assert.Equal(Enumerable.Range(1, 9), arvore.EmOrdem());
        Assert.Equal(9, arvore.Tamanho);
    }

    [Fact]
    public void BuscarAchaOQueFoiInseridoENaoInventa()
    {
        var arvore = Splay.EmOrdem(100);

        for (var i = 0; i < 100; i++) Assert.True(arvore.Buscar(i));
        Assert.False(arvore.Buscar(100));
        Assert.False(arvore.Buscar(-1));
    }

    [Fact]
    public void InserirDuasVezesNaoDuplica()
    {
        var arvore = new Splay();
        arvore.Inserir(7);
        arvore.Inserir(7);

        Assert.Equal(1, arvore.Tamanho);
        Assert.Equal(new[] { 7 }, arvore.EmOrdem());
    }

    /// <summary>
    /// Inserir em ordem crescente constroi uma CORRENTE, e esse e o pior caso de
    /// toda arvore de busca sem equilibrio.
    /// </summary>
    [Fact]
    public void InserirEmOrdemConstroiUmaCorrente()
    {
        var arvore = Splay.EmOrdem(500);
        Assert.Equal(500, arvore.Altura);
    }

    /// <summary>
    /// E a primeira busca no fundo ja desmonta a corrente: ela custa n, e a
    /// seguinte custa UM. E a mesma ideia da compressao de caminho.
    /// </summary>
    [Fact]
    public void APrimeiraBuscaNoFundoDesmontaACorrente()
    {
        var arvore = Splay.EmOrdem(1000);
        Assert.Equal(1000, arvore.Altura);

        arvore.Contador.Zerar();
        arvore.Buscar(0);
        var primeira = arvore.Contador.Total;

        arvore.Contador.Zerar();
        arvore.Buscar(0);
        var segunda = arvore.Contador.Total;

        Assert.Equal(1000, primeira);
        Assert.Equal(1, segunda);
        Assert.True(arvore.Altura < 600, $"a altura deveria ter caido, deu {arvore.Altura}");
    }

    /// <summary>
    /// A propriedade que nenhuma arvore de equilibrio fixo tem: com os acessos
    /// CONCENTRADOS em poucas chaves, o custo por acesso nao cresce com o
    /// tamanho da arvore.
    /// </summary>
    [Fact]
    public void ComAcessosConcentradosOCustoNaoCresceComAArvore()
    {
        var (pequena, _) = Splay.Concentrado(1000, 1000, 10, 7);
        var (grande, _) = Splay.Concentrado(4000, 4000, 10, 7);

        var porAcessoPequena = pequena / 1000.0;
        var porAcessoGrande = grande / 4000.0;

        Assert.InRange(porAcessoGrande / porAcessoPequena, 0.8, 1.2);
        Assert.True(porAcessoGrande < 8, $"deu {porAcessoGrande:N1} por acesso");
    }

    /// <summary>
    /// E quanto mais concentrado, mais barato. Uma arvore equilibrada cobraria o
    /// logaritmo em todos os tres casos.
    /// </summary>
    [Fact]
    public void QuantoMaisConcentradoMaisBarato()
    {
        var (dez, _) = Splay.Concentrado(4000, 4000, 10, 7);
        var (cem, _) = Splay.Concentrado(4000, 4000, 100, 7);
        var (todas, _) = Splay.Espalhado(4000, 4000, 7);

        Assert.True(dez < cem);
        Assert.True(cem < todas);
    }

    /// <summary>
    /// Com acessos espalhados ela se equilibra sozinha: a altura cai de 4.000
    /// para algumas dezenas, sem guardar altura, cor nem contador nenhum.
    /// </summary>
    [Fact]
    public void ComAcessosEspalhadosElaSeEquilibraSozinha()
    {
        var (_, altura) = Splay.Espalhado(4000, 4000, 7);
        Assert.True(altura < 60, $"a altura ficou em {altura}");
    }

    [Fact]
    public void OCustoPorAcessoEspalhadoFicaNaOrdemDoLogaritmo()
    {
        var (passos, _) = Splay.Espalhado(4000, 4000, 7);
        var porAcesso = passos / 4000.0;
        var logaritmo = Math.Log2(4000);

        Assert.InRange(porAcesso / logaritmo, 0.5, 2.5);
    }

    [Fact]
    public void ABuscaQueFalhaTambemSobeOUltimoVisitado()
    {
        var arvore = Splay.EmOrdem(200);
        arvore.Buscar(500);

        // Se o ultimo visitado nao subisse, a altura continuaria a mesma da
        // corrente e a busca seguinte pagaria tudo de novo.
        Assert.True(arvore.Altura < 200);
    }

    [Fact]
    public void AsRotacoesSaoContadas()
    {
        var arvore = Splay.EmOrdem(100);
        Assert.True(arvore.Rotacoes > 0);

        var antes = arvore.Rotacoes;
        arvore.Buscar(0);
        Assert.True(arvore.Rotacoes > antes);
    }

    [Fact]
    public void AArvoreVaziaNaoQuebra()
    {
        var arvore = new Splay();
        Assert.False(arvore.Buscar(1));
        Assert.Empty(arvore.EmOrdem());
        Assert.Equal(0, arvore.Altura);
    }
}
