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
public class TrailerDoorButton:MonoBehaviour {
 public GameObject door;bool used;
 public bool Activate(Transform player){if(used||Vector3.Distance(player.position,transform.position)>2.3f)return false;used=true;StartCoroutine(Open());return true;}
 IEnumerator Open(){TrailerGameplayDirector.Event("button_press");var start=door.transform.position;for(float t=0;t<1.2f;t+=Time.deltaTime){door.transform.position=start+Vector3.up*(2.4f*Mathf.SmoothStep(0,1,t/1.2f));yield return null;}door.GetComponent<Collider>().enabled=false;TrailerGameplayDirector.Event("door_open");}
}
