"""Gera os três sons originais do DS-15, sem samples ou dependências externas."""
import math
from pathlib import Path
import struct
import wave

RATE = 44100
OUTPUT = Path(__file__).resolve().parents[1]


def write(name, notes):
    duration = max(start + length for start, length, _, _ in notes)
    samples = []
    for i in range(round(duration * RATE)):
        t = i / RATE
        value = 0.0
        for start, length, frequency, end_frequency in notes:
            local = t - start
            if not 0 <= local < length:
                continue
            envelope = min(1.0, local / 0.008, (length - local) / 0.04)
            phase = 2 * math.pi * (frequency * local + (end_frequency - frequency) * local**2 / (2 * length))
            value += 0.25 * envelope * (math.sin(phase) + 0.15 * math.sin(2 * phase))
        samples.append(round(max(-0.95, min(0.95, value)) * 32767))
    with wave.open(str(OUTPUT / name), "wb") as file:
        file.setnchannels(1)
        file.setsampwidth(2)
        file.setframerate(RATE)
        file.writeframes(struct.pack("<" + "h" * len(samples), *samples))


if __name__ == "__main__":
    write("Coleta.wav", [(0, 0.13, 660, 660), (0.09, 0.17, 990, 990)])
    write("Erro.wav", [(0, 0.38, 240, 75)])
    write("Acerto.wav", [(0, 0.14, 523.25, 523.25), (0.09, 0.14, 659.25, 659.25), (0.18, 0.24, 783.99, 783.99)])
