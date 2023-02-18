using BepInEx;
using System.Diagnostics;
using System;
using System.Security.Permissions;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RemixAutoRestart;

[BepInPlugin(MOD_ID, "Remix Auto Restarter", "1.0.0")]
public class RemixAutoRestart : BaseUnityPlugin
{
    public const string MOD_ID = "Gamer025.RemixAutoRestart";
   
    public void OnEnable()
    {
        On.Menu.ModdingMenu.Singal += ModdingMenu_Singal;
    }

    private void ModdingMenu_Singal(On.Menu.ModdingMenu.orig_Singal orig, Menu.ModdingMenu self, Menu.MenuObject sender, string message)
    {
        if (message == "RESTART")
        {
            var process = Process.GetCurrentProcess();
            string fullPath = $"\"{process.MainModule.FileName}\"";
            //string launcherPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..") + Path.DirectorySeparatorChar + "LaunchHelper.exe";

            //Logger.LogInfo($"RW exe: {fullPath}, Launcher exe: {launcherPath}");

            //ProcessStartInfo psi = new ProcessStartInfo()
            //{
            //    FileName = launcherPath,
            //    Arguments = fullPath,
            //    CreateNoWindow = true,
            //    UseShellExecute = false
            //};
            //Process.Start(psi);

            var s_SavedEnv = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
            List<string> itemsToRemove = new List<string>();
            foreach (DictionaryEntry ent in s_SavedEnv)
            {
                if (ent.Key.ToString().StartsWith("DOORSTOP"))
                {
                    itemsToRemove.Add(ent.Key.ToString());
                }
            }

            foreach (var item in itemsToRemove)
                s_SavedEnv.Remove(item);

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.EnvironmentVariables.Clear();
            foreach (DictionaryEntry ent in s_SavedEnv)
            {
                psi.EnvironmentVariables.Add((string)ent.Key, (string)ent.Value);
            }
            psi.UseShellExecute = false;
            psi.FileName = fullPath;
            Process.Start(psi);
            UnityEngine.Application.Quit();
        }
        orig(self, sender, message);
    }
}