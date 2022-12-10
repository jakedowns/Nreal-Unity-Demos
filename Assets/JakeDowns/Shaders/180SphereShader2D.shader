Shader "JakeDowns/180SphereShader2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        

        Pass
        {
            // Disable culling for this Pass.
            // You would typically do this for special effects, such as transparent objects or double-sided walls.
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float minA = 0.0;
            float maxA = 1.0;
            float minB = 0.25;
            float maxB = 0.75;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 remapped_uv = float2(i.uv);
                
                // Map the input value from the input range to the output range
                //remapped_uv.x = (i.uv.x - minA) / (maxA - minA) * (maxB - minB) + minB;
                
                // sample the texture
                fixed4 col = tex2D(_MainTex, remapped_uv);

                if (i.uv.x < .25 || i.uv.x > .75) {
                    col = fixed4(0.0, 0.0, 0.0, 1.0); // black
                }

                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
