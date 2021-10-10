using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStaticPrefabs : MonoBehaviour
{
    public GameObject _AbilityBuffPre;

    public static GameObject AbilityBuffPre;
    // Start is called before the first frame update
    void Start()
    {
        AbilityBuffPre = _AbilityBuffPre;
        if (!NetworkClient.prefabs.ContainsValue(AbilityBuffPre))
        {
            NetworkClient.RegisterPrefab(AbilityBuffPre);
        }
    }

}
