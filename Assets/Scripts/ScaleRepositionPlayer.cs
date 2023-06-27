using UnityEngine;

public class ScaleRepositionPlayer : MonoBehaviour
{
    [SerializeField]
    private float Scale = 1.0f;

    public GameObject SpawnPoint;
    private Transform Player;
    private Transform Camera;

    private void Resize(GameObject Player)
    {
        Player.transform.localScale = Vector3.one * Scale;
    }

    private void ResetPlayerPosition()
    {
        if (Player == null) return;
        // Reset position
        Vector3 offset = Camera.position - Player.position;
        offset.y = 0;
        Player.position = SpawnPoint.transform.position - offset;

        // Reset rotation
        Vector3 SpawnPointForward = SpawnPoint.transform.forward;
        SpawnPointForward.y = 0;
        Vector3 CameraForward = Camera.forward;
        CameraForward.y = 0;

        float angle = Vector3.SignedAngle(CameraForward, SpawnPointForward, Vector3.up);

        Player.RotateAround(Camera.position, Vector3.up, angle);
    }

    void Start()
    {
        GameObject PlayerObj = GameObject.FindGameObjectWithTag("Player");
        Player = PlayerObj.transform;
        GameObject CameraObj = GameObject.FindGameObjectsWithTag("MainCamera")[0];
        Camera = CameraObj.transform;
        Resize(PlayerObj);
        ResetPlayerPosition();
    }
}
