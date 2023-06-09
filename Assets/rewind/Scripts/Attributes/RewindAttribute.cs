using UnityEngine;

namespace ccl.rewind_plugin
{
    public class RewindAttribute : PropertyAttribute
    {
        public bool Lerp { get; set; } = true;
    }
}