using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 5 — Laboratório (tema roxo/rosa): lasers com ritmos diferentes e escolha de rota.
    ///
    /// Percurso: recepção → corredor de calibração (1º laser) → sala das bancadas (pulos baixos) → sala de triagem,
    /// onde o caminho se divide em três:
    ///   • em frente, a passarela que aponta direto para a porta de conclusão (truque de cenário: é piso falso);
    ///   • à esquerda, a rota segura e longa (dois lasers lentos em "onda", serra e espinhos em ciclo);
    ///   • à direita, a rota curta: pega o Escudo e atravessa de propósito o laser mais difícil.
    ///
    /// Eixo Z aponta da entrada para a conclusão; X negativo = oeste (rota segura), X positivo = leste (rota curta).
    /// Coordenadas, grafo e cálculo dos pulos: docs/Fase5.md (os números de lá batem com os daqui).
    /// </summary>
    public sealed class Fase5Builder : IPhaseBuilder
    {
        public int Order => 5;
        public string SceneName => "Fase5_Laboratorio";
        public PhaseTheme Theme => PhaseTheme.Fase5;

        static readonly Color Rosa = new Color(1f, 0.45f, 0.75f);
        static readonly Color Lilas = new Color(0.75f, 0.55f, 1f);

        // ritmos dos lasers (segundos). Cada laser tem ciclo e offset próprios.
        const float CalibracaoLigado = 1.2f, CalibracaoDesligado = 2.0f, CalibracaoOffset = 0.7f;
        // os dois lentos têm o mesmo ciclo (4 s); o segundo está 0,6 s atrasado em relação ao primeiro
        // (offset 3,9 = 0,5 + 3,4 ≡ 0,5 − 0,6 no ciclo de 4 s): quem corre 3 m a 5 m/s passa pelos dois na mesma janela
        const float LentoLigado = 1.5f, LentoDesligado = 2.5f, Lento1Offset = 0.5f, Lento2Offset = 3.9f;
        // o laser da barreira fica ligado 4,2 s e só abre 0,8 s por ciclo: é o que o escudo resolve
        const float BarreiraLigado = 4.2f, BarreiraDesligado = 0.8f, BarreiraOffset = 1.3f;

        public void Build(PhaseKit kit)
        {
            Recepcao(kit);
            CorredorDeCalibracao(kit);
            SalaDasBancadas(kit);
            SalaDeTriagem(kit);
            PassarelaFalsa(kit);
            RotaSegura(kit);
            RotaCurta(kit);
            SalaFinal(kit);
            kit.FallZone(-8f);
        }

        // ------------------------------------------------------------------ 1. recepção (N0)

        static void Recepcao(PhaseKit kit)
        {
            kit.Platform("Piso Recepcao", P(0f, 0f, 0f), new Vector2(8f, 8f)); // x[-4,4] z[-4,4]
            Parede(kit, "Parede Recepcao sul", -4f, -4f, 4f, -4f);
            Parede(kit, "Parede Recepcao oeste", -4f, -4f, -4f, 4f);
            Parede(kit, "Parede Recepcao leste", 4f, -4f, 4f, 4f);
            Parede(kit, "Parede Recepcao norte A", -4f, 4f, -1.5f, 4f); // portal x[-1,5;1,5]
            Parede(kit, "Parede Recepcao norte B", 1.5f, 4f, 4f, 4f);

            kit.Spawn(P(0f, 0f, -1f), 0f);
            kit.Platform("Bancada decorativa recepcao", P(-2.5f, 0.9f, -3.2f), new Vector2(2.5f, 1.2f));
            kit.Sign("FASE 5 - LABORATORIO", P(0f, 2.5f, 3.7f), 0f, 0.08f);

            kit.Pole(P(-3.3f, 0f, 3.3f), Rosa);
            kit.Pole(P(3.3f, 0f, 3.3f), Lilas);
            kit.CeilingLight(P(0f, 3f, 0f));
        }

        // ------------------------------------------------------------------ 2. corredor de calibração (N1 → N2)

        static void CorredorDeCalibracao(PhaseKit kit)
        {
            // 3 m de largura: as paredes (0,3 m) deixam 2,7 m livres, exatamente o comprimento do feixe
            kit.Platform("Piso Corredor de calibracao", P(0f, 0f, 9f), new Vector2(3f, 10f)); // z[4,14]
            Parede(kit, "Parede Calibracao oeste", -1.5f, 4f, -1.5f, 14f);
            Parede(kit, "Parede Calibracao leste", 1.5f, 4f, 1.5f, 14f);

            kit.Laser("Laser de calibracao", P(0f, 0f, 9f), 0f, CalibracaoLigado, CalibracaoDesligado, CalibracaoOffset);
            kit.Sign("OBSERVE O RITMO", P(0f, 2.3f, 6.5f), 0f, 0.05f);
            kit.CeilingLight(P(0f, 3f, 12f), 90f);
        }

        // ------------------------------------------------------------------ 3. sala das bancadas (N2 → N5)

        static void SalaDasBancadas(PhaseKit kit)
        {
            // fosso entre bancadas de laboratório; quem cai volta ao início
            kit.Platform("Patamar das bancadas", P(0f, 0f, 15f), new Vector2(6f, 2f));      // z[14,16]  y 0
            kit.Platform("Bancada 1", P(-0.75f, 0.5f, 18f), new Vector2(2.5f, 2f));         // z[17,19]  y 0,5  x[-2;0,5]
            kit.Platform("Bancada 2", P(0.75f, 0.9f, 21.5f), new Vector2(2.5f, 2f));        // z[20,5;22,5] y 0,9 x[-0,5;2]
            kit.Platform("Patamar de saida", P(0f, 0f, 26.25f), new Vector2(6f, 3.5f));     // z[24,5;28] y 0

            Parede(kit, "Parede Bancadas sul A", -3f, 14f, -1.5f, 14f);
            Parede(kit, "Parede Bancadas sul B", 1.5f, 14f, 3f, 14f);
            Parede(kit, "Parede Bancadas oeste", -3f, 14f, -3f, 28f);
            Parede(kit, "Parede Bancadas leste", 3f, 14f, 3f, 28f);

            kit.CeilingLight(P(0f, 3f, 18f), 0f, Lilas);
            kit.CeilingLight(P(0f, 3f, 24f));
        }

        // ------------------------------------------------------------------ 4. sala de triagem: a bifurcação (N6)

        static void SalaDeTriagem(PhaseKit kit)
        {
            kit.Platform("Piso Triagem", P(0f, 0f, 32f), new Vector2(12f, 8f)); // x[-6,6] z[28,36]
            Parede(kit, "Parede Triagem sul A", -6f, 28f, -1.5f, 28f);
            Parede(kit, "Parede Triagem sul B", 1.5f, 28f, 6f, 28f);
            Parede(kit, "Parede Triagem oeste A", -6f, 28f, -6f, 30.5f);   // portal oeste z[30,5;33,5] -> rota segura
            Parede(kit, "Parede Triagem oeste B", -6f, 33.5f, -6f, 36f);
            Parede(kit, "Parede Triagem leste", 6f, 28f, 6f, 36f);
            Parede(kit, "Parede Triagem norte A", -6f, 36f, -1f, 36f);     // portal x[-1,1] -> passarela falsa
            Parede(kit, "Parede Triagem norte B", 1f, 36f, 3f, 36f);       // portal x[3,6]  -> rota curta (escudo)

            kit.Platform("Bancada decorativa triagem", P(-3f, 0.9f, 29f), new Vector2(3f, 1.2f));
            kit.Pillar("Pilar Triagem SO", P(-5.3f, 0f, 28.7f));
            kit.Pillar("Pilar Triagem SE", P(5.3f, 0f, 28.7f));
            kit.Pillar("Pilar Triagem NO", P(-5.3f, 0f, 35.3f));
            kit.Pillar("Pilar Triagem N", P(2f, 0f, 35.3f));

            kit.Pole(P(5.4f, 0f, 30f), Rosa);
            kit.Pole(P(-4f, 0f, 35.4f), Lilas);
            kit.CeilingLight(P(-3f, 3f, 32f));
            kit.CeilingLight(P(3f, 3f, 32f), 0f, Lilas);
        }

        // ------------------------------------------------------------------ 5. truque: passarela falsa (T0)

        static void PassarelaFalsa(PhaseKit kit)
        {
            // Liga a triagem direto à sala final e a porta aparece no fim dela, mas é piso falso.
            // 14 m de comprimento: depois de pisar sobram 0,8 s (4 m correndo); somando o pulo de entrada,
            // o pulo de saída e as folgas, o máximo alcançável é ~48,5 m < 50 m (início da sala final).
            // Ela treme antes de ceder (aviso justo) e fica sem luz, ao contrário das rotas verdadeiras.
            kit.CrumblingPlatform("Passarela falsa", P(0f, 0f, 43f), new Vector2(2f, 14f), 0.8f); // x[-1,1] z[36,50]
        }

        // ------------------------------------------------------------------ 6-9. rota segura, longa (A1..A5)

        static void RotaSegura(PhaseKit kit)
        {
            // 6. corredor oeste com um vão de 1,5 m
            kit.Platform("Corredor oeste", P(-7.5f, 0f, 32f), new Vector2(3f, 3f)); // x[-9,-6] z[30,5;33,5]
            Parede(kit, "Parede Oeste sul", -6f, 30.5f, -13.5f, 30.5f);             // também fecha o sul da galeria
            Parede(kit, "Parede Oeste norte", -6f, 33.5f, -10.5f, 33.5f);
            kit.CeilingLight(P(-8f, 3f, 32f), 0f, Lilas);

            // 7. galeria de lasers lentos (3 m de largura, mesma regra do corredor de calibração)
            kit.Platform("Galeria de lasers", P(-12f, 0f, 39.75f), new Vector2(3f, 18.5f)); // x[-13,5;-10,5] z[30,5;49]
            Parede(kit, "Parede Galeria oeste", -13.5f, 30.5f, -13.5f, 58f);               // segue pela sala da centrifuga
            Parede(kit, "Parede Galeria leste", -10.5f, 33.5f, -10.5f, 49f);
            kit.Laser("Laser lento 1", P(-12f, 0f, 38f), 0f, LentoLigado, LentoDesligado, Lento1Offset);
            kit.Laser("Laser lento 2", P(-12f, 0f, 41f), 0f, LentoLigado, LentoDesligado, Lento2Offset);
            kit.CeilingLight(P(-12f, 3f, 35f), 90f);
            kit.CeilingLight(P(-12f, 3f, 45f), 90f, Lilas);

            // 8. sala da centrífuga: serra que varre a sala de oeste a leste em z = 51,5
            kit.Platform("Sala da centrifuga", P(-9.25f, 0f, 53.5f), new Vector2(8.5f, 9f)); // x[-13,5;-5] z[49,58]
            Parede(kit, "Parede Centrifuga sul", -10.5f, 49f, -5f, 49f);
            Parede(kit, "Parede Centrifuga norte", -13.5f, 58f, -5f, 58f);
            Parede(kit, "Parede Centrifuga leste A", -5f, 49f, -5f, 53f);   // portal z[53,55]
            Parede(kit, "Parede Centrifuga leste B", -5f, 55f, -5f, 58f);
            // yaw 0: a lâmina gira no plano XY e rola ao longo de X; o deslocamento é em X puro
            kit.Saw("Serra da centrifuga", P(-12.5f, 0f, 51.5f), new Vector3(6.5f, 0f, 0f), 4f, 0f);
            kit.Pillar("Pilar Centrifuga", P(-12.9f, 0f, 57.4f));
            kit.CeilingLight(P(-9.25f, 3f, 55f));

            // 9. passagem de amostras: 2 m de largura, coberta por espinhos em ciclo
            kit.Platform("Passagem de amostras", P(-4f, 0f, 54f), new Vector2(2f, 2f)); // x[-5,-3] z[53,55]
            Parede(kit, "Parede Passagem sul", -5f, 53f, -3f, 53f);
            Parede(kit, "Parede Passagem norte", -5f, 55f, -3f, 55f);
            kit.Spikes("Espinhos da passagem", P(-4f, 0f, 54f), true, 1.0f, 2.0f, 0f);
        }

        // ------------------------------------------------------------------ 10-11. rota curta (B1..B4)

        static void RotaCurta(PhaseKit kit)
        {
            kit.Platform("Corredor do escudo", P(4.5f, 0f, 46.5f), new Vector2(3f, 21f)); // x[3,6] z[36,57]
            Parede(kit, "Parede Escudo oeste", 3f, 36f, 3f, 50f);   // dali em diante a parede é a da sala final
            Parede(kit, "Parede Escudo leste", 6f, 36f, 6f, 57f);
            Parede(kit, "Parede Escudo norte", 3f, 57f, 6f, 57f);

            // 10. o escudo fica no caminho, 6 m antes do laser que ele resolve
            kit.Pickup(PickupKind.Shield, P(4.5f, 0f, 39f));
            kit.Sign("ESCUDO = 1 TOQUE", P(4.5f, 2.3f, 40.5f), 0f, 0.05f);

            // 11. barreira: o laser mais difícil (aberto só 0,8 s a cada 5 s)
            kit.Laser("Laser da barreira", P(4.5f, 0f, 45f), 0f, BarreiraLigado, BarreiraDesligado, BarreiraOffset);

            kit.CeilingLight(P(4.5f, 3f, 41f), 90f, Lilas);
            kit.CeilingLight(P(4.5f, 3f, 52f), 90f);
        }

        // ------------------------------------------------------------------ 12. sala final (G1, G)

        static void SalaFinal(PhaseKit kit)
        {
            kit.Platform("Piso Sala final", P(0f, 0f, 54f), new Vector2(6f, 8f)); // x[-3,3] z[50,58]
            Parede(kit, "Parede Final sul A", -3f, 50f, -1f, 50f);  // portal x[-1,1]: fim da passarela falsa
            Parede(kit, "Parede Final sul B", 1f, 50f, 3f, 50f);
            Parede(kit, "Parede Final norte", -3f, 58f, 3f, 58f);
            Parede(kit, "Parede Final oeste A", -3f, 50f, -3f, 53f); // portal z[53,55]: chegada da rota segura
            Parede(kit, "Parede Final oeste B", -3f, 55f, -3f, 58f);
            Parede(kit, "Parede Final leste A", 3f, 50f, 3f, 53f);   // portal z[53,55]: chegada da rota curta
            Parede(kit, "Parede Final leste B", 3f, 55f, 3f, 58f);

            kit.Goal(P(0f, 0f, 56.5f), 0f);

            kit.Pillar("Pilar Final O", P(-2.4f, 0f, 57.4f));
            kit.Pillar("Pilar Final L", P(2.4f, 0f, 57.4f));
            // os postes emolduram a porta vista da triagem: a isca do truque
            kit.Pole(P(-2.4f, 0f, 50.6f), Rosa);
            kit.Pole(P(2.4f, 0f, 50.6f), Rosa);
            kit.CeilingLight(P(0f, 3f, 53f), 0f, Lilas);
        }

        // ------------------------------------------------------------------ utilitários

        static Vector3 P(float x, float y, float z) => new Vector3(x, y, z);

        /// <summary>Parede de 3 m com base no piso (y = 0), de (x1, z1) até (x2, z2).</summary>
        static void Parede(PhaseKit kit, string nome, float x1, float z1, float x2, float z2) =>
            kit.Wall(nome, new Vector3(x1, 0f, z1), new Vector3(x2, 0f, z2));
    }
}
