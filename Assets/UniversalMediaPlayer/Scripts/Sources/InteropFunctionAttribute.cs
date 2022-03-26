using System;

namespace UMP.Wrappers
{
    [AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false)]
    internal sealed class InteropFunctionAttribute : Attribute
    {
        public string FunctionName { get; private set; }

        public InteropFunctionAttribute(string functionName)
        {
            FunctionName = functionName;
        }
    }
}
