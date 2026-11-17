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
public class TrailerTrap:MonoBehaviour {
 public GameObject floor;public float delay=.22f;bool fired;Vector3 initial;Quaternion rotation;
 void Awake(){initial=floor.transform.position;rotation=floor.transform.rotation;}
 void OnTriggerEnter(Collider c){if(fired||!c.GetComponentInParent<CharacterMotor>())return;fired=true;StartCoroutine(Fall());}
 IEnumerator Fall(){yield return new WaitForSeconds(delay);TrailerGameplayDirector.Event("floor_opens");var col=floor.GetComponent<Collider>();col.enabled=false;float elapsed=0;while(elapsed<.55f){elapsed+=Time.deltaTime;floor.transform.position=initial+Vector3.down*(elapsed*elapsed*5);floor.transform.rotation=rotation*Quaternion.Euler(elapsed*110,0,0);yield return null;}}
 public void Restore(){StopAllCoroutines();fired=false;floor.transform.position=initial;floor.transform.rotation=rotation;floor.GetComponent<Collider>().enabled=true;}
}
