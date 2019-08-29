using UnityEngine;

public class WalkingMan : MonoBehaviour {
    Animator animator;

    public SpawningSoundObject spawningSoundObject;
    public Person person;
    float targetPositionX;

    float MOVEMENT_PER_FRAME = 0.1f;
    float MAX_TIME_TO_WAIT_TO_CATCHUP = 2f;

    float timeLeftToMoveTowardsBag = 0f;
    bool isWalking = false;

    void Awake() {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start() {
    }

    void walk () {
        animator.StartPlayback();
    }

    void stop () {
        animator.StopPlayback();
    }

    void Update() {
        if (timeLeftToMoveTowardsBag > 0f) {
            timeLeftToMoveTowardsBag -= Time.deltaTime;
            if (timeLeftToMoveTowardsBag <= 0f) {
                timeLeftToMoveTowardsBag = 0f;
                isWalking = true;
                animator.SetBool("idling", false);
            }
        }

        if (isWalking) {
            transform.position = new Vector3(transform.position.x + MOVEMENT_PER_FRAME, transform.position.y, transform.position.z);

            if (transform.position.x >= targetPositionX) {
                isWalking = false;
                animator.SetBool("idling", true);
                if (person == null) {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    public void reportPositionX(float positionX) {
        targetPositionX = positionX;

        if (targetPositionX > transform.position.x && timeLeftToMoveTowardsBag == 0f && !isWalking) {
            timeLeftToMoveTowardsBag = Misc.randomRange(MAX_TIME_TO_WAIT_TO_CATCHUP - 0.5f, MAX_TIME_TO_WAIT_TO_CATCHUP + 0.5f);
        }
    }
}
