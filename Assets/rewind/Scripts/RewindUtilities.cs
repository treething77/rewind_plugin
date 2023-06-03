using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ccl.rewind_plugin
{
    public static class RewindUtilities
    {
        /// <summary>
        /// Correctly lerp between 2 integers, giving equal weight to each value
        /// </summary>
        public static int LerpInt(int a, int b, float t)
        {
            //When interpolating integers we want to divide the 0-1 space into (b-a+1) buckets of equal size

            float fT = a + ((b - a + (1-Mathf.Epsilon)) * t);
                    
            return Mathf.FloorToInt(fT);
        }
        
    }
}