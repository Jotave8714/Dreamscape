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
public class TrailerSpeedPickup:MonoBehaviour {
 void OnTriggerEnter(Collider c){var m=c.GetComponentInParent<CharacterMotor>();if(!m)return;typeof(CharacterMotor).GetField("sprintSpeed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(m,7.0f);TrailerGameplayDirector.Event("speed_collected");gameObject.SetActive(false);}
}
