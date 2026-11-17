using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class DreamscapeArtPass {
 public static void Build(){
 EditorSceneManager.OpenScene("Assets/Trailer/Trailer_Gameplay.unity");
 EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/CincoFases_EmProducao.unity");
 var root=new GameObject("Architectural details - Blender");
 foreach(var g in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)){
 string model=null;Vector3 p=g.position;Quaternion q=Quaternion.identity;
 if(g.name=="Ceiling fitting"){model="FluorescentFixture";p.y-=.08f;}
 if(g.name=="Shelf board")continue;
 if(model!=null){var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/ArchitecturalKit/"+model+".fbx");if(asset){var o=(GameObject)PrefabUtility.InstantiatePrefab(asset);o.transform.SetParent(root.transform);o.transform.position=p;o.transform.rotation=q;var r=g.GetComponent<Renderer>();if(r)r.enabled=false;}}
 }
 foreach(float x in new float[]{0,30,60,-30}){
 for(int i=0;i<2;i++){var a=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/ArchitecturalKit/StorageShelf.fbx");var o=(GameObject)PrefabUtility.InstantiatePrefab(a);o.transform.SetParent(root.transform);o.transform.position=new Vector3(x+3.2f,0,2+i*2.5f);o.transform.rotation=Quaternion.Euler(0,-90,0);}
 }
 EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
 }
}
