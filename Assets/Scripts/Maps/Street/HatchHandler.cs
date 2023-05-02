using System.Collections;
using UnityEngine;

public class HatchHandler : MonoBehaviour
{
    [SerializeField] private Rigidbody[] _hatches;
    [SerializeField] private ParticleSystem _explode;
    [SerializeField] private AudioClip _explodeSound;

    private BoxCollider[] _hatchColliders = new BoxCollider[4];

    private bool[] _hatchIsBroken = new bool[4];
    private int _brokenHatchCount = 0;
    private CameraShaker _shaker;

    private void Start()
    {
        for(int i = 0; i < _hatches.Length; i++)
        {
            _hatchColliders[i] = _hatches[i].GetComponent<BoxCollider>();
        }
        _shaker = Camera.main.GetComponent<CameraShaker>();
        StartCoroutine(BreakHatch());
    }

    private IEnumerator BreakHatch()
    {
        yield return new WaitForSeconds(30f);
        if(_brokenHatchCount >= _hatches.Length || _hatchIsBroken[_brokenHatchCount])
        {
            yield break;
        }

        _hatches[_brokenHatchCount].isKinematic = false;
        _hatches[_brokenHatchCount].useGravity = true;
        _hatchIsBroken[_brokenHatchCount] = true;

        Vector3 explosionPos = new(_hatches[_brokenHatchCount].transform.position.x - 0.2f, _hatches[_brokenHatchCount].transform.position.y - 0.5f, _hatches[_brokenHatchCount].transform.position.z);
        _hatches[_brokenHatchCount].AddExplosionForce(600f, explosionPos, 3);
        _hatchColliders[_brokenHatchCount].isTrigger = true;
        StartCoroutine(DeleteHatch(_brokenHatchCount));

        _explode.transform.position = explosionPos;
        _explode.Play();
        _shaker.CameraShake(null);
        AudioSource.PlayClipAtPoint(_explodeSound, explosionPos);

        _brokenHatchCount++;
        StartCoroutine(BreakHatch());
    }

    private IEnumerator DeleteHatch(int hatchID)
    {
        yield return new WaitForSeconds(5f);
        Destroy(_hatches[hatchID].gameObject);
    }
}
