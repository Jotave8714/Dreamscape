# Fluxo de branches

## Branches permanentes

| Branch    | Papel                                                                 | Recebe merge de            |
|-----------|-----------------------------------------------------------------------|----------------------------|
| `main`    | Versão estável/entregue do jogo. Cada merge aqui gera uma tag (`vX.Y.Z`). | `homolog`, `hotfix/*`      |
| `homolog` | Versão candidata, usada para testes/playtest antes da entrega.        | `develop`, `hotfix/*`      |
| `develop` | Integração contínua do trabalho do dia a dia.                         | `feature/*`, `fix/*`, `chore/*`, `refactor/*`, `art/*` |

Nenhum commit vai direto em `main`, `homolog` ou `develop`: tudo entra via Pull Request.

## Branches temporárias

Criadas a partir de `develop` (exceto `hotfix/*`, criada a partir de `main`):

- `feature/<descricao>` — nova mecânica, cena, sistema (ex.: `feature/movimento-moema`)
- `fix/<descricao>` — correção de bug encontrado em `develop`
- `art/<descricao>` — modelos, texturas, animações, áudio
- `chore/<descricao>` — configuração, pacotes, settings do projeto
- `refactor/<descricao>` — reorganização de código sem mudar comportamento
- `hotfix/<descricao>` — correção urgente em produção (`main`)

Use nomes curtos, minúsculos e com hífen.

## Ciclo

```
feature/* ──PR──▶ develop ──PR──▶ homolog ──PR──▶ main ──▶ tag vX.Y.Z
                     ▲                              │
                     └──────── hotfix/* ◀───────────┘
```

1. `git switch develop && git pull`
2. `git switch -c feature/minha-feature`
3. Commits pequenos; abrir PR para `develop`.
4. Quando `develop` estiver estável para testes: PR `develop → homolog`.
5. Validado em `homolog`: PR `homolog → main` e criar a tag da versão.
6. Hotfix: branch de `main`, PR para `main` e depois para `homolog` e `develop`, mantendo as três sincronizadas.

## Dicas para Unity

- Evite que duas pessoas editem a mesma cena (`.unity`) ou prefab na mesma branch ao mesmo tempo — merges de YAML da Unity são frágeis.
- Sempre faça commit do `.meta` junto com o asset correspondente.
- Atualize sua branch com `develop` antes de abrir o PR (`git merge develop` ou `git rebase develop`).
