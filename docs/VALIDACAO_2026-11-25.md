# Validação — DS-19, DS-22 e Fase 18

Base: `develop` em `ed34b9a`. Branch: `feature/ds19-ds22-jardim-entrelinhas`. Unity 6000.6.0f1, macOS ARM64. A apresentação será em outro computador Windows x64, com acesso posterior, conforme confirmação do usuário.

## Resultados registrados

| Verificação | Resultado | Evidência local |
| --- | --- | --- |
| Reprodução dos defeitos DS-19 na base atual | 2/7 aprovados; cinco falhas reproduzidas | `Logs/DS19/before-current.xml` |
| Correção DS-19 | 7/7 aprovados | `Logs/DS19/after-current.xml` |
| PlayMode com interface gráfica | 59/59 aprovados | `Logs/Fase18/playmode-final.xml` |
| EditMode: busca de rotas e seleção de cenas | 19/19 aprovados | `Logs/Fase18/editmode-final.xml` |
| Percursos e transições após acrescentar dois testes de player | 8/8 aprovados | `Logs/Fase18/player-routes-editor.xml` |
| Player macOS fora do Editor | 15/15 aprovados, 0 ignorados | `Logs/DS22/native-player.xml` |
| Empacotador Python | 12/12 aprovados | `python3 -m unittest discover -s scripts/tests -v` |

A primeira execução PlayMode, sem dispositivo gráfico, passou em 55/57: falharam mouse e captura do cursor. Os dois cenários passaram na repetição com Game View gráfica, junto com toda a suíte. Nenhum teste foi ignorado para obter aprovação. A execução nativa macOS levou 36,3 s de testes, em um player de desenvolvimento com as mesmas cenas e scripts do jogo; esse executável de testes não é o pacote distribuível.

## O que os testes verificam

- Cinco defeitos DS-19: pedido de pulo antigo após teleporte, contato/coyote time de chão antigo, orçamento de pulo extra consumido e dois problemas na zona de queda (colisor sólido e contato durante a pausa). Testes positivos preservam a aterrissagem e rejeitam personagens sem controle de jogador.
- As Fases 1 e 2 são percorridas por movimento, pulos e contatos físicos, passando pelos coletáveis e portais reais. Não se teleporta para aprovar esses percursos.
- A Fase 18 é percorrida do spawn à porta a 30, 60 e 120 FPS simulados. Há testes separados para cinco quedas e restauração, expiração/recoleta do pulo duplo e integração com a Fase 17 e o encerramento.
- As 18 transições de cena têm limpeza de pulo duplo, velocidade e Pluma e atualização de checkpoint verificadas. Nesse teste de integração usa-se teleporte até a porta; não se verifica a rota jogável de cada fase intermediária.
- O empacotador verifica runtime completo, relatório com plataforma/versão/escopo compatíveis, conteúdo distribuído, exclusão de diagnósticos, preservação de permissões e links e igualdade SHA-256 da reserva.

## Limites de aceite

Não houve playtest humano cego, avaliação auditiva da equipe nem execução no Windows da apresentação. Os resultados de passos simulados não são medições de FPS renderizado. DS-19 recebeu aceite expresso do solicitante; Com autorização expressa do solicitante, o DS-22 foi encerrado como preparação da entrega. A abertura e o percurso no computador da apresentação, as confirmações humanas das dependências DS-20/DS-21 e a reserva em outro dispositivo foram transferidos ao card QA-WIN: https://trello.com/c/A4xw82bp. Esses testes permanecem pendentes; não foram declarados como executados.

## Roteiro para o Windows da apresentação

1. Extrair o ZIP inteiro. Comparar `Get-FileHash .\Dreamscape-ARQUIVO.zip -Algorithm SHA256` com o `.sha256` correspondente.
2. Abrir `Dreamscape.exe`, conferir menu, controles, legibilidade, áudio e pausa. Iniciar Novo Jogo.
3. Na Fase 1, cair intencionalmente; conferir spawn, poder e chão restaurados. Coletar pulo duplo e concluir a rota elevada. Confirmar que a Fase 2 começa sem esse poder.
4. Na Fase 2, falhar pela rota direta; repetir pela pista lateral, coletar velocidade e cruzar a ponte. No pacote `prototype`, conferir encerramento. No `full`, conferir entrada na Fase 3 sem velocidade.
5. No encerramento do protótipo, testar Menu Principal e Jogar Novamente. Conferir que não existe poder residual nem opção Continuar sem novo progresso.
6. Na campanha completa, jogar a Fase 18 quando o progresso chegar a ela: pista azul à esquerda, página falsa, dois degraus com pulo duplo, última porta e encerramento.
7. Registrar versão, hardware, resultado, problemas e nome do revisor. Copiar o pacote e o checksum para outro dispositivo e verificar a cópia.

A Fase 18 também pode ser aberta diretamente no Editor pela cena `Assets/Scenes/Fase18_JardimDasEntrelinhas.unity`; isso não substitui o aceite do executável no Windows.

## Sincronização dos cards implementados nos PRs

DS-14 e DS-15 foram encerrados com base nas aprovações formais dos PRs #17/#18 e nas regressões atuais. Em 25/11 o usuário confirmou que não há exigência acadêmica adicional ao GDD. DS-19 foi encerrado com aceite expresso do solicitante; DS-22 foi encerrado como preparação após o usuário autorizar o desmembramento do teste Windows para o card QA-WIN. O novo trabalho está no PR #49, em rascunho.

A demonstração macOS ARM64 0.2.0-preview.18 abriu nativamente. O player registra avisos URP de shaders de profundidade de campo e Panini; esses passes de pós-processamento não executam. Abertura nativa não equivale a playtest manual. A inspeção preservou e restaurou o progresso anterior.

## Pacotes verificados

Todos compilados com zero erros e dois avisos. ZIPs passaram na verificação CRC; cada reserva é idêntica por SHA-256. Arquiteturas conferidas: PE x86-64 no Windows e Mach-O ARM64 no macOS. Nenhuma assembly de testes ou NUnit foi distribuída. Origem do código: `acafd94d3f7c726bcdbeaf10f99c3e85fcb6bc38`.

| Pacote | Escopo | Cenas | SHA-256 |
| --- | --- | --- | --- |
| `Dreamscape-0.2.0-preview.18-macOS-ARM64.zip` | `phase18` | 3 | `0289e3f2005093ce7783a8d7245eef8ba0835c1bc7650cb2df31b07161e97b95` |
| `Dreamscape-0.2.0-rc.1-Windows-x64.zip` | `full` | 20 | `6c564b4342b8586b4ab7cfbf62e690667f0ea220451b5de1b0c9f548de05248f` |
| `Dreamscape-0.1.0-rc.1-Windows-x64.zip` | `prototype` | 4 | `3a593d3fc143d330287afb90e14fd665a7f9c5dcdff864fba0abcff6bde29234` |
| `Dreamscape-0.2.0-preview.18-Windows-x64.zip` | `phase18` | 3 | `6505d8acf82aca055ede4b26fe6d5ecf214a11f9712aecf7d74df6ac1b229535` |

Candidatas em `Builds/DS22/Candidates/` e `Builds/Fase18/Candidates/`; reservas nas pastas `Backup/` correspondentes. Verificação detalhada: `Logs/DS22/package-validation-current.json`. A demonstração inicia diretamente a Fase 18 pelo botão Novo Jogo. O protótipo preserva a entrega original com duas fases.

## Aceite e disponibilização

O solicitante autorizou expressamente “Encerrar preparação e separar teste Windows”. O DS-22 foi renomeado para preparação da candidata e pacote de entrega, seu checklist foi ajustado ao escopo aceito e o card foi concluído. O teste no computador da apresentação segue em [QA-WIN](https://trello.com/c/A4xw82bp), com checklist próprio e sem revisor ou data de acesso inventados. DS-14, DS-15, DS-19 e DS-22 estão concluídos; os demais cards de Nelson já estavam concluídos.

Os quatro arquivos foram enviados ao [rascunho de release](https://github.com/Jotave8714/Dreamscape/releases/tag/untagged-7b7ba783f2e8eb37c339). Os digests SHA-256 retornados pelo GitHub coincidem com os arquivos locais. A release permanece em rascunho; não houve publicação de entrega final. PR de código e documentação: [#49](https://github.com/Jotave8714/Dreamscape/pull/49).
