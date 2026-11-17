using UnityEngine;
using UnityEngine.InputSystem;
using Dreamscape.Characters;
using System.Reflection;
public class DreamscapeCampaign : MonoBehaviour {
 public CharacterMotor player;public Vector3[] starts;public Vector3[] exits;public string[] titles;public int phase;public int deaths;public bool finished;public Camera view;
 float notice;string message;float originalSpeed;ProductionMechanism[] mechanisms;
 void Start(){originalSpeed=5;mechanisms=FindObjectsByType<ProductionMechanism>();Enter(0);}
 public void Enter(int index){phase=index;player.GetComponent<PlayerInputController>().ResetControls();view.GetComponent<Dreamscape.CameraSystem.FirstPersonCamera>().ResetView();foreach(var l in FindObjectsByType<Light>())if(l.type!=LightType.Directional)l.enabled=Mathf.Abs(l.transform.position.x-starts[phase].x)<15;player.HasDoubleJump=false;typeof(CharacterMotor).GetField("sprintSpeed",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(player,originalSpeed);foreach(var m in mechanisms)m.ResetState();player.Teleport(starts[phase],Quaternion.identity);message=titles[phase];notice=4;}
 void Update(){if(Time.timeScale==0)return;notice-=Time.deltaTime;if(finished)return;if(player.transform.position.y< -4){deaths++;ProductionAudio.Cue("Death");Enter(phase);}
 if(Keyboard.current!=null&&Keyboard.current.rKey.wasPressedThisFrame)Enter(phase);
 if(Vector3.Distance(player.transform.position,exits[phase])<1.3f){if(phase<starts.Length-1)Enter(phase+1);else{finished=true;message="VOCÊ ENCONTROU A SAÍDA";notice=100;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}}
 if(Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame){RaycastHit hit;if(Physics.Raycast(view.transform.position,view.transform.forward,out hit,2.8f)){var b=hit.collider.GetComponentInParent<ProductionMechanism>();if(b&&b.kind==2)b.Activate();}}
 }
 public void Power(bool jump){if(jump)player.HasDoubleJump=true;else typeof(CharacterMotor).GetField("sprintSpeed",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(player,7f);message=jump?"PULO DUPLO • ESPAÇO NO AR":"VELOCIDADE • SHIFT";notice=3;}
 void OnGUI(){float s=Screen.height/720f;GUI.matrix=Matrix4x4.Scale(new Vector3(s,s,1));var label=new GUIStyle(GUI.skin.label){fontSize=18};label.normal.textColor=new Color(.87f,.89f,.83f);GUI.Label(new Rect(30,24,800,35),"DREAMSCAPE   /   "+(phase+1).ToString("00")+"   "+titles[phase],label);label.fontSize=13;GUI.Label(new Rect(30,680,1100,30),"WASD  MOVER     ESPAÇO  PULAR     SHIFT  CORRER     C  AGACHAR     E  INTERAGIR     R  REINICIAR",label);GUI.Label(new Rect(Screen.width/s/2-4,350,20,20),"·",label);if(notice>0){label.fontSize=24;label.alignment=TextAnchor.MiddleCenter;GUI.Label(new Rect(0,90,Screen.width/s,45),message,label);}RaycastHit h;if(Physics.Raycast(view.transform.position,view.transform.forward,out h,2.8f)&&h.collider.GetComponentInParent<ProductionMechanism>()?.kind==2){label.alignment=TextAnchor.MiddleCenter;GUI.Label(new Rect(0,410,Screen.width/s,40),"E  •  ACIONAR",label);}}
}
