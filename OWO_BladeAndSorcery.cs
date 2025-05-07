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

        [HarmonyPatch(typeof(PlayerTeleporter), "Teleport", new System.Type[] { typeof(Transform) })]
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
                if (!owoSkin.CanFeel() || !canJump || !active) return;
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
                if (!owoSkin.CanFeel()) return;

                canJump = true;
                if (velocity.magnitude >= 0.9f)
                    //intensity per velocity(?
                    owoSkin.Feel("Landing");
            }
        }

        [HarmonyPatch(typeof(BowString), "ManagedUpdate")]
        public class OnManagedUpdateBow
        {
            [HarmonyPostfix]
            public static void Postfix(BowString __instance)
            {
                //owoSkin.LOG($"BowString ManagedUpdate Hand: {__instance.stringHandle.handlers[0].playerHand.side} - String Pull: {__instance.currentPullRatio}", "EVENT");
                if (!owoSkin.CanFeel()) return;

                owoSkin.bowRightArm = __instance.stringHandle.handlers[0].playerHand.side.ToString() == "right";

                if (__instance.currentPullRatio > 0.3f)
                {
                    owoSkin.stringBowIntensity = Mathf.FloorToInt(Mathf.Clamp(__instance.currentPullRatio * 100.0f, 40, 100));
                    owoSkin.StartStringBow();
                }
                else if (owoSkin.stringBowIsActive)
                {
                    owoSkin.StopStringBow();
                }
            }
        }

        [HarmonyPatch(typeof(PlayerHand), "OnUnGrabEvent")]
        public class OnUnGrabEvent
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.CanFeel()) return;

                if (owoSkin.stringBowIsActive)
                    owoSkin.StopStringBow();
            }
        }

        #endregion

        [HarmonyPatch(typeof(UIInventory), "InvokeOnOpen")]
        public class OnOpenInventory
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"OnOpenInventory", "EVENT");
            }
        }

        #region Prueba

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

                bool isGripping = Player.local.handLeft.ragdollHand.climb != null && Player.local.handLeft.ragdollHand.climb.isGripping;
                bool isGripping2 = Player.local.handRight.ragdollHand.climb != null && Player.local.handRight.ragdollHand.climb.isGripping;

                bool isLadder = Player.local.handLeft.ragdollHand.grabbedHandle != null && Player.local.handLeft.ragdollHand.grabbedHandle.data.id.ToLowerInvariant().Contains("ladder");
                bool isLadder2 = Player.local.handRight.ragdollHand.grabbedHandle != null && Player.local.handRight.ragdollHand.grabbedHandle.data.id.ToLowerInvariant().Contains("ladder");


                if (isGripping || isLadder || (__instance.creature.ragdoll.ik != null && __instance.creature.ragdoll.ik.handLeftEnabled && __instance.creature.ragdoll.ik.handLeftTarget != null && heldItem == null && __instance.creature.equipment.GetHeldHandle(Side.Left) != null && !__instance.creature.equipment.GetHeldHandle(Side.Left).customRigidBody.isKinematic && Math.Abs(__instance.creature.ragdoll.ik.GetHandPositionWeight(Side.Left) - 1f) < 0.0001f))
                {
                    if (!leftHandClimbing)
                    {
                        leftHandClimbing = true;
                        owoSkin.LOG($"ESCALADA IZQUIERDA", "EVENT");
                    }
                }
                else if (leftHandClimbing)
                {
                    leftHandClimbing = false;
                    owoSkin.LOG($"PARAR ESCALADA IZQUIERDA", "EVENT");
                }


                if (isGripping2 || isLadder2 || (__instance.creature.ragdoll.ik != null && __instance.creature.ragdoll.ik.handRightEnabled && __instance.creature.ragdoll.ik.handRightTarget != null && heldItem2 == null && __instance.creature.equipment.GetHeldHandle(Side.Right) != null && !__instance.creature.equipment.GetHeldHandle(Side.Right).customRigidBody.isKinematic && Math.Abs(__instance.creature.ragdoll.ik.GetHandPositionWeight(Side.Right) - 1f) < 0.0001f))
                {
                    if (!rightHandClimbing)
                    {
                        rightHandClimbing = true;
                        owoSkin.LOG($"ESCALADA DERECHA", "EVENT");

                    }
                }
                else if (rightHandClimbing)
                {
                    rightHandClimbing = false;
                    owoSkin.LOG($"PARAR ESCALADA DERECHA", "EVENT");
                }

            }
        }


        [HarmonyPatch(typeof(ItemModuleEdible), "OnMouthTouch")]
        public class OnOnMouthTouch
        {
            [HarmonyPostfix]
            public static void Postfix(Item item, CreatureMouthRelay mouthRelay)
            {
                if (!mouthRelay.creature.player) return;
                owoSkin.Feel($"Eating");
            }
        }
        [HarmonyPatch(typeof(Creature), "Damage", new System.Type[] { typeof(CollisionInstance) })]

        public class OnDamage
        {
            [HarmonyPrefix]
            public static void Prefix(Creature __instance, out float __state)
            {
                __state = __instance.currentHealth;
            }

            [HarmonyPostfix]
            public static void Postfix(Creature __instance, float __state, CollisionInstance collisionInstance)
            {
                if (!(bool)__instance.player) return;
                float damage = __state - __instance.currentHealth;
                owoSkin.LOG($"Creature Damage - damage: {damage}", "EVENT");
            }
        }

        [HarmonyPatch(typeof(Wearable), "EquipItem")]
        public class OnEquipItem
        {
            [HarmonyPostfix]
            public static void Postfix(Wearable __instance)
            {
                RagdollPart.Type type = __instance.Part.type;
                switch (type)
                {
                    case RagdollPart.Type.Torso:
                        owoSkin.Feel($"Equip Chest");
                        break;
                    case RagdollPart.Type.RightArm:
                        owoSkin.Feel($"Equip Gauntlet R");
                        break;
                    case RagdollPart.Type.LeftArm:
                        owoSkin.Feel($"Equip Gauntlet L");
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(Wearable), "UnEquip")]
        public class OnUnEquip
        {
            [HarmonyPostfix]
            public static void Postfix(Wearable __instance)
            {
                RagdollPart.Type type = __instance.Part.type;
                switch (type)
                {
                    case RagdollPart.Type.Torso:
                        owoSkin.Feel($"Equip Chest");
                        break;
                    case RagdollPart.Type.RightArm:
                        owoSkin.Feel($"Equip Gauntlet R");
                        break;
                    case RagdollPart.Type.LeftArm:
                        owoSkin.Feel($"Equip Gauntlet L");
                        break;
                }
            }
        }


        [HarmonyPatch(typeof(Equipment), "ManageHolsterHandlers")]
        public class OnManageHolsterHandlers
        {
            [HarmonyPostfix]
            public static void Postfix(Equipment __instance, bool add)
            {
                foreach (Holder holder in __instance.creature.holders)
                {
                    if (!(holder == null) && (!(holder.name != "HandleShoulderR") || !(holder.name != "HandleShoulderL")))
                    {
                        if (!add)
                        {
                            owoSkin.Feel((holder.name == "HandleShoulderR") ? "Unholster Right Shoulder" : "Unholster Left Shoulder");
                        }
                        else
                        {
                            owoSkin.Feel((holder.name == "HandleShoulderR") ? "Holster Right Shoulder" : "Holster Left Shoulder");
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(CollisionHandler), "OnCollisionEnter")]
        public class OnOnCollisionEnter
        {

            [HarmonyPostfix]
            public static void Postfix(CollisionHandler __instance, Collision collision)
            {

                ContactPoint contact = collision.GetContact(0);
                Vector3 point = contact.point;
                Collider otherCollider = contact.otherCollider;
                ColliderGroup componentInParent2 = otherCollider.GetComponentInParent<ColliderGroup>();

                Vector3 result = __instance.CalculateLastPointVelocity(point);
                Vector3 vector = componentInParent2?.collisionHandler?.CalculateLastPointVelocity(point) ?? Vector3.zero;
                result.x -= vector.x;
                result.y -= vector.y;
                result.z -= vector.z;

                Vector3 velocity = (__instance.checkMinVelocity ? result : Vector3.zero);

                owoSkin.LOG($"VELOCITY COLLISION MELEE {velocity.magnitude}", "EVENT");

            }
        }
    }

    #endregion

}
