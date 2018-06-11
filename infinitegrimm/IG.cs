﻿using Modding;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace infinitegrimm
{
    // ReSharper disable once InconsistentNaming because modding api.
    // ReSharper disable once UnusedMember.Global
    public class InfiniteGrimmMod : Mod<InfiniteSettings, InfiniteGlobalSettings>
    {
        private bool grimmchildupgrades;

        // Version detection code originally by Seanpr, used with permission.
        public override string GetVersion()
        {
            string ver = infinite_global_vars.VERSION;
            const int minApi = 40;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minApi;
            bool noModCommon = !(from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "ModCommon" select type).Any();
            bool gcup = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "GrimmchildUpgrades" select type).Any();
            if (gcup) ver += " + Gc U";


            if (apiTooLow) ver += " (Error: ModAPI too old)";
            if (noModCommon) ver += " (Error: Infinite Grimm requires ModCommon)";

            return ver;
        }

        public override void Initialize()
        {

            setupSettings();

            

            grimmchildupgrades = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "GrimmchildUpgrades" select type).Any();
            if (grimmchildupgrades)
            {
                Modding.Logger.Log("[Infinite Grimm] Grimmchild, you're looking powerful as ever!");
            }
            infinite_grimm.hardmode = GlobalSettings.HardMode;
            infinite_grimm.noLagMode = GlobalSettings.ReduceLagInGrimmFight;

            infinite_grimm.danceSpeedIncreaseDmg = GlobalSettings.DamageToIncreaseDanceSpeedByOne;
            infinite_grimm.maxDanceSpeed = GlobalSettings.MaximumDanceSpeed;
            infinite_grimm.startingDanceSpeed = GlobalSettings.StartingDanceSpeedMultiplier;
            
            infinite_grimm.staggerIncreaseDamage = GlobalSettings.DamageToIncreaseStaggerHitsByOne;
            infinite_grimm.startingStaggerHits = GlobalSettings.StartingHitsToStagger;

            infinite_tent.godMode = GlobalSettings.NightmareGodGrimm;
            
            
            
            infinite_tent.hardmode = GlobalSettings.HardMode;
            infinite_NGG.hardmode = GlobalSettings.HardMode;
            
            ModHooks.Instance.AfterSavegameLoadHook += addToGame;
            ModHooks.Instance.NewGameHook += newGame;
            ModHooks.Instance.ApplicationQuitHook += SaveGlobalSettings;
            ModHooks.Instance.SavegameSaveHook += saveLocalData;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += resetModSaveData;


        }

        // quick hack to fix problem in modding api... no seriously.
        // local data doesn't reset properly.
        private void resetModSaveData(Scene arg0, Scene arg1)
        {
            if (arg1.name != "Menu_Title") return;
            Settings.IGDamageHighScore = 0;
            Settings.IGGrimmTalkState = 0;
        }

        private void newGame()
        {
            Modding.Logger.Log("[Infinite Grimm] Current damage record for this file is: " + Settings.IGDamageHighScore);
            infinite_global_vars.maximumDamage = Settings.IGDamageHighScore;

            GameManager.instance.gameObject.AddComponent<infinite_dirtmouth>();
            GameManager.instance.gameObject.AddComponent<infinite_tent>();

            if (!GlobalSettings.NightmareGodGrimm)
            {
                GameManager.instance.gameObject.AddComponent<infinite_grimm>();
                Modding.Logger.Log("[Infinite Grimm] Please welcome Grimm to your world!");
            }
            else
            {
                GameManager.instance.gameObject.AddComponent<infinite_NGG>();
                Modding.Logger.Log("[Infinite Grimm] Please welcome the Grimm Gods to your world..." +
                                   "Beware. They want your blood.");
            }

        }

        private void addToGame(SaveGameData data)
        {
            newGame();
        }

        private void saveLocalData(int saveID)
        {
            Settings.IGDamageHighScore = infinite_global_vars.maximumDamage;
        }

        private void setupSettings()
        {
            string settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";

            bool forceReloadGlobalSettings = (GlobalSettings != null && GlobalSettings.settingsVersion != version_info.SETTINGS_VER);

            if (forceReloadGlobalSettings || !File.Exists(settingsFilePath))
            {
                if (forceReloadGlobalSettings)
                {
                    Modding.Logger.Log("[Infinite Grimm] Settings outdated! Rebuilding.");
                }
                else
                {
                    Modding.Logger.Log("[Infinite Grimm] Settings not found, rebuilding... File will be saved to: " + settingsFilePath);
                }

                GlobalSettings?.Reset();
            }
            SaveGlobalSettings();
        }

        public override int LoadPriority()
        {
            return infinite_global_vars.LOAD_ORDER;
        }


    }
}
