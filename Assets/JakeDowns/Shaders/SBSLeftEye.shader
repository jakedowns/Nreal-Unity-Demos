Shader "JakeDowns/SBSLeftEye"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _AspectRatio ("Aspect Ratio", Range(0, 100)) = 1
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _AspectRatio;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 coord = i.uv;
                // shift lookup x coordinate to sample just the left half of the texture           
                coord.x = coord.x * 0.5;

                // Calculate the UV offset based on the aspect ratio difference
                float2 uvOffset = float2(0.0, 0.0); //

                float inputAspectRatio = _AspectRatio;
                float outputAspectRatio = _ScreenParams.x / _ScreenParams.y;
                if (inputAspectRatio < 1.75)
                {
                    //uvOffset.x += (inputAspectRatio - outputAspectRatio) * 0.25;
                }

                // Sample the input texture using the adjusted UV coordinates
                coord = coord + uvOffset;

                // sample the texture
                fixed4 col = tex2D(_MainTex, coord.xy);
                
                return col;
            }
            ENDCG
        }
    }
}
