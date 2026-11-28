# Dreamscape — Instruções de execução

Este pacote é uma candidata para revisão. Consulte `NOTAS_DA_VERSAO.md` para verificar as limitações e o estado dos testes desta versão.

## Windows x64

Extraia o ZIP inteiro e abra `Dreamscape.exe`. Mantenha `Dreamscape_Data`, `UnityPlayer.dll` e as demais pastas ao lado do executável.

## Linux x64

Extraia o arquivo `.tar.gz` preservando as permissões e execute `./Dreamscape.x86_64`. Mantenha `Dreamscape_Data`, `UnityPlayer.so` e as demais pastas ao lado do executável. É necessário um ambiente gráfico com suporte à API de renderização utilizada pela Unity.

## macOS ARM64

Extraia o ZIP inteiro e abra `Dreamscape.app`. A versão local validada usa macOS ARM64 (Apple Silicon). O aplicativo não foi assinado com Developer ID nem notarizado para distribuição; a abertura em outros computadores ainda precisa ser verificada.

O jogo inicia pelo menu. Novo Jogo começa na Fase 1; Continuar retoma a entrada da fase salva. O campo `scope` em `manifest.json` identifica o pacote: `prototype` contém as Fases 1 e 2 e termina após a segunda; `full` contém a campanha de 18 fases e termina após o Jardim das Entrelinhas; `phase18` abre diretamente o jardim ao escolher Novo Jogo, para testar a nova fase sem percorrer as 17 anteriores. Jogar Novamente inicia uma partida com os poderes no estado inicial. Os poderes duram 30 segundos e seus coletáveis reaparecem ao expirar.

| Comando | Ação |
| --- | --- |
| WASD | Andar |
| Mouse | Controlar a câmera |
| Shift | Correr |
| C | Agachar ou levantar |
| Espaço | Pular |
| Esc | Abrir ou fechar a pausa |

Para conferir a integridade, use o checksum SHA-256 que acompanha o pacote. No macOS: `shasum -a 256 -c ARQUIVO.sha256`. No Linux: `sha256sum -c ARQUIVO.sha256`. No PowerShell: `Get-FileHash .\ARQUIVO.zip -Algorithm SHA256` e compare o valor com o arquivo `.sha256`. O pacote inclui `manifest.json`, com versão, commit e identificação dos arquivos utilizados na compilação.

A reserva contém o mesmo arquivo e checksum da candidata. Guarde-a em outro dispositivo antes da apresentação, após confirmar o funcionamento do percurso completo em cada sistema.
