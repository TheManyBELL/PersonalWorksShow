Shader "Unlit/CharacterShaderFace"
{
    Properties
    {
        [Header(Texture)]
        _BaseMap ("Texture", 2D) = "white" {} // 基础纹理贴图
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
                //光源
                Light mainLight = GetMainLight(); 
                float4 mainLightColor = float4(mainLight.color, 1); 

                //方向
                float3 uDirWS = float3(0,1,0);
                float3 fDirWS = _fDirWS;
                float3 rDirWS = cross(uDirWS,fDirWS);
                float3 lDirWS = normalize(mainLight.direction);
                
                //纹理
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float4 lightMap = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, i.uv); 

                //光照
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
