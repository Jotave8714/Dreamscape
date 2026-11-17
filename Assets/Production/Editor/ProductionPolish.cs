using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
public static class ProductionPolish {
 static Transform root;static string dir="Assets/Production/Generated/";static Material metal,oak,plaster,blue,glow;
 static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
 static Material Get(string n)=>AssetDatabase.LoadAssetAtPath<Material>(dir+n+".mat");
 static Material M(string n,Color c,float metallic=0){var path=dir+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=c;m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.32f);return m;}
 static GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool solid=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
 static GameObject Model(string name,Vector3 p,float yaw=0){var o=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/ArchitecturalKit/"+name+".fbx"));o.transform.SetParent(root);o.transform.position=p;o.transform.rotation=Quaternion.Euler(0,yaw,0);foreach(var r in o.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++){var n=mats[i]?mats[i].name:"";mats[i]=n.Contains("Oak")?oak:n.Contains("Paper")?Get("Paper"):n.Contains("fabric")?Get("Fabric"):n.Contains("rose")?Get("Rose"):n.Contains("red")?Get("PipeRed"):n.Contains("enamel")?Get("Enamel"):metal;}r.sharedMaterials=mats;}return o;}
 static void Sign(string text,Vector3 p,float width=1.2f){Box("Sign plate",p,V(width,.30f,.045f),metal,false);var go=new GameObject("Sign "+text);go.transform.SetParent(root);go.transform.position=p-V(0,0,.03f);var t=go.AddComponent<TextMesh>();t.text=text;t.characterSize=.045f;t.fontSize=80;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.83f,.87f,.75f);}
 public static void Apply(){
 EditorSceneManager.OpenScene("Assets/Scenes/Dreamscape_CincoFases.unity");var previous=GameObject.Find("Final environment dressing");if(previous)Object.DestroyImmediate(previous);root=new GameObject("Final environment dressing").transform;
 metal=Get("Graphite");oak=Get("Oak");plaster=Get("Plaster");blue=Get("Blue power");glow=Get("Fluorescent");M("Paper",new Color(.72f,.68f,.52f));M("Fabric",new Color(.07f,.20f,.28f));M("Rose",new Color(.50f,.16f,.24f));M("PipeRed",new Color(.38f,.10f,.06f),.45f);M("Enamel",new Color(.53f,.58f,.49f));
 // Prevent skirting and pilasters from inheriting texture stretch and make rooms subtly distinct.
 var palettes=new[]{new Color(.31f,.38f,.33f),new Color(.36f,.39f,.35f),new Color(.33f,.36f,.40f),new Color(.31f,.36f,.30f),new Color(.39f,.34f,.27f)};
 for(int i=0;i<5;i++){float x=i*40;float length=i==3?16:24;var dado=M("Dado"+i,palettes[i]);var phase=GameObject.Find("FASE "+(i+1)+" - "+new[]{"Saída fácil","Pressa pra quê","Portas parecidas","Moema abre caminho","Olhe o chão"}[i]);foreach(var rr in phase.GetComponentsInChildren<Renderer>())if(rr.name=="Sage dado")rr.sharedMaterial=dado;
 Model("NoticeBoard",V(x+3.6f,1.8f,-.78f));Model("ArchiveCabinet",V(x-4.25f,0,.1f),0);Model("WallVent",V(x-2.8f,3.4f,-.78f));Sign("SETOR "+(i+1).ToString("00"),V(x+1.8f,1.8f,-.78f));
 for(int s=-1;s<=1;s+=2){for(int z=3;z<length;z+=6){var g=Model("WallVent",V(x+s*4.83f,3.1f,z),s*90);}}
 // Below the floor, supports and pipes explain the construction rather than an empty void.
 if(i==0||i==1||i==4){for(int z=5;z<length-3;z+=3){Box("Pit cross member",V(x,-2.2f,z),V(9.5f,.22f,.22f),metal,false);for(int s=-1;s<=1;s+=2)Box("Pit support",V(x+s*3.4f,-2,z),V(.18f,4,.18f),metal,false);}for(int z=5;z<length;z+=2)Model("FlangedPipe",V(x+4.3f,-1.7f,z),0).transform.rotation=Quaternion.Euler(90,0,0);}
 }
 Model("CampusBench",V(3.4f,0,1.8f),-90);Model("LibraryDesk",V(3.6f,0,3));Model("BookSet",V(3.6f,.80f,3));Sign("ÁREA EM MANUTENÇÃO",V(3.5f,1.9f,4.2f),1.8f);
 Model("ElectricalPanel",V(44.65f,1.8f,3),-90);Sign("ALTURA REDUZIDA",V(37,1.9f,9.6f),1.6f);
 for(int z=3;z<23;z+=2){var p=Model("FlangedPipe",V(44.5f,2.7f,z));p.transform.rotation=Quaternion.Euler(90,0,0);}
 // Door reveals: aperture narrowed to match the physical door, eliminating bypass.
 foreach(var m in Object.FindObjectsByType<ProductionMechanism>().Where(q=>q.kind==3)){var p=m.transform.position;for(int s=-1;s<=1;s+=2)Box("Door reveal",V(p.x+s*.88f,1.2f,p.z),V(.62f,2.4f,.24f),plaster);}
 for(int s=-1;s<=1;s+=2){float x=80+s*3;for(int side=-1;side<=1;side+=2)Box("Archive niche wall",V(x+side*1.1f,1.6f,21),V(.14f,3.2f,2),plaster);var door=Object.FindObjectsByType<ProductionMechanism>().First(q=>q.kind==3&&Mathf.Abs(q.transform.position.x-x)<.1f);var b=Box("Archive door push button",V(x+.75f,1.2f,19.75f),V(.16f,.2f,.1f),Get("Amber"));var trigger=b.AddComponent<ProductionMechanism>();trigger.kind=2;trigger.linked=door;Model("BookSet",V(x,.03f,21));}
 // A narrow opening around the central exit and continuous partition makes the button meaningful.
 for(int s=-1;s<=1;s+=2)Box("Exit partition",V(80+s*3,1.7f,23.9f),V(4.8f,3.4f,.2f),plaster);
 Model("CampusBench",V(76.1f,0,10),90);Model("ArchiveCabinet",V(76.1f,0,14),90);Sign("ACESSO CONTROLADO",V(80,2.9f,23.7f),1.8f);
 // Give shelf contents individual colors and real book silhouettes.
 for(int s=-1;s<=1;s+=2)for(int k=0;k<4;k++)for(int row=0;row<3;row++)Model("BookSet",V(120+s*4,.16f+row*.51f,3+k*2.7f),s*90);
 foreach(var r in Object.FindObjectsByType<SkinnedMeshRenderer>()){if(r.transform.root.name.Contains("Moema"))r.sharedMaterials=Enumerable.Repeat(blue,r.sharedMaterials.Length).ToArray();}
 Sign("MOEMA / E PARA ACIONAR",V(120,.52f,7.2f),1.8f);
 Model("ElectricalPanel",V(164.65f,1.7f,2.5f),-90);Sign("OBSERVE AS FIXAÇÕES",V(163,1.8f,3.8f),1.9f);
 // Library opening set, separate from all gameplay spaces.
 float lib=-40;Box("Library floor",V(lib,-.18f,7),V(12,.36f,18),Get("Terrazzo"));Box("Library rear wall",V(lib,2.2f,15),V(12,4.4f,.2f),plaster);Box("Library ceiling",V(lib,4.4f,7),V(12,.2f,18),plaster);
 for(int s=-1;s<=1;s+=2){Box("Library wall",V(lib+s*6,2.2f,7),V(.2f,4.4f,18),plaster);for(int k=0;k<5;k++){Model("StorageShelf",V(lib+s*4.5f,0,1+k*2.4f),s*90);for(int row=0;row<4;row++)Model("BookSet",V(lib+s*4.5f,.16f+row*.51f,1+k*2.4f),s*90);}}
 Model("BlueBeanbag",V(lib,0,9));Model("LibraryDesk",V(lib+2,0,8));Model("BookSet",V(lib+2,.8f,8));Model("CampusBench",V(lib-2,0,12));
 for(int z=2;z<14;z+=4){Model("FluorescentFixture",V(lib,4.25f,z));var l=new GameObject("Library fluorescent").AddComponent<Light>();l.transform.SetParent(root);l.transform.position=V(lib,3.6f,z);l.intensity=4;l.range=8;l.color=new Color(1,.85f,.65f);}
 Sign("BIBLIOTECA",V(lib,2.7f,14.8f),2);var character=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/Protagonist/Protagonist.fbx"));character.name="Carlinhos opening";character.transform.SetParent(root);character.transform.position=V(lib,0,3);character.AddComponent<TrailerWalkAnimation>();
 var mat=M("CarlinhosRose",new Color(.55f,.18f,.27f));foreach(var r in character.GetComponentsInChildren<SkinnedMeshRenderer>()){var arr=r.sharedMaterials;for(int k=0;k<arr.Length;k++)if(arr[k]&& (arr[k].name.ToLower().Contains("hood")||arr[k].name.ToLower().Contains("shirt")||arr[k].name.ToLower().Contains("blus")))arr[k]=mat;r.sharedMaterials=arr;}
 var key=new GameObject("Soft directional fill").AddComponent<Light>();key.type=LightType.Directional;key.intensity=.12f;key.transform.rotation=Quaternion.Euler(65,25,0);key.shadows=LightShadows.None;
 EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
 }
}
