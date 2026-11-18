# Fase 10: Escadaria de Livros

Cena: `Assets/Scenes/Fase10_EscadariaDeLivros.unity`, gerada por `Assets/Scripts/Editor/Phases/Fase10Builder.cs`. O tema é próprio (`Fase10Builder.Tema`): madrugada azulada, saturação 8, exposição 0, neblina 0,026 e destaque **azul-celeste** (0,45; 0,72; 1).

## Ideia central

Uma subida vertical em espiral, no sentido anti-horário visto de cima, em volta de uma torre de estantes de 19 m com um relógio flutuando no topo. O relógio é visível desde o início. A pista tem 3 m de largura e encosta na torre, que fica do lado de dentro; do lado de fora é o vazio.

A ideia legível é **"cada volta tem uma regra"**:

| Volta | Altura | Regra |
| --- | --- | --- |
| 1 | 0,5 → 4,0 | Sem poder: escada de livros, serra, livro que cede. |
| 2 | 4,5 → 12,8 | **Pulo duplo**: cada quina tem o coletável, e o livro seguinte está 1,7 m acima. Dois desses livros têm espinhos em ciclo. |
| 3 | 13,3 → 16,0 | Corredor fechado com **duas prensas**, escada de livros que cedem e **Pluma** para o salto final até a ilha da porta. |

## Geometria da espiral

- **Torre**: x −2..2, z −2..2. Quatro estantes sólidas de 0,6 m e 19 m de altura, com tampo a 19,5 m.
- **Pista**: quinas de 3 × 3 m centradas em (±3,5; ±3,5) e trechos do meio entre elas (4 m de comprimento): S em z −5..−2, L em x 2..5, N em z 2..5 e O em x −5..−2.

| Volta | SO | trecho S | SE | trecho L | NE | trecho N | NO | trecho O |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 0,5 | degraus 1,0 / 1,5 | 2,0 | 2,0 (serra) | 2,5 | 3,0 (cede) | 3,5 | 4,0 |
| 2 | 4,5 (Pulo duplo) | livro 6,2 (espinhos) | 7,1 (Pulo duplo) | livro 8,8 | 9,7 (Pulo duplo) | livro 11,4 (espinhos) | 12,3 | 12,8 |
| 3 | corredor 13,3 | corredor 13,3 (prensas) | corredor 13,3 | degraus que cedem 14,2 / 15,1 | 16,0 | 16,0 | 16,0 (Pluma) | — |

O início S0 tem topo em 0, centro (−3,5; 0; −8,5) e tamanho 6 × 5. A **ilha da porta** é um livro aberto em (−13,5; 16; 3,5), de 6 × 6 (x −16,5..−10,5, z 0,5..6,5). A porta fica em (−15; 16; 3,5) com yaw 90, voltada para o leste.

Detalhes das peças:

- **Livros da volta 2**: S e N medem 3 × 2 (x −1,5..1,5) e L mede 2 × 3 (z −1,5..1,5). Todos ficam a 0,5 m das quinas.
- **Corredor da volta 3**: x −5..5, z −3,6..−2. Tem 1,6 m livres entre a torre e o parapeito, uma estante de 3,4 m em z −3,9 que vai de x −5,3 a 5,3.
- **Degraus que cedem**: (3,5; 14,2; −1) e (3,5; 15,1; 1), de 3 × 1,6.
- **Armadilhas**:
  - Serra em (2,8; 2; 0), com `travel` (1,4; 0; 0) e período de 3 s.
  - Espinhos S: 1,0 s expostos e 2,2 s recolhidos. Espinhos N: 1,2 s expostos e 2,0 s recolhidos.
  - Prensas em (∓1,6; 13,3; −2,8): 1,6 s erguidas e 0,8 s embaixo, com offsets de 0 e 1,7 s.

Percurso: início 6 m, volta 1 28 m, volta 2 28 m, volta 3 ~24 m e salto final com a porta ~10 m. Total de **~96 m**, subindo 16 m.

## Contas de cada vão e degrau

Os números do motor estão em `docs/Fase8.md`. **Pulo duplo**: o pé chega a 2,2 m, ou ~2,48 m rolando na quina. **Pluma**: plano 6,63 m nominal.

| Salto | Vão | Desnível | Alcance / altura | Margem |
| --- | --- | --- | --- | --- |
| S0 → V1 SO | 1,0 | +0,5 | 2,88 | 1,88 |
| Degraus da volta 1 (SO → S1 → S2 → SE) | contínuo | +0,5 cada | apogeu 1,1 | 0,6 |
| SE → corredor da serra → NE | contínuo | 0 / +0,5 | — | 0,6 |
| NE → livro que cede → NO | 1,0 + 1,0 | +0,5 cada | 2,88 | 1,88 |
| NO → trecho O → V2 SO | contínuo | +0,5 cada | — | 0,6 |
| **Quina → livro (volta 2)** | 0,5 | **+1,7** | pulo duplo 2,2 | **0,5** de altura |
| Livro → quina (volta 2) | 0,5 | +0,9 | 2,37 | 1,87; 0,2 de altura |
| V2 NO → O → corredor | contínuo | +0,5 cada | — | 0,6 |
| Corredor → degrau que cede 1 → 2 → NE | 0,2 / 0,4 / 0,2 | +0,9 cada | 2,37 | ≥ 1,97; 0,2 de altura |
| **V3 NO → ilha com Pluma** | **5,5** | 0 | 6,63 | **1,13** |

No pulo duplo da volta 2, com o segundo pulo no apogeu, o pé fica acima de +1,7 m entre 0,44 s e 0,89 s (veja `docs/Fase6.md`). O livro tem 3 m de profundidade, então dá para pousar saindo de até ~1,5 m antes da borda. Nos degraus que cedem, quem passa da conta bate na face do degrau seguinte e cai de volta no atual: a escada se corrige sozinha.

### Atalhos indesejados (impossíveis)

- **Uma volta para a de cima**: na mesma célula, cada volta fica ao menos **3,0 m** acima da anterior. Nas células vizinhas entre voltas, a menor diferença também é 3,0 m (V2 NO 12,3 → V3 trecho N 15,0+; V2 O 12,8 → V3 NO 16,0). O pulo duplo alcança 2,48 m.
- **Uma quina para a seguinte na volta 2, pulando o livro de espinhos**: **+2,6 m**, mais que os 2,48 m do pulo duplo. Pousar no livro é obrigatório.
- **Pular o livro que cede da volta 1**: NE (2,5) → NO (3,5) são 4,0 m a +1,0, contra o limite de 2,71.
- **Corredor da volta 3 → NE sem os degraus**: +2,7 m, acima até do pulo duplo.
- **Prensas**:
  - O topo da prensa erguida fica a 13,3 + 2,85 = 16,15 m, e o pulo duplo leva o pé a 15,5. Não dá para saltar por cima.
  - O parapeito vai até 16,7 m. O corredor só se abre a oeste (para o trecho O da volta 2, pelo norte) e a leste (para os degraus).
  - Fora do parapeito não há piso. Contornar seria saltar sobre o vazio partindo do piso, que fica atrás do parapeito.
- **Ilha sem Pluma**: 5,5 m contra 4,16. A partir do corredor da volta 3, a diferença é +2,7 m; a partir do trecho O da volta 2, +3,2 m. Ambos impossíveis.
- **Serra**: saltar exigiria o pé acima de 1,63 m. Ela ocupa a pista entre a torre e o vazio.

### Alternativas aceitas

- Se o pulo duplo ainda estiver ativo na quina NO da volta 3, ele também vence os 5,5 m (6,63 nominal). É o mesmo salto final com outro poder, sem pular nenhum trecho.
- Pulo duplo e Pluma juntos só são possíveis na quina NO, onde a Pluma está. Com os dois (pé a 4,4 m), dá para subir no tampo da torre (19,5 m). É um beco sem saída e não encurta nada.

## Ritmo das armadilhas

- **Serra (volta 1)**: vai e volta entre x 2,8 e 4,2 a cada 3 s e fere a menos de 1,06 m.
  - Junto à torre (x 2,6), a janela livre é de **1,26 s** por ciclo. No lado de fora (x 4,4) é igual.
  - A faixa de perigo em Z tem 2,12 m, o que leva 0,42 s correndo. A dica é passar pelo lado oposto ao da serra.
- **Espinhos da volta 2**: S tem 2,2 s recolhidos; N, 2,0 s. O jogador vê, de baixo, as pontas surgindo na borda do livro.
  - O pulo duplo leva ~0,9 s no ar. Somando a travessia (~0,3 s) e o pulo para a quina, sobra ~1 s de folga.
  - As pontas do livro (0,6 m fora do gatilho) não servem de refúgio confiável.
- **Prensas (volta 3)**: ciclo de 3,4 s. Ferem de 1,6 s a 2,6 s e são seguras nos outros 2,4 s, inclusive na subida.
  - Passar por baixo cobre 2,16 m, o que leva 0,43 s.
  - Entre as duas há um refúgio de 1,6 m (x −0,8..0,8). Como as prensas estão meio ciclo defasadas, atravessa-se a oeste, espera-se no refúgio e atravessa-se a leste.
- **Coletáveis**: cada quina da volta 2 tem um Pulo duplo, e o efeito dura 30 s. Quem perde o poder cai ou volta à quina, onde ele reaparece. A Pluma fica na própria quina do salto final.

## Riscos e pendências

- As quedas levam à zona de queda (y −6) e reiniciam a fase. Não há piso de recuperação sob a espiral.
- A torre usa estantes de 19 m. Com o novo `Bookshelf` (fileiras compartilhadas) o custo deve ser aceitável, mas vale medir.
- O livro com espinhos mede 3 × 2 e o gatilho, 1,8 × 1,8. As pontas descobertas têm 0,6 m: dá para pisar, mas não para esperar. É preciso confirmar no *playtest*.
- As luminárias penduradas ficam fora da pista (x ou z ±5,4), sobre o vazio.
