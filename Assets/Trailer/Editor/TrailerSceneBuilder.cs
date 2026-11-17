using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;
using System.Linq;
using Dreamscape.Characters;
public static class TrailerSceneBuilder {
 static Material wall,lower,stone,metal,wood,black,lightmat,blue,gold,glass;
 static string dir="Assets/Trailer/Generated";
 static Vector3 V(float x,float y,float z){return new Vector3(x,y,z);}
 static Material Mat(string name,Color c,bool texture=false,float smooth=.15f){
 var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.color=c;m.SetFloat("_Smoothness",smooth);
 if(texture){var t=new Texture2D(256,256);t.wrapMode=TextureWrapMode.Repeat;for(int y=0;y<256;y++)for(int x=0;x<256;x++){float n=Mathf.PerlinNoise(x*.15f,y*.15f)*.035f+.965f;float grout=(name.Contains("tiles")&&(x<2||y<2))? .68f:1f;t.SetPixel(x,y,new Color(n*grout,n*grout,n*grout,1));}t.Apply();AssetDatabase.CreateAsset(t,dir+"/"+name+"-tiles.asset");m.mainTexture=t;}
 AssetDatabase.CreateAsset(m,dir+"/"+name+".mat");return m;}
 static GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool collider=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.position=p;g.transform.localScale=s;var ren=g.GetComponent<Renderer>();ren.sharedMaterial=m;if(m.mainTexture){var pb=new MaterialPropertyBlock();float tiles=Mathf.Max(s.x,s.y,s.z)/1.5f;pb.SetVector("_BaseMap_ST",new Vector4(tiles,tiles,0,0));ren.SetPropertyBlock(pb);}if(!collider)Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
 static void Label(string text,Vector3 p,float size,Color c,Quaternion rot){var g=new GameObject(text);g.transform.position=p;g.transform.rotation=rot;var tm=g.AddComponent<TextMesh>();tm.text=text;tm.fontSize=80;tm.characterSize=size;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=c;}
 static void Lamp(Vector3 p){Box("Ceiling fitting",p,V(1.8f,.1f,.65f),metal,false);Box("Fluorescent diffuser",p-V(0,.065f,0),V(1.65f,.035f,.50f),lightmat,false);var g=new GameObject("Soft fluorescent light");g.transform.position=p-V(0,.22f,0);var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.92f,.77f);l.intensity=4.5f;l.range=9;l.shadows=Mathf.Abs(p.z-6)<.1f?LightShadows.Soft:LightShadows.None;}
 static void Room(float x,float width,float length,Color band){
 Box("Ceiling",V(x,4.7f,length/2),V(width,.25f,length),wall);
 for(int s=-1;s<=1;s+=2){Box("Plaster wall",V(x+s*width/2,2.4f,length/2),V(.22f,4.8f,length),wall);Box("Painted dado",V(x+s*(width/2-.13f),.66f,length/2),V(.045f,1.3f,length),lower);Box("Skirting",V(x+s*(width/2-.17f),.08f,length/2),V(.08f,.16f,length),metal);
 for(float z=2;z<length;z+=4){Box("Concrete pilaster",V(x+s*(width/2-.22f),2.3f,z),V(.28f,4.6f,.35f),stone);Box("Wall service conduit",V(x+s*(width/2-.35f),3.5f,z),V(.09f,.09f,4),metal,false);}}
 for(float z=2;z<length;z+=4)Lamp(V(x,4.45f,z));
 Box("Back wall",V(x,2.4f,length+.1f),V(width,4.8f,.2f),wall);Box("Front wall",V(x,2.4f,-2),V(width,4.8f,.2f),wall);
 }
 static GameObject Door(float x,float y,float z,string name){
 Box("Door lintel",V(x,y+2.25f,z),V(1.8f,.18f,.22f),metal);Box("Door jamb",V(x-.85f,y+1.1f,z),V(.13f,2.2f,.22f),metal);Box("Door jamb",V(x+.85f,y+1.1f,z),V(.13f,2.2f,.22f),metal);
 var door=Box(name,V(x,y+1.1f,z+.05f),V(1.55f,2.1f,.12f),lower);Box("Door handle",V(x+.56f,y+1,z-.1f),V(.25f,.05f,.05f),metal,false).transform.SetParent(door.transform,true);
 Box("Door number plaque",V(x,y+2.55f,z-.14f),V(.8f,.28f,.04f),metal,false);Label(name,V(x,y+2.55f,z-.17f),.026f,Color.white,Quaternion.identity);return door;}
 static void Shelf(float x,float z){for(int i=0;i<4;i++){Box("Shelf plank",V(x,.3f+i*.55f,z),V(1.5f,.06f,.5f),wood);for(int j=0;j<5;j++)Box("Archive binder",V(x-.55f+j*.26f,.51f+i*.55f,z),V(.16f,.36f,.28f),(j%2==0)?lower:wall,false);}for(int s=-1;s<=1;s+=2)Box("Shelf upright",V(x+s*.72f,1.2f,z),V(.045f,2.4f,.5f),metal,false);}
 static void Pickup(Vector3 p,bool speed){
 var g=new GameObject(speed?"Speed collectible":"Double jump collectible");g.transform.position=p;
 for(int i=0;i<2;i++)for(int s=-1;s<=1;s+=2){var b=Box("Power chevron",p+V(s*.11f,i*.24f,0),V(.10f,.31f,.11f),speed?gold:blue,false);b.transform.rotation=Quaternion.Euler(0,0,s*-40);b.transform.SetParent(g.transform,true);}
 var trigger=g.AddComponent<SphereCollider>();trigger.radius=.65f;trigger.isTrigger=true;var rb=g.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;
 if(speed)g.AddComponent<TrailerSpeedPickup>();else g.AddComponent<Dreamscape.DoubleJumpPickup>();g.AddComponent<TrailerSpin>();
 }
 public static void Build(){
 Directory.CreateDirectory(dir);AssetDatabase.Refresh();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
 wall=Mat("Warm-plaster",new Color(.72f,.69f,.59f),true);lower=Mat("Sage-painted-metal",new Color(.31f,.39f,.34f),false,.28f);stone=Mat("Concrete-tiles",new Color(.47f,.46f,.42f),true,.25f);metal=Mat("Graphite-steel",new Color(.16f,.18f,.18f),false,.55f);wood=Mat("Library-oak",new Color(.25f,.12f,.065f));black=Mat("Dark",new Color(.025f,.035f,.045f));blue=Mat("Power-blue",new Color(.08f,.42f,.82f),false,.35f);gold=Mat("Power-speed",new Color(.9f,.54f,.12f));glass=Mat("Night-window",new Color(.025f,.045f,.09f),false,.7f);
 lightmat=Mat("Fluorescent",new Color(.9f,.87f,.70f));lightmat.EnableKeyword("_EMISSION");lightmat.SetColor("_EmissionColor",new Color(1,.92f,.75f)*2);
 RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.65f,.65f,.62f);RenderSettings.ambientEquatorColor=new Color(.43f,.43f,.40f);RenderSettings.ambientGroundColor=new Color(.24f,.24f,.22f);RenderSettings.fog=false;
 var sun=new GameObject("Ambient key").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=.25f;sun.transform.rotation=Quaternion.Euler(55,-25,0);sun.shadows=LightShadows.None;
 // Phase 1, faithful beige escape room with side platforms and deceptive central floor.
 Room(0,9,20,Color.white);Box("Entry floor",V(0,-.2f,2),V(9,.4f,8),stone);Box("Pit bottom",V(0,-6,11),V(9,.4f,12),stone);
 var trap=Box("False floor",V(1.3f,-.2f,8),V(3.2f,.4f,4),stone);var tc=new GameObject("False floor trigger");tc.transform.position=V(1.3f,.25f,7.1f);var bc=tc.AddComponent<BoxCollider>();bc.size=V(3.1f,.5f,2.2f);bc.isTrigger=true;var rb=tc.AddComponent<Rigidbody>();rb.isKinematic=true;tc.AddComponent<TrailerTrap>().floor=trap;
 Box("Side step 1",V(-2.9f,.15f,6),V(2.2f,.7f,2.2f),stone);Box("Side step 2",V(-2.9f,.65f,9.6f),V(2.2f,.7f,2.0f),stone);Box("Side step 3",V(-2.9f,1.15f,13.2f),V(2.2f,.7f,2),stone);Box("Exit landing",V(0,1.7f,17.3f),V(9,.6f,5.4f),stone);
 for(int i=0;i<3;i++)Box("Route marker",V(-2.9f,.51f+i*.5f,6+i*3.6f),V(.65f,.025f,.13f),blue,false);
 for(int j=0;j<4;j++){Box("Pit service pipe",V(3.7f,-2.8f,7+j*2),V(.18f,.18f,2),metal,false);Box("Platform edge trim",V(-2.9f,.49f+j*.5f,6+j*3.6f),V(2.2f,.05f,.14f),metal,false);}Pickup(V(-1.9f,1,2.6f),false);Door(0,2,19.7f,"01");Shelf(3.4f,2.5f);Box("Misleading direction plaque",V(2.7f,1.55f,5.8f),V(1.3f,.42f,.08f),lower,false);Label("SAÍDA  >",V(2.7f,1.55f,5.74f),.048f,Color.white,Quaternion.identity);
 // Phase 2: maintenance route, low beam, plates and an orange speed pickup.
 Room(30,8,24,Color.white);Box("Maintenance entry",V(30,-.2f,3),V(8,.4f,8),stone);Box("Maintenance pit",V(30,-6,15),V(8,.4f,18),stone);Box("Safe lateral route",V(27.3f,-.2f,10),V(2.2f,.4f,8),stone);Box("Dead end shortcut",V(31,-.2f,9),V(2,.4f,4),stone);Box("Low beam",V(27.3f,1.75f,10),V(2.3f,1.2f,.6f),metal);
 for(int i=0;i<4;i++){var tile=Box("Timed plate "+i,V(27.3f,-.12f,15+i*1.5f),V(2.1f,.24f,1.44f),metal);var t=new GameObject("Timed plate trigger "+i);t.transform.position=tile.transform.position+V(0,.35f,0);var col=t.AddComponent<BoxCollider>();col.isTrigger=true;col.size=V(2,.5f,1.3f);var r=t.AddComponent<Rigidbody>();r.isKinematic=true;var tr=t.AddComponent<TrailerTrap>();tr.floor=tile;tr.delay=.65f;}
 Box("Maintenance exit",V(30,-.2f,22),V(8,.4f,4),stone);Door(27.3f,0,23.7f,"02");Pickup(V(28.4f,1,3),true);
 for(int i=0;i<2;i++)Box("Red service pipe",V(33.7f,3+i*.3f,12),V(.15f,.15f,24),wood,false);
 // Moema room: a fixed blue sculpture on a button base opens a conventional door.
 Room(60,8,14,Color.white);Box("Store floor",V(60,-.2f,7),V(8,.4f,14),stone);Shelf(56.9f,4);Shelf(56.9f,7);Shelf(63,4);var door4=Door(60,0,13.7f,"04");var baseGo=Box("Moema activation base",V(60,.09f,7),V(1.3f,.18f,1.3f),metal);var moema=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/Moema/Moema.fbx"));moema.name="Moema blue statue";moema.transform.position=V(60,.18f,7);foreach(var rr in moema.GetComponentsInChildren<Renderer>())rr.sharedMaterials=Enumerable.Repeat(blue,rr.sharedMaterials.Length).ToArray();
 var button=Box("Moema button",V(60,.8f,5.7f),V(.32f,.25f,.12f),gold);var interact=button.AddComponent<TrailerDoorButton>();interact.door=door4;Label("E",V(60,1.1f,5.6f),.06f,Color.white,Quaternion.identity);
 // Opening: ordinary campus corridor, matching the storyboard's night and pink hoodie.
 Room(-30,8,15,Color.white);Box("Campus entrance backdrop",V(-30,2.3f,-1.7f),V(8,4.6f,.2f),wall);Box("Campus floor",V(-30,-.2f,7),V(8,.4f,18),stone);for(int i=0;i<5;i++){Box("Night window",V(-26.2f,2.3f,1+i*3),V(.06f,3.8f,2.6f),glass,false);Box("Window mullion",V(-26.3f,2.3f,2.4f+i*3),V(.1f,3.8f,.12f),metal,false);}Shelf(-33,10);Shelf(-33,12);Label("UNIFOR",V(-33.7f,2,3),.12f,Color.white,Quaternion.Euler(0,-90,0));Label("22:40",V(-33.7f,2.7f,3),.10f,new Color(1,.25f,.17f),Quaternion.Euler(0,-90,0));
 var avatar=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/Protagonist/Protagonist.fbx"));avatar.name="Carlinhos cinematic";avatar.transform.position=V(-30,0,2);avatar.AddComponent<TrailerWalkAnimation>();
 var player=new GameObject("Trailer Player");player.tag="Player";player.transform.position=V(0,.05f,0);var cc=player.AddComponent<CharacterController>();cc.height=1.76f;cc.radius=.24f;cc.center=V(0,.88f,0);cc.stepOffset=.28f;cc.skinWidth=.025f;player.AddComponent<CharacterMotor>();var inp=player.AddComponent<PlayerInputController>();inp.enabled=false;
 var camGO=new GameObject("Trailer main camera");camGO.tag="MainCamera";var cam=camGO.AddComponent<Camera>();cam.fieldOfView=72;cam.nearClipPlane=.035f;cam.farClipPlane=130;cam.backgroundColor=new Color(.025f,.03f,.04f);cam.clearFlags=CameraClearFlags.SolidColor;var fp=camGO.AddComponent<Dreamscape.CameraSystem.FirstPersonCamera>();var fpSerialized=new SerializedObject(fp);fpSerialized.FindProperty("target").objectReferenceValue=cc;fpSerialized.ApplyModifiedPropertiesWithoutUndo();var data=camGO.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=true;
 var volume=new GameObject("Trailer grading").AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=ScriptableObject.CreateInstance<VolumeProfile>();var tone=volume.sharedProfile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);var grade=volume.sharedProfile.Add<ColorAdjustments>();grade.postExposure.Override(.55f);grade.contrast.Override(10);grade.saturation.Override(-6);var vign=volume.sharedProfile.Add<Vignette>();vign.intensity.Override(.19f);AssetDatabase.CreateAsset(volume.sharedProfile,dir+"/Grading.asset");
 // Safety edges, bolts and below-floor construction make the traversable geometry readable.
 for(int i=0;i<3;i++){
  float z=6+i*3.6f;float y=.5f+i*.5f;
  for(int k=0;k<9;k++)Box("Safety edge marking",V(-3.8f+k*.22f,y+.03f,z-.99f),V(.10f,.025f,.16f),k%2==0?gold:metal,false);
  for(int k=-1;k<=1;k+=2){var bolt=GameObject.CreatePrimitive(PrimitiveType.Cylinder);bolt.name="Anchor bolt";bolt.transform.position=V(-2.9f+k*.85f,y+.025f,z+.75f);bolt.transform.localScale=V(.065f,.025f,.065f);bolt.GetComponent<Renderer>().sharedMaterial=metal;Object.DestroyImmediate(bolt.GetComponent<Collider>());}
 }
 foreach(float cx in new float[]{0,30}){
  Box("Below floor wall",V(cx+3.8f,-2.8f,12),V(.25f,5.6f,12),stone);Box("Below floor wall",V(cx-3.8f,-2.8f,12),V(.25f,5.6f,12),stone);
  var lamp=new GameObject("Underfloor work light").AddComponent<Light>();lamp.type=LightType.Point;lamp.transform.position=V(cx,-2,12);lamp.intensity=1.6f;lamp.range=13;lamp.color=new Color(.85f,.66f,.39f);
 }
 var director=new GameObject("Trailer gameplay recorder").AddComponent<TrailerGameplayDirector>();director.player=player.GetComponent<CharacterMotor>();director.cam=cam;director.avatar=avatar.transform;TrailerArmsBuilder.Make(cam.transform);
 EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Trailer/Trailer_Gameplay.unity");AssetDatabase.SaveAssets();Debug.Log("TRAILER_SCENE_READY");director.captureMode=true;EditorApplication.isPlaying=true;
 }
}
