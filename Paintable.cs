using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Old
{
    public static class Paintable
    {
        public static void SetPaintable(Transform t)
        {
            Renderer rend = t.GetComponent<Renderer>();
            Material mat  = t.GetComponent<Renderer>().material;
            mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            mat.SetFloat("_SpecularHighlights", 0f);
            // duplicate the original texture and assign to the material

            Texture2D mainTexInstance = Object.Instantiate(rend.material.mainTexture) as Texture2D;
            Texture2D paintTexInstance =
                Object.Instantiate(rend.material.GetTexture("_PaintMap") as Texture2D) as Texture2D;

            var basetex = new Texture2D(mainTexInstance.width, mainTexInstance.height, TextureFormat.ARGB32, false);
            basetex.filterMode = FilterMode.Point;
            basetex.SetPixels(mainTexInstance.GetPixels());
            basetex.Apply();

            var painttex = new Texture2D(paintTexInstance.width, paintTexInstance.height, TextureFormat.ARGB32, false);
            painttex.filterMode = FilterMode.Point;
            painttex.SetPixels(paintTexInstance.GetPixels());
            painttex.Apply();



            rend.material.mainTexture = basetex;
            rend.material.SetTexture("_PaintMap", painttex);

        }

    }
}