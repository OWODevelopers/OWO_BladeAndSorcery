using HarmonyLib;
using System;
using ThunderRoad;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;


namespace OWO_BladeAndSorcery
{
    public class OWO_BladeAndSorcery : ThunderScript
    {
        public static OWOSkin owoSkin;
        private Harmony harmony;

        public static bool canJump;

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
                owoSkin.LOG($"OnTelekinesisGrab: {spellTelekinesis.spellCaster.ragdollHand.playerHand.name == "HandR"}", "EVENT");

                if (!owoSkin.CanFeel()) return;

                owoSkin.StartTelekinesis(spellTelekinesis.spellCaster.ragdollHand.playerHand.name == "HandR");
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

                owoSkin.StopTelekinesis(spellTelekinesis.spellCaster.ragdollHand.playerHand.name == "HandR");
            }
        }

        [HarmonyPatch(typeof(Locomotion), "Jump")]
        public class OnJump
        {
            [HarmonyPostfix]
            public static void Postfix(bool active)
            {
                if (!canJump || !active) return;
                owoSkin.Feel("Jump");
                canJump = false;
            }
        }

        [HarmonyPatch(typeof(Locomotion), "OnGround")]
        public class OnOnGround
        {
            [HarmonyPostfix]
            public static void Postfix(Vector3 velocity)
            {
                canJump = true;
                if (velocity.magnitude >= 0.5f)
                    //intensity per velocity(?
                    owoSkin.Feel("Landing");
            }
        }

        #endregion

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

        #region Prueba

        [HarmonyPatch(typeof(Locomotion), "OnGround")]
        public class OnOnGround
        {
            [HarmonyPostfix]
            public static void Postfix(Vector3 velocity)
            {
                canJump = true;
                if(velocity.magnitude >= 0.5f)
                    //intensity per velocity(?
                owoSkin.Feel("Landing");
            }
        }

        
        [HarmonyPatch(typeof(Locomotion), "Jump")]
        public class OnJump
        {
            [HarmonyPostfix]
            public static void Postfix(bool active)
            {
                if (!canJump || !active) return;
                owoSkin.Feel("Jump");
                canJump = false;
            }
        }

        [HarmonyPatch(typeof(BowString), "ManagedUpdate")]
        public class OnManagedUpdateBow
        {
            [HarmonyPostfix]
            public static void Postfix(BowString __instance)
            {
                owoSkin.LOG($"BowString ManagedUpdate Hand: {__instance.stringHandle.handlers[0].playerHand.side} - String Pull: {__instance.currentPullRatio}", "EVENT");
                __instance.IsInvoking();
            }
        }

        [HarmonyPatch(typeof(Player), "ManagedUpdate")]
        public class OnPlayerUpdate
        {
            public static bool leftHandClimbing = false;
            public static bool rightHandClimbing = false;

            [HarmonyPostfix]
            public static void Postfix(Player __instance)
            {
                Item heldItem = __instance.creature.equipment.GetHeldItem(Side.Left);
                Item heldItem2 = __instance.creature.equipment.GetHeldItem(Side.Right);
                if (Player.local.handLeft.ragdollHand.climb.isGripping || (__instance.creature.ragdoll.ik != null && __instance.creature.ragdoll.ik.handLeftEnabled && __instance.creature.ragdoll.ik.handLeftTarget != null && heldItem == null && __instance.creature.equipment.GetHeldHandle(Side.Left) != null && !__instance.creature.equipment.GetHeldHandle(Side.Left).customRigidBody.isKinematic && Math.Abs(__instance.creature.ragdoll.ik.GetHandPositionWeight(Side.Left) - 1f) < 0.0001f))
                {
                    if (!leftHandClimbing)
                    {
                        leftHandClimbing = true;
                        owoSkin.LOG($"PRUEBA ESCALADA IZQUIERDA", "EVENT");
                    }
                }
                else
                {
                    leftHandClimbing = false;
                }

                if (Player.local.handRight.ragdollHand.climb.isGripping || (__instance.creature.ragdoll.ik != null && __instance.creature.ragdoll.ik.handRightEnabled && __instance.creature.ragdoll.ik.handRightTarget != null && heldItem2 == null && __instance.creature.equipment.GetHeldHandle(Side.Right) != null && !__instance.creature.equipment.GetHeldHandle(Side.Right).customRigidBody.isKinematic && Math.Abs(__instance.creature.ragdoll.ik.GetHandPositionWeight(Side.Right) - 1f) < 0.0001f))
                {
                    if (!rightHandClimbing)
                    {
                        rightHandClimbing = true;
                        owoSkin.LOG($"PRUEBA ESCALADA DERECHA", "EVENT");

                    }
                }
                else
                {
                    rightHandClimbing = false;
                }
            }
        }

        #endregion




    }
}
