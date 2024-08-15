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

    LayerMask unitLayerMask; // For use in Physics.Raycast, Physics.OverlapSphere etc. to find other Units
    float positionErrorMargin = 10;

    Vector3 moveLocation;

    private Vector3 originalPosition; 
    private float defendTimer; 
    private const float defendTimeout = 5f;

    enum AICommandState
    {
        Idle,
        Defend,
        Move,
        Attack,
        AttackMove

    }
    //this variable will hold our current state
    AICommandState commandState = AICommandState.Idle;




    private void FixedUpdate()
    {
        DetectEnemyUnits(); // DetectEnemyUnits fills the List detectedEnemies with units within detectionRange and LOS
        switch (commandState)
        {
            case AICommandState.Idle:
                IdleBehaviour();
                break;
            case AICommandState.Defend:
                DefendBehaviour();
                break;
            case AICommandState.Move:
                MoveBehaviour();
                break;
            case AICommandState.Attack:
                AttackBehaviour();
                break;
            case AICommandState.AttackMove:
                AttackMoveBehaviour();
                break;
        }


    }



    private void Start()
    {
        self = GetComponent<Unit>();
        locomotion = GetComponent<TankLocomotion>();
        turretControl = GetComponentInChildren<TurretControl>();
        launcher = GetComponentInChildren<Launcher>();
        unitLayerMask = LayerMask.GetMask("Unit");

        //Defending
        originalPosition = transform.position;
        defendTimer = 0f;
    }

    private void IdleBehaviour()
    {

        if (detectedEnemies.Count > 0)
        {
            
            commandState = AICommandState.Defend;
        }

       
        DebugDrawRanges();
    }
    private void DefendBehaviour()
    {
        Debug.Log(gameObject.name + ": Defend State");
        if (detectedEnemies.Count > 0)
        {
            Unit enemy = detectedEnemies[0];
            MoveTo(enemy.transform.position, false); // Déplace l'unité vers l'ennemi
            Attack(enemy); 

            
            defendTimer = 0f;
        }
        else
        {
            // Si aucun ennemi n'est détecté, augmenter le timer
            defendTimer += Time.fixedDeltaTime;

            // Si le timer dépasse le délai d'attente, retourner à Idle
            if (defendTimer >= defendTimeout)
            {
                commandState = AICommandState.Idle;
                Debug.Log(gameObject.name + ": No enemies detected, switching to Idle");
                // Retourner à la position d'origine
                MoveTo(originalPosition, true);
            }
        }
    }
    private void MoveBehaviour()
    {

        Debug.Log(gameObject.name + ": Move State");
    

        bool shouldExist = Vector3.Distance(transform.position, moveLocation) < positionErrorMargin;

        if (shouldExist)
        {
            commandState = AICommandState.Idle;
        }
    }


    private void AttackBehaviour()
    {
        Debug.Log(gameObject.name + ": Attack State");
    }
    private void AttackMoveBehaviour()
    {
        Debug.Log(gameObject.name + ": AttackMove State");

        // Exit condition of the state
        if (detectedEnemies.Count > 0)
        {
            //
            Stop();
            Debug.DrawLine(transform.position, detectedEnemies[0].transform.position, Color.red);
        }
        else
        {
            if (locomotion.GetFinalTargetLocation() != moveLocation)
            {
                locomotion.MoveTo(moveLocation, false);
            }

        }

        bool shouldExist = Vector3.Distance(transform.position, moveLocation) < positionErrorMargin;

        if (shouldExist)
        {
            commandState = AICommandState.Idle;
        }




    }
    //Check if there is line of sight to the unit within the provided range
    private bool CanSee(Unit unit, float range)
    {
        Vector3 toEnemy = unit.transform.position - transform.position;
        Ray LOSRay = new Ray(transform.position, toEnemy);

        RaycastHit[] hitInfo = Physics.RaycastAll(LOSRay, range);

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
        commandState = AICommandState.AttackMove;
        moveLocation = targetLocation;

        Debug.Log(gameObject.name + ": Attack-Move to location: " + targetLocation);
        Debug.DrawLine(transform.position, targetLocation, Color.red, 1.0f, false);
        return locomotion.MoveTo(moveLocation, false);
    }

    // Move Command
    public bool MoveTo(Vector3 moveTargetPosition, bool shouldQueue)
    {
        commandState = AICommandState.Move;
        moveLocation = moveTargetPosition;

        Debug.Log(gameObject.name + ": Move to: " + moveTargetPosition);
        return locomotion.MoveTo(moveTargetPosition, shouldQueue);
    }

    // Stop self
    public void Stop()
    {
        locomotion.Stop();
    }
}
