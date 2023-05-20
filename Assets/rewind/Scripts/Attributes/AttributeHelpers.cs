using System;
using System.Reflection;

namespace rewind_plugin
{
    public class RewindAttributeHelper
    {
        public static FieldInfo[] GetRewindFields(Object attributeTest)
        {
            //get all the fields on this object that have the Rewind attribute
            FieldInfo[] rewindFields = attributeTest.GetType().GetFields(BindingFlags.Public |
                                                                         BindingFlags.NonPublic |
                                                                         BindingFlags.Instance |
                                                                         BindingFlags.FlattenHierarchy);
            return rewindFields;
        }
    }
}