using UnityEngine;

public class CameraMoveToPlayer : MonoBehaviour
{
    public Character Player;
    [SerializeField] private float _damping; // 0.02f && 12
    //private float[] _limitPosX = { -1.8f, 1.8f };
    //private float[] _limitPosZ = { -3.1f, 3.1f };
    //private float[] _limitCameraPosX = { -0.68f, 0.68f };
    [SerializeField] private float[] _limitCameraPosZ; // -10f, -8.16f

    //private float _currentCameraPosZ = 0;

    void FixedUpdate()
    {
        if(Player != null) //   && !IsPlayerOnEdge()
        {
            MoveCameraToPlayer();
        }
    }

    private void MoveCameraToPlayer()
    {
        Vector3 target = new(Player.transform.position.x, 10.83f, Player.transform.position.z - 8.19f); // 9.19f

        if (transform.position.z > _limitCameraPosZ[1]) target.z = _limitCameraPosZ[1] - 0.01f;
        else if (transform.position.z < _limitCameraPosZ[0]) target.z = _limitCameraPosZ[0] + 0.01f;

        /*if (Player.transform.position.z >= _limitPosZ[1] || Player.transform.position.z <= _limitPosZ[0])
        {
            if(_currentCameraPosZ == 0)
            {
                _currentCameraPosZ = transform.position.z;
            }
            target.z = _currentCameraPosZ;
        }
        else
        {
            if(_currentCameraPosZ > 0)
            {
                _currentCameraPosZ = 0;
            }
        }*/
        transform.position = Vector3.Lerp(transform.position, target, _damping);
    }
}
