using UnityEngine;

namespace aeric.rewind_plugin {
    public class RewindAttribute : PropertyAttribute {
        public bool Lerp { get; set; } = true;
    }
}