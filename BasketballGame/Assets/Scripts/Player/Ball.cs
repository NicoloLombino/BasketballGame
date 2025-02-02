using UnityEngine;

public class Ball : MonoBehaviour
{
    AudioSource audioSource;
    MeshRenderer ballMesh;
    internal Material baseMaterial;

    [SerializeField]
    private PlayerBase playerOwner;

    [Header("Fire Bonus Components")]
    [SerializeField]
    protected GameObject fireOnBallParticles;

    [Header("Lucky Ball Components")]
    [SerializeField]
    private Material luckyBallMaterial;
    [SerializeField]
    protected GameObject luckyBallParticles;

    [Header("Ball audioclips")]
    [SerializeField]
    private AudioClip clipBounce;
    [SerializeField, Tooltip("clips to instantiate: " +
        "0 hit basket and go out, " +
        "1 enter in basket and take points," +
        "2 hit backboard and go out, " +
        "3 hit backboard and enter to take 2 points")]
    private AudioClip[] clips;

    [Header("Particles")]
    [SerializeField]
    private Transform basketPosition;
    [SerializeField, Tooltip("clips to instantiate: " +
    "0 take 2 points, " +
    "1 take 3 points without fire bonus" +
    "2 take 3 points and have fire bonus " +
    "3 take points and have lucky ball bonus")]
    private GameObject[] particlesToUse;

    [System.Serializable]
    public struct BallThrowingPosition
    {
        [SerializeField, Tooltip("Positions: " +
        "0 go out, " +
        "1 hit basket and go out," +
        "2 take 2 points," +
        "3 take 3 points," +
        "4 hit backboard and go out 1," +
        "5 hit backboars and take 2 points," +
        "6 hit backboard and go out 2")]
        public Transform[] ballPositions;
    }

    [Header("Ball Throwing Position to use according to player throw")]
    public BallThrowingPosition[] ballThrowingPositions;

    internal bool hasMakeSound;

    private int clipToPlay;
    private int pointsToGive;
    private bool isFireBonusActive;
    private bool isLuckyBallActive;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        ballMesh = GetComponent<MeshRenderer>();
        baseMaterial = ballMesh.material;
    }

    void Update()
    {
        if(!playerOwner.ignoreInputs)
        {
            if(audioSource.clip == null)
            {
                audioSource.clip = clipBounce;
            }
            if(!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            audioSource.clip = null;
        }

        if(playerOwner.ignoreInputs)
        {
            transform.eulerAngles += new Vector3(0, 150, -300) * Time.deltaTime;
        }
    }

    /// <summary>
    /// clips to instantiate: 
    /// "0 hit basket and go out" 
    /// "1 enter in basket and take points" 
    /// "2 hit backboard and go out" 
    /// "3 hit backboard and enter to take 2 points"
    /// </summary>
    public void SetAudioClipToPlayAndParticlesToUse(int clip, int points, bool hasFireBonus)
    {
        clipToPlay = clip;
        pointsToGive = points;
        isFireBonusActive = hasFireBonus;
    }

    public void HandleLuckyBallBonus(bool active)
    {
        ballMesh.material = active == true ? luckyBallMaterial : baseMaterial;
        luckyBallParticles.SetActive(active);
        isLuckyBallActive = active;
    }

    // check the particles to instantiate after a point
    private void CheckParticlesToUseOnBasket(int points, bool isFireBonusActive)
    {
        if (isLuckyBallActive)
        {
            Instantiate(particlesToUse[3], basketPosition.position, Quaternion.identity);
        }
        else
        {
            if (points == 2)
            {
                GameObject particlesOnBasket = Instantiate(particlesToUse[0], basketPosition.position, Quaternion.identity);
                // rotate the particles
                particlesOnBasket.transform.eulerAngles += Vector3.right * -90;
            }
            else if (points == 3)
            {
                if (isFireBonusActive)
                {
                    Instantiate(particlesToUse[2], basketPosition.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(particlesToUse[1], basketPosition.position, Quaternion.identity);
                }
            }
        }
        pointsToGive = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("BasketZone"))
        {
            audioSource.PlayOneShot(clips[clipToPlay]);
            hasMakeSound = true;
        }
        else if(other.CompareTag("Basket"))
        {
            CheckParticlesToUseOnBasket(pointsToGive, isFireBonusActive);
            // Avoid multiple collisions
            if (!hasMakeSound)
            {
                audioSource.PlayOneShot(clips[clipToPlay]);
                hasMakeSound = true;
            }
        }
    }
}
