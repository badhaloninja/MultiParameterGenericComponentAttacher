using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MultiParameterGenericComponentAttacher
{
    public class MultiParameterGenericComponentAttacher : NeosMod
    {
        public override string Name => "MultiParameterGenericComponentAttacher";
        public override string Author => "badhaloninja";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/badhaloninja/MultiParameterGenericComponentAttacher";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("me.badhaloninja.MultiParameterGenericComponentAttacher");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(ComponentAttacher), "GetCustomGenericType")]
        private class ComponentAttacher_GetCustomGenericType_Patch
        {
            public static void Postfix(SyncType ____genericType, SyncRef<TextField> ____customGenericType, ref Type __result)
            {
                if (__result != null || ____genericType.Value == null || !____genericType.Value.IsGenericType)
                    return;

                int genericArgumentCount = ____genericType.Value.GetGenericArguments().Length;
                if (genericArgumentCount < 2)
                    return;

                TextField target = ____customGenericType.Target;
                string text = target?.TargetString;
                if (string.IsNullOrWhiteSpace(text)) return;

                List<Type> types = new List<Type>();
                
                foreach(var arg in text.Split('|'))
                {
                    if (string.IsNullOrWhiteSpace(arg)) return;
                    var trimmedArg = arg.Trim();

                    types.Add(TypeHelper.TryResolveAlias(trimmedArg) ?? WorkerManager.GetType(trimmedArg));
                }
                if (types.Contains(null) || types.Count != genericArgumentCount) return;


                try
                {
                    Type constructedType = ____genericType.Value.MakeGenericType(types.ToArray());
                    PropertyInfo isValidProperty = constructedType.GetProperty("IsValidGenericInstance", BindingFlags.Static | BindingFlags.Public);

                    if (isValidProperty != null && !(bool)isValidProperty.GetValue(null)) return;
                    __result = constructedType;
                } catch (ArgumentException) {
                    return;
                }
            }
        }
    }
}