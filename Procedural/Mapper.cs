using UnityEngine;

namespace Plugins.Procedural
{
    public class Mapper : Floor
    {
        public int seed2;
        public float[] noiseValues;
	
        public void SetNoise() {
            
            Random.InitState(seed2);
            noiseValues = new float[10];
            for (int i = 0; i < noiseValues.Length; i++)
            {
                noiseValues[i] = Random.value;
                Debug.Log(noiseValues[i]);
            }
        }
        
        
        public void NewSeed()
        {
            seed2 = Random.Range(0, 99999);
        }
    }
}