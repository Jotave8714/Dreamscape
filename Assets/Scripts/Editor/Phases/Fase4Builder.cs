using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 4 — "Bloco de Aulas" (tema âmbar). Mecânica central: pisos que cedem 0,8 s depois de pisados.
    /// No sonho, as salas de aula viram salas de leitura: a primeira sala ainda é escola (paredes, carteiras),
    /// e a cada trecho as paredes viram estantes, as carteiras viram mesas de leitura e o chão vira livro.
    ///
    /// Percurso em +Z (de z = 0 a z = 70,5), com um desvio curto para +X que guarda o power-up Velocidade:
    ///  1. Sala de aula (entrada, carteiras)                 z  -2 .. 3
    ///  2. Travessia de 3 pisos que cedem (2,5 m cada)       z   3 .. 10,5  — andando cai, correndo passa
    ///  3. Corredor de estantes com espinhos em ciclo (onda) z 10,5 .. 21
    ///  4. Sala de leitura de espera (bifurcação / truque)   z  21 .. 27
    ///  5. Sala lateral com Velocidade (desvio)              x   4 .. 7
    ///  6. Corredor sob o livro gigante: 3 placas de 5 m     z 27 .. 47 — só passa com Velocidade
    ///  7. Salão de leitura (patamar)                        z  47 .. 51
    ///  8. Varanda do escudo (opcional, salto de 3,5 m)      x -11,5 .. -7,5
    ///  9. Subida até o patamar do pêndulo (y = 2), por duas rotas:
    ///     a. curta e arriscada: arquibancada de 3 degraus que cedem (z 52,5 .. 58,5);
    ///     b. longa e segura: escada de pilhas de livros pela direita (x 5,5 .. 8,5), 4 pulos de 1,5 m.
    /// 10. Patamar do pêndulo (observar o ritmo)             z 58,5 .. 61,5 (y = 2)
    /// 11. Ponte de livro do pêndulo                         z 61,5 .. 67,5
    /// 12. Saída: livro aberto gigante com a porta           z 67,5 .. 71,5
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
            EscadaDeLivros(kit);
            PenduloESaida(kit);

            // marco no horizonte: atrás da porta, visível desde o spawn pelo eixo do percurso
            kit.DreamClock("Horizonte", new Vector3(0f, 10f, 76f), 3f, 0f, 3f);

            // abaixo de tudo: qualquer queda reinicia a fase
            kit.FallZone(-8f);
            // moldura de estantes gigantes cobrindo x -12..9,5 e z -3..72
            kit.LibraryBackdrop(new Vector2(-12f, -3f), new Vector2(9.5f, 72f), -5f, 15f, 6f);
        }

        // ------------------------------------------------------------------ 1. sala de aula (entrada)

        static void Entrada(PhaseKit kit)
        {
            // ainda é escola: piso, paredes e carteiras de sala de aula
            kit.Platform("Sala de aula - entrada", new Vector3(0f, 0f, 0.5f), new Vector2(8f, 5f));
            kit.Spawn(new Vector3(0f, 0f, 0f), 0f);

            kit.Wall("Parede - fundo da sala", new Vector3(-4f, 0f, -2f), new Vector3(4f, 0f, -2f));
            kit.Wall("Parede - sala esquerda", new Vector3(-4f, 0f, -2f), new Vector3(-4f, 0f, 3f));
            kit.Wall("Parede - sala direita", new Vector3(4f, 0f, -2f), new Vector3(4f, 0f, 3f));
            // frente: as paredes vão até os pés do arco; a abertura de 3 m fica alinhada com a travessia
            kit.Wall("Parede - frente esquerda", new Vector3(-4f, 0f, 3f), new Vector3(-2.1f, 0f, 3f));
            kit.Wall("Parede - frente direita", new Vector3(2.1f, 0f, 3f), new Vector3(4f, 0f, 3f));
            // ...mas a porta da sala já é um arco de estantes: o sonho começa ali
            kit.ShelfArch("Porta da sala", new Vector3(0f, 0f, 3f), 3f, 3.6f, 0f);

            // carteiras: pilares baixos em fileiras, deixando o corredor central livre
            float[] fileiras = { -0.6f, 1.6f };
            for (int i = 0; i < fileiras.Length; i++)
            {
                kit.Pillar($"Carteira esquerda {i + 1}", new Vector3(-2.7f, 0f, fileiras[i]), 0.75f);
                kit.Pillar($"Carteira direita {i + 1}", new Vector3(2.7f, 0f, fileiras[i]), 0.75f);
            }

            kit.CeilingLight(new Vector3(-2f, 3.2f, 0.5f));
            kit.CeilingLight(new Vector3(2f, 3.2f, 0.5f));
            kit.ReadingLamp(new Vector3(-4f, 3f, 1f), 0f);
            // páginas fugindo da sala por cima das paredes (acima de 4 m)
            kit.FloatingPages("Sala de aula", new Vector3(0f, 5.2f, 0.5f), 2.5f, 12);

            // no lintel do arco (face sul em z = 2,35)
            kit.Sign("FASE 4 - BLOCO DE AULAS", new Vector3(0f, 4.4f, 2.3f), 0f, 0.07f);
            kit.Sign("PISO QUE TREME CAI - CORRA", new Vector3(0f, 3.75f, 2.3f), 0f, 0.03f);
        }

        // ------------------------------------------------------------------ 2. travessia de pisos que cedem

        static void TravessiaQueCede(PhaseKit kit)
        {
            // 3 placas de 3 x 2,5 m sobre o vazio. Cada uma cede 0,8 s depois de pisada:
            // correndo (5 m/s) fica no máximo 0,59 s em cada; andando (2,2 m/s) fica 1,08 s e cai.
            float[] centros = { 4.25f, 6.75f, 9.25f };
            for (int i = 0; i < centros.Length; i++)
                kit.CrumblingPlatform($"Piso que cede {i + 1}", new Vector3(0f, 0f, centros[i]), new Vector2(3f, 2.5f));

            // estantes baixas nas bordas (face interna em x = ±1,35): o jogador não anda na beirada fora da área
            // que detecta o passo (de z 3 a 3,6 a borda é o pé do arco, face em ±1,5). Na saída, as estantes do corredor de espinhos (face em ±0,95) fazem o funil.
            kit.Bookshelf("Travessia esquerda", new Vector3(-1.65f, 0f, 3.6f), new Vector3(-1.65f, 0f, 10.5f), 1.4f);
            kit.Bookshelf("Travessia direita", new Vector3(1.65f, 0f, 3.6f), new Vector3(1.65f, 0f, 10.5f), 1.4f);

            kit.CeilingLight(new Vector3(0f, 3.2f, 6.75f));
            // livros voando dos dois lados, a mais de 2 m da estante
            kit.FloatingBooks("Travessia oeste", new Vector3(-5f, 1f, 7f), 1.5f, 5);
            kit.FloatingBooks("Travessia leste", new Vector3(5f, 1.5f, 8f), 1.5f, 5);
        }

        // ------------------------------------------------------------------ 3. corredor de espinhos em ciclo

        static void CorredorDeEspinhos(PhaseKit kit)
        {
            // 2,2 m de piso; as estantes (face em ±0,95) não deixam faixa lateral livre dos espinhos (gatilho de 1,8 m)
            kit.Platform("Corredor de espinhos", new Vector3(0f, 0f, 15.75f), new Vector2(2.2f, 10.5f));
            kit.Bookshelf("Espinhos esquerda", new Vector3(-1.25f, 0f, 10.5f), new Vector3(-1.25f, 0f, 21f), 2.4f);
            kit.Bookshelf("Espinhos direita", new Vector3(1.25f, 0f, 10.5f), new Vector3(1.25f, 0f, 21f), 2.4f);

            // ciclo de 2,7 s (1,2 s expostos, 1,5 s recolhidos); a onda anda para frente 0,9 s por campo.
            // Entre os campos há refúgios seguros (z 14,4..15,85 e 17,65..19,1), marcados por tapetes.
            kit.Spikes("Espinhos 1", new Vector3(0f, 0f, 13.5f), true, 1.2f, 1.5f, 1.8f);
            kit.Spikes("Espinhos 2", new Vector3(0f, 0f, 16.75f), true, 1.2f, 1.5f, 0.9f);
            kit.Spikes("Espinhos 3", new Vector3(0f, 0f, 20f), true, 1.2f, 1.5f, 0f);
            kit.Carpet(new Vector3(0f, 0f, 15.12f), new Vector2(1.6f, 1.2f), 0f);
            kit.Carpet(new Vector3(0f, 0f, 18.37f), new Vector2(1.6f, 1.2f), 0f);

            kit.CeilingLight(new Vector3(0f, 3.2f, 14.5f));
            kit.CeilingLight(new Vector3(0f, 3.2f, 18.5f));
            kit.ReadingLamp(new Vector3(-1.25f, 2.4f, 12f), 0f);
            kit.ReadingLamp(new Vector3(1.25f, 2.4f, 19.5f), 180f);
        }

        // ------------------------------------------------------------------ 4/5. sala de espera e sala lateral

        static void SalaDeEspera(PhaseKit kit)
        {
            // a sala de aula já virou sala de leitura: estantes nas paredes e mesas com vela
            kit.Platform("Sala de espera", new Vector3(0f, 0f, 24f), new Vector2(8f, 6f));

            kit.Bookshelf("Espera fundo esquerda", new Vector3(-4.6f, 0f, 20.7f), new Vector3(-1.55f, 0f, 20.7f), 3f);
            kit.Bookshelf("Espera fundo direita", new Vector3(1.55f, 0f, 20.7f), new Vector3(4.3f, 0f, 20.7f), 3f);
            kit.Bookshelf("Espera esquerda", new Vector3(-4.3f, 0f, 20.4f), new Vector3(-4.3f, 0f, 27.6f), 3f);
            // frente: a abertura de 2,3 m (faces em ±1,15) é a boca do corredor de forro baixo (o caminho "óbvio")
            kit.Bookshelf("Espera frente esquerda", new Vector3(-4.6f, 0f, 27.3f), new Vector3(-1.75f, 0f, 27.3f), 3f);
            kit.Bookshelf("Espera frente direita", new Vector3(1.75f, 0f, 27.3f), new Vector3(4.6f, 0f, 27.3f), 3f);
            // lado direito: estantes baixas nas pontas, abertura de 4 m (z 22..26) para a sala lateral
            kit.Bookshelf("Espera direita fundo", new Vector3(4.3f, 0f, 20.4f), new Vector3(4.3f, 0f, 22f), 1.4f);
            kit.Bookshelf("Espera direita frente", new Vector3(4.3f, 0f, 26f), new Vector3(4.3f, 0f, 27.6f), 1.4f);

            kit.ReadingDesk("Espera 1", new Vector3(-2.7f, 0f, 22.6f), 90f);
            kit.ReadingDesk("Espera 2", new Vector3(-2.7f, 0f, 25.4f), 90f);
            // o friso do tapete corre em X e aponta para a sala lateral
            kit.Carpet(new Vector3(0.5f, 0f, 24f), new Vector2(4f, 3f), 0f);

            kit.CeilingLight(new Vector3(-2f, 3.2f, 24f));
            kit.CeilingLight(new Vector3(2f, 3.2f, 24f));
            kit.ReadingLamp(new Vector3(-4.3f, 3f, 24f), 0f);

            // dica do truque: aponta para a sala lateral (+X)
            kit.Sign("ATRASADO? ->", new Vector3(2.65f, 1.8f, 26.95f), 0f, 0.045f);

            // sala lateral (desvio curto) com o power-up Velocidade
            kit.Platform("Sala lateral", new Vector3(5.5f, 0f, 24f), new Vector2(3f, 4f));
            kit.Bookshelf("Lateral fundo", new Vector3(7.3f, 0f, 21.4f), new Vector3(7.3f, 0f, 26.6f), 3f);
            kit.Bookshelf("Lateral sul", new Vector3(4.6f, 0f, 21.7f), new Vector3(7.6f, 0f, 21.7f), 3f);
            kit.Bookshelf("Lateral norte", new Vector3(4.6f, 0f, 26.3f), new Vector3(7.6f, 0f, 26.3f), 3f);
            kit.Pickup(PickupKind.Speed, new Vector3(5.6f, 0f, 24f));
            kit.Pole(new Vector3(6.4f, 0f, 25.4f), Ambar);
            kit.ReadingLamp(new Vector3(7.3f, 3f, 23f), 0f);
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

            // forro a 2,0 m do piso: um único livro gigante deitado (0,5 m, topo em 2,5). O jogador (1,76 m)
            // quase não sobe ao pular, então pular não encurta o tempo sobre as placas
            kit.BookPlatform("Forro rebaixado", new Vector3(0f, 2.5f, 37f), new Vector2(3f, 20f), 0f, 0.3f);

            // estantes até o forro, face interna em ±1,15 (a placa vai a ±1,5): a cápsula sempre cai na área que
            // detecta o passo
            kit.Bookshelf("Corredor esquerda", new Vector3(-1.45f, 0f, 27f), new Vector3(-1.45f, 0f, 47f), 2f);
            kit.Bookshelf("Corredor direita", new Vector3(1.45f, 0f, 27f), new Vector3(1.45f, 0f, 47f), 2f);

            // luminárias embutidas no forro
            kit.CeilingLight(new Vector3(0f, 2.2f, 30f));
            kit.CeilingLight(new Vector3(0f, 2.2f, 37f));
            kit.CeilingLight(new Vector3(0f, 2.2f, 44f));
        }

        // ------------------------------------------------------------------ 7/8. salão de leitura e varanda do escudo

        static void PatamarEVarandaDoEscudo(PhaseKit kit)
        {
            // salão amplo depois do corredor apertado: daqui se vê a subida, o pêndulo, a porta e o relógio
            kit.Platform("Patamar do corredor", new Vector3(0f, 0f, 49f), new Vector2(8f, 4f));
            kit.Carpet(new Vector3(0f, 0f, 49f), new Vector2(2.4f, 3.6f), 0f);
            kit.ReadingDesk("Salao de leitura", new Vector3(2.4f, 0f, 50.5f), 0f);
            // arco de estantes na borda direita: emoldura a rota longa (escada de livros)
            kit.ShelfArch("Rota dos livros", new Vector3(4f, 0f, 49f), 2f, 3.6f, 90f);
            kit.CeilingLight(new Vector3(0f, 3.2f, 49f));
            kit.FloatingBooks("Salao de leitura", new Vector3(0f, 7.5f, 53f), 4f, 8);

            // recompensa opcional: vão de 3,5 m para -X (só com Velocidade: alcance 4,97 m; sem ela 3,32 m)
            kit.OpenBookPlatform("Varanda do escudo", new Vector3(-9.5f, 0f, 49f), new Vector2(4f, 3f), 0f);
            kit.Pickup(PickupKind.Shield, new Vector3(-9.5f, 0f, 49f));
            kit.Pole(new Vector3(-10.8f, 0f, 50f), Ambar);
            kit.FloatingPages("Varanda do escudo", new Vector3(-9.5f, 4.5f, 49f), 2f, 8);
        }

        // ------------------------------------------------------------------ 9a. arquibancada que cede (rota curta)

        static void ArquibancadaQueCede(PhaseKit kit)
        {
            // vão de 1,5 m e depois 3 degraus colados que cedem, subindo 0,5 m cada (acima do degrau automático de 0,3 m)
            kit.CrumblingPlatform("Degrau que cede 1", new Vector3(0f, 0.5f, 53.5f), new Vector2(2.5f, 2f));
            kit.CrumblingPlatform("Degrau que cede 2", new Vector3(0f, 1.0f, 55.5f), new Vector2(2.5f, 2f));
            kit.CrumblingPlatform("Degrau que cede 3", new Vector3(0f, 1.5f, 57.5f), new Vector2(2.5f, 2f));
            kit.CeilingLight(new Vector3(0f, 4.2f, 55.5f));
        }

        // ------------------------------------------------------------------ 9b. escada de livros (rota longa e segura)

        static void EscadaDeLivros(PhaseKit kit)
        {
            // 4 pulos de 1,5 m subindo 0,5 m (alcance 2,88 m; com Velocidade 4,32 m, que ainda pousa dentro de cada pilha)
            kit.BookPlatform("Escada de livros 1", new Vector3(7f, 0.5f, 49f), new Vector2(3f, 2.5f));
            kit.BookPlatform("Escada de livros 2", new Vector3(7f, 1f, 53.25f), new Vector2(3f, 3f));
            kit.BookPlatform("Escada de livros 3", new Vector3(7f, 1.5f, 57.75f), new Vector2(3f, 3f));
            // o último degrau encosta na borda sul do patamar do pêndulo (z = 58,5, x 1,5..4)
            kit.BookPlatform("Escada de livros 4", new Vector3(2.75f, 2f, 57.5f), new Vector2(2.5f, 2f));
            // estante alta do lado de fora: amparo contra pulo longo demais e parede da rota
            kit.Bookshelf("Escada de livros fora", new Vector3(8.8f, 0f, 47.5f), new Vector3(8.8f, 0f, 59.5f), 4.5f);
            kit.CeilingLight(new Vector3(7f, 4f, 49f));
            kit.CeilingLight(new Vector3(7f, 4.5f, 53.25f));
            kit.CeilingLight(new Vector3(7f, 5f, 57.75f));
            kit.ReadingLamp(new Vector3(8.8f, 4.5f, 51f), 0f);
            kit.ReadingLamp(new Vector3(8.8f, 4.5f, 56f), 0f);
        }

        // ------------------------------------------------------------------ 10-12. pêndulo e saída

        static void PenduloESaida(PhaseKit kit)
        {
            // patamar firme de onde se vê o pêndulo balançar antes de atravessar (x -2..4: recebe as duas rotas)
            kit.BookPlatform("Patamar do pendulo", new Vector3(1f, 2f, 60f), new Vector2(6f, 3f));
            kit.Pole(new Vector3(-1.7f, 2f, 61.1f), Ambar);
            // estantes nas quinas norte: só a boca da ponte (x ±1,3) fica aberta. Sem elas, com Velocidade,
            // um pulo de quina a quina (6,08 m) até a saída ficaria perto do limite teórico e pularia o pêndulo
            kit.Bookshelf("Patamar norte E", new Vector3(-2.3f, 2f, 61.8f), new Vector3(-1.3f, 2f, 61.8f), 3f);
            kit.Bookshelf("Patamar norte D", new Vector3(1.3f, 2f, 61.8f), new Vector3(4.3f, 2f, 61.8f), 3f);

            // ponte estreita (2 m), um livro gigante sobre o vazio, com um único pêndulo balançando de lado (plano X)
            kit.BookPlatform("Ponte do pendulo", new Vector3(0f, 2f, 64.5f), new Vector2(2f, 6f), 0f, 0.6f);
            kit.Pendulum("Pendulo", new Vector3(0f, 2f, 64.5f), 55f, 2.4f, 0f, 0f);

            // saída: livro aberto gigante, emoldurado por um arco de estantes
            kit.OpenBookPlatform("Saida", new Vector3(0f, 2f, 69.5f), new Vector2(6f, 4f), 0f);
            kit.ShelfArch("Portal da saida", new Vector3(0f, 2f, 68.2f), 2.6f, 3.6f, 0f);
            kit.Bookshelf("Saida fundo", new Vector3(-3.3f, 2f, 71.8f), new Vector3(3.3f, 2f, 71.8f), 4f);
            kit.Pole(new Vector3(2.4f, 2f, 70.8f), Ambar);
            kit.ReadingLamp(new Vector3(-2.2f, 6f, 71.8f), 90f);
            kit.ReadingLamp(new Vector3(2.2f, 6f, 71.8f), 90f);
            kit.Goal(new Vector3(0f, 2f, 70.5f), 0f);
        }
    }
}
