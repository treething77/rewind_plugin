using UnityEngine;

namespace aeric.rewind_plugin
{
    public static class RewindUtilities
    {
        /// <summary>
        /// Correctly lerp between 2 integers, giving equal weight to each value
        /// </summary>
        public static int LerpInt(int a, int b, float t)
        {
            //While interpolating integers, it is customary to divide the range from 0 to 1 into (b-a+1) buckets,
            //ensuring they are of equal size.
            float fT = a + ((b - a + (1-Mathf.Epsilon)) * t);
                    
            return Mathf.FloorToInt(fT);
        }
        
    }
}