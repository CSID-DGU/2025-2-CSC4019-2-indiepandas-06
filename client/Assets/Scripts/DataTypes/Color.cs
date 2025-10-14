using UnityEngine;

using Unity.Mathematics;



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Color
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public static class ColorExtensions {

	// Methods

	public static Color ToRGB(this Color hsv) {
		float h = hsv.r;
		float s = hsv.g;
		float v = hsv.b;
		float a = hsv.a;
		float c = v * s;
		float p = h % 360f / 60f;
		float x = c * (1f - math.abs(p % 2f - 1f));
		float m = v - c;
		Color rgb = new(0f, 0f, 0f, a);
		switch ((int)p) {
			case 0: rgb.r = c; rgb.g = x; rgb.b = 0; break;
			case 1: rgb.r = x; rgb.g = c; rgb.b = 0; break;
			case 2: rgb.r = 0; rgb.g = c; rgb.b = x; break;
			case 3: rgb.r = 0; rgb.g = x; rgb.b = c; break;
			case 4: rgb.r = x; rgb.g = 0; rgb.b = c; break;
			case 5: rgb.r = c; rgb.g = 0; rgb.b = x; break;
		}
		rgb.r += m;
		rgb.g += m;
		rgb.b += m;
		return rgb;
	}

	public static Color ToHSV(this Color rgb) {
		float max = math.max(math.max(rgb.r, rgb.g), rgb.b);
		float min = math.min(math.min(rgb.r, rgb.g), rgb.b);
		float delta = max - min;
		float h = 0f;
		float s = 0f;
		float v = max;
		float a = rgb.a;
		if (max != 0f) s = delta / max;
		if (delta != 0f) {
			switch (max) {
				case float n when n == rgb.r: h = (rgb.g - rgb.b) / delta + 0f; break;
				case float n when n == rgb.g: h = (rgb.b - rgb.r) / delta + 2f; break;
				case float n when n == rgb.b: h = (rgb.r - rgb.g) / delta + 4f; break;
			}
			h *= 60f;
			if (h < 0f) h += 360f;
		}
		return new(h, s, v, a);
	}
}
