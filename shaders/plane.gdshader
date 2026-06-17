shader_type canvas_item;
render_mode skip_vertex_transform;

uniform float zoom = 1;
uniform float bgZoom = 2;
uniform vec2 offset = vec2(0);

void vertex() {
	VERTEX = (inverse(SCREEN_MATRIX) * vec4(VERTEX - 1.0, 0, 1)).xy;
}

void fragment() {
	float z = zoom * bgZoom;
	COLOR = texture(TEXTURE,
	((SCREEN_UV - 0.5) / SCREEN_PIXEL_SIZE + offset * z / bgZoom) * TEXTURE_PIXEL_SIZE / z + 0.5);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
