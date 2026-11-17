using UnityEngine;
public class ProductionAudio:MonoBehaviour {
 public AudioClip jump,land,collect,button,door,death;AudioSource source;public static ProductionAudio Instance;
 void Awake(){Instance=this;source=gameObject.AddComponent<AudioSource>();source.volume=.45f;source.spatialBlend=0;}
 void Start(){var m=FindAnyObjectByType<DreamscapeCampaign>().player;m.Jumped+=()=>Play(jump);m.Landed+=()=>Play(land);}
 public void Play(AudioClip clip){if(clip)source.PlayOneShot(clip);}
 public static void Cue(string name){if(!Instance)return;var a=Instance;var clip=name=="Collect"?a.collect:name=="Button"?a.button:name=="Door"?a.door:name=="Death"?a.death:null;a.Play(clip);}
}
