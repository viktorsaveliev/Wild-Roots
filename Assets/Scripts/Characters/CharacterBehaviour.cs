using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterBehaviour : MonoBehaviour
{
    private const float GRENADE_DANGER_DISTANCE = 2.0f; // Distance at which bot considers grenade dangerous
    private const float SAFETY_DISTANCE = 1.0f; // Distance at which bot considers itself safe
    private const float THROW_DECISION_PROBABILITY = 0.5f; // Probability that bot will decide to throw weapon

    [SerializeField] private MeshRenderer _platform;

    private List<Character> _characters = new();
    private List<Weapon> _allWeaponsOnTheMap = new();
    
    private Character _character;
    private IMoveable _characterMoveable;
    private IWeaponable _characterWeaponable;

    private void Start()
    {
        _character = GetComponent<Character>();
        _characterMoveable = GetComponent<IMoveable>();
        _characterWeaponable = GetComponent<IWeaponable>();
        // Initialize references
        _characters.AddRange(FindObjectsOfType<Character>());
        _allWeaponsOnTheMap.AddRange(FindObjectsOfType<Weapon>());
    }

    private void Update()
    {
        if (IsInSafePosition())
        {
            if (_characterWeaponable.GetCurrentWeapon() != null)
            {
                // Decide whether to throw weapon or look for target
                if (Random.value < THROW_DECISION_PROBABILITY)
                {
                    var target = FindClosestCharacterToEdge();
                    if (target != null)
                    {
                        //var direction = (target.transform.position - transform.position).normalized;
                        //ThrowWeapon(direction);
                        print("Ћов≥ сука");
                    }
                }
                else
                {
                    var target = FindClosestCharacter();
                    if (target != null)
                    {
                        var direction = (target.transform.position - transform.position).normalized;
                        _characterMoveable.Move(direction);
                    }
                }
            }
            else
            {
                // Find nearest weapon and move towards it
                var weapon = FindNearestWeapon();
                if (weapon != null)
                {
                    var direction = (weapon.transform.position - transform.position).normalized;
                    _characterMoveable.Move(direction);
                }
            }
        }
        else
        {
            // Avoid falling off platform and evade grenades
            AvoidFalling();
            EvadeGrenades();
        }
    }

    private Character FindClosestCharacterToEdge()
    {
        // Find player closest to the edge of the platform
        Character closestChar = null;
        float closestDistance = Mathf.Infinity;
        foreach (Character player in _characters)
        {
            if (IsCharacterNearEdge(player))
            {
                var distance = Vector3.Distance(player.transform.position, PlatformEdge(player.transform.position));
                if (distance < closestDistance)
                {
                    closestChar = player;
                    closestDistance = distance;
                }
            }
        }
        return closestChar;
    }

    private Character FindClosestCharacter()
    {
        // Find player closest to the bot
        Character closestCharacter = null;
        float closestDistance = Mathf.Infinity;
        foreach (var player in _characters)
        {
            if (!IsCharacterNearEdge(player))
            {
                var distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestCharacter = player;
                    closestDistance = distance;
                }
            }
        }
        return closestCharacter;
    }

    private bool IsInSafePosition()
    {
        if (IsCharacterNearEdge(_character)) return false;

        var weapons = FindActiveWeapon();
        foreach (var weapon in weapons)
        {
            if (IsCharacterNearWeapon(weapon, _character))
            {
                return true;
            }
        }

        return false;
    }

    private List<Weapon> FindActiveWeapon()
    {
        List<Weapon> weapons = new();
        int weaponMaxCount = _allWeaponsOnTheMap.Count;

        for(int i = 0; i < weaponMaxCount; i++)
        {
            if (!_allWeaponsOnTheMap[i].gameObject.activeSelf) continue;
            weapons.Add(_allWeaponsOnTheMap[i]);
        }

        return weapons;
    }

    private Weapon FindNearestWeapon()
    {
        List<Weapon> weapons = FindActiveWeapon();
        Weapon nearestWeapon = null;
        float nearestDistance = Mathf.Infinity;
        foreach (var weapon in weapons)
        {
            var distance = Vector3.Distance(weapon.transform.position, transform.position);
            if (distance < nearestDistance)
            {
                nearestWeapon = weapon;
                nearestDistance = distance;
            }
        }
        return nearestWeapon;
    }

    private void AvoidFalling()
    {
        // Avoid falling off the platform
        var platformBounds = _platform.bounds;
        var botPosition = transform.position;
        var botRadius = 2;

        var isNearTop = botPosition.y > platformBounds.max.y - botRadius;
        var isNearBottom = botPosition.y < platformBounds.min.y + botRadius;
        var isNearLeft = botPosition.x < platformBounds.min.x + botRadius;
        var isNearRight = botPosition.x > platformBounds.max.x - botRadius;

        var horizontalDirection = isNearLeft ? Vector3.right : (isNearRight ? Vector3.left : Vector3.zero);
        var verticalDirection = isNearTop ? Vector3.down : (isNearBottom ? Vector3.up : Vector3.zero);

        var direction = (horizontalDirection + verticalDirection).normalized;
        _characterMoveable.Move(direction);
    }

    private bool IsCharacterNearWeapon(Weapon weapon, Character character)
    {
        float distance = Vector3.Distance(character.transform.position, weapon.transform.position);
        return distance < SAFETY_DISTANCE;
    }

    private void EvadeGrenades()
    {
        // Evade grenades that are thrown at the bot
        foreach (Weapon weapon in _allWeaponsOnTheMap)
        {
            if (Vector3.Distance(transform.position, weapon.transform.position) < GRENADE_DANGER_DISTANCE)
            {
                var direction = (transform.position - weapon.transform.position).normalized;
                _characterMoveable.Move(direction);
            }
        }
    }

    private bool IsCharacterNearEdge(Character character)
    {
        var playerPosition = character.transform.position;
        var platformEdge = PlatformEdge(playerPosition);
        var distance = Vector3.Distance(playerPosition, platformEdge);
        return distance < SAFETY_DISTANCE;
    }

    private Vector3 PlatformEdge(Vector3 point)
    {
        // Calculate edge of platform closest to a given point
        var platformBounds = _platform.bounds;
        var edgePoint = point;
        if (point.x < platformBounds.min.x) edgePoint.x = platformBounds.min.x;
        if (point.x > platformBounds.max.x) edgePoint.x = platformBounds.max.x;
        if (point.y < platformBounds.min.y) edgePoint.y = platformBounds.min.y;
        if (point.y > platformBounds.max.y) edgePoint.y = platformBounds.max.y;
        return edgePoint;
    }
}

