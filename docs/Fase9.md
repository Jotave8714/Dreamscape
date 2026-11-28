# Fase 9: Estantes Sem Fim

Cena: `Assets/Scenes/Fase9_EstantesSemFim.unity`, gerada por `Assets/Scripts/Editor/Phases/Fase9Builder.cs`. O tema é próprio (`Fase9Builder.Tema`): noite funda e esverdeada, saturação 4, exposição −0,05, neblina 0,028 e destaque **verde-esmeralda** (0,30; 1; 0,55), a cor dos abajures de leitura.

## Ideia central

Um labirinto de estantes de 5 m de altura. Ninguém sobe nelas e elas escondem o horizonte, só o relógio do fim aparece por cima. A regra é **ler o caminho pelas luzes**:

- Na quina de cada curva e bifurcação da rota certa há um abajur verde em cima da estante.
- A rota tem luminárias no tom do tema, uma célula sim, outra não.
- Os becos são **escuros**, e em três deles o chão é de papel e cede (`CrumblingPlatform`).

Ritmo: **apresentar** (primeira bifurcação, com beco escuro) → **testar** (espinhos em ciclo, serra cruzando o corredor, bifurcações enganosas) → **escolher** (laser ou espinhos) → **desafio final** (andar no topo de estantes sobre o vazio, com um pêndulo).

## O labirinto

A grade tem células de 3 m: a célula (i, j) ocupa x de 3i a 3i+3 e z de 3j a 3j+3. As estantes ficam sobre as linhas da grade e deixam corredores de 2,4 m. O código lista as **ligações** entre células e põe estante em toda aresta de célula usada sem ligação. As arestas colineares se juntam numa estante só. A entrada é a aresta sul de (3,0) e a saída, a aresta norte de (5,9).

Mapa das células usadas (norte em cima). `R` é a rota, `A` a rota alternativa, `n` os nichos da serra, `b` um beco firme e `p` o papel que cede.

```
j=9   .  .  A  A  A  R  p      saída ao norte de (5,9)
j=8   .  .  R  R  R* R  p      * laser em (4,8)
j=7   .  p  R  .  .  .  b
j=6   .  p  R  R  R  R  b
j=5   .  b  .  n  .  R  .
j=4   .  R  R  R  R  R  .      serra cruza (3,4) entre os nichos
j=3   .  R  .  n  .  p  .
j=2   .  R  .  .  .  p  .
j=1   .  R  R  R  b  b  .
j=0   .  .  .  R  .  .  .      entrada ao sul de (3,0)
     i=0  1  2  3  4  5  6
```

- **Rota principal**: (3,0) (3,1) (2,1) (1,1) (1,2) (1,3) (1,4) (2,4) (3,4) (4,4) (5,4) (5,5) (5,6) (4,6) (3,6) (2,6) (2,7) (2,8) (3,8) (4,8) (5,8) (5,9). São 21 passagens, ~63 m.
- **Rota alternativa**: (2,8) (2,9) (3,9) (4,9) (5,9). Tem o mesmo comprimento (4 passagens) e troca o laser por dois espinhos em ciclo. As duas rotas são iluminadas.
- **Becos**:
  - B1 sai de (3,1) para leste. É a primeira bifurcação e ensina a regra: o beco fica escuro e o papel cede em (5,2) e (5,3).
  - B2 sai de (1,4) e segue reto para o norte; cede em (1,6) e (1,7).
  - B3 sai de (5,6) para leste e depois norte, parecendo ir direto para a saída; cede em (6,8) e (6,9).

## Sequência e percurso

1. **Vestíbulo** (x 6..15, z −8..0): *spawn* em (10,5; 0; −5,5), placas `FASE 9`, `ESTANTES SEM FIM` e `siga os abajures verdes`, e o portal de estantes na boca do labirinto.
2. **Primeira bifurcação** em (3,1): o abajur marca o oeste; o leste (B1) é escuro.
3. **Espinhos em ciclo** no corredor (1,1)→(1,4).
4. **Bifurcação** em (1,4): o abajur marca o leste; o norte (B2) é escuro.
5. **Serra** cruzando (3,4), com uma luz de alerta avermelhada em cima.
6. **Bifurcação** em (5,6): o abajur marca o oeste; o leste (B3) é escuro e enganoso.
7. **Escolha** em (2,8): laser pela fileira 8 ou espinhos pela fileira 9.
8. **Varanda E1**, com o Escudo opcional.
9. **Ponte 1**: topo de estante (0,6 m de largura) a y 0, de z 35 a 42.
10. **Ponte 2**: y 0,5, de z 44 a 50, com o pêndulo em z 47.
11. **Patamar da porta E2**: livro aberto, porta voltada para o sul.

Percurso: vestíbulo 5,5 m + labirinto ~64 m + saída ~28 m. Total de **~98 m**, igual pelas duas rotas.

## Peças (topo em Y)

| Peça | Centro do topo | Tamanho / extensão |
| --- | --- | --- |
| F0 Vestíbulo | (10,5, 0, −4) | 9 × 8, x 6..15, z −8..0 |
| Piso de cada célula | (3i+1,5, 0, 3j+1,5) | 3 × 3 (`BookPlatform`; papel nos becos marcados) |
| E1 Varanda | (16,5, 0, 32,5) | 5 × 5, x 14..19, z 30..35 |
| Ponte 1 (topo de estante) | x 16,5, topo 0 | 0,6 × 7, z 35..42 |
| Ponte 2 (topo de estante) | x 16,5, topo 0,5 | 0,6 × 6, z 44..50 |
| E2 Patamar da porta | (16,5, 0,5, 56) | 8 × 8, z 52..60 |

As estantes do labirinto e do vestíbulo têm **5 m**. As armadilhas são:

| Armadilha | Posição | Configuração |
| --- | --- | --- |
| Espinhos em ciclo A e B | (4,5, 0, 7,5) e (4,5, 0, 10,5) | 1,2 s expostos e 1,6 s recolhidos; B com offset 1,4 |
| Serra | sai de (10,5, 0, 10,5) | `travel` (0, 0, 6), período 4 s |
| Laser | (13,5, 0, 25,5) | yaw 90; 1,5 s ligado e 1,8 s desligado |
| Espinhos da rota alternativa | (10,5, 0, 28,5) e (13,5, 0, 28,5) | mesmo ciclo dos primeiros; B com offset 1,4 |
| Pêndulo | (16,5, 0,5, 47) | yaw 0, 50°, 2,8 s |

O Escudo fica em (15, 0, 33,5) e a porta em (16,5, 0,5, 58,5), com yaw 180. A zona de queda fica em y = −8.

## Contas

Usam os números do motor de `docs/Fase8.md`: alcance nominal plano 3,32 e +0,5 2,88; limite teórico plano 4,16.

| Salto | Vão | Desnível | Alcance | Margem |
| --- | --- | --- | --- | --- |
| Vestíbulo → labirinto → varanda | contínuo | 0 | — | — |
| E1 → Ponte 1 | contínuo | 0 | — | — |
| **Ponte 1 → Ponte 2** | **2,0** | +0,5 | 2,88 | 0,88 em Z; ±0,3 m de mira lateral (±0,58 m contando a borda da cápsula) |
| Ponte 2 → E2 | 2,0 | 0 | 3,32 | 1,32 |

### Atalhos indesejados (impossíveis)

- **Por cima das estantes**: as estantes têm 5 m, e a fase não tem poder de altura. O pé alcança 1,38 m no máximo.
- **Pular os espinhos**: o corredor tem 2,4 m livres e o gatilho cobre 1,8 m. Sobram 0,3 m de cada lado, menos que os 0,56 m do corpo. Saltar por cima exige 2,36 m com o pé acima de 0,65 m, e o pulo só dá 2,12 m.
- **Contornar o laser**: o feixe tem 2,7 m e o corredor, 2,4 m.
- **Saltar o laser**: o pé chega a 1,1 m, abaixo do feixe (0,825 a 1,175 m).
- **E1 → Ponte 2** (9 m), **Ponte 1 → E2** (10 m) e **E1 → E2** (17 m): todos impossíveis.
- **Contornar o pêndulo**: a ponte tem 0,6 m de largura, e a massa varre ±2,93 m (alcance mais o corpo).

## Ritmo das armadilhas

- **Espinhos em ciclo**: período de 2,8 s, com B meio ciclo atrasado.
  - O refúgio entre os gatilhos vai de z 8,4 a 9,6 (1,2 m).
  - Atravessar um campo cobre 2,36 m, o que leva 0,47 s, contra 1,6 s recolhido.
- **Serra**: vai de z 10,5 a 16,5 e cruza a rota (z 12..15) duas vezes por ciclo de 4 s.
  - Fere a menos de 1,06 m. Em qualquer faixa da rota, o perigo dura ~0,5 s por passagem e há **~1,5 s livres** entre passagens.
  - A faixa de perigo em X tem 2,12 m, o que leva 0,42 s correndo.
  - Parada nos nichos, a serra fica a 2,08 m da faixa mais próxima da rota. Por isso a espera em (2,4) é segura.
- **Pêndulo**: pelo centro da ponte, fere em 0,365 s por passagem e deixa **1,04 s livres**. A faixa de perigo tem 1,8 m, o que leva 0,36 s correndo.
  - O ideal é esperar na Ponte 2, logo antes de z 45,6, e passar assim que a massa cruzar o centro.
  - O Escudo da varanda absorve um toque.

## Riscos e pendências

- A ponte de 0,6 m depende de o `CharacterController` aceitar ficar sobre uma borda estreita. Se for difícil demais no *playtest*, troque as pontes por `BookPlatform` de 1,0 m.
- Os abajures ficam no topo das estantes, a 5 m, e iluminam o corredor de cima (alcance 6 m). Vale conferir se leem bem através da neblina.
- O labirinto tem ~25 luzes pontuais entre luminárias e abajures. É preciso verificar o desempenho no URP.
- A serra usa yaw 0 (veja as observações de `docs/Fase6.md` sobre o giro do modelo).
- As células não usadas não têm piso. Se o jogador conseguir enxergar por uma fresta entre estantes, verá o vazio.
