using Conde.Amortizado;
using Xunit;

namespace Conde.Amortizado.Testes;

public class ConjuntosTestes
{
    [Fact]
    public void CadaItemComecaSozinho()
    {
        var conjuntos = new Conjuntos(10);
        Assert.Equal(10, conjuntos.Quantos());
        for (var i = 0; i < 10; i++)
            for (var j = i + 1; j < 10; j++)
                Assert.False(conjuntos.Juntos(i, j));
    }

    [Fact]
    public void JuntarFazOsDoisFicaremJuntos()
    {
        var conjuntos = new Conjuntos(5);
        Assert.True(conjuntos.Juntar(0, 1));
        Assert.True(conjuntos.Juntos(0, 1));
        Assert.Equal(4, conjuntos.Quantos());
    }

    [Fact]
    public void JuntarDeNovoNaoFazNada()
    {
        var conjuntos = new Conjuntos(5);
        conjuntos.Juntar(0, 1);
        Assert.False(conjuntos.Juntar(1, 0));
        Assert.Equal(4, conjuntos.Quantos());
    }

    [Fact]
    public void AUniaoEhTransitiva()
    {
        var conjuntos = new Conjuntos(6);
        conjuntos.Juntar(0, 1);
        conjuntos.Juntar(1, 2);
        conjuntos.Juntar(3, 4);

        Assert.True(conjuntos.Juntos(0, 2));
        Assert.False(conjuntos.Juntos(0, 3));
        Assert.True(conjuntos.Juntos(3, 4));
    }

    /// <summary>
    /// As quatro combinacoes devolvem o MESMO agrupamento. Elas mudam o custo e
    /// nao o resultado, e sem este teste as outras medidas poderiam estar
    /// comparando estruturas que nem fazem a mesma coisa.
    /// </summary>
    [Fact]
    public void AsQuatroCombinacoesDaoOMesmoAgrupamento()
    {
        foreach (var (nome, compressao, altura) in Conjuntos.Todas())
        {
            var conjuntos = Conjuntos.Corrente(50, compressao, altura);
            Assert.Equal(1, conjuntos.Quantos());

            for (var i = 1; i < 50; i++) Assert.True(conjuntos.Juntos(0, i), nome);
        }
    }

    /// <summary>
    /// Sem nenhuma otimizacao, a corrente da custo QUADRATICO: dobrar o tamanho
    /// multiplica o custo por quatro.
    /// </summary>
    [Fact]
    public void SemOtimizacaoACorrenteDaQuadratico()
    {
        var pequeno = Conjuntos.Carga(1000, false, false, 1000);
        var grande = Conjuntos.Carga(2000, false, false, 2000);
        Assert.InRange(grande / (double)pequeno, 3.5, 4.5);
    }

    [Fact]
    public void QualquerUmaDasDuasJaResolveACorrente()
    {
        var sem = Conjuntos.Carga(4000, false, false, 4000);

        foreach (var (nome, compressao, altura) in Conjuntos.Todas())
        {
            if (!compressao && !altura) continue;
            Assert.True(Conjuntos.Carga(4000, compressao, altura, 4000) < sem / 100, nome);
        }
    }

    /// <summary>
    /// O que me corrigiu: na carga da corrente, a uniao por altura sozinha ja
    /// empata com as duas juntas. Nao e que a compressao seja inutil: e que
    /// aquela carga ja esta resolvida pela outra otimizacao.
    /// </summary>
    [Fact]
    public void NaCorrenteAUniaoPorAlturaSozinhaJaEmpataComAsDuas()
    {
        var soAltura = Conjuntos.Carga(4000, false, true, 4000);
        var asDuas = Conjuntos.Carga(4000, true, true, 4000);
        Assert.Equal(soAltura, asDuas);
    }

    /// <summary>
    /// E com unioes sorteadas as quatro se separam, com as duas juntas ganhando.
    /// </summary>
    [Fact]
    public void ComUnioesSorteadasAsQuatroSeSeparam()
    {
        var (semNada, profundoSemNada) = Conjuntos.CargaSorteada(4000, false, false, 4000, 7);
        var (soCompressao, _) = Conjuntos.CargaSorteada(4000, true, false, 4000, 7);
        var (soAltura, _) = Conjuntos.CargaSorteada(4000, false, true, 4000, 7);
        var (asDuas, profundoAsDuas) = Conjuntos.CargaSorteada(4000, true, true, 4000, 7);

        Assert.True(semNada > soCompressao * 50);
        Assert.True(semNada > soAltura * 50);
        Assert.True(asDuas < soCompressao);
        Assert.True(asDuas < soAltura);

        Assert.True(profundoSemNada > 100, $"sem nada a profundidade deu {profundoSemNada}");
        Assert.True(profundoAsDuas <= 5, $"com as duas a profundidade deu {profundoAsDuas}");
    }

    /// <summary>
    /// A compressao acontece na LEITURA: buscar MUDA a estrutura, e e por isso
    /// que a analise amortizada e a unica que faz sentido aqui.
    /// </summary>
    [Fact]
    public void BuscarComCompressaoEncurtaOCaminho()
    {
        var conjuntos = Conjuntos.Corrente(100, comCompressao: true, porAltura: false);
        var antes = conjuntos.Profundidade(0);

        conjuntos.Raiz(0);
        var depois = conjuntos.Profundidade(0);

        Assert.True(antes > 50, $"a corrente deveria ser funda, deu {antes}");
        Assert.Equal(1, depois);
    }

    [Fact]
    public void BuscarSemCompressaoNaoMudaNada()
    {
        var conjuntos = Conjuntos.Corrente(100, comCompressao: false, porAltura: false);
        var antes = conjuntos.Profundidade(0);

        conjuntos.Raiz(0);
        Assert.Equal(antes, conjuntos.Profundidade(0));
    }

    [Fact]
    public void ASegundaBuscaEhBaratissimaDepoisDaPrimeira()
    {
        var conjuntos = Conjuntos.Corrente(1000, comCompressao: true, porAltura: false);
        conjuntos.Contador.Zerar();

        conjuntos.Raiz(0);
        var primeira = conjuntos.Contador.Total;

        conjuntos.Contador.Zerar();
        conjuntos.Raiz(0);
        var segunda = conjuntos.Contador.Total;

        Assert.True(primeira > 500);
        Assert.True(segunda <= 1);
    }
}
