#version 400
precision highp float;

uniform vec3 AmbientDirection;
uniform vec3 CamPos;
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gColorSpec;

in vec2 fTexCoord;

out vec4 outColor;

void main()
{	
    // retrieve data from G-buffer
    vec3 FragPos = texture(gPosition, fTexCoord).rgb;
    vec3 Normal = texture(gNormal, fTexCoord).rgb;
    vec3 Color = texture(gColorSpec, fTexCoord).rgb;
    float Spec = texture(gColorSpec, fTexCoord).a;
    
    // then calculate lighting as usual
    vec3 lighting = Color * 0.1; // hard-coded ambient component
    vec3 viewDir = normalize(CamPos - FragPos);
}