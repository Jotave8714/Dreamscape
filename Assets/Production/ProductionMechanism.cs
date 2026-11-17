using UnityEngine;
using Dreamscape.Characters;
public class ProductionMechanism:MonoBehaviour {
 public int kind;public Transform moving;public float delay=.65f;public bool jump;public ProductionMechanism linked;Vector3 origin;bool used;float timer;bool initialized;bool opened;
 void Awake(){Initialize();}void Initialize(){if(initialized)return;initialized=true;if(moving)origin=moving.position;}
 public void ResetState(){Initialize();used=false;opened=false;timer=0;gameObject.SetActive(true);if(moving){moving.position=origin;var c=moving.GetComponent<Collider>();if(c)c.enabled=true;}}
 public void Activate(){if(used)return;used=true;timer=0;if(kind==2){var arms=FindAnyObjectByType<TrailerFirstPersonArms>();if(arms)arms.Reach();ProductionAudio.Cue("Button");ProductionCapture.Emit("button_press");}if(kind==3)ProductionAudio.Cue("Door");if(linked)linked.Activate();}
 void OnTriggerEnter(Collider c){var m=c.GetComponentInParent<CharacterMotor>();if(!m)return;if(kind==0){ProductionAudio.Cue("Collect");ProductionCapture.Emit(jump?"double_jump_collected":"speed_collected");FindAnyObjectByType<DreamscapeCampaign>().Power(jump);gameObject.SetActive(false);}if(kind==1)Activate();}
 void Update(){if(kind==0){transform.Rotate(0,45*Time.deltaTime,0);return;}if(!used||!moving)return;timer+=Time.deltaTime;if(kind==1&&timer>delay){if(!opened){opened=true;ProductionCapture.Emit("floor_opens");}moving.position=origin+Vector3.down*Mathf.Min(8,Mathf.Pow((timer-delay)*4,2));var c=moving.GetComponent<Collider>();if(c)c.enabled=false;}if(kind==3)moving.position=Vector3.Lerp(origin,origin+Vector3.up*2.7f,Mathf.SmoothStep(0,1,timer/1.3f));}
}
