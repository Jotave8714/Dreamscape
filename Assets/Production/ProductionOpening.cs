using UnityEngine;
using UnityEngine.InputSystem;
using Dreamscape.Characters;
using System.Collections;
[DefaultExecutionOrder(50)]
public class ProductionOpening:MonoBehaviour {
 DreamscapeCampaign game;Camera cam;float elapsed;bool running;Transform hands;
 IEnumerator Start(){game=GetComponent<DreamscapeCampaign>();cam=game.view;running=true;game.enabled=false;game.player.GetComponent<PlayerInputController>().enabled=false;cam.GetComponent<Dreamscape.CameraSystem.FirstPersonCamera>().enabled=false;hands=cam.transform.Find("First person sleeves and hands");if(hands)hands.gameObject.SetActive(false);foreach(var l in FindObjectsByType<Light>())if(l.type!=LightType.Directional)l.enabled=Mathf.Abs(l.transform.position.x+40)<15;
var actor=GameObject.Find("Carlinhos opening");var walk=actor.GetComponent<TrailerWalkAnimation>();walk.gait=1;
 for(float t=0;t<2.7f;t+=Time.deltaTime){actor.transform.position=new Vector3(-40,0,3+t*1.25f);cam.transform.position=actor.transform.position+new Vector3(-1.3f,1.45f,2.8f);cam.transform.LookAt(actor.transform.position+Vector3.up*1.2f);yield return null;}walk.gait=0;walk.ponder=true;for(float t=0;t<1.3f;t+=Time.deltaTime){cam.transform.position=actor.transform.position+new Vector3(-.7f,1.45f,1.6f-t*.2f);cam.transform.LookAt(actor.transform.position+Vector3.up*1.4f);yield return null;}
 walk.ponder=false;walk.gait=1;for(float t=0;t<1.8f;t+=Time.deltaTime){actor.transform.position=Vector3.Lerp(new Vector3(-40,0,6.375f),new Vector3(-40,0,9.1f),t/1.8f);cam.transform.position=actor.transform.position+new Vector3(1.3f,1.5f,2.3f);cam.transform.LookAt(actor.transform.position+Vector3.up*1.1f);yield return null;}walk.gait=0;walk.seated=true;var book=CinematicBook.Create(actor.transform);for(float t=0;t<1.7f;t+=Time.deltaTime){actor.transform.position=new Vector3(-40,-.37f*Mathf.SmoothStep(0,1,t/.7f),9.1f);walk.sleep=Mathf.Clamp01((t-.6f)/1.1f);cam.transform.position=new Vector3(-41.2f,1.1f,10.7f);cam.transform.LookAt(actor.transform.position+Vector3.up*1.05f);yield return null;}Finish();}
 void Update(){if(!running)return;elapsed+=Time.deltaTime;if(Keyboard.current?.spaceKey.wasPressedThisFrame==true)Finish();}
 public void Finish(){if(!running)return;running=false;StopAllCoroutines();game.enabled=true;game.Enter(0);game.player.GetComponent<PlayerInputController>().enabled=true;cam.GetComponent<Dreamscape.CameraSystem.FirstPersonCamera>().enabled=true;if(hands)hands.gameObject.SetActive(true);}
 void OnGUI(){if(!running)return;var style=new GUIStyle(GUI.skin.label){fontSize=Mathf.RoundToInt(Screen.height/32f),alignment=TextAnchor.MiddleCenter};style.normal.textColor=new Color(.93f,.91f,.82f);GUI.Label(new Rect(0,Screen.height*.08f,Screen.width,60),elapsed<3?"CARLINHOS / SEM ÔNIBUS. SEM SALDO.":"UMA BIBLIOTECA. ALGUNS MINUTOS DE DESCANSO.",style);style.fontSize=14;GUI.Label(new Rect(0,Screen.height-50,Screen.width,30),"ESPAÇO PARA PULAR",style);if(elapsed>7){GUI.color=new Color(0,0,0,Mathf.Clamp01((elapsed-7)/.6f));GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;}}
}

