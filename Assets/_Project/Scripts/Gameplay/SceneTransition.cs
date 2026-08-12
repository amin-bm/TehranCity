using UnityEngine.SceneManagement;

public static class SceneTransition
{
    public static string PendingSpawn = "";

    public static void Load(string sceneName, string spawnPointName)
    {
        PendingSpawn = spawnPointName ?? "";
        SceneManager.LoadScene(sceneName);
    }
}