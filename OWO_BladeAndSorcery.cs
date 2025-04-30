using HarmonyLib;
using OWO_BaldeAndSorcery;
using ThunderRoad;


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

        [HarmonyPatch(typeof(Player), "Start")]
        public class OnStart
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.playing = true;
            }
        }
    }
}
