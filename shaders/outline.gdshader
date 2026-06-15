shader_type canvas_item;
render_mode skip_vertex_transform;

uniform int nearRadius = 2;
uniform float vertextScale = 2.0;

const vec3 outlineColor = vec3(1.0, 1.0, 0.0);

varying vec2 s;

vec4 clampedTexture(sampler2D t, vec4 r, vec2 uv) {
	vec2 ruv = (uv - r.xy) / r.zw;
	
	return mix(texture(t, uv), vec4(0.0),
		clamp(
		float(ruv.x < 0.0) +
		float(ruv.y < 0.0) +
		float(ruv.x > 1.0) +
		float(ruv.y > 1.0),
		0.0, 1.0)
	);
}

float isNear(vec2 uv, sampler2D t, vec2 pixelSize, vec4 r) {
	float near = 0.0;
	for (int x = 0; x < nearRadius * 2 + 1; x++)
	{
		for (int y = 0; y < nearRadius * 2 + 1; y++) {
			vec4 color = clampedTexture(t, r, uv + vec2(float(x - nearRadius), float(y - nearRadius)) * pixelSize);
			near = max(color.a, near);
		}
	}
	return near;
}

void vertex() {
	VERTEX *= vertextScale;
	VERTEX = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	vec2 uv = UV;
	uv -= REGION_RECT.xy;
	uv /= REGION_RECT.zw;
	uv -= 0.5;
	uv *= vertextScale;
	uv += 0.5;
	uv *= REGION_RECT.zw;
	uv += REGION_RECT.xy;
	
	COLOR = clampedTexture(TEXTURE, REGION_RECT, uv);
	float n = isNear(uv, TEXTURE, TEXTURE_PIXEL_SIZE, REGION_RECT);
	COLOR = mix(vec4(outlineColor, n), COLOR, COLOR.a);
}
