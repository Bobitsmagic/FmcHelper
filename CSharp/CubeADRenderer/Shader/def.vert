#version 400
uniform mat4x4 PVMatrix;
uniform mat4x4 ModelMatrix;

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec4 vColor;
layout(location = 3) in vec3 vNormal;

out vec4 fColor;
out vec3 fNormal;
out vec3 fPosition;

void main()
{	
	fColor = vColor;
	fPosition = (ModelMatrix * vec4(vPosition, 1)).xyz;
	fNormal = (ModelMatrix * vec4(vNormal, 0)).xyz;

	gl_Position = PVMatrix * ModelMatrix * vec4(vPosition, 1);
}