Shader "Unlit/CharacterShaderBody"
{
    Properties
    {
        [Header(Texture)]
        _BaseMap ("Texture", 2D) = "white" {} // 基础纹理贴图
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
            "RenderPipeline"="UniversalPipeline" // 走自定义渲染管线，需要在Setting中设置
        }
        LOD 100
        HLSLINCLUDE
        #pragma vertex vert // 使用了预处理指令，指定使用定义的顶点着色器（vert）
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

        //顶点结构体
        struct appdata
        {
            float4 posOS            : POSITION;
            float2 uv               : TEXCOORD0;
            float3 normalOS         : NORMAL;
            float4 tangentOS        : TANGENT;
            float4 vertexColor      : COLOR;
        };

        //片元结构体
        struct v2f
        {
            float4 posCS            : SV_POSITION;
            float2 uv               : TEXCOORD0;
            float3 posWS            : TEXCOORD1;
            float3 nDirWS           : TEXCOORD2;
            float4 col              : COLOR;
        };

        //Ramp采样函数
        float remap(float x, float t1, float t2, float s1, float s2)
        {
            return (x - t1) / (t2 - t1) * (s2 - s1) + s1;
        }
        ENDHLSL

        Pass
        {
            Cull Off // 表示不进行剔除，即不对模型的背面进行渲染剔除。该设置将使模型的前面和背面都可见。
            Name "Forward" // 给当前Pass设置一个名称，以便在需要引用或识别该Pass时使用。
            Tags{ "LightMode" = "UniversalForward" } // 这个Pass使用的是Universal渲染管线的前向渲染方式。
            

            HLSLPROGRAM
            //CBUFFER_END
            v2f vert(appdata v)
            {
                v2f o;
                o.posCS = TransformObjectToHClip(v.posOS.xyz); // 将顶点坐标转换到裁剪坐标系
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap); // 根据uv坐标对Diffuse贴图进行采样
                o.posWS = TransformObjectToWorld((float3)v.posOS); // 获取世界坐标
                o.nDirWS = TransformObjectToWorldNormal(v.normalOS); // 获取世界坐标系下的法向量
                o.col = v.vertexColor; // 获取顶点颜色
                return o;
            }

            // half4是半精度浮点数4分量颜色，低精度高性能
            half4 frag(v2f i) : SV_Target
            {
                //获取场景光源
                Light mainLight = GetMainLight();
                float4 mainLightColor = float4(mainLight.color, 1);

                //计算主要方向
                float3 lDirWS = normalize(mainLight.direction); // 将主光源方向标准化
                float3 vDirWS = normalize(GetCameraPositionWS() - i.posWS); // 获取顶点到相机的标准向量
                float3 nDirVS = TransformWorldToView(i.nDirWS); // 将顶点法向量（世界）转换到视口坐标系
                float ndotl = dot(i.nDirWS, lDirWS); // 视口顶点法向量和光源方向法向量夹角
                float ndoth = dot(i.nDirWS, normalize(vDirWS + lDirWS));
                float ndotv = dot(i.nDirWS, vDirWS);
                float ldotv = dot(lDirWS, vDirWS);

                //纹理采样
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float4 lightMap = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, i.uv);

                //通道拆分
                float AOMask = saturate(lightMap.g * 2.0);
                float shadowMask = step(0.8, lightMap.g);
                float MatID = lightMap.a * 0.45;
                float nonmetalSpecMask = saturate(sign(0.45 - abs(0.45 - lightMap.r)));
                float metalSpecMask = step(0.8, lightMap.r);
                float specularIntensity = lightMap.b;
                float emissionMask = step(0.5, baseMap.a);

                //光照计算
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
