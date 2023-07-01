using System.Collections;
using UnityEngine;

public class HatchHandler : MonoBehaviour
{
    [SerializeField] private Rigidbody _hatches; // []
    [SerializeField] private ParticleSystem _explode;
    [SerializeField] private AudioClip _explodeSound;

    private BoxCollider _hatchColliders = new();

    private bool _hatchIsBroken; // []
    //private int _brokenHatchCount = 0;
    private CameraShaker _shaker;

    private void Start()
    {
        //for(int i = 0; i < _hatches.Length; i++)
        //{
            _hatchColliders = _hatches.GetComponent<BoxCollider>();
        //}
        _shaker = Camera.main.GetComponent<CameraShaker>();
        StartCoroutine(BreakHatch());
    }

    private IEnumerator BreakHatch()
    {
        yield return new WaitForSeconds(120f);
        if(_hatchIsBroken)
        {
            yield break;
        }

        _hatches.isKinematic = false;
        _hatches.useGravity = true;
        _hatchIsBroken = true;

        Vector3 explosionPos = new(_hatches.transform.position.x - 0.2f, _hatches.transform.position.y - 0.5f, _hatches.transform.position.z);
        _hatches.AddExplosionForce(600f, explosionPos, 3);
        _hatchColliders.isTrigger = true;
        StartCoroutine(DeleteHatch());

        _explode.transform.position = explosionPos;
        _explode.Play();
        _shaker.CameraShake(null);
        AudioSource.PlayClipAtPoint(_explodeSound, explosionPos);

        //_brokenHatchCount++;
        StartCoroutine(BreakHatch());
    }

    private IEnumerator DeleteHatch()
    {
        yield return new WaitForSeconds(5f);
        Destroy(_hatches.gameObject);
    }
}
