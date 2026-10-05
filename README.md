# amortizado

Análise amortizada do zero em C# e .NET 8: vetor que cresce, conjuntos
disjuntos, fila com duas pilhas e contador binário. Nada é medido com relógio:
o que se conta é **operação elementar**, e o método do potencial, que é uma
técnica de prova, vira conferência numérica.

```
$ dotnet medidor.dll vetor

     fator   insercoes        copias   custo medio   pior insercao
     dobra      16,000        16,383          2.02           8,193
     x 1,5      16,000        36,422          3.28          12,139
      + 10      16,000    12,793,600        800.60          15,992
       + 1      16,000   127,992,000      8,000.50          16,000
```

Dobrar dá custo médio **constante**. Crescer de um em um dá custo médio igual à
metade do número de inserções, ou seja, total quadrático. E crescer de dez em dez
não conserta: ele só divide o quadrático por dez.

Qualquer passo **fixo** dá quadrático e qualquer fator **maior que um** dá
constante. Não há meio termo.

## Por que contar operação e não tempo

O relógio mede o cache, o coletor de lixo, o compilador e a temperatura da
máquina, e a diferença entre dobrar e crescer de um em um aparece como ruído no
meio disso tudo. O que este repositório conta é cada cópia de item, cada passo de
ponteiro, cada bit trocado. O número que sai é o mesmo em qualquer máquina.

## Amortizado e pior caso respondem perguntas diferentes

```
$ dotnet medidor.dll fila

  itens por lote   operacoes   custo medio   pior saida     razao
              10         100          1.50           11         7x
           1,000      10,000          1.50        1,001       667x
          10,000     100,000          1.50       10,001     6,667x
```

Numa fila feita com duas pilhas, cada item é movido no máximo **duas vezes** na
vida: uma ao virar a pilha e outra ao sair. Daí sai o custo amortizado, e ele é
exato, não é média de caso típico.

Mas uma única saída pode custar dez mil. Num sistema com prazo, saber que a média
é 1,5 não ajuda quando a operação que estourou o prazo foi a que custou dez mil. A
escolha de qual dos dois números citar é quase sempre feita sem dizer que houve
escolha.

## O que as medidas me corrigiram

**As duas otimizações do union-find empatam na carga que eu tinha escolhido.**

```
$ dotnet medidor.dll conjuntos

  a carga e a CORRENTE: n unioes em ordem, e depois n buscas

otimizacoes                  n = 1.000       n = 4.000        n = 16.000
nenhuma das duas               499,500       7,998,000       127,992,000
so compressao                    2,995          11,995            47,995
so uniao por altura                999           3,999            15,999
as duas                            999           3,999            15,999
```

As duas últimas linhas são **idênticas**. Não é que a compressão de caminho seja
inútil: é que nessa carga a união por altura já deixa a árvore com altura um, e
não sobra caminho nenhum para comprimir.

Com uniões em ordem sorteada, que deixa a árvore mais funda, as quatro se
separam:

```
  n = 20.000:
    nenhuma das duas          22,419,903   profundidade 2,042
    so compressao                 39,715   profundidade     9
    so uniao por altura           37,647   profundidade     6
    as duas                       23,728   profundidade     4
```

A carga certa é parte do resultado, e a primeira que escolhi escondia metade do
achado.

**E a compressão de caminho acontece na leitura.** Buscar a raiz **muda** a
estrutura, o que é incomum: uma busca cara deixa as próximas baratas. Um teste
mostra isso direto: a primeira busca numa corrente de mil custa mais de
quinhentos passos, e a segunda custa um.

É por isso que a análise amortizada é a única que faz sentido ali: analisar uma
busca isolada não diz nada sobre a sequência.

## O método do potencial, conferido

```
$ dotnet medidor.dll potencial

estrutura                         real    amortizado  maior amortizado    vale
vetor que dobra                 26,383        30,000               3.0     sim
fila com duas pilhas            30,000        30,000               2.0     sim
contador binario                19,995        20,000               2.0     sim
```

O método inventa uma função do estado da estrutura, o potencial, e define o custo
amortizado como o custo real mais a variação do potencial. A prova precisa de duas
coisas:

1. o potencial nunca fica abaixo do inicial;
2. a soma dos amortizados é pelo menos a soma dos reais.

Normalmente isso é verificado no papel. Aqui as duas condições rodam a cada passo,
com os números de verdade. A coluna do maior amortizado é o resultado que a técnica
existe para dar: nenhuma operação, em nenhuma das três estruturas, custa mais que
três, e o custo real de algumas delas passa de mil.

A coluna "vale" é a conferência que o papel costuma pular. Uma função de potencial
mal escolhida quebra a primeira condição de maneira **silenciosa**: a prova parece
funcionar e não prova.

## O contador binário

```
$ dotnet medidor.dll contador

   incrementos   bits trocados     media   pior incremento
         1,000           1,994     1.994                10
       100,000         199,994     2.000                17
     1,000,000       1,999,993     2.000                20
```

É o exemplo mais antigo do assunto e o mais fácil de conferir na mão. A média
converge para **dois** e nunca passa dele, e a conta é geométrica: o bit zero muda
em todo incremento, o bit um em metade deles, o bit dois em um quarto, e a soma
dessa série é dois.

## As peças

| arquivo | o que faz |
| --- | --- |
| `Contador.cs` | a contagem exata de operações, e o verificador do potencial |
| `VetorDinamico.cs` | o fator de crescimento como chave, e o espaço desperdiçado |
| `Conjuntos.cs` | compressão de caminho e união por altura, ligáveis uma a uma |
| `FilaComPilhas.cs` | a fila com duas pilhas e o contador binário |

## Como rodar

```
dotnet test testes/Amortizado.Testes/Amortizado.Testes.csproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

As medidas aceitam `vetor`, `conjuntos`, `fila`, `contador`, `potencial` e
`tudo`.

## Licença

MIT.
