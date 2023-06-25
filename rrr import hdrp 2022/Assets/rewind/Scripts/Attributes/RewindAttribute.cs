using UnityEngine;

namespace aeric.rewind_plugin {
    /// <summary>
    /// Attribute that is added to members of a class so their state will be stored/restored by the
    /// rewind plugin system. The class the members are in must be derived from RewindCustomMonoBehaviourAttributes
    /// </summary>
    public class RewindAttribute : PropertyAttribute {
        public bool Lerp { get; set; } = true;
    }
}