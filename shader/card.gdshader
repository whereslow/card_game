shader_type canvas_item;

void fragment() {
    vec4 texColor = texture(TEXTURE, UV); // 采样纹理颜色

    // 让透明部分显示为白色，但仍然保留原始透明度
    texColor.rgb = mix(vec3(1.0), texColor.rgb, texColor.a);

    COLOR = texColor;
}
