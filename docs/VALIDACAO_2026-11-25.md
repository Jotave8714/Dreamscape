# Validação — DS-19, DS-22 e Fase 18

Base: `develop` em `ed34b9a`. Branch: `feature/ds19-ds22-jardim-entrelinhas`. Unity 6000.6.0f1, macOS ARM64. A apresentação será em outro computador Windows x64, com acesso posterior, conforme confirmação do usuário.

## Resultados registrados

| Verificação | Resultado | Evidência local |
| --- | --- | --- |
| Reprodução dos defeitos DS-19 na base atual | 2/7 aprovados; cinco falhas reproduzidas | `Logs/DS19/before-current.xml` |
| Correção DS-19 | 7/7 aprovados | `Logs/DS19/after-current.xml` |
| PlayMode com interface gráfica | 57/57 aprovados | `Logs/Fase18/playmode-graphics.xml` |
| EditMode: busca de rotas e seleção de cenas | 18/18 aprovados | `Logs/Fase18/editmode.xml` |
| Percursos e transições após acrescentar dois testes de player | 8/8 aprovados | `Logs/Fase18/player-routes-editor.xml` |
| Empacotador Python | 12/12 aprovados | `python3 -m unittest discover -s scripts/tests -v` |

A primeira execução PlayMode, sem dispositivo gráfico, passou em 55/57: falharam mouse e captura do cursor. Os dois cenários passaram na repetição com Game View gráfica, junto com toda a suíte. Nenhum teste foi ignorado para obter aprovação.

## O que os testes verificam

- Cinco defeitos DS-19: pedido de pulo antigo após teleporte, contato/coyote time de chão antigo, orçamento de pulo extra consumido e dois problemas na zona de queda (colisor sólido e contato durante a pausa). Testes positivos preservam a aterrissagem e rejeitam personagens sem controle de jogador.
- As Fases 1 e 2 são percorridas por movimento, pulos e contatos físicos, passando pelos coletáveis e portais reais. Não se teleporta para aprovar esses percursos.
- A Fase 18 é percorrida do spawn à porta a 30, 60 e 120 FPS simulados. Há testes separados para cinco quedas e restauração, expiração/recoleta do pulo duplo e integração com a Fase 17 e o encerramento.
- As 18 transições de cena têm limpeza de pulo duplo, velocidade e Pluma e atualização de checkpoint verificadas. Nesse teste de integração usa-se teleporte até a porta; não se verifica a rota jogável de cada fase intermediária.
- O empacotador verifica runtime completo, relatório com plataforma/versão/escopo compatíveis, conteúdo distribuído, exclusão de diagnósticos, preservação de permissões e links e igualdade SHA-256 da reserva.

## Limites de aceite

Não houve playtest humano cego, avaliação auditiva da equipe nem execução no Windows da apresentação. Os resultados de passos simulados não são medições de FPS renderizado. DS-19 exige revisão cruzada; DS-22 exige abertura e percurso no computador da apresentação e confirmação das dependências DS-20/DS-21. Esses requisitos permanecem explícitos no Trello, sem marcar trabalho não verificado como concluído.

## Roteiro para o Windows da apresentação

1. Extrair o ZIP inteiro. Comparar `Get-FileHash .\Dreamscape-ARQUIVO.zip -Algorithm SHA256` com o `.sha256` correspondente.
2. Abrir `Dreamscape.exe`, conferir menu, controles, legibilidade, áudio e pausa. Iniciar Novo Jogo.
3. Na Fase 1, cair intencionalmente; conferir spawn, poder e chão restaurados. Coletar pulo duplo e concluir a rota elevada. Confirmar que a Fase 2 começa sem esse poder.
4. Na Fase 2, falhar pela rota direta; repetir pela pista lateral, coletar velocidade e cruzar a ponte. No pacote `prototype`, conferir encerramento. No `full`, conferir entrada na Fase 3 sem velocidade.
5. No encerramento do protótipo, testar Menu Principal e Jogar Novamente. Conferir que não existe poder residual nem opção Continuar sem novo progresso.
6. Na campanha completa, jogar a Fase 18 quando o progresso chegar a ela: pista azul à esquerda, página falsa, dois degraus com pulo duplo, última porta e encerramento.
7. Registrar versão, hardware, resultado, problemas e nome do revisor. Copiar o pacote e o checksum para outro dispositivo e verificar a cópia.

A Fase 18 também pode ser aberta diretamente no Editor pela cena `Assets/Scenes/Fase18_JardimDasEntrelinhas.unity`; isso não substitui o aceite do executável no Windows.
