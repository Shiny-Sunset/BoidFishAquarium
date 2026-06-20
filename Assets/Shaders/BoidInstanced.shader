Shader "Unlit/BoidInstanced"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Off // この行を追加して両面を描画する

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct BoidData
            {
                float3 position;
                float3 velocity;
                float3 forward;
            };

            StructuredBuffer<BoidData> _BoidsBuffer;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                BoidData boid = _BoidsBuffer[v.instanceID];

                float3 up = float3(0, 1, 0);
                float3 right = cross(up, boid.forward);
                up = cross(boid.forward, right);
                float3x3 rotationMatrix = float3x3(right, up, boid.forward);

                float3 worldPos = mul(rotationMatrix, v.vertex.xyz) + boid.position;
                o.vertex = UnityWorldToClipPos(worldPos);
                o.worldNormal = mul(rotationMatrix, v.normal);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = saturate(dot(i.worldNormal, lightDir));
                return _Color * ndotl;
            }
            ENDCG
        }
    }
}