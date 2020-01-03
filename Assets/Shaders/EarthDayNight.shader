// Adapted from https://wiki.unity3d.com/index.php/Earth/Planet
// -- Earth Shader created by Julien Lynge @ Fragile Earth Studios
// -- Upgrade of a shader originally put together in Strumpy Shader Editor by Clamps

Shader "Custom/EarthDayNight" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Normals("Normal Map", 2D) = "black" {}
        _Lights("Lights", 2D) = "black" {}
        _Specular("Specular", 2D) = "black" {}
        _Clouds("Clouds", 2D) = "black" {}
        _LightScale("Light Scale", Float) = 1
        _AtmosNear("Atmos Near", Color) = (0.169,0.737,1,1)
        _AtmosFar("Atmos Far", Color) = (0.45,0.519,0.985,1)
        _AtmosFalloff("Atmos Falloff", Float) = 3
        _LightsEmission ("Lights Emission", Float) = 0
        _Shininess("Shininess", Float) = 10
        _SpecularIntensity("Specular Intensity", Color) = (0.5,0.5,0.5,1)
        _CloudSpeed("Clouds Speed", Float) = -0.02

    }
    SubShader {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry"
            "IgnoreProjector"="False"
        }
        Cull Back
        ZWrite On
        ZTest LEqual
        ColorMask RGBA
        LOD 200
        
        CGPROGRAM
        #pragma surface surf BlinnPhongCustom

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Normals;
        sampler2D _Lights;
        sampler2D _Specular;
        sampler2D _Clouds;
        float _LightScale;
        float4 _AtmosNear;
        float4 _AtmosFar;
        float _AtmosFalloff;
        float _LightsEmission;
        float _Shininess;
        float _CloudSpeed;
        float4 _SpecularIntensity;
        
        struct CustomSurfaceOutput
        {
            half3 Albedo;
            half3 Normal;
            half3 Emission;
            half3 Gloss;
            half3 Specular;
            half Alpha;
            half4 Custom;
        };
        
        inline half4 LightingBlinnPhongCustom_PrePass (CustomSurfaceOutput s, half4 light)
        {
            half3 spec = s.Gloss;
            half4 c;
            c.rgb = (s.Albedo * light.rgb + light.rgb * spec.rgb);
            c.g -= .01 * s.Alpha;
            c.r -= .03 * s.Alpha;
            c.rg += min(s.Custom, s.Alpha);
            c.b += 0.75 * min(s.Custom, s.Alpha);
            c.a = 1.0;
            return c;
        }

        inline half4 LightingBlinnPhongCustom (CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half3 h = normalize (lightDir + viewDir);
    
            half diff = max (0, dot ( lightDir, s.Normal ));
    
            float nh = max (0, dot (s.Normal, h));
            float spec = pow (nh, s.Specular*128.0);
    
            half4 res;
            res.rgb = _LightColor0.rgb * diff;
            res.w = spec * Luminance (_LightColor0.rgb);
            res *= atten * 2.0;
    
            // specular highlights
            float3 lightReflectDirection = reflect(-lightDir, s.Normal);
            float3 lightSeeDirection = max(0.0,dot(lightReflectDirection, viewDir));
            float3 shininessPower = pow(lightSeeDirection, _Shininess);
            // float3 shininessPower = smoothstep(0, 1, lightSeeDirection);
            float3 specularReflection = atten * s.Specular.rgb * shininessPower;      
            // pass specular highlights as Gloss
            s.Gloss = specularReflection;
                 
            //s.Alpha is set to 1 where the earth is dark.  The value of night lights has been saved to Custom
            half invdiff = 1 - saturate(16 * diff);
            s.Alpha = invdiff;
            
            return LightingBlinnPhongCustom_PrePass( s, res );
        }

        struct Input {
            float3 viewDir;
            float2 uv_MainTex;
            float2 uv_Normals;
            float2 uv_Lights;
            float2 uv_Specular;
            float2 uv_Clouds;
        };
        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout CustomSurfaceOutput o)
        {
            o.Gloss = 0.0;
            o.Custom = 0.0;
            o.Alpha = 1.0;
            
            float4 BasicOutline = float4(0,0,1,1);
            
            // inverse effect - needs to be stronger at the edges
            float4 FresnelSimple = (1.0 - dot( 
                normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z, 1.50).xyz), 
                normalize( BasicOutline.xyz ) )).xxxx;
            
            // Apply atmospheric falloff color
            float4 Pow0 = pow( FresnelSimple, _AtmosFalloff.xxxx);
            float4 Saturate0 = saturate(Pow0);
            // Make gradient between near and far atmospheric falloff
            float4 Lerp0 = lerp(_AtmosNear, _AtmosFar, Saturate0);
            
            float4 FresnelEffect = Lerp0 * Saturate0;
            float4 MainTex2D = tex2D(_MainTex, IN.uv_MainTex.xy);
            
            // animate moving clouds
            float2 animatedCloudUV = IN.uv_Clouds.xy;
            animatedCloudUV.x += _CloudSpeed * _Time.x;
            float4 CloudsTex2D = tex2D(_Clouds, animatedCloudUV);
            
            // combine main texture, fresnel, and clouds
            float4 FinalMainTex = FresnelEffect + MainTex2D + CloudsTex2D;
            
            float4 Normals2D = tex2D(_Normals,IN.uv_Normals.xy);
            float4 UnpackNormal0 = float4(UnpackNormal(Normals2D).xyz, 1.0);
            
            float4 Lights2D = tex2D(_Lights,IN.uv_Lights.xy);
            float4 LightsTex = Lights2D * _LightScale.xxxx;
            
            float4 SpecularTex = tex2D(_Specular, IN.uv_Specular.xy) * _SpecularIntensity;

            o.Albedo = FinalMainTex;
            o.Normal = UnpackNormal0;
            o.Specular = SpecularTex;
            o.Emission = LightsTex * _LightsEmission;
            o.Custom = tex2D(_Lights, IN.uv_Lights.xy).r * _LightScale;
            o.Normal = normalize(o.Normal);
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}