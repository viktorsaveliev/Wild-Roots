using UnityEngine;

public class TrajectoryRenderer : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private Vector3[] _points = new Vector3[20];

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void ShowTrajectory(Vector3 origin, Vector3 speed)
    {
        _lineRenderer.positionCount = _points.Length;

        for(int i = 0; i < _points.Length; i++)
        {
            float time = i * 0.1f;
            _points[i] = origin + speed * time + time * time * Physics.gravity / 1.8f; // was / 2f

            if (_points[i].y < 0)
            {
                _lineRenderer.positionCount = i + 1;
                break;
            }
        }

        _lineRenderer.SetPositions(_points);
    }

    public void ResetTrajectory()
    {
        _lineRenderer.positionCount = 0;
    }
}
