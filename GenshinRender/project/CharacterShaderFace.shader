Shader "Unlit/CharacterShaderFace"
{
    Properties
    {
        [Header(Texture)]
        _BaseMap ("Texture", 2D) = "white" {} // ����������ͼ
        _LightMap ("LightMap",2D) = "white" {} 
        _RampMap("RampMap",2D) = "white" {}

        [Header(Parameter)]      
        [Enum(Day,2,Night,1)]_TimeShift("Day&Night Switch",int) = 2
        _ShadowOffset("Shadow Offset",range(0.0,1.0)) = 0
        _ShadowRampWidth("Shadow Ramp Width",range(0.0,1.0)) = 0.1
        _ShadowRampThreshold("Shadow Ramp Threshold",range(0.0,1.0)) = 0.0
    }
    SubShader
    {
        Tags
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline" // ���Զ�����Ⱦ���ߣ���Ҫ��Setting������
        }
        LOD 100
        HLSLINCLUDE
        #pragma vertex vert // ʹ����Ԥ����ָ�ָ��ʹ�ö���Ķ�����ɫ����vert��
        #pragma fragment frag
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        float4 _LightMap_ST;
        float4 _RampMap_ST;
        float _ShadowOffset;
        float _ShadowRampWidth;
        float _ShadowRampThreshold;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_LightMap);
        SAMPLER(sampler_LightMap);
        TEXTURE2D(_RampMap);
        SAMPLER(sampler_RampMap);


        //����ṹ��
        struct appdata
        {
            float4 posOS            : POSITION;
            float2 uv               : TEXCOORD0;
            float3 normalOS         : NORMAL;
            float4 tangentOS        : TANGENT;
            float4 vertexColor      : COLOR;
        };

        //ƬԪ�ṹ��
        struct v2f
        {
            float4 posCS            : SV_POSITION;
            float2 uv               : TEXCOORD0;
            float3 posWS            : TEXCOORD1;
            float3 nDirWS           : TEXCOORD2;
            float4 col              : COLOR;
        };

        //Ramp��������
        float remap(float x, float t1, float t2, float s1, float s2)
        {
            return (x - t1) / (t2 - t1) * (s2 - s1) + s1;
        }
        ENDHLSL

        Pass
        {
            Cull Off // ��ʾ�������޳���������ģ�͵ı��������Ⱦ�޳��������ý�ʹģ�͵�ǰ��ͱ��涼�ɼ���
            Name "Forward" // ����ǰPass����һ�����ƣ��Ա�����Ҫ���û�ʶ���Passʱʹ�á�
            Tags{ "LightMode" = "UniversalForward" } // ���Passʹ�õ���Universal��Ⱦ���ߵ�ǰ����Ⱦ��ʽ��
            

            HLSLPROGRAM
            //CBUFFER_END
            v2f vert(appdata v)
            {
                v2f o;
                o.posCS = TransformObjectToHClip(v.posOS.xyz); // ����������ת�����ü�����ϵ
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap); // ����uv�����Diffuse��ͼ���в���
                o.posWS = TransformObjectToWorld((float3)v.posOS); // ��ȡ��������
                o.nDirWS = TransformObjectToWorldNormal(v.normalOS); // ��ȡ��������ϵ�µķ�����
                o.col = v.vertexColor; // ��ȡ������ɫ
                return o;
            }

            // half4�ǰ뾫�ȸ�����4������ɫ���;��ȸ�����
            half4 frag(v2f i) : SV_Target
            {
                //��Դ
                Light mainLight = GetMainLight(); 
                float4 mainLightColor = float4(mainLight.color, 1); 

                //����
                float3 uDirWS = float3(0,1,0);
                float3 fDirWS = _fDirWS;
                float3 rDirWS = cross(uDirWS,fDirWS);
                float3 lDirWS = normalize(mainLight.direction);
                
                //����
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float4 lightMap = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, i.uv); 

                //����
                float2 lDir = normalize(lDirWS.xz);
                float rdotl = dot(normalize(rDirWS.xz),lDir);
                float fdotl = dot(normalize(fDirWS.xz),lDir);
                float faceShadowR=SAMPLE_TEXTURE2D(_LightMap,sampler_LightMap,float2(-i.uv.x,i.uv.y)).a;
                float faceShadowL=SAMPLE_TEXTURE2D(_LightMap,sampler_LightMap,i.uv).a;

                float shadowMap = faceShadowL*step(0,-rdotl)+faceShadowR*step(0,rdotl);
                float inShadow = step(0,shadowMap-(1-fdotl)/2);

                float2 rampUV=float2(clamp(inShadow,0.1,0.9),0.35+(_TimeShift-1.0)*0.5);
                half4 shadowColor = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampUV);
                half4 color=lerp(shadowColor, max(mainLightColor,shadowColor), inShadow) * baseMap;
                
                return col;
            }
            ENDHLSL
        }
    }
}
