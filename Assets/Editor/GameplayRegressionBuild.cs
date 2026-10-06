using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class GameplayRegressionBuild
{
    public static void Run()
    {
        CheckSteering();
        CheckPopupOrder();
        foreach (string skill in new[] { "PlayerUpperSlash", "PlayerSpiralAttack", "PlayerRollingAttack", "PlayerDevideEarth" })
        {
            SkillSO asset = AssetDatabase.LoadAssetAtPath<SkillSO>(
                "Assets/Resources/Data/ScriptableObjects/SkillData/" + skill + ".asset");
            Require(asset.EventSequence.eventClips.SelectMany(c => c.events)
                .Where(e => e.eventName == eAnimEvent.EFFECT)
                .All(e => !e.effectSetting.delayEffectActive), skill + " must stop immediately");
        }
        CheckChainExit();
        Debug.Log("GAMEPLAY_REGRESSIONS_PASSED: CBS, popup order, skill effects, chain event-off and exit");

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = "Build/UnityChanRPG.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        Require(report.summary.result == BuildResult.Succeeded, "Windows build failed");
        Debug.Log("GAMEPLAY_BUILD_SUCCEEDED");
    }

    private static void CheckChainExit()
    {
        GameObject owner = new GameObject("Chain lifecycle test");
        ChainLightningProj chain = owner.AddComponent<ChainLightningProj>();
        var state = new PlayerRangedSkillState();
        var active = (List<ChainLightningProj>)typeof(PlayerRangedSkillState)
            .GetField("_activeChains", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(state);
        FieldInfo shooting = typeof(ChainLightningProj).GetField("shooting", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo generation = typeof(ChainLightningProj).GetField("_chainGen", BindingFlags.Instance | BindingFlags.NonPublic);
        try
        {
            for (int i = 0; i < 2; i++)
            {
                shooting.SetValue(chain, true);
                active.Add(chain);
                int previous = (int)generation.GetValue(chain);
                if (i == 0) state.FireEvents(eAnimEvent.SUMMON_PROJECTILES, false);
                else state.Exit();
                Require(active.Count == 0 && !(bool)shooting.GetValue(chain), "Chain must stop with skill");
                Require((int)generation.GetValue(chain) > previous, "Pending chain links must be invalidated");
                state.Exit(); // Repeated cleanup must be harmless.
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(owner); }
    }

    private static void CheckSteering()
    {
        GameObject target = new GameObject("CBS test target") { tag = "Player" };
        GameObject owner = new GameObject("CBS test owner");
        owner.SetActive(false);
        owner.transform.position = new Vector3(10000, 10000, 10000);
        MonsterBattleScript movement = owner.AddComponent<MonsterBattleScript>();
        try
        {
            movement.Awake();
            for (int heading = 0; heading < 360; heading += 30)
            {
                owner.transform.rotation = Quaternion.Euler(0, heading, 0);
                for (int angle = 0; angle < 360; angle += 15)
                {
                    Vector3 targetDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                    // Retreat -> chase -> strafe -> chase must not retain lateral weights.
                    Vector3 retreat = movement.CalculateInterests(1f, targetDir, true);
                    Require(Vector3.Dot(retreat, -targetDir) > 0.999f, "Retreat direction");
                    Vector3 chase = movement.CalculateInterests(20f, targetDir, false);
                    Require(Vector3.Dot(chase, targetDir) > 0.999f, "Chase after retreat");
                    movement.CalculateInterests(6f, targetDir, false);
                    chase = movement.CalculateInterests(20f, targetDir, false);
                    Require(Vector3.Dot(chase, targetDir) > 0.999f, "Chase after strafe");
                }
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(owner);
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    private static void CheckPopupOrder()
    {
        UIManager manager = new UIManager();
        var stack = (Stack<UI_Popup>)typeof(UIManager)
            .GetField("_popupStack", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
        var windows = new List<GameObject>();
        try
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject window = new GameObject("Popup test " + i, typeof(RectTransform), typeof(Canvas));
                windows.Add(window);
                window.GetComponent<Canvas>().sortingOrder = 10 + i;
                stack.Push(window.AddComponent<UI_Popup>());
            }
            UI_Popup first = windows[0].GetComponent<UI_Popup>();
            Vector3 position = first.transform.position;
            manager.BringPopupToFront(first);
            Require(stack.Count == 3 && stack.Peek() == first, "Click must reorder without duplicating");
            Require(windows[0].GetComponent<Canvas>().sortingOrder > windows[2].GetComponent<Canvas>().sortingOrder,
                "Clicked popup must render in front");
            manager.BringPopupToFront(first);
            Require(stack.Count == 3 && first.gameObject.activeSelf && first.transform.position == position,
                "Repeated click must preserve active popup and position");
        }
        finally
        {
            foreach (GameObject window in windows) UnityEngine.Object.DestroyImmediate(window);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
