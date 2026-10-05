using Conde.Amortizado;
using Xunit;

namespace Conde.Amortizado.Testes;

public class VetorTestes
{
    [Fact]
    public void OVetorGuardaOQueFoiPosto()
    {
        var vetor = new VetorDinamico();
        for (var i = 0; i < 100; i++) vetor.Acrescentar(i * 3);

        Assert.Equal(100, vetor.Tamanho);
        for (var i = 0; i < 100; i++) Assert.Equal(i * 3, vetor[i]);
    }

    [Fact]
    public void OContadorFechaUmaChamadaPorInsercao()
    {
        var vetor = new VetorDinamico();
        for (var i = 0; i < 50; i++) vetor.Acrescentar(i);
        Assert.Equal(50, vetor.Contador.Chamadas);
    }

    /// <summary>
    /// Dobrar da custo medio CONSTANTE: ele nao cresce com o numero de
    /// insercoes.
    /// </summary>
    [Fact]
    public void DobrarDaCustoMedioConstante()
    {
        foreach (var insercoes in new[] { 1000, 10_000, 100_000 })
        {
            var (contador, _) = VetorDinamico.Inserir(insercoes, 2);
            Assert.InRange(contador.Medio, 2.0, 3.0);
        }
    }

    /// <summary>
    /// E crescer de um em um da custo medio igual a METADE das insercoes, ou
    /// seja, total quadratico.
    /// </summary>
    [Fact]
    public void CrescerDeUmEmUmDaCustoQuadratico()
    {
        foreach (var insercoes in new[] { 100, 1000, 4000 })
        {
            var (contador, _) = VetorDinamico.Inserir(insercoes, 1, 1, 1);
            Assert.InRange(contador.Medio, insercoes / 2.0 * 0.9, insercoes / 2.0 * 1.1);
        }
    }

    /// <summary>
    /// Passo fixo MAIOR nao conserta, so divide o quadratico. Qualquer passo
    /// fixo da quadratico e qualquer fator maior que um da constante.
    /// </summary>
    [Fact]
    public void PassoFixoMaiorSoDivideOQuadratico()
    {
        var (deUm, _) = VetorDinamico.Inserir(4000, 1, 1, 1);
        var (deDez, _) = VetorDinamico.Inserir(4000, 1, 1, 10);

        Assert.InRange(deUm.Medio / deDez.Medio, 8, 12);
        Assert.True(deDez.Medio > 100, "continua crescendo com n");
    }

    [Fact]
    public void QualquerFatorMaiorQueUmDaConstante()
    {
        foreach (var fator in new[] { 1.1, 1.5, 2.0, 3.0 })
        {
            var (pequeno, _) = VetorDinamico.Inserir(1000, fator);
            var (grande, _) = VetorDinamico.Inserir(20_000, fator);
            Assert.InRange(grande.Medio / pequeno.Medio, 0.5, 2.0);
        }
    }

    /// <summary>
    /// O custo medio e constante e o PIOR CASO e proporcional a n. Os dois
    /// numeros juntos sao o assunto inteiro.
    /// </summary>
    [Fact]
    public void OPiorCasoCresceComOTamanho()
    {
        var (pequeno, _) = VetorDinamico.Inserir(1000, 2);
        var (grande, _) = VetorDinamico.Inserir(16_000, 2);

        Assert.True(grande.Pico > pequeno.Pico * 8);
        Assert.InRange(grande.Medio, 2.0, 3.0);
    }

    /// <summary>
    /// O metodo do potencial vale: o potencial nunca fica negativo e a soma dos
    /// amortizados limita a dos reais.
    /// </summary>
    [Fact]
    public void OPotencialDoVetorVale()
    {
        foreach (var insercoes in new[] { 100, 1000, 10_000 })
        {
            var (_, potencial) = VetorDinamico.Inserir(insercoes, 2);
            Assert.True(potencial.NuncaFicouNegativo, $"potencial minimo {potencial.Menor}");
            Assert.True(potencial.Limita);
            Assert.True(potencial.MaiorAmortizado <= 3.0001, $"maior amortizado {potencial.MaiorAmortizado}");
        }
    }

    [Fact]
    public void ODesperdicioDeEspacoEhAOutraPontaDaTroca()
    {
        var dobra = new VetorDinamico(2);
        var dePouco = new VetorDinamico(1, 1, 10);

        for (var i = 0; i < 1001; i++)
        {
            dobra.Acrescentar(i);
            dePouco.Acrescentar(i);
        }

        Assert.True(dobra.Desperdicio > dePouco.Desperdicio * 5);
    }

    [Fact]
    public void OsParametrosInvalidosReclamam()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VetorDinamico(0.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => new VetorDinamico(2, 0));
    }

    [Fact]
    public void OContadorSomaCertoEZeraCerto()
    {
        var contador = new Contador();
        contador.Gastar(5);
        contador.Fechar();
        contador.Gastar(3);
        contador.Fechar();

        Assert.Equal(8, contador.Total);
        Assert.Equal(2, contador.Chamadas);
        Assert.Equal(5, contador.Pico);
        Assert.Equal(4, contador.Medio, 1e-9);

        contador.Zerar();
        Assert.Equal(0, contador.Total);
        Assert.Equal(0, contador.Chamadas);
    }
}
