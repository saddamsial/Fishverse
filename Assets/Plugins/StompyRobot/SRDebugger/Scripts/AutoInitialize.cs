﻿using SRDebugger.Services;
using SRF.Service;

namespace SRDebugger
{
    using UnityEngine;

    public static class AutoInitialize
    {
        /// <summary>
        /// Initialize the console service before the scene has loaded to catch more of the initialization log.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnLoadBeforeScene()
        {
            // Populate service manager with types from SRDebugger assembly (asmdef)
            SRServiceManager.RegisterAssembly<IDebugService>();

            if (Settings.Instance.IsEnabled)
            {
                // Initialize console if it hasn't already initialized.
                SRServiceManager.GetService<IConsoleService>();
            }
        }

        /// <summary>
        /// Initialize SRDebugger after the scene has loaded.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnLoad()
        {
            if (Settings.Instance.IsEnabled)
            {
#if !PRODUCTION
                SRDebug.Init();
#endif
            }
        }
    }
}