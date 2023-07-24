namespace PotionSeller
{
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Adds the mod's version number to the main menu.
    /// </summary>
    [HarmonyPatch(typeof(MenuMain), "Start")]
    class MenuMainPatch
    {
        static void Prefix(MenuMain __instance)
        {
            GameObject gobject = Object.Instantiate(__instance.transform.Find("TopPanel/OptionStart/text").gameObject);
            gobject.name = "Potion Seller version";
            gobject.transform.parent = __instance.transform.Find("TopPanel");
            gobject.transform.localPosition = new Vector3(388, -300);
            TextMesh textMesh = gobject.GetComponent<TextMesh>();
            textMesh.text = $"Potion Seller {Plugin.Version}";
        }
    }
}
