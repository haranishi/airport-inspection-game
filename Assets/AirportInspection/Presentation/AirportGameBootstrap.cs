using UnityEngine;

namespace AirportInspection.Presentation
{
    public static class AirportGameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartGame()
        {
            if (Object.FindFirstObjectByType<AirportGameController>() != null)
                return;

            var root = new GameObject("Airport Inspection Game");
            root.AddComponent<AirportGameController>();
        }
    }
}
