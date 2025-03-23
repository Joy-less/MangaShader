// Manga Shader for Godot (MIT)
// Made by Joy-less: https://github.com/Joy-less/MangaShader
// Inspired by Exuin: https://godotshaders.com/shader/screentone-black-spaced-pixels

shader_type canvas_item;

uniform bool enabled = true;
uniform bool inverted = false;
uniform bool grayscale = false;
uniform float strength = 0.5; // Strength of the overlay effect
uniform float light = 1.0; // Brightness of light pixels
uniform float dark = 0.5; // Brightness of dark pixels
uniform float dot_spread = 17.0; // Distance between dots
uniform float dot_size = 1.0; // Size of each dot
uniform float alpha_threshold = 0.0; // Minimum pixel alpha to show shader

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

bool is_light_pixel(float average, ivec2 pixel_pos) {
	int max_i = int(average * dot_spread);
	
	return (((pixel_pos.x) % 4 == 0 && (pixel_pos.y + 3) % 4 == 0)
		|| (max_i > 1 && (pixel_pos.x + 2) % 4 == 0 && (pixel_pos.y + 1) % 4 == 0)
		|| (max_i > 2 && (pixel_pos.x + 2) % 4 == 0 && (pixel_pos.y + 3) % 4 == 0)
		|| (max_i > 3 && (pixel_pos.x) % 4 == 0 && (pixel_pos.y + 1) % 4 == 0)
		|| (max_i > 4 && (pixel_pos.x + 1) % 4 == 0 && (pixel_pos.y + 2) % 4 == 0)
		|| (max_i > 5 && (pixel_pos.x + 3) % 4 == 0 && (pixel_pos.y) % 4 == 0)
		|| (max_i > 6 && (pixel_pos.x + 3) % 4 == 0 && (pixel_pos.y + 2) % 4 == 0)
		|| (max_i > 7 && (pixel_pos.x + 1) % 4 == 0 && (pixel_pos.y) % 4 == 0)
		|| (max_i > 8 && (pixel_pos.x + 1) % 4 == 0 && (pixel_pos.y + 3) % 4 == 0)
		|| (max_i > 9 && (pixel_pos.x + 3) % 4 == 0 && (pixel_pos.y + 1) % 4 == 0)
		|| (max_i > 10 && (pixel_pos.x + 3) % 4 == 0 && (pixel_pos.y + 3) % 4 == 0)
		|| (max_i > 11 && (pixel_pos.x + 1) % 4 == 0 && (pixel_pos.y + 1) % 4 == 0)
		|| (max_i > 12 && (pixel_pos.x) % 4 == 0 && (pixel_pos.y + 2) % 4 == 0)
		|| (max_i > 13 && (pixel_pos.x + 2) % 4 == 0 && (pixel_pos.y) % 4 == 0)
		|| (max_i > 14 && (pixel_pos.x + 2) % 4 == 0 && (pixel_pos.y + 2) % 4 == 0)
		|| (max_i > 15));
}

void fragment() {
	vec4 color = texture(screen_texture, UV);
	if (color.a >= alpha_threshold) {
		if (enabled) {
			vec2 screen_size = vec2(textureSize(screen_texture, 0));
			vec2 pixel_pos = floor(UV * screen_size / dot_size);
			
			float color_average = (color.r + color.g + color.b) / 3.0;
			
			vec3 manga_color = is_light_pixel(color_average, ivec2(pixel_pos))
				? vec3(light)
				: vec3(dark);
			
			if (inverted) {
				manga_color = vec3(1) - manga_color;
			}
			
			COLOR.rgb = mix(color.rgb, manga_color, strength);
			
			if (grayscale) {
				COLOR.rgb = vec3((COLOR.r + COLOR.g + COLOR.b) / 3.0);
			}
		}
		else {
			COLOR.rgb = color.rgb;
		}
	}
	else {
		COLOR = vec4(color.rgb, 0.0);
	}
}