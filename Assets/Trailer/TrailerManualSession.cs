using UnityEngine;
using UnityEngine.InputSystem;
public class TrailerManualSession:MonoBehaviour {
 public TrailerGameplayDirector director;
 void Update(){if(director.player.transform.position.y< -3)director.ResetPlayer(new Vector3(0,.05f,0));if(Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame){foreach(var b in FindObjectsByType<TrailerDoorButton>())b.Activate(director.player.transform);}}
}
