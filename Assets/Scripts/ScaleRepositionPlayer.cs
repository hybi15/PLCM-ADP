using UnityEngine;

public class ScalePlayer : MonoBehaviour
{
    [SerializeField]
    private float Scale = 1.0f;

    public GameObject SpawnPoint;

    private void Resize(GameObject Player)
    {
        Player.transform.localScale = Vector3.one * Scale;
    }

    private void ResetPlayerPosition(GameObject Player, GameObject Position)
    {
        if (Position == null) return;
        Player.transform.position = Position.transform.position;
    }

    void Start()
    {
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        Resize(Player);
        ResetPlayerPosition(Player, SpawnPoint);
    }
}