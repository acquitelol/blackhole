#version 330

in vec2 fragTexCoord;
out vec4 fragColor;

uniform sampler2D texture0;

uniform vec2 blackhole;
uniform vec2 resolution;
uniform float eventHorizon;

const int RAY_STEPS = 300;
const float MASS = 0.05;

void main() {
    float aspect = resolution.x / resolution.y;

    vec2 rayPos = fragTexCoord * vec2(aspect, 1.0);
    vec2 bhPos = (blackhole / resolution) * vec2(aspect, 1.0);
    float horizon = eventHorizon / resolution.y;

    if (length(rayPos - bhPos) < horizon) {
        fragColor = vec4(0.0);
        return;
    }

    vec2 pos = rayPos;
    vec2 vel = vec2(0.0);
    float stepSize = 1.0 / float(RAY_STEPS);

    for (int i = 0; i < RAY_STEPS; i++) {
        vec2 toBlackHole = bhPos - pos;
        float depth = 0.5 - float(i) * stepSize;
        float dist = sqrt(dot(toBlackHole, toBlackHole) + depth * depth);

        vec2 gravitationalPull = toBlackHole * (MASS / (dist * dist * dist));
        vel += gravitationalPull * stepSize;
        pos += vel * stepSize;
    }

    vec2 lensedUV = pos / vec2(aspect, 1.0);
    float edgeSoftness = smoothstep(horizon, horizon * 1.1, length(rayPos - bhPos));
    fragColor = mix(vec4(0.0), texture(texture0, clamp(lensedUV, 0.0, 1.0)), edgeSoftness);
}
