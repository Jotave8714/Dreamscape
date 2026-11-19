# Fase 17: Despertar

Cena: `Assets/Scenes/Fase17_Despertar.unity`, gerada por `Assets/Scripts/Editor/Phases/Fase17Builder.cs`. Não confundir com `Fase7_Despertar`, o falso despertar. O tema é próprio da classe (`Fase17Builder.Tema`): o amanhecer, a fase **mais clara e colorida do jogo**. A saturação é 30, a exposição 0,3, a neblina é clara e rala (0,01) e o sol é quente e baixo (intensidade 1,7, elevação 24°), nascendo à frente do jogador. O destaque é **pêssego de aurora** (1; 0,66; 0,55).

## Objetivo

É o despertar de verdade e a última fase do jogo. A biblioteca do sonho se enche da luz da manhã, e o caminho vira um **corredor de lembranças**: cada trecho revisita uma ideia de uma fase anterior, da 3 à 16, em ordem crescente de dificuldade. O fim é o livro aberto gigante onde o estudante adormeceu. Ali ficam o canto de estudo (mesa, abajur, tapete e pilha de livros) e a porta de saída do sonho, sob um sol que é um relógio voltando a andar. Junto com a Fase 16, é uma das duas fases mais difíceis do jogo.

O pufe só é criado pelo `Spawn` (não há método público para ele), por isso ele fica no começo da fase, e o fim reproduz o canto de estudo sem o pufe.

## As lembranças

| Ato | Trecho | Fase lembrada | Ideia |
| --- | --- | --- | --- |
| I (y 0) | Cela 1 | 3 Corredor das Luzes | Espinhos fixos: desviar pelo lado livre. |
| I | Cela 2 | 9 Estantes Sem Fim e 3 | Divisórias de estantes com aberturas alternadas. A linha reta entre elas cruza um **piso escuro** que cede sobre um fosso raso e fechado: onde há luz, há chão. |
| I | Cela 3 | 8 Saguão dos Sussurros | Mesas como degraus até a plataforma de 1,6 m. |
| II (y 1,6) | Carteiras | 4 Bloco de Aulas | Três pisos que cedem: andando cai, correndo passa. |
| II | Vão da pressa | 7 Falso Despertar | Vão de 4,4 m que só a **Velocidade** vence. |
| III | Laboratório | 5 Laboratório | **Escudo** opcional e um laser com outro ritmo. |
| III | Corredor dos feixes | 13 Ala Proibida | Dois feixes em onda. |
| IV | Oficina | 6 Oficina das Serras | Serra que vai e volta: passe pelo lado vazio. |
| IV | Galeria do relógio | 14 Torre do Relógio | Três pêndulos **sincronizados**. |
| IV | Arquivo | 12 Arquivo das Lembranças | Prensas em onda e uma página que cede. |
| V (y 2,1–3,1) | Ilhas de leitura | 11 Sala de Leitura Flutuante | Livros abertos com mesas, flutuando, cada um 0,5 m mais alto. |
| V (y 3,6) | Mar de páginas | 15 Mar de Páginas | **Pluma** e vãos de 5,2 m sobre páginas brilhando. |
| VI (y 3,6–8,7) | Escadaria | 10 Escadaria de Livros | Degraus de +1,7 m com o **Pulo Duplo**. |
| VI (y 8,7) | Trilho da manhã | 16 Observatório dos Sonhos | A serra corre ao longo de um trilho estreito e é saltada com o pulo duplo. |

**Ritmo:** apresentar sem risco (fosso raso, mesas), testar (carteiras, vão), apertar o *timing* com refúgios entre as armadilhas (atos III e IV), poderes de longo alcance (ato V) e o desafio final no ponto mais alto (ato VI).

**Marcos e linha de visão.** O percurso é quase reto em +Z (desvio lateral máximo de 2,4 m dentro das salas), e a porta fica no ponto mais alto. O sol (`DreamClock`, raio 6) nasce em (0, 18, 186), atrás da porta, e aparece pelo portal da entrada acima das estantes. A neblina rala (0,01) deixa ver o fim desde as ilhas de leitura. O relógio da galeria (z 84) marca o meio da fase.

**Leitura da rota.** A luz quente e branca marca o chão firme. Os pisos que cedem (o piso escuro, as carteiras, a página do arquivo e a página solta) não têm luminária em cima. Lilás marca o trecho da Pluma; azul-claro, o do pulo duplo; luz quente sobre os corredores marca os refúgios entre as armadilhas.

Placas (sem acentos): `FASE 17` / `DESPERTAR` / `LEMBRE DE TUDO O QUE APRENDEU`, `ONDE HA LUZ, HA CHAO`, `AS MESAS SAO DEGRAUS`, `CORRA: AS CARTEIRAS CEDEM`, `4,4 M: SO COM PRESSA`, `ESCUDO: UM ERRO PERDOADO`, `PASSE PELO LADO VAZIO`, `MAR DE PAGINAS: PLUMA`, `PULO DUPLO: SUBA E SALTE`, `A ULTIMA SERRA` / `SALTE QUANDO ELA VIER` e, no fim, `BOM DIA` / `VOCE ACORDOU`.

## Poderes

Cada tipo de poder aparece **uma vez só**. Um segundo coletável igual teria o efeito desfeito quando o primeiro acabasse.

| Poder | Onde | Exigido em | Distância e tempo |
| --- | --- | --- | --- |
| Velocidade | (0; 1,6; 21,6), plataforma da pressa | Vão de 4,4 m (z 40,7 → 45,1) | 19 m, ~3 s; sobram ~27 s. |
| Escudo | (−1,9; 1,6; 48,4), laboratório | Nunca: absorve um toque de armadilha | Opcional, fora da linha reta. |
| Pluma | (0; 3,1; 113,8), cais | Três vãos de 5,2 m (z 115,3 → 136,1) | 22 m e três saltos, ~6 s. |
| Pulo duplo | (0; 3,6; 138,6), base da escadaria | Três degraus de +1,7 m e a serra | ~3 s de subida e no máximo 4,5 s de espera pela serra. |

Se o efeito acabar no meio (por exemplo, parado numa ilha do mar de páginas), o coletável reaparece no lugar original. Quem estiver nas ilhas cai e recomeça. Quem estiver no patamar da manhã pode descer os degraus até a base.

## Plataformas (topo em Y, medidas X × Z)

| Peça | Tipo | Centro do topo | Tamanho | Extensão |
| --- | --- | --- | --- | --- |
| R0 Entrada | `Platform` | (0, 0, 0) | 8 × 6 | z −3..3 |
| Cela 1 | `Platform` | (0, 0, 5) | 8 × 4 | z 3..7 |
| Cela 2 (sul, norte, oeste, leste) | `Platform` | anel em volta de (0, 0, 9,5) | faixas de 1,5 m e blocos 3 × 2 | z 7..12 |
| Piso escuro | `CrumblingPlatform` | (0; 0; 9,5) | 2 × 2 | x −1..1; z 8,5..10,5 |
| Fosso | `Platform` | (0; −0,8; 9,5) | 2 × 2 | sob o piso escuro |
| Cela 3 Sala das mesas | `Platform` | (0, 0, 15) | 8 × 6 | z 12..18 |
| Mesas degrau | `ReadingDesk` (topo 0,8) | (±2,5; 0; 16,6) | 1,6 × 0,8 | z 16,2..17,0 |
| L Plataforma da pressa | `BookPlatform` | (0; 1,6; 20,5) | 8 × 5 | z 18..23 |
| C1, C2, C3 Carteiras | `CrumblingPlatform` | (0; 1,6; 26,05 / 30,35 / 34,65) | 2,5 × 2,5 | vãos de 1,8 m |
| Lp Lançamento | `BookPlatform` | (0; 1,6; 39,2) | 3 × 3 | z 37,7..40,7 |
| V Bancada do laboratório | `Platform` | (0; 1,6; 47,6) | 6 × 5 | z 45,1..50,1 |
| Corredor dos feixes | `Platform` | (0; 1,6; 56,6) | 3,9 × 13 | z 50,1..63,1 |
| W1 Oficina | `Platform` | (0; 1,6; 66,6) | 6 × 7 | z 63,1..70,1 |
| W2 Galeria do relógio | `Platform` | (0; 1,6; 76,1) | 6 × 12 | z 70,1..82,1 |
| W3 Arquivo | `Platform` | (0; 1,6; 86,1) | 3,2 × 8 | z 82,1..90,1 |
| CF Página do arquivo | `CrumblingPlatform` | (0; 1,6; 91,6) | 2 × 3 | z 90,1..93,1 |
| X1 Ilha de leitura | `OpenBookPlatform` | (0; 2,1; 96,9) | 3 × 3 | z 95,4..98,4 |
| X2 Ilha de leitura | `OpenBookPlatform` | (1,5; 2,6; 102,2) | 3 × 3 | x 0..3; z 100,7..103,7 |
| X3 Ilha de leitura | `OpenBookPlatform` | (−1,5; 3,1; 107,5) | 3 × 3 | x −3..0; z 106..109 |
| Cais da Pluma | `OpenBookPlatform` | (0; 3,1; 113,3) | 6 × 4 | z 111,3..115,3 |
| Y1 Ilha de páginas | `BookPlatform` | (0; 3,6; 121,8) | 2,6 × 2,6 | z 120,5..123,1 |
| Y2 Página solta | `CrumblingPlatform` | (0; 3,6; 129,6) | 2,6 × 2,6 | z 128,3..130,9 |
| Base da escadaria | `Platform` | (0; 3,6; 138,1) | 6 × 4 | z 136,1..140,1 |
| S1 Degrau | `BookPlatform` | (1,5; 5,3; 142,5) | 2,6 × 2,4 | x 0,2..2,8; z 141,3..143,7 |
| S2 Degrau | `BookPlatform` | (−1,5; 7,0; 146,1) | 2,6 × 2,4 | x −2,8..−0,2; z 144,9..147,3 |
| Pat Patamar da manhã | `OpenBookPlatform` | (0; 8,7; 150,7) | 6 × 4,4 | z 148,5..152,9 |
| Trilho da manhã | `BookPlatform` | (0; 8,7; 159,9) | 2,8 × 14 | z 152,9..166,9 |
| Fim Livro do despertar | `OpenBookPlatform` | (0; 8,7; 171,9) | 9 × 8 | z 167,9..175,9 |

Spawn em (0, 0, −1) com yaw 0 (pufe em z −2,4). Porta em (0; 8,7; 174,2) com yaw 180. Zona de queda em y −8. O percurso tem cerca de 179 m em Z e sobe 8,7 m. As mesas das ilhas X1 e X3 ficam giradas (0,8 m em X) na borda oposta à rota.

**Paredes** (linha central; a face interna fica a 0,3 m dela):

| Trecho | Estantes | Face interna | Altura |
| --- | --- | --- | --- |
| Ato I | x = ±4,3 (z −3..18); divisórias em z 7 (abertura x 1,6..3,2) e z 12 (abertura x −3,2..−1,6) | ±4,0 | 4 m |
| Plataforma da pressa | x = ±4,3 | ±4,0 | 3 m |
| Corredor dos feixes | x = ±1,65 | ±1,35 (2,7 m = feixe) | 4 m |
| Oficina | x = ±3,3 | ±3,0 | 4 m |
| Galeria do relógio | x = ±3,3 | ±3,0 | 5 m |
| Arquivo | x = ±1,1 | ±0,8 (1,6 m = prensa) | 4,5 m |
| Trilho da manhã | x = ±1,1 | ±0,8 (1,6 m) | 5 m |

## Números do motor

Os mesmos de `docs/Fase3.md` e `docs/Fase16.md`.

- **Alcance nominal.** Sem poder: plano 3,32 m; +0,5 m 2,88 m; +0,8 m 2,52 m. Velocidade: plano 4,97 m. Pluma: plano 6,63 m; +0,5 m 6,23 m. Pulo duplo: +1,7 m 4,90 m.
- **Limite teórico** (*coyote* + 0,28 m de borda de cada lado). Sem poder: plano 4,16 m; +0,5 m 3,55 m. Velocidade: plano 5,95 m; +0,5 m 5,05 m. Velocidade + Pluma: plano 11,04 m; +0,5 m 10,39 m. Pluma + pulo duplo: plano 14,19 m. Os três juntos: plano 21,0 m.
- **Altura.** O apogeu é 1,1 m sem poder e 2,2 m com Pluma ou pulo duplo. A borda alcançável (`stepOffset` de 0,3 m) é 1,4 m, 2,5 m e 4,7 m (Pluma e pulo duplo juntos).

## Cálculo de cada pulo

| Salto | Vão | Desnível | Alcance nominal | Margem | Sem o poder |
| --- | --- | --- | --- | --- | --- |
| Sala → mesa | — | +0,8 | apogeu 1,1 | 0,3 m de altura (0,6 com o `stepOffset`) | — |
| Mesa → L | 1,0 m | +0,8 | 2,52 m | 1,52 m | do chão, +1,6 m é mais que a borda alcançável (1,4 m): as mesas são obrigatórias |
| Fosso → Cela 2 | — | +0,8 | apogeu 1,1 | 0,3 m de altura | — |
| L → C1 → C2 → C3 → Lp | 1,8 m | 0 | 3,32 m | 1,52 m | — |
| **Lp → V (Velocidade)** | 4,4 m | 0 | 4,97 m | **0,57 m** | limite 4,16 m: impossível |
| CF → X1, X1 → X2, X2 → X3 | 2,3 m | +0,5 | 2,88 m | 0,58 m | — |
| X3 → Cais | 2,3 m | 0 | 3,32 m | 1,02 m | — |
| **Cais → Y1 (Pluma)** | 5,2 m | +0,5 | 6,23 m | 1,03 m | limite 3,55 m; só com a Velocidade, 5,05 m: impossível |
| Y1 → Y2, Y2 → Base (Pluma) | 5,2 m | 0 | 6,63 m | 1,43 m | limite 4,16 m: impossível |
| Base → S1, S2 → Pat (pulo duplo) | 1,2 m | +1,7 | 4,90 m | 3,7 m; 0,5 m de altura | +1,7 > 1,4 m: impossível |
| S1 → S2 (pulo duplo) | 1,26 m | +1,7 | 4,90 m | 3,6 m | impossível |
| Trilho → Fim | 1,0 m | 0 | 3,32 m | 2,32 m | — |

O vão mais apertado da fase é o da Velocidade (margem de 0,57 m), seguido das ilhas de leitura (0,58 m, sem poder, subindo 0,5 m). Nenhum salto tem margem abaixo de 0,5 m.

## Atalhos

Um script testou **todos os pares de plataformas** que não são vizinhas na rota, com o limite teórico. Em cada plataforma, supôs ativos todos os poderes já coletados até ali, mesmo os que na prática já teriam acabado. Resultado:

- **Ato I:** o único par alcançável (entrada → cela 2, 4 m) está separado pela divisória de 4 m. As estantes têm 4 m e a borda alcançável de cima de uma mesa é 0,8 + 1,4 = 2,2 m.
- **Carteiras (Velocidade):** pular uma carteira exige 1,8 + 2,5 + 1,8 = 6,1 m, contra o limite de 5,95 m com a Velocidade.
- **Atos III e IV (Velocidade e Escudo talvez ativos):** são corredores de piso contínuo entre estantes de 4 a 5 m, sem piso por fora, e **não há espinhos depois da Velocidade**. Correndo a 7,5 m/s, o pé fica acima de 0,65 m por 3,18 m, mais que os 2,36 m do campo de espinhos. Por isso os espinhos ficaram só no ato I. A Velocidade não muda a altura: o feixe (0,825 m) e o peso do pêndulo (até 1,77 m) não são saltados, e a prensa erguida ocupa de 1,95 a 2,85 m. O Escudo pode absorver um toque em qualquer armadilha até acabar ou ser usado: é a lembrança da Fase 5 e não pula trecho nenhum.
- **Ilhas e mar de páginas (Velocidade e Pluma):** nenhum par alcançável. Pular uma ilha do mar exige 13,0 m (Cais → Y2, Y1 → Base), contra o limite de 10,39 a 11,04 m com Velocidade e Pluma. Só com a Velocidade, nem o primeiro vão é possível (5,05 m < 5,2 m). A mesa da ilha X3 (topo 3,9) até Y1 dá 12 m.
- **Escadaria (Pluma e pulo duplo):** com a Pluma ainda ativa, a escada se sobe sem o pulo duplo. Com os dois, dá para pular um degrau. Isso só encurta a subida.
- **Trilho da manhã:** o corredor tem 1,6 m e a serra 1,56 m. O centro do jogador fica no máximo a 0,52 m do eixo, contra 1,06 m de distância mínima para não tocar na serra. Não há lado livre. Do patamar ao livro final são 15 m: com Pluma e pulo duplo (limite 14,19 m), não dá para voar por cima do trilho todo. Só os três poderes juntos (21 m) passariam. Isso pede a Velocidade coletada menos de 30 s antes, a ~130 m de rota, com o laser de outro ritmo, os pêndulos e as prensas no meio. Mesmo assim, voar sobre o trilho é só outra forma de saltar a serra.

## Ritmo das armadilhas

Os ciclos usam `Time.timeSinceLevelLoad` e recomeçam iguais a cada tentativa. `Onda(d) = (−d / 5) mod período`: quem corre a 5 m/s encontra as armadilhas em onda na mesma fase.

**Espinhos fixos (cela 1).** Os campos ficam em x −2 e 0 (gatilho de x −2,9 a 0,9). A passagem livre fica à direita (x 0,9..4,0), de frente para a abertura da divisória. Sem poder, não dá para saltar por cima (2,12 m acima de 0,65 m, contra 2,36 m necessários).

**Piso escuro (cela 2).** Cede 0,8 s depois de pisado e cai num fosso de 0,8 m. As lajes vizinhas (0,5 m de espessura) deixam só 0,3 m por baixo, então o fosso é fechado e se sai dele com um pulo. O chão iluminado contorna o fosso por faixas de 1,2 m livres.

**Carteiras.** A 5 m/s, cada carteira é cruzada em 0,5 s, menos que o atraso de 0,8 s. Andando (2,2 m/s), o pulo alcança 1,46 m e não vence o vão de 1,8 m: é preciso correr, como na Fase 4.

**Corredor dos feixes.** Os feixes 1 e 2 ficam 1,2 s ligados e 2,0 s apagados (3,2 s), em onda. O feixe 3 fica 1,8 s ligado e 1,2 s apagado (3 s), com outro ritmo, e é o único em que vale parar e observar. Atravessar um feixe leva 0,81 m (0,16 s). Os refúgios entre os feixes têm 3,25 m e luz quente. O Escudo deixa atravessar um feixe ligado, com 1 s de graça.

**Serra da oficina.** Ela vai de x −2 a 2 e volta em 3 s, na sala de 6 m. Com a serra num extremo, o lado oposto tem mais de 1,7 m livres. A faixa da serra tem 2,12 m em Z (0,42 s correndo).

**Pêndulos sincronizados.** Ângulo de 55°, período de 2,4 s e o mesmo *offset* nos três: passam pelo meio juntos, a cada 1,2 s. O tempo de contato por ciclo é 0,56 s no centro, 1,02 s com x = 1,5 e 0,51 s colado na estante (x = 2,72). Não há lado seguro. Entre os pêndulos há refúgios de 1,8 m (z 73,4..75,2 e 77,0..78,8). A cada passagem pelo meio, o jogador avança um refúgio: a faixa de um peso (1,8 m) leva 0,36 s, e a janela entre passagens é de ~0,9 s.

**Prensas do arquivo.** Ciclo de 3,2 s (1,4 s erguida, 0,2 s descendo, 0,8 s embaixo, 0,8 s subindo), com janela segura de 2,2 s. A segunda prensa está em onda (3,3 m, 0,66 s depois). Logo depois vem a página que cede (3 m, 0,6 s correndo).

**Última serra.** Vai de z 154,9 a 164,9 e volta em 4,5 s. Para a 1,22 m do patamar e a 1,22 m do fim do trilho. Com o segundo pulo no apogeu, o pé fica acima de 1,63 m por 0,48 s (2,4 m a 5 m/s), contra a faixa de contato de 2,12 m. É preciso saltar quando a serra vem na direção do jogador. O pulo simples nunca passa.

## Estética

- **Luz da manhã:** paleta quente e clara, sol baixo à frente, *bloom* 1,1, vinheta 0,15 e poeira dourada (`DreamMotes`) a partir de 14 m de altura, acima da cabeça em toda a fase (máximo 12,7 m).
- **Biblioteca:** estantes como paredes em toda a parte coberta, `ShelfArch` na entrada e no laboratório, mesas com abajur (degraus no ato I, ilhas no ato V e o canto de estudo no fim), pilhas e livros abertos como plataformas, e `LibraryBackdrop` de 20 m (base −8) a 9 m de margem.
- **Cenário fora do caminho:** relógios nas laterais (x ±10), livros e páginas a 8 m ou mais do eixo, páginas do mar 3 m abaixo das ilhas e escada encostada fora da escadaria (x 5,6). As placas laterais ficam a 1,5 m ou mais da borda dos pisos.

## Riscos e pendências

- Fase escrita sem abrir a Unity: precisa de *playtest*. Pontos a observar: o vão da Velocidade (margem de 0,57 m), as ilhas de leitura (0,58 m), a legibilidade das placas sob o sol forte, o número de luzes pontuais (cerca de 45, entre luminárias, abajures e velas) e a orientação do disco da serra no trilho (yaw 90).
- Com 14 lembranças, a fase é longa (~179 m). Se o *playtest* mostrar cansaço, o primeiro corte sugerido é a oficina (F6), que repete a ideia da última serra.
