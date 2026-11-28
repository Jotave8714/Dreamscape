# Notas da candidata — 25/11/2026

Estado: **candidata para revisão e teste no Windows da apresentação**. Esta versão não é uma entrega final aprovada.

- Os PRs #14, #15, #16, #17 e #18 foram aprovados e integrados na `develop`. A candidata parte de `ed34b9a` e inclui as correções DS-19, o gerador DS-22 atualizado e a nova Fase 18. O commit exato e os hashes de origem constam em `manifest.json`.
- O pacote com `scope: prototype` contém menu, duas fases e encerramento. O pacote com `scope: full` contém menu, 18 fases e encerramento. `scope: phase18` contém menu, nova fase e encerramento para teste direto. A seleção não altera a ordem de cenas salva no projeto.
- Foram corrigidos o estado do pulo após teleporte e a zona de queda durante a pausa. Os cinco casos que falhavam passaram na suíte de sete testes DS-19.
- A Fase 18 é um epílogo novo: margem azul segura, página falsa que cede e dois degraus que exigem pulo duplo. Evidências e limites em `docs/Fase18.md` e `docs/VALIDACAO_2026-11-25.md` no repositório.
- Os testes de percurso usam comandos do motor com física e contatos reais. As transições das outras fases são testadas pelas portas, com teleporte apenas para preparar o contato. Isso não representa playtest manual de todas as 18 fases.
- O usuário confirmou que a apresentação será em outro computador, **Windows 64 bits**, com acesso posterior. A abertura e o percurso nesse computador permanecem pendentes.
- O macOS é compilado para ARM64. O app não tem assinatura Developer ID nem notarização; distribuição em outros Macs não foi aprovada. Windows e Linux precisam de execução nativa em seus respectivos sistemas.
- A equipe ainda precisa concluir a revisão humana e os cards de QA/DS-20/DS-21 relacionados à entrega. Resultados automatizados não substituem a confirmação de legibilidade, percepção de áudio e clareza das pistas.
- Guardar a reserva em outro dispositivo antes da apresentação. As duas cópias locais têm SHA-256 verificado, mas não constituem uma reserva em dispositivo separado.
