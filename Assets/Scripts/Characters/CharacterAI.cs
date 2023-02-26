using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Weapon[] _weapons;
    private Character[] _characters;
    private const float _safeDistance = 3f;

    public bool IsHavePath;
    private bool _isGoToSafeZone;

    private Tasks _characterTask;
    private float _characterTaskTimer;
    private Transform _taskTarget;

    private IWeaponable _myWeapon;
    private IMoveable _movement;
    private Character _currentCharacter;
    private Coroutine _timer;

    private enum Tasks
    {
        None,
        AimAtTarget
    }

    private void OnEnable()
    {
        StartInfinityTimer();
    }

    private void OnDisable()
    {
        if (_timer != null)
        {
            StopCoroutine(_timer);
            _timer = null;
        }
    }

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _weapons = FindObjectsOfType<Weapon>(true);
        _characters = FindObjectsOfType<Character>();

        _myWeapon = GetComponent<IWeaponable>();
        _movement = GetComponent<IMoveable>();
        _currentCharacter = GetComponent<Character>();  
    }

    private void Update()
    {
        if(PhotonNetwork.IsMasterClient == false) return;
        if (_movement.IsCanMove())
        {
            if (_characterTask == Tasks.AimAtTarget)
            {
                float singleStep = 5 * Time.deltaTime;
                Vector3 targetDirection = _taskTarget.position - transform.position;
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirection);
            }
        }
        else
        {
            if (_characterTask != Tasks.None) SetTask(Tasks.None);
        }
    }

    private Coroutine _checkForStack;
    private void OnCollisionEnter(Collision collision)
    {
        if (_agent == null || _movement == null || PhotonNetwork.IsMasterClient == false) return;
        if(_movement.IsCanMove() && _agent.enabled && _checkForStack == null && _isGoToSafeZone == false)
        {
            _checkForStack = StartCoroutine(OnCharacterStoped(collision.transform.position, Random.Range(1, 3)));
        }
    }

    private IEnumerator OnCharacterStoped(Vector3 position, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if(Vector3.Distance(transform.position, position) < 1f)
        {
            if (_movement.IsCanMove() && _agent.enabled)
            {
                Vector3 collisionPos = GetRandomDirectionAwayFrom(position);
                _movement.Move(collisionPos);
            }
        }
        _checkForStack = null;
    }
    private IEnumerator InfinityTimer()
    {
        if (PhotonNetwork.IsMasterClient == false) yield break;

        float timeToRepeat = 0.2f;
        yield return new WaitForSeconds(timeToRepeat);

        while (true)
        {
            if (_movement.IsCanMove() == false || _agent.enabled == false)
            {
                yield return new WaitForSeconds(timeToRepeat);
            }

            bool isWeaponNearby = IsGetWeaponNearby(out Vector3 nearestWeapon);
            bool isNearNavMeshEdge = IsNearNavMeshEdge(1.5f, out Vector3 nearestEdge);
            if (isWeaponNearby && isNearNavMeshEdge && _isGoToSafeZone == false)
            {
                _isGoToSafeZone = true;
                _movement.Move(GetRandomDirectionAwayFrom(nearestWeapon));
            }
            else if (isWeaponNearby)
            {
                MoveToSafeDistanceFromWeapon(nearestWeapon);
            }
            else if (isNearNavMeshEdge && _isGoToSafeZone == false)
            {
                _isGoToSafeZone = true;
                StartCoroutine(ResetAction(Random.Range(0.5f, 1.5f)));
                _movement.Move(GetRandomDirectionAwayFrom(nearestEdge));
            }
            else
            {
                if (IsHavePath || _isGoToSafeZone)
                {
                    if (_agent.enabled && _agent.remainingDistance < 0.2f) //if ((_agent.pathEndPosition - _agent.transform.position).magnitude == 0)
                    {
                        _isGoToSafeZone = false;
                        _movement.Stop();
                    }
                }
                else if (_characterTask != Tasks.None)
                {
                    if (_characterTaskTimer < Time.time)
                    {
                        if (_myWeapon != null && _taskTarget != null)
                        {
                            float throwStrengthNormalized = 10f;
                            Vector3 direction = (_taskTarget.position - transform.position) / throwStrengthNormalized;
                            if(PhotonNetwork.OfflineMode)
                            {
                                _myWeapon.GetCurrentWeapon().Shoot(direction, transform.position, transform.rotation, true);
                            }
                            else
                            {
                                StringBus stringBus = new();
                                _myWeapon.GetCurrentWeapon().PhotonViewObject.RPC(stringBus.Shoot, RpcTarget.All, direction, transform.position, transform.rotation, true);
                            }
                        }
                        SetTask(Tasks.None);
                    }
                }
                else
                {
                    if (_myWeapon.GetCurrentWeapon() is Punch)
                    {
                        FindNearestFreeWeapon();
                    }
                    else
                    {
                        ChooseTargetAndThrowWeapon();
                    }
                }
            }
            yield return new WaitForSeconds(timeToRepeat);
        }
    }

    private bool IsGetWeaponNearby(out Vector3 nearestWeapon)
    {
        nearestWeapon = Vector3.zero;
        float minDistance = float.MaxValue;
        foreach(Weapon weapon in _weapons)
        {
            if (!weapon.IsActive || weapon is RootsMine || weapon is Punch) continue;

            float distance = Vector3.Distance(transform.position, weapon.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                nearestWeapon = weapon.transform.position;
            }
        }
        return minDistance < _safeDistance;
    }

    private void MoveToSafeDistanceFromWeapon(Vector3 weaponPos)
    {
        if (_movement.IsCanMove() == false || _agent.enabled == false) return;

        Vector3 directionToGrenade = weaponPos - transform.position;
        float distanceToGrenade = directionToGrenade.magnitude;
        Vector3 directionToMove;

        if (distanceToGrenade > _safeDistance)
        {
            return;
        }
        else if (distanceToGrenade == 0f)
        {
            directionToMove = Random.onUnitSphere;
            directionToMove.y = 0f;
        }
        else
        {
            directionToMove = -directionToGrenade.normalized;
        }

        Vector3 destination = transform.position + directionToMove * _safeDistance;
        _movement.Move(destination);
    }
    
    private void FindNearestFreeWeapon()
    {
        Weapon nearestWeapon = null;
        float nearestWeaponDistance = float.MaxValue;

        foreach (Weapon weapon in _weapons)
        {
            if (weapon.Owner != -1 || weapon.IsActive || weapon.gameObject.activeSelf == false) continue;
            float distance = Vector3.Distance(transform.position, weapon.transform.position);
            if (distance < nearestWeaponDistance)
            {
                nearestWeapon = weapon;
                nearestWeaponDistance = distance;
            }
        }

        if (nearestWeapon != null)
        {
            _movement.Move(nearestWeapon.transform.position);
        }
        else
        {
            RandomAction();
        }
    }

    private void ChooseTargetAndThrowWeapon()
    {
        Character target = null;
        float targetDistance = float.MaxValue;

        foreach (Character character in _characters)
        {
            if (character == _currentCharacter || character.gameObject.activeSelf == false) continue;

            float distance = Vector3.Distance(transform.position, character.transform.position);

            if (distance < targetDistance)
            {
                target = character;
                targetDistance = distance;
            }
        }

        if (target != null)
        {
            if (PhotonNetwork.OfflineMode)
            {
                _myWeapon.TakeAim();
            }
            else
            {
                StringBus stringBus = new();
                _myWeapon.GetPhotonView().RPC(stringBus.PlayerTakeAim, RpcTarget.All);
            }
            SetTask(Tasks.AimAtTarget, Random.Range(0.3f, 1.7f), target.transform);
        }
        else
        {
            RandomAction();
        }
    }

    private void SetTask(Tasks task, float time = 0, Transform target = null)
    {
        _characterTask = task;
        _characterTaskTimer = Time.time + time;
        _taskTarget = target;
        if (task == Tasks.None)
        {
            if (PhotonNetwork.OfflineMode)
            {
                _myWeapon.HideAim(_currentCharacter.PhotonView.ViewID);
            }
            else
            {
                StringBus stringBus = new();
                _myWeapon.GetPhotonView().RPC(stringBus.PlayerHideAim, RpcTarget.All, _currentCharacter.PhotonView.ViewID);
            }
        }
    }

    private void RandomAction()
    {
        GoToRandomPointOnMap();
    }

    private void GoToRandomPointOnMap()
    {
        if (NavMesh.SamplePosition(Vector3.zero, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            Vector3 randomDirection = Random.insideUnitSphere * 5f;
            randomDirection += hit.position;
            NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas);

            _movement.Move(hit.position);
        }
    }

    private bool IsNearNavMeshEdge(float thresholdDistance, out Vector3 edgePosition)
    {
        edgePosition = Vector3.zero;
        if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit navMeshHit, NavMesh.AllAreas))
        {
            float distance = Vector3.Distance(transform.position, navMeshHit.position);
            if (distance < thresholdDistance)
            {
                edgePosition = navMeshHit.position;
                return true;
            }
        }
        return false;
    }

    private Vector3 GetRandomDirectionAwayFrom(Vector3 directionToAvoid)
    {
        float angle = Random.Range(120f, 240f);
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 newDirection = rotation * directionToAvoid;

        if (NavMesh.SamplePosition(transform.position + newDirection, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            return (hit.position - transform.position).normalized;
        }
        else
        {
            return directionToAvoid;
        }
    }

    public void StartInfinityTimer()
    {
        if (_timer != null)
        {
            StopCoroutine(_timer);
            _timer = null;
        }
        _timer = StartCoroutine(InfinityTimer());
    }

    private IEnumerator ResetAction(float time)
    {
        yield return new WaitForSeconds(time);
        _isGoToSafeZone = false;
        _movement.Stop();
    }
}