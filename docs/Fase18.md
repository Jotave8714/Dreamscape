# Fase 18 — Jardim das Entrelinhas

Epílogo da Biblioteca do Sonho. A porta da Fase 17 leva ao jardim; a porta do jardim abre `PrototypeEnding`. O encerramento permanece no fim da campanha. A build do protótipo de duas fases permanece independente (`--scope prototype`).

## Regra e percurso

A placa ATALHO aponta para uma página que cede 0,7 s depois de pisada. O caminho estável fica à esquerda, marcado por tapete e três luzes azuis. A solução é observar a margem do livro, contornar a página e coletar o pulo duplo na clareira. Dois degraus altos levam ao mirante; a passarela final é contínua até a porta.

Percurso: entrada → margem azul → clareira do pulo → degrau 1 → degrau 2 → mirante → passarela → última página → encerramento.

| Superfície | Centro do topo (x, y, z) | Tamanho x × z | Intervalo z |
| --- | --- | --- | --- |
| Entrada | (0, 0, 0) | 10 × 8 | −4..4 |
| Atalho falso | (0, 0, 9) | 3 × 10 | 4..14 |
| Margem segura | (−5, 0, 10) | 3 × 16 | 2..18 |
| Clareira | (0, 0, 19) | 13 × 6 | 16..22 |
| Degrau 1 | (0; 1,7; 26) | 5 × 4 | 24..28 |
| Degrau 2 | (0; 3,4; 32) | 5 × 4 | 30..34 |
| Mirante | (0; 3,4; 39) | 8 × 6 | 36..42 |
| Passarela | (0; 3,4; 46) | 3 × 8 | 42..50 |
| Última página | (0; 3,4; 54) | 10 × 8 | 50..58 |

Spawn (0, 0, −1), poder (0, 0, 19), porta (0; 3,4; 56), zona de queda y = −7. O pufe marca a entrada. O poder dura 30 s, reaparece ao expirar e é removido ao morrer ou mudar de fase.

## Saltos e recuperação

Os dois primeiros saltos têm vão de 2 m e subida de 1,7 m. O salto básico sobe 1,1 m (até 1,4 m considerando o degrau de 0,3 m); portanto, o pulo duplo é necessário. O terceiro salto tem vão de 2 m e altura constante: alcance nominal correndo de aproximadamente 3,32 m, margem nominal de 1,32 m. Plataformas de 5 m de largura permitem corrigir o alinhamento.

O teste de percurso anda desde o spawn, coleta pelo contato físico, pede os dois pulos em frames diferentes e chega à porta sem teleporte. Os testes de falha usam teleporte somente para preparar a queda; não são evidência de conclusão do percurso. A regra do piso falso e as pistas se repetem depois de cada morte. A janela de 0,7 s é fixa; apenas o tremor visual do componente existente varia.

## Construção e evidências

Unity 6000.6.0f1. Menu `Dreamscape > Fases > Gerar Fase 18 - Jardim das Entrelinhas`, ou `-executeMethod Dreamscape.EditorTools.Fase18Builder.Generate`. O gerador recria somente a cena da Fase 18, inclui-a antes do encerramento e ordena as fases. `Fase18Builder.Preview` captura três vistas em `Temp/previews/`.

A fase reutiliza modelos próprios do kit de ambiente e as peças procedurais da biblioteca. Não adiciona recursos de terceiros. Estantes de fundo, páginas, livros e partículas ficam fora do volume de movimento. A cena utiliza nove fontes pontuais sem sombras e um sol direcional; desempenho na máquina Windows da apresentação ainda precisa ser medido.

Testes em `Assets/Tests/Player/GardenRouteTests.cs`: percurso a 30/60/120 FPS simulados, cinco mortes com restauração, expiração e recoleta do poder e transição Fase 17 → Fase 18 → encerramento → menu. Resultados e capturas em `Logs/Fase18/`. Os FPS são passos de simulação, não medições de desempenho gráfico.

A avaliação humana da clareza da pista e a execução no Windows da apresentação permanecem pendentes até o teste da equipe.
