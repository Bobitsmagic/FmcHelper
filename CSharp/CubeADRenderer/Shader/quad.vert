#version 400

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec4 vColor;
layout(location = 3) in vec3 vNormal;

out vec2 fTexCoords;

void main()
{	
	fTexCoords = vPosition.xy;
	gl_Position = vec4(vPosition, 0);
} 