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
    vec3 viewDir = normalize(CamPos - FragPos);
    vec3 lighting = Color * max(0.1f, abs(dot(AmbientDirection, Normal)));

    //light calc
    outColor = vec4(FragPos, 1);
    //outColor = vec4(1, 0, 0, 1);
}