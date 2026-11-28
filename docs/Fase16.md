# Fase 16: Observatório dos Sonhos

Cena: `Assets/Scenes/Fase16_ObservatorioDosSonhos.unity`, gerada por `Assets/Scripts/Editor/Phases/Fase16Builder.cs` (menu **Dreamscape > Fases > Gerar todas as fases novas**). O tema é próprio da classe (`Fase16Builder.Tema`): noite estrelada índigo com destaque **azul-estrela** (0,62; 0,70; 1). É mais clara e colorida que a Fase 15 (saturação 25, exposição 0,2, neblina 0,015, *bloom* 1,0).

## Objetivo

O topo da biblioteca do sonho se abre para o céu. O estudante sobe de y 0 a y 9,6 por plataformas altas sobre o vazio até a cúpula, onde fica a porta, sob uma lua que é um relógio gigante. Cair ou tocar uma armadilha reinicia a cena. Os ciclos usam `Time.timeSinceLevelLoad`, então cada tentativa começa no mesmo ponto de cada ciclo.

A regra da fase é **as estrelas marcam o chão**: todo piso firme tem uma "estrela" (luminária azul) pendurada acima, e os pisos que cedem (as estrelas cadentes) não têm. Luz quente marca refúgio; lilás, o trecho da Pluma; azul-claro, o do pulo duplo.

Junto com a Fase 17, é uma das duas fases mais difíceis do jogo. Os trechos de *timing* são separados por refúgios, nenhum vão exige precisão de pixel (margem nominal mínima de 0,78 m) e cada poder é coletado logo antes do trecho que o exige.

## Ritmo e espaço

| Ato | Trecho | Papel | Espaço |
| --- | --- | --- | --- |
| 1 | E0 mirante, S1 escada da constelação | Apresenta a regra: pilhas firmes com estrela e estrelas cadentes sem estrela. | Mirante pequeno e quente; depois, o vazio com o céu inteiro à vista. |
| 2 | S2 ponte dos cometas | *Timing*: dois pêndulos cruzam a ponte, com um refúgio no meio. | Ponte de 2 m sobre o vazio, vigas e colunas. |
| 3 | S3 mapa celeste, S4 ilhas | Poder: Pluma para quatro vãos de 5 m. | Livro aberto e ilhas soltas, páginas brilhando embaixo. |
| 4 | S5 galeria das lunetas | Teste de ritmo: onda de laser, espinhos e laser sob teto baixo; refúgio; duas prensas. | O trecho mais fechado da fase. |
| 5 | S6 escadaria, S7 trilho do cometa, cúpula | Pulo duplo: quatro degraus de +1,7 m e a serra que só se vence saltando. | Volta ao céu aberto, no ponto mais alto. |

**Marcos e linha de visão.** A lua (`DreamClock`, raio 5) flutua em (0, 21, 162), atrás da porta, com o mostrador voltado para o spawn. Ela aparece pelo portal do mirante de entrada e, depois, sobre a galeria e no topo da escadaria. O percurso é quase reto em +Z, e o desvio lateral máximo é de 1,5 m, então a direção nunca se perde.

## Mecânicas e poderes

| Mecânica | Onde | Observação |
| --- | --- | --- |
| Pisos que cedem (0,8 s) | P2, P4, I2, C1, C2 | São as "estrelas cadentes": sem luminária em cima. Atravessá-los correndo leva de 0,4 a 0,5 s. |
| Pêndulos (65°, 2,6 s) | Ponte dos cometas, z 29,5 e 34,5 | Meio período de defasagem (1,3 s). |
| **Pluma** (`Gravity`) | Mapa celeste, (1,6; 1,8; 42,6) | Exigida nas ilhas (vãos de 5,0 a 5,1 m). A primeira ilha está a 6,7 m do coletável. |
| Laser e espinhos em ciclo, em onda | Galeria baixa (G1) | Teto a 2,3 m: nem a Pluma salta por cima. |
| Prensas em onda | Corredor de 1,6 m (G3) | |
| **Pulo duplo** (`DoubleJump`) | Pé da luneta, (0; 2,8; 105,6) | Exigido nos quatro degraus de +1,7 m e na serra. O primeiro degrau está a 2,7 m do coletável. |
| Serra (período 4,5 s) | Trilho do cometa | Corre **ao longo** de um corredor de 1,6 m: não há lado livre e é preciso saltá-la. |

Cada tipo de poder aparece **uma vez só**. Quando o efeito de um coletável acaba, `ActivePowerUps` chama `Remove`, que desfaz o efeito do tipo inteiro. Um segundo coletável igual pego antes disso perderia o efeito no meio do trecho.

Placas (sem acentos): `FASE 16` / `OBSERVATORIO DOS SONHOS` / `AS ESTRELAS MARCAM O CHAO` no portal, `ESTRELA CADENTE NAO ESPERA`, `PASSE LOGO DEPOIS DO COMETA`, `A PLUMA LEVA AS ILHAS` / `NAO PARE: SAO 30 S`, `GALERIA DAS LUNETAS` / `TETO BAIXO: SIGA A ONDA`, `PULO DUPLO: SUBA A LUNETA` e `SALTE A SERRA` / `COM O PULO DUPLO`.

## Estética

- **Céu:** duas camadas de `DreamMotes` grandes e altas fazem as estrelas: o "céu" (36 × 6 × 175 m, centro a y 23, 900 partículas) e a "via láctea" (y 17, sobre a metade final). A camada mais baixa começa a y 15,5, e o ponto mais alto que a cabeça alcança na fase é 9,6 + 2,2 + 1,76 = 13,6. Dezesseis grupos de `FloatingPages` ficam em x ±11,5, entre 11 e 18 m de altura, como estrelas maiores e distantes.
- **Biblioteca:** pilhas de livros nas escadas e ilhas (`BookPlatform`), livros abertos nos patamares (`OpenBookPlatform`: mapa celeste, mirante, cúpula), estantes sólidas como paredes da galeria e do trilho, `ShelfArch` no portal e na boca da galeria, mesas com abajur no mirante de entrada, no mapa celeste e na cúpula, e `LibraryBackdrop` com estantes de 26 m (base em y −12) a 9 m de margem.
- **Cenário fora do caminho:** relógios parados nas laterais (x ±11), livros voando a 7 m ou mais do eixo e páginas sob as ilhas (y −2,5). Os abajures ficam sobre as estantes e as estrelas ficam a 4,6 m ou mais acima do piso (6 m nas ilhas da Pluma). As placas laterais ficam a 1,5 m ou mais da borda.
- O pátio da galeria **não tem mesa**, de propósito. Dela (0,8 m) daria para subir no teto da galeria com a Pluma.

## Números do motor usados

Mesmos números de `docs/Fase3.md`: gravidade −20 m/s², pulo 1,1 m (v0 = 6,633 m/s), corrida 5 m/s (7,5 com Velocidade), raio 0,28 m, *coyote time* 0,12 s, `stepOffset` 0,3 m.

- **Alcance nominal** (sair da borda correndo e pousar na borda). Sem poder: plano 3,32 m; +0,6 m 2,78 m. Pluma: plano 6,63 m; +0,5 m 6,23 m. Pulo duplo: +1,7 m 4,90 m, com o segundo pulo no meio da subida.
- **Limite teórico**, usado para provar que um atalho é impossível: soma o pulo depois de 0,12 s de *coyote* (sai 0,38 m mais baixo, ou 0,31 m com a Pluma) e 0,28 m de borda do controlador de cada lado. Sem poder: plano 4,16 m; +0,5 m 3,55 m. Pluma: plano 7,55 m. Pluma + pulo duplo: plano 14,19 m; +1,7 m 12,68 m.
- **Altura:** o apogeu é 1,1 m sem poder e 2,2 m com Pluma ou pulo duplo. A borda alcançável, somando o `stepOffset`, é 1,4 m sem poder, 2,5 m com um dos dois e 4,7 m com os dois juntos.

As contas foram feitas por um script que integra o salto (com o segundo pulo no melhor instante) e testa **todos os pares de plataformas**, supondo ativos todos os poderes já coletados até ali. O resultado está em "Atalhos".

## Plataformas (topo em Y, medidas X × Z)

| Peça | Tipo | Centro do topo | Tamanho | Extensão |
| --- | --- | --- | --- | --- |
| E0 Mirante de entrada | `Platform` | (0, 0, 0) | 8 × 6 | z −3..3 |
| P1 Estrela | `BookPlatform` | (−0,8; 0,6; 6,2) | 2,4 × 2,4 | z 5,0..7,4 |
| P2 Estrela cadente | `CrumblingPlatform` | (0,8; 0,6; 10,6) | 2,4 × 2,4 | z 9,4..11,8 |
| P3 Estrela | `BookPlatform` | (−0,8; 1,2; 15,0) | 2,4 × 2,4 | z 13,8..16,2 |
| P4 Estrela cadente | `CrumblingPlatform` | (0,8; 1,2; 19,4) | 2,4 × 2,4 | z 18,2..20,6 |
| P5 Patamar da ponte | `BookPlatform` | (0; 1,8; 24,2) | 4 × 3,2 | z 22,6..25,8 |
| B Ponte dos cometas | `BookPlatform` | (0; 1,8; 32,0) | 2 × 12,4 | z 25,8..38,2 |
| M Mapa celeste | `OpenBookPlatform` | (0; 1,8; 41,2) | 6 × 6 | z 38,2..44,2 |
| I1 Ilha | `BookPlatform` | (0; 2,3; 50,6) | 2,6 × 2,6 | z 49,3..51,9 |
| I2 Ilha cadente | `CrumblingPlatform` | (−0,8; 2,3; 58,3) | 2,6 × 2,6 | z 57,0..59,6 |
| I3 Ilha | `BookPlatform` | (0,8; 2,8; 66,0) | 2,6 × 2,6 | z 64,7..67,3 |
| Pat Pátio das lunetas | `Platform` | (0; 2,8; 75,3) | 8 × 6 | z 72,3..78,3 |
| G1 Galeria baixa | `Platform` + teto `BookPlatform` (topo 5,6) | (0; 2,8; 84,3) | 3,9 × 12 | z 78,3..90,3 |
| G2 Refúgio | `Platform` | (0; 2,8; 91,8) | 4 × 3 | z 90,3..93,3 |
| G3 Corredor das prensas | `Platform` | (0; 2,8; 98,8) | 3,4 × 11 | z 93,3..104,3 |
| Pé da luneta | `Platform` | (0; 2,8; 106,3) | 4 × 4 | z 104,3..108,3 |
| T1 Degrau | `BookPlatform` | (1,5; 4,5; 110,7) | 2,6 × 2,4 | x 0,2..2,8; z 109,5..111,9 |
| T2 Degrau | `BookPlatform` | (−1,5; 6,2; 114,3) | 2,6 × 2,4 | x −2,8..−0,2; z 113,1..115,5 |
| T3 Degrau | `BookPlatform` | (1,5; 7,9; 117,9) | 2,6 × 2,4 | z 116,7..119,1 |
| Mir Mirante da cúpula | `OpenBookPlatform` | (0; 9,6; 122,5) | 6 × 4,4 | z 120,3..124,7 |
| Trilho do cometa | `BookPlatform` | (0; 9,6; 131,7) | 2,8 × 14 | z 124,7..138,7 |
| C1, C2 Estrelas cadentes | `CrumblingPlatform` | (0; 9,6; 140,7) e (0; 9,6; 143,7) | 2 × 2 | z 139,7..141,7 e 142,7..144,7 |
| Fim Cúpula das estrelas | `OpenBookPlatform` | (0; 9,6; 150,2) | 8 × 8 | z 146,2..154,2 |

Spawn em (0, 0, −1) com yaw 0 (pufe em z −2,4). Porta em (0; 9,6; 152,5) com yaw 180. Zona de queda em y −10. O percurso tem cerca de 160 m em Z e sobe 9,6 m.

**Paredes que limitam o caminho** (linha central; a face interna fica a 0,3 m dela):

| Trecho | Estantes | Face interna | Altura |
| --- | --- | --- | --- |
| G1 | x = ±1,65 | ±1,35 (2,7 m, a largura do feixe) | 2,3 m, sob o teto |
| G3 | x = ±1,1 | ±0,8 (1,6 m, a largura da prensa) | 4,5 m |
| Trilho do cometa | x = ±1,1 | ±0,8 (1,6 m) | 5 m |
| Pátio | x = ±4,3; frente z 78,6 fora da boca | ±4,0 | 3,5 m |

## Cálculo de cada pulo

| Salto | Vão | Desnível | Alcance nominal | Margem | Sem o poder |
| --- | --- | --- | --- | --- | --- |
| E0 → P1, P2 → P3, P4 → P5 | 2,0 m | +0,6 | 2,78 m | 0,78 m | — |
| P1 → P2, P3 → P4 | 2,0 m | 0 | 3,32 m | 1,32 m | — |
| M → I1 (Pluma) | 5,1 m | +0,5 | 6,23 m | 1,13 m | limite 3,55 m: impossível |
| I1 → I2 (Pluma) | 5,1 m | 0 | 6,63 m | 1,53 m | limite 4,16 m: impossível |
| I2 → I3 (Pluma) | 5,1 m | +0,5 | 6,23 m | 1,13 m | impossível |
| I3 → Pátio (Pluma) | 5,0 m | 0 | 6,63 m | 1,63 m | impossível |
| Pé → T1 (pulo duplo) | 1,2 m | +1,7 | 4,90 m | 3,7 m; altura 0,5 m (0,8 com o `stepOffset`) | borda alcançável 1,4 m < 1,7: impossível |
| T1 → T2, T2 → T3 (pulo duplo) | 1,26 m (diagonal 1,2 × 0,4) | +1,7 | 4,90 m | 3,6 m; altura 0,5 m | impossível |
| T3 → Mirante (pulo duplo) | 1,2 m | +1,7 | 4,90 m | 3,7 m; altura 0,5 m | impossível |
| Trilho → C1, C1 → C2 | 1,0 m | 0 | 3,32 m | 2,32 m | — |
| C2 → Cúpula | 1,5 m | 0 | 3,32 m | 1,82 m | — |

**Tempo da Pluma.** Do coletável à borda do pátio são cerca de 30 m: quatro saltos de ~1,25 s e 7 m de corrida, ou 6,5 a 7 s. Sobram mais de 20 s. Se o efeito acabar com o jogador parado numa ilha, ele não tem como seguir e cai. A placa `NAO PARE: SAO 30 S` avisa disso, e o coletável reaparece no mapa celeste.

**Tempo do pulo duplo.** Do coletável até o mirante são quatro saltos (~1 s cada). Depois vem no máximo um ciclo da serra (4,5 s) de espera. Sobram mais de 20 s. Se o efeito acabar antes da serra, dá para descer os degraus (−1,7 m cada) até o pé da luneta, onde o coletável já reapareceu.

## Atalhos

O script testou todos os pares de plataformas que não são vizinhas na rota, com o limite teórico e supondo ativos todos os poderes coletados até ali (Pluma a partir do mapa celeste; Pluma e pulo duplo a partir do pé da luneta).

- **Até o mapa celeste (sem poder):** nenhum par alcançável. Pular uma pilha da escada exige 6,4 m (2,0 + 2,4 + 2,0), contra o limite de 4,16 m. A ponte é o único piso entre P5 e o mapa, e o pêndulo cruza toda a largura dela (veja abaixo).
- **Ilhas (Pluma):** pular uma ilha exige 12,8 m (mapa → I2, I1 → I3) ou 12,7 m (I2 → pátio), contra o limite de 7,55 m. O pulo duplo ainda não foi coletado. A ilha cadente I2 não pode ser evitada.
- **Galeria (Pluma ainda ativa):** o teto de G1 fica 2,8 m acima do pátio, e a borda alcançável com a Pluma é 2,5 m. Não há mesa no pátio, e as estantes do pátio têm 3,5 m. Sob o teto, o pé sobe no máximo 2,3 − 1,76 = 0,54 m, menos que os 0,65 m do topo do gatilho dos espinhos e os 0,825 m da base do feixe. Por fora da galeria só há vazio: do pátio a G2 são 12 m. Em G3, a prensa erguida ocupa de 1,95 a 2,85 m de altura e as estantes têm 4,5 m: não se passa por cima.
- **Escadaria (Pluma talvez ainda ativa):** a Pluma sozinha também sobe +1,7 m (apogeu 2,2 m), então quem chega rápido nem precisa do pulo duplo. Com os dois, dá para pular um degrau (+3,4 m). Isso só encurta a subida, sem pular nenhuma armadilha. G3 → T1 com a Pluma (5,2 m, +1,7, limite 5,46 m) também é possível e só evita o pé da luneta.
- **Trilho do cometa:** o corredor tem 1,6 m e a serra tem 1,56 m de diâmetro. O centro do jogador fica no máximo a 0,52 m do eixo, e a distância mínima ao centro da serra para não tocá-la é 0,78 + 0,28 = 1,06 m. Não há como passar ao lado (a lição da borda de 0,28 m vale aqui). Do mirante a C1 são 15 m, contra o limite de 14,19 m com Pluma e pulo duplo: não dá para voar por cima do trilho todo. A partir do trilho, o script aponta alcances até C2 ou até a cúpula com Pluma e pulo duplo. Eles só valem depois de passar pela serra e pulam apenas os pisos que cedem.
- **Estantes:** todas têm 2,3 m (sob o teto), 3 m ou mais, e a borda alcançável em cada trecho é menor que elas. A única exceção são as estantes de G3 (4,5 m), alcançáveis do pé da luneta com Pluma e pulo duplo juntos (4,7 m). Elas levam de volta à galeria e não adiantam nada.

## Ritmo das armadilhas

**Pêndulos (ponte dos cometas).** Ângulo de 65°, período de 2,6 s, pivô a 3,8 m. O peso (raio 0,62 m) fica a 1,15 m do piso no ponto mais baixo. Acima de 57,6° ele passa por cima da cabeça (base > 1,76 m), e no extremo chega a x ±2,40 m. Calculado ao longo do ciclo, o tempo em que o peso toca o jogador é de **0,51 s por ciclo** no meio da ponte, 0,56 s com x = 0,72 e 0,74 s na borda (x = 1,28, centro 0,28 m além do piso). Não há lado seguro. Cada perigo vem em duas passagens curtas por ciclo, com cerca de 1 s livre entre elas. Atravessar a faixa do peso (1,8 m) leva 0,36 s. O segundo pêndulo tem 1,3 s de defasagem, e entre os dois há 3,2 m de refúgio sob a luz quente.

**Galeria baixa (G1).** Período comum de 3 s. Os lasers ficam 1,4 s ligados e 1,6 s apagados, piscando 0,4 s antes de ligar. Os espinhos ficam 1,2 s fora e 1,8 s dentro. Os *offsets* seguem `Onda(d) = (−d / 5) mod 3`, então quem corre a 5 m/s encontra todos na mesma fase. Atravessar um laser leva 0,81 m (0,16 s); um campo de espinhos, 2,36 m (0,47 s). Correndo sem parar, a janela comum é de cerca de 1,1 s. Quem perdeu o ritmo para entre as armadilhas: os refúgios têm 2,25 m (z 81,25..83,5 e 85,5..87,75) e cada um tem uma luz quente. O corredor tem 2,7 m livres e o campo de espinhos tem 1,8 m. De lado sobram 0,45 m, menos que os 0,56 m do corpo.

**Prensas (G3).** Ciclo de 3,2 s: 1,4 s erguida, 0,2 s descendo, 0,8 s embaixo e 0,8 s subindo. Elas só ferem descendo ou embaixo, então a janela segura é de 2,2 s seguidos. Passar por baixo leva 2,16 m (0,43 s). A segunda prensa está em onda com a primeira (3,6 m, 0,72 s depois).

**Serra (trilho do cometa).** Ela vai de z 126,7 a 136,7 e volta em 4,5 s (`SmoothStep`: para nas pontas e chega a 6,7 m/s no meio). No mirante não há perigo: a serra para a 1,22 m da borda. Para saltá-la, o pé precisa passar de 0,85 + 0,78 = 1,63 m. Com o segundo pulo no apogeu do primeiro, o pé fica acima de 1,63 m por **0,48 s** (2,4 m de avanço a 5 m/s), contra os 2,12 m da faixa de contato. Saltando quando a serra **vem na direção do jogador**, a velocidade relativa sobe e a margem aumenta. O pulo simples (apogeu 1,1 m) nunca passa. Com a Pluma ainda ativa, o pé fica 0,68 s acima de 1,63 m: fica mais fácil.

## Riscos e pendências

- Fase escrita sem abrir a Unity: precisa de *playtest*. Pontos a observar: a leitura do teto baixo da galeria em primeira pessoa, a orientação do disco da serra (yaw 90 para correr em Z), a quantidade de luzes pontuais (cerca de 30) e a legibilidade das placas laterais.
- A Pluma ainda ativa facilita a escadaria e a serra. Isso é intencional (premia quem foi rápido) e não pula nenhum trecho.
