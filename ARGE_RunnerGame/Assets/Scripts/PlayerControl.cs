using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using TMPro;
using DG.Tweening;

public class PlayerControl : MonoBehaviour
{
    Touch touch;
    [SerializeField] float horizontalSpeed=0.01f;
    [SerializeField] float speed =0f;
    float newPositionZ; 
    float newPositionX;
    public TMP_Text collectibleText; 
    public TMP_Text toyText;
    public GameObject shopPanel;
    public GameObject shopPlatform;
    public Camera cam; 
    public GameObject sackJellyPrefab;
    public List<GameObject> sackJellies;
    private int jellyIndex = 0;
    public Transform pool;
    public bool inShop = false;
    private float shopLowerBound;

    float jellyCount, toyCount = 0;

    public Animator capacityFullAnimator;

    public int capacity;
    public float toyCost;
    float minSpeed = 7f;
    public int jellyworth;

    public Vector3 GetSackTransform(float offset)
    {
        Vector3 parentTransform = gameObject.transform.position;
        return new Vector3(parentTransform.x, parentTransform.y + (offset / 2), parentTransform.z - 0.85f);
    }

    public void PutInSack()
    {
        GameObject jelly = sackJellies[jellyIndex];
        jelly.SetActive(true);
        jelly.transform.SetParent(gameObject.transform.GetChild(1));
        jelly.transform.position = GetSackTransform(jellyIndex);
        jellyIndex++;
    }

    public void RemoveFromSack()
    {
        GameObject jelly = sackJellies[--jellyIndex];
        jelly.SetActive(false);
        jelly.transform.SetParent(pool);
    }

    public void PutInPool()
    {
        GameObject jelly = Instantiate(sackJellyPrefab);
        jelly.transform.SetParent(pool);
        jelly.SetActive(false);
        sackJellies.Add(jelly);
    }

    public void MakeToys()
    {
        if (jellyCount >= 1)
        {
            toyCount += 2 * jellyCount;
            for(int i = 0; i < jellyCount; i++)
            {
                RemoveFromSack();
            }
            jellyCount = 0;
            collectibleText.text = jellyCount.ToString();
            toyText.text = toyCount.ToString();
        }
    }

    

    public void ExitShoppingPhase()
    {
        speed = 7f;
        inShop = false;
        StartCoroutine(Zoom(-1,-0.4f));
        cam.transform.position = new Vector3(gameObject.transform.position.x, cam.transform.position.y, gameObject.transform.position.z-10);
        cam.transform.SetParent(gameObject.transform);

    }

    public void UpgradeTrait(int trait)  // 0 for jelly, 1 for capacity 
    {

        if (toyCount >= upgradeCosts[trait])
        {
            toyCount -= upgradeCosts[trait];
            string traitString = string.Empty;
            toyText.text = toyCount.ToString();
            switch (trait)
            {
                case 0:
                    jellyworth += upgradeImpacts[trait];
                    upgradeCosts[trait] += 80;
                    traitString = "Jelly";
                    break;
                case 1:
                    capacity += upgradeImpacts[trait];
                    for(int i = 0; i < upgradeImpacts[trait]; i++)
                    {
                        PutInPool();
                    }
                    upgradeCosts[trait]++;
                    traitString = "Capacity";
                    break;
            }
            GameObject.FindWithTag(traitString + "Text").GetComponent<TMP_Text>().text = string.Format("{0} +1 ({1} Toys)", traitString, upgradeCosts[trait]);
        }
    }

    void EnterShoppingPhase()
    {
        shopLowerBound = gameObject.transform.position.z + 1;
        speed = 0;
        inShop = true;
        gameObject.transform.Translate(0, 0, 2);
        StartCoroutine(Zoom(1,0.4f));
        cam.transform.parent = null;
    }

    void EnterShop()
    {
        Time.timeScale = 0f;
        shopPanel.SetActive(true);
    }
    public void ExitShop()
    {
        Time.timeScale = 1;
        shopPanel.SetActive(false);

    }

    IEnumerator Zoom(float rotate, float translate)
    {
        int timer = 0;
        while (timer++ < 30)
        {
            cam.gameObject.transform.Rotate(rotate, 0, 0);
            cam.gameObject.transform.Translate(0, translate, 0);
            yield return new WaitForSeconds(0.015f);
        }
    }
    void IncreaseJellyBy(int by, GameObject jelly)
    {
        if (jellyCount < capacity)
        {
            jellyCount += by;
            speed += 2.5f;
            collectibleText.text = jellyCount.ToString();
            jelly.transform.DOMove(cam.ScreenToWorldPoint(new Vector3(collectibleText.transform.position.x, collectibleText.transform.position.y, collectibleText.transform.position.z + speed)), 0.5f);
            PutInSack();
            //Destroy(jelly);
        }
        else    //avoid jelly
        {
            jelly.GetComponent<BoxCollider>().enabled = false;
            capacityFullAnimator.SetTrigger("PlayFullCapacity");
        }
    }

    void DecreaseJellyBy(int by)
    {
        jellyCount -= by;
        collectibleText.text = jellyCount.ToString();
        RemoveFromSack();
    }

    void Crash()
    {
        if (jellyCount == 0)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }
        else
        {
            speed = minSpeed;
            DecreaseJellyBy(1);
        }
    }

    readonly int[] upgradeCosts = { 80, 10 }; //0:jelly, 1:capacity
    readonly int[] upgradeImpacts = { 1, 1 };

    // Start is called before the first frame update
    void Start()
    {
        collectibleText.text = jellyCount.ToString();
        toyText.text = toyCount.ToString();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(0, 0, speed * Time.deltaTime);
        if (Input.touchCount > 0) { StartMoving(); }
    }

    void StartMoving()
    {
        touch = Input.GetTouch(0);

        if (!inShop)
        {
            if (speed == 0) { speed = 7f; } //hiz tanimlamasi bu sekilde degistirildi cunku jelly toplandiginda da hizin arttirilmasi gerekiyor.
            
            if (touch.phase == TouchPhase.Moved)
            {
                newPositionX = Mathf.Clamp(transform.position.x + touch.deltaPosition.x * horizontalSpeed, transform.localScale.x / 2f - 5, 5 - transform.localScale.x / 2f);

                transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);
            }
        }
        else
        {
            if (touch.phase == TouchPhase.Moved)
            {
                newPositionX = Mathf.Clamp(transform.position.x + touch.deltaPosition.x * horizontalSpeed, transform.localScale.x / 2f - 8, 8 - transform.localScale.x / 2f);

                newPositionZ =  Mathf.Clamp(transform.position.z + touch.deltaPosition.y * horizontalSpeed, shopLowerBound, shopLowerBound + 13.5f);

                transform.position = new Vector3(newPositionX, transform.position.y, newPositionZ);
            }
        }
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        switch(collision.gameObject.tag)
        {
            case "Jelly":               
                IncreaseJellyBy(jellyworth, collision.gameObject); 
                break;
            case "Obstacle": 
                Crash();
                Destroy(collision.gameObject);
                break;
            case "EnteringPlatform":
                EnterShoppingPhase();
                collision.collider.GetComponent<MeshCollider>().enabled = false;
                break;
            case "ShopPlatform":
                EnterShop();
                break;
            case "ExitingPlatform":
                ExitShoppingPhase();
                break;
        }
    }
    
}
