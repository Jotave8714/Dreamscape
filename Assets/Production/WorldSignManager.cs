using UnityEngine;
public class WorldSignManager:MonoBehaviour {
 public Material material;Font font;
 void Awake(){font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");foreach(var t in FindObjectsByType<TextMesh>()){t.font=font;font.RequestCharactersInTexture(t.text,t.fontSize,t.fontStyle);t.GetComponent<Renderer>().sharedMaterial=material;}UpdateAtlas(font);Font.textureRebuilt+=UpdateAtlas;}
 void UpdateAtlas(Font f){if(material&&font)material.mainTexture=font.material.mainTexture;}
 void OnDestroy(){Font.textureRebuilt-=UpdateAtlas;}
}
