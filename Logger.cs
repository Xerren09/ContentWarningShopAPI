using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ShopAPI
{
    internal static class Logger
    {
        public static void Log(string info)
        {
            Debug.Log($"[ShopAPI]: {info}");
        }
        public static void LogError(string error)
        {
            Debug.LogError($"[ShopAPI]: {error}");
        }

        public static void LogWarning(string warning)
        {
            Debug.LogWarning($"[ShopAPI]: {warning}");
        }
    }
}
