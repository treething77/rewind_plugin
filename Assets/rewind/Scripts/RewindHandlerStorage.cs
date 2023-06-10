using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace aeric.rewind_plugin
{
    public class RewindHandlerStorage
    {
        readonly int handlerStorageOffset;
        readonly int handlerFrameSizeBytes;

        public RewindHandlerStorage(int _handlerStorageOffset, int _handlerFrameSizeBytes)
        {
            handlerStorageOffset = _handlerStorageOffset;
            handlerFrameSizeBytes = _handlerFrameSizeBytes;
        }

        public int HandlerStorageOffset => handlerStorageOffset;
        public int HandlerFrameSizeBytes => handlerFrameSizeBytes;
    }
}