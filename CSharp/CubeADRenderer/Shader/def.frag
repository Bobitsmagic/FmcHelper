#version 400 core
layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gColorSpec;

in vec4 fColor;
in vec3 fPos;
in vec3 fNormal;

void main()
{    
    // store the fragment position vector in the first gbuffer texture
    gPosition = fPos;
    // also store the per-fragment normals into the gbuffer
    gNormal = normalize(fNormal);
    // and the diffuse per-fragment color
    gColorSpec= fColor;
} 