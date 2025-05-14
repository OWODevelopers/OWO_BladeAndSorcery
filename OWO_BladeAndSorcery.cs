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

        public static bool canJump = true;

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
                owoSkin.LOG("PLAYER START");
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

                owoSkin.Feel("Teleport", 2);
            }
        }

        [HarmonyPatch(typeof(Handle), "OnTelekinesisGrab")]
        public class OnTelekinesisGrab
        {
            [HarmonyPostfix]
            public static void Postfix(SpellTelekinesis spellTelekinesis)
            {
                //owoSkin.LOG($"OnTelekinesisGrab: {spellTelekinesis.spellCaster.ragdollHand.playerHand.name == "HandR"}", "EVENT");

                if (!owoSkin.CanFeel() || !spellTelekinesis.spellCaster.mana.creature.player || !spellTelekinesis.spellCaster.mana.creature.player.isLocal) return;

                owoSkin.StartTelekinesis(spellTelekinesis.spellCaster.ragdollHand.playerHand.name == "HandR");
            }
        }

        [HarmonyPatch(typeof(Handle), "OnTelekinesisRelease")]
        public class OnTelekinesisRelease
        {
            [HarmonyPostfix]
            public static void Postfix(SpellTelekinesis spellTelekinesis)
            {
                //owoSkin.LOG($"OnTelekinesisGrab: {spellTelekinesis.spellCaster.ragdollHand.playerHand.name == "HandR"}", "EVENT");

                if (!owoSkin.CanFeel() || !spellTelekinesis.spellCaster.mana.creature.player || !spellTelekinesis.spellCaster.mana.creature.player.isLocal) return;

                owoSkin.StopTelekinesis(spellTelekinesis.spellCaster.ragdollHand.playerHand.name == "HandR");
            }
        }

        [HarmonyPatch(typeof(Locomotion), "Jump")]
        public class OnJump
        {
            [HarmonyPostfix]
            public static void Postfix(Locomotion __instance, bool active)
            {
                if (!owoSkin.CanFeel() || !canJump || !active || !__instance.player || !__instance.player.isLocal) return;
                owoSkin.Feel("Jump", 2);
                canJump = false;
            }
        }

        [HarmonyPatch(typeof(Locomotion), "OnGround")]
        public class OnGround
        {
            [HarmonyPostfix]
            public static void Postfix(Locomotion __instance, Vector3 velocity)
            {
                if (!owoSkin.CanFeel() || !__instance.player || !__instance.player.isLocal || !(bool)Player.local.creature) return;

                canJump = true;
                if (velocity.magnitude >= Player.local.creature.data.playerFallDamageCurve.GetFirstTime())
                    owoSkin.Feel("Fall Damage", 3);
                else if (velocity.magnitude >= 0.9f)
                    owoSkin.Feel("Landing", 2);
            }
        }

        [HarmonyPatch(typeof(BowString), "ManagedUpdate")]
        public class OnManagedUpdateBow
        {
            [HarmonyPostfix]
            public static void Postfix(BowString __instance)
            {
                //owoSkin.LOG($"BowString ManagedUpdate Hand: {__instance.stringHandle.handlers[0].playerHand.side} - String Pull: {__instance.currentPullRatio}", "EVENT");
                if (!owoSkin.CanFeel() || !__instance.item.IsHeldByPlayer) return;

                if (!owoSkin.stringBowIsActive && !__instance.stringHandle.handlers.IsNullOrEmpty())
                    owoSkin.bowRightArm = __instance.stringHandle.handlers[0].playerHand.side == Side.Right;

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

        [HarmonyPatch(typeof(PlayerFoot), "Kick", new System.Type[] { typeof(Transform), typeof(Vector3) })]
        public class OnKick
        {
            [HarmonyPrefix]
            public static void Prefix(PlayerFoot __instance)
            {
                if (!owoSkin.CanFeel() || !__instance.player.isLocal) return;
                if (!__instance.isAutoKicking && (bool)__instance.ragdollFoot)
                    owoSkin.Feel("Kick", 2);

            }
        }

        [HarmonyPatch(typeof(SpellCastCharge), "Fire")]
        public class OnSpellFire
        {
            [HarmonyPostfix]
            public static void Postfix(SpellCastCharge __instance, bool active)
            {
                if (!owoSkin.CanFeel() || !__instance.spellCaster.mana.creature.player || !__instance.spellCaster.mana.creature.player.isLocal) return;

                Side actualHand = __instance.spellCaster.ragdollHand.side;

                //owoSkin.LOG($"SPELL - ACTIVE: {active} -- order: {__instance.order} - wheelDisplayName: {__instance.wheelDisplayName}, castDescription - {__instance.castDescription}", "EVENT");
                //__instance.wheelDisplayName || __instance.order
                //FIRE - {SpellFire} || 0
                //LIGHTNING - {SpellLightning} || 1
                //GRAVITY - {SpellGravity} || 2
                //CUP - Cup || 3

                if (actualHand == Side.Right)
                {
                    if (active)
                        owoSkin.StartSpell(true);
                    else
                        owoSkin.StopSpell(true);
                }
                else if (active)
                    owoSkin.StartSpell(false);
                else
                    owoSkin.StopSpell(false);
            }
        }

        [HarmonyPatch(typeof(SpellMergeData), "Merge")]
        public class OnSpellMerge
        {
            [HarmonyPostfix]
            public static void Postfix(SpellMergeData __instance, bool active)
            {
                if (!owoSkin.CanFeel() || !__instance.mana.creature.player || !__instance.mana.creature.player.isLocal) return;

                if (active)
                {
                    owoSkin.StartSpell(true);
                    owoSkin.StartSpell(false);
                }
                else
                {
                    owoSkin.StopSpell(true);
                    owoSkin.StopSpell(false);
                }


                //owoSkin.LOG($"SPELL - ACTIVE: {active} -- description: {__instance.description} - id: {__instance.id}", "EVENT");
                //__instance.description
                //FIRE - FireMerge
                //LIGHTNING - LightningMerge
                //GRAVITY - GravityMerge
                //CUP - SpellMergeTest
            }
        }

        [HarmonyPatch(typeof(SpellCastCharge), "Throw")]
        public class OnSpellThrow
        {
            [HarmonyPostfix]
            public static void Postfix(SpellCastCharge __instance)
            {
                if (!owoSkin.CanFeel() || !__instance.spellCaster.mana.creature.player || !__instance.spellCaster.mana.creature.player.isLocal) return;

                owoSkin.FeelWithMuscles("Throw", __instance.spellCaster.ragdollHand.side == Side.Right ? "Right Arm" : "Left Arm", 2);
            }
        }

        [HarmonyPatch(typeof(SpellMergeData), "Throw")]
        public class OnMergeThrow
        {
            [HarmonyPostfix]
            public static void Postfix(SpellMergeData __instance)
            {
                if (!owoSkin.CanFeel() || !__instance.mana.creature.player || !__instance.mana.creature.player.isLocal) return;

                owoSkin.FeelWithMuscles("Throw", "Both Arms", 2);
            }
        }

        [HarmonyPatch(typeof(Player), "ManagedUpdate")]
        public class OnPlayerUpdateClimbing
        {
            public static bool leftHandClimbing = false;
            public static bool rightHandClimbing = false;

            [HarmonyPostfix]
            public static void Postfix(Player __instance)
            {
                if (!owoSkin.CanFeel() || !__instance.isLocal) return;

                try
                {
                    if (owoSkin.heartBeatIsActive && Player.local.creature.currentHealth >= 25) owoSkin.StopHeartBeat();

                    Item heldItem = __instance.creature.equipment.GetHeldItem(Side.Left);
                    Item heldItem2 = __instance.creature.equipment.GetHeldItem(Side.Right);

                    bool isGripping = Player.local.handLeft.ragdollHand.climb != null && Player.local.handLeft.ragdollHand.climb.isGripping;
                    bool isGripping2 = Player.local.handRight.ragdollHand.climb != null && Player.local.handRight.ragdollHand.climb.isGripping;

                    bool isLadder = Player.local.handLeft.ragdollHand.grabbedHandle != null && Player.local.handLeft.ragdollHand.grabbedHandle.data.id.ToLowerInvariant().Contains("ladder");
                    bool isLadder2 = Player.local.handRight.ragdollHand.grabbedHandle != null && Player.local.handRight.ragdollHand.grabbedHandle.data.id.ToLowerInvariant().Contains("ladder");

                    if (isGripping || isLadder || (__instance.creature.ragdoll.ik != null && __instance.creature.ragdoll.ik.handLeftEnabled && __instance.creature.ragdoll.ik.handLeftTarget != null && heldItem == null && __instance.creature.equipment.GetHeldHandle(Side.Left) != null && !__instance.creature.equipment.GetHeldHandle(Side.Left).customRigidBody.isKinematic && Math.Abs(__instance.creature.ragdoll.ik.GetHandPositionWeight(Side.Left) - 1f) < 0.0001f))
                    {
                        float num = Math.Abs(Player.local.creature.handLeft.transform.position.y - Player.local.head.transform.position.y);
                        owoSkin.climbingLIntensity = Mathf.FloorToInt(Mathf.Clamp(num * 200, 40, 100));
                        if (!leftHandClimbing)
                        {
                            leftHandClimbing = true;
                            owoSkin.StartClimb(false);
                            //owoSkin.LOG($"CLIMB LEFT -- {num} -- {owoSkin.climbingLIntensity}", "EVENT");
                        }
                    }
                    else if (leftHandClimbing)
                    {
                        owoSkin.climbingLIntensity = 0;
                        leftHandClimbing = false;
                        owoSkin.StopClimb(false);
                        //owoSkin.LOG($"STOP CLIMB LEFT", "EVENT");
                    }

                    if (isGripping2 || isLadder2 || (__instance.creature.ragdoll.ik != null && __instance.creature.ragdoll.ik.handRightEnabled && __instance.creature.ragdoll.ik.handRightTarget != null && heldItem2 == null && __instance.creature.equipment.GetHeldHandle(Side.Right) != null && !__instance.creature.equipment.GetHeldHandle(Side.Right).customRigidBody.isKinematic && Math.Abs(__instance.creature.ragdoll.ik.GetHandPositionWeight(Side.Right) - 1f) < 0.0001f))
                    {
                        float num2 = Math.Abs(Player.local.creature.handRight.transform.position.y - Player.local.head.transform.position.y);
                        owoSkin.climbingRIntensity = Mathf.FloorToInt(Mathf.Clamp(num2 * 200, 40, 100));
                        if (!rightHandClimbing)
                        {
                            rightHandClimbing = true;
                            owoSkin.StartClimb(true);
                            //owoSkin.LOG($"CLIMB RIGHT -- {num2} -- {owoSkin.climbingRIntensity}", "EVENT");
                        }
                    }
                    else if (rightHandClimbing)
                    {
                        owoSkin.climbingRIntensity = 0;
                        rightHandClimbing = false;
                        owoSkin.StopClimb(true);
                        //owoSkin.LOG($"STOP CLIMB RIGHT", "EVENT");
                    }
                }
                catch (Exception)
                {
                }

            }
        }

        [HarmonyPatch(typeof(ItemModuleEdible), "OnMouthTouch")]
        public class OnMouthTouch
        {
            [HarmonyPostfix]
            public static void Postfix(Item item, CreatureMouthRelay mouthRelay)
            {
                if (!owoSkin.CanFeel() || !mouthRelay.creature.player || !mouthRelay.creature.player.isLocal) return;
                owoSkin.Feel("Eating", 1);
            }
        }

        [HarmonyPatch(typeof(Creature), "Damage", new System.Type[] { typeof(CollisionInstance) })]
        public class OnDamage
        {
            [HarmonyPostfix]
            public static void Postfix(Creature __instance, bool __state, CollisionInstance collisionInstance)
            {
                if (!owoSkin.CanFeel()) return;
                if (!(bool)__instance.player || !__instance.player.isLocal) return;

                float angle;

                Creature creature = collisionInstance.sourceColliderGroup?.collisionHandler.item?.lastHandler?.creature;
                if (!creature)
                {
                    creature = collisionInstance.casterHand?.mana.creature;
                }

                try
                {
                    Vector3 playerForward = Player.local.creature.transform.forward.ToXZ();
                    Vector3 attackerForward = creature.transform.forward.ToXZ();
                    //owoSkin.LOG($"{playerForward}");
                    //owoSkin.LOG($"{attackerForward}");

                    angle = Vector3.SignedAngle(playerForward, -attackerForward, Vector3.up);
                    angle += 180f;

                    switch (angle)
                    {
                        case float a when (a > 135 && a <= 225):
                            owoSkin.FeelWithMuscles("Damage", "Front Damage", 3); //Front
                            break;
                        case float a when (a > 45 && a <= 135):
                            owoSkin.FeelWithMuscles("Damage", "Right Damage", 3); //Right
                            break;
                        case float a when ((a >= 0 && a <= 45) || (a > 315 && a <= 360)):
                            owoSkin.FeelWithMuscles("Damage", "Back Damage", 3); //Back
                            break;
                        case float a when (a > 225 && a <= 315):
                            owoSkin.FeelWithMuscles("Damage", "Left Damage", 3); //Left
                            break;
                        default:
                            owoSkin.FeelWithMuscles("Damage", "Front Damage", 3); //Default
                            break;
                    }
                }
                catch (Exception)
                {
                    owoSkin.FeelWithMuscles("Damage", "Front Damage", 3);
                }

                //owoSkin.LOG($"HEALTH - {__instance.currentHealth}");
                if (__instance.currentHealth <= 25) owoSkin.StartHeartBeat();
            }
        }

        [HarmonyPatch(typeof(Wearable), "EquipItem")]
        public class OnEquipItem
        {
            [HarmonyPostfix]
            public static void Postfix(Wearable __instance)
            {
                if (!owoSkin.CanFeel() || !__instance.Creature.player || !__instance.Creature.player.isLocal) return;
                RagdollPart.Type type = __instance.Part.type;
                switch (type)
                {
                    case RagdollPart.Type.Torso:
                        owoSkin.Feel($"Equip Chest", 2);
                        break;
                    case RagdollPart.Type.RightArm:
                        owoSkin.FeelWithMuscles($"Equip Gauntlet", "Right Gauntlet", 2);
                        break;
                    case RagdollPart.Type.LeftArm:
                        owoSkin.FeelWithMuscles($"Equip Gauntlet", "Left Gauntlet", 2);
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(Wearable), "UnEquip", new Type[] { typeof(ItemContent), typeof(Action<Item>), typeof(bool) })]
        public class OnUnEquip
        {
            [HarmonyPostfix]
            public static void Postfix(Wearable __instance)
            {
                if (!owoSkin.CanFeel() || !__instance.Creature.player || !__instance.Creature.player.isLocal) return;
                RagdollPart.Type type = __instance.Part.type;
                switch (type)
                {
                    case RagdollPart.Type.Torso:
                        owoSkin.Feel($"Unequip Chest", 2);
                        break;
                    case RagdollPart.Type.RightArm:
                        owoSkin.FeelWithMuscles($"Unequip Gauntlet", "Right Gauntlet", 2);
                        break;
                    case RagdollPart.Type.LeftArm:
                        owoSkin.FeelWithMuscles($"Unequip Gauntlet", "Left Gauntlet", 2);
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(Equipment), "ManageHolsterHandlers")]
        public class OnManageHolsterHandlers
        {
            [HarmonyPostfix]
            public static void Postfix(Equipment __instance)
            {
                __instance.OnHolsterInteractedEvent -= OnHolsterChange;
                __instance.OnHolsterInteractedEvent += OnHolsterChange;


                void OnHolsterChange(Holder holder, Item item, bool added)
                {
                    if (!owoSkin.CanFeel() || !__instance.creature.player || !__instance.creature.player.isLocal) return;
                    switch (holder.name)
                    {
                        case "BackLeft":
                            owoSkin.FeelWithMuscles(added ? "Holster" : "Unholster", "Left Back", Priority: 2);
                            break;
                        case "BackRight":
                            owoSkin.FeelWithMuscles(added ? "Holster" : "Unholster", "Right Back", Priority: 2);
                            break;
                        case "HipsLeft":
                            owoSkin.FeelWithMuscles(added ? "Holster" : "Unholster", "Left Hip", Priority: 2);
                            break;
                        case "HipsRight":
                            owoSkin.FeelWithMuscles(added ? "Holster" : "Unholster", "Right Hip", Priority: 2);
                            break;
                    }
                }
            }

        }

        [HarmonyPatch(typeof(LiquidHealth), "OnLiquidReception")]
        public class OnLiquidReceptionHealth
        {

            [HarmonyPostfix]
            public static void Postfix(LiquidReceiver liquidReceiver)
            {
                if (!owoSkin.CanFeel() || !liquidReceiver.Relay.creature.player || !liquidReceiver.Relay.creature.player.isLocal) return;
                owoSkin.Feel($"Potion Drinking", 1);
            }
        }

        [HarmonyPatch(typeof(LiquidPoison), "OnLiquidReception")]
        public class OnLiquidReceptionPoison
        {

            [HarmonyPostfix]
            public static void Postfix(LiquidReceiver liquidReceiver)
            {
                if (!owoSkin.CanFeel() || !liquidReceiver.Relay.creature.player || !liquidReceiver.Relay.creature.player.isLocal) return;
                owoSkin.Feel($"Poison Drinking", 1);
            }
        }

        [HarmonyPatch(typeof(RagdollHand), "OnHandCollision")]
        public class OnPunch
        {
            [HarmonyPostfix]
            public static void Postfix(RagdollHand __instance, CollisionInstance hit)
            {
                if (!owoSkin.CanFeel() || !__instance.creature.player || !__instance.creature.player.isLocal) return;

                Vector3 velocity = __instance.Velocity();

                //owoSkin.LOG($"OnHandCollision hit - {velocity.magnitude}");
                int intensity = Mathf.FloorToInt(Mathf.Clamp(velocity.magnitude * 10, 60, 100));

                if (velocity.magnitude <= 2) return;

                owoSkin.FeelWithMuscles("Punch", __instance.side == Side.Right ? "Right Arm" : "Left Arm", 1, intensity);
            }
        }

        #region Grab
        [HarmonyPatch(typeof(RagdollHand), "Grab", new Type[] { typeof(Handle), typeof(HandlePose), typeof(float), typeof(bool), typeof(bool) })]
        public class OnGrab
        {
            [HarmonyPostfix]
            public static void Postfix(RagdollHand __instance, Handle handle)
            {
                if (!owoSkin.CanFeel() || !__instance.creature.player || !__instance.creature.player.isLocal || !handle.item) return;

                //owoSkin.LOG($"OnHandCollision Grab - {handle.item.name} -- {__instance.side} ");

                foreach (CollisionHandler collisionHandler2 in handle.item.collisionHandlers)
                {
                    if (collisionHandler2 != null)
                    {
                        collisionHandler2.OnCollisionStartEvent += HandleHeldCollisionStart;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RagdollHand), "UnGrab")]
        public class OnUnGrab
        {
            [HarmonyPrefix]
            public static void Prefix(RagdollHand __instance)
            {
                if (!owoSkin.CanFeel() || !__instance.creature.player || !__instance.creature.player.isLocal || !__instance.grabbedHandle.item) return;

                //owoSkin.LOG($"OnHandCollision UnGrab - {__instance.grabbedHandle.item.name} -- {__instance.side} ");

                foreach (CollisionHandler collisionHandler2 in __instance.grabbedHandle.item.collisionHandlers)
                {
                    if (collisionHandler2 != null)
                    {
                        collisionHandler2.OnCollisionStartEvent -= HandleHeldCollisionStart;
                    }
                }
            }
        }
        public static void HandleHeldCollisionStart(CollisionInstance collision)
        {
            Item item = collision.sourceColliderGroup.collisionHandler.item;

            ContactPoint contact = collision.startingCollision.GetContact(0);
            Vector3 point = contact.point;
            Collider otherCollider = contact.otherCollider;
            ColliderGroup componentInParent2 = otherCollider.GetComponentInParent<ColliderGroup>();

            Vector3 result = collision.sourceColliderGroup.collisionHandler.CalculateLastPointVelocity(point);
            Vector3 vector = componentInParent2?.collisionHandler?.CalculateLastPointVelocity(point) ?? Vector3.zero;
            result.x -= vector.x;
            result.y -= vector.y;
            result.z -= vector.z;

            Vector3 velocity = (collision.sourceColliderGroup.collisionHandler.checkMinVelocity ? result : Vector3.zero);

            string muscleSensation = "Right Arm";
            //owoSkin.LOG($"OnGrabbedItemCollision hit - {velocity.magnitude}");
            if (velocity.magnitude <= 3) return;
            for (int i = 0; i < item.handlers.Count; i++)
            {
                RagdollHand ragdollHand = item.handlers[i];
                if ((bool)ragdollHand.playerHand)
                {
                    if (ragdollHand.playerHand.side == Side.Left)
                    {
                        muscleSensation = "Left Arm";
                    }
                    else
                    {
                        muscleSensation = "Right Arm";
                    }
                }
            }

            int intensity = Mathf.FloorToInt(Mathf.Clamp(velocity.magnitude * 10, 40, 100));
            owoSkin.FeelWithMuscles("Melee", muscleSensation, 1, intensity);
        }

        #endregion

        [HarmonyPatch(typeof(Creature), "Kill", new Type[] { typeof(CollisionInstance) })]
        public class OnDeath
        {
            [HarmonyPrefix]
            public static void Prefix(Creature __instance)
            {
                if (!owoSkin.CanFeel() || !__instance.player || !__instance.player.isLocal) return;

                owoSkin.StopAllHapticFeedback();
                owoSkin.Feel("Death", 4);
                owoSkin.playing = false;
            }
        }

        #endregion

        [HarmonyPatch(typeof(SpellPowerSlowTime), "SlowTime")]
        public class OnSpellSlowtimeUse
        {
            [HarmonyPostfix]
            public static void Postfix(bool __result)
            {
                if (!owoSkin.CanFeel() || !__result) return;
                owoSkin.StartSlowMotion();
            }
        }

        [HarmonyPatch(typeof(SpellPowerSlowTime), "StopSlowTime")]
        public class OnSpellStopSlowtimeUse
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.StopSlowMotion();
            }
        }
    }

}
