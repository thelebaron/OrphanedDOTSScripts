using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Old
{
    public class PaintBrush : MonoBehaviour
    {
        /// <summary>
        /// Debug stuff
        /// </summary>
        public Renderer EditorRenderer;

        public Texture2D EditorTexture2D;
        public int MinSplash = 5;
        public int MaxSplash = 115;
        public int PixelRange = 50;








        Vector2 stored;
        public int Resolution = 512;
        Texture2D m_whiteMap;
        public float BrushSize;
        public Texture2D BrushTexture;
        public static Dictionary<Collider, RenderTexture> PaintTextures = new Dictionary<Collider, RenderTexture>();

        void Start()
        {
            CreateClearTexture(); // clear white texture to draw on
        }

        void Update()
        {
            //Debug.DrawRay(transform.position, Vector3.down * 20f, Color.magenta);


            RaycastHit hit;
            // uncomment for mouse painting
            //if (Physics.Raycast(transform.position, Vector3.down, out hit))
            //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) /*&& Input.GetKeyDown(KeyCode.Space)*/)
            if (Physics.Raycast(transform.position + Vector3.up, Camera.main.transform.forward,
                out hit) /*&& Input.GetKeyDown(KeyCode.Space)*/)
            {
#if UnityEditor


            //stored = hit.lightmapCoord;
            UnityEditor.StaticEditorFlags flags =
 UnityEditor.GameObjectUtility.GetStaticEditorFlags(hit.transform.gameObject);

            if ((flags & UnityEditor.StaticEditorFlags.LightmapStatic) == 0)
                return;
            
            Debug.DrawRay(transform.position + Vector3.up, (Camera.main.transform.forward * 5f), Color.magenta); // debug ray

            if (stored != hit.lightmapCoord) // stop drawing on the same point
            {
                Paint(hit.point, hit);
                stored = hit.lightmapCoord;
            }

            Collider coll = hit.collider;
            return;
            /*
            //old code for rendertexture
            if (coll != null)
            {
                if (!paintTextures.ContainsKey(coll)) // if there is already paint on the material, add to that
                {
                    Renderer rend = hit.transform.GetComponent<Renderer>();
                    paintTextures.Add(coll, getWhiteRT());
                    rend.material.SetTexture("_PaintMap", paintTextures[coll]);


                }
                if (stored != hit.lightmapCoord) // stop drawing on the same point
                {

                    ///

                    
                   
                    stored = hit.lightmapCoord;
                    Vector2 pixelUV = hit.lightmapCoord;
                    pixelUV.y *= resolution;
                    pixelUV.x *= resolution;

                    ///
                    //editorTexture2D.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.red);
                    //editorTexture2D.Apply();
                    
                    //old//DrawTexture(paintTextures[coll], pixelUV.x, pixelUV.y);
                    
                }
            }*/
#endif
            }
        }

        void DrawTexture(RenderTexture rt, float posX, float posY)
        {
            RenderTexture.active = rt;                        // activate rendertexture for drawtexture;
            GL.PushMatrix();                                  // save matrixes
            GL.LoadPixelMatrix(0, Resolution, Resolution, 0); // setup matrix for correct size

            // draw brushtexture
            Graphics.DrawTexture(
                new Rect(posX - BrushTexture.width / BrushSize, (rt.height - posY) - BrushTexture.height / BrushSize,
                    BrushTexture.width / (BrushSize * 0.5f), BrushTexture.height / (BrushSize * 0.5f)), BrushTexture);
            GL.PopMatrix();
            RenderTexture.active = null; // turn off rendertexture


        }

        RenderTexture getWhiteRT()
        {
            RenderTexture rt = new RenderTexture(Resolution, Resolution, 32);
            Graphics.Blit(m_whiteMap, rt);
            return rt;
        }

        [AddComponentMenu("clear")]
        void CreateClearTexture()
        {
            m_whiteMap = new Texture2D(1, 1);
            m_whiteMap.SetPixel(0, 0, Color.white);
            m_whiteMap.Apply();
        }


        public void Paint(Vector3 location, RaycastHit hit)
        {
            int drops = Random.Range(MinSplash, MaxSplash);
            int n     = -1;



            EditorRenderer  = hit.transform.GetComponent<Renderer>();
            EditorTexture2D = EditorRenderer.material.GetTexture("_PaintMap") as Texture2D;

            if (EditorTexture2D == null)
                return;
            Resolution = EditorTexture2D.height;

            Vector2 pixelUV2 = hit.lightmapCoord;
            pixelUV2.y *= Resolution;
            pixelUV2.x *= Resolution;


            while (n <= drops)
            {
                n++;

                float chancex = UnityEngine.Random.Range((float) 0, (float) 1);
                float chancey = UnityEngine.Random.Range((float) 0, (float) 1);

                int randomX = UnityEngine.Random.Range(-PixelRange, PixelRange);
                int randomY = UnityEngine.Random.Range(-PixelRange, PixelRange);
                //pixelUV2.x += randomX;
                //pixelUV2.y += randomY;
                if (chancex > 0.8)
                    randomX *= randomX * randomX;
                if (chancey > 0.8)
                    randomY *= randomY * randomX;

                EditorTexture2D.SetPixel((int) (pixelUV2.x + randomX), (int) (pixelUV2.y + randomY), Color.red);
            }


            EditorTexture2D.Apply();


            /*
            // Generate multiple decals in once
            while (n <= drops)
            {
                n++;
    
                // Get a random direction (beween -n and n for each vector component)
                var fwd = transform.TransformDirection(Random.onUnitSphere * SplashRange);
    
                mRaysDebug.Add(new Ray(location, fwd));
    
                // Raycast around the position to splash everwhere we can
                if (Physics.Raycast(location, fwd, out hit, SplashRange))
                {
                    // Create a splash if we found a surface
                    var paintSplatter = GameObject.Instantiate(PaintPrefab,hit.point,
                                                               // Rotation from the original sprite to the normal
                                                               // Prefab are currently oriented to z+ so we use the opposite
                                                               Quaternion.FromToRotation(Vector3.back, hit.normal)
                                                               ) as Transform;
    
                    // Random scale
                    var scaler = Random.Range(MinScale, MaxScale);
    
                    paintSplatter.localScale = new Vector3(
                        paintSplatter.localScale.x * scaler,
                        paintSplatter.localScale.y * scaler,
                        paintSplatter.localScale.z
                    );
    
                    // Random rotation effect
                    var rater = Random.Range(0, 359);
                    paintSplatter.transform.RotateAround(hit.point, hit.normal, rater);
    
    
                    // TODO: What do we do here? We kill them after some sec?
                    Destroy(paintSplatter.gameObject, 25);
                }
    
            }*/
        }
    }
}