# Fase 8: Saguão dos Sussurros

Cena: `Assets/Scenes/Fase8_SaguaoDosSussurros.unity`, gerada por `Assets/Scripts/Editor/Phases/Fase8Builder.cs`. O tema é próprio da fase (`Fase8Builder.Tema`) e abre o segundo arco, "a noite na biblioteca até o amanhecer": noite quente, saturação 0, exposição −0,1, neblina densa (0,03) e destaque **âmbar de vela** (1; 0,66; 0,30).

## Ideia central

O hall de entrada da biblioteca mágica. Cada sala reapresenta uma mecânica, com calma, e tem uma placa curta (um "sussurro") que explica o que fazer. A regra visual da fase é **onde há tapete, dá para parar**. Todo patamar seguro tem um tapete vermelho com o friso âmbar, e os trechos sem tapete pedem atenção: espinhos, pêndulo, laser e a ponte que cede.

Ritmo: **apresentar** (pulos, degraus, espinhos fixos) → **testar** (ritmo dos espinhos, pêndulo, ponte que cede, laser) → **desafio final** (salto de 5 m com a Pluma até a porta).

## Sequência

| # | Trecho | Mecânica | Sussurro |
| --- | --- | --- | --- |
| 1 | S0 Saguão | *spawn*, portal de estantes | `onde ha tapete, da para parar` |
| 2 | P1–P3 Livros empilhados | vãos de 1,5 / 2,0 / 2,0 m, sobe e desce 0,5 m | — |
| 3 | F1 Sala de leitura | mesas de leitura como degraus de 0,8 m até o mezanino (+1,6 m) | `suba pelas mesas` |
| 4 | M1 Mezanino | duas fileiras de espinhos fixos desencontradas | `desvie, nao pule` |
| 5 | Corredor do ritmo | dois pares de espinhos em ciclo, com refúgio no meio | `observe o ritmo` |
| 6 | R1 Rotunda | livro aberto gigante; Escudo **opcional** | `espere o pendulo passar` |
| 7 | Galeria do pêndulo | pêndulo que atravessa a passarela | — |
| 8 | L1 → ponte → L2 | quatro livros que cedem (sem tapete) | `nao pare` |
| 9 | Corredor do laser | laser na largura exata do corredor | `espere apagar` |
| 10 | L3 → GP | Pluma e vão de 5 m até a porta | `a pluma te leva` |

Marcos: um relógio sobre a rotunda (visto do mezanino e do corredor) e outro atrás da porta. A porta fica voltada para o sul, de frente para quem chega da Pluma.

Percurso: saguão → corredor 50,5 m; rotunda → L1 19 m; L1 → L2 16,2 m; L2 → L3 13 m; L3 → porta 14,3 m. Total de **~113 m**.

## Números do motor

São os mesmos de `docs/Fase3.md`: gravidade −20, pulo de 1,1 m (v0 = 6,633 m/s), corrida 5 m/s, raio 0,28 m, *coyote* 0,12 s.

- Tempo no ar até pousar Δh acima: t = (6,633 + √(44 − 40·Δh)) / 20. O alcance é 5·t.
  - Nominal: plano **3,32**; +0,5 **2,88**; +0,8 **2,52**; −0,5 **3,66**.
- **Limite teórico** (o mesmo critério da Fase 3, usado para provar que um atalho é impossível): L = máx(0,56 + 5·t(Δh); 1,16 + 5·t(Δh + 0,38)).
  - Plano **4,16**; +0,5 **3,56**; +1,0 **2,71**.
- Altura: o pé chega a 1,1 m e, rolando sobre a quina, a ~1,38 m.
- **Pluma** (g = 10): t = (6,633 + √(44 − 20·Δh)) / 10. Plano **6,63** nominal; apogeu 2,2 m.

## Plataformas (topo em Y; extensão em X e Z)

| Peça | Centro do topo | Tamanho | Extensão |
| --- | --- | --- | --- |
| S0 Saguão | (0, 0, 0) | 10 × 8 | x −5..5, z −4..4 |
| P1 | (0, 0, 7) | 3 × 3 | z 5,5..8,5 |
| P2 | (0, 0,5, 12) | 3 × 3 | z 10,5..13,5 |
| P3 | (0, 0, 17,25) | 3 × 3,5 | z 15,5..19 |
| F1 Sala de leitura | (0, 0, 24) | 8 × 8 | x −4..4, z 20..28 |
| Mesas-degrau (sólidas, tampo 0,8) | (±1,4, 0, 26,4) | 1,6 × 0,8 | z 26..26,8 |
| M1 Mezanino | (0, 1,6, 32) | 6 × 10 | x −3..3, z 27..37 |
| Corredor do ritmo | (0, 1,6, 43) | 2,9 × 12 | z 37..49; 1,7 m livres entre as estantes |
| R1 Rotunda (livro aberto) | (0, 1,6, 53) | 8 × 8 | x −4..4, z 49..57 |
| Galeria do pêndulo | (10, 1,6, 53) | 12 × 3 | x 4..16, z 51,5..54,5 |
| L1 | (19, 1,6, 53) | 4 × 4 | x 17..21, z 51..55 |
| Livros que cedem 1–4 | (19, 1,6, 57 / 59,8 / 62,6 / 65,4) | 2 × 2 | vãos de 0,8 m |
| L2 | (19, 1,6, 69,2) | 4 × 4 | z 67,2..71,2 |
| Corredor do laser | (13, 1,6, 69,2) | 8 × 3,9 | x 9..17; 2,7 m livres |
| L3 (Pluma) | (6, 1,6, 69,2) | 6 × 6 | x 3..9, z 66,2..72,2 |
| GP Patamar da porta (livro aberto) | (6, 1,6, 81,2) | 8 × 8 | x 2..10, z 77,2..85,2 |

Armadilhas e coletáveis:

- **Espinhos fixos**: (−1,4 / 0,4; 1,6; 30,8) e (1,4 / −0,4; 1,6; 33,6).
- **Espinhos em ciclo**: par A em z 40 e 42; par B em z 46 e 48. Ficam 1,2 s expostos e 1,8 s recolhidos; o par B tem offset de 1,5 s.
- **Pêndulo**: (10; 1,6; 53), yaw 90, 50°, 3,2 s.
- **Laser**: (13; 1,6; 69,2), yaw 90, 1,4 s ligado e 2,0 s desligado.
- **Coletáveis**: Escudo em (2,8; 1,6; 53) e Pluma em (5; 1,6; 69,2).
- **Marcos**: porta em (6; 1,6; 83,5) com yaw 180, *spawn* em (0, 0, −1,5) e zona de queda em y = −6.

## Contas de cada vão e degrau

| Salto | Vão | Desnível | Alcance | Margem |
| --- | --- | --- | --- | --- |
| S0 → P1 | 1,5 | 0 | 3,32 | 1,82 |
| P1 → P2 | 2,0 | +0,5 | 2,88 | 0,88 |
| P2 → P3 | 2,0 | −0,5 | 3,66 | 1,66 |
| P3 → F1 | 1,0 | 0 | 3,32 | 2,32 |
| F1 → tampo da mesa | — | +0,8 | apogeu 1,1 | 0,3 de altura |
| Tampo → M1 | 0,2 | +0,8 | 2,52 | 0,3 de altura |
| GA → L1 | 1,0 | 0 | 3,32 | 2,32 |
| L1 → livro 1, entre livros, livro 4 → L2 | 1,0 / 0,8 / 0,8 | 0 | 3,32 | ≥ 2,3 |
| **L3 → GP com Pluma** | **5,0** | 0 | 6,63 | **1,63** |

Os demais trechos são piso contínuo: corredor → R1 → galeria, e L2 → corredor do laser → L3.

### Atalhos indesejados (impossíveis)

- **F1 → M1 sem as mesas**: +1,6 m, mais que os 1,38 m do pé rolando na quina. A face do mezanino (colisor de 1,1 a 1,6 m) barra quem passa por baixo.
- **Mesa → fileira 1 de espinhos**: o pulo mais longo sai da borda da mesa (z 26,8) e chega a 26,8 + 2,52 + 0,28 = **29,6**. O gatilho começa em z 29,9, então não dá para cair nos espinhos pulando do degrau.
- **Pular uma fileira de espinhos**: o pé fica acima do gatilho (0,65 m) por 2,12 m, e a fileira exige 1,8 + 0,56 = 2,36 m. Desviar é obrigatório. As passagens livres medem 1,1 m (x 1,3..2,4 e −2,4..−1,3), há 1,0 m entre as fileiras e 1,9 m entre a fileira 2 e a parede.
- **Estantes da sala (3,4 m) a partir do mezanino**: +1,8 m, impossível.
- **Corredor do ritmo**: 1,7 m livres contra 1,8 m de espinhos. Não há como passar pelo lado, e as estantes têm 3 m.
- **L3 → GP sem Pluma**: 5,0 m contra o limite de 4,16. O vão é impossível.
- **Do corredor do laser direto para GP**: de (10; 71,15) a z 77,2 são 6,05 m, e a estante norte do corredor (3 m) fica no meio.
- **L2 → GP**: 9,2 m. **R1 → L3**: 9,2 m.
- **Pular um livro que cede**: 0,8 + 2 + 0,8 = 3,6 m, menos que o limite. É possível, mas inofensivo, porque o livro seguinte também cede.

## Ritmo das armadilhas

Os ciclos usam `Time.timeSinceLevelLoad`, então recomeçam iguais a cada tentativa.

- **Espinhos em ciclo**: período de 3,0 s; o par B está meio ciclo atrasado.
  - Atravessar um par cobre 3,8 + 0,56 = 4,36 m, o que leva 0,87 s correndo, contra uma janela de 1,8 s.
  - O refúgio vai de z 42,9 a 45,1 (2,2 m) e tem tapete. A observação se faz sob o portal (z 37..39,1).
- **Pêndulo**: a massa fica a 2,65 m do pivô e tem raio de 0,62 m.
  - Pelo centro da passarela, ele fere enquanto |θ| < 19,9°: são 0,42 s por passagem e **1,18 s livres** entre passagens.
  - A faixa de perigo tem 1,8 m, o que leva 0,36 s correndo.
  - O alcance lateral (2,03 + 0,9 m) cobre a passarela inteira, então não há como contornar.
- **Livros que cedem**: cada livro dura 0,8 s depois de pisado. Correndo, gasta-se ~0,4 a 0,6 s em cada um.
- **Laser**: o feixe vai de 0,825 m a 1,175 m de altura, mais alto que o pulo alcança.
  - Atravessar leva 0,16 s, contra 2,0 s apagado.
  - O corredor tem 2,7 m livres, a mesma largura do feixe, e fora das estantes não há piso.
- **Escudo** (opcional): absorve um toque do pêndulo ou do laser. Fica na rotunda, logo antes dos dois.

## Riscos e pendências

- Nada foi jogado ainda; os números vêm do motor.
- A face do mezanino é um colisor de 0,5 m sobre o vazio. Vale conferir se o `CharacterController` não fica preso sob a borda entre z 27 e 28.
- Pousar no tampo da mesa: o colisor tem 0,8 m de profundidade em Z. Se ficar difícil, girar as mesas (yaw 90) dá 1,6 m.
- A viga do pêndulo é um `BookPlatform` sólido a 3,8 m do piso. É inalcançável, porque a Pluma só aparece no fim.
- O laser (yaw 90) tem os postes encostados nas estantes. É preciso conferir se eles aparecem.
