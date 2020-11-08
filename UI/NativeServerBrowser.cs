﻿using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.UI.Components;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ServerBrowser.UI
{
    public class NativeServerBrowser : MonoBehaviour
    {
        #region Creation / Instance
        private const string GameObjectName = "NativeServerBrowserButItsTheMod";

        public static NativeServerBrowser Instance
        {
            get;
            private set;
        }

        public static GameServerBrowserViewController ViewController
        {
            get;
            private set;
        }

        public static NativeServerBrowser SetUp()
        {
            if (ViewController == null)
            {
                ViewController = Resources.FindObjectsOfTypeAll<GameServerBrowserViewController>().FirstOrDefault();
            }

            if (Instance == null)
            {
                Instance = ViewController.gameObject.AddComponent<NativeServerBrowser>();
            }

            return Instance;
        }
        #endregion

        #region Core / Data
        private void OnEnable()
        {
            MpModeSelection.SetTitle("Server Browser");

            HostedGameBrowser.OnUpdate += OnBrowserUpdate;
            _tableView.joinButtonPressedEvent += OnJoinPressed;

            DoFullRefresh();
        }

        private void OnDisable()
        {
            HostedGameBrowser.OnUpdate -= OnBrowserUpdate;
            _tableView.joinButtonPressedEvent -= OnJoinPressed;
        }

        private void DoFullRefresh()
        {
            _refreshButton.interactable = false;
            _filterButton.interactable = false;

            _mainLoadingControl.ShowLoading("Loading server list");

            HostedGameBrowser.FullRefresh(null);
        }

        private void OnBrowserUpdate()
        {
            _refreshButton.interactable = true;
            _filterButton.interactable = true;

            _mainLoadingControl.Hide();

            _tableView.SetData(HostedGameBrowser.LobbiesOnPage, true);
        }

        private void OnJoinPressed(INetworkPlayer game)
        {
            ((HostedGameData)game).Join();
        }
        #endregion

        #region Components
        private Button _createServerButton;
        private Button _filterButton;
        private CurvedTextMeshPro _filterButtonLabel;
        private Button _refreshButton;
        private LoadingControl _mainLoadingControl;
        private GameServersListTableView _tableView;
        #endregion

        #region Awake (UI Setup)
        private void Awake()
        {
            // Create server button
            var createServerButtonTransform = transform.Find("CreateServerButton");
            createServerButtonTransform.localPosition = new Vector3(-76.50f, 40.0f, 0.0f);

            _createServerButton = transform.Find("CreateServerButton").GetComponent<Button>();
            _createServerButton.onClick.AddListener(delegate
            {
                MpModeSelection.OpenCreateServerMenu();
            });

            // Move the top-right loading control up, so the refresh button aligns properly
            (transform.Find("Filters/SmallLoadingControl") as RectTransform).localPosition = new Vector3(62.0f, 3.5f, 0.0f);

            // Resize the filters bar so it doesn't overlap the refresh button
            var filterButtonTransform = (transform.Find("Filters/FilterButton") as RectTransform);
            filterButtonTransform.sizeDelta = new Vector2(-11.0f, 10.0f);
            filterButtonTransform.offsetMax = new Vector2(-11.0f, 5.0f);

            _filterButton = filterButtonTransform.GetComponent<Button>();
            _filterButton.onClick.AddListener(delegate
            {
                // TODO Filter click
                MpModeSelection.SetTitle("FILTER CLICK");
            });

            // Filters lable
            _filterButtonLabel = transform.Find("Filters/FilterButton/Content/Text").GetComponent<CurvedTextMeshPro>();
            _filterButtonLabel.text = "Hello world!";

            // Hide top-right loading spinner
            Destroy(transform.Find("Filters/SmallLoadingControl/LoadingContainer").gameObject);
            Destroy(transform.Find("Filters/SmallLoadingControl/DownloadingContainer").gameObject);

            // Refresh button
            var refreshContainer = transform.Find("Filters/SmallLoadingControl/RefreshContainer");
            refreshContainer.gameObject.SetActive(true);

            _refreshButton = refreshContainer.Find("RefreshButton").GetComponent<Button>();
            _refreshButton.onClick.AddListener(delegate
            {
                DoFullRefresh();
            });

            // Change "Music Packs" table header to "Type"
            transform.Find("GameServersListTableView/GameServerListTableHeader/LabelsContainer/MusicPack").GetComponent<CurvedTextMeshPro>()
                .SetText("Type");

            // Main loading control
            _mainLoadingControl = transform.Find("GameServersListTableView/TableView/Viewport/MainLoadingControl").GetComponent<LoadingControl>();
            _mainLoadingControl.ShowLoading("Initializing");

            // Table view
            _tableView = transform.Find("GameServersListTableView").GetComponent<GameServersListTableView>();

            // Modify content cell prefab (add a background)
            var contentCellPrefab = _tableView.GetField<GameServerListTableCell, GameServersListTableView>("_gameServerListCellPrefab");

            var backgroundBase = Resources.FindObjectsOfTypeAll<ImageView>().First(x => x.gameObject?.name == "Background"
                && x.sprite != null && x.sprite.name.StartsWith("RoundRect10"));

            var backgroundClone = UnityEngine.Object.Instantiate(backgroundBase);
            backgroundClone.transform.SetParent(contentCellPrefab.transform, false);
            backgroundClone.transform.SetAsFirstSibling();
            backgroundClone.name = "Background";

            var backgroundTransform = backgroundClone.transform as RectTransform;
            backgroundTransform.anchorMin = new Vector2(0.0f, 0.0f);
            backgroundTransform.anchorMax = new Vector2(0.95f, 1.0f);
            backgroundTransform.offsetMin = new Vector2(0.5f, 0.0f);
            backgroundTransform.offsetMax = new Vector2(5.0f, 0.0f);
            backgroundTransform.sizeDelta = new Vector2(4.50f, 0.0f);

            var cellBackgroundHelper = contentCellPrefab.gameObject.AddComponent<CellBackgroundHelper>();
            cellBackgroundHelper.Cell = contentCellPrefab;
            cellBackgroundHelper.Background = backgroundClone;
        }
        #endregion
    }
}
