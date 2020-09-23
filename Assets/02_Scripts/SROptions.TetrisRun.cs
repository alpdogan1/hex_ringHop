// ReSharper disable UnusedMember.Global
using System.ComponentModel;
using Hex.Modules;
using TetrisRun;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once InconsistentNaming
public partial class SROptions
{
    /*[Category("Testing")]
    public void ShowTutorial()
    {
        var tutorialHandler = Object.FindObjectOfType<UITutorialHandler>();
        tutorialHandler.Show();
    }*/

    /*public void AdvanceLevel()
    {
        var levelHandler = Object.FindObjectOfType<LevelHandler>();
        levelHandler.StartCoroutine(levelHandler.DEV_AdvanceLevel());

        var mobs = Object.FindObjectsOfType<Monster>();
        foreach (var monster in mobs)
        {
            monster.Die();
        }
    }*/

    public int LevelIndex
    {
        get => Object.FindObjectOfType<SceneLevelManager>().CurrentLevelIndex;
        set
        {
            Object.FindObjectOfType<Game>().StopGame(false);
            Object.FindObjectOfType<SceneLevelManager>().CurrentLevelIndex = value;
            Object.FindObjectOfType<Game>().StartGame();
        }
    }
}