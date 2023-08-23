using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    AudioSource audioSource;

    [SerializeField]
    private PlayerBase playerOwner;

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
    "2 take 3 points and have fire bonus ")]
    private GameObject[] particlesToUse;

    [Header("Ball Materials")]
    [SerializeField]
    private Material[] materials;

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

    //[Header("Ball Throwing Position to use according to player throw")]
    //[SerializeField, Tooltip("Positions: " +
    //"0 go out, " +
    //"1 hit basket and go out" +
    //"2 take 2 points" +
    //"3 take 3 points ")]

    internal bool hasMakeSound;

    private int clipToPlay;
    private int pointsToGive;
    private bool isFireBonusActive;



    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (playerOwner.gameObject.GetComponent<Player>() != null)
        {
            gameObject.GetComponent<MeshRenderer>().material = materials[PlayerPrefs.GetInt("BallMaterialIndex")];
        }
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

    private void CheckParticlesToUseOnBasket(int points, bool fireActive)
    {
        GameObject particlesOnBasket = new GameObject();
        if (points == 2)
        {
            particlesOnBasket = Instantiate(particlesToUse[0], basketPosition.position, Quaternion.identity);
            // rotate the particles
            particlesOnBasket.transform.eulerAngles += Vector3.right * -90;
        }
        else if (points == 3)
        {
            if (fireActive)
            {
                particlesOnBasket = Instantiate(particlesToUse[2], basketPosition.position, Quaternion.identity);
            }
            else
            {
                particlesOnBasket = Instantiate(particlesToUse[1], basketPosition.position, Quaternion.identity);
            }
        }
        pointsToGive = 0;
        Destroy(particlesOnBasket, 1);
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
            if(!hasMakeSound)
            {
                audioSource.PlayOneShot(clips[clipToPlay]);
                hasMakeSound = true;
            }
        }
    }


}
