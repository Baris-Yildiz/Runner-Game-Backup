using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using TMPro;

public class PlayerControl : MonoBehaviour
{
    Touch touch;
    [SerializeField] float horizontalSpeed=0.01f;
    [SerializeField] float speed =0f;
    float newPositionX;
    public TMP_Text collectibleText;
    public TMP_Text toyText;
    public GameObject shopPanel;

    float jellyCount, toyCount = 0;

    public Animator capacityFullAnimator;

    public int capacity;
    public float toyCost;
    float minSpeed = 7f;
    public int jellyworth;

    public enum Traits
    {
        Speed = 0,
        Capacity = 1,
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
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + speed * Time.deltaTime);
        if (Input.touchCount > 0) { StartMoving(); }
    }

    void StartMoving()
    {
        if (speed == 0) { speed = 7f; } //hiz tanimlamasi bu sekilde degistirildi cunku jelly toplandiginda da hizin arttirilmasi gerekiyor.

        touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Moved)
        {
            newPositionX = transform.position.x + touch.deltaPosition.x * horizontalSpeed;

            if (newPositionX < (0.5f + transform.localScale.x / 2f - 5.5f)) { newPositionX = 0.5f + transform.localScale.x / 2f - 5.5f; }
            if (newPositionX > (5.5f - 0.5f - transform.localScale.x / 2f)) { newPositionX = 5.5f - 0.5f - transform.localScale.x / 2f; }
            transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        switch(collision.gameObject.tag)    //obstacle ve collectible lardan scale modifiye ozelligi kaldirildi
        {
            case "Jelly":               
                IncreaseJellyBy(jellyworth, collision.gameObject); 
                break;
            case "Obstacle": 
                Crash();
                Destroy(collision.gameObject);
                break;
            case "Shop":
                EnterShoppingPhase();
                break;
        }
    }

    public void MakeToys()
    {
        if(jellyCount >= 1)
        {
            toyCount += 2 * jellyCount;
            jellyCount = 0;
            collectibleText.text = jellyCount.ToString();
            toyText.text = toyCount.ToString();
        }
    }

    public void ExitShop()
    {
        Time.timeScale = 1;
        shopPanel.SetActive(false);
        speed = 7f;
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
                    upgradeCosts[trait]+= 80;
                    traitString = "Jelly";
                    break;
                case 1:
                    capacity += upgradeImpacts[trait];
                    upgradeCosts[trait]++;
                    traitString = "Capacity";
                    break;
            }
            GameObject.FindWithTag(traitString+"Text").GetComponent<TMP_Text>().text = string.Format("{0} +1 ({1} Toys)", traitString, upgradeCosts[trait]);
        }
    }

    void EnterShoppingPhase()
    {
        Time.timeScale = 0;
        shopPanel.SetActive(true);
    }

    void IncreaseJellyBy(int by, GameObject jelly)
    {
        if (jellyCount < capacity)
        {
            jellyCount += by;
            speed += 2.5f;
            collectibleText.text = jellyCount.ToString();
            Destroy(jelly);
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
    }

    void Crash()
    {
        if(jellyCount == 0)
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

}
