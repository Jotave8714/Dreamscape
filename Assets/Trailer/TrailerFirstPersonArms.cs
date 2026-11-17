using UnityEngine;
using System.Collections.Generic;
public class TrailerFirstPersonArms:MonoBehaviour {
 float reachUntil;public void Reach(){reachUntil=Time.time+.45f;}
 Dictionary<Transform,Quaternion> rest=new Dictionary<Transform,Quaternion>();Vector3 initial;Transform leftArm,rightArm,leftFore,rightFore,leftHand,rightHand;
 Transform Find(string s){foreach(var t in GetComponentsInChildren<Transform>())if(t.name.EndsWith(s))return t;return null;}
 void Awake(){foreach(var t in GetComponentsInChildren<Transform>())rest[t]=t.localRotation;initial=transform.localPosition;leftArm=Find("LeftArm");rightArm=Find("RightArm");leftFore=Find("LeftForeArm");rightFore=Find("RightForeArm");leftHand=Find("LeftHand");rightHand=Find("RightHand");}
 void Point(Transform a,Transform b,Vector3 direction){if(a&&b)a.rotation=Quaternion.FromToRotation(b.position-a.position,transform.parent.TransformDirection(direction))*a.rotation;}
 void LateUpdate(){foreach(var p in rest)p.Key.localRotation=p.Value;var motor=Object.FindAnyObjectByType<Dreamscape.Characters.CharacterMotor>();float speed=motor?motor.HorizontalSpeed:0;float w=Mathf.Sin(Time.time*9)*Mathf.Clamp01(speed/5);bool jump=motor&&!motor.IsGrounded;transform.localPosition=initial+new Vector3(w*.012f,Mathf.Abs(w)*.012f,0);
 Point(leftArm,leftFore,new Vector3(-.18f,-.85f,.9f));Point(rightArm,rightFore,new Vector3(.18f,-.85f,.9f));
 Point(leftFore,leftHand,new Vector3(-.08f,jump?.03f:-.18f+w*.10f,1));Point(rightFore,rightHand,new Vector3(.08f,jump?.03f:-.18f-w*.10f,1));if(Time.time<reachUntil){Point(rightFore,rightHand,new Vector3(-.08f,.04f,1));}
 }
}
