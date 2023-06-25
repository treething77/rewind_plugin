using System.Collections.Generic;
using UnityEngine;

namespace aeric.rewind_plugin {
    /// <summary>
    /// Represents a list of IRewindHandlers where each handler in the list has a unique id
    /// </summary>
    public class RewindScene {
        public List<IRewindHandler> RewindHandlers { get; } = new();

        public void addRewindHandler(IRewindHandler rewindHandler) {
            RewindHandlers.Add(rewindHandler);
        }
        
        public void addRewindObject(GameObject rewindObject) {
            foreach (var c in rewindObject.GetComponents<IRewindHandler>()) addRewindHandler(c);
        }
        
        /// <summary>
        ///     Ensure that the ID of this handler doesn't conflict with anything already in the scene.
        /// </summary>
        /// <param name="rewindHandler"></param>
        public void ensureUniqueID(IRewindHandler rewindHandler) {
            while (RewindHandlers.Find(x => x.ID == rewindHandler.ID) != null) rewindHandler.ID = RewindComponentIDGenerator.generateID(rewindHandler);
        }

        public void ensureUniqueIDForAllChildren(GameObject parentObj) {
            foreach (var c in parentObj.GetComponentsInChildren<IRewindHandler>(true)) ensureUniqueID(c);
        }

        /// <summary>
        ///     Helpful method to add all the childs of a parent object as rewind handlers
        /// </summary>
        /// <param name="parentObj"></param>
        public void addAllChildren(GameObject parentObj) {
            foreach (var c in parentObj.GetComponentsInChildren<IRewindHandler>(true)) addRewindHandler(c);
        }

        public IRewindHandler getHandler(uint handlerID) {
            return RewindHandlers.Find(x => x.ID == handlerID);
        }
    }
}