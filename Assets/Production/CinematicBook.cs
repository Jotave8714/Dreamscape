using UnityEngine;
[DefaultExecutionOrder(200)]
public class CinematicBook:MonoBehaviour {
 Transform actor,left,right;
 public static GameObject Create(Transform actor){var root=new GameObject("Carlinhos reading book");root.transform.SetParent(actor);var cover=new Material(Shader.Find("Universal Render Pipeline/Lit"));cover.color=new Color(.055f,.10f,.16f);cover.SetFloat("_Smoothness",.22f);var paper=new Material(cover);paper.color=new Color(.80f,.77f,.65f);
 for(int s=-1;s<=1;s+=2){Part(root.transform,"Book cover",new Vector3(s*.10f,-.014f,0),new Vector3(.215f,.012f,.265f),cover,s*8);Part(root.transform,"Pages",new Vector3(s*.10f,.003f,0),new Vector3(.19f,.022f,.24f),paper,s*8);}Part(root.transform,"Spine",new Vector3(0,-.018f,0),new Vector3(.025f,.025f,.265f),cover,0);
 var follow=root.AddComponent<CinematicBook>();follow.actor=actor;foreach(var t in actor.GetComponentsInChildren<Transform>()){if(t.name.EndsWith("LeftHand"))follow.left=t;if(t.name.EndsWith("RightHand"))follow.right=t;}return root;}
 static void Part(Transform parent,string name,Vector3 p,Vector3 scale,Material mat,float roll){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.transform.localRotation=Quaternion.Euler(0,0,roll);g.GetComponent<Renderer>().sharedMaterial=mat;Destroy(g.GetComponent<Collider>());}
 void LateUpdate(){if(!left||!right)return;transform.position=(left.position+right.position)*.5f+actor.forward*.015f;transform.rotation=actor.rotation*Quaternion.Euler(12,0,0);}
}
