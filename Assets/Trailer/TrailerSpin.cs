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
public class TrailerSpin:MonoBehaviour {void Update(){transform.Rotate(0,45*Time.deltaTime,0);}}
