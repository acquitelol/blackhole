#version 330

in vec2 fragTexCoord;
out vec4 fragColor;

uniform sampler2D texture0;
uniform vec2 blackhole;
uniform vec2 resolution;

const float STRENGTH = 3000000.0;

void main() {
    vec2 uv = fragTexCoord;
    vec2 pos = uv * resolution;

    vec2 d = pos - blackhole;
    float dist = length(d) + 0.001;

    float offset = STRENGTH / (dist * dist + 1.0);
    vec2 lensed_pos = pos - (d / dist) * offset;
    vec2 lensed_uv = lensed_pos / resolution;
    lensed_uv = clamp(lensed_uv, 0.0, 1.0);

    lensed_uv.y = 1.0 - lensed_uv.y;
    fragColor = texture(texture0, lensed_uv);
}
