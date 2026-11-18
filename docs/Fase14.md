# Fase 14: Torre do Relógio

Cena: `Assets/Scenes/Fase14_TorreDoRelogio.unity`, gerada por `Assets/Scripts/Editor/Phases/Fase14Builder.cs`. O tema fica na própria classe (`Fase14Builder.Tema`): continua a noite da Fase 13, agora em anil profundo com destaque de latão (`Accent` 1,0 / 0,78 / 0,35). Saturação 16, exposição 0,09, neblina 0,022.

## Ideia central

Uma torre de estantes de 14 m (o núcleo, 12 × 12 m) com uma passarela em espiral em volta. **Tudo bate no mesmo tique de 3 s.** Lasers, espinhos, prensas e pêndulos usam o mesmo período, e os offsets formam ondas: quem corre a 5 m/s encontra cada armadilha na mesma fase do ciclo. Quem não pega a onda espera nos cantos ou nos vãos entre armadilhas. O objetivo fica no topo, sob um mostrador gigante, visível da praça de entrada.

Ritmo: a **volta 1** apresenta cada armadilha sozinha, a **volta 2** junta duas no mesmo trecho e a **volta 3** é o desafio (carrilhão de três pêndulos e um lado inteiro em onda). O último canto guarda o Pulo Duplo para o degrau final.

Dicas no cenário: `FASE 14` / `TORRE DO RELOGIO` / `TUDO BATE NO MESMO TIQUE` na praça, e `ULTIMO DEGRAU: PULO DUPLO` sobre o último canto. A luz de latão marca refúgios e patamares; a luz anil marca as sacadas dos pêndulos.

## Números do motor

Os valores são os mesmos de `docs/Fase3.md`: gravidade −20 m/s², pulo 1,1 m (v0 = 6,633 m/s), corrida 5 m/s, raio 0,28 m, altura 1,76 m, *coyote* 0,12 s e `stepOffset` 0,3 m.

- Apogeu do pulo: 1,1 m. Somando o `stepOffset`, o pé alcança no máximo uma borda **1,4 m** acima. Com o Pulo Duplo, o apogeu vai a 2,2 m e a borda alcançável a **2,5 m**.
- Pulo comum: no plano alcança 3,32 m; em +0,6 m, (6,633 + √(44 − 24)) / 20 · 5 = **2,78 m**.
- Pulo Duplo em +1,6 m (melhor momento: o segundo pulo a meia altura): T = (6,633 + √(44 − 20 · 1,6)) / 10 = 1,01 s, ou seja, **5,0 m**.
- Um pulo comum mantém o pé acima de 0,65 m (topo do gatilho dos espinhos) por 2,12 m e nunca acima de 1,175 m (topo do feixe). Por isso **não se pula laser nem uma fileira de espinhos** (1,8 + 0,56 = 2,36 m) sem poder.

## Geometria da espiral

O núcleo ocupa |x|, |z| ≤ 6. A passarela tem 3 m de largura (de 6 a 9 do centro), com o núcleo de um lado e o vazio do outro. São 10 lados de 12 m. Cada lado tem o trecho **A** (s −6..0) e o trecho **B** (s 0..6, 0,6 m mais alto), e depois vem um canto 3 × 3 m, mais 0,6 m acima. Cada lado sobe 1,2 m e cada volta sobe 4,8 m.

O sentido é anti-horário visto de cima: oeste subindo em +Z, norte em +X, leste em −Z e sul em −X. No código, `P(d, s, u, y)` converte (lado, posição ao longo do lado, deslocamento lateral) em coordenadas do mundo; u > 0 aponta para fora do núcleo.

| Lado | Faixa | Trecho A (y) | Trecho B (y) | Canto seguinte (y) |
| --- | --- | --- | --- | --- |
| S1 oeste | x −9..−6, z −6..6 | 0,0 | 0,6 | C1 NO (−9..−6, 6..9): 1,2 |
| S2 norte | z 6..9, x −6..6 | 1,2 | 1,8 (sacada) | C2 NE: 2,4 |
| S3 leste | x 6..9, z 6..−6 | 2,4 | 3,0 | C3 SE: 3,6 |
| S4 sul | z −9..−6, x 6..−6 | 3,6 (sacada) | 4,2 (sacada) | C4 SO: 4,8 |
| S5 oeste | x −9..−6 | 4,8 | 5,4 (sacada) | C5: 6,0 |
| S6 norte | z 6..9 | 6,0 | 6,6 | C6: 7,2 |
| S7 leste | x 6..9 | 7,2 (sacada) | 7,8 | C7: 8,4 |
| S8 sul | z −9..−6 | 8,4 | 9,0 | C8: 9,6 |
| S9 oeste | x −9..−6 | 9,6 (sacada) | 10,2 (sacada) | C9: 10,8 |
| S10 norte | z 6..9 | 10,8 | 11,4 | C10 NE (6..9, 6..9): 12,0, com o **Pulo Duplo** |
| S11 último degrau | x 6..9, z 1..6 | 13,6 | | |
| Mostrador (topo) | x −6..6, z −6..6 | 13,6, com a porta em (−2,5; 13,6; 0), yaw 90 | | |

A **praça** de entrada vai de x −14 a −6 e de z −14 a −6, em y 0, e contém o canto C0. O *spawn* fica em (−7,5; 0; −11,5), olhando para +Z. As **sacadas** são os trechos com pêndulo: têm 5,6 m (u −1,5..4,1) e o pivô fica em u = 1,3. O pêndulo de 52° alcança 2,65 · sen 52° + 0,62 = 2,71 m para cada lado do pivô, isto é, de u −1,41 a 4,01, sem entrar no núcleo nem passar da borda. A viga de cada pêndulo fica entre 3,7 m e 4,2 m acima do piso, abaixo da volta de cima (4,3 m). Todos os pisos da espiral são `BookPlatform` com `stackDepth` 0,5, um livro só da espessura do colisor, então nada pende para dentro da volta de baixo. A zona de queda fica em y = −8.

## O tique de 3 s

| Armadilha | Parâmetros | Fere quando T (= (t + offset) mod 3) está em | Janela segura | Travessia a 5 m/s |
| --- | --- | --- | --- | --- |
| Laser | 1,5 ligado / 1,5 apagado | 0..1,5 (pisca 0,4 s antes) | 1,5 s | 0,25 + 0,56 = 0,81 m → 0,16 s |
| Espinhos (fileira de 2 campos) | 1,2 fora / 1,8 dentro | 0..1,2 (sobem 0,3 s antes) | 1,8 s | 1,8 + 0,56 = 2,36 m → 0,47 s |
| Prensa | 1,4 erguida, 0,2 descendo, 0,6 embaixo, 0,8 subindo | 1,4..2,2 | 2,2 s | 1,6 + 0,56 = 2,16 m → 0,43 s |
| Pêndulo (52°, período 3 s) | x = 2,65 · sen(52° · sen(2πT/3)) | ver abaixo | | faixa de 1,8 m → 0,36 s |

**Onda.** Uma armadilha a `d` metros da primeira do grupo recebe offset `Onda(d) = (−d / 5) mod 3`. Quem cruza a primeira na fase φ chega a ela d/5 s depois, de novo na fase φ. Valores usados: Onda(2,5) = 2,5; Onda(3) = 2,4; Onda(6) = 1,8; Onda(9) = 1,2.

- Uma onda só de lasers aceita qualquer φ de 1,58 a 2,92 s (janela de 1,34 s).
- Laser seguido de espinhos: os espinhos exigem φ ± 0,24 dentro de [1,2; 3,0), e a janela comum fica entre 1,58 e 2,74 s (**1,16 s**).
- Laser seguido de prensa (S7B e S10B): a prensa fere para φ entre −0,02 e 1,22, faixa disjunta da janela do laser (1,58..2,92). **Quem passou o laser passa a prensa.**

**Pêndulo na sacada** (amplitude 52°, fase φ = 2πT/3):

- Junto ao núcleo (u = −1,2), o peso toca quando está a mais de 1,6 m para o lado do núcleo: sen θ < −0,604, θ < −37,1°, sen φ < −0,714. Isso dá 24,7% do ciclo, **0,74 s ferindo e 2,26 s seguros contínuos**. A altura do peso nesse ângulo é 1,68 m, então a esfera (raio 0,62) cobre o corpo.
- Junto à borda (u = 3,8), a situação é simétrica: 2,26 s seguros.
- No meio (sob o pivô), toca quando |deslocamento| < 0,9 m, ou seja, |θ| < 19,9°. São duas janelas de 0,38 s por ciclo e duas janelas seguras de 1,12 s. Cruzar a faixa leva 0,36 s, o que deixa **0,76 s de entrada** em cada uma.
- Regra que a fase ensina: **passe pelo lado de onde o peso acabou de sair.**

## Lados e armadilhas

As posições são dadas em s (ao longo do lado) e todos os offsets estão em segundos.

| Lado | Trecho A | Trecho B | O que treina |
| --- | --- | --- | --- |
| S1 | laser s −3, offset 0 | laser s 3, offset 1,5 | O tique: os dois lasers se alternam |
| S2 | espinhos s −3, offset 0 | pêndulo s 3 (sacada), offset 0 | Espinhos e o primeiro pêndulo |
| S3 | prensa s −3 (portão s −4..−2), offset 0 | laser s 1,5, offset 0, e espinhos s 4,5, Onda(3) = 2,4 | Prensa; primeira onda |
| S4 | pêndulo s −3, offset 0 | pêndulo s 3, offset 1,5 | Par em oposição: um à esquerda, outro à direita |
| S5 | lasers s −4,5 (0) e s −1,5 (2,4) | pêndulo s 3, offset 0 | Onda de lasers |
| S6 | prensas s −4,5 (0) e s −1,5 (2,4), portão s −5,5..−0,5 | espinhos s 1,2 (0) e s 4,8 (1,5) | Onda de prensas; fileiras alternadas com espera obrigatória no meio |
| S7 | pêndulos s −4,3 (0) e s −1,3 (0,75) | laser s 1,5 (0) e prensa s 4,5 (0,6), portão s 3,5..5,5 | Pêndulos em contratempo; laser → prensa |
| S8 | lasers s −4,5 (0) e s −1,5 (2,4) | lasers s 1,5 (1,8) e s 4,5 (1,2) | A contagem: quatro lasers numa onda só |
| S9 | pêndulos s −4,3 (0) e s −1,3 (1,0) | pêndulo s 3 (2,0) | Carrilhão: cada peso passa pelo centro 1 s depois do anterior |
| S10 | laser s −4,5 (0) e espinhos s −2 (2,5) | prensa s 1,5 (0), portão s 0,5..2,5, e laser s 4,5 (1,2) | O lado inteiro numa onda (janela de 1,16 s) |
| C10 → S11 | Pulo Duplo no canto; degrau de +1,6 m | | |

**Refúgios.** Quando não se pega a onda, sempre há onde parar (a posição do centro do corpo já desconta o raio de 0,28 m):

- Cantos: 3 × 3 m livres.
- Entre lasers a 3 m um do outro: 1,9 m.
- Laser → espinhos: em S3B, 1,4 m; em S10A, 0,9 m.
- Entre as fileiras de S6B: 1,24 m. Aqui a parada é obrigatória: quando a primeira fileira recolhe (T de 1,2 a 3,0), a segunda está ferindo (T de 1,5 a 2,7).
- Entre as prensas de S6A: 0,84 m.
- Espinhos → degrau → portão em S10: 1,24 m.
- Laser → prensa: 1,5 m.
- Entre pêndulos a 3 m: 1,2 m.

**Borda do controlador.** O controlador fica de pé na borda com o centro até 0,28 m além dela. Numa passarela de 3 m, então, o centro do corpo vai de u −1,22 (núcleo) a u **1,78** (borda externa). Por isso cada laser e cada fileira de espinhos tem uma **grade**: uma estante sólida de 3 m × 2,5 m na borda externa, centrada em u 1,8 (de 1,5 a 2,1). Com ela, o centro fica limitado a u ≤ 1,22. Contornar a grade pelo ar exigiria sair além de u 2,38 e voltar ao longo de 3 m. Com o controle aéreo (25 × 0,35 = 8,75 m/s²), uma saída de 0,6 m e a volta levariam 0,74 s, mais que o tempo de um pulo (0,66 s).

**Lasers.** O feixe tem 2,7 m (u −1,35..1,35); passar ao lado exigiria |u| > 1,63, mas o corpo fica entre −1,22 e 1,22.

**Fileiras de espinhos.** São dois campos 2 × 2 em u = ±0,5, que cobrem u de −1,4 a 1,4 (gatilho de 1,8 m cada). Passar ao lado exigiria |u| > 1,68.

**Portões das prensas.** As estantes de 3 m ficam paralelas ao caminho: a interna em u −1,1 (face em −0,8) e a externa em u 1,3 (face em 1,0, já passando da borda em 1,5). O bloco da prensa ocupa u −0,8..0,8; sobram 0,2 m ao lado, menos que o corpo (0,56 m). Entre o núcleo e a estante interna sobram 0,1 m. Por fora da estante externa não há piso: para ficar de pé ali, o centro do corpo teria de estar além de u 1,88, e o máximo é 1,78.

## Grafo

Os pontos são o *spawn* (N0), cada trecho (SiA, SiB), cada canto (Ci), o último degrau (S11) e a porta (G). Todas as arestas da espiral são passos de +0,6 m ou caminhadas no plano: não há vãos.

```mermaid
flowchart LR
    N0["N0 praça"] --> S1A --> S1B --> C1 --> S2A --> S2B --> C2 --> S3A --> S3B --> C3 --> S4A --> S4B --> C4
    C4 --> S5A --> S5B --> C5 --> S6A --> S6B --> C6 --> S7A --> S7B --> C7 --> S8A --> S8B --> C8
    C8 --> S9A --> S9B --> C9 --> S10A --> S10B --> C10["C10 Pulo Duplo"]
    C10 ==>|"+1,6 m, exige Pulo Duplo"| S11["S11 último degrau"] --> G["porta no mostrador"]
    style C10 fill:#cfeefe,stroke:#3a8fb5
    style G fill:#cdeccd,stroke:#3a8a3a
```

Percurso: 4 m na praça + 10 lados × 15 m (12 m de passarela e 3 m de canto) + 5 m no S11 + 8,5 m no topo, cerca de **168 m**.

## Saltos e degraus

| Passagem | Vão | Desnível | Alcance | Margem |
| --- | --- | --- | --- | --- |
| Trecho A → B e B → canto (×20) | 0 | +0,6 m | apogeu 1,1 m | 0,5 m de altura |
| C10 → S11 com Pulo Duplo | 0 (encostados em z = 6) | +1,6 m | apogeu 2,2 m | 0,6 m |
| C10 → S11 sem poder | 0 | +1,6 m | 1,1 + 0,3 = 1,4 m | impossível |
| S11 → mostrador | 0 | 0 | caminhar pela abertura do parapeito (z 1..5,4) | |

## Atalhos impossíveis

- **Pular para a volta de cima:** a volta seguinte está 4,8 m acima, e a borda alcançável é de 1,4 m (2,5 m com o Pulo Duplo, que só existe no fim). Também não dá para subir pelo lado de fora: o piso de cima está 4,3 m acima da cabeça de quem está embaixo.
- **Cortar o canto por dentro:** o núcleo é sólido até y 13,1. Entre o fim de um lado e o começo do seguinte só há o canto ou o ponto de encontro dos dois trechos junto à quina do núcleo. Passar por essa quina não pula nenhuma armadilha; os cantos não têm armadilhas.
- **Contornar armadilhas:** feixes e fileiras de espinhos (com a grade da borda) e portões das prensas cobrem a largura útil, como descrito acima. Os pêndulos varrem toda a sacada, de u −1,41 a 4,01, numa sacada de u −1,5 a 4,1.
- **Saltar armadilhas:** sem poder não se pula laser (o pé nunca passa de 1,175 m) nem fileira de espinhos (2,12 m < 2,36 m). O Pulo Duplo só existe no último canto, depois de todas as armadilhas.
- **Chegar ao topo pelo S10:** o S10B está em 11,4 m e o topo do parapeito norte em 13,6 + 0,9 = 14,5 m. São 3,1 m, mais que 2,5 m, mesmo com o Pulo Duplo.
- **Chegar ao topo pelo C10:** do C10 (12,0) ao parapeito norte são exatos 2,5 m com o Pulo Duplo, o limite teórico. Mesmo que alguém consiga, só pula o S11, que não tem armadilha.
- **Cair:** a borda externa dá para o vazio, então cair reinicia a fase. Quem cai de uma sacada da volta 2 ou 3 passa ao lado da volta de baixo (as sacadas ficam fora do anel de 3 m) e também cai no vazio.

## Poderes

Só há o **Pulo Duplo**, no canto C10 (6..9, 6..9, y 12,0). Ele é coletado ao lado do único degrau que o exige. Se o efeito de 30 s acabar, o coletável reaparece no mesmo canto.

## Riscos e pendências

- O código não foi rodado na Unity quando a fase foi escrita; os números precisam de *playtest*.
- O ciclo usa `Time.timeSinceLevelLoad`, então cada tentativa recomeça no mesmo ponto do tique. Mesmo assim, quem para num refúgio entra na onda em qualquer momento.
- Pontos a observar:
  - a altura real do modelo da prensa sob a volta de cima (4,3 m livres);
  - a viga dos pêndulos (topo a 4,2 m) encostando visualmente no piso de cima (4,3 m);
  - os postes do laser, que ficam na face do núcleo (u = −1,5) e na borda;
  - as grades de estante (u 1,5..2,1), que ficam penduradas fora da passarela; em S10A, a grade do laser e a dos espinhos se sobrepõem 0,5 m.
- O núcleo tem 4 estantes de 12 × 14,6 m. Com as fileiras compartilhadas do `Bookshelf` o custo deve ser aceitável, mas vale medir.
