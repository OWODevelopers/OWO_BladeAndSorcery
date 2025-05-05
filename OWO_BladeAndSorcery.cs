using HarmonyLib;
using OWO_BaldeAndSorcery;
using System;
using ThunderRoad;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;
using static ThunderRoad.RevealMaskTester;


namespace OWO_BladeAndSorcery
{
    public class OWO_BladeAndSorcery : ThunderScript
    {
        public static OWOSkin owoSkin;
        private Harmony harmony;

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData); //Load on PlayGame button pressed
            owoSkin = new OWOSkin();

            this.harmony = new Harmony("owo.patch.bladeandsorcery");
            this.harmony.PatchAll();
        }


        #region Game Control

        [HarmonyPatch(typeof(Player), "Start")]
        public class OnStart
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.playing = true;
            }
        }

        [HarmonyPatch(typeof(UIMenu), "MenuOpened")]
        public class OnMenuOpened
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
               //Se llama al cambiar de opcion en el menu del juego
                //owoSkin.LOG("MenuOpened");
            }
        }

        [HarmonyPatch(typeof(UIMenu), "MenuClosed")]
        public class OnMenuClosed
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                //owoSkin.LOG("MenuClosed");
            }
        }

        #endregion

        #region Player        
        [HarmonyPatch(typeof(Player), "OnEnterWater")]
        public class OnEnterWater
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.CanFeel()) return;

                owoSkin.StartSwimming();
            }
        }

        [HarmonyPatch(typeof(Player), "OnExitWater")]
        public class OnExitWater
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.CanFeel()) return;

                owoSkin.StopSwimming();
            }
        }

        //[HarmonyPatch(typeof(PlayerHand), "Start")]
        //public class OnPHStart
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(PlayerHand __instance)
        //    {
        //        owoSkin.LOG($"Start hand: {__instance.controlHand.side}");
        //    }
        //}

        //Funciona perfecto para parar la telequinesis, pero se lllama al subir escaleras por ejemplo
        //[HarmonyPatch(typeof(PlayerHand), "OnGrabEvent")]
        //public class OnGrabEvent
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(Side side)
        //    {
        //        owoSkin.LOG($"OnGrabEvent hand: {side}", "EVENT");
        //    }
        //}

        [HarmonyPatch(typeof(PlayerTeleporter), "Teleport", new Type[] { typeof(Transform)})]
        public class OnTeleport
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.CanFeel()) return;

                owoSkin.Feel("Teleport");              
            }
        }

        [HarmonyPatch(typeof(Handle), "OnTelekinesisGrab")]
        public class OnTelekinesisGrab
        {
            [HarmonyPostfix]
            public static void Postfix(SpellTelekinesis spellTelekinesis)
            {
                owoSkin.LOG($"OnTelekinesisGrab: {spellTelekinesis.spellCaster.ragdollHand.playerHand}", "EVENT");

                if (!owoSkin.CanFeel()) return;

                owoSkin.StartTelekinesis(true);

                //owoSkin.StartTelekinesis(spellTelekinesis.spellCaster.ragdollHand.playerHand);                
                //(owoSkin.LOG($"OnTelekinesisGrab", "EVENT");
            }
        }

        [HarmonyPatch(typeof(Handle), "OnTelekinesisRelease")]
        public class OnTelekinesisRelease
        {
            [HarmonyPostfix]
            public static void Postfix(SpellTelekinesis spellTelekinesis)
            {
                owoSkin.LOG($"OnTelekinesisGrab: {spellTelekinesis.spellCaster.ragdollHand.playerHand}", "EVENT");

                if (!owoSkin.CanFeel()) return;

                owoSkin.StopTelekinesis(true);
            }
        }

        #endregion

        [HarmonyPatch(typeof(PlayerWaist), "Awake")]
        public class OnPlayerWaist
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"PlayerWaist AWAKE", "EVENT");
            }
        }

        [HarmonyPatch(typeof(UIInventory), "Awake")]
        public class OnOpenInventoryAwake
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"Inventory Awake", "EVENT");
            }
        }

        [HarmonyPatch(typeof(UIInventory), "InvokeOnOpen")]
        public class OnOpenInventory
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"OnOpenInventory", "EVENT");
            }
        }

        //funciona pero lo mismo no nos sirve cuando nos dan
        [HarmonyPatch(typeof(EventManager), "InvokeCreatureAttack")]
        public class OnInvokeCreatureAttack
        {
            [HarmonyPostfix]
            public static void Postfix(Creature attacker, Creature targetCreature, Transform targetTransform, BrainModuleAttack.AttackType type, BrainModuleAttack.AttackStage stage)
            {
                owoSkin.LOG($"InvokeCreatureAttack player:{targetCreature.isPlayer}", "EVENT");
            }
        }

        [HarmonyPatch(typeof(Creature), "Awake")]
        public class OnCreatueAwake
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"Creature Awake", "EVENT");
            }
        }
    }
}
