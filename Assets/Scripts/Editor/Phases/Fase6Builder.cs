using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 6 — Oficina das Serras. Galpão de oficina em tons de terracota: serras que cruzam corredores,
    /// pêndulos e uma escadaria de degraus altos (1,6 a 1,8 m) que só se sobe com o Pulo Duplo.
    ///
    /// Truque de cenário: da entrada, a porta aparece logo à frente, no fim de uma passarela reta.
    /// A passarela termina num degrau de 1,7 m (um pulo alcança 1,1 m) e a placa manda procurar o poder no fundo.
    /// O caminho certo desce ao poço (queda de 3,4 m, sem volta), pega o Pulo Duplo, atravessa as armadilhas
    /// e volta a subir pela escadaria até o mezanino da porta. Documentação e grafo em docs/Fase6.md.
    ///
    /// Eixos: +Z é "para frente" a partir da entrada; X é lateral. Pisos de 0,5 m de espessura (topo informado).
    /// </summary>
    public sealed class Fase6Builder : IPhaseBuilder
    {
        public int Order => 6;
        public string SceneName => "Fase6_OficinaDasSerras";
        public PhaseTheme Theme => PhaseTheme.Fase6;

        // alturas do topo dos pisos
        const float YEntrada = 0f;
        const float YBaixo = -3.4f;    // poço, corredores e galpão: 3,4 m abaixo da entrada (não dá para voltar nem com pulo duplo)
        const float YDegrau1 = -1.8f;  // +1,6 m sobre o galpão
        const float YDegrau2 = -0.1f;  // +1,7 m sobre o degrau 1
        const float YMezanino = 1.7f;  // +1,8 m sobre o degrau 2 e +1,7 m sobre a passarela direta

        // paredes da parte baixa com topo em +1,6: da entrada (y = 0) seria preciso subir 1,6 m, mais que um pulo
        const float AlturaParedeBaixa = 5f;
        // paredes do galpão com topo em +1,2, encostadas por baixo do mezanino
        const float AlturaParedeGalpao = 4.6f;

        static readonly Color LuzAlerta = new Color(1f, 0.42f, 0.28f);
        static readonly Color LuzQuente = new Color(1f, 0.82f, 0.6f);

        public void Build(PhaseKit kit)
        {
            Entrada(kit);
            PassarelaDireta(kit);
            Mezanino(kit);
            Poco(kit);
            CorredorDaSerra(kit);
            GaleriaDoPendulo(kit);
            Curva(kit);
            PassarelaEstreita(kit);
            Galpao(kit);
            Escalada(kit);
            kit.FallZone(-8f);
        }

        // ------------------------------------------------------------------ 1. entrada (N0)

        void Entrada(PhaseKit kit)
        {
            // x [-3, 3], z [-1, 5]; a borda oeste (x = -3) é a descida para o poço
            kit.Platform("Entrada", new Vector3(0f, YEntrada, 2f), new Vector2(6f, 6f));
            kit.Wall("Parede fundo da entrada", new Vector3(-3f, YEntrada, -1.15f), new Vector3(3f, YEntrada, -1.15f));
            kit.Wall("Parede leste da entrada", new Vector3(3.15f, YEntrada, -1f), new Vector3(3.15f, YEntrada, 5f));
            kit.Pillar("Pilar entrada fundo", new Vector3(2.6f, YEntrada, -0.6f));
            kit.Pillar("Pilar entrada frente", new Vector3(2.6f, YEntrada, 4.6f));
            kit.Pole(new Vector3(-2.6f, YEntrada, 4.4f));
            kit.Pole(new Vector3(-2.6f, YEntrada, -0.4f));

            kit.Spawn(new Vector3(0f, YEntrada, 1.6f), 0f);
            kit.Sign("FASE 6", new Vector3(0f, 5f, 6f), 0f, 0.07f);
            kit.Sign("OFICINA DAS SERRAS", new Vector3(0f, 4.3f, 6f), 0f, 0.09f);
        }

        // ------------------------------------------------------------------ 2. passarela direta: o truque (N1)

        void PassarelaDireta(PhaseKit kit)
        {
            // reta da entrada até a porta, z [5, 12]; termina sob a borda do mezanino, 1,7 m acima
            kit.Platform("Passarela direta", new Vector3(0f, YEntrada, 8.5f), new Vector2(2f, 7f));
            // face do degrau: parede até a parte de baixo do mezanino (topo em +1,2), fechando o vão
            kit.Wall("Parede sob o mezanino", new Vector3(-3f, -1f, 12.15f), new Vector3(3f, -1f, 12.15f), 2.2f);
            kit.CeilingLight(new Vector3(0f, 3.6f, 8.5f), 90f);

            // dica 1: lida no fim da passarela, na altura dos olhos
            kit.Sign("Alto demais para um pulo", new Vector3(0f, 0.95f, 11.95f), 0f, 0.035f);
            kit.Sign("Procure o poder no fundo", new Vector3(0f, 0.6f, 11.95f), 0f, 0.03f);
        }

        // ------------------------------------------------------------------ 10. mezanino e porta (N10, N11)

        void Mezanino(PhaseKit kit)
        {
            // x [-3, 13], z [12, 16.8]; a escadaria chega pela borda norte, entre x 8 e 13
            kit.Platform("Mezanino da porta", new Vector3(5f, YMezanino, 14.4f), new Vector2(16f, 4.8f));
            kit.Wall("Guarda-corpo norte", new Vector3(-3f, YMezanino, 16.65f), new Vector3(8f, YMezanino, 16.65f), 1f);
            kit.Wall("Guarda-corpo oeste", new Vector3(-2.85f, YMezanino, 12f), new Vector3(-2.85f, YMezanino, 16.5f), 1f);
            // a borda leste ao sul de z = 16 dá para o vazio; o trecho norte fica livre para quem chega da escalada
            kit.Wall("Guarda-corpo leste", new Vector3(12.85f, YMezanino, 12f), new Vector3(12.85f, YMezanino, 16f), 1f);

            // a porta fica de frente para a entrada: é ela que torna a passarela direta tentadora
            kit.Goal(new Vector3(0f, YMezanino, 14.4f), 0f);

            // apoios que descem do mezanino até o fundo escuro
            kit.Pillar("Pilar sob o mezanino oeste", new Vector3(-2.4f, -7.5f, 16.2f), 8.7f);
            kit.Pillar("Pilar sob o mezanino centro", new Vector3(4.5f, -7.5f, 16.2f), 8.7f);

            kit.Pole(new Vector3(-2.3f, YMezanino, 12.6f));
            kit.Pole(new Vector3(8.6f, YMezanino, 12.6f), LuzQuente);
            kit.CeilingLight(new Vector3(0f, 5.6f, 14.4f));
            kit.CeilingLight(new Vector3(7f, 5.2f, 14.4f));
        }

        // ------------------------------------------------------------------ 3. poço do pulo duplo (N2)

        void Poco(PhaseKit kit)
        {
            // x [-11, -3], z [-2, 6]; recebe quem desce da borda oeste da entrada
            kit.Platform("Poco do pulo duplo", new Vector3(-7f, YBaixo, 2f), new Vector2(8f, 8f));
            kit.Wall("Parede sob a entrada", new Vector3(-2.85f, YBaixo, -2f), new Vector3(-2.85f, YBaixo, 6f), 2.9f);
            kit.Wall("Parede oeste do poco", new Vector3(-11.15f, YBaixo, -2f), new Vector3(-11.15f, YBaixo, 6f), AlturaParedeBaixa);
            kit.Wall("Parede sul do poco", new Vector3(-11f, YBaixo, -2.15f), new Vector3(-3f, YBaixo, -2.15f), AlturaParedeBaixa);
            kit.Wall("Parede norte do poco oeste", new Vector3(-11f, YBaixo, 6f), new Vector3(-10f, YBaixo, 6f), AlturaParedeBaixa);
            kit.Wall("Parede norte do poco leste", new Vector3(-4f, YBaixo, 6f), new Vector3(-3f, YBaixo, 6f), AlturaParedeBaixa);
            kit.Pillar("Pilar do poco sul", new Vector3(-10.5f, YBaixo, -1.5f), AlturaParedeBaixa);
            kit.Pillar("Pilar do poco norte", new Vector3(-10.5f, YBaixo, 5.4f), AlturaParedeBaixa);

            // na parte baixa, antes de qualquer armadilha; dura até a fase reiniciar
            kit.Pickup(PickupKind.DoubleJump, new Vector3(-7.5f, YBaixo, 2f));

            kit.Pole(new Vector3(-10.5f, YBaixo, 2f));
            kit.CeilingLight(new Vector3(-7f, 0.2f, 2f));
        }

        // ------------------------------------------------------------------ 4. corredor da serra (N3 -> N4)

        void CorredorDaSerra(PhaseKit kit)
        {
            // x [-10, -4], z [6, 17]; as paredes seguem pela galeria e pela curva
            kit.Platform("Corredor da serra", new Vector3(-7f, YBaixo, 11.5f), new Vector2(6f, 11f));
            kit.Wall("Parede oeste do corredor", new Vector3(-10.15f, YBaixo, 6f), new Vector3(-10.15f, YBaixo, 32f), AlturaParedeBaixa);
            kit.Wall("Parede leste do corredor", new Vector3(-3.85f, YBaixo, 6f), new Vector3(-3.85f, YBaixo, 29f), AlturaParedeBaixa);

            // cruza o corredor em X (de -8,9 a -5,1) a cada 3 s; parada em cada ponta: passe pelo lado oposto
            kit.Saw("Serra do corredor", new Vector3(-8.9f, YBaixo, 12f), new Vector3(3.8f, 0f, 0f), 3f);

            kit.CeilingLight(new Vector3(-7f, 0.2f, 8f));
            kit.CeilingLight(new Vector3(-7f, 0.2f, 12f), 0f, LuzAlerta);
        }

        // ------------------------------------------------------------------ 5. galeria do pêndulo (N4 -> N5)

        void GaleriaDoPendulo(PhaseKit kit)
        {
            // x [-10, -4], z [17, 28]
            kit.Platform("Galeria do pendulo", new Vector3(-7f, YBaixo, 22.5f), new Vector2(6f, 11f));

            // balança em X, atravessando toda a largura; offset 0
            kit.Pendulum("Pendulo da galeria", new Vector3(-7f, YBaixo, 22f), 50f, 2.4f, 0f, 0f);
            kit.Platform("Viga do pendulo da galeria", new Vector3(-7f, YBaixo + 4.3f, 22f), new Vector2(6.3f, 0.6f));

            kit.CeilingLight(new Vector3(-7f, 0.2f, 19f));
            kit.CeilingLight(new Vector3(-7f, 0.2f, 25.5f));
        }

        // ------------------------------------------------------------------ 6. curva (N5)

        void Curva(PhaseKit kit)
        {
            // x [-10, -4], z [28, 32]; a saída leste (z 29 a 31) leva à passarela estreita
            kit.Platform("Curva", new Vector3(-7f, YBaixo, 30f), new Vector2(6f, 4f));
            kit.Wall("Parede norte da curva", new Vector3(-10f, YBaixo, 32.15f), new Vector3(-4f, YBaixo, 32.15f), AlturaParedeBaixa);
            kit.Wall("Parede leste da curva", new Vector3(-3.85f, YBaixo, 31f), new Vector3(-3.85f, YBaixo, 32f), AlturaParedeBaixa);
            kit.Pole(new Vector3(-9.4f, YBaixo, 31.4f));

            // dica 2: sobre a entrada da passarela estreita, lida olhando para +X
            kit.Sign("Leia o ritmo antes de correr", new Vector3(-4.3f, YBaixo + 2.2f, 30f), 90f, 0.04f);
        }

        // ------------------------------------------------------------------ 7. passarela estreita: serra + pêndulo (N5 -> N6 -> N7)

        void PassarelaEstreita(PhaseKit kit)
        {
            // x [-4, 6], z [29, 31]: 2 m de largura sobre o vazio
            kit.Platform("Passarela estreita", new Vector3(1f, YBaixo, 30f), new Vector2(10f, 2f));

            // serra em x = -1 cruzando a passarela em Z (de 27 a 33) a cada 3,2 s, sobre trilhos dos dois lados
            kit.Platform("Trilho da serra sul", new Vector3(-1f, YBaixo, 27.5f), new Vector2(0.8f, 3f));
            kit.Platform("Trilho da serra norte", new Vector3(-1f, YBaixo, 32.5f), new Vector2(0.8f, 3f));
            kit.Saw("Serra da passarela", new Vector3(-1f, YBaixo, 27f), new Vector3(0f, 0f, 6f), 3.2f);

            // pêndulo em x = 3,5 balançando em Z (yaw 90), com offset diferente do da galeria
            kit.Pendulum("Pendulo da passarela", new Vector3(3.5f, YBaixo, 30f), 50f, 2.4f, 0.6f, 90f);
            kit.Platform("Viga do pendulo da passarela", new Vector3(3.5f, YBaixo + 4.3f, 30f), new Vector2(0.6f, 7.4f));
            kit.Pillar("Pilar da viga sul", new Vector3(3.5f, -7.5f, 26.6f), 7.9f);
            kit.Pillar("Pilar da viga norte", new Vector3(3.5f, -7.5f, 33.4f), 7.9f);

            // o bolsão seguro entre as duas armadilhas (x de 0,1 a 2,6) fica sob uma luz quente
            kit.CeilingLight(new Vector3(1.3f, 0.2f, 30f), 90f, LuzQuente);
            kit.CeilingLight(new Vector3(-1f, 0.2f, 30f), 90f, LuzAlerta);
        }

        // ------------------------------------------------------------------ 8. galpão da escalada (N7) e rota de queda

        void Galpao(PhaseKit kit)
        {
            // x [6, 14], z [16, 34]: fica sob toda a escadaria; quem cai dos degraus volta para cá
            kit.Platform("Galpao da escalada", new Vector3(10f, YBaixo, 25f), new Vector2(8f, 18f));
            kit.Wall("Parede leste do galpao", new Vector3(14.15f, YBaixo, 16f), new Vector3(14.15f, YBaixo, 34f), AlturaParedeGalpao);
            kit.Wall("Parede norte do galpao", new Vector3(6f, YBaixo, 34.15f), new Vector3(14f, YBaixo, 34.15f), AlturaParedeGalpao);
            kit.Wall("Parede sul do galpao", new Vector3(6f, YBaixo, 15.85f), new Vector3(14f, YBaixo, 15.85f), AlturaParedeGalpao);
            kit.Wall("Parede oeste do galpao sul", new Vector3(5.85f, YBaixo, 16f), new Vector3(5.85f, YBaixo, 29f), AlturaParedeGalpao);
            kit.Wall("Parede oeste do galpao norte", new Vector3(5.85f, YBaixo, 31f), new Vector3(5.85f, YBaixo, 34f), AlturaParedeGalpao);

            // pilares que sustentam a borda norte do mezanino (topo em +1,2) e cantos do galpão
            kit.Pillar("Pilar do mezanino oeste", new Vector3(7f, YBaixo, 16.4f), AlturaParedeGalpao);
            kit.Pillar("Pilar do mezanino leste", new Vector3(13f, YBaixo, 16.4f), AlturaParedeGalpao);
            kit.Pillar("Pilar do galpao nordeste", new Vector3(13.5f, YBaixo, 33.5f), AlturaParedeGalpao);
            kit.Pillar("Pilar do galpao noroeste", new Vector3(6.5f, YBaixo, 33.5f), AlturaParedeGalpao);

            kit.Pole(new Vector3(13.4f, YBaixo, 28f), LuzQuente);
            kit.CeilingLight(new Vector3(10f, 0.8f, 30f));
            kit.CeilingLight(new Vector3(9f, 0.8f, 26.5f));
        }

        // ------------------------------------------------------------------ 9. escadaria de degraus altos (N8, N9)

        void Escalada(PhaseKit kit)
        {
            // sobe em -Z: galpão (-3,4) -> degrau 1 (-1,8) -> degrau 2 (-0,1) -> mezanino (+1,7)
            // vãos de 1,2 m entre os degraus; abaixo de todos está o piso do galpão
            kit.Platform("Degrau alto 1", new Vector3(11f, YDegrau1, 23.7f), new Vector2(4f, 3f));   // z [22.2, 25.2]
            kit.Pillar("Pilar do degrau 1", new Vector3(11f, YBaixo, 23.7f), YDegrau1 - 0.5f - YBaixo);
            kit.Platform("Degrau alto 2", new Vector3(11f, YDegrau2, 19.5f), new Vector2(4f, 3f));   // z [18, 21]
            kit.Pillar("Pilar do degrau 2", new Vector3(11f, YBaixo, 19.5f), YDegrau2 - 0.5f - YBaixo);

            kit.CeilingLight(new Vector3(11f, 3.2f, 19.5f));
        }
    }
}
