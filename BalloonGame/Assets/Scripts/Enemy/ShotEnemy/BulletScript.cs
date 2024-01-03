using UnityEngine;

public class BulletScript : MonoBehaviour,IHittable
{
    [SerializeField] AudioSource deathAudio = default!;
    [SerializeField] float speed = default!;
    [SerializeField] Vector3 moveVec = default!;
    private Rigidbody rb = default;
    private PlayerGameOverEvent gameOverEvent;
    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        SoundManager.Instance.PlaySE(deathAudio, SoundSource.SE011_Hermit_Chase);
        gameOverEvent.GameOver();
        Destroy(gameObject);
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {

    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        gameOverEvent = GetComponent<PlayerGameOverEvent>();

        rb.AddForce(moveVec, ForceMode.Impulse);
    }
}
