using System;
using LogicAPI.Client;
using System.Collections.Generic;
using LICC;
using System.IO;
using EccsLogicWorldAPI.Client.Hooks;
using LogicWorld;

namespace CheeseUtilMod.Client
{
    public class CheeseUtilClient : ClientMod
    {
        public static List<FileLoadable> fileLoadables = new List<FileLoadable>();
        
        protected override void Initialize()
        {
            WorldHook.worldLoading += () => {
                try
                {
                    RamMenu.init();
                }
                catch(Exception e)
                {
                    Logger.Error("Could not initialize CheeseUtilMod Gui");
                    SceneAndNetworkManager.TriggerErrorScreen(e);
                }
            };
            
            Logger.Info("Cheese Util Mod - Client Loaded");
        }
        
        [Command("loadram", Description = "Loads a file into any RAM components with the L pin active, does not clear out memory after the end of the file")]
        public static void loadram(string file)
        {
            LineWriter lineWriter = LConsole.BeginLine();
            if (File.Exists(file))
            {
                var bs = File.ReadAllBytes(file);
                foreach (var item in fileLoadables) item.Load(bs, lineWriter, false);
            }
            else
            {
                lineWriter.WriteLine($"Attempted to load file {file} that does not exist!");
            }
            lineWriter.End();
        }
    }
}
