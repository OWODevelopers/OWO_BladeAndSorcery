using OWOGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace OWO_BladeAndSorcery
{
    public class OWOSkin
    {
        public bool playing = false;
        public bool stringBowIsActive = false;
        public bool bowRightArm = true;
        public int stringBowIntensity = 40;
        public int climbingLIntensity = 40;
        public int climbingRIntensity = 40;
        public bool spellLIsActive = false;
        public bool spellRIsActive = false;

        private bool suitEnabled = false;
        private string modPath = "BladeAndSorcery_Data\\StreamingAssets\\Mods\\OWO";
        private Dictionary<String, Sensation> sensationsMap = new Dictionary<String, Sensation>();
        private Dictionary<String, Muscle[]> muscleMap = new Dictionary<String, Muscle[]>();

        private bool telekinesisIsActive = false;
        private bool telekinesisLIsActive = false;
        private bool telekinesisRIsActive = false;
        private bool climbIsActive = false;
        private bool climbLIsActive = false;
        private bool climbRIsActive = false;
        private bool spellIsActive = false;
        private bool slowMotionIsActive = false;
        public bool heartBeatIsActive = false;

        public Dictionary<string, Sensation> SensationsMap { get => sensationsMap; set => sensationsMap = value; }

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            DefineAllMuscleGroups();
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
        private void DefineAllMuscleGroups()
        {
            Muscle[] leftGauntlet = { Muscle.Arm_L};
            muscleMap.Add("Left Gauntlet", leftGauntlet);

            Muscle[] rightGauntlet = { Muscle.Arm_R};
            muscleMap.Add("Right Gauntlet", rightGauntlet);

            Muscle[] leftDamage = { Muscle.Arm_L, Muscle.Pectoral_L, Muscle.Abdominal_L, Muscle.Dorsal_L, Muscle.Lumbar_L };
            muscleMap.Add("Left Damage", leftDamage);

            Muscle[] rightDamage = { Muscle.Arm_R, Muscle.Pectoral_R, Muscle.Abdominal_R, Muscle.Dorsal_R, Muscle.Lumbar_R };
            muscleMap.Add("Right Damage", rightDamage);

            Muscle[] frontDamage = Muscle.Front;
            muscleMap.Add("Front Damage", frontDamage);

            Muscle[] backDamage = Muscle.Back;
            muscleMap.Add("Back Damage", backDamage);

            Muscle[] leftBack = { Muscle.Dorsal_L.WithIntensity(100) };
            muscleMap.Add("Left Back", leftBack);

            Muscle[] rightBack = { Muscle.Dorsal_R.WithIntensity(100) };
            muscleMap.Add("Right Back", rightBack);

            Muscle[] leftHip = { Muscle.Lumbar_L.WithIntensity(100), Muscle.Abdominal_L.WithIntensity(100) };
            muscleMap.Add("Left Hip", leftHip);

            Muscle[] rightHip = { Muscle.Lumbar_R.WithIntensity(100), Muscle.Abdominal_R.WithIntensity(100) };
            muscleMap.Add("Right Hip", rightHip);

            Muscle[] leftArm = { Muscle.Arm_L.WithIntensity(100), Muscle.Pectoral_L.WithIntensity(70), Muscle.Dorsal_L.WithIntensity(50) };
            muscleMap.Add("Left Arm", leftArm);

            Muscle[] rightArm = { Muscle.Arm_R.WithIntensity(100), Muscle.Pectoral_R.WithIntensity(70), Muscle.Dorsal_R.WithIntensity(50) };
            muscleMap.Add("Right Arm", rightArm);

            Muscle[] bothArms = leftArm.Concat(rightArm).ToArray();
            muscleMap.Add("Both Arms", bothArms);
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

        public void FeelWithMuscles(String key, String muscleKey = "Right Arm", int Priority = 0, int intensity = 0)
        {
            LOG($"FEEL WITH MUSCLES: {key} - {muscleKey}");

            if (!muscleMap.ContainsKey(muscleKey))
            {
                LOG("MuscleGroup not registered: " + muscleKey, "OWO-SENSATION");
                return;
            }

            if (SensationsMap.ContainsKey(key))
            {
                if (intensity != 0)
                {
                    OWO.Send(SensationsMap[key].WithMuscles(muscleMap[muscleKey].WithIntensity(intensity)).WithPriority(Priority));
                }
                else
                {
                    OWO.Send(SensationsMap[key].WithMuscles(muscleMap[muscleKey]).WithPriority(Priority));
                }
            }

            else LOG("Feedback not registered: " + key, "OWO-SENSATION");
        }

        public void DynamicClimbing()
        {
            Muscle[] leftArmClimb = { Muscle.Arm_L.WithIntensity(climbingLIntensity), Muscle.Pectoral_L.WithIntensity(Mathf.FloorToInt(climbingLIntensity * 0.7f)), Muscle.Dorsal_L.WithIntensity(Mathf.FloorToInt(climbingLIntensity * 0.5f)) };

            Muscle[] rightArmClimb = { Muscle.Arm_R.WithIntensity(climbingRIntensity), Muscle.Pectoral_R.WithIntensity(Mathf.FloorToInt(climbingRIntensity * 0.7f)), Muscle.Dorsal_R.WithIntensity(Mathf.FloorToInt(climbingRIntensity * 0.5f)) };


            Muscle[] armsClimb = leftArmClimb.Concat(rightArmClimb).ToArray();

            if (!muscleMap.ContainsKey("Arms Climbing")) muscleMap.Add("Arms Climbing", armsClimb);
            else muscleMap["Arms Climbing"] = armsClimb;
        }


        public void StopAllHapticFeedback()
        {
            StopStringBow();
            StopTelekinesis(true);
            StopTelekinesis(false);
            StopSlowMotion();
            StopClimb(true);
            StopClimb(false);
            StopSpell(true);
            StopSpell(false);
            StopHeartBeat();

            OWO.Stop();
        }

        public bool CanFeel()
        {
            return suitEnabled && playing;
        }

        #region Loops

        #region Telekinesis

        public void StartTelekinesis(bool isRight)
        {
            if (isRight)
                telekinesisRIsActive = true;

            if (!isRight)
                telekinesisLIsActive = true;


            if (!telekinesisIsActive)
                TelekinesisFuncAsync();

            telekinesisIsActive = true;
        }

        public void StopTelekinesis(bool isRight)
        {
            if (isRight)
            {
                telekinesisRIsActive = false;
            }
            else
            {
                telekinesisLIsActive = false;
            }
        }

        public async Task TelekinesisFuncAsync()
        {
            string musclesToFeel = "";

            while (telekinesisRIsActive || telekinesisLIsActive)
            {
                if (telekinesisRIsActive && telekinesisLIsActive)
                {
                    musclesToFeel = "Both Arms";
                }
                else
                {
                    if (telekinesisRIsActive)
                        musclesToFeel = "Right Arm";

                    if (telekinesisLIsActive)
                        musclesToFeel = "Left Arm";
                }

                FeelWithMuscles("Telekinesis", musclesToFeel);
                await Task.Delay(1000);
            }

            telekinesisIsActive = false;
        }

        #endregion

        #region StringBow
        public void StartStringBow()
        {
            if (stringBowIsActive) return;

            stringBowIsActive = true;
            StringBowFuncAsync();
        }

        public void StopStringBow()
        {
            stringBowIsActive = false;
        }

        public async Task StringBowFuncAsync()
        {
            while (stringBowIsActive)
            {
                FeelWithMuscles("Bow Pull", bowRightArm ? "Right Arm" : "Left Arm", 1, stringBowIntensity);
                await Task.Delay(250);
            }
        }
        #endregion

        #region Climb

        public void StartClimb(bool isRight)
        {
            if (isRight)
                climbRIsActive = true;

            if (!isRight)
                climbLIsActive = true;

            if (!climbIsActive)
                ClimbFuncAsync();

            climbIsActive = true;
        }

        public void StopClimb(bool isRight)
        {
            if (isRight)
            {
                climbRIsActive = false;
            }
            else
            {
                climbLIsActive = false;
            }
            climbIsActive = false;
        }

        public async Task ClimbFuncAsync()
        {
            string musclesToFeel = "Arms Climbing";

            while (climbRIsActive || climbLIsActive)
            {
                DynamicClimbing();
                FeelWithMuscles("Climbing", musclesToFeel, 2);
                await Task.Delay(400);
            }

            climbIsActive = false;
        }

        #endregion        

        #region Spell

        public void StartSpell(bool isRight)
        {
            if (isRight)
                spellRIsActive = true;

            if (!isRight)
                spellLIsActive = true;


            if (!spellIsActive)
                SpellFuncAsync();

            spellIsActive = true;
        }

        public void StopSpell(bool isRight)
        {
            if (isRight)
            {
                spellRIsActive = false;
            }
            else
            {
                spellLIsActive = false;
            }
        }

        public async Task SpellFuncAsync()
        {
            string musclesToFeel = "";

            while (spellRIsActive || spellLIsActive)
            {
                if (spellRIsActive && spellLIsActive)
                {
                    musclesToFeel = "Both Arms";
                }
                else
                {
                    if (spellRIsActive)
                        musclesToFeel = "Right Arm";

                    if (spellLIsActive)
                        musclesToFeel = "Left Arm";
                }

                FeelWithMuscles("Spell", musclesToFeel);
                await Task.Delay(400);
            }

            spellIsActive = false;
        }

        #endregion

        #region Slow Motion

        public void StartSlowMotion()
        {
            if (slowMotionIsActive) return;

            slowMotionIsActive = true;
            SlowMotionFuncAsync();
        }

        public void StopSlowMotion()
        {
            slowMotionIsActive = false;
        }

        public async Task SlowMotionFuncAsync()
        {
            while (slowMotionIsActive)
            {
                Feel("Slow Motion", 1);
                await Task.Delay(1000);
            }
        }

        #endregion

        #region HeartBeat

        public void StartHeartBeat()
        {
            if (heartBeatIsActive) return;

            heartBeatIsActive = true;
            HeartBeatFuncAsync();
        }

        public void StopHeartBeat()
        {
            heartBeatIsActive = false;
        }

        public async Task HeartBeatFuncAsync()
        {
            while (heartBeatIsActive)
            {
                Feel("Heart Beat", 0);
                await Task.Delay(1000);
            }
        }

        #endregion

        #endregion
    }
}
