using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Menu "Dreamscape > Configurar animações do protagonista": configura o modelo e os FBX do Mixamo como Humanoid,
    /// corta o pulo em subida, queda e aterrissagem e gera o Animator Controller. Pode rodar de novo sem duplicar nada.
    /// Os FBX precisam ser baixados com skin: sem ela não há T-pose, a referência vira o primeiro quadro da animação
    /// e o joelho do nosso modelo chega cerca de 30° menos dobrado, chegando a inverter.
    /// </summary>
    public static class ProtagonistAnimationSetup
    {
        const string ModelPath = "Assets/Art/Characters/Protagonist/Protagonist.fbx";
        const string MoemaModelPath = "Assets/Art/Characters/Moema/Moema.fbx";
        const string Folder = "Assets/Art/Characters/Protagonist/Animations/";
        const string ControllerPath = "Assets/Resources/ProtagonistAnimator.controller";
        const string MoemaControllerPath = "Assets/Resources/MoemaAnimator.controller";

        // Velocidade (m/s) medida em cada animação; a reprodução é ajustada para os pés acompanharem o chão.
        const float WalkClipSpeed = 1.66f;
        const float JogClipSpeed = 2.87f;
        const float RunClipSpeed = 5.48f;

        // Velocidades do CharacterMotor do protagonista, com e sem o power-up.
        const float WalkSpeed = 2.2f;
        const float BoostedWalkSpeed = 3.3f;
        const float RunSpeed = 5f;
        const float BoostedRunSpeed = 7.5f;

        // Velocidades da Moema no CharacterMotor. A perna dela é mais curta, então cada passada cobre menos chão
        // (fração medida no jogo); sem compensar no ritmo, os pés deslizam.
        const float MoemaWalkSpeed = 2.3f;
        const float MoemaJogSpeed = 4f;
        const float MoemaRunSpeed = 6.5f;
        const float MoemaWalkStride = 0.87f;
        const float MoemaJogStride = 0.82f;
        const float MoemaRunStride = 0.79f;

        // A caminhada deixa os pés um pouco altos no nosso modelo; valor positivo (m) desce o corpo.
        const float WalkHeightOffset = 0.02f;

        // Quadros do Jump.fbx (30 fps). O preparo antes de sair do chão fica de fora: no jogo o pulo é imediato.
        const float RiseStart = 15f, RiseEnd = 24f;
        const float FallStart = 24f, FallEnd = 32f;
        const float LandStart = 33f, LandEnd = 52f;

        // A queda começa no topo do salto da animação; desce o corpo (m) para encaixar com a subida e a aterrissagem.
        const float FallHeightOffset = 0.6f;

        [MenuItem("Dreamscape/Configurar animações do protagonista")]
        public static void Setup()
        {
            ImportCharacter(ModelPath);
            ImportCharacter(MoemaModelPath);

            ImportLoop("Run", 0f);
            ImportLoop("Walk", WalkHeightOffset);
            ImportLoop("Jog", 0f);
            ImportLoop("Idle", 0f);
            ImportJump();

            BuildController(ControllerPath, new[]
            {
                Child(LoadClip("Idle", "Idle"), 0f, 1f),
                Child(LoadClip("Walk", "Walk"), WalkSpeed, WalkSpeed / WalkClipSpeed),
                // cada velocidade usa a animação de ritmo natural mais próximo, para não parecer acelerada
                Child(LoadClip("Jog", "Jog"), BoostedWalkSpeed, BoostedWalkSpeed / JogClipSpeed),
                Child(LoadClip("Run", "Run"), RunSpeed, RunSpeed / RunClipSpeed),
                Child(LoadClip("Run", "Run"), BoostedRunSpeed, BoostedRunSpeed / RunClipSpeed),
            });
            BuildController(MoemaControllerPath, new[]
            {
                Child(LoadClip("Idle", "Idle"), 0f, 1f),
                Child(LoadClip("Walk", "Walk"), MoemaWalkSpeed, MoemaWalkSpeed / (WalkClipSpeed * MoemaWalkStride)),
                Child(LoadClip("Jog", "Jog"), MoemaJogSpeed, MoemaJogSpeed / (JogClipSpeed * MoemaJogStride)),
                Child(LoadClip("Run", "Run"), MoemaRunSpeed, MoemaRunSpeed / (RunClipSpeed * MoemaRunStride)),
            });
            Debug.Log("Dreamscape: animações do protagonista configuradas.");
        }

        static void ImportCharacter(string path)
        {
            var model = (ModelImporter)AssetImporter.GetAtPath(path);
            model.animationType = ModelImporterAnimationType.Human;
            model.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            model.SaveAndReimport();
        }

        static void ImportLoop(string file, float heightOffset)
        {
            ModelImporter importer = HumanoidImporter(file);
            ModelImporterClipAnimation clip = importer.defaultClipAnimations[0];
            clip.name = file;
            clip.loopTime = true;
            clip.loopPose = true;
            BakeRoot(clip, bakeHeight: true);
            clip.heightOffset = heightOffset;
            importer.clipAnimations = new[] { clip };
            importer.SaveAndReimport();
        }

        static void ImportJump()
        {
            ModelImporter importer = HumanoidImporter("Jump");
            string take = importer.defaultClipAnimations[0].takeName;
            importer.clipAnimations = new[]
            {
                // no ar quem controla a altura é o motor; a altura da animação somaria com a dele
                JumpClip("Jump Rise", take, RiseStart, RiseEnd, bakeHeight: false, 0f),
                JumpClip("Jump Fall", take, FallStart, FallEnd, bakeHeight: false, FallHeightOffset),
                // na aterrissagem o corpo precisa descer com a animação, senão os pés flutuam ao dobrar os joelhos
                JumpClip("Jump Land", take, LandStart, LandEnd, bakeHeight: true, 0f),
            };
            importer.SaveAndReimport();
        }

        static ModelImporterClipAnimation JumpClip(string name, string take, float first, float last, bool bakeHeight, float heightOffset)
        {
            var clip = new ModelImporterClipAnimation
            {
                name = name,
                takeName = take,
                firstFrame = first,
                lastFrame = last,
                heightOffset = heightOffset,
            };
            BakeRoot(clip, bakeHeight);
            return clip;
        }

        static ModelImporter HumanoidImporter(string file)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(Folder + file + ".fbx");
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.materialImportMode = ModelImporterMaterialImportMode.None; // só o movimento interessa, não o boneco
            return importer;
        }

        // Quem desloca o personagem é o CharacterMotor. O deslocamento horizontal e o giro ficam na raiz, que o motor
        // ignora: gravados na pose, fariam o corpo escorregar para os lados, porque os clipes não correm em linha reta.
        static void BakeRoot(ModelImporterClipAnimation clip, bool bakeHeight)
        {
            clip.lockRootRotation = false;
            clip.lockRootPositionXZ = false;
            clip.lockRootHeightY = bakeHeight;
            clip.keepOriginalPositionY = true;
        }

        static AnimationClip LoadClip(string file, string name) =>
            AssetDatabase.LoadAllAssetsAtPath(Folder + file + ".fbx").OfType<AnimationClip>().First(c => c.name == name);

        static void BuildController(string controllerPath, ChildMotion[] locomotionClips)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.DeleteAsset(controllerPath);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorState locomotion = controller.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
            foreach (AnimatorControllerParameter stray in controller.parameters)
                controller.RemoveParameter(stray); // a criação da blend tree adiciona um parâmetro próprio

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            tree.children = locomotionClips;

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState rise = machine.AddState("Jump Rise");
            rise.motion = LoadClip("Jump", "Jump Rise");
            AnimatorState fall = machine.AddState("Jump Fall");
            fall.motion = LoadClip("Jump", "Jump Fall");
            AnimatorState land = machine.AddState("Jump Land");
            land.motion = LoadClip("Jump", "Jump Land");
            machine.defaultState = locomotion;

            // o pulo duplo reinicia a subida, por isso a transição pode voltar para o próprio estado
            AnimatorStateTransition toRise = machine.AddAnyStateTransition(rise);
            Configure(toRise, 0.05f);
            toRise.canTransitionToSelf = true;
            toRise.AddCondition(AnimatorConditionMode.If, 0f, "Jump");

            AnimatorStateTransition riseToFall = rise.AddTransition(fall);
            Configure(riseToFall, 0.1f);
            riseToFall.AddCondition(AnimatorConditionMode.Less, 0f, "VerticalSpeed");

            // sair de uma plataforma sem pular; o motor mantém -2 no chão para colar em rampas
            AnimatorStateTransition walkOff = locomotion.AddTransition(fall);
            Configure(walkOff, 0.15f);
            walkOff.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            walkOff.AddCondition(AnimatorConditionMode.Less, -3f, "VerticalSpeed");

            AnimatorStateTransition riseToLand = rise.AddTransition(land);
            Configure(riseToLand, 0.05f);
            riseToLand.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            riseToLand.AddCondition(AnimatorConditionMode.Less, 0f, "VerticalSpeed");

            AnimatorStateTransition fallToLand = fall.AddTransition(land);
            Configure(fallToLand, 0.05f);
            fallToLand.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");

            // em movimento não faz a aterrissagem inteira, senão ele deslizaria agachado
            AnimatorStateTransition landMoving = land.AddTransition(locomotion);
            Configure(landMoving, 0.12f);
            landMoving.AddCondition(AnimatorConditionMode.Greater, 0.5f, "Speed");

            AnimatorStateTransition landDone = land.AddTransition(locomotion);
            Configure(landDone, 0.15f);
            landDone.hasExitTime = true;
            landDone.exitTime = 0.85f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        static ChildMotion Child(AnimationClip clip, float threshold, float timeScale) =>
            new ChildMotion { motion = clip, threshold = threshold, timeScale = timeScale };

        static void Configure(AnimatorStateTransition transition, float duration)
        {
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = duration;
        }
    }
}
