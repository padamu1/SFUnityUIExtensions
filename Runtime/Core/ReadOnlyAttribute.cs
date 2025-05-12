using System;
using UnityEngine;

namespace SFUnityUIExtensions.Runtime.Core
{
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}