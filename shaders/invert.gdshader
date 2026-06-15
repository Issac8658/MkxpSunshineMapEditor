shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

void fragment() {
	COLOR.rgb = (vec4(1.0) - texture(SCREEN_TEXTURE, SCREEN_UV)).rgb;
}