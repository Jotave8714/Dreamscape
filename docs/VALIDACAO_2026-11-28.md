# Primeira pessoa em toda a campanha — 28/11/2026

As Fases 1 (`DS07_Fase1_Jogavel`) e 2 (`DS09_Fase2_Jogavel`) ainda usavam `ThirdPersonCamera`. Ambas agora usam `FirstPersonCamera`, como as Fases 3 a 18. A câmera é filha do jogador, fica a 1,64 m dos pés em pé, acompanha o agachamento e tem plano próximo de 0,05 m. O movimento lateral não gira o corpo. O modelo do protagonista aparece somente nas sombras; a Moema continua visível.

O menu de montagem dos personagens usa a mesma configuração do prefab em primeira pessoa. O comando `Dreamscape > Converter fases para primeira pessoa` migra câmeras orbitais antigas nas cenas habilitadas do build sem reconstruir os cenários.

## Validação

Unity **6000.6.0f1**, macOS ARM64:

| Verificação | Resultado | Evidência local (ignorada pelo Git) |
| --- | --- | --- |
| EditMode | 38/38 aprovados | `Logs/first-person-editmode.xml` |
| PlayMode com interface gráfica | 60/60 aprovados | `Logs/first-person-playmode.xml` |
| Empacotador Python | 12/12 aprovados | `python3 -m unittest discover -s scripts/tests -v` |

Os testes de edição abrem cada uma das 18 fases e verificam câmera única em primeira pessoa, altura e orientação dos olhos, referência do controle, ausência de câmera orbital, AudioListener único e modelo sem obstrução da visão. A montagem repetida também é verificada, preservando a visibilidade da companheira.

O teste de execução das Fases 1 e 2 verifica posição dos olhos depois de agachar e teleportar, pausa sem movimento da câmera e restauração do cursor ao retomar. A suíte completa também cobre movimento, poderes, reinícios, transições e percursos já existentes.

Comandos usados no diretório do projeto (substitua `Unity` pelo executável 6000.6.0f1):

```sh
Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/first-person-editmode.xml -logFile Logs/first-person-editmode.log
Unity -projectPath . -runTests -testPlatform PlayMode -testResults Logs/first-person-playmode.xml -logFile Logs/first-person-playmode.log
python3 -m unittest discover -s scripts/tests -v
```

## Integração e limites

O solicitante autorizou integrar os PRs até a `main`, seguindo `feature → develop → homolog → main`. Essa autorização refere-se ao código; não registra teste realizado no computador Windows da apresentação.

O [QA-WIN](https://trello.com/c/A4xw82bp) permanece pendente. Os quatro ZIPs preparados em 25/11, descritos em `VALIDACAO_2026-11-25.md`, antecedem esta alteração de câmera e precisam ser regenerados para incluí-la. Nenhuma nova validação de executável Windows foi declarada.
