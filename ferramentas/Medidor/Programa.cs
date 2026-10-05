using Conde.Amortizado;

namespace Conde.Amortizado.Medidor;

/// <summary>
/// As medidas. Contagem exata de operacoes elementares, nunca relogio: o numero
/// que sai aqui e o mesmo em qualquer maquina.
/// </summary>
public static class Programa
{
    public static int Main(string[] argumentos)
    {
        var qual = argumentos.Length > 0 ? argumentos[0] : "tudo";
        switch (qual)
        {
            case "vetor": Vetor(); break;
            case "conjuntos": Conjuntos(); break;
            case "fila": Fila(); break;
            case "contador": Binario(); break;
            case "potencial": Potenciais(); break;
            case "tudo": Vetor(); Conjuntos(); Fila(); Binario(); Potenciais(); break;
            default:
                Console.Error.WriteLine("medidas: vetor, conjuntos, fila, contador, potencial, tudo");
                return 1;
        }
        return 0;
    }

    /// <summary>O fator de crescimento do vetor, que muda a classe de custo.</summary>
    private static void Vetor()
    {
        Console.WriteLine("== o vetor que cresce: o fator muda a classe de custo ==");
        Console.WriteLine($"{"fator",10}{"insercoes",12}{"copias",14}{"custo medio",14}{"pior insercao",16}");

        foreach (var (nome, fator, passo) in new (string, double, int)[]
                 { ("dobra", 2, 1), ("x 1,5", 1.5, 1), ("+ 10", 1, 10), ("+ 1", 1, 1) })
            foreach (var insercoes in new[] { 1000, 4000, 16000 })
            {
                var (contador, _) = VetorDinamico.Inserir(insercoes, fator, 1, passo);
                Console.WriteLine($"{nome,10}{insercoes,12:N0}{contador.Total - insercoes,14:N0}" +
                                  $"{contador.Medio,14:N2}{contador.Pico,16:N0}");
            }

        Console.WriteLine();
        Console.WriteLine("dobrar da custo medio CONSTANTE: 2,02 por insercao, nao importa quantas");
        Console.WriteLine("sejam. Crescer de um em um da custo medio igual a METADE do numero de");
        Console.WriteLine("insercoes, ou seja, o total e quadratico");
        Console.WriteLine();
        Console.WriteLine("e crescer de dez em dez nao conserta: ele so divide o quadratico por dez.");
        Console.WriteLine("Qualquer passo FIXO da quadratico, e qualquer fator MAIOR QUE UM da");
        Console.WriteLine("constante. Nao ha meio termo entre os dois");
        Console.WriteLine();
        Console.WriteLine("a ultima coluna e a outra metade da historia: com o fator dois, uma");
        Console.WriteLine("insercao em dezesseis mil custa 8.193. O custo medio e dois e o pior caso e");
        Console.WriteLine("proporcional a n, e num sistema com prazo e o pior caso que importa");
        Console.WriteLine();

        Console.WriteLine($"  e o espaco que cada fator desperdica logo depois de crescer:");
        Console.WriteLine($"  {"fator",10}{"capacidade",14}{"usado",10}{"desperdicio",14}");

        foreach (var (nome, fator, passo) in new (string, double, int)[]
                 { ("dobra", 2, 1), ("x 1,5", 1.5, 1), ("+ 10", 1, 10) })
        {
            var vetor = new VetorDinamico(fator, 1, passo);
            for (var i = 0; i < 1001; i++) vetor.Acrescentar(i);
            Console.WriteLine($"  {nome,10}{vetor.Capacidade,14:N0}{vetor.Tamanho,10:N0}" +
                              $"{vetor.Desperdicio,14:N0}");
        }

        Console.WriteLine();
        Console.WriteLine("  e a outra ponta da troca: dobrar deixa ate metade do vetor vazio, e");
        Console.WriteLine("  crescer de pouco em pouco quase nao desperdica e paga em copia. Nenhuma");
        Console.WriteLine("  escolha ganha nas duas");
        Console.WriteLine();
    }

    /// <summary>As duas otimizacoes do union-find, que se multiplicam.</summary>
    private static void Conjuntos()
    {
        Console.WriteLine("== conjuntos disjuntos: duas otimizacoes que se multiplicam ==");
        Console.WriteLine("  a carga e a CORRENTE: n unioes em ordem, e depois n buscas");
        Console.WriteLine();
        Console.WriteLine($"{"otimizacoes",-24}{"n = 1.000",16}{"n = 4.000",16}{"n = 16.000",18}");

        foreach (var (nome, compressao, altura) in Amortizado.Conjuntos.Todas())
        {
            Console.Write($"{nome,-24}");
            foreach (var itens in new[] { 1000, 4000, 16000 })
                Console.Write($"{Amortizado.Conjuntos.Carga(itens, compressao, altura, itens),16:N0}");
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine("sem nenhuma das duas, a corrente vira uma lista de n itens e cada busca");
        Console.WriteLine("anda ate o fim: o total e quadratico, 128 milhoes de passos com dezesseis");
        Console.WriteLine("mil itens");
        Console.WriteLine();
        Console.WriteLine("as duas ultimas linhas sao IDENTICAS, e isso me corrigiu. Nesta carga a");
        Console.WriteLine("compressao nao tem o que fazer: a uniao por altura ja deixa a arvore com");
        Console.WriteLine("altura um, e nao sobra caminho nenhum para comprimir. Nao e que ela seja");
        Console.WriteLine("inutil, e que esta carga ja esta resolvida pela outra");
        Console.WriteLine();

        Console.WriteLine("  com unioes em ordem SORTEADA, que deixa a arvore mais funda:");
        Console.WriteLine($"  {"otimizacoes",-24}{"passos",14}{"profundidade",16}");

        foreach (var itens in new[] { 4000, 20000 })
        {
            Console.WriteLine($"  n = {itens:N0}:");
            foreach (var (nome, compressao, altura) in Amortizado.Conjuntos.Todas())
            {
                var (passos, profundidade) = Amortizado.Conjuntos.CargaSorteada(itens, compressao, altura, itens, 7);
                Console.WriteLine($"  {"  " + nome,-24}{passos,14:N0}{profundidade,16:N0}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("  agora as quatro se separam. A profundidade cai de 2.042 para 4, e cada");
        Console.WriteLine("  otimizacao sozinha ja derruba o custo em tres ordens de grandeza; as duas");
        Console.WriteLine("  juntas cortam mais um terco em cima disso");
        Console.WriteLine();
        Console.WriteLine("  o custo das duas juntas e o inverso da funcao de Ackermann, que NAO e");
        Console.WriteLine("  constante e so parece: para qualquer n que caiba no universo ele e menor");
        Console.WriteLine("  que cinco");
        Console.WriteLine();
    }

    /// <summary>A fila com duas pilhas: amortizado dois, pior caso n.</summary>
    private static void Fila()
    {
        Console.WriteLine("== a fila com duas pilhas: amortizado baixo, pior caso alto ==");
        Console.WriteLine($"{"itens por lote",16}{"operacoes",12}{"custo medio",14}{"pior saida",13}{"razao",10}");

        foreach (var porLote in new[] { 10, 100, 1000, 10000 })
        {
            var (contador, _, piorSaida) = FilaComPilhas.EmLotes(5, porLote);
            Console.WriteLine($"{porLote,16:N0}{contador.Chamadas,12:N0}{contador.Medio,14:N2}" +
                              $"{piorSaida,13:N0}{piorSaida / contador.Medio,10:N0}x");
        }

        Console.WriteLine();
        Console.WriteLine("o custo medio e 1,50 em toda linha, e a pior saida acompanha o tamanho do");
        Console.WriteLine("lote. Com lotes de dez mil, uma operacao custa 10.001 e a media continua em");
        Console.WriteLine("1,5: a distancia entre os dois numeros e de quase sete mil vezes");
        Console.WriteLine();
        Console.WriteLine("cada item e movido no maximo DUAS vezes na vida: uma ao virar a pilha e");
        Console.WriteLine("outra ao sair. E dai que sai o custo amortizado, e ele e exato, nao e media");
        Console.WriteLine("de caso tipico");
        Console.WriteLine();
        Console.WriteLine("num sistema com prazo, saber que a media e dois nao ajuda quando a operacao");
        Console.WriteLine("que estourou o prazo foi a que custou dez mil. Amortizado e pior caso");
        Console.WriteLine("respondem perguntas diferentes, e a escolha de qual citar e quase sempre");
        Console.WriteLine("feita sem dizer que houve escolha");
        Console.WriteLine();
    }

    /// <summary>O contador binario, o exemplo mais antigo do assunto.</summary>
    private static void Binario()
    {
        Console.WriteLine("== o contador binario: a serie geometrica aparecendo sozinha ==");
        Console.WriteLine($"{"incrementos",14}{"bits trocados",16}{"media",10}{"pior incremento",18}");

        foreach (var quantos in new[] { 100, 1_000, 10_000, 100_000, 1_000_000 })
        {
            var (contador, _) = ContadorBinario.Contar(quantos);
            Console.WriteLine($"{quantos,14:N0}{contador.Total,16:N0}{contador.Medio,10:N3}{contador.Pico,18}");
        }

        Console.WriteLine();
        Console.WriteLine("a media converge para DOIS e nunca passa dele. A conta e geometrica: o bit");
        Console.WriteLine("zero muda em todo incremento, o bit um em metade deles, o bit dois em um");
        Console.WriteLine("quarto, e a soma dessa serie e dois");
        Console.WriteLine();
        Console.WriteLine("o pior incremento cresce como o logaritmo, porque ele e o numero de uns");
        Console.WriteLine("seguidos no fim, e com um milhao de incrementos ele chega a vinte");
        Console.WriteLine();
    }

    /// <summary>O metodo do potencial, conferido numericamente.</summary>
    private static void Potenciais()
    {
        Console.WriteLine("== o metodo do potencial, conferido em cima dos numeros ==");
        Console.WriteLine();
        Console.WriteLine("  ele e uma tecnica de PROVA: inventa-se uma funcao do estado da estrutura,");
        Console.WriteLine("  o potencial, e define-se o custo amortizado como o custo real mais a");
        Console.WriteLine("  variacao do potencial. A prova precisa de duas coisas:");
        Console.WriteLine();
        Console.WriteLine("    1. o potencial nunca fica abaixo do inicial");
        Console.WriteLine("    2. a soma dos amortizados e pelo menos a soma dos reais");
        Console.WriteLine();
        Console.WriteLine("  normalmente isso e verificado no papel. Aqui as duas condicoes rodam a");
        Console.WriteLine("  cada passo, com os numeros de verdade");
        Console.WriteLine();

        Console.WriteLine($"{"estrutura",-26}{"real",12}{"amortizado",14}{"maior amortizado",18}{"vale",8}");

        var (_, vetor) = VetorDinamico.Inserir(10000, 2);
        Console.WriteLine($"{"vetor que dobra",-26}{vetor.SomaReal,12:N0}{vetor.SomaAmortizada,14:N0}" +
                          $"{vetor.MaiorAmortizado,18:N1}{(vetor.Vale ? "sim" : "NAO"),8}");

        var (_, fila, _) = FilaComPilhas.EmLotes(10, 1000);
        Console.WriteLine($"{"fila com duas pilhas",-26}{fila.SomaReal,12:N0}{fila.SomaAmortizada,14:N0}" +
                          $"{fila.MaiorAmortizado,18:N1}{(fila.Vale ? "sim" : "NAO"),8}");

        var (_, binario) = ContadorBinario.Contar(10000);
        Console.WriteLine($"{"contador binario",-26}{binario.SomaReal,12:N0}{binario.SomaAmortizada,14:N0}" +
                          $"{binario.MaiorAmortizado,18:N1}{(binario.Vale ? "sim" : "NAO"),8}");

        Console.WriteLine();
        Console.WriteLine("a coluna do maior amortizado e o resultado que a tecnica existe para dar:");
        Console.WriteLine("nenhuma operacao, em nenhuma das tres estruturas, tem custo amortizado");
        Console.WriteLine("maior que tres. O custo real de algumas delas passa de mil");
        Console.WriteLine();
        Console.WriteLine("e a coluna 'vale' e a conferencia que o papel costuma pular. Uma funcao de");
        Console.WriteLine("potencial mal escolhida quebra a primeira condicao de maneira SILENCIOSA: a");
        Console.WriteLine("prova parece funcionar e nao prova");
        Console.WriteLine();
    }
}
