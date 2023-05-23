using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rewind_plugin
{
    public class RewindScene
    {
        public List<IRewindHandler> RewindHandlers { get; } = new();

        public void addRewindObject(IRewindHandler rewindHandler)
        {
            RewindHandlers.Add(rewindHandler);
        }

        public void addAllChildren(GameObject parentObj)
        {
            foreach (var c in parentObj.GetComponentsInChildren<IRewindHandler>())
            {
                addRewindObject(c);
            }
        }
    }
}