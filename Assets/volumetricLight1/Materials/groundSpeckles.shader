Shader "VolumetricLightRays/groundSpeckles"
{
    Properties
    {
    _Color ("Color", Color) = (1,1,1,1)
    _ValueMultiplier ("Value Multiplier", float) = .01
    _RaySize ("Ray Size", float) = .01
    _NormalOffset ("Normal Offset", float) = .01
    _RayLocationOsscilationSpeed ("Ray Location Osscilation Speed", float) = .01
    _RayLocationOsscilationSize ("Ray Location Osscilation Size", float) = .01
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
                float depth : TEXCOORD1;
                float matchVal : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

     
      float hash( float n ){
        return frac(sin(n)*43758.5453);
      }

       sampler2D _DepthTexture;

       float _RaySize;
       float _NormalOffset;
       float _ValueMultiplier;
       float4 _Color;
       float _RayLocationOsscilationSpeed;
       float _RayLocationOsscilationSize;


        float4x4 _CameraMatrix;
        float2 _CameraSize;
        float _CameraNear;
        float _CameraFar;

            v2f vert ( uint vid : SV_VertexID )
            {
                v2f o;

                int pID = vid/6;

                float angle = hash(pID * 1212);
                float radius = hash(pID * 3141);

                angle = 2 * 3.14159 * angle;
             
               float x = sin( angle )* radius;
               float y = -cos( angle )* radius;



                x += _RayLocationOsscilationSize * sin(_RayLocationOsscilationSpeed*_Time.y * hash(pID * 12121));
                y += _RayLocationOsscilationSize * sin(_RayLocationOsscilationSpeed*_Time.y * hash(pID * 1121121));


                x += 1;
                x /= 2;

                y += 1;
                y /= 2;

                float3 forward  = normalize(mul( _CameraMatrix , float4(0,0,1,0)).xyz);
                float3 up       = normalize(mul( _CameraMatrix , float4(0,1,0,0)).xyz);
                float3 left     = normalize(mul( _CameraMatrix , float4(1,0,0,0)).xyz);
                float3 pos = mul(_CameraMatrix,float4(0,0,0,1)).xyz;

                float3 startPos = 2*(x - .5) * _CameraSize.x * left + 2*(y-.5) * _CameraSize.y * up + forward * _CameraNear + pos;


                float eps = .01;
                float3 startPosX = 2*(x + eps - .5) * _CameraSize.x * left + 2*(y-.5) * _CameraSize.y * up + forward * _CameraNear + pos;
                float3 startPosX1 = 2*(x - eps - .5) * _CameraSize.x * left + 2*(y-.5) * _CameraSize.y * up + forward * _CameraNear + pos;
                float3 startPosY = 2*(x - .5) * _CameraSize.x * left + 2*(y+eps-.5) * _CameraSize.y * up + forward * _CameraNear + pos;
                float3 startPosY1 = 2*(x - .5) * _CameraSize.x * left + 2*(y-eps-.5) * _CameraSize.y * up + forward * _CameraNear + pos;

                float depth = tex2Dlod(_DepthTexture,float4(x,y,0,0));
                float depthX = tex2Dlod(_DepthTexture,float4(x+eps,y,0,0));
                float depthY = tex2Dlod(_DepthTexture,float4(x,y+eps,0,0));
                float depthX1 = tex2Dlod(_DepthTexture,float4(x-eps,y,0,0));
                float depthY1 = tex2Dlod(_DepthTexture,float4(x,y-eps,0,0));


                o.depth = depth;

                float3 endPos =  startPos + forward * (1-depth) * ( _CameraFar - _CameraNear);
                float3 endPosX = startPosX + forward * (1-depthX) * ( _CameraFar - _CameraNear);
                float3 endPosX1 = startPosX1 + forward * (1-depthX1) * ( _CameraFar - _CameraNear);
                float3 endPosY = startPosY + forward * (1-depthY) * ( _CameraFar - _CameraNear);
                float3 endPosY1 = startPosY1 + forward * (1-depthY1) * ( _CameraFar - _CameraNear);

                float3 nor = normalize(cross(endPosX - endPosX1 , endPosY-endPosY1));

                float m = abs(dot(forward,nor));

                float3 xDir = normalize(cross(nor,float3(1,0,0)));
                float3 yDir = normalize(cross(nor,xDir));
                 xDir = normalize(cross(nor,yDir));

                 float fSize = _RaySize;// * m;
                 o.matchVal = m;

                float3 p1 = endPos-nor *.1 *hash(pID*131) * (.8 +.2) - xDir * fSize - yDir* fSize;
                float3 p2 = endPos-nor *.1 *hash(pID*131) * (.8 +.2) + xDir * fSize - yDir* fSize;
                float3 p3 = endPos-nor *.1 *hash(pID*131) * (.8 +.2) - xDir * fSize + yDir* fSize;
                float3 p4 = endPos-nor *.1 *hash(pID*131) * (.8 +.2) + xDir * fSize + yDir* fSize;


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
                
                o.uv = fUV;
                
                o.vertex = mul( UNITY_MATRIX_VP , float4(fPos,1));

                return o;
            }
    
            fixed4 frag (v2f v) : SV_Target
            {


                 //depth = pow(Linear01Depth(depth), _DepthLevel);

                // sample the texture

                float l = saturate((.5-length(v.uv-.5)) * 2);
                fixed4 col = .002 * l * _ValueMultiplier * _Color * (v.matchVal * .3 + .7) * pow(v.depth,2);//*v.depth *v.depth * v.depth * v.depth * v.depth;// tex2D(_DepthTexture, i.uv).a;
                //col = 1;
                return col;
            }
            ENDCG
        }
    }
}
