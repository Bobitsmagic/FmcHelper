#version 400
precision highp float;

uniform mat4x4 PVMatrix;
uniform mat4x4 ModelMatrix;

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec4 vColor;
layout(location = 3) in vec3 vNormal;

void main()
{	
	gl_Position = PVMatrix * ModelMatrix * vec4(vPosition, 1);
}