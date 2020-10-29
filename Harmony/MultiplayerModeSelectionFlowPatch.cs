﻿using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using LobbyBrowserMod.UI;

namespace LobbyBrowserMod.Harmony
{

    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "HandleMultiplayerLobbyControllerDidFinish", MethodType.Normal)]
    class MultiplayerModeSelectionFlowPatch
    {
        static bool Prefix(MultiplayerModeSelectionFlowCoordinator __instance, MultiplayerModeSelectionViewController viewController, MultiplayerModeSelectionViewController.MenuButton menuButton)
        {
            // When the "GameBrowser" button is clicked, bypass the game's own (broken) code & open our view instead

            if (menuButton == MultiplayerModeSelectionViewController.MenuButton.GameBrowser)
            {
                __instance.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("PresentViewController", new object[] {
                    BeatSaberUI.CreateViewController<LobbyBrowserViewController>(), null, ViewController.AnimationDirection.Vertical, false
                });

                return false;
            }

            return true;
        }
    }
}