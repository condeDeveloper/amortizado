using Conde.Amortizado;
using Xunit;

namespace Conde.Amortizado.Testes;

public class PotencialTestes
{
    [Fact]
    public void AFilaRespeitaAOrdemDeEntrada()
    {
        var fila = new FilaComPilhas();
        for (var i = 0; i < 20; i++) fila.Entrar(i);
        for (var i = 0; i < 20; i++) Assert.Equal(i, fila.Sair());
        Assert.True(fila.Vazia);
    }

    [Fact]
    public void AFilaIntercaladaTambemRespeitaAOrdem()
    {
        var fila = new FilaComPilhas();
        fila.Entrar(1);
        fila.Entrar(2);
        Assert.Equal(1, fila.Sair());

        fila.Entrar(3);
        Assert.Equal(2, fila.Sair());
        Assert.Equal(3, fila.Sair());
    }

    [Fact]
    public void SairDaFilaVaziaReclama()
    {
        Assert.Throws<InvalidOperationException>(() => new FilaComPilhas().Sair());
    }

    /// <summary>
    /// O custo medio e constante e a PIOR SAIDA acompanha o tamanho do lote. A
    /// distancia entre os dois numeros e o assunto.
    /// </summary>
    [Fact]
    public void OMedioEhConstanteEOPiorCasoCresce()
    {
        long piorAnterior = 0;

        foreach (var porLote in new[] { 10, 100, 1000, 10_000 })
        {
            var (contador, _, piorSaida) = FilaComPilhas.EmLotes(5, porLote);
            Assert.InRange(contador.Medio, 1.0, 2.0);
            Assert.True(piorSaida > piorAnterior * 5, $"lote {porLote}");
            piorAnterior = piorSaida;
        }
    }

    /// <summary>
    /// Cada item e movido no MAXIMO DUAS vezes na vida: uma ao virar a pilha e
    /// outra ao sair. O custo total de n entradas e n saidas nao passa de 3n.
    /// </summary>
    [Fact]
    public void CadaItemEhMovidoNoMaximoDuasVezes()
    {
        foreach (var porLote in new[] { 100, 1000 })
        {
            var (contador, _, _) = FilaComPilhas.EmLotes(3, porLote);
            var operacoes = 3 * porLote * 2;
            Assert.True(contador.Total <= operacoes * 3 / 2);
        }
    }

    [Fact]
    public void OPotencialDaFilaVale()
    {
        foreach (var porLote in new[] { 10, 100, 1000 })
        {
            var (_, potencial, _) = FilaComPilhas.EmLotes(5, porLote);
            Assert.True(potencial.Vale);
            Assert.True(potencial.MaiorAmortizado <= 2.0001);
        }
    }

    [Fact]
    public void OContadorBinarioContaCerto()
    {
        var contador = new ContadorBinario(10);
        for (var i = 1; i <= 100; i++)
        {
            contador.Somar();
            Assert.Equal(i, contador.Valor);
        }
    }

    /// <summary>
    /// A media converge para DOIS e nunca passa dele. E a serie geometrica
    /// aparecendo sozinha na saida.
    /// </summary>
    [Fact]
    public void AMediaDoContadorConvergeParaDois()
    {
        foreach (var quantos in new[] { 1000, 10_000, 100_000 })
        {
            var (contador, _) = ContadorBinario.Contar(quantos);
            Assert.InRange(contador.Medio, 1.9, 2.0);
        }
    }

    [Fact]
    public void OPiorIncrementoCresceComOLogaritmo()
    {
        var pequeno = ContadorBinario.Contar(1000).Contador.Pico;
        var grande = ContadorBinario.Contar(1_000_000).Contador.Pico;

        Assert.True(grande > pequeno);
        Assert.True(grande < pequeno * 3, "cresce devagar, como logaritmo");
    }

    [Fact]
    public void OPotencialDoContadorVale()
    {
        var (_, potencial) = ContadorBinario.Contar(10_000);
        Assert.True(potencial.Vale);
        Assert.True(potencial.MaiorAmortizado <= 2.0001);
    }

    /// <summary>
    /// As duas condicoes do metodo sao conferidas separadamente, porque elas
    /// falham de maneiras diferentes e uma funcao de potencial ruim quebra so
    /// uma delas.
    /// </summary>
    [Fact]
    public void UmPotencialQueDesceAbaixoDoInicialNaoVale()
    {
        var ruim = new Potencial(0);
        ruim.Registrar(1, -5);
        ruim.Registrar(1, -5);

        Assert.False(ruim.NuncaFicouNegativo);
        Assert.False(ruim.Vale);
    }

    [Fact]
    public void UmPotencialQueSobeSempreLimitaOReal()
    {
        var bom = new Potencial(0);
        bom.Registrar(1, 1);
        bom.Registrar(1, 2);
        bom.Registrar(10, 0);

        Assert.True(bom.NuncaFicouNegativo);
        Assert.True(bom.Limita);
        Assert.Equal(12, bom.SomaReal);
    }

    /// <summary>
    /// Nas tres estruturas, nenhuma operacao tem custo amortizado maior que
    /// tres, e o custo real de algumas delas passa de mil.
    /// </summary>
    [Fact]
    public void NenhumaEstruturaTemAmortizadoMaiorQueTres()
    {
        var (vetorContador, vetor) = VetorDinamico.Inserir(10_000, 2);
        var (filaContador, fila, _) = FilaComPilhas.EmLotes(10, 1000);
        var (binarioContador, binario) = ContadorBinario.Contar(10_000);

        Assert.True(vetor.MaiorAmortizado <= 3.0001);
        Assert.True(fila.MaiorAmortizado <= 3.0001);
        Assert.True(binario.MaiorAmortizado <= 3.0001);

        Assert.True(vetorContador.Pico > 1000);
        Assert.True(filaContador.Pico > 1000);
        Assert.True(binarioContador.Pico < 20);
    }
}
