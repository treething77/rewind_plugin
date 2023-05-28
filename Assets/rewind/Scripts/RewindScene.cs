using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ccl.rewind_plugin
{
    public class RewindScene
    {
        public List<IRewindHandler> RewindHandlers { get; } = new();

        public void addRewindObject(IRewindHandler rewindHandler)
        {
            RewindHandlers.Add(rewindHandler);
        }

        /// <summary>
        /// Helpful method to add all the childs of a parent object as rewind handlers
        /// </summary>
        /// <param name="parentObj"></param>
        public void addAllChildren(GameObject parentObj)
        {
            foreach (var c in parentObj.GetComponentsInChildren<IRewindHandler>())
            {
                addRewindObject(c);
            }
        }
    }
}