using UnityEngine;

namespace Plugins.Procedural
{
    public class Seed : MonoBehaviour
    {
        public string seed;
        public bool useRandomSeed;
        protected System.Random pseudoRandom;

        public virtual void Generate()
        {
            
        }
        
        protected void GetSeed()
        {
            if (!useRandomSeed)
            {
                pseudoRandom = new System.Random(seed.GetHashCode());
            }
            else
            {
                seed = null;
                seed = Time.time.ToString();
                pseudoRandom = new System.Random(seed.GetHashCode());
            }
        }
    }
}