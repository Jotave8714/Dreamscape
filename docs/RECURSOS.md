# Recursos adicionados ao protótipo

## Sons do DS-15

Os três arquivos foram sintetizados para o projeto Dreamscape, com código em Python e funções matemáticas. Não usam gravações, samples, bibliotecas de áudio, serviços pagos nem arquivos de terceiros.

| Arquivo | Uso | Duração | Síntese |
| --- | --- | --- | --- |
| `Assets/Audio/DS15/Coleta.wav` | Coleta da velocidade | 0,26 s | Duas notas ascendentes, 660 e 990 Hz |
| `Assets/Audio/DS15/Erro.wav` | Queda ou expiração sem acerto | 0,38 s | Variação descendente de 240 a 75 Hz |
| `Assets/Audio/DS15/Acerto.wav` | Chegada ao outro lado da ponte | 0,42 s | Três notas ascendentes, aproximadamente 523, 659 e 784 Hz |

Fonte: `Assets/Audio/DS15/Source~/generate_feedback.py`, incluída no repositório. WAV PCM mono, 16 bits, 44.100 Hz, com ataque e término suavizados. A geração é determinística e usa apenas a biblioteca padrão do Python.

Para regenerar:

```sh
python3 Assets/Audio/DS15/Source~/generate_feedback.py
```

A regeneração foi conferida por SHA-256: os três arquivos reproduzem exatamente os mesmos bytes. A pasta `Source~` mantém a fonte junto dos assets sem importá-la como recurso da Unity.

As cores do DS-15 são ajustes de materiais do próprio projeto; os originais DS-03 permanecem preservados. As licenças dos demais recursos, como a fonte Press Start 2P, continuam acompanhando seus respectivos arquivos.
