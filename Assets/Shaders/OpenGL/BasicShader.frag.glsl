#version 450 core

in vec3 v_WorldPosition;
in vec3 v_Normal;
in vec2 v_TexCoord;

out vec4 o_Color;

uniform sampler2D u_AlbedoTexture;
uniform sampler2D u_NormalTexture;
uniform sampler2D u_MetallicRoughnessTexture;
uniform sampler2D u_OcclusionTexture;

uniform int u_HasAlbedoTexture;
uniform int u_HasNormalTexture;
uniform int u_HasMetallicRoughnessTexture;
uniform int u_HasOcclusionTexture;

uniform vec3  u_BaseColorFactor;
uniform float u_MetallicFactor;
uniform float u_RoughnessFactor;

uniform vec3 u_CameraPosition;
uniform vec3 u_LightDirection;
uniform vec3 u_LightColor;

const float PI = 3.14159265359;

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a  = roughness * roughness;
    float a2 = a * a;
    float NdotH  = max(dot(N, H), 0.0);
    float denom  = NdotH * NdotH * (a2 - 1.0) + 1.0;
    return a2 / (PI * denom * denom);
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = roughness + 1.0;
    float k = (r * r) / 8.0;
    return NdotV / (NdotV * (1.0 - k) + k);
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    return GeometrySchlickGGX(max(dot(N, V), 0.0), roughness)
         * GeometrySchlickGGX(max(dot(N, L), 0.0), roughness);
}

vec3 FresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

mat3 ComputeTBN(vec3 N)
{
    vec3 Q1  = dFdx(v_WorldPosition);
    vec3 Q2  = dFdy(v_WorldPosition);
    vec2 st1 = dFdx(v_TexCoord);
    vec2 st2 = dFdy(v_TexCoord);

    float det = st1.s * st2.t - st2.s * st1.t;
    vec3 T = normalize((Q1 * st2.t - Q2 * st1.t) / det);
    vec3 B = normalize((Q2 * st1.s - Q1 * st2.s) / det);
    return mat3(T, B, N);
}

void main()
{
    vec3  albedo = u_BaseColorFactor;
    float alpha  = 1.0;

    if (u_HasAlbedoTexture != 0)
    {
        vec4 s = texture(u_AlbedoTexture, v_TexCoord);
        albedo = s.rgb * u_BaseColorFactor;
        alpha  = s.a;
    }

    vec3 N = normalize(v_Normal);

    if (u_HasNormalTexture != 0)
    {
        mat3 TBN = ComputeTBN(N);
        vec3 ns  = texture(u_NormalTexture, v_TexCoord).rgb * 2.0 - 1.0;
        N = normalize(TBN * ns);
    }

    float metallic  = clamp(u_MetallicFactor,  0.0, 1.0);
    float roughness = clamp(u_RoughnessFactor, 0.04, 1.0);

    if (u_HasMetallicRoughnessTexture != 0)
    {
        vec4 mr  = texture(u_MetallicRoughnessTexture, v_TexCoord);
        roughness = clamp(mr.g * u_RoughnessFactor, 0.04, 1.0);
        metallic  = clamp(mr.b * u_MetallicFactor,  0.0,  1.0);
    }

    float ao = 1.0;
    if (u_HasOcclusionTexture != 0)
        ao = texture(u_OcclusionTexture, v_TexCoord).r;

    vec3 V = normalize(u_CameraPosition - v_WorldPosition);
    vec3 L = normalize(-u_LightDirection);
    vec3 H = normalize(V + L);

    vec3 F0 = mix(vec3(0.04), albedo, metallic);

    float NDF = DistributionGGX(N, H, roughness);
    float G   = GeometrySmith(N, V, L, roughness);
    vec3  F   = FresnelSchlick(max(dot(H, V), 0.0), F0);

    vec3 specular = (NDF * G * F) /
                    (4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001);

    vec3 kD    = (vec3(1.0) - F) * (1.0 - metallic);
    float NdotL = max(dot(N, L), 0.0);
    vec3 Lo    = (kD * albedo / PI + specular) * u_LightColor * NdotL;

    vec3 ambient = vec3(0.03) * albedo * ao;
    vec3 color   = ambient + Lo;

    color = color / (color + vec3(1.0));
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));

    o_Color = vec4(color, alpha);
}
