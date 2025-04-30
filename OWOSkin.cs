using OWOGame;
using System;
using System.Collections.Generic;
using System.IO;

namespace OWO_BaldeAndSorcery
{
    public class OWOSkin
    {
        public Dictionary<String, Sensation> SensationsMap = new Dictionary<String, Sensation>();

        public OWOSkin()
        {
            //RegisterAllSensationsFiles();
            //InitializeOWO();
        }

        public void LOG(String msg, bool warning = false)
        {
            string pretext = warning ? "[OWO-WARNING]: " : "[OWO]: ";
            try
            {
                using (StreamWriter w = File.AppendText("BladeAndSorcery_Data\\StreamingAssets\\Mods\\OWO\\Logs\\owolog_" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                {
                    w.WriteLine($"{pretext}{msg}");
                }
            }
            catch (Exception ex){}        
        }

        #region Skin Configuration

        //private void RegisterAllSensationsFiles()
        //{
        //    string configPath = Directory.GetCurrentDirectory() + "\\BepinEx\\Plugins\\OWO";
        //    DirectoryInfo d = new DirectoryInfo(configPath);
        //    FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
        //    for (int i = 0; i < Files.Length; i++)
        //    {
        //        string filename = Files[i].Name;
        //        string fullName = Files[i].FullName;
        //        string prefix = Path.GetFileNameWithoutExtension(filename);
        //        if (filename == "." || filename == "..")
        //            continue;
        //        string tactFileStr = File.ReadAllText(fullName);
        //        try
        //        {
        //            Sensation test = Sensation.Parse(tactFileStr);
        //            FeedbackMap.Add(prefix, test);
        //        }
        //        catch (Exception e) { LOG(e.Message); }

        //    }
        //}

        //private async void InitializeOWO()
        //{
        //    LOG("Initializing OWO skin");

        //    var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("14534911");

        //    OWO.Configure(gameAuth);
        //    string[] myIPs = GetIPsFromFile("OWO_Manual_IP.txt");
        //    if (myIPs.Length == 0) await OWO.AutoConnect();
        //    else
        //    {
        //        await OWO.Connect(myIPs);
        //    }

        //    if (OWO.ConnectionState == OWOGame.ConnectionState.Connected)
        //    {
        //        suitEnabled = true;
        //        LOG("OWO suit connected.");
        //        Feel("Heart Beat");
        //    }
        //    if (!suitEnabled) LOG("OWO is not enabled?!?!");
        //}

        //public BakedSensation[] AllBakedSensations()
        //{
        //    var result = new List<BakedSensation>();

        //    foreach (var sensation in FeedbackMap.Values)
        //    {
        //        if (sensation is BakedSensation baked)
        //        {
        //            LOG("Registered baked sensation: " + baked.name);
        //            result.Add(baked);
        //        }
        //        else
        //        {
        //            LOG("Sensation not baked? " + sensation);
        //            continue;
        //        }
        //    }
        //    return result.ToArray();
        //}

        //public string[] GetIPsFromFile(string filename)
        //{
        //    List<string> ips = new List<string>();
        //    string filePath = Directory.GetCurrentDirectory() + "\\BepinEx\\Plugins\\OWO" + filename;
        //    if (File.Exists(filePath))
        //    {
        //        LOG("Manual IP file found: " + filePath);
        //        var lines = File.ReadLines(filePath);
        //        foreach (var line in lines)
        //        {
        //            if (IPAddress.TryParse(line, out _)) ips.Add(line);
        //            else LOG("IP not valid? ---" + line + "---");
        //        }
        //    }
        //    return ips.ToArray();
        //}

        //~OWOSkin()
        //{
        //    LOG("Destructor called");
        //    DisconnectOWO();
        //}

        //public void DisconnectOWO()
        //{
        //    LOG("Disconnecting OWO skin.");
        //    OWO.Disconnect();
        //}
        #endregion
    }
}
