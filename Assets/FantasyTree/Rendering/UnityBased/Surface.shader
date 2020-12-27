Shader "FantasyTree/Surface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Bumpmap", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _TipColorMultiplier("Tip color multiplier" , Range(0,10)) = .5            
        _BaseColorMultiplier("Base color multiplier" , Range(0,10)) = .5            
        _AlphaCutoff ("AlphaCuttoff", Range(0,1)) = 0.0            
        _FallingAmount("falling amount" , Range(0,1)) = .5    
        _AmountShown ("AmountShown", Range (0, 1)) = .5
        [Toggle] _FadeFromTop("fadeFromTop", Float) = 0
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard  vertex:vert fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
            sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float  life;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;


        

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
            float3 lerpDown( float timeInBranch, float lerp){

               float lerpVal = lerp;

                
                lerpVal =  (1-lerp)-timeInBranch * .9;
                lerpVal = lerpVal;
                lerpVal *= 9;
                lerpVal = saturate(lerpVal);

                return 1-lerpVal;

            }
            
        
            float _AmountShown;
            float _FadeFromTop;
            float _AlphaCutoff;
            float _TipColorMultiplier;
            float _BaseColorMultiplier;
           
        void vert (inout appdata_full v,out Input o) {
            
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.life = v.texcoord2.z;

          v.vertex.xyz=  lerp( v.texcoord1.xyz, v.vertex.xyz , saturate(abs(-_FadeFromTop+lerpDown(1-_AmountShown, v.texcoord2.z))));
          
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex ) * _Color;
            o.Albedo = c.rgb *  lerp(_BaseColorMultiplier, _TipColorMultiplier, IN.life);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
            o.Alpha = c.a;

           if( c.a<_AlphaCutoff){ discard;}
        }
        ENDCG
    }
    FallBack "Diffuse"
}
