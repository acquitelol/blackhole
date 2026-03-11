#version 330

in vec2 fragTexCoord;
out vec4 fragColor;

uniform sampler2D texture0;
uniform vec2 resolution;

uniform vec3 camPos;
uniform vec3 camTarget;
uniform vec3 camUp;

const float PI = 3.141592;
const int factor = 8;
const int RAY_STEPS = 4000 / factor;
const float STEP_SIZE = 0.01 * factor;

const float SCALE = 2;
const float MASS = 2 * SCALE;
const float RADIUS = 1 * SCALE;

const float DISK_INNER = RADIUS * SCALE;
const float DISK_OUTER = RADIUS * PI * SCALE;
const float DISK_BRIGHTNESS = 5.0;

const float PHOTON_SPHERE_R  = RADIUS * 1.5;
const float PHOTON_GLOW_WIDTH = RADIUS * 1;
const float PHOTON_BRIGHTNESS = 0.5;

vec4 sampleBackground(vec3 dir) {
	dir = normalize(dir);
	float u = 0.5 + atan(dir.z, dir.x) / (2.0 * PI);
	float v = 0.5 - asin(clamp(dir.y, -1.0, 1.0)) / PI;
	return texture(texture0, vec2(u, v));
}

void main() {
	vec2 uv = (fragTexCoord * 2.0 - 1.0) * vec2(resolution.x / resolution.y, 1.0);

	vec3 forward = normalize(camTarget - camPos);
	vec3 right = normalize(cross(forward, camUp));
	vec3 up = cross(right, forward);

	vec3 rayPos = camPos;
	vec3 rayDir = normalize(forward + uv.x * right + uv.y * up);

	bool hitHorizon = false;
	vec3 diskGlow = vec3(0.0);
	float diskAlpha = 0.0;
	float prevY = rayPos.y;

	for (int i = 0; i < RAY_STEPS; i++) {
		float dist = length(rayPos);
		if (dist < RADIUS) { hitHorizon = true; break; }

		float photonDist = abs(dist - PHOTON_SPHERE_R);

		if (photonDist < PHOTON_GLOW_WIDTH) {
			float t = 1.0 - (photonDist / PHOTON_GLOW_WIDTH);
			t = pow(t, 2.5);
			float angle  = atan(rayPos.z, rayPos.x);
			float doppler = 0.6 + 0.4 * sin(angle);
			vec3 photonColor = vec3(0.8, 0.75, 0.7) * doppler;
			diskGlow += photonColor * t * PHOTON_BRIGHTNESS * STEP_SIZE;
		}

		float currY = rayPos.y;

		if (prevY * currY < 0.0) { // ray crossed y = 0 plane
			float r = length(rayPos.xz);

			if (r > DISK_INNER && r < DISK_OUTER) {
				float t = clamp((r - DISK_INNER) / (DISK_OUTER - DISK_INNER), 0.0, 1.0);
				vec3 dc = mix(
					mix(vec3(1.0, 0.95, 0.8), vec3(1.0, 0.5, 0.1), t),
					vec3(0.4, 0.05, 0.0),
					pow(t, 2.5)
				);
				//
				// vec3 dc = mix(
				// 	mix(vec3(0.8, 0.6, 0.8), vec3(1.0, 0.5, 0.1), t),
				// 	vec3(0.4, 0.05, 0.4),
				// 	pow(t, 2.5)
				// );

				float angle = atan(rayPos.z, rayPos.x);
				float doppler = 0.55 + 0.45 * sin(angle);
				dc *= doppler;

				float contribution = (1.0 - t) * DISK_BRIGHTNESS * (1.0 - diskAlpha);
				diskGlow += dc * contribution;
				diskAlpha = min(diskAlpha + 0.7, 1.0);
			}
		}

		prevY = currY;

		// newtonian gravitational acceleration
		// a = −GM * r / ∣r∣^3
		vec3 grav = -rayPos * MASS / (dist * dist * dist);
		rayDir += grav * STEP_SIZE;
		rayPos += rayDir * STEP_SIZE;
	}

	if (hitHorizon) {
		fragColor = vec4(diskGlow, 1.0);
	} else {
		fragColor = vec4(sampleBackground(rayDir).rgb + diskGlow, 1.0);
	}
}
