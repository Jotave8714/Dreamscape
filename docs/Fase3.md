# Fase 3: Corredor das Luzes

Cena: `Assets/Scenes/Fase3_CorredorDasLuzes.unity`, gerada por `Assets/Scripts/Editor/Phases/Fase3Builder.cs` (menu **Dreamscape > Fases > Gerar todas as fases novas**). O tema é `PhaseTheme.Fase3`, ciano e verde-água. Ainda é uma fase escura, mas tem mais cor que as Fases 1 e 2 (saturação -6, exposição 0), seguindo a paleta progressiva do GDD.

## Objetivo

O estudante cochilou na biblioteca e o sonho o leva por um corredor de estantes iluminado por luminárias frias. Ele precisa chegar à porta de conclusão, sobre um livro aberto gigante no fim do corredor (N13). Cair ou tocar uma armadilha reinicia a cena. Nada é sorteado: os ciclos são fixos e cada tentativa tem a mesma geometria.

A regra da fase, repetida em todo trecho, é **onde há luminária no teto, há chão firme**.

## Ritmo e espaço

| Ato | Trecho | Papel | Espaço |
| --- | --- | --- | --- |
| 1 | N0 Sala de leitura | Apresenta a regra em segurança: um piso escuro ao lado do caminho cede e o jogador cai num fosso raso, de onde sobe sozinho. | Sala pequena e quente (mesa, tapete, abajures). |
| 2 | N1 Espinhos fixos, N2–N3 corredor do ritmo | Testa: desviar e depois esperar o ritmo. | Sala média, depois o trecho mais apertado da fase (1,7 m entre estantes de 4,5 m). |
| 3 | N4 Salão da escolha, N5 Pluma, N6–N7 galeria | Escolha: rota curta e arriscada (vão de 5,5 m com a Pluma) ou rota longa e segura (galeria lateral de livros gigantes). | O maior salão, com estantes de 6 m e livros voando no alto. |
| 4 | N8 Salão de chegada, T0 corredor escuro | Teste da regra: o caminho reto até a porta é escuro e cede. | Salão médio, com o corredor escuro e estreito à frente. |
| 5 | N9–N12 laser, degrau e porta | Desafio final: laser, dois degraus e a porta no alto (y = 1,5). | Corredor de estantes que sobe até o livro aberto. |

**Marcos e linha de visão.** O relógio do sonho (`DreamClock`, raio 3 m) flutua a 12 m de altura atrás da porta, em (0, 12, 66), com o mostrador voltado para o spawn. Do spawn ele aparece pelo arco de estantes da sala de leitura, acima do corredor (≈ 9° acima do olho, enquanto as estantes de 3,5 m da sala N1 cobrem só até ≈ 6°). Do salão N8, a porta aparece em linha reta ao fundo do corredor escuro: é o marco que o truque usa.

**Leitura da rota.** A luz marca o chão (luminárias de teto sobre todo piso firme, nenhuma sobre os pisos falsos). A cor de destaque aparece nos frisos dos arcos de estante (`ShelfArch`) e dos tapetes: o tapete do refúgio N3 marca onde parar entre os pares de espinhos, e o tapete do salão N8 aponta para o oeste (corredor do laser), não para o corredor escuro.

## Mecânicas

| Mecânica | Onde | O que ensina |
| --- | --- | --- |
| Piso escuro que cede (apresentação) | N0, ao lado do spawn | Sem risco: o piso escuro (sem luminária, material de piso falso) cede e o jogador cai 0,8 m num fosso fechado, de onde sobe com um pulo. A placa `ONDE HÁ LUZ, HÁ CHÃO` fica na estante ao lado. |
| Espinhos fixos | N1, duas fileiras desencontradas | Desviar. A passagem livre fica à direita na primeira fileira e à esquerda na segunda. Não dá para saltar por cima (ver cálculo). |
| Espinhos em ciclo | Corredor N2–N3, pares A e B | Observar antes de passar. Os pares se alternam: quando A recolhe, B sobe. Atravessa-se A, espera-se no refúgio N3 (tapete) e atravessa-se B. |
| Pluma (`PickupKind.Gravity`) | Pedestal de livros N5 | Opcional. Com metade da gravidade, o mesmo impulso rende um salto mais alto (2,2 m) e mais longo (6,6 m), o que vence o vão de 5,5 m entre N4 e N8. |
| Laser | Corredor N9–N10 | Uma única vez, perto do fim. O feixe pisca antes de ligar, e o corredor tem exatamente a largura do feixe. |
| Truque de cenário | Corredor escuro T0 | Mostra o caminho direto para a porta, mas é piso falso e sem luz. Cobra a regra apresentada em N0. |

Dicas no cenário (`kit.Sign`): `FASE 3` / `CORREDOR DAS LUZES` no arco de saída da sala de leitura, `ONDE HÁ LUZ, HÁ CHÃO` ao lado do fosso, `OBSERVE O RITMO` na boca do corredor dos espinhos em ciclo e `SIGA AS LUZES` no salão N8, ao lado do corredor escuro.

## Estética (biblioteca do sonho)

- **Paredes:** todas as paredes viraram estantes sólidas (`Bookshelf`, 0,6 m de espessura). Cada estante fica **por fora** do piso, com a face interna na borda, então as larguras jogáveis são as mesmas da versão com `Wall`. Alturas: 3 m nas salas comuns, 3,5 m em N1/N8/laser, 4,5 m no corredor do ritmo (sensação de aperto) e 6 m no salão da escolha (sensação de amplitude).
- **Arco de estantes** (`ShelfArch`) na saída da sala de leitura, emoldurando o corredor e o relógio.
- **Plataformas:** o chão das salas continua `Platform` (o piso "da escola"); o pedestal da Pluma (N5), a galeria lateral (N6), o degrau lateral (N7) e o degrau final (N11) são pilhas de livros gigantes (`BookPlatform`); o patamar da porta (N12) é um livro aberto gigante (`OpenBookPlatform`).
- **Mobília:** mesas de leitura com vela (`ReadingDesk`) em N0, N4 e N8, sempre longe das linhas de pulo; tapetes em N0, no refúgio N3 e em N8; abajures de leitura (`ReadingLamp`) só **em cima das estantes**, fora do volume jogável.
- **Cenário flutuante:** livros voando a 7,5 m no salão da escolha e acima da porta; um rastro de páginas a 6,2 m sobre o vão da Pluma (sugere o voo, bem acima da cabeça de quem pula com a Pluma, ≈ 4 m); livros do lado de fora da galeria lateral. Tudo fica a mais de 2,5 m acima do caminho ou fora das estantes.
- **Moldura:** `LibraryBackdrop` cobre x −9,5..10,5 e z −4..61, com 6 m de margem, base em y = −5 e 14 m de altura.

## Números do motor usados

Valores de `CharacterMotor` e do prefab `FirstPersonPlayer`: gravidade -20 m/s², altura de pulo 1,1 m, corrida 5 m/s, raio do controlador 0,28 m, *coyote time* 0,12 s.

- Velocidade inicial do pulo: v0 = √(2 · 1,1 · 20) = **6,633 m/s**. A Pluma não muda esse impulso, só a gravidade da queda (`SetGravityScale(0,5)`), então o apogeu passa a 6,633² / (2 · 10) = **2,2 m**.
- Tempo no ar até pousar `dh` metros acima: t = (v0 + √(v0² − 2·g·dh)) / g. O alcance é 5 · t.
- Alcance nominal correndo: plano **3,32 m**; +0,5 m **2,88 m**; +0,75 m **2,59 m**; −0,5 m **3,66 m**. Com a Pluma (g = 10): plano **6,63 m**; +1,5 m **5,19 m**.
- O **limite teórico** é usado para provar que um atalho indesejado é impossível. Ele soma o pulo depois de 0,12 s de *coyote* (0,6 m a mais, saindo 0,38 m mais baixo) e 0,28 m de borda do controlador em cada lado. Plano: **4,16 m**. +0,5 m: **3,55 m**. Pluma a +1,5 m: **5,87 m**.

## Plataformas (topo em Y, medidas X × Z)

| Peça | Tipo | Centro do topo | Tamanho | Extensão |
| --- | --- | --- | --- | --- |
| N0 Entrada | `Platform` | (1, 0, −0,5) | 4 × 5 | x −1..3, z −3..2 |
| N0 Entrada canto E | `Platform` | (−2, 0, −2,25) | 2 × 1,5 | x −3..−1, z −3..−1,5 |
| N0 Entrada frente E | `Platform` | (−2, 0, 1,25) | 2 × 1,5 | x −3..−1, z 0,5..2 |
| N0 Piso escuro | `CrumblingPlatform` | (−2, 0, −0,5) | 2 × 2 | x −3..−1, z −1,5..0,5 |
| F0 Fosso | `Platform` | (−2, −0,8, −0,5) | 2 × 2 | sob o piso escuro |
| N1 Sala dos espinhos fixos | `Platform` | (0, 0, 8,5) | 6 × 9 | z 4..13 |
| N2-N3 Corredor do ritmo | `Platform` | (0, 0, 18,5) | 2 × 11 | x −1..1, z 13..24 |
| N4 Patamar da escolha | `Platform` | (0, 0, 28) | 8 × 8 | x −4..4, z 24..32 |
| N5 Pedestal da Pluma | `BookPlatform` (1 livro) | (−3, 0,5, 27) | 2 × 2 | x −4..−2, z 26..28 (sobre N4) |
| N6 Galeria lateral | `BookPlatform` | (7,5, 0, 29) | 3 × 8 | x 6..9, z 25..33 |
| N7 Degrau lateral | `BookPlatform` | (8, 0,5, 37,5) | 3 × 5 | x 6,5..9,5, z 35..40 |
| N8 Salão | `Platform` | (0, 0, 41,5) | 8 × 8 | x −4..4, z 37,5..45,5 |
| T0 Corredor escuro | `CrumblingPlatform` | (0, −1, 48,75) | 2 × 6,5 | z 45,5..52 |
| N9-N10 Corredor do laser | `Platform` | (−7, 0, 45) | 3 × 10 | x −8,5..−5,5, z 40..50 |
| N11 Degrau final | `BookPlatform` | (−7, 0,75, 53) | 3 × 4 | z 51..55 |
| N12 Patamar da porta | `OpenBookPlatform` | (−0,5, 1,5, 56) | 7 × 8 | x −4..3, z 52..60 |

As armadilhas são espinhos fixos em (−2, 0, 7,5), (0, 0, 7,5), (0, 0, 11) e (2, 0, 11). Os espinhos em ciclo do par A ficam em (0, 0, 15) e (0, 0, 17); os do par B, em (0, 0, 21) e (0, 0, 23). O laser fica em (−7, 0, 47,5), com yaw 0. A Pluma fica em (−3, 0,5, 27), a porta em (0, 1,5, 58) com yaw 180, o *spawn* em (0,5, 0, −0,8) com yaw 0 (pufe em z = −2,2) e a zona de queda em y = −8.

**Estantes que limitam o caminho** (linha central; a face interna fica a 0,3 m dela):

| Trecho | Estantes | Face interna |
| --- | --- | --- |
| N0 | x = ±3,3; fundo z = −3,3; frente z = 2,3 (fora do arco) | x = ±3; arco com vão de 2,4 m (x ±1,2) |
| N1 | x = ±3,3; frente z = 13,3 (de \|x\| 1,45 para fora) | x = ±3; boca de 1,7 m |
| N2–N3 | x = ±1,15, altura 4,5 | x = ±0,85 (1,7 m livres) |
| N4 | esquerda x = −4,3, fundo z = 23,7, altura 6 | x = −4; z = 24 |
| T0 | x = ±1,15 (base −1, altura 4); fundo z = 51,7 (topo em y = 1,5) | x = ±0,85 |
| N9–N10 | x = −8,65 e x = −5,35 (esta só de z 45,5 a 50) | x = −8,35 e −5,65 (2,7 m) |
| N12 | direita x = 3,3; frente z = 60,3; fundo z = 51,7 (x −3..−1,45 e 1,45..3,6) | x = 3; z = 60 |

## Pontos do grafo

| Ponto | Coordenada | Referência no mapa |
| --- | --- | --- |
| N0 | (0,5, 0, −0,8) | Spawn, com o pufe atrás |
| F0 | (−2, −0,8, −0,5) | Fosso sob o piso escuro (seguro, sem saída para fora) |
| N1 | (0, 0, 9,25) | Faixa livre entre as fileiras de espinhos fixos |
| N2 | (0, 0, 13,5) | Observação, antes do par A |
| N3 | (0, 0, 19) | Refúgio entre os pares A e B (tapete) |
| N4 | (0, 0, 28) | Patamar da escolha, à beira do vão de 5,5 m |
| N5 | (−3, 0,5, 27) | Pedestal da Pluma |
| N6 | (7,5, 0, 29) | Galeria lateral, desvio sem poder |
| N7 | (8, 0,5, 37,5) | Degrau lateral |
| N8 | (0, 0, 41,5) | Salão, onde os dois caminhos se juntam |
| T0 | (0, −1, 48,75) | Corredor escuro, **inseguro** |
| N9 | (−7, 0, 43) | Entrada do corredor do laser |
| N10 | (−7, 0, 49,5) | Fim do corredor do laser |
| N11 | (−7, 0,75, 53) | Degrau final |
| N12 | (−0,5, 1,5, 56) | Patamar da porta |
| N13 | (0, 1,5, 58) | Porta de conclusão |

## Grafo das rotas

Como no DS-14, o estado é o par **(ponto, tem Pluma)**: as arestas duplas (`==>`) só existem com a Pluma. As arestas pontilhadas levam ao ponto inseguro. O peso de cada aresta é a distância em linha reta no plano XZ.

```mermaid
flowchart LR
    N0["N0 spawn"] -.->|"pisar no escuro · 2,52"| F0["F0 fosso"]
    F0 -->|"subir 0,8 m · 2,52"| N0
    N0 -->|"pulo 2,0 m · 10,06"| N1["N1 espinhos fixos"]
    N1 -->|"caminhar, desvio · 4,25"| N2["N2 observação"]
    N2 -->|"ritmo: par A · 5,5"| N3["N3 refúgio"]
    N3 -->|"ritmo: par B · 9,0"| N4["N4 patamar da escolha"]
    N4 -->|"subir 0,5 m · 3,16"| N5["N5 Pluma"]
    N5 -->|"descer, ganha a Pluma · 3,16"| N4
    N4 ==>|"pulo com Pluma 5,5 m · 13,5"| N8["N8 salão"]
    N4 -->|"pulo 2,0 m · 7,57"| N6["N6 galeria lateral"]
    N6 -->|"pulo 2,0 m, +0,5 · 8,51"| N7["N7 degrau lateral"]
    N7 -->|"pulo 2,5 m, −0,5 · 8,94"| N8
    N8 -->|"pulo 1,5 m · 7,16"| N9["N9 corredor do laser"]
    N9 -->|"ritmo: laser · 6,5"| N10["N10 fim do corredor"]
    N10 -->|"pulo 1,0 m, +0,75 · 3,5"| N11["N11 degrau final"]
    N11 -->|"pulo 1,5 m, +0,75 · 7,16"| N12["N12 patamar da porta"]
    N10 ==>|"pulo com Pluma 2,5 m, +1,5 · 9,19"| N12
    N12 -->|"caminhar · 2,06"| N13["N13 porta"]
    N8 -.->|"descer 1 m · 7,25"| T0["T0 corredor escuro"]
    T0 -.->|"voltar em menos de 0,8 s"| N8
    style T0 fill:#f8cccc,stroke:#bb3333
    style F0 fill:#f3e3c3,stroke:#b58a3a
    style N5 fill:#cfeefe,stroke:#3a8fb5
    style N13 fill:#cdeccd,stroke:#3a8a3a
```

F0 é um beco seguro: não leva a lugar nenhum além de N0. Não há aresta de T0 para N12 nem de T0 para N13. A porta fica 2,5 m acima do piso do corredor, mais que os 2,2 m de apogeu com a Pluma, e o piso cede de qualquer forma.

**Rotas e distâncias** (somando os pesos, sem contar o zigue-zague dentro de N1):

| Rota | Pontos | Arestas | Distância |
| --- | --- | --- | --- |
| Curta e arriscada (com Pluma) | N0 → N1 → N2 → N3 → N4 → N5 → N4 → N8 → N9 → N10 → N12 → N13 | 11 | **73,5 m** |
| Com Pluma, pelo degrau | … → N10 → N11 → N12 → N13 | 12 | 75,0 m |
| Longa e segura (sem Pluma) | N0 → N1 → N2 → N3 → N4 → N6 → N7 → N8 → N9 → N10 → N11 → N12 → N13 | 12 | **80,2 m** |

A rota com a Pluma é a mais curta em arestas e em metros, mas tem o salto mais apertado da fase (margem de 1,13 m sobre o vazio). A rota sem Pluma sempre existe e só tem pulos com mais de 0,88 m de margem: o poder é um atalho, não uma exigência.

## Cálculo de cada pulo

O vão é medido de borda a borda; a margem é o alcance nominal menos o vão.

| Salto | Vão | Desnível | Alcance nominal | Margem |
| --- | --- | --- | --- | --- |
| F0 → N0 (sair do fosso) | — | +0,8 m | apogeu 1,1 m | 0,3 m de altura |
| N0 → N1 | 2,0 m | 0 | 3,32 m | 1,32 m |
| N4 → N5 (subir no pedestal) | — | +0,5 m | apogeu 1,1 m | 0,6 m de altura |
| N4 → N6 | 2,0 m | 0 | 3,32 m | 1,32 m |
| N6 → N7 | 2,0 m | +0,5 m | 2,88 m | 0,88 m |
| N7 → N8 | 2,5 m | −0,5 m | 3,66 m | 1,16 m |
| **N4 → N8 com Pluma** | 5,5 m | 0 | 6,63 m | **1,13 m** |
| N4 → N8 sem Pluma | 5,5 m | 0 | 3,32 m (limite teórico 4,16 m) | impossível, faltam 1,34 m |
| N8 → N9 | 1,5 m | 0 | 3,32 m | 1,82 m |
| N10 → N11 | 1,0 m | +0,75 m | 2,59 m | 1,59 m |
| N11 → N12 | 1,5 m | +0,75 m | 2,59 m | 1,09 m |
| N10 → N12 com Pluma | 2,5 m (diagonal 1,5 × 2,0) | +1,5 m | 5,19 m | 2,69 m |
| N10 → N12 sem Pluma | — | +1,5 m | — | impossível, o pulo sobe 1,1 m |
| T0 → N8 (fuga) | ~0,5 m | +1,0 m | 2,16 m | 0,1 m de altura; só antes de o piso ceder |

**O fosso de N0 é fechado.** O piso do fosso fica 0,8 m abaixo do piso da sala. Nos lados que dão para os outros pedaços do piso da sala, a laje vizinha (0,5 m de espessura) deixa só 0,3 m de vão por baixo, menos que a cápsula de 1,76 m. No lado oeste, a estante esquerda desce até y = −0,8, e na frente o piso "frente E" e o arco fecham o fosso. O jogador só sai pulando de volta para N0 (0,8 m, abaixo do degrau máximo de 0,9 m). O piso escuro não volta depois de cair, mas fica fora da rota; o fosso continua seguro.

Para não sobrar atalho indesejado, cada um foi comparado com o limite teórico:

- Quina de N4 (4, 32) até a quina de N7 (6,5, 35): 3,90 m e +0,5 m, contra o limite de 3,55 m. Não dá para pular N6, e o pilar em (3,5, 0, 31,5) ainda atrapalha a corrida.
- Quina de N6 (6, 33) até a quina de N8 (4, 37,5): 4,92 m contra o limite de 4,16 m. Não dá para pular N7.
- Do salão N8 (z 45,5) até N12 (z 52) por cima do corredor escuro, com a Pluma: 6,5 m e +1,5 m, contra o limite de 5,87 m. Também é impossível, e a estante de fundo de N12 (topo em y = 4,5) ainda fica no caminho.
- A mesa de N4, em (2,5, 0, 25,2), sobe 0,8 m, mas fica no fundo do salão: dela até N8 são mais de 11 m, e até N6 o pulo só encurta um trecho que já é seguro.
- As estantes têm de 3 a 6 m acima do piso em que estão e o pé alcança no máximo 2,2 m com a Pluma (3 m saindo de cima de uma mesa, que fica sempre longe das estantes de 3 m), então nenhuma estante pode ser escalada.

## Ritmo das armadilhas

**Espinhos fixos (N1).** O gatilho de cada campo mede 1,8 m e fica de 0,05 m a 0,65 m acima do piso. Um pulo normal mantém o pé acima de 0,65 m por apenas 2,12 m de avanço. Para passar por cima, seria preciso cobrir 1,8 + 2 · 0,28 = 2,36 m, então o desvio é obrigatório. A primeira fileira (z 7,5) deixa livre o lado direito (x +2), e a segunda (z 11) deixa livre o lado esquerdo (x −2). Entre o pouso do vão e a primeira fileira há 2,6 m de piso.

**Espinhos em ciclo (N2–N3).** Os espinhos ficam 1,2 s expostos e 1,5 s recolhidos, num período de 2,7 s; nos últimos 0,3 s recolhidos começam a subir, como aviso. O par B tem `offset` de 1,35 s (meio período). Em termos da fase de A (T_A = tempo mod 2,7):

- A fere quando T_A está entre 0 e 1,2 s; B fere quando T_A está entre 1,35 e 2,55 s. É uma alternância quase perfeita.
- Atravessar um par vai de z 13,82 a 18,18, ou seja, 4,36 m a 5 m/s: 0,87 s (cerca de 0,97 s saindo parado). Cada janela segura dura 1,5 s, o que deixa cerca de 0,5 s de folga.
- O jeito de aprender é parar em N2 e ver um ciclo. Quando A recolhe (T_A = 1,2), atravessa-se A e espera-se no refúgio N3: são 2,2 m livres (z 17,9..20,1, com o tapete de 1,8 m no meio), e o corpo tem 0,56 m. Quando B recolhe (T_A = 2,55), atravessa-se B até T_A = 4,05.
- Quem já entendeu o ritmo pode atravessar sem parar, correndo a 5 m/s a partir de z 13,82 e entrando entre T_A = 1,35 e 1,83 s (uma janela de 0,48 s).
- As estantes do corredor (face em x = ±0,85) deixam 1,7 m livres, menos que os 1,8 m do gatilho: não há faixa lateral sem espinho.
- As fases dos ciclos usam `Time.time`, que não zera ao recarregar a cena. O ritmo é sempre o mesmo, mas o ponto do ciclo em que a tentativa começa varia. Por isso o jogador deve observar antes de passar.

**Laser (N9–N10).** O feixe fica 1,5 s ligado e 1,8 s apagado, e pisca nos 0,4 s antes de ligar. O gatilho vai de 0,825 m a 1,175 m de altura. Agachado, o jogador tem 1,06 m e ainda toca o feixe; o pulo normal não passa por cima. A travessia cobre 0,25 + 0,56 m, o que leva 0,16 s correndo ou 0,37 s andando, contra uma janela de 1,8 s. A largura interna do corredor é 2,7 m, a mesma do feixe, então não há como contornar. Com a Pluma, o pé fica acima de 1,175 m por 4,53 m e dá para saltar o laser: é uma recompensa extra de quem pegou o poder.

## Truque de cenário: o corredor escuro

Do salão N8 o jogador vê a porta em linha reta, no fim de um corredor estreito de estantes altas. É o caminho óbvio. O caminho verdadeiro desvia para o oeste, por um corredor iluminado com um laser, e sobe dois degraus de livros.

A falha é justa:

- A regra foi apresentada sem risco na sala de leitura (N0), com o mesmo piso escuro e a placa `ONDE HÁ LUZ, HÁ CHÃO`.
- O corredor não tem nenhuma luminária, enquanto todo piso firme da fase tem uma.
- O piso é o `Trap_PisoFalso`, com material diferente, e fica 1 m abaixo do salão.
- O letreiro `SIGA AS LUZES` está ao lado da entrada, e o tapete do salão aponta para o oeste.
- O piso treme por 0,8 s antes de ceder, e dá para voltar ao salão (+1 m) nesse tempo.

Se o jogador insiste, o piso cai. Mesmo que ele chegasse ao fundo, a porta está 2,5 m acima e protegida por uma estante, fora do alcance até com a Pluma. A lição vale para as fases seguintes: o caminho mais curto à vista nem sempre é uma aresta do grafo, e a luz marca o chão firme.

## Como gerar

Com a Unity fechada para outros processos:

```sh
/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath "$PWD" \
  -executeMethod Dreamscape.EditorTools.PhaseBuildMenu.BuildAllBatch
```

## Pendências

- Esta versão foi escrita sem abrir a Unity. Os números vêm do motor e precisam de *playtest*. Pontos a observar: a altura real dos letreiros (a placa do fosso usa acentos com a fonte `LegacyRuntime`), os postes do laser, que ficam 0,15 m dentro das estantes laterais, e o número de luzes pontuais por sala (luminárias, abajures e velas).
- O grafo deste documento descreve a fase; a cena ainda não tem um `PhaseRouteGraph` como o da Fase 1.
