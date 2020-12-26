Shader "VolumetricLightRays/withDust"
{
    Properties
    {
    _Color ("Color", Color) = (1,1,1,1)
    _DustColor ("DustColor", Color) = (1,1,1,1)
    _ValueMultiplier ("Value Multiplier", float) = .01
    _RaySize ("Ray Size", float) = .01
    _FadeIn ("_FadeIn", float) = .01
    _FadeOut ("_FadeOut", float) = .01
       _DustMap("_DustMap", 2D) = "white" {}
    }
    SubShader
    {
        // inside SubShader
Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

// inside Pass
ZWrite Off
Blend One One
        LOD 100

        Pass
        {

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
                float2 uv2 : TEXCOORD5;
                float depth : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 endPos : TEXCOORD2;
                float3 startPos : TEXCOORD4;
                float3 world : TEXCOORD3;
                float4 screenPos : TEXCOORD6;
                float id : TEXCOORD7;
            };

     
      float hash( float n ){
        return frac(sin(n)*43758.5453);
      }

       sampler2D _DepthTexture;
       sampler2D _DustMap;

       float _RaySize;
       float _ValueMultiplier;
       float4 _Color;
       float4 _DustColor;


        float4x4 _CameraMatrix;
        float2 _CameraSize;
        float _CameraNear;
        float _CameraFar;

        float _FadeIn;
        float _FadeOut;
    

            v2f vert ( uint vid : SV_VertexID )
            {
                v2f o;

                int pID = vid/6;
                 float x  = hash( pID * 100 )* .8 + .1;
                float y  = hash( pID * 121 )* .8 + .1;

                x += .1 * sin(.3*_Time.y * hash(pID * 12121));
                y += .1 * sin(.3*_Time.y * hash(pID * 1121121));
//
         

                float3 forward  = normalize(mul( _CameraMatrix , float4(0,0,1,0)).xyz);
                float3 up       = normalize(mul( _CameraMatrix , float4(0,1,0,0)).xyz);
                float3 left     = normalize(mul( _CameraMatrix , float4(1,0,0,0)).xyz);
                float3 pos = mul(_CameraMatrix,float4(0,0,0,1)).xyz;

                float3 startPos = 2*(x - .5) * _CameraSize.x * left + 2*(y-.5) * _CameraSize.y * up + forward * _CameraNear + pos;

                float depth = tex2Dlod(_DepthTexture,float4(x,y,0,0));
                o.depth = depth;
                float3 endPos = startPos + forward * (1-depth) * ( _CameraFar - _CameraNear);

                float3 p1 = startPos - .1*_RaySize*normalize(cross(UNITY_MATRIX_V[2], forward));
                float3 p2 = startPos + .1*_RaySize*normalize(cross(UNITY_MATRIX_V[2], forward));;
                float3 p3 = endPos   - .1*_RaySize*normalize(cross(UNITY_MATRIX_V[2], forward));
                float3 p4 = endPos   + .1*_RaySize*normalize(cross(UNITY_MATRIX_V[2], forward));


                float3 fPos = 0;
                float2 fUV = 0;


                int which = vid % 6;

                if( which == 0 || which == 3 ){
                    fPos = p1; fUV = float2(0,0);
                }else if( which == 2 || which == 4 ){
                    fPos = p4; fUV = float2(1,1);               
                }else if( which == 1 ){
                    fPos = p2; fUV = float2(1,0);
                }else{
                    fPos = p3; fUV = float2(0,1);
                }
                

                float col = hash(pID *3131);
                col *= 30;
                col = floor(col)/30;

                o.uv = fUV;
                o.uv2 = fUV * float2( .01,(1-depth)) + float2( hash(pID *415) , hash(pID * 4651));
                o.endPos = endPos;
                o.startPos = startPos;
                o.world = fPos;

                
                
                o.vertex = mul( UNITY_MATRIX_VP , float4(fPos,1));
                o.screenPos = ComputeScreenPos(o.vertex);
                o.id = pID;
                return o;
            }
    
            fixed4 frag (v2f v) : SV_Target
            {


                 //depth = pow(Linear01Depth(depth), _DepthLevel);
                float d = length( v.world - v.endPos );
                float endFade = saturate(d * _FadeOut);


                d = length( v.world - v.startPos );
                float startFade = saturate(d * _FadeIn);

                float4 dust = _DustColor * tex2D(_DustMap,v.uv2 *40).r;
               
               

                // sample the texture

                float l = saturate((.5-length(v.uv.x-.5)) * 2);
                fixed4 col = .002 * l * _ValueMultiplier * _Color;// tex2D(_DepthTexture, i.uv).a;


                col = dust;


                
                col *= endFade * startFade;
                col *= endFade * startFade;
                //col = dust;
                return col;
            }
            ENDCG
        }
    }
}
