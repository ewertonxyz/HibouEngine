#version 450 core

in vec3 fragmentNormal;
in vec3 fragmentPosition;
in vec2 fragmentTextureCoordinate;

out vec4 finalColor;

uniform vec3 lightDirection;
uniform vec3 lightColor;
uniform vec3 objectColor;

void main()
{
    vec3 normalizedNormal = normalize(fragmentNormal);
    vec3 normalizedLightDirection = normalize(-lightDirection);
    
    float diffuseImpact = max(dot(normalizedNormal, normalizedLightDirection), 0.0);
    vec3 diffuseLighting = diffuseImpact * lightColor;
    
    vec3 ambientLighting = 0.2 * lightColor;
    
    vec3 resultLighting = (ambientLighting + diffuseLighting) * objectColor;
    finalColor = vec4(resultLighting, 1.0);
}