using BepInEx;
using System.Diagnostics;
using System;
using System.Security.Permissions;
using System.Collections;
using System.Collections.Generic;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RemixAutoRestart;

[BepInPlugin(MOD_ID, "Remix Auto Restarter", "1.1.0")]
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

            //Command line args
            List<string> new_args = new List<string>();
            string[] current_args = Environment.GetCommandLineArgs();
            for (int i = 0; i < current_args.Length; i++)
            {
                //Skip the first elements because that is the process file itself
                if (i == 0)
                    continue;

                //Something (Doorstop?) is adding a logFile arg to the process in the format "-logFile C:\path\to\Rain World\output.log"
                //We need to skip that arg and the following one (the logfile path itself) otherwise the process args just keep growing with more and more -logFile args
                if (current_args[i] == "-logFile")
                {
                    i++;
                    continue;
                }

                new_args.Add(current_args[i]);
            }
            psi.Arguments = String.Join(" ", new_args.ToArray());
            Process.Start(psi);
            UnityEngine.Application.Quit();
        }
        orig(self, sender, message);
    }
}