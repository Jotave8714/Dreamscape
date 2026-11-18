# Biblioteca do Sonho — direção de arte e regras das fases

O estudante adormece estudando na biblioteca da faculdade (o pufe do checkpoint é onde ele cochilou). Todo o jogo acontece no sonho dele: a escola se mistura com uma **biblioteca infinita e mágica**. Corredores viram estantes altíssimas, livros gigantes viram plataformas, páginas e livros voam devagar, abajures verdes de leitura iluminam o caminho e relógios parados flutuam no horizonte.

## Sensação

- **Sonho:** escala impossível (estantes de 8 a 14 m que somem na neblina), objetos flutuando, poeira luminosa (`DreamMotes`), névoa colorida, silêncio.
- **Biblioteca:** madeira escura, lombadas coloridas (vinho, marinho, verde, ocre, ameixa, petróleo, creme), latão, papel, tapetes vermelhos e luz quente de leitura contra a luz fria do tema.
- **Leitura clara do caminho:** o que é chão é sólido e iluminado; o cenário (estantes de fundo, livros voando) fica fora do caminho e sem colisão. A cor de destaque do tema (`Accent`, nos frisos) marca a rota.

## Arco das fases

| Fase | Nome | Papel |
| --- | --- | --- |
| 1–2 | Fases feitas à mão (DS07, DS09) | Tutorial. Só decoração de biblioteca, sem mexer na jogabilidade (há testes das rotas e da ponte). |
| 3 | Corredor das Luzes | Corredor de estantes; "siga as luzes". |
| 4 | Bloco de Aulas | Salas de aula que viram salas de leitura. |
| 5 | Laboratório | Laboratório de alquimia/pesquisa dentro da biblioteca. |
| 6 | Oficina das Serras | Oficina de encadernação: serras, prensas de livros. |
| 7 | Falso Despertar | Parece o fim (luz de manhã), mas o sonho continua. |
| 8 | Saguão dos Sussurros | Hall de entrada da biblioteca mágica. Reapresenta as mecânicas com calma. |
| 9 | Estantes Sem Fim | Labirinto de estantes; ler o caminho pelas luzes. |
| 10 | Escadaria de Livros | Subida vertical por pilhas de livros. |
| 11 | Sala de Leitura Flutuante | Ilhas de mesas e livros abertos no vazio. |
| 12 | Arquivo das Lembranças | Arquivo com prensas e pisos que cedem. |
| 13 | Ala Proibida | Lasers e pêndulos; atmosfera roxa intensa. |
| 14 | Torre do Relógio | Ritmo: pêndulos e ciclos sincronizados. |
| 15 | Mar de Páginas | Grandes vãos com Pluma e Velocidade sobre um mar de páginas. |
| 16 | Observatório dos Sonhos | Plataformas altas sob um céu estrelado. |
| 17 | Despertar | O despertar de verdade: a fase mais clara e colorida, junta tudo. |

**Paleta:** as Fases 1–7 seguem a progressão do GDD (escuro → colorido). As Fases 8–17 são um segundo arco, "a noite na biblioteca até o amanhecer": a 8 começa noturna e quente (saturação perto de 0, exposição −0,1), e cada fase ganha um pouco de saturação e exposição até a 17 (saturação ~30, exposição ~0,3, neblina clara).

## Regras de jogabilidade (valem para todas as fases geradas)

- Limites do jogador (`CharacterMotor`): gravidade −20 m/s², pulo 1,1 m, corrida 5 m/s (7,5 com Velocidade), controlador de raio 0,28 m, *coyote time* 0,12 s. **Vão seguro sem poder: até 2,8 m no plano. Degrau seguro: até 0,9 m.** Com Velocidade, ~5 m; com Pluma, ~6,6 m no plano; o pulo duplo soma ~1,1 m de altura.
- Todo poder exigido é coletado **antes** do trecho que o exige, e o trecho seguinte não pode ficar impossível se o efeito (30 s) acabar no meio: o coletável reaparece depois que o efeito termina, então coloque-o perto do trecho.
- Nada aleatório: ciclos fixos, mesma geometria a cada tentativa.
- Sempre `Spawn`, `Goal`, `FallZone` e pelo menos uma placa com o nome da fase (`Sign`).
- Cenário sem colisão (`BackdropShelf`, `FloatingBooks`, `FloatingPages`, `DreamClock`, `ReadingLamp`, `LibraryLadder`, `Carpet`, `DreamMotes`) **nunca** deve ficar dentro do volume em que o jogador anda ou pula: deixe ao menos 1,5 m de folga lateral e 2,5 m acima do caminho.
- Estantes sólidas (`Bookshelf`, `ShelfArch`) funcionam como paredes (0,6 m de espessura).
- Cada fase tem um documento `docs/FaseN.md` com objetivo, mecânicas, tabela de plataformas e as contas dos vãos (veja `docs/Fase3.md`).

## Peças do kit (`PhaseKit` + `PhaseKitLibrary.cs`)

Peças de jogo (já existiam): `Platform`, `Wall`, `Pillar`, `Ramp`, `Pickup`, `Spikes`, `Saw`, `Pendulum`, `Laser`, `Press`, `CrumblingPlatform`, `Spawn`, `Goal`, `FallZone`, `Pole`, `CeilingLight`, `Sign`.

Peças da biblioteca:

| Peça | Colisão | Uso |
| --- | --- | --- |
| `Bookshelf(name, from, to, height=3, solid=true)` | caixa (parede) | Paredes de corredor, divisórias de labirinto. |
| `BackdropShelf(name, from, to, height=8)` | não | Estantes gigantes de fundo. |
| `LibraryBackdrop(min, max, baseY=-4, height=12, margin=6)` | não | Moldura de estantes ao redor da fase inteira, mais partículas. Chame no fim do `Build`. |
| `BookPlatform(name, topCenter, size, yaw=0, stackDepth=1.2)` | caixa 0,5 m | Mesmo contrato de `Platform`, feito de livros gigantes empilhados. |
| `OpenBookPlatform(name, topCenter, size, yaw=0)` | caixa 0,5 m | Livro aberto gigante; bom para patamares de chegada. |
| `ReadingDesk(name, groundPos, yaw=0)` | caixa 1,6 × 0,8 × 0,8 | Mesa com vela; obstáculo ou degrau de 0,8 m. |
| `ShelfArch(name, groundCenter, width=2.4, height=3.6, yaw=0)` | laterais | Portal de estantes emoldurando uma passagem. |
| `FloatingBooks(name, center, radius=3, count=6)` | não | Livros voando. |
| `FloatingPages(name, center, radius=2.5, count=10)` | não | Páginas brilhando no ar. |
| `DreamClock(name, center, radius=1.6, yaw=0, spin=4)` | não | Relógio flutuante; com yaw 0 o mostrador olha para −Z. |
| `ReadingLamp(groundPos, yaw=0, lightColor=null)` | não | Abajur verde com luz quente. |
| `LibraryLadder(groundPos, height=3, yaw=0)` | não | Escada encostada numa estante. |
| `Carpet(topCenter, size, yaw=0)` | não | Tapete sobre uma superfície. |
| `DreamMotes(name, center, size, color=null, maxParticles=160)` | não | Poeira luminosa. |

As Fases 1 e 2 são decoradas com `PhaseKit.Dress(scene, theme)`, que cria tudo sob o objeto `Cenario biblioteca` e pode rodar de novo sem duplicar.

Para gerar uma fase: menu **Dreamscape > Fases > Gerar todas as fases novas**, ou `PhaseBuildMenu.BuildPhase("NomeDaCena")`. As fases entram no build antes do `PrototypeEnding`.
