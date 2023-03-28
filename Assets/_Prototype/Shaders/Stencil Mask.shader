Shader "Custom/StencilMask"
{
    Properties
    {
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque" 
            "RenderPipeLine" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        Blend Zero One
        ZWrite off
        
        Pass {
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
                Fail Keep
            }
        }
    }
}
