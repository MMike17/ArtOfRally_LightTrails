using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace LightTrails
{
    public class Main
    {
        public static bool enabled { get; private set; }

        /// <summary>This is provided by UnityModManager to log messages to the console</summary>
        public static ModEntry.ModLogger Logger;
        /// <summary>Main access to this mod's settings</summary>
        public static Settings settings;
        /// <summary>This will be called when the mod is toggles on/off</summary>
        public static event Action<bool> OnToggle;

        public static GameObject trailPrefab;

        // Called by the mod manager
        static bool Load(ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = ModSettings.Load<Settings>(modEntry);

            // Harmony patching
            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll();

            // hook in mod manager event
            modEntry.OnToggle = OnToggleEvent;
            modEntry.OnGUI = settings.Draw;
            modEntry.OnSaveGUI = settings.Save;

            Try("Loading asset bundle", () =>
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(modEntry.Path, "light_trails"));

                if (bundle != null)
                    trailPrefab = bundle.LoadAsset<GameObject>("LightTrail");
                else
                    Error("Couldn't load asset bundle \"light_trails\"");

                if (bundle != null && !settings.disableInfoLogs)
                    Log("Loaded bundle \"light_trails\"");
            });

            return true;
        }

        static bool OnToggleEvent(ModEntry modEntry, bool state)
        {
            enabled = state;
            OnToggle?.Invoke(state);
            return true;
        }

        /// <summary>Logs a message to the console</summary>
        public static void Log(string message)
        {
            if (!settings.disableInfoLogs)
                Logger.Log(message);
        }

        /// <summary>Logs an error message to the console</summary>
        public static void Error(string message) => Logger.Error(message);

        /// <summary>Use this to log possible execution errors to the console</summary>
        public static void Try(string message, Action callback)
        {
            try
            {
                callback?.Invoke();
            }
            catch (Exception e)
            {
                Error(message + "\n" + e.ToString());
            }
        }

        /// <summary>BindingFlags.NonPrivate is implicit / source can be null</summary>
        public static T GetField<T, U>(U source, string fieldName, BindingFlags flags)
        {
            FieldInfo info = typeof(U).GetField(fieldName, flags | BindingFlags.NonPublic);

            if (info == null)
            {
                Error("Couldn't find field info for field \"" + fieldName + "\" in type \"" + source.GetType() + "\"");
                return default(T);
            }

            return (T)info.GetValue(source);
        }

        /// <summary>BindingFlags.NonPrivate is implicit / source can be null</summary>
        public static void SetField<T>(T source, string fieldName, BindingFlags flags, object value)
        {
            FieldInfo info = typeof(T).GetField(fieldName, flags | BindingFlags.NonPublic);

            if (info == null)
            {
                Error("Couldn't find field info for field \"" + fieldName + "\" in type \"" + source.GetType() + "\"");
                return;
            }

            info.SetValue(source, value);
        }

        /// <summary>BindingFlags.NonPrivate is implicit / source can be null</summary>
        public static void InvokeMethod<T>(T source, string methodName, BindingFlags flags, object[] args)
        {
            MethodInfo info = typeof(T).GetMethod(methodName, flags | BindingFlags.NonPublic);

            if (info == null)
            {
                Error("Couldn't find method info for method \"" + methodName + "\" in type \"" + source.GetType() + "\"");
                return;
            }

            info.Invoke(source, args);
        }
    }
}
