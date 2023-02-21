using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;

public class BotTest : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Weapon[] _weapons;
    private Character[] _characters;
    private const float _safeDistance = 3f;

    public bool IsHavePath;

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
        if (_timer != null)
        {
            StopCoroutine(_timer);
            _timer = null;
        }
        _timer = StartCoroutine(InfinityTimer());
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
        float timeToRepeat = 0.2f;
        yield return new WaitForSeconds(timeToRepeat);
        while (true)
        {
            if (_movement.GetMoveActive() == false || _agent.enabled == false)
            {
                yield return new WaitForSeconds(timeToRepeat);
            }
            if (IsGetWeaponNearby(out Weapon nearestWeapon))
            {
                MoveToSafeDistanceFromWeapon(nearestWeapon);
            }
            else
            {
                if (IsHavePath)
                {
                    if (_agent.enabled && _agent.remainingDistance < 0.2f) //if ((_agent.pathEndPosition - _agent.transform.position).magnitude == 0)
                    {
                        _movement.Stop();
                        IsHavePath = false;
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
                            _myWeapon.GetCurrentWeapon().Shoot(direction, transform.position, transform.rotation, true);
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

    private bool IsGetWeaponNearby(out Weapon nearestWeapon)
    {
        nearestWeapon = null;
        //float distanceFromGrenade = 5f;
        float minDistance = float.MaxValue;
        foreach(Weapon weapon in _weapons)
        {
            if (!weapon.IsActive || weapon is RootsMine) continue;

            float distance = Vector3.Distance(transform.position, weapon.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                nearestWeapon = weapon;
            }
        }
        return minDistance < _safeDistance;
    }
    private void MoveToSafeDistanceFromWeapon(Weapon weapon)
    {
        if (_movement.GetMoveActive() == false || _agent.enabled == false) return;

        Vector3 directionToGrenade = weapon.transform.position - transform.position;
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
        IsHavePath = true;
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
            IsHavePath = true;
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
            IsHavePath = true;
        }
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