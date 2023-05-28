using System;
using System.Reflection;

namespace ccl.rewind_plugin
{
    public static class RewindAttributeHelper
    {
        public static FieldInfo[] GetRewindFields(Object attributeTest)
        {
            //get all the fields on this object that have the Rewind attribute
            FieldInfo[] fields = attributeTest.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo[] rewindFields = Array.FindAll(fields, fieldInfo => fieldInfo.GetCustomAttributes(typeof(RewindAttribute), false).Length > 0);
            
            return rewindFields;
        }
    }
}