using System;
using System.Reflection;

namespace aeric.rewind_plugin
{
    public static class RewindAttributeHelper
    {
        //get all the fields on this object that have the Rewind attribute
        public static FieldInfo[] GetRewindFields(Object attributeTest)
        {
            FieldInfo[] fields = attributeTest.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo[] rewindFields = Array.FindAll(fields, fieldInfo => fieldInfo.GetCustomAttributes(typeof(RewindAttribute), false).Length > 0);
            
            return rewindFields;
        }
    }
}