//This is the only MonoBehaviour-derived class you may modify for this challenge
//By Joss Moo-Young 
using System.Collections.Generic;
using UnityEngine;

public class AIUnitBehaviour : MonoBehaviour, IAttackMoveCommandable
{
    [SerializeField]
    private float detectionRange = 80; // Range to spot enemies
    [SerializeField]
    private float attackRange = 60; // Range to attack enemies

    //Components attached to this gameobject, that this AIUnitBehaviour controls
    Unit self; // Responsible for teams, hit points
    TankLocomotion locomotion; // Responsible for movement, pathfinding
    TurretControl turretControl; // Responsible for aiming
    Launcher launcher; // Responsible for shooting projectiles

    List<Unit> detectedEnemies = new List<Unit>(); // List of enemies currently known by this Unit on by its own vision in DetectEnemyUnits()
    Unit target = null; // What enemy is this AI targeting? If no target, then null

    LayerMask unitLayerMask ; // For use in Physics.Raycast, Physics.OverlapSphere etc. to find other Units

    private void Start()
    {
        self = GetComponent<Unit>();
        locomotion = GetComponent<TankLocomotion>();
        turretControl = GetComponentInChildren<TurretControl>();
        launcher = GetComponentInChildren<Launcher>();
        unitLayerMask = LayerMask.GetMask("Unit");
    }

    private void FixedUpdate()
    {
        DetectEnemyUnits(); // DetectEnemyUnits fills the List detectedEnemies with units within detectionRange and LOS
        IdleBehaviour();
    }

    private void IdleBehaviour()
    {
        if (target == null)
        {
            DebugDrawRanges(); // Debug line drawing
        }
    }
  
    //Check if there is line of sight to the unit within the provided range
    private bool CanSee(Unit unit, float range)
    {
        Vector3 toEnemy = unit.transform.position - transform.position;
        Ray LOSRay = new Ray(transform.position, toEnemy);

        RaycastHit[] hitInfo = Physics.RaycastAll(LOSRay, range, unitLayerMask);

        if (hitInfo.Length > 0)
        {
            System.Array.Sort(hitInfo, (a, b) => (a.distance.CompareTo(b.distance)));

            if (hitInfo[0].collider == unit.collider)
            {
                return true; // if hit enemy
            }

            if (hitInfo[0].collider == self.collider && hitInfo.Length > 1) // if hit self but there is another...
            {
                if (hitInfo[1].collider == unit.collider) // check second hit
                {
                    return true;
                }
            }
        }
        return false;
    }

    //Find enemies
    private void DetectEnemyUnits()
    {
        detectedEnemies.Clear();
        Team myTeam = GetComponent<Unit>().Team;

        //Assign new variable the return value from OverlapSphere
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, unitLayerMask);

        foreach (Collider collider in colliders)
        {
            //Find out if it is a Unit
            Unit contact = collider.GetComponent<Unit>();
            if (contact == null) continue; // skip this collider; it is not a Unit.

            bool isMyTeam = (contact.Team == myTeam);

            if (isMyTeam) continue; // skip this Unit; it is not an enemy.

            if (CanSee(contact, detectionRange))
            {
                //Has LOS to the enemy
                detectedEnemies.Add(contact);
                //Debug.DrawLine(transform.position, contact.transform.position, Color.yellow);
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void DebugDrawRanges()
    {
        //Current target
        if (target)
        {
            Debug.DrawLine(transform.position, target.transform.position, Color.red);
        }

        //detectionRange
        DebugDrawing.DrawCircleDotted(transform.position, Quaternion.Euler(90, 0, 0), detectionRange, 32, 2, 16, Color.yellow, Time.fixedDeltaTime, false);
    }

    // Attack Command
    public bool Attack(Unit unit)
    {
        //TODO
        Debug.Log(gameObject.name + ": Attacking " + unit.gameObject.name);
        Debug.DrawLine(transform.position, unit.transform.position, Color.red, 1.0f, false);
        return true;
    }

    // Attack-Move command
    public bool AttackMove(Vector3 targetLocation)
    {
        //TODO
        Debug.Log(gameObject.name + ": Attack-Move to location: " + targetLocation);
        Debug.DrawLine(transform.position, targetLocation, Color.red, 1.0f, false);
        return true;
    }

    // Move Command
    public bool MoveTo(Vector3 moveTargetPosition, bool shouldQueue)
    {
        //TODO: This should cancel other commands
        Debug.Log(gameObject.name + ": Move to: " + moveTargetPosition);
        return locomotion.MoveTo(moveTargetPosition, shouldQueue);
    }

    // Stop self
    public void Stop()
    {
        locomotion.Stop();
    }
}
