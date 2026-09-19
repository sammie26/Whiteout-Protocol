using System;
using UnityEngine;

// full disclosure: in order to properly get the state machine working - i had a friend go over the code with me and help me out 
// i also use this tutorial to better grasp the state machine concept:
// https://www.youtube.com/watch?v=Vt8aZDPzRjI


public class AnimationHashes
{
    public int Sitting { get; set; }
    public int Waving { get; set; }
    public int Walking { get; set; }
    public int Turning { get; set; }
    public int PickingUp { get; set; }
    public int TriggerPickup { get; set; }

}

public abstract class State
{
    protected SurvivorBehaviour survivor;
    protected Animator animator;
    protected int? animationHash;
    public State(SurvivorBehaviour survivor, Animator animator = null)
    {
        this.survivor = survivor;

        animator = animator == null ? survivor.GetComponent<Animator>() : animator;
        this.animator = animator;
    }
    public virtual void Enter()
    {
        if (animationHash.HasValue) animator.SetBool((int)animationHash, true);
    }
    public virtual void Exit()
    {
        if (animationHash.HasValue) animator.SetBool((int)animationHash, false);
    }
    public virtual void Update()
    {
        // Debug.Log($"IsDroneDetected {survivor.IsDroneDetected}");
        // Debug.Log($"IsHealthKitDetected {survivor.IsHealthKitDetected}");
        // Debug.Log($"IsHealthKitInPickUpRange {survivor.IsHealthKitInPickUpRange}");
        // Debug.Log($"IsFacingHealthKit {survivor.IsFacingHealthKit}");
    }
    public abstract bool CanTransitionToNextState(out State nextState);

    public virtual void OnAnimatorMove()
    {
        // Basic Inhertited Movment From Animation: handling Root Motion From Animation
        survivor.transform.position += animator.deltaPosition;
        survivor.transform.rotation *= animator.deltaRotation;
    }
}

public class SittingState : State
{
    public SittingState(SurvivorBehaviour survivor, Animator animator = null) : base(survivor, animator)
    {
        animationHash = survivor.animationHashes.Sitting;
    }

    public override bool CanTransitionToNextState(out State nextState)
    {
        nextState = null;
        if (survivor.IsDroneDetected & !survivor.ObtainedHealthKit)
        {
            nextState = new WavingState(survivor, animator);
        }

        return nextState != null;
    }
}

public class WavingState : State
{
    public WavingState(SurvivorBehaviour survivor, Animator animator = null) : base(survivor, animator)
    {
        animationHash = survivor.animationHashes.Waving;
    }

    public override void Update()
    {
        base.Update();
        // looking for healthkits.
        if (survivor.HealthKit == null)
        {
            survivor.FindNearestHealthKit();
        }

    }
    public override bool CanTransitionToNextState(out State nextState)
    {
        nextState = null;
        if (!survivor.IsDroneDetected)
        {
            nextState = new SittingState(survivor);
        }
        if (survivor.IsHealthKitAvailable)
        {
            nextState = new TurningState(survivor);
        }

        return nextState != null;
    }

}

public class TurningState : State
{
    public TurningState(SurvivorBehaviour survivor, Animator animator = null) : base(survivor, animator)
    {
        animationHash = survivor.animationHashes.Turning;
    }


    public override bool CanTransitionToNextState(out State nextState)
    {
        nextState = null;

        if (survivor.IsFacingHealthKit)
        {
            nextState = new WalkingState(survivor);
        }

        return nextState != null;
    }

    public override void OnAnimatorMove()
    {
        survivor.transform.position += animator.deltaPosition;
        // transform.rotation *= animator.deltaRotation;

        // at small angle deltaRotation likely to overshoot - locking it towards target.
        if (survivor.AngleToTargetXZ(survivor.HealthKit.gameObject) <= survivor.RotationAngleLockThreshold)
        {
            Vector3 vectorToTarget = survivor.HealthKit.transform.position - survivor.transform.position;
            vectorToTarget.y = 0;
            survivor.transform.rotation = Quaternion.LookRotation(vectorToTarget);

            // preserve rootmotion rotation on x,z axis from animations.
            Quaternion animationDeltaRotationXZ = Quaternion.Euler(
                animator.deltaRotation.eulerAngles.x,
                0,
                animator.deltaRotation.eulerAngles.z
            );

            survivor.transform.rotation *= animationDeltaRotationXZ;
        }
        else
        {
            // rely on root animation to turn survivor.
            survivor.transform.rotation *= animator.deltaRotation;
        }

    }

}

public class WalkingState : State
{
    public WalkingState(SurvivorBehaviour survivor, Animator animator = null) : base(survivor, animator)
    {
        animationHash = survivor.animationHashes.Walking;
    }

    public override void OnAnimatorMove()
    {
        Vector3 position = survivor.transform.position;
        position += animator.deltaPosition;
        position.y = Terrain.activeTerrain.SampleHeight(survivor.transform.position);
        survivor.transform.position = position;


        Vector3 vectorToTarget = survivor.HealthKit.transform.position - survivor.transform.position;
        vectorToTarget.y = 0;
        survivor.transform.rotation = Quaternion.LookRotation(vectorToTarget);
    }

    public override bool CanTransitionToNextState(out State nextState)
    {
        nextState = null;
        if (survivor.IsHealthKitInPickUpRange) nextState = new PickingUpState(survivor);
        return nextState != null;
    }

}

public class PickingUpState : State
{
    public PickingUpState(SurvivorBehaviour survivor, Animator animator = null) : base(survivor, animator)
    {
        animationHash = survivor.animationHashes.Walking;
    }
    public override void Enter()
    {
        base.Enter();
        animator.SetTrigger(survivor.animationHashes.TriggerPickup);
    }

    public override bool CanTransitionToNextState(out State nextState)
    {
        nextState = null;
        if (survivor.ObtainedHealthKit)
        {
            nextState = new SittingState(survivor);
        }

        return nextState != null;
    }

}


public class SurvivorBehaviour : MonoBehaviour
{

    [SerializeField] private FirstAidKit healthKit = null;
    public FirstAidKit HealthKit => healthKit;
    [SerializeField] public GameObject drone;
    public GameObject Drone => drone;

    [SerializeField] private float droneDetectionRange = 25f;
    [SerializeField] private float healthKitDetectionRange = 10f;
    [SerializeField] private readonly float healthKitPickUpRange = 0.565f; // tuned for animation.
    [SerializeField] private float rotationAngleLockThreshold = 3f; //rotation variance for locking.
    public float RotationAngleLockThreshold => rotationAngleLockThreshold;

    [SerializeField] private bool obtainedHealthKit = false;
    public bool ObtainedHealthKit => obtainedHealthKit;
    private GameObject holdingHealthKitVisual;



    // State Transition Conditonals

    public bool IsDroneDetected => DistanceToTargetXZ(drone) <= droneDetectionRange;
    public bool IsHealthKitAvailable
    {
        get
        {
            if (healthKit == null) return false;
            return healthKit.IsReadyForPickUp;
        }
    }
    public bool IsHealthKitInPickUpRange => DistanceToTargetXZ(healthKit.gameObject) <= healthKitPickUpRange;
    public bool IsFacingHealthKit => AngleToTargetXZ(healthKit.gameObject) <= 0.1f;



    private State currentState;
    private Animator animator;
    public readonly AnimationHashes animationHashes = new AnimationHashes();



    private void Awake()
    {
        animator = GetComponent<Animator>();

        Debug.Log($"animator: {animator}");
        animationHashes.Sitting = Animator.StringToHash("Sitting");
        animationHashes.Waving = Animator.StringToHash("Waving");
        animationHashes.Turning = Animator.StringToHash("Turning");
        animationHashes.Walking = Animator.StringToHash("Walking");
        animationHashes.PickingUp = Animator.StringToHash("PickingUp");
        animationHashes.TriggerPickup = Animator.StringToHash("TriggerPickup");

        Debug.Log($"animationHashes.Sitting: {animationHashes.Sitting}");
        Debug.Log($"animationHashes.Waving: {animationHashes.Waving}");
        Debug.Log($"animationHashes.Turning: {animationHashes.Turning}");
        Debug.Log($"animationHashes.Walking: {animationHashes.Walking}");
        Debug.Log($"animationHashes.PickingUp: {animationHashes.PickingUp}");
        Debug.Log($"animationHashes.TriggerPickup: {animationHashes.TriggerPickup}");

    }

    private void Start()
    {
        currentState = new SittingState(this, animator);

        holdingHealthKitVisual = GetComponentInChildren<FirstAidKit>().gameObject;
        if (holdingHealthKitVisual == null)
            throw new MissingMemberException("Must Nest First AidKit");
        holdingHealthKitVisual.SetActive(false);

    }
    private void Update()
    {
        // next state ready (conditions for transition passed)
        if (currentState.CanTransitionToNextState(out State nextState))
        {
            // clean up this state
            currentState.Exit();

            // assign next state to this state and enter
            currentState = nextState;
            currentState.Enter();

            // return early to skip Update() incase next state transtion is immediate.
            return;
        }
        currentState.Update();
    }

    private void OnAnimatorMove()
    {
        currentState.OnAnimatorMove();
    }

    private void AnimationEventHandlerPickUpHealthKit(string _)
    {
        // Make Game Object Disappear.
        // Set HealthKitObtained to True
        // Add GameObject to Correct Model Location
        Debug.Log("PickUp Event Has Been Fired");

        // set flag as true
        obtainedHealthKit = true;

        // Make nested prefab visible.
        holdingHealthKitVisual.SetActive(true);

        // Destroy HealthKit on ground.
        Debug.Log($"DestroyingKit: {healthKit.gameObject}");
        Destroy(healthKit.gameObject);

    }

    public float DistanceToTargetXZ(GameObject target)
    {
        // Calculate the distance to the target - ignoring y component.
        Vector3 delta = transform.position - target.transform.position;
        delta.y = 0;
        return delta.magnitude;
    }

    public float AngleToTargetXZ(GameObject target)
    {
        // Calculates the angle to face object on the XZ Plane - ignoring y component.
        Vector3 vectorToTarget = target.transform.position - transform.position;
        vectorToTarget.y = 0;

        Vector3 characterForward = transform.forward;
        characterForward.y = 0;

        // return the angle between these forward vector and vector to target
        return Vector3.Angle(characterForward, vectorToTarget);
    }


    public void FindNearestHealthKit()
    {

        int maxColliders = 100;
        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, healthKitDetectionRange, hitColliders);

        for (int i = 0; i < numColliders; i++)
        {
            FirstAidKit hitCollider = hitColliders[i].GetComponent<FirstAidKit>();
            if (hitCollider != null && hitCollider.IsReadyForPickUp)
            {
                Debug.Log("HealthKit Found");
                healthKit = hitCollider;
            }
            hitColliders[i] = null;
        }

    }

    private void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position, showing the checkRadius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, healthKitDetectionRange);
    }
}

