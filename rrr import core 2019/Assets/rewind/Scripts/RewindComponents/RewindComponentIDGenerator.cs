using UnityEngine;
using Random = System.Random;

namespace aeric.rewind_plugin {
    public static class RewindComponentIDGenerator {
        private static Random _idRandom;

        public static uint generateID(IRewindHandler rewindHandler) {
            if (_idRandom == null) _idRandom = new Random();

            //Pack a random value and the handler type into the id
            var id = (uint)_idRandom.Next(0, 2 << 24) << 8;
            id |= rewindHandler.HandlerTypeID;
            return id;
        }
    }
}