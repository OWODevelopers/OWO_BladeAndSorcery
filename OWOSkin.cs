using OWOGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace OWO_BladeAndSorcery
{
    public class OWOSkin
    {
        public Dictionary<String, Sensation> SensationsMap = new Dictionary<String, Sensation>();
        public bool playing = false;

        private string modPath = "BladeAndSorcery_Data\\StreamingAssets\\Mods\\OWO";
        private bool suitEnabled = false;

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            InitializeOWO();
        }

        public void LOG(String msg, string logCategory = "OWO")
        {
            try
            {
                using (StreamWriter w = File.AppendText($"{modPath}\\Logs\\owolog_" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                {
                    w.WriteLine($"[{logCategory}]: {msg}");
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[OWO-WARNING]{ex}");
            }        
        }

        #region Skin Configuration

        private void RegisterAllSensationsFiles()
        {
            string configPath = $"{modPath}\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    Sensation test = Sensation.Parse(tactFileStr);
                    SensationsMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.Message); }
            }
        }

        private async void InitializeOWO()
        {
            LOG("Initializing OWO skin");

            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("66587306");

            OWO.Configure(gameAuth);
            string[] myIPs = GetIPsFromFile("OWO_Manual_IP.txt");
            if (myIPs.Length == 0) await OWO.AutoConnect();
            else
            {
                await OWO.Connect(myIPs);
            }

            if (OWO.ConnectionState == OWOGame.ConnectionState.Connected)
            {
                suitEnabled = true;
                LOG("OWO suit connected.");
                Feel("Heart Beat");
            }
            if (!suitEnabled) LOG("OWO is not enabled?!?!");
        }

        public BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in SensationsMap.Values)
            {
                if (sensation is BakedSensation baked)
                {
                    LOG("Registered baked sensation: " + baked.name);
                    result.Add(baked);
                }
                else
                {
                    LOG("Sensation not baked? " + sensation);
                    continue;
                }
            }
            return result.ToArray();
        }

        public string[] GetIPsFromFile(string filename)
        {
            List<string> ips = new List<string>();
            string filePath = Directory.GetCurrentDirectory() + "\\BepinEx\\Plugins\\OWO" + filename;
            if (File.Exists(filePath))
            {
                LOG("Manual IP file found: " + filePath);
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    if (IPAddress.TryParse(line, out _)) ips.Add(line);
                    else LOG("IP not valid? ---" + line + "---");
                }
            }
            return ips.ToArray();
        }

        ~OWOSkin()
        {
            LOG("Destructor called");
            DisconnectOWO();
        }

        public void DisconnectOWO()
        {
            LOG("Disconnecting OWO skin.");
            OWO.Disconnect();
        }
        #endregion

        public void Feel(String key, int Priority = 0, int intensity = 0)
        {
            if (SensationsMap.ContainsKey(key))
            {
                Sensation toSend = SensationsMap[key];

                if (intensity != 0)
                {
                    toSend = toSend.WithMuscles(Muscle.All.WithIntensity(intensity));
                }

                OWO.Send(toSend.WithPriority(Priority));
            }

            else LOG("Feedback not registered: " + key, "OWO-SENSATION");
        }

        public void StopAllHapticFeedback()
        {
            //Call all stop methods from loops

            OWO.Stop();
        }

        public bool CanFeel()
        {
            return suitEnabled && playing;
        }
    }
}
