using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpaceShooterMenus.EditorTools
{
    /// <summary>
    /// Builds the "Neon Arcade" menus into the project:
    ///  - Assets/Scenes/MainMenu.unity  (Play / How to Play / Quit)
    ///  - Assets/Menus/Prefabs/Game Menus.prefab (pause menu, Victory screen, Game Over screen)
    ///    and one instance of it in SampleScene
    ///  - Build Settings: MainMenu first, then SampleScene.
    /// Menu: Space Shooter / Build Menus. Safe to run again (it rebuilds its own objects only).
    /// Layout numbers are the design's pixels on a 1280 x 720 frame.
    /// </summary>
    public static class MenuBuilder
    {
        const string Root = "Assets/Menus";
        const string ArtDir = Root + "/Art";
        const string FontDir = Root + "/Fonts";
        const string PrefabDir = Root + "/Prefabs";
        const string GameMenusPrefabPath = PrefabDir + "/Game Menus.prefab";
        const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        const string GameScenePath = "Assets/Scenes/SampleScene.unity";
        const string GameMenusName = "Game Menus";

        // ---------------------------------------------------------------- palette (design tokens)
        static readonly Color Ink = Hex("070A1A");
        static readonly Color Cyan = Hex("22D3EE");
        static readonly Color Pink = Hex("F0479B");
        static readonly Color Ice = Hex("E9FBFF");
        static readonly Color IceSoft = Hex("CFF7FF");
        static readonly Color Muted = Hex("8EA0C8");
        static readonly Color KeyLine = Hex("3A4A7A");
        static readonly Color DarkOnCyan = Hex("04121A");
        static readonly Color Blush = Hex("FFE9F3");
        static readonly Color BlushMuted = Hex("C9A3B8");
        static readonly Color DarkOnPink = Hex("1A0412");

        // ---------------------------------------------------------------- assets
        static TMP_FontAsset orb600, orb800, orb900, raj600, raj700;
        static Material glowCyanTitle, glowCyanPaused, glowPinkTitle;
        static Sprite rr4Fill, rr4Line2, rr6Fill, rr6Line1, rr8Fill, rr8Line1, glowSprite, menuBg, winDeco;

        [MenuItem("Space Shooter/Build Menus", priority = 20)]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Space Shooter Menus", "Stop Play mode first, then run Build Menus again.", "OK");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameScenePath) == null)
            {
                EditorUtility.DisplayDialog("Space Shooter Menus", "Can't find " + GameScenePath + ".", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            try
            {
                EditorUtility.DisplayProgressBar("Space Shooter Menus", "Fonts and sprites", 0.1f);
                LoadAssets();
                EditorUtility.DisplayProgressBar("Space Shooter Menus", "Main menu scene", 0.4f);
                BuildMainMenuScene();
                EditorUtility.DisplayProgressBar("Space Shooter Menus", "Pause / Victory / Game Over in SampleScene", 0.7f);
                BuildGameMenus();
                SetBuildScenes();
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Debug.Log("[Space Shooter Menus] Built " + MainMenuScenePath + " and " + GameMenusPrefabPath + " (added to " + GameScenePath + ").");
        }

        // ================================================================ assets
        static void LoadAssets()
        {
            rr4Fill = SpriteAt("ui_rr4_fill.png", 12);
            rr4Line2 = SpriteAt("ui_rr4_line2.png", 12);
            rr6Fill = SpriteAt("ui_rr6_fill.png", 16);
            rr6Line1 = SpriteAt("ui_rr6_line1.png", 16);
            rr8Fill = SpriteAt("ui_rr8_fill.png", 20);
            rr8Line1 = SpriteAt("ui_rr8_line1.png", 20);
            glowSprite = SpriteAt("ui_glow.png", 72);
            menuBg = SpriteAt("MainMenu_BG.png", 0);
            winDeco = SpriteAt("A_Win_Deco.png", 0);

            orb600 = FontAssetFor("Orbitron-SemiBold", 9);
            orb800 = FontAssetFor("Orbitron-ExtraBold", 12);
            orb900 = FontAssetFor("Orbitron-Black", 16);
            raj600 = FontAssetFor("Rajdhani-SemiBold", 9);
            raj700 = FontAssetFor("Rajdhani-Bold", 9);

            glowCyanTitle = GlowMaterial(orb900, "Glow Cyan", new Color(Cyan.r, Cyan.g, Cyan.b, 0.6f), 0.55f);
            glowPinkTitle = GlowMaterial(orb900, "Glow Pink", new Color(Pink.r, Pink.g, Pink.b, 0.7f), 0.55f);
            glowCyanPaused = GlowMaterial(orb800, "Glow Cyan", new Color(Cyan.r, Cyan.g, Cyan.b, 0.5f), 0.45f);
        }

        static Sprite SpriteAt(string file, int border)
        {
            string path = ArtDir + "/" + file;
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) throw new Exception("[Space Shooter Menus] missing texture " + path);

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.filterMode = FilterMode.Bilinear;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.maxTextureSize = 2048;
            ti.spriteBorder = new Vector4(border, border, border, border);
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            ti.SetTextureSettings(settings);
            ti.SaveAndReimport();

            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) throw new Exception("[Space Shooter Menus] could not load sprite " + path);
            return s;
        }

        static TMP_FontAsset FontAssetFor(string ttfName, int padding)
        {
            string assetPath = FontDir + "/" + ttfName + " SDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) return existing;

            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontDir + "/" + ttfName + ".ttf");
            if (font == null) throw new Exception("[Space Shooter Menus] missing font " + ttfName + ".ttf");

            TMP_FontAsset fa = TMP_FontAsset.CreateFontAsset(font, 90, padding, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            fa.name = ttfName + " SDF";
            AssetDatabase.CreateAsset(fa, assetPath);

            Texture2D[] atlases = fa.atlasTextures;
            if (atlases != null)
            {
                for (int i = 0; i < atlases.Length; i++)
                {
                    if (atlases[i] == null) continue;
                    atlases[i].name = ttfName + " Atlas" + (i == 0 ? "" : " " + i);
                    AssetDatabase.AddObjectToAsset(atlases[i], fa);
                }
            }
            fa.material.name = ttfName + " SDF Material";
            AssetDatabase.AddObjectToAsset(fa.material, fa);

            fa.TryAddCharacters(" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789:/%.,!?-'()", out string _);
            EditorUtility.SetDirty(fa);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        }

        static Material GlowMaterial(TMP_FontAsset fa, string suffix, Color glow, float outer)
        {
            string path = FontDir + "/" + fa.name + " " + suffix + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(fa.material);
                AssetDatabase.CreateAsset(m, path);
            }
            Shader sdf = Shader.Find("TextMeshPro/Distance Field");
            if (sdf != null && m.shader != sdf) m.shader = sdf;
            m.SetTexture("_MainTex", fa.atlasTexture);
            m.EnableKeyword("GLOW_ON");
            m.SetColor("_GlowColor", glow);
            m.SetFloat("_GlowOffset", 0f);
            m.SetFloat("_GlowInner", 0.05f);
            m.SetFloat("_GlowOuter", outer);
            m.SetFloat("_GlowPower", 0.75f);
            EditorUtility.SetDirty(m);
            return m;
        }

        // ================================================================ Main menu scene
        static void BuildMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Ink;

            CreateEventSystem();

            RectTransform canvas = CreateCanvas("Main Menu Canvas", 0);
            var controller = canvas.gameObject.AddComponent<MainMenuController>();

            // background picture (the game's own starfield, ships and planets) covers any screen shape
            Image bg = Img(canvas, "Background", menuBg, Color.white, false);
            var fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;

            // ---- main panel
            RectTransform mainPanel = Stretch(NewRect("Main Panel", canvas));
            RectTransform f = Frame(mainPanel);

            // game name
            Txt(f, "Title", "SPACE SHOOTER", orb900, 88, Ice, 0.05f, TextAlignmentOptions.Center, 0, 140, 1280, 96, glowCyanTitle);

            NeonButton play = MakeButton(f, "PLAY", 460, 307, 360, 62, CyanStyle(26, 24, 0.24f, Hex("070A1A", 0.6f)));
            NeonButton how = MakeButton(f, "HOW TO PLAY", 460, 385, 360, 62, CyanStyle(26, 24, 0.24f, Hex("070A1A", 0.6f)));
            NeonButton quit = MakeButton(f, "QUIT", 460, 463, 360, 62, CyanStyle(26, 24, 0.24f, Hex("070A1A", 0.6f)));
            OnClick(play, controller.Play);
            OnClick(how, controller.OpenHowToPlay);
            OnClick(quit, controller.Quit);
            mainPanel.gameObject.AddComponent<SelectOnEnable>().firstSelected = play.gameObject;

            // controls row
            KeyHint(f, "WASD", "MOVE", 409, 661, 72);
            KeyHint(f, "SPACE", "SHOOT", 573, 661, 77);
            KeyHint(f, "ESC", "PAUSE", 751, 661, 55);

            // ---- how to play panel (same style as the pause dialog)
            RectTransform howPanel = Stretch(NewRect("How To Play Panel", canvas));
            Img(howPanel, "Dim", null, Hex("040610", 0.74f), false).raycastTarget = true;
            RectTransform hf = Frame(howPanel);
            Dialog(hf, 370, 120, 540, 480, Hex("0A0E22", 0.94f), Hex("22D3EE", 0.45f), Hex("22D3EE", 0.18f));
            Txt(hf, "Title", "HOW TO PLAY", orb800, 38, Ice, 0.12f, TextAlignmentOptions.Center, 370, 156, 540, 52, glowCyanPaused);
            ControlRow(hf, "MOVE", "WASD / ARROWS", 236, 150);
            ControlRow(hf, "SHOOT", "HOLD SPACE", 284, 116);
            ControlRow(hf, "PAUSE", "ESC / P", 332, 86);
            var goal = Txt(hf, "Goal", "Destroy every enemy to win.\nEnemy fireballs cost you a heart.\nLose all your hearts and it's game over.",
                raj600, 19, IceSoft, 0.04f, TextAlignmentOptions.Center, 370, 382, 540, 84);
            goal.lineSpacing = 4f;
            NeonButton back = MakeButton(hf, "BACK", 460, 494, 360, 58, CyanStyle(24, 22, 0.22f, Color.clear));
            OnClick(back, controller.CloseHowToPlay);
            howPanel.gameObject.AddComponent<SelectOnEnable>().firstSelected = back.gameObject;
            howPanel.gameObject.SetActive(false);

            controller.mainPanel = mainPanel.gameObject;
            controller.howToPlayPanel = howPanel.gameObject;
            controller.gameScene = Path.GetFileNameWithoutExtension(GameScenePath);
            EditorUtility.SetDirty(controller);

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        // ================================================================ SampleScene: pause / victory / game over
        static void BuildGameMenus()
        {
            Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

            // remove what an earlier run of this builder made (nothing else in the scene is touched)
            foreach (GameObject rootGo in scene.GetRootGameObjects())
                if (rootGo.name == GameMenusName) Object.DestroyImmediate(rootGo);

            // the scene already has an EventSystem; only add one if it's missing
            if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) == null) CreateEventSystem();

            RectTransform canvas = CreateCanvas(GameMenusName, 20);
            var flow = canvas.gameObject.AddComponent<GameFlow>();
            var pause = canvas.gameObject.AddComponent<PauseMenuController>();
            flow.mainMenuScene = "MainMenu";
            pause.mainMenuScene = "MainMenu";

            // ---- pause
            RectTransform pausePanel = Stretch(NewRect("Pause Panel", canvas));
            Img(pausePanel, "Dim", null, Hex("040610", 0.74f), false).raycastTarget = true;
            RectTransform pf = Frame(pausePanel);
            Dialog(pf, 420, 156, 440, 408, Hex("0A0E22", 0.94f), Hex("22D3EE", 0.45f), Hex("22D3EE", 0.18f));
            Txt(pf, "Title", "PAUSED", orb800, 46, Ice, 0.14f, TextAlignmentOptions.Center, 461, 197, 358, 58, glowCyanPaused);
            NeonButton resume = MakeButton(pf, "RESUME", 461, 283, 358, 58, CyanStyle(24, 22, 0.22f, Color.clear));
            NeonButton restart = MakeButton(pf, "RESTART", 461, 355, 358, 58, CyanStyle(24, 22, 0.22f, Color.clear));
            NeonButton pauseMenu = MakeButton(pf, "MAIN MENU", 461, 427, 358, 58, CyanStyle(24, 22, 0.22f, Color.clear));
            OnClick(resume, pause.Resume);
            OnClick(restart, pause.Restart);
            OnClick(pauseMenu, pause.MainMenu);
            Txt(pf, "Hint Left", "Press", raj600, 17, Muted, 0.08f, TextAlignmentOptions.Right, 461, 509, 127, 22);
            KeyCap(pf, "ESC", 596, 507, 49, 26, 17);
            Txt(pf, "Hint Right", "to resume", raj600, 17, Muted, 0.08f, TextAlignmentOptions.Left, 653, 509, 160, 22);
            pausePanel.gameObject.AddComponent<SelectOnEnable>().firstSelected = resume.gameObject;
            pausePanel.gameObject.SetActive(false);
            pause.pausePanel = pausePanel.gameObject;

            // ---- victory
            RectTransform winPanel = Stretch(NewRect("Victory Panel", canvas));
            Img(winPanel, "Dim", null, Hex("070A1A", 0.9f), false).raycastTarget = true;
            Image deco = Img(winPanel, "Rings", winDeco, Color.white, false);
            var decoFit = deco.gameObject.AddComponent<AspectRatioFitter>();
            decoFit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            decoFit.aspectRatio = 16f / 9f;
            RectTransform wf = Frame(winPanel);
            Txt(wf, "Kicker", "ALL ENEMIES DESTROYED", raj700, 20, Cyan, 0.4f, TextAlignmentOptions.Center, 0, 92, 1280, 26);
            Txt(wf, "Title", "VICTORY", orb900, 112, Ice, 0.08f, TextAlignmentOptions.Center, 0, 128, 1280, 112, glowCyanTitle);
            var winEnd = winPanel.gameObject.AddComponent<EndGamePanel>();
            winEnd.scoreValue = StatTile(wf, "SCORE", "000300", 310, 296, 209, Hex("0A0E22", 0.8f), Hex("22D3EE", 0.35f), Muted, Ice);
            winEnd.timeValue = StatTile(wf, "TIME", "01:24", 535, 296, 209, Hex("0A0E22", 0.8f), Hex("22D3EE", 0.35f), Muted, Ice);
            winEnd.heartsValue = StatTile(wf, "HEARTS LEFT", "3/4", 761, 296, 209, Hex("0A0E22", 0.8f), Hex("22D3EE", 0.35f), Muted, Ice);
            NeonButton again = MakeButton(wf, "PLAY AGAIN", 372, 449, 260, 60, CyanStyle(24, 22, 0.22f, Hex("070A1A", 0.6f)));
            NeonButton winMenu = MakeButton(wf, "MAIN MENU", 648, 449, 260, 60, CyanStyle(24, 22, 0.22f, Hex("070A1A", 0.6f)));
            OnClick(again, winEnd.PlayAgain);
            OnClick(winMenu, winEnd.MainMenu);
            winPanel.gameObject.AddComponent<SelectOnEnable>().firstSelected = again.gameObject;
            winPanel.gameObject.SetActive(false);

            // ---- game over
            RectTransform losePanel = Stretch(NewRect("Game Over Panel", canvas));
            Img(losePanel, "Dim", null, Hex("12040E", 0.76f), false).raycastTarget = true;
            RectTransform lf = Frame(losePanel);
            Txt(lf, "Kicker", "YOUR SHIP WAS DESTROYED", raj700, 20, Pink, 0.4f, TextAlignmentOptions.Center, 0, 108, 1280, 26);
            Txt(lf, "Title", "GAME OVER", orb900, 104, Blush, 0.06f, TextAlignmentOptions.Center, 0, 144, 1280, 104, glowPinkTitle);
            var loseEnd = losePanel.gameObject.AddComponent<EndGamePanel>();
            loseEnd.scoreValue = StatTile(lf, "SCORE", "000200", 410, 304, 222, Hex("1A0816", 0.85f), Hex("F0479B", 0.4f), BlushMuted, Blush);
            loseEnd.timeValue = StatTile(lf, "SURVIVED", "00:47", 648, 304, 222, Hex("1A0816", 0.85f), Hex("F0479B", 0.4f), BlushMuted, Blush);
            var pinkStyle = new ButtonStyle
            {
                accent = Pink, idleBorder = Hex("F0479B", 0.6f), idleFill = Color.clear, idleText = Blush, activeText = DarkOnPink,
                activeSize = 24, idleSize = 22, spacing = 0.22f, glow = Hex("F0479B", 0.45f),
            };
            NeonButton retry = MakeButton(lf, "RETRY", 372, 457, 260, 60, pinkStyle);
            NeonButton loseMenu = MakeButton(lf, "MAIN MENU", 648, 457, 260, 60, pinkStyle);
            OnClick(retry, loseEnd.PlayAgain);
            OnClick(loseMenu, loseEnd.MainMenu);
            losePanel.gameObject.AddComponent<SelectOnEnable>().firstSelected = retry.gameObject;
            losePanel.gameObject.SetActive(false);

            flow.winPanel = winPanel.gameObject;
            flow.losePanel = losePanel.gameObject;

            // Save it all as one prefab, so the scene only gets a single prefab instance
            // (keeps SampleScene changes small for the rest of the team).
            if (!AssetDatabase.IsValidFolder(PrefabDir)) AssetDatabase.CreateFolder(Root, "Prefabs");
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvas.gameObject, GameMenusPrefabPath, InteractionMode.AutomatedAction);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static void SetBuildScenes()
        {
            var list = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(MainMenuScenePath, true) };
            bool hasGame = false;
            foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
            {
                if (s.path == MainMenuScenePath) continue;
                if (s.path == GameScenePath) { hasGame = true; list.Add(new EditorBuildSettingsScene(GameScenePath, true)); }
                else list.Add(s); // keep anything the team added
            }
            if (!hasGame) list.Insert(1, new EditorBuildSettingsScene(GameScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        // ================================================================ building blocks
        struct ButtonStyle
        {
            public Color accent, idleBorder, idleFill, idleText, activeText, glow;
            public float activeSize, idleSize, spacing;
        }

        static ButtonStyle CyanStyle(float activeSize, float idleSize, float spacing, Color idleFill)
        {
            return new ButtonStyle
            {
                accent = Cyan, idleBorder = Hex("22D3EE", 0.55f), idleFill = idleFill, idleText = IceSoft, activeText = DarkOnCyan,
                activeSize = activeSize, idleSize = idleSize, spacing = spacing, glow = Hex("22D3EE", 0.45f),
            };
        }

        static NeonButton MakeButton(RectTransform parent, string label, float x, float y, float w, float h, ButtonStyle st)
        {
            RectTransform r = Place(NewRect(label + " Button", parent), x, y, w, h);
            var hit = r.gameObject.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0); // invisible, catches the mouse
            var button = r.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = hit;

            Image glow = Img(r, "Glow", glowSprite, st.glow, true);
            glow.rectTransform.offsetMin = new Vector2(-28, -28);
            glow.rectTransform.offsetMax = new Vector2(28, 28);
            Image fill = Img(r, "Fill", rr4Fill, st.idleFill, true);
            Image border = Img(r, "Border", rr4Line2, st.idleBorder, true);
            TextMeshProUGUI text = Txt(r, "Label", label, raj600, st.idleSize, st.idleText, st.spacing, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);

            var nb = r.gameObject.AddComponent<NeonButton>();
            nb.fill = fill;
            nb.border = border;
            nb.glow = glow;
            nb.label = text;
            nb.accent = st.accent;
            nb.idleFill = st.idleFill;
            nb.idleBorder = st.idleBorder;
            nb.idleText = st.idleText;
            nb.activeText = st.activeText;
            nb.idleFont = raj600;
            nb.activeFont = raj700;
            nb.idleSize = st.idleSize;
            nb.activeSize = st.activeSize;
            nb.Apply(false);
            return nb;
        }

        static void OnClick(NeonButton b, UnityAction action)
        {
            UnityEventTools.AddPersistentListener(b.GetComponent<Button>().onClick, action);
        }

        static void Dialog(RectTransform parent, float x, float y, float w, float h, Color fill, Color line, Color glow)
        {
            RectTransform r = Place(NewRect("Dialog", parent), x, y, w, h);
            Image g = Img(r, "Glow", glowSprite, glow, true);
            g.rectTransform.offsetMin = new Vector2(-28, -28);
            g.rectTransform.offsetMax = new Vector2(28, 28);
            Img(r, "Fill", rr8Fill, fill, true);
            Img(r, "Border", rr8Line1, line, true);
        }

        static TMP_Text StatTile(RectTransform parent, string label, string sample, float x, float y, float w, Color fill, Color line, Color labelColor, Color valueColor)
        {
            RectTransform tile = Place(NewRect(label + " Tile", parent), x, y, w, 105);
            Img(tile, "Fill", rr6Fill, fill, true);
            Img(tile, "Border", rr6Line1, line, true);
            Txt(tile, "Label", label, raj600, 16, labelColor, 0.22f, TextAlignmentOptions.Center, 0, 19, w, 21);
            return Txt(tile, "Value", sample, orb800, 34, valueColor, 0f, TextAlignmentOptions.Center, 0, 44, w, 42);
        }

        static void KeyCap(RectTransform parent, string key, float x, float y, float w, float h, float size)
        {
            RectTransform r = Place(NewRect(key + " Key", parent), x, y, w, h);
            Img(r, "Line", rr6Line1, KeyLine, true);
            Stretch(Txt(r, "Text", key, raj600, size, IceSoft, 0.08f, TextAlignmentOptions.Center).rectTransform);
        }

        static void KeyHint(RectTransform parent, string key, string action, float x, float y, float keyWidth)
        {
            KeyCap(parent, key, x, y, keyWidth, 29, 18);
            Txt(parent, action + " Hint", action, raj600, 18, Muted, 0.08f, TextAlignmentOptions.Left, x + keyWidth + 10, y + 1, 90, 27);
        }

        static void ControlRow(RectTransform parent, string action, string keys, float y, float keyWidth)
        {
            Txt(parent, action + " Label", action, raj600, 20, Muted, 0.2f, TextAlignmentOptions.Left, 420, y, 200, 32);
            KeyCap(parent, keys, 860 - keyWidth, y + 1, keyWidth, 30, 18);
        }

        // ================================================================ low-level UI helpers
        static RectTransform CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = LayerMask.NameToLayer("UI");
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referencePixelsPerUnit = 100;
            return (RectTransform)go.transform;
        }

        static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
            // Same UI input module as SampleScene (Input System package); looked up by name so this file
            // still compiles if the package ever changes.
            Type module = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (module != null)
            {
                Component c = go.AddComponent(module);
                MethodInfo assign = module.GetMethod("AssignDefaultActions", BindingFlags.Public | BindingFlags.Instance);
                if (assign != null) assign.Invoke(c, null);
            }
            else
            {
                go.AddComponent<StandaloneInputModule>();
            }
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static RectTransform Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            return r;
        }

        /// <summary>A 1280 x 720 box in the middle of the screen; children use the design's pixel positions.</summary>
        static RectTransform Frame(RectTransform parent)
        {
            RectTransform r = NewRect("Frame 1280x720", parent);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = new Vector2(1280, 720);
            return r;
        }

        /// <summary>Top-left x, y, width, height inside the parent (design pixels).</summary>
        static RectTransform Place(RectTransform r, float x, float y, float w, float h)
        {
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
            return r;
        }

        static Image Img(Transform parent, string name, Sprite sprite, Color color, bool sliced)
        {
            RectTransform r = Stretch(NewRect(name, parent));
            var img = r.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            if (sliced)
            {
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 2f; // sprites are drawn at 2x
                img.fillCenter = true;
            }
            return img;
        }

        static TextMeshProUGUI Txt(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color, float spacingEm, TextAlignmentOptions align)
        {
            RectTransform r = NewRect(name, parent);
            var t = r.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSize = size;
            t.color = color;
            t.characterSpacing = spacingEm * 100f;
            t.alignment = align;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false;
            t.text = text;
            return t;
        }

        static TextMeshProUGUI Txt(RectTransform parent, string name, string text, TMP_FontAsset font, float size, Color color, float spacingEm, TextAlignmentOptions align,
            float x, float y, float w, float h, Material material = null)
        {
            TextMeshProUGUI t = Txt(parent, name, text, font, size, color, spacingEm, align);
            Place(t.rectTransform, x, y, w, h);
            if (material != null) t.fontSharedMaterial = material;
            return t;
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            c.a = alpha;
            return c;
        }
    }
}
