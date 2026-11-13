#ifndef WUXIA_CHARACTER_CEL_LIGHTING_INCLUDED
#define WUXIA_CHARACTER_CEL_LIGHTING_INCLUDED

half3 CharacterCelLighting(half ndotl, half shadow, half3 lightColor,
                           half3 shadeTint, half threshold, half width)
{
    half band = smoothstep(threshold - width * 0.5h, threshold + width * 0.5h, ndotl);
    return lerp(shadeTint, lightColor, band * saturate(shadow));
}

#endif
