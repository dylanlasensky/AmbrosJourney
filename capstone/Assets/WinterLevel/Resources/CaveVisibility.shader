// Masks cave darkness around the player and the scene's lantern light pools.
Shader "AmbrosJourney/CaveVisibility"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            float4 _WorldRect, _Player, _Lanterns[8], _Chamber;
            float _ChamberLight;
            int _LanternCount;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                float2 world=_WorldRect.xy+i.uv*_WorldRect.zw;
                float sight=1-smoothstep(_Player.z*.7,_Player.z,length(world-_Player.xy));
                float warm=0;
                for(int n=0;n<8;n++)
                {
                    if(n>=_LanternCount)break;
                    float glow=1-smoothstep(_Lanterns[n].z*.25,_Lanterns[n].z,length(world-_Lanterns[n].xy));
                    warm=max(warm,glow);
                }
                // Only the northern room opens up; a one-tile feather softens its walls.
                float2 outside=max(abs(world-_Chamber.xy)-_Chamber.zw,0);
                float chamber=(1-smoothstep(0,1,length(outside)))*_ChamberLight;
                float light=max(max(sight,warm*.92),chamber);
                // Warm pools tint only their soft edge; the player's small circle remains clear.
                float tint=warm*(1-sight)*(1-chamber);
                return fixed4(lerp(float3(.018,.026,.055),float3(.32,.19,.055),tint),lerp(.985,.08,light));
            }
            ENDCG
        }
    }
}
