#version 400
precision highp float;

uniform vec3 AmbientDirection;

in vec4 fColor;
in vec3 fNormal;
in vec3 fPosition;

out vec4 outColor;

void main()
{	
	outColor = fColor * abs(dot(fNormal, AmbientDirection));
}