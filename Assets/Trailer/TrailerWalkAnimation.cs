using UnityEngine;
using System.Collections.Generic;
public class TrailerWalkAnimation:MonoBehaviour {
 public float gait;public bool ponder,seated;public float sleep;
 readonly Dictionary<Transform,Quaternion> rest=new Dictionary<Transform,Quaternion>();
 Transform hips,head,spine;Vector3 hipsRest,previous;float phase,walkBlend,seatBlend;Quaternion leftFootRest,rightFootRest;
 Transform Find(string suffix){foreach(var t in GetComponentsInChildren<Transform>())if(t.name.EndsWith(suffix))return t;return null;}
 void Awake(){foreach(var t in GetComponentsInChildren<Transform>())rest[t]=t.localRotation;hips=Find("Hips");head=Find("Head");spine=Find("Spine");hipsRest=hips.localPosition;leftFootRest=Quaternion.Inverse(transform.rotation)*Find("LeftFoot").rotation;rightFootRest=Quaternion.Inverse(transform.rotation)*Find("RightFoot").rotation;previous=transform.position;var a=GetComponentInChildren<Animator>();if(a)a.enabled=false;}
 void Point(string bone,string end,Vector3 direction){var a=Find(bone);var b=Find(end);if(a&&b)a.rotation=Quaternion.FromToRotation(b.position-a.position,transform.TransformDirection(direction))*a.rotation;}
 void Leg(string side,Vector3 target){var upper=Find(side+"UpLeg");var lower=Find(side+"Leg");var foot=Find(side+"Foot");if(!upper||!lower||!foot)return;float a=Vector3.Distance(upper.position,lower.position),b=Vector3.Distance(lower.position,foot.position);Vector3 delta=target-upper.position;float d=Mathf.Clamp(delta.magnitude,.05f,a+b-.001f);Vector3 axis=delta.normalized;Vector3 bend=Vector3.ProjectOnPlane(transform.forward,axis).normalized;float along=(a*a-b*b+d*d)/(2*d);Vector3 knee=upper.position+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,knee-upper.position)*upper.rotation;lower.rotation=Quaternion.FromToRotation(foot.position-lower.position,target-lower.position)*lower.rotation;foot.rotation=transform.rotation*(side=="Left"?leftFootRest:rightFootRest);}
 Vector3 Step(float p,float side){p=Mathf.Repeat(p,1);float z,y=.09f;if(p<.6f)z=Mathf.Lerp(.33f,-.33f,p/.6f);else{float u=(p-.6f)/.4f;z=Mathf.Lerp(-.33f,.33f,Mathf.SmoothStep(0,1,u));y+=Mathf.Sin(u*Mathf.PI)*.085f;}return new Vector3(side*.105f,y,z);}
 void LateUpdate(){float distance=Vector3.ProjectOnPlane(transform.position-previous,Vector3.up).magnitude;previous=transform.position;if(distance<.3f)phase+=distance/1.1f;float moving=gait>0&&distance>.0001f?1:0;walkBlend=Mathf.MoveTowards(walkBlend,moving,Time.deltaTime*5);seatBlend=Mathf.MoveTowards(seatBlend,seated?1:0,Time.deltaTime*2.8f);foreach(var r in rest)r.Key.localRotation=r.Value;
 hips.localPosition=hipsRest+Vector3.up*((-.07f+.012f*Mathf.Cos(phase*Mathf.PI*4))*walkBlend*(1-seatBlend));
 float arm=Mathf.Sin(phase*Mathf.PI*2)*.20f*walkBlend*(1-seatBlend);
 Point("LeftArm","LeftForeArm",new Vector3(.045f,-1,-arm));Point("RightArm","RightForeArm",new Vector3(-.045f,-1,arm));Point("LeftForeArm","LeftHand",new Vector3(0,-1,.10f-arm*.35f));Point("RightForeArm","RightHand",new Vector3(0,-1,.10f+arm*.35f));
 for(int i=0;i<2;i++){float side=i==0?-1:1;var neutral=new Vector3(side*.105f,.09f,0);var step=Vector3.Lerp(neutral,Step(phase+(i==0?0:.5f),side),walkBlend);Vector3 target=transform.TransformPoint(step);target.y=step.y;var sit=transform.TransformPoint(new Vector3(side*.14f,0,.48f));sit.y=.09f;target=Vector3.Lerp(target,sit,seatBlend);Leg(i==0?"Left":"Right",target);}
 if(spine)spine.localRotation=rest[spine]*Quaternion.Euler(Mathf.Lerp(0,-6*sleep,seatBlend),Mathf.Sin(phase*Mathf.PI*2)*2*walkBlend,0);
 if(head)head.localRotation=rest[head]*Quaternion.Euler(seatBlend*(12+sleep*20)+(ponder?15:0),ponder?8:0,0);
 if(seatBlend>0){Point("LeftArm","LeftForeArm",Vector3.Lerp(new Vector3(.05f,-1,0),new Vector3(-.10f,-.65f,.7f),seatBlend));Point("RightArm","RightForeArm",Vector3.Lerp(new Vector3(-.05f,-1,0),new Vector3(.10f,-.65f,.7f),seatBlend));Point("LeftForeArm","LeftHand",new Vector3(.12f,-.13f,1));Point("RightForeArm","RightHand",new Vector3(-.12f,-.13f,1));}
 else if(ponder){Point("LeftArm","LeftForeArm",new Vector3(.1f,-.7f,.6f));Point("LeftForeArm","LeftHand",new Vector3(0,.65f,.8f));}
 }
}
