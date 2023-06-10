using System.Collections.Generic;
using UnityEngine;

namespace aeric.rewind_plugin
{
    public class RewindScene
    {
        public List<IRewindHandler> RewindHandlers { get; } = new();

        public void addRewindObject(IRewindHandler rewindHandler)
        {
            RewindHandlers.Add(rewindHandler);
        }

        /// <summary>
        /// Ensure that the ID of this handler doesn't conflict with anything already in the scene.
        /// </summary>
        /// <param name="rewindHandler"></param>
        public void ensureUniqueID(IRewindHandler rewindHandler)
        {
            while (RewindHandlers.Find(x => x.ID == rewindHandler.ID) != null)
            {
                rewindHandler.ID = RewindComponentIDGenerator.generateID(rewindHandler);
            }
        }
        
        public void ensureUniqueIDForAllChildren(GameObject parentObj)
        {
            foreach (var c in parentObj.GetComponentsInChildren<IRewindHandler>())
            {
                ensureUniqueID(c);
            }
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