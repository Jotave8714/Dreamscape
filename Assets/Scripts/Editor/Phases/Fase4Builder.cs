using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 4 — "Bloco de Aulas" (tema âmbar). Mecânica central: pisos que cedem 0,8 s depois de pisados.
    ///
    /// Percurso em +Z (de z = 0 a z = 70,5), com um desvio curto para +X que guarda o power-up Velocidade:
    ///  1. Sala de aula (entrada, carteiras)            z  -2 .. 3
    ///  2. Travessia de 3 pisos que cedem (2,5 m cada)  z   3 .. 10,5  — andando cai, correndo passa
    ///  3. Corredor de espinhos em ciclo (onda)         z 10,5 .. 21
    ///  4. Sala de espera (bifurcação / truque)         z  21 .. 27
    ///  5. Sala lateral com Velocidade (desvio)         x   4 .. 7
    ///  6. Corredor de forro rebaixado: 3 placas de 5 m z 27 .. 47 — só passa com Velocidade
    ///  7. Patamar do corredor                          z  47 .. 51
    ///  8. Varanda do escudo (opcional, salto de 3,5 m) x -11,5 .. -7,5
    ///  9. Arquibancada de degraus que cedem            z 52,5 .. 58,5 (sobe 0,5 m por degrau)
    /// 10. Patamar do pêndulo (observar o ritmo)        z 58,5 .. 61,5 (y = 2)
    /// 11. Ponte do pêndulo                             z 61,5 .. 67,5
    /// 12. Saída (porta de conclusão)                   z 67,5 .. 71,5
    ///
    /// Truque de cenário: a sala de espera aponta direto para o corredor das placas longas. Sem Velocidade
    /// a primeira placa cede antes do fim (5,03 m a 5 m/s = 1,0 s &gt; 0,8 s) e o jogador cai — de forma justa:
    /// a placa treme ao ser pisada e o letreiro "ATRASADO? -&gt;" indica a sala lateral. O forro rebaixado
    /// (2,0 m) impede pular por cima das placas: só a velocidade horizontal resolve. Cálculos em docs/Fase4.md.
    /// </summary>
    public class Fase4Builder : IPhaseBuilder
    {
        public int Order => 4;
        public string SceneName => "Fase4_BlocoDeAulas";
        public PhaseTheme Theme => PhaseTheme.Fase4;

        // luz âmbar dos postes, combinando com o tema
        static readonly Color Ambar = new Color(1f, 0.8f, 0.45f);

        public void Build(PhaseKit kit)
        {
            Entrada(kit);
            TravessiaQueCede(kit);
            CorredorDeEspinhos(kit);
            SalaDeEspera(kit);
            CorredorDeForroBaixo(kit);
            PatamarEVarandaDoEscudo(kit);
            ArquibancadaQueCede(kit);
            PenduloESaida(kit);

            // abaixo de tudo: qualquer queda reinicia a fase
            kit.FallZone(-8f);
        }

        // ------------------------------------------------------------------ 1. sala de aula (entrada)

        static void Entrada(PhaseKit kit)
        {
            kit.Platform("Sala de aula - entrada", new Vector3(0f, 0f, 0.5f), new Vector2(8f, 5f));
            kit.Spawn(new Vector3(0f, 0f, 0f), 0f);

            kit.Wall("Parede - fundo da sala", new Vector3(-4f, 0f, -2f), new Vector3(4f, 0f, -2f));
            kit.Wall("Parede - sala esquerda", new Vector3(-4f, 0f, -2f), new Vector3(-4f, 0f, 3f));
            kit.Wall("Parede - sala direita", new Vector3(4f, 0f, -2f), new Vector3(4f, 0f, 3f));
            // frente com abertura de 3 m alinhada com a travessia
            kit.Wall("Parede - frente esquerda", new Vector3(-4f, 0f, 3f), new Vector3(-1.5f, 0f, 3f));
            kit.Wall("Parede - frente direita", new Vector3(1.5f, 0f, 3f), new Vector3(4f, 0f, 3f));

            // carteiras: pilares baixos em fileiras, deixando o corredor central livre
            float[] fileiras = { -0.6f, 1.6f };
            for (int i = 0; i < fileiras.Length; i++)
            {
                kit.Pillar($"Carteira esquerda {i + 1}", new Vector3(-2.7f, 0f, fileiras[i]), 0.75f);
                kit.Pillar($"Carteira direita {i + 1}", new Vector3(2.7f, 0f, fileiras[i]), 0.75f);
            }

            kit.CeilingLight(new Vector3(-2f, 3.2f, 0.5f));
            kit.CeilingLight(new Vector3(2f, 3.2f, 0.5f));

            kit.Sign("FASE 4 - BLOCO DE AULAS", new Vector3(0f, 2.6f, 2.8f), 0f, 0.07f);
            kit.Sign("PISO QUE TREME CAI - CORRA", new Vector3(0f, 2.05f, 2.8f), 0f, 0.03f);
        }

        // ------------------------------------------------------------------ 2. travessia de pisos que cedem

        static void TravessiaQueCede(PhaseKit kit)
        {
            // 3 placas de 3 x 2,5 m sobre o vazio. Cada uma cede 0,8 s depois de pisada:
            // correndo (5 m/s) fica no máximo 0,59 s em cada; andando (2,2 m/s) fica 1,08 s e cai.
            float[] centros = { 4.25f, 6.75f, 9.25f };
            for (int i = 0; i < centros.Length; i++)
                kit.CrumblingPlatform($"Piso que cede {i + 1}", new Vector3(0f, 0f, centros[i]), new Vector2(3f, 2.5f));

            // divisórias nas bordas: o jogador não consegue andar na beirada fora da área que detecta o passo
            kit.Wall("Divisoria - travessia esquerda", new Vector3(-1.5f, 0f, 3f), new Vector3(-1.5f, 0f, 10.5f), 1.4f);
            kit.Wall("Divisoria - travessia direita", new Vector3(1.5f, 0f, 3f), new Vector3(1.5f, 0f, 10.5f), 1.4f);
            // funil da travessia (3 m) para o corredor de espinhos (2,2 m)
            kit.Wall("Divisoria - funil esquerdo", new Vector3(-1.5f, 0f, 10.5f), new Vector3(-1.1f, 0f, 10.5f), 1.4f);
            kit.Wall("Divisoria - funil direito", new Vector3(1.1f, 0f, 10.5f), new Vector3(1.5f, 0f, 10.5f), 1.4f);

            kit.CeilingLight(new Vector3(0f, 3.2f, 6.75f));
        }

        // ------------------------------------------------------------------ 3. corredor de espinhos em ciclo

        static void CorredorDeEspinhos(PhaseKit kit)
        {
            // 2,2 m de largura: com as divisórias, não há faixa lateral livre dos espinhos (gatilho de 1,8 m)
            kit.Platform("Corredor de espinhos", new Vector3(0f, 0f, 15.75f), new Vector2(2.2f, 10.5f));
            kit.Wall("Divisoria - espinhos esquerda", new Vector3(-1.1f, 0f, 10.5f), new Vector3(-1.1f, 0f, 21f), 1.4f);
            kit.Wall("Divisoria - espinhos direita", new Vector3(1.1f, 0f, 10.5f), new Vector3(1.1f, 0f, 21f), 1.4f);

            // ciclo de 2,7 s (1,2 s expostos, 1,5 s recolhidos); a onda anda para frente 0,9 s por campo.
            // Entre os campos há refúgios seguros (z 14,4..15,85 e 17,65..19,1) para esperar a vez.
            kit.Spikes("Espinhos 1", new Vector3(0f, 0f, 13.5f), true, 1.2f, 1.5f, 1.8f);
            kit.Spikes("Espinhos 2", new Vector3(0f, 0f, 16.75f), true, 1.2f, 1.5f, 0.9f);
            kit.Spikes("Espinhos 3", new Vector3(0f, 0f, 20f), true, 1.2f, 1.5f, 0f);

            kit.CeilingLight(new Vector3(0f, 3.2f, 14.5f));
            kit.CeilingLight(new Vector3(0f, 3.2f, 18.5f));
        }

        // ------------------------------------------------------------------ 4/5. sala de espera e sala lateral

        static void SalaDeEspera(PhaseKit kit)
        {
            kit.Platform("Sala de espera", new Vector3(0f, 0f, 24f), new Vector2(8f, 6f));

            kit.Wall("Parede - espera fundo esquerda", new Vector3(-4f, 0f, 21f), new Vector3(-1.1f, 0f, 21f));
            kit.Wall("Parede - espera fundo direita", new Vector3(1.1f, 0f, 21f), new Vector3(4f, 0f, 21f));
            kit.Wall("Parede - espera esquerda", new Vector3(-4f, 0f, 21f), new Vector3(-4f, 0f, 27f));
            // frente: a abertura de 2,6 m é a boca do corredor de forro baixo (o caminho "óbvio")
            kit.Wall("Parede - espera frente esquerda", new Vector3(-4f, 0f, 27f), new Vector3(-1.3f, 0f, 27f));
            kit.Wall("Parede - espera frente direita", new Vector3(1.3f, 0f, 27f), new Vector3(4f, 0f, 27f));
            // lado direito: divisórias baixas nas pontas, abertura de 4 m para a sala lateral
            kit.Wall("Divisoria - espera direita fundo", new Vector3(4f, 0f, 21f), new Vector3(4f, 0f, 22f), 1.4f);
            kit.Wall("Divisoria - espera direita frente", new Vector3(4f, 0f, 26f), new Vector3(4f, 0f, 27f), 1.4f);

            float[] carteiras = { 22.5f, 24f, 25.5f };
            for (int i = 0; i < carteiras.Length; i++)
                kit.Pillar($"Carteira espera {i + 1}", new Vector3(-2.8f, 0f, carteiras[i]), 0.75f);

            kit.CeilingLight(new Vector3(-2f, 3.2f, 24f));
            kit.CeilingLight(new Vector3(2f, 3.2f, 24f));

            // dica do truque: aponta para a sala lateral (+X)
            kit.Sign("ATRASADO? ->", new Vector3(2.65f, 1.8f, 26.8f), 0f, 0.045f);

            // sala lateral (desvio curto) com o power-up Velocidade
            kit.Platform("Sala lateral", new Vector3(5.5f, 0f, 24f), new Vector2(3f, 4f));
            kit.Wall("Parede - lateral fundo", new Vector3(7f, 0f, 22f), new Vector3(7f, 0f, 26f));
            kit.Wall("Parede - lateral sul", new Vector3(4f, 0f, 22f), new Vector3(7f, 0f, 22f));
            kit.Wall("Parede - lateral norte", new Vector3(4f, 0f, 26f), new Vector3(7f, 0f, 26f));
            kit.Pickup(PickupKind.Speed, new Vector3(5.6f, 0f, 24f));
            kit.Pole(new Vector3(6.4f, 0f, 25.4f), Ambar);
        }

        // ------------------------------------------------------------------ 6. corredor de forro rebaixado

        static void CorredorDeForroBaixo(PhaseKit kit)
        {
            // 2,5 m de piso firme sob o forro antes da primeira placa: não dá para entrar pulando de longe
            kit.Platform("Corredor - entrada", new Vector3(0f, 0f, 28.25f), new Vector2(3f, 2.5f));

            // 3 placas longas (5 m) seguidas. Cada uma precisa ser cruzada em menos de 0,8 s depois do primeiro
            // contato: 5,03 m (até 5,31 m no pior caso) a 7,5 m/s = 0,67..0,71 s; a 5 m/s = 1,0 s, cai.
            float[] centros = { 32f, 37f, 42f };
            for (int i = 0; i < centros.Length; i++)
                kit.CrumblingPlatform($"Placa longa {i + 1}", new Vector3(0f, 0f, centros[i]), new Vector2(3f, 5f));

            kit.Platform("Corredor - saida", new Vector3(0f, 0f, 45.75f), new Vector2(3f, 2.5f));

            // forro a 2,0 m do piso (laje de 0,5 m com topo em 2,5): o jogador (1,76 m) quase não sobe ao pular,
            // então pular não encurta o tempo sobre as placas
            kit.Platform("Forro rebaixado", new Vector3(0f, 2.5f, 37f), new Vector2(3f, 20f));

            // paredes até o forro; a 1,3 m do eixo (a placa vai a 1,5 m), a cápsula sempre cai na área que detecta o passo
            kit.Wall("Parede - corredor esquerda", new Vector3(-1.3f, 0f, 27f), new Vector3(-1.3f, 0f, 47f), 2f);
            kit.Wall("Parede - corredor direita", new Vector3(1.3f, 0f, 27f), new Vector3(1.3f, 0f, 47f), 2f);

            // luminárias embutidas no forro
            kit.CeilingLight(new Vector3(0f, 2.2f, 30f));
            kit.CeilingLight(new Vector3(0f, 2.2f, 37f));
            kit.CeilingLight(new Vector3(0f, 2.2f, 44f));
        }

        // ------------------------------------------------------------------ 7/8. patamar e varanda do escudo

        static void PatamarEVarandaDoEscudo(PhaseKit kit)
        {
            kit.Platform("Patamar do corredor", new Vector3(0f, 0f, 49f), new Vector2(8f, 4f));
            kit.Pillar("Pilar patamar 1", new Vector3(3.5f, 0f, 47.6f));
            kit.Pillar("Pilar patamar 2", new Vector3(3.5f, 0f, 50.4f));
            kit.CeilingLight(new Vector3(0f, 3.2f, 49f));

            // recompensa opcional: vão de 3,5 m para -X (só com Velocidade: alcance 4,97 m; sem ela 3,32 m)
            kit.Platform("Varanda do escudo", new Vector3(-9.5f, 0f, 49f), new Vector2(4f, 3f));
            kit.Pickup(PickupKind.Shield, new Vector3(-9.5f, 0f, 49f));
            kit.Pole(new Vector3(-10.8f, 0f, 50f), Ambar);
        }

        // ------------------------------------------------------------------ 9. arquibancada que cede

        static void ArquibancadaQueCede(PhaseKit kit)
        {
            // vão de 1,5 m e depois 3 degraus colados que cedem, subindo 0,5 m cada (acima do degrau automático de 0,3 m)
            kit.CrumblingPlatform("Degrau que cede 1", new Vector3(0f, 0.5f, 53.5f), new Vector2(2.5f, 2f));
            kit.CrumblingPlatform("Degrau que cede 2", new Vector3(0f, 1.0f, 55.5f), new Vector2(2.5f, 2f));
            kit.CrumblingPlatform("Degrau que cede 3", new Vector3(0f, 1.5f, 57.5f), new Vector2(2.5f, 2f));
            kit.CeilingLight(new Vector3(0f, 4.2f, 55.5f));
        }

        // ------------------------------------------------------------------ 10-12. pêndulo e saída

        static void PenduloESaida(PhaseKit kit)
        {
            // patamar firme de onde se vê o pêndulo balançar antes de atravessar
            kit.Platform("Patamar do pendulo", new Vector3(0f, 2f, 60f), new Vector2(4f, 3f));
            kit.Pole(new Vector3(1.6f, 2f, 59f), Ambar);

            // ponte estreita (2 m) com um único pêndulo balançando de lado (plano X), período 2,4 s
            kit.Platform("Ponte do pendulo", new Vector3(0f, 2f, 64.5f), new Vector2(2f, 6f));
            kit.Pendulum("Pendulo", new Vector3(0f, 2f, 64.5f), 55f, 2.4f, 0f, 0f);

            kit.Platform("Saida", new Vector3(0f, 2f, 69.5f), new Vector2(6f, 4f));
            kit.Wall("Parede - saida fundo", new Vector3(-3f, 2f, 71.5f), new Vector3(3f, 2f, 71.5f));
            kit.Pillar("Pilar saida esquerda", new Vector3(-2.4f, 2f, 68.2f));
            kit.Pillar("Pilar saida direita", new Vector3(2.4f, 2f, 68.2f));
            kit.Pole(new Vector3(2.4f, 2f, 70.8f), Ambar);
            kit.Goal(new Vector3(0f, 2f, 70.5f), 0f);
        }
    }
}
