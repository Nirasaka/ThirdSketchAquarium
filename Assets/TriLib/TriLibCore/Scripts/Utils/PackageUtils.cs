#if UNITY_EDITOR
using UnityEditor.PackageManager;
#endif
using UnityEngine;

namespace TriLibCore
{
    public static class PackageUtils
    {
        public static bool HasPackage(string packageName)
        {
#if UNITY_EDITOR
            var request = Client.List(true);
            while (!request.IsCompleted)
            {

            }
            foreach (var item in request.Result)
            {
                if (item.name == packageName)
                {
                    return true;
                }
            }
            return false;
#else
            return true;
#endif
        }
    }
}