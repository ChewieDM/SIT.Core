﻿using Comfort.Common;
using EFT;
using EFT.CameraControl;
using EFT.UI;
using HarmonyLib;
using SIT.Tarkov.Core;
using System;
using UnityEngine;

namespace SIT.Core.Coop.FreeCamera
{
    /// <summary>
    /// This is HEAVILY based on Terkoiz's work found here. Thanks for your work Terkoiz! 
    /// https://dev.sp-tarkov.com/Terkoiz/Freecam/raw/branch/master/project/Terkoiz.Freecam/FreecamController.cs
    /// </summary>

    public class FreeCameraController : MonoBehaviour
    {
        private GameObject _mainCamera;
        private FreeCamera _freeCamScript;

        private BattleUIScreen _playerUi;
        private bool _uiHidden;

        private GamePlayerOwner _gamePlayerOwner;


        public void Start()
        {
            // Find Main Camera
            //_mainCamera = GameObject.Find("FPS Camera");
            _mainCamera = FPSCamera.Instance.Camera.gameObject;
            //_mainCamera = Camera.current.gameObject;
            if (_mainCamera == null)
            {
                return;
            }

            // Add Freecam script to main camera in scene
            _freeCamScript = _mainCamera.AddComponent<FreeCamera>();
            if (_freeCamScript == null)
            {
                return;
            }

            // Get GamePlayerOwner component
            _gamePlayerOwner = GetLocalPlayerFromWorld().GetComponentInChildren<GamePlayerOwner>();
            if (_gamePlayerOwner == null)
            {
                return;
            }
        }

        private DateTime _lastTime = DateTime.MinValue;

        public void Update()
        {
            if (_gamePlayerOwner == null)
                return;

            if (_gamePlayerOwner.Player == null)
                return;

            if (_gamePlayerOwner.Player.PlayerHealthController == null)
                return;

            if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGC))
                return;

            var coopGame = coopGC.LocalGameInstance as CoopGame;
            if (coopGame == null)
                return;

            var quitState = coopGC.GetQuitState();

            if (
                (
                Input.GetKey(KeyCode.F9)
                ||
                (quitState != CoopGameComponent.EQuitState.NONE && !_freeCamScript.IsActive)
                )
                && _lastTime < DateTime.Now.AddSeconds(-3)
            )
            {
                _lastTime = DateTime.Now;
                ToggleCamera();
                ToggleUi();
            }

            // Player is dead. Remove all effects!
            if (!_gamePlayerOwner.Player.PlayerHealthController.IsAlive && _freeCamScript.IsActive)
            {
                var fpsCamInstance = FPSCamera.Instance;
                if (fpsCamInstance == null)
                    return;


                if (fpsCamInstance.EffectsController == null)
                    return;


                // Death Fade (the blink to death). Don't show this as we want to continue playing after death!
                var deathFade = fpsCamInstance.EffectsController.GetComponent<DeathFade>();
                if (deathFade != null)
                {
                    deathFade.enabled = false;
                    GameObject.Destroy(deathFade);
                }

                // Fast Blur. Don't show this as we want to continue playing after death!
                var fastBlur = fpsCamInstance.EffectsController.GetComponent<FastBlur>();
                if (fastBlur != null)
                {
                    fastBlur.enabled = false;
                }

            }

            //if (_freeCamScript.IsActive && (!_lastOcclusionCullCheck.HasValue))
            //{
            //    _lastOcclusionCullCheck = DateTime.Now;
            //    if (!_playerDeathOrExitPosition.HasValue)
            //        _playerDeathOrExitPosition = _gamePlayerOwner.Player.Position;

            //    if (showAtDeathOrExitPosition)
            //        _gamePlayerOwner.Player.Position = _playerDeathOrExitPosition.Value;
            //    else
            //        _gamePlayerOwner.Player.Position = (Camera.current.transform.position);


            //    showAtDeathOrExitPosition = !showAtDeathOrExitPosition;

            //}
            //else if (!_freeCamScript.IsActive)
            //{
            //    _lastOcclusionCullCheck = null;
            //}
        }

        //DateTime? _lastOcclusionCullCheck = null;
        //Vector3? _playerDeathOrExitPosition;
        //bool showAtDeathOrExitPosition;

        /// <summary>
        /// Toggles the Freecam mode
        /// </summary>
        public void ToggleCamera()
        {
            // Get our own Player instance. Null means we're not in a raid
            var localPlayer = GetLocalPlayerFromWorld();
            if (localPlayer == null)
                return;

            if (!_freeCamScript.IsActive)
            {
                SetPlayerToFreecamMode(localPlayer);
            }
            else
            {
                SetPlayerToFirstPersonMode(localPlayer);
            }
        }

        /// <summary>
        /// Hides the main UI (health, stamina, stance, hotbar, etc.)
        /// </summary>
        private void ToggleUi()
        {
            // Check if we're currently in a raid
            if (GetLocalPlayerFromWorld() == null)
                return;

            // If we don't have the UI Component cached, go look for it in the scene
            if (_playerUi == null)
            {
                _playerUi = GameObject.Find("BattleUIScreen").GetComponent<BattleUIScreen>();

                if (_playerUi == null)
                {
                    //FreecamPlugin.Logger.LogError("Failed to locate player UI");
                    return;
                }
            }

            _playerUi.gameObject.SetActive(_uiHidden);
            _uiHidden = !_uiHidden;
        }

        /// <summary>
        /// A helper method to set the Player into Freecam mode
        /// </summary>
        /// <param name="localPlayer"></param>
        private void SetPlayerToFreecamMode(EFT.Player localPlayer)
        {
            // We set the player to third person mode
            // This means our character will be fully visible, while letting the camera move freely
            localPlayer.PointOfView = EPointOfView.ThirdPerson;

            // Get the PlayerBody reference. It's a protected field, so we have to use traverse to fetch it
            var playerBody = Traverse.Create(localPlayer).Field<PlayerBody>("_playerBody").Value;
            if (playerBody != null)
            {
                playerBody.PointOfView.Value = EPointOfView.FreeCamera;
                localPlayer.GetComponent<PlayerCameraController>().UpdatePointOfView();
            }

            _gamePlayerOwner.enabled = false;
            _freeCamScript.IsActive = true;
        }

        /// <summary>
        /// A helper method to reset the player view back to First Person
        /// </summary>
        /// <param name="localPlayer"></param>
        private void SetPlayerToFirstPersonMode(EFT.Player localPlayer)
        {
            _freeCamScript.IsActive = false;

            //if (FreecamPlugin.CameraRememberLastPosition.Value)
            //{
            //    _lastPosition = _mainCamera.transform.position;
            //    _lastRotation = _mainCamera.transform.rotation;
            //}

            // re-enable _gamePlayerOwner
            _gamePlayerOwner.enabled = true;

            localPlayer.PointOfView = EPointOfView.FirstPerson;
            FPSCamera.Instance.SetOcclusionCullingEnabled(true);

        }

        /// <summary>
        /// Gets the current <see cref="Player"/> instance if it's available
        /// </summary>
        /// <returns>Local <see cref="Player"/> instance; returns null if the game is not in raid</returns>
        private EFT.Player GetLocalPlayerFromWorld()
        {
            // If the GameWorld instance is null or has no RegisteredPlayers, it most likely means we're not in a raid
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null)
                return null;

            // One of the RegisteredPlayers will have the IsYourPlayer flag set, which will be our own Player instance
            return gameWorld.MainPlayer;
        }

        public void OnDestroy()
        {
            // Destroy FreeCamScript before FreeCamController if exists
            Destroy(_freeCamScript);
            Destroy(this);
        }
    }
}
