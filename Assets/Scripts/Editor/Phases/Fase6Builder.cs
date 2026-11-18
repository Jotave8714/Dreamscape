using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 6 — Oficina das Serras: a oficina de encadernação da biblioteca do sonho (tema terracota).
    /// Serras que cortam corredores de estantes, prensas de livros, pêndulos e o Pulo Duplo para subir.
    ///
    /// Ritmo da fase:
    ///   1. apresentar — da entrada, a porta aparece logo à frente, no mezanino; a passarela direta termina num degrau
    ///      de 1,7 m (truque: "alto demais para um pulo"). A placa manda descer ao poço, onde está o Pulo Duplo e a
    ///      primeira estante-degrau de 1,6 m, que já exige o poder (falhar só devolve ao poço).
    ///   2. testar — corredor da serra (passe pelo lado oposto), galeria do pêndulo e o corredor das prensas.
    ///   3. escolher — na curva há um segundo Pulo Duplo e duas saídas:
    ///        • rota segura: a passarela estreita com serra e pêndulo, separados por um bolsão iluminado;
    ///        • rota arriscada: pilhas de livros altas sobre o vazio, com dois vãos de 4,4 m que só o pulo duplo vence.
    ///   4. final — no galpão, um terceiro Pulo Duplo e a escadaria de dois degraus de 1,75 m até o mezanino.
    ///
    /// Cada coletável fica perto do trecho que exige o poder (o efeito dura 30 s).
    /// Eixos: +Z é "para frente" a partir da entrada; X é lateral. Documentação, grafo e contas em docs/Fase6.md.
    /// </summary>
    public sealed class Fase6Builder : IPhaseBuilder
    {
        public int Order => 6;
        public string SceneName => "Fase6_OficinaDasSerras";
        public PhaseTheme Theme => PhaseTheme.Fase6;

        // alturas do topo dos pisos
        const float YEntrada = 0f;
        const float YPoco = -3.4f;     // poço: 3,4 m abaixo da entrada (não dá para voltar nem com pulo duplo)
        const float YOficina = -1.8f;  // corredores, curva, passarela estreita e galpão: +1,6 m sobre o poço
        const float YAlta = -0.1f;     // pilhas da rota arriscada: +1,7 m sobre a curva
        const float YDegrau = -0.05f;  // degrau da escadaria: +1,75 m sobre o galpão
        const float YMezanino = 1.7f;  // +1,75 m sobre o degrau e +1,7 m sobre a passarela direta

        // paredes da oficina: base -1,8, topo +2,2 (4 m acima do piso; nem com pulo duplo se alcança o topo)
        const float AlturaOficina = 4f;

        static readonly Color LuzAlerta = new Color(1f, 0.42f, 0.28f);
        static readonly Color LuzQuente = new Color(1f, 0.82f, 0.6f);
        static readonly Color LuzPoder = new Color(0.95f, 0.8f, 0.35f);

        public void Build(PhaseKit kit)
        {
            Entrada(kit);
            PassarelaDireta(kit);
            Mezanino(kit);
            Poco(kit);
            CorredorDaSerra(kit);
            GaleriaDoPendulo(kit);
            CorredorDasPrensas(kit);
            Curva(kit);
            PassarelaEstreita(kit);
            RotaAlta(kit);
            Galpao(kit);
            Escalada(kit);
            Horizonte(kit);

            kit.FallZone(-9f);
            // retângulo que cobre todas as plataformas (x -11,3..14,3, z -2,3..38,3)
            kit.LibraryBackdrop(new Vector2(-11.3f, -2.3f), new Vector2(14.3f, 38.3f), -7f, 15f, 6f);
        }

        // ------------------------------------------------------------------ 1. entrada (N0)

        void Entrada(PhaseKit kit)
        {
            // x [-3, 3], z [-1, 5]; a borda oeste (x = -3) é a descida para o poço
            kit.Platform("Entrada", new Vector3(0f, YEntrada, 2f), new Vector2(6f, 6f));
            Estante(kit, "Entrada fundo", -3f, -1.3f, 3.3f, -1.3f, YEntrada, 4f);
            Estante(kit, "Entrada leste", 3.3f, -1.3f, 3.3f, 5f, YEntrada, 4f);
            kit.ReadingDesk("Mesa de costura da entrada", new Vector3(2.2f, YEntrada, 0.4f), 90f);
            kit.ReadingLamp(new Vector3(2.6f, YEntrada, 4.5f), -135f);
            kit.Carpet(new Vector3(0f, YEntrada, 2f), new Vector2(2f, 5.6f));

            kit.Spawn(new Vector3(0f, YEntrada, 1.6f), 0f);
            kit.Sign("FASE 6", new Vector3(0f, 5f, 6f), 0f, 0.07f);
            kit.Sign("OFICINA DAS SERRAS", new Vector3(0f, 4.3f, 6f), 0f, 0.09f);
            kit.Sign("a encadernacao do sonho", new Vector3(0f, 3.8f, 6f), 0f, 0.04f);
            // luz sobre a borda oeste: o caminho certo desce por ali
            kit.CeilingLight(new Vector3(-2f, 3.4f, 2f));
        }

        // ------------------------------------------------------------------ 2. passarela direta: o truque (N1)

        void PassarelaDireta(PhaseKit kit)
        {
            // reta de livros da entrada até a porta, z [5, 12]; termina sob a borda do mezanino, 1,7 m acima
            kit.BookPlatform("Passarela direta", new Vector3(0f, YEntrada, 8.5f), new Vector2(2f, 7f), 0f, 1.5f);
            // face do degrau: estante até a parte de baixo do mezanino (topo em +1,2), fechando o vão
            Estante(kit, "Sob o mezanino", -3f, 12.3f, 3f, 12.3f, -1f, 2.2f);

            // dica 1: lida no fim da passarela, na altura dos olhos
            kit.Sign("Alto demais para um pulo", new Vector3(0f, 0.95f, 11.95f), 0f, 0.035f);
            kit.Sign("Procure o poder no fundo", new Vector3(0f, 0.6f, 11.95f), 0f, 0.03f);
        }

        // ------------------------------------------------------------------ 12. mezanino e porta (N11, N12)

        void Mezanino(PhaseKit kit)
        {
            // x [-3, 13], z [12, 16.8]; a escadaria chega pela borda norte, entre x 8 e 13
            kit.Platform("Mezanino da porta", new Vector3(5f, YMezanino, 14.4f), new Vector2(16f, 4.8f));
            Estante(kit, "Guarda-corpo norte", -3f, 16.5f, 8f, 16.5f, YMezanino, 1f);
            Estante(kit, "Guarda-corpo oeste", -2.7f, 12f, -2.7f, 16.2f, YMezanino, 1f);
            // a borda leste ao sul de z = 16 dá para o vazio; o trecho norte fica livre para quem chega da escalada
            Estante(kit, "Guarda-corpo leste", 12.7f, 12f, 12.7f, 16f, YMezanino, 1f);

            // a porta fica de frente para a entrada: é ela que torna a passarela direta tentadora
            kit.Goal(new Vector3(0f, YMezanino, 14.4f), 0f);
            kit.Carpet(new Vector3(6f, YMezanino, 14.4f), new Vector2(11f, 1.8f));

            // apoios que descem do mezanino até o fundo escuro
            kit.Pillar("Pilar sob o mezanino oeste", new Vector3(-2.4f, -7.5f, 16.2f), 8.7f);
            kit.Pillar("Pilar sob o mezanino centro", new Vector3(4.5f, -7.5f, 16.2f), 8.7f);

            kit.ReadingLamp(new Vector3(-2.2f, YMezanino, 12.5f), 45f);
            kit.ReadingLamp(new Vector3(6f, YMezanino, 12.4f), 0f);
            kit.CeilingLight(new Vector3(0f, 5.6f, 14.4f));
            kit.CeilingLight(new Vector3(9f, 5.2f, 14.4f));
        }

        // ------------------------------------------------------------------ 3. poço do pulo duplo (N2)

        void Poco(PhaseKit kit)
        {
            // x [-11, -3], z [-2, 6]; recebe quem desce da borda oeste da entrada
            kit.Platform("Poco do pulo duplo", new Vector3(-7f, YPoco, 2f), new Vector2(8f, 8f));
            Estante(kit, "Sob a entrada", -2.7f, -2f, -2.7f, 6f, YPoco, 3.4f);   // topo rente à entrada (y 0)
            Estante(kit, "Poco oeste", -11.3f, -2.3f, -11.3f, 6.3f, YPoco, 5f);  // topo em +1,6
            Estante(kit, "Poco sul", -11.3f, -2.3f, -2.4f, -2.3f, YPoco, 5f);
            Estante(kit, "Poco norte O", -11.3f, 6.3f, -10f, 6.3f, YPoco, 5f);
            Estante(kit, "Poco norte L", -4f, 6.3f, -2.4f, 6.3f, YPoco, 5f);
            // a estante-degrau: 1,6 m de altura, topo rente ao piso do corredor (face em z = 6)
            Estante(kit, "Estante-degrau do poco", -10f, 6.3f, -4f, 6.3f, YPoco, 1.6f);

            // primeiro Pulo Duplo: já é exigido para sair do poço (+1,6 m); errar só devolve ao chão do poço
            kit.Pickup(PickupKind.DoubleJump, new Vector3(-7f, YPoco, 2.5f));
            kit.Sign("PULO DUPLO", new Vector3(-7f, 1.6f, 6.3f), 0f, 0.05f);
            kit.Sign("pule de novo no ar", new Vector3(-7f, 1.1f, 6.3f), 0f, 0.035f);

            kit.ReadingLamp(new Vector3(-10.4f, YPoco, -1.4f), 45f);
            kit.ReadingLamp(new Vector3(-3.6f, YPoco, -1.4f), -45f);
            kit.CeilingLight(new Vector3(-7f, 0.6f, 2f), 0f, LuzPoder);
        }

        // ------------------------------------------------------------------ 4. corredor da serra (N3 -> N4)

        void CorredorDaSerra(PhaseKit kit)
        {
            // x [-10, -4], z [6, 21]; faces internas das estantes em -10 e -4
            kit.Platform("Corredor da serra", new Vector3(-7f, YOficina, 13.5f), new Vector2(6f, 15f));
            Estante(kit, "Corredor oeste", -10.3f, 6f, -10.3f, 21.3f, YOficina, AlturaOficina);
            Estante(kit, "Corredor leste", -3.7f, 6f, -3.7f, 21.3f, YOficina, AlturaOficina);

            // cruza o corredor em X (de -8,9 a -5,1) a cada 3 s: passe pelo lado oposto ao da serra
            kit.Saw("Serra do corredor", new Vector3(-8.9f, YOficina, 11f), new Vector3(3.8f, 0f, 0f), 3f);
            kit.Sign("Passe pelo lado oposto", new Vector3(-7f, 1.4f, 7f), 0f, 0.04f);

            kit.ReadingLamp(new Vector3(-10.3f, YOficina + AlturaOficina, 8f), 90f);
            kit.CeilingLight(new Vector3(-7f, 1.8f, 8.5f));
            kit.CeilingLight(new Vector3(-7f, 1.8f, 11f), 0f, LuzAlerta);
        }

        // ------------------------------------------------------------------ 5. galeria do pêndulo (N4 -> N5)

        void GaleriaDoPendulo(PhaseKit kit)
        {
            // mesmo piso do corredor, z [14, 21]; balança em X, atravessando toda a largura
            kit.Pendulum("Pendulo da galeria", new Vector3(-7f, YOficina, 17f), 50f, 2.4f, 0f, 0f);
            kit.Platform("Viga do pendulo da galeria", new Vector3(-7f, YOficina + 4.3f, 17f), new Vector2(6.6f, 0.6f));

            kit.ReadingLamp(new Vector3(-3.7f, YOficina + AlturaOficina, 15f), -90f);
            kit.CeilingLight(new Vector3(-7f, 1.8f, 19.5f), 0f, LuzAlerta);
        }

        // ------------------------------------------------------------------ 6. corredor das prensas de livros (N5 -> N6)

        void CorredorDasPrensas(PhaseKit kit)
        {
            // 1,6 m livres entre as estantes (faces em -7,8 e -6,2): a prensa cobre a largura toda
            kit.Platform("Corredor das prensas", new Vector3(-7f, YOficina, 25f), new Vector2(2.2f, 8f)); // z [21, 29]
            Estante(kit, "Prensas oeste", -8.1f, 21.6f, -8.1f, 28.4f, YOficina, AlturaOficina);
            Estante(kit, "Prensas leste", -5.9f, 21.6f, -5.9f, 28.4f, YOficina, AlturaOficina);
            // fecham os bolsões sem piso dos dois lados do corredor
            Estante(kit, "Prensas sul O", -10.3f, 21.3f, -8.1f, 21.3f, YOficina, AlturaOficina);
            Estante(kit, "Prensas sul L", -5.9f, 21.3f, -3.7f, 21.3f, YOficina, AlturaOficina);
            Estante(kit, "Prensas norte O", -10.3f, 28.7f, -8.1f, 28.7f, YOficina, AlturaOficina);
            Estante(kit, "Prensas norte L", -5.9f, 28.7f, -3.7f, 28.7f, YOficina, AlturaOficina);

            // ciclo de 3,8 s (2 s erguida, 0,2 s descendo, 0,8 s embaixo, 0,8 s subindo); a segunda está meio ciclo à frente
            kit.Press("Prensa de livros 1", new Vector3(-7f, YOficina, 23f), 2f, 0.8f, 0f);
            kit.Press("Prensa de livros 2", new Vector3(-7f, YOficina, 27f), 2f, 0.8f, 1.9f);
            kit.Sign("PRENSAS: NAO PULE EMBAIXO", new Vector3(-7f, 2.2f, 21.2f), 0f, 0.035f);

            // páginas recém-prensadas flutuando bem acima das estantes
            kit.FloatingPages("Sobre as prensas", new Vector3(-7f, 5.5f, 25f), 1.5f, 12);
            kit.CeilingLight(new Vector3(-7f, 1.8f, 25f), 90f, LuzQuente);
        }

        // ------------------------------------------------------------------ 7. curva: a escolha (N7)

        void Curva(PhaseKit kit)
        {
            // x [-10, -4], z [29, 34]; saída leste z [30.5, 32.5] (rota segura) e abertura norte x [-7, -5] (rota alta)
            kit.Platform("Curva", new Vector3(-7f, YOficina, 31.5f), new Vector2(6f, 5f));
            Estante(kit, "Curva oeste", -10.3f, 28.7f, -10.3f, 34.3f, YOficina, AlturaOficina);
            Estante(kit, "Curva norte O", -10.3f, 34.3f, -7f, 34.3f, YOficina, AlturaOficina);
            Estante(kit, "Curva norte L", -5f, 34.3f, -3.7f, 34.3f, YOficina, AlturaOficina);
            Estante(kit, "Curva leste S", -3.7f, 28.7f, -3.7f, 30.5f, YOficina, AlturaOficina);
            Estante(kit, "Curva leste N", -3.7f, 32.5f, -3.7f, 34.3f, YOficina, AlturaOficina);

            // segundo Pulo Duplo: o do poço já terá acabado; este serve à rota alta e dura até o galpão
            kit.Pickup(PickupKind.DoubleJump, new Vector3(-8.5f, YOficina, 31.5f));
            kit.CeilingLight(new Vector3(-8.5f, 1.8f, 31.5f), 0f, LuzPoder);

            // dicas: sobre a saída leste (lida olhando para +X) e sobre a pilha da rota alta (lida olhando para +Z)
            kit.Sign("Leia o ritmo antes de correr", new Vector3(-4.3f, YOficina + 3.2f, 31.5f), 90f, 0.04f);
            kit.Sign("ROTA ALTA: SO COM PULO DUPLO", new Vector3(-6f, 3.6f, 34f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ 8. passarela estreita: serra + pêndulo (rota segura, N7 -> N8 -> N9)

        void PassarelaEstreita(PhaseKit kit)
        {
            // x [-4, 6], z [30.5, 32.5]: 2 m de largura sobre o vazio
            kit.Platform("Passarela estreita", new Vector3(1f, YOficina, 31.5f), new Vector2(10f, 2f));

            // serra em x = -1 cruzando a passarela em Z (de 28,5 a 34,5) a cada 3,2 s, sobre trilhos dos dois lados
            kit.Platform("Trilho da serra sul", new Vector3(-1f, YOficina, 29f), new Vector2(0.8f, 3f));
            kit.Platform("Trilho da serra norte", new Vector3(-1f, YOficina, 34f), new Vector2(0.8f, 3f));
            kit.Saw("Serra da passarela", new Vector3(-1f, YOficina, 28.5f), new Vector3(0f, 0f, 6f), 3.2f);

            // pêndulo em x = 3,5 balançando em Z (yaw 90), com offset diferente do da galeria
            kit.Pendulum("Pendulo da passarela", new Vector3(3.5f, YOficina, 31.5f), 50f, 2.4f, 0.6f, 90f);
            kit.Platform("Viga do pendulo da passarela", new Vector3(3.5f, YOficina + 4.3f, 31.5f), new Vector2(0.6f, 6.4f));
            kit.Pillar("Pilar da viga", new Vector3(3.5f, -7.5f, 28f), 10f);

            // o bolsão seguro entre as duas armadilhas (x de 0,1 a 2,6) fica sob uma luz quente
            kit.CeilingLight(new Vector3(1.3f, 1.8f, 31.5f), 90f, LuzQuente);
            kit.CeilingLight(new Vector3(-1f, 1.8f, 31.5f), 90f, LuzAlerta);
        }

        // ------------------------------------------------------------------ 9. rota alta: pilhas de livros sobre o vazio (H0 -> H1 -> H2)

        void RotaAlta(PhaseKit kit)
        {
            // +1,7 m sobre a curva; vãos de 4,4 m (um pulo alcança no máximo 4,16 m, o pulo duplo 5,66 m)
            kit.BookPlatform("Pilha alta H0", new Vector3(-6f, YAlta, 35.4f), new Vector2(2f, 2.8f), 0f, 1.7f);     // x [-7, -5]     z [34, 36.8]
            kit.BookPlatform("Pilha alta H1", new Vector3(0.6f, YAlta, 36.8f), new Vector2(2.4f, 2f), 0f, 2.5f);    // x [-0.6, 1.8]  z [35.8, 37.8]
            kit.BookPlatform("Pilha alta H2", new Vector3(7.4f, YAlta, 36.8f), new Vector2(2.4f, 2f), 0f, 1.7f);    // x [6.2, 8.6]   z [35.8, 37.8]
            kit.CeilingLight(new Vector3(-6f, 4.4f, 35.6f), 0f, LuzPoder);
            kit.CeilingLight(new Vector3(0.6f, 4.4f, 36.8f), 0f, LuzPoder);
            kit.CeilingLight(new Vector3(7.4f, 4.4f, 36.8f), 0f, LuzPoder);
            // livros voando muito abaixo dos vãos
            kit.FloatingBooks("Vazio da rota alta", new Vector3(3.5f, -6f, 37f), 2.5f, 6);
        }

        // ------------------------------------------------------------------ 10. galpão da escalada (N10) e rota de queda

        void Galpao(PhaseKit kit)
        {
            // x [6, 14], z [16, 38]: fica sob toda a escadaria; quem cai do degrau volta para cá
            kit.Platform("Galpao da escalada", new Vector3(10f, YOficina, 27f), new Vector2(8f, 22f));
            Estante(kit, "Galpao leste", 14.3f, 15.7f, 14.3f, 38.3f, YOficina, 3.5f);
            Estante(kit, "Galpao norte", 5.7f, 38.3f, 14.3f, 38.3f, YOficina, 3.5f);
            Estante(kit, "Galpao sul", 5.7f, 15.7f, 14.3f, 15.7f, YOficina, 3f); // topo encosta sob o mezanino (+1,2)
            // guarda-corpo baixo do lado do vazio; abertura z [30.5, 32.5] para a passarela estreita
            Estante(kit, "Galpao oeste S", 5.7f, 15.7f, 5.7f, 30.5f, YOficina, 1.2f);
            Estante(kit, "Galpao oeste N", 5.7f, 32.5f, 5.7f, 38.3f, YOficina, 1.2f);

            // mesas de encadernação longe da escadaria (z >= 28)
            kit.ReadingDesk("Mesa de encadernacao 1", new Vector3(13.2f, YOficina, 29f), 90f);
            kit.ReadingDesk("Mesa de encadernacao 2", new Vector3(13.2f, YOficina, 33f), 90f);
            kit.ReadingLamp(new Vector3(13.5f, YOficina, 37.5f), -135f);
            kit.CeilingLight(new Vector3(10f, 1.4f, 32f));
            kit.CeilingLight(new Vector3(10f, 1.4f, 25f), 0f, LuzPoder);
        }

        // ------------------------------------------------------------------ 11. escadaria de degraus altos (N11)

        void Escalada(PhaseKit kit)
        {
            // terceiro Pulo Duplo: ao pé da escadaria, para quem chegou pela passarela estreita
            kit.Pickup(PickupKind.DoubleJump, new Vector3(11f, YOficina, 25f));

            // sobe em -Z: galpão (-1,8) -> degrau (-0,05) -> mezanino (+1,7); vão de 1,2 m entre o degrau e o mezanino
            kit.BookPlatform("Degrau alto", new Vector3(11f, YDegrau, 19.5f), new Vector2(4f, 3f), 0f, 1.2f); // x [9, 13] z [18, 21]
            // estante sob o degrau: a face (z = 21) fica inteira, sem fresta por baixo; topo rente à parte de baixo do degrau
            Estante(kit, "Sob o degrau", 9f, 20.7f, 13f, 20.7f, YOficina, 1.25f);
            kit.Sign("SUBA COM PULO DUPLO", new Vector3(11f, 3f, 21.1f), 180f, 0.04f);
            kit.CeilingLight(new Vector3(11f, 3.4f, 19.5f));
        }

        // ------------------------------------------------------------------ marcos no horizonte

        void Horizonte(PhaseKit kit)
        {
            // relógio gigante acima e atrás da porta: visto da entrada, marca o objetivo
            kit.DreamClock("Oficina", new Vector3(0f, 9f, 22f), 3f, 0f, 5f);
            kit.FloatingBooks("Alto do poco", new Vector3(-7f, 7f, -5f), 2.5f, 6);
            kit.FloatingPages("Alto do galpao", new Vector3(17.5f, 5f, 27f), 2f, 12);
        }

        // ------------------------------------------------------------------ utilitários

        /// <summary>Estante sólida (parede de 0,6 m) de (x1, z1) até (x2, z2), com base em <paramref name="y"/>.</summary>
        static void Estante(PhaseKit kit, string nome, float x1, float z1, float x2, float z2, float y, float altura) =>
            kit.Bookshelf(nome, new Vector3(x1, y, z1), new Vector3(x2, y, z2), altura);
    }
}
