using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
public static class TrailerArmsBuilder {
 public static void Make(Transform camera){
 var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/Protagonist/Protagonist.fbx");var go=(GameObject)PrefabUtility.InstantiatePrefab(model);go.name="First person sleeves and hands";go.transform.SetParent(camera);go.transform.localPosition=new Vector3(0,-1.55f,.10f);go.transform.localRotation=Quaternion.identity;
 foreach(var sm in go.GetComponentsInChildren<SkinnedMeshRenderer>()){
 var source=sm.sharedMesh;var m=Object.Instantiate(source);var weights=m.boneWeights;var allowed=new HashSet<int>();for(int b=0;b<sm.bones.Length;b++){string n=sm.bones[b].name;if(n.Contains("ForeArm")||n.Contains("Hand"))allowed.Add(b);}
 bool[] keep=new bool[weights.Length];for(int i=0;i<weights.Length;i++){var w=weights[i];float weight=0;if(allowed.Contains(w.boneIndex0))weight+=w.weight0;if(allowed.Contains(w.boneIndex1))weight+=w.weight1;if(allowed.Contains(w.boneIndex2))weight+=w.weight2;if(allowed.Contains(w.boneIndex3))weight+=w.weight3;keep[i]=weight>.5f;}
 for(int sub=0;sub<m.subMeshCount;sub++){int[] ids=m.GetTriangles(sub);var result=new List<int>();for(int i=0;i<ids.Length;i+=3)if(keep[ids[i]]&&keep[ids[i+1]]&&keep[ids[i+2]]){result.Add(ids[i]);result.Add(ids[i+1]);result.Add(ids[i+2]);}m.SetTriangles(result,sub);}
 m.name="FirstPersonArms";AssetDatabase.CreateAsset(m,"Assets/Trailer/Generated/FirstPersonArms.asset");sm.sharedMesh=m;sm.updateWhenOffscreen=true;sm.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
 }
 foreach(var mr in go.GetComponentsInChildren<MeshRenderer>())mr.enabled=false;
 var anim=go.GetComponentInChildren<Animator>();if(anim)anim.enabled=false;go.AddComponent<TrailerFirstPersonArms>();
 }
}
