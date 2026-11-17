Shader "Dreamscape/WorldSign" {
 Properties { _MainTex ("Font atlas", 2D) = "white" {} _Color ("Tint", Color) = (1,1,1,1) }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest LEqual Cull Off
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct V {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float4 _Color;
 V vert(A i){V o;o.pos=TransformObjectToHClip(i.vertex.xyz);o.uv=i.uv;o.color=i.color*_Color;return o;}
 half4 frag(V i):SV_Target{half a=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a;return half4(i.color.rgb,i.color.a*a);}
 ENDHLSL }
 }
}
