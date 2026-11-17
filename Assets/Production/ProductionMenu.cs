using UnityEngine;
using UnityEngine.InputSystem;
using Dreamscape.Characters;
public class ProductionMenu:MonoBehaviour {
 DreamscapeCampaign game;bool open;Texture2D panel;GUIStyle title,body,button;
 void Start(){game=GetComponent<DreamscapeCampaign>();panel=new Texture2D(1,1);panel.SetPixel(0,0,new Color(.035f,.05f,.055f,.97f));panel.Apply();}
 void Update(){if(Keyboard.current?.escapeKey.wasPressedThisFrame==true)Toggle(!open);if(game.finished&&!open)Toggle(true);}
 void LateUpdate(){if(!open&&game&&!game.finished){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}}
 void Toggle(bool value){if(value){var intro=GetComponent<ProductionOpening>();if(intro)intro.Finish();}open=value;Time.timeScale=open?0:1;game.player.GetComponent<PlayerInputController>().enabled=!open;Cursor.lockState=open?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=open;}
 void OnDisable(){Time.timeScale=1;}
 void OnGUI(){if(!open)return;GUI.matrix=Matrix4x4.identity;GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),panel);float x=(Screen.width-560)/2f,y=Mathf.Max(24,(Screen.height-540)/2f);if(title==null){title=new GUIStyle(GUI.skin.label){fontSize=38,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};title.normal.textColor=new Color(.9f,.91f,.83f);body=new GUIStyle(GUI.skin.label){fontSize=16,alignment=TextAnchor.MiddleCenter};body.normal.textColor=new Color(.58f,.69f,.67f);button=new GUIStyle(GUI.skin.button){fontSize=17,fixedHeight=42};}
 GUI.Label(new Rect(x,y,560,60),"DREAMSCAPE",title);GUI.Label(new Rect(x,y+64,560,32),game.finished?"Você encontrou a saída das cinco salas.":"PAUSA",body);
 if(!game.finished&&GUI.Button(new Rect(x+90,y+116,380,42),"Continuar",button))Toggle(false);
 if(GUI.Button(new Rect(x+90,y+168,380,42),"Reiniciar esta fase",button)){game.finished=false;game.Enter(game.phase);Toggle(false);}
 GUI.Label(new Rect(x,y+227,560,28),"ESCOLHER FASE",body);
 for(int i=0;i<game.titles.Length;i++)if(GUI.Button(new Rect(x+90,y+264+i*46,380,40),(i+1).ToString("00")+"  /  "+game.titles[i],button)){game.finished=false;game.Enter(i);Toggle(false);}
 }
}
