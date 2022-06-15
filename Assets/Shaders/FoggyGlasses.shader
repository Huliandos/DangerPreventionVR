Shader "Custom/Unlit/FoggyGlasses"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Blur("Blur", range(0, 1)) = 1

        _NoiseTex("Texture", 2D) = "white" {}
        _Level("Fog Spread Level", Range(0.0, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

        GrabPass{"_GrabTexture"}
        Pass
        {   
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #define S(a, b, t) smoothstep(a, b, t)

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 grabUv : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex, _GrabTexture;
            float4 _MainTex_ST;
            float _Size, _Distortion, _Blur;

            sampler2D _NoiseTex;
            float _Level;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grabUv = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float N21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                ///Fog Noise Adjustment
                float cutout = tex2D(_NoiseTex, i.uv).r;

                float4 col;
                ///fog
                if (cutout < _Level) col = 3;     //tweak this value to change the oppacity of our glasses
                else col = 2;


                //idea of this is to reduce noise in distance. It causes weird problems with some of the Objects Triangles though
                //float fade = saturate(fwidth(i.uv) * 50);
                //float blur = _Blur * 7 * (1-fade); // * (1-distortionMap here)


                //col = tex2Dlod(_MainTex, float4(i.uv * _Distortion, 0, blur));
                float blur = _Blur * 7;

                float2 projUv = i.grabUv.xy / i.grabUv.w;
                blur *= .01;

                const float numSamples = 32;    //tweak this value if performance is too bad
                float a = N21(i.uv) * 6.2831;     // * 2pi
                for (float i = 0; i < numSamples; i++) {
                    if (cutout < _Level) {
                        float2 offset = float2(sin(a), cos(a)) * blur;
                        float d = frac(sin((i + 1) * 546.) * 5424.);
                        d = sqrt(d);
                        offset *= d;
                        col += tex2D(_GrabTexture, projUv + offset);
                    }
                    else {
                        col += tex2D(_GrabTexture, projUv);
                    }
                    a++;
                }
                col /= numSamples;

                return col;
            }
            ENDCG
        }
    }
}
