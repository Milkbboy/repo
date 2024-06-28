using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RogueEngine
{
    //Script to manage transitions between scenes
    public class SceneNav
    {
        public static void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public static void GoTo(World world)
        {
            MapData pdata = MapData.Get(world.map_id);
            if (pdata != null)
                GoTo(pdata.map_scene);
        }

        public static void GoTo(string scene)
        {
            SceneManager.LoadScene(scene);
        }

        public static void GoToLoginMenu()
        {
            SceneManager.LoadScene("LoginMenu");
        }

        public static void GoToMenu()
        {
            SceneManager.LoadScene("Menu");
        }

        public static void GoToSetup()
        {
            SceneManager.LoadScene("GameSetup");
        }

        public static void GoToMap()
        {
            SceneManager.LoadScene("Map");
        }

        public static string GetCurrentScene()
        {
            return SceneManager.GetActiveScene().name;
        }

        public static bool DoSceneExist(string scene)
        {
            return Application.CanStreamedLevelBeLoaded(scene);
        }
    }
}