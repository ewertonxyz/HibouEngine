#version 450 core

layout (location = 0) in vec3 vertexPosition;
layout (location = 1) in vec3 vertexNormal;
layout (location = 2) in vec2 vertexTextureCoordinate;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

out vec3 fragmentNormal;
out vec3 fragmentPosition;
out vec2 fragmentTextureCoordinate;

void main()
{
    vec4 worldPosition = modelMatrix * vec4(vertexPosition, 1.0);
    fragmentPosition = vec3(worldPosition);
    
    mat3 normalMatrix = transpose(inverse(mat3(modelMatrix)));
    fragmentNormal = normalMatrix * vertexNormal;
    
    fragmentTextureCoordinate = vertexTextureCoordinate;
    
    gl_Position = projectionMatrix * viewMatrix * worldPosition;
}