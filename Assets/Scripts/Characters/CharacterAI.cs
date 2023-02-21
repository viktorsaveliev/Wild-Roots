using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
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
        if (_movement.GetMoveActive())
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

    private IEnumerator InfinityTimer()
    {
        if (PhotonNetwork.IsMasterClient == false) yield break;

        float timeToRepeat = 0.2f;
        yield return new WaitForSeconds(timeToRepeat);

        while (true)
        {
            if (_movement.GetMoveActive() == false || _agent.enabled == false)
            {
                yield return new WaitForSeconds(timeToRepeat);
            }

            bool isWeaponNearby = IsGetWeaponNearby(out Vector3 nearestWeapon);
            bool isNearNavMeshEdge = IsNearNavMeshEdge(2f, out Vector3 nearestEdge);
            if (isWeaponNearby && isNearNavMeshEdge && _isGoToSafeZone == false)
            {
                _isGoToSafeZone = true;
                _movement.Move(GetRandomDirectionAwayFrom(nearestWeapon, 120f));
            }
            else if (isWeaponNearby)
            {
                MoveToSafeDistanceFromWeapon(nearestWeapon);
            }
            else if (isNearNavMeshEdge && _isGoToSafeZone == false)
            {
                _isGoToSafeZone = true;
                _movement.Move(GetRandomDirectionAwayFrom(nearestEdge, 120f));
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
            if (!weapon.IsActive || weapon is RootsMine) continue;

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
        if (_movement.GetMoveActive() == false || _agent.enabled == false) return;

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
            // Move in the opposite direction to the grenade
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
            if (character == _currentCharacter || character.Move.GetMoveActive() == false) continue;

            float distance = Vector3.Distance(transform.position, character.transform.position);

            if (distance < targetDistance)
            {
                target = character;
                targetDistance = distance;
            }
        }

        if (target != null)
        {
            _myWeapon.TakeAim();
            SetTask(Tasks.AimAtTarget, Random.Range(0.5f, 2f), target.transform);
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
        if(task == Tasks.None) _myWeapon.HideAim(_currentCharacter.PhotonView.ViewID);
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

    private List<Vector3> _validDirections = new();

    private void InitializeValidDirections(Vector3 directionToAvoid, float minAngle)
    {
        _validDirections.Clear();

        Vector3[] allDirections = new Vector3[200];
        int index = 0;
        float offset = 1f / Mathf.Sqrt(2f);
        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                for (int k = -1; k <= 1; k += 2)
                {
                    allDirections[index] = new Vector3(i * offset, j * offset, k * offset);
                    index++;
                }
            }
        }

        foreach (Vector3 dir in allDirections)
        {
            if (Vector3.Angle(directionToAvoid, dir) >= minAngle)
            {
                _validDirections.Add(dir);
            }
        }
    }

    private Vector3 GetRandomDirectionAwayFrom(Vector3 directionToAvoid, float minAngle)
    {
        InitializeValidDirections(directionToAvoid, minAngle);

        int randomIndex = Random.Range(0, _validDirections.Count);
        Vector3 newDirection = _validDirections[randomIndex];
        Vector3 point = transform.position + (_safeDistance * newDirection);

        if (NavMesh.SamplePosition(point, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return point;
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

    /*private bool IsBotNearEdge(out NavMeshHit hit)
    {
        float edgeThreshold = 2f;
        float distanceThreshold = _agent.radius + edgeThreshold;

        if (NavMesh.SamplePosition(transform.position, out hit, distanceThreshold, NavMesh.AllAreas))
        {
            return hit.distance < distanceThreshold;
        }
        else
        {
            hit = new NavMeshHit();
            return true;
        }
    }

    // Move the bot to a safe location on the platform
    private void MoveToSafeLocation()
    {
        if (IsBotNearEdge(out NavMeshHit hit))
        {
            if (_antiflood >= Time.time) return;
            // Get the position of the closest point on the NavMesh to the bot's position
            Vector3 closestPoint = hit.position;

            // Calculate the direction away from the edge
            Vector3 directionAwayFromEdge = transform.position - closestPoint;
            directionAwayFromEdge.y = 0f;
            directionAwayFromEdge.Normalize();

            // Calculate the safe position as the current position plus the direction away from the edge times the safe distance
            Vector3 safePosition = transform.position + directionAwayFromEdge * _safeDistance;

            // Calculate a path to the safe position
            NavMeshPath path = new();
            if (NavMesh.CalculatePath(transform.position, safePosition, NavMesh.AllAreas, path))
            {
                // Set the NavMeshAgent to follow the path to the safe position
                _agent.SetPath(path);
            }

            SetAntiflood();
        }
        _isHaveTask = true;
        print("move to safe");
    }*/
}