#version 400

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec4 vColor;
layout(location = 3) in vec3 vNormal;

uniform mat4x4 PVMatrix;
//uniform mat4x4 ModelMatrix;

out vec2 fTexCoord;

void main()
{	
	fTexCoord = (vPosition.xy + vec2(1, 1)) / 2;

	gl_Position = vec4(vPosition, 1);

	//gl_Position = vec4(vPosition, 0);
} 