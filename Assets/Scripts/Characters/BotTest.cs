using UnityEngine;
using UnityEngine.AI;

public class BotTest : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Weapon[] _weapons;
    private Character[] _characters;
    private Weapon _currentWeapon;
    private const float _safeDistance = 1f;
    private float _antiflood;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _weapons = FindObjectsOfType<Weapon>(true);
        _characters = FindObjectsOfType<Character>();
    }

    void Update()
    {
        // Determine the state of the bot
        /*if (IsBotNearEdge(out NavMeshHit hit))
        {
            MoveToSafeLocation();
        }
        else*/ if (IsGetWeaponNearby(out Weapon nearestWeapon))
        {
            MoveToSafeDistanceFromWeapon(nearestWeapon);
        }
        else
        {
            // If the player is in a safe position, look for the nearest free weapon and move towards it
            if (_currentWeapon == null)
            {
                FindNearestFreeWeapon();
            }
            else
            {
                ChooseTargetAndThrowWeapon();
            }
        }
    }

    // Check if the bot is near the edge of the platform
    private bool IsBotNearEdge(out NavMeshHit hit)
    {
        float edgeThreshold = 2f;
        float distanceThreshold = _agent.radius + edgeThreshold;

        if (NavMesh.SamplePosition(transform.position, out hit, distanceThreshold, NavMesh.AllAreas))
        {
            // Check if the bot's position is too close to the edge of the NavMesh area
            return hit.distance < distanceThreshold;
        }
        else
        {
            // The bot is not on the NavMesh, so it must be considered near the edge
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

            _antiflood = Time.time + 1;
        }
        print("move to safe");
    }

    // Check if there is a grenade nearby
    private bool IsGetWeaponNearby(out Weapon nearestWeapon)
    {
        nearestWeapon = null;
        float distanceFromGrenade = 3f; // Adjust this value to set the distance from the grenade
        float minDistance = float.MaxValue;
        foreach(Weapon weapon in _weapons)
        {
            float distance = Vector3.Distance(transform.position, weapon.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                nearestWeapon = weapon;
            }
        }
        print("Get Nearby Weapon");
        return minDistance < distanceFromGrenade;
    }

    // Move the bot to a safe distance from the grenade
    private void MoveToSafeDistanceFromWeapon(Weapon weapon)
    {
        Vector3 directionToGrenade = weapon.transform.position - transform.position;
        float distanceToGrenade = directionToGrenade.magnitude;
        Vector3 directionToMove;

        if (distanceToGrenade > _safeDistance)
        {
            // The bot is already at a safe distance from the grenade
            return;
        }
        else if (distanceToGrenade == 0f)
        {
            // The bot is on top of the grenade, so move away in a random direction
            directionToMove = Random.onUnitSphere;
            directionToMove.y = 0f;
        }
        else
        {
            // Move in the opposite direction to the grenade
            directionToMove = -directionToGrenade.normalized;
        }

        Vector3 destination = transform.position + directionToMove * _safeDistance;

        _agent.SetDestination(destination);
        print("MoveToSavedistance from weapon");
    }

    private void FindNearestFreeWeapon()
    {
        Weapon nearestWeapon = null;
        float nearestWeaponDistance = float.MaxValue;

        foreach (Weapon weapon in _weapons)
        {
            if (weapon.Owner == -1)
            {
                float distance = Vector3.Distance(transform.position, weapon.transform.position);
                if (distance < nearestWeaponDistance)
                {
                    nearestWeapon = weapon;
                    nearestWeaponDistance = distance;
                }
            }
        }

        if (nearestWeapon != null)
        {
            _agent.SetDestination(nearestWeapon.transform.position);
        }
        print("find nearest weapon");
    }

    // Choose a target and throw the weapon
    private void ChooseTargetAndThrowWeapon()
    {
        float throwDistance = 10f; // Adjust this value to set the throw distance

        Character target = null;
        float targetDistance = float.MaxValue;

        // Choose a target randomly: either the one next to the bot, or the one who stands near the edge of the platform
        foreach (Character character in _characters)
        {
            if (character != gameObject)
            {
                float distance = Vector3.Distance(transform.position, character.transform.position);

                if (distance < targetDistance && character.transform.position.y < 1.0f)
                {
                    target = character;
                    targetDistance = distance;
                }
            }
        }

        if (target != null)
        {
            Vector3 throwDirection = (target.transform.position - transform.position).normalized;
            Vector3 throwPosition = transform.position + throwDirection * throwDistance;

            // Throw the weapon at the target
            
            _currentWeapon.Shoot(throwDirection, throwPosition, Quaternion.identity);
            _currentWeapon = null;
            print("Throw weapon");
        }
    }
}