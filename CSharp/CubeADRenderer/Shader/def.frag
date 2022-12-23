#version 400 core
layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gColorSpec;

in vec4 fColor;
in vec3 fNormal;
in vec3 fPosition;

void main()
{    
    // store the fragment position vector in the first gbuffer texture
    
    gPosition = fPosition;
    // also store the per-fragment normals into the gbuffer
    gNormal = normalize(fNormal);
    // and the diffuse per-fragment color
    gColorSpec = fColor;
//    gNormal = vec3(0, 1, 0);
//    gPosition = vec3(0, 0, 1);
//    gColorSpec = vec4(1, 0, 0, 1);
} 