Shader "Unlit/CharacterShaderBody"
{
    Properties
    {
        [Header(Texture)]
        _BaseMap ("Texture", 2D) = "white" {} // ����������ͼ
        _LightMap ("LightMap",2D) = "white" {} 
        _RampMap("RampMap",2D) = "white" {}
        _MetalMap("MetalMap",2D) = "white" {}

        [Header(Parameter)]      
        [Enum(Day,2,Night,1)]_TimeShift("Day&Night Switch",int) = 2
        _BoundaryIntensity("Boundary Intensity",range(0.0,1.0)) = 0.1
        _ShadowOffset("Shadow Offset",range(0.0,1.0)) = 0
        _ShadowRampWidth("Shadow Ramp Width",range(0.0,1.0)) = 0.1
        _ShadowRampThreshold("Shadow Ramp Threshold",range(0.0,1.0)) = 0.0

        _BPSpecularIntensity("Blin-Phong Specular Intensity",range(0.0,10.0)) = 0.5
        [PowerSlider(2)]_SpecularExp("Specular Exponent",range(1.0,128.0)) = 0.5
        _ViewSpecularWidth("View Specular Width",range(0.0,1.0)) = 0.5
        _NonmetalSpecularIntensity("Nonmetal Specular Intensity",range(0.0,10.0)) = 0.5
        _MetalSpecularIntensity("Metal Specular Intensity",range(0.0,10.0)) = 0.5
   
        _EmissionIntensity("Emission Intensity",range(0.0,10.0)) = 1.0
     
        _RimLightWidth("Rim Light Width",range(0.0,0.01)) = 0.006
        _RimLightThreshold("Rim Light Threshold",range(0.0,1)) = 0.6
        _RimLightIntensity("Rim Light Intensity",range(0.0,1)) = 0.25
    
        _OutlineColor("Outline Color",color) = (0.0,0.0,0.0)
        _OutlineWidth("Outline Width",range(0.0,0.01)) = 0.01
        [HideInInspector]
        _fDirWS("Face Front Direction",vector) = (0,0,1,0)

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
        float4 _MetalMap_ST;
        float4 _RampMap_ST;
        float _BoundaryIntensity;
        float _ShadowOffset;
        float _ShadowRampWidth;
        float _ShadowRampThreshold;
        float _ViewSpecularWidth;
        float _NonmetalSpecularIntensity;
        float _MetalSpecularIntensity;
        float _SpecularExp;
        float _BPSpecularIntensity;
        float _EmissionIntensity;
        float _RimLightWidth;
        float _RimLightThreshold;
        float _RimLightIntensity;
        float _OutlineWidth;
        half4 _OutlineColor;
        float3 _fDirWS;
        int _TimeShift;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_LightMap);
        SAMPLER(sampler_LightMap);
        TEXTURE2D(_MetalMap);
        SAMPLER(sampler_MetalMap);
        TEXTURE2D(_RampMap);
        SAMPLER(sampler_RampMap);
        TEXTURE2D_X_FLOAT(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);

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
                //��ȡ������Դ
                Light mainLight = GetMainLight();
                float4 mainLightColor = float4(mainLight.color, 1);

                //������Ҫ����
                float3 lDirWS = normalize(mainLight.direction); // ������Դ�����׼��
                float3 vDirWS = normalize(GetCameraPositionWS() - i.posWS); // ��ȡ���㵽����ı�׼����
                float3 nDirVS = TransformWorldToView(i.nDirWS); // �����㷨���������磩ת�����ӿ�����ϵ
                float ndotl = dot(i.nDirWS, lDirWS); // �ӿڶ��㷨�����͹�Դ���������н�
                float ndoth = dot(i.nDirWS, normalize(vDirWS + lDirWS));
                float ndotv = dot(i.nDirWS, vDirWS);
                float ldotv = dot(lDirWS, vDirWS);

                //�������
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float4 lightMap = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, i.uv);

                //ͨ�����
                float AOMask = saturate(lightMap.g * 2.0);
                float shadowMask = step(0.8, lightMap.g);
                float MatID = lightMap.a * 0.45;
                float nonmetalSpecMask = saturate(sign(0.45 - abs(0.45 - lightMap.r)));
                float metalSpecMask = step(0.8, lightMap.r);
                float specularIntensity = lightMap.b;
                float emissionMask = step(0.5, baseMap.a);

                //���ռ���
                float lambert = clamp(ndotl + AOMask - 1, -1, 1);
                float lambertRemap = smoothstep(-_ShadowOffset, -_ShadowOffset + _ShadowRampWidth, lambert);
                float halfLambert = lambert * 0.5 + 0.5;
                float rampSampler = remap(halfLambert, 0, 1, _ShadowRampThreshold, 1);
                float2 rampUV = float2(clamp(rampSampler, 0.1, 0.9), MatID + (_TimeShift - 1.0) * 0.5 - 0.1);
                half4 rampShadow = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampUV);

                half4 shadowLayer = step(0.1, lambert) + lerp(rampShadow, max(mainLightColor, rampShadow), 1 - _BoundaryIntensity) * baseMap;
                half4 litLayer = lerp(rampShadow, max(mainLightColor, rampShadow), lambertRemap) * baseMap;
                half4 diffuse = lerp(min(shadowLayer, litLayer), baseMap, shadowMask); // diffuse done

                float nonmetalSpec = step(1 - _ViewSpecularWidth, saturate(ndotv) * step(0, lambert)) * _NonmetalSpecularIntensity * nonmetalSpecMask * specularIntensity;
                nonmetalSpec += pow(saturate(ndoth), _SpecularExp) * nonmetalSpecMask * specularIntensity * _BPSpecularIntensity;
                float metalMap = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, vDirWS.xy * 0.5 + 0.5).r;
                float metalSpec = metalMap * _MetalSpecularIntensity * metalSpecMask * specularIntensity;
                half4 specular = lerp(nonmetalSpec, metalSpec * baseMap, specularIntensity); // specular done

                half4 emission = _EmissionIntensity * baseMap * emissionMask; // emission done

                float2 scrUV = float2(i.posCS.x / _ScreenParams.x, i.posCS.y / _ScreenParams.y);
                float2 offsetPos = scrUV + float2(nDirVS.xy * _RimLightWidth * clamp(-ldotv, 0.5, 1) / i.posCS.w);
                float offsetDepth = LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, offsetPos), _ZBufferParams);
                float originalDepth = LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, scrUV), _ZBufferParams);
                float rimMask = step(0.25, smoothstep(0, _RimLightThreshold, offsetDepth - originalDepth));
                half4 rimLight = rimMask * baseMap * _RimLightIntensity; // rimLight done

                // sample the texture
                half4 col = diffuse + specular + emission + rimLight;
                return col;
            }
            ENDHLSL
        }
    }
}
