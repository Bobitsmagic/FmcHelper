#version 400
precision highp float;

uniform vec3 AmbientDirection;
uniform vec3 CamPos;
uniform vec3 LightPos;
uniform vec3 LightColor;

uniform mat4 PVMatrix;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gColorSpec;
uniform sampler2D DepthMap;

in vec2 fTexCoord;

out vec4 outColor;

float ShadowCalculation(vec4 fragPosLightSpace, vec4 normal)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = (projCoords + 1) / 2;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(DepthMap, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // check whether current frag pos is in shadow
    float shadow = currentDepth - 0.05 > closestDepth  ? 0.2 : 1;
    
    return normal.z >= 0 ? 1 : shadow;
} 

void main()
{	
    // retrieve data from G-buffer
    vec3 FragPos = texture(gPosition, fTexCoord).rgb;
    vec3 Normal = texture(gNormal, fTexCoord).rgb;
    vec3 Color = texture(gColorSpec, fTexCoord).rgb;
    float Spec = texture(gColorSpec, fTexCoord).a;

    vec2 lightCoord = (PVMatrix * vec4(FragPos, 1)).xy;
    float LightDepth = texture(DepthMap, lightCoord).r;
    
    // then calculate lighting as usual
    const float MIN_LIGHT = 0.2;
    vec3 lighting = Color * (MIN_LIGHT + (1 - MIN_LIGHT) * max(0, -dot(Normal, AmbientDirection)));
    
    float shadow = ShadowCalculation(PVMatrix * vec4(FragPos, 1), PVMatrix * vec4(Normal, 0)); 
    lighting *= shadow;
    outColor = vec4(lighting, 1);
    //outColor = vec4(shadow, shadow, shadow, 1);
}