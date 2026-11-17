using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dreamscape.Characters;
#if UNITY_EDITOR
using UnityEditor;
#endif





[DefaultExecutionOrder(-1000)]
public class TrailerGameplayDirector:MonoBehaviour {
 public bool captureMode;public CharacterMotor player;public Camera cam;public Transform avatar;public Transform hands;
 static TrailerGameplayDirector instance;int frame;string output;RenderTexture rt;Texture2D tex;bool recording=true;string section="intro";Vector3 aim;bool cinematic=true;bool hadPower;float stepDistance;Vector3 previous;bool death;Vector3 spawn= new Vector3(0,.05f,0);readonly List<string> events=new List<string>();
 public static void Event(string value){if(instance){instance.events.Add(instance.frame+","+(instance.frame/24f).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+instance.section+","+value);Debug.Log("TRAILER_EVENT "+instance.frame+" "+value);}}
 void Awake(){instance=this;if(!captureMode){player.GetComponent<PlayerInputController>().enabled=true;cam.GetComponent<Dreamscape.CameraSystem.FirstPersonCamera>().enabled=true;var manual=gameObject.AddComponent<TrailerManualSession>();manual.director=this;enabled=false;return;}cam.GetComponent<Dreamscape.CameraSystem.FirstPersonCamera>().enabled=false;Time.captureFramerate=24;Time.fixedDeltaTime=1f/60;QualitySettings.vSyncCount=0;Application.runInBackground=true;output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/trailer-v3/frames"));Directory.CreateDirectory(output);rt=new RenderTexture(1280,720,24);rt.Create();tex=new Texture2D(1280,720,TextureFormat.RGB24,false);cam.targetTexture=rt;player.Jumped+=()=>Event("jump");player.Landed+=()=>Event("land");previous=player.transform.position;aim=Vector3.forward;}
 IEnumerator Start(){yield return null;
 // Actual skeletal walk and head motion for the brief opening.
 section="intro_walk";Event("shot_start");var walk=avatar.GetComponent<TrailerWalkAnimation>();walk.gait=1;
 for(float t=0;t<3.2f;t+=Time.deltaTime){avatar.position=new Vector3(-30,0,2+t*1.3f);cam.transform.position=avatar.position+new Vector3(-1.5f,1.4f,2.8f);cam.transform.LookAt(avatar.position+Vector3.up*1.15f);yield return null;}
 section="intro_thought";Event("shot_start");walk.gait=0;walk.ponder=true;
 for(float t=0;t<1.5f;t+=Time.deltaTime){cam.transform.position=avatar.position+new Vector3(-.65f,1.48f,1.25f-t*.14f);cam.transform.LookAt(avatar.position+Vector3.up*1.5f);yield return null;}
 avatar.gameObject.SetActive(false);cinematic=false;
 section="pickup_and_death";Event("shot_start");ResetPlayer(new Vector3(0,.05f,0));yield return Walk(new Vector3(-1.9f,0,2.6f),false,3);yield return Wait(.5f);yield return Walk(new Vector3(1.3f,0,5.1f),true,3);yield return Walk(new Vector3(1.3f,0,9),true,3);yield return Wait(1.6f);
 if(!death){Event("WARNING_no_death");}yield return Wait(.5f);
 section="retry_doublejump";Event("shot_start");ResetPlayer(new Vector3(0,.05f,0));yield return Walk(new Vector3(-1.9f,0,2.6f),true,2);yield return Walk(new Vector3(-2.9f,0,4.4f),true,2);
 yield return Leap(new Vector3(-2.9f,.5f,6),false);yield return Leap(new Vector3(-2.9f,1,9.6f),true);yield return Leap(new Vector3(-2.9f,1.5f,13.2f),true);yield return Leap(new Vector3(-2.9f,2,16),true);yield return Walk(new Vector3(0,2,18.4f),true,2);yield return Wait(.6f);
 section="speed_and_crouch";Event("shot_start");spawn=new Vector3(30,.05f,0);ResetPlayer(spawn);yield return Walk(new Vector3(28.4f,0,3),true,2);yield return Walk(new Vector3(27.3f,0,7.8f),true,2);player.SetCrouch(true);Event("crouch");yield return Walk(new Vector3(27.3f,0,11.3f),true,4);player.SetCrouch(false);Event("stand");yield return Walk(new Vector3(27.3f,0,21.5f),true,3);yield return Wait(.6f);
 section="moema_interaction";Event("shot_start");spawn=new Vector3(60,.05f,1);ResetPlayer(spawn);yield return Walk(new Vector3(60,0,4.5f),true,2);aim=new Vector3(60,1.2f,7);yield return Wait(.8f);var button=FindAnyObjectByType<TrailerDoorButton>();button.Activate(player.transform);yield return Wait(1.5f);yield return Walk(new Vector3(62,0,7),true,2);yield return Walk(new Vector3(60,0,12.3f),true,2);yield return Wait(.7f);
 Event("finished");recording=false;File.WriteAllLines(Path.Combine(output,"events.csv"),new[]{"frame,time,section,event"}.Concat(events));File.WriteAllText(Path.Combine(output,"complete.txt"),"frames="+frame);
 #if UNITY_EDITOR
 EditorApplication.Exit(0);
 #endif
 }
 public void ResetPlayer(Vector3 p){foreach(var tr in FindObjectsByType<TrailerTrap>())tr.Restore();foreach(var power in Resources.FindObjectsOfTypeAll<Dreamscape.DoubleJumpPickup>())if(power.gameObject.scene.IsValid())power.gameObject.SetActive(true);foreach(var power in Resources.FindObjectsOfTypeAll<TrailerSpeedPickup>())if(power.gameObject.scene.IsValid())power.gameObject.SetActive(true);player.HasDoubleJump=false;hadPower=false;typeof(CharacterMotor).GetField("sprintSpeed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(player,5f);player.Teleport(p,Quaternion.identity);player.SetMoveInput(Vector3.zero);player.SetCrouch(false);previous=p;cam.transform.rotation=Quaternion.identity;aim=p+new Vector3(0,1.2f,6);Event("restart");}
 IEnumerator Wait(float duration){player.SetMoveInput(Vector3.zero);for(float t=0;t<duration;t+=Time.deltaTime)yield return null;}
 IEnumerator Walk(Vector3 target,bool sprint,float timeout){float elapsed=0;player.SetSprint(sprint);while(elapsed<timeout){var diff=target-player.transform.position;diff.y=0;aim=player.transform.position+Vector3.up*1.5f+diff.normalized*5;if(diff.magnitude<.13f)break;player.SetMoveInput(diff.normalized*Mathf.Clamp01(diff.magnitude/.4f));elapsed+=Time.deltaTime;yield return null;if(death&&section=="pickup_and_death")break;}player.SetMoveInput(Vector3.zero);}
 IEnumerator Leap(Vector3 target,bool second){yield return Wait(.15f);Event("leap_start");player.SetSprint(true);player.RequestJump();float elapsed=0;bool extra=false;while(elapsed<1.65f){var diff=target-player.transform.position;diff.y=0;aim=target+Vector3.up*.8f;player.SetMoveInput(diff.normalized*Mathf.Clamp01(diff.magnitude/.6f));if(second&&!extra&&elapsed>.27f){player.RequestJump();extra=true;}elapsed+=Time.deltaTime;yield return null;if(elapsed>.5f&&player.IsGrounded&&diff.magnitude<.5f)break;}player.SetMoveInput(Vector3.zero);Event("leap_end_y="+player.transform.position.y.ToString("F2",System.Globalization.CultureInfo.InvariantCulture));}
 void LateUpdate(){
 if(!recording)return;foreach(var tm in FindObjectsByType<TextMesh>())tm.GetComponent<Renderer>().enabled=Vector3.Distance(tm.transform.position,cam.transform.position)<12;var viewArms=cam.transform.Find("First person sleeves and hands");if(viewArms)viewArms.gameObject.SetActive(!cinematic);
 if(!cinematic){
  if(player.HasDoubleJump&&!hadPower){Event("double_jump_collected");hadPower=true;}
  if(player.transform.position.y< -3){death=true;Event("death");ResetPlayer(spawn);}
  var delta=player.transform.position-previous;stepDistance+=new Vector2(delta.x,delta.z).magnitude;previous=player.transform.position;
  var eye=player.transform.position+Vector3.up*(player.Controller.height-.12f);if(player.IsGrounded)eye.y+=Mathf.Sin(stepDistance*7)*.025f;
  var desired=Quaternion.LookRotation(aim-eye);cam.transform.position=eye;cam.transform.rotation=Quaternion.Slerp(cam.transform.rotation,desired,.15f);cam.fieldOfView=Mathf.Lerp(cam.fieldOfView,player.HorizontalSpeed>5.5f?79:72,.1f);
 }
 cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(output,frame.ToString("D5")+".jpg"),tex.EncodeToJPG(93));frame++;
 if(frame>2400){File.WriteAllLines(Path.Combine(output,"events.csv"),events);
 #if UNITY_EDITOR
 EditorApplication.Exit(2);
 #endif
 }
 }
}
