using System.Collections;
using UnityEngine;
using TMPro;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager instance;

    public int collectedItems = 0;
    public TextMeshProUGUI collectibleText;
    public AnimatorOverrideController newSkinAnimator;

    private Player currentPlayer;

    private void Awake()
    {
        // makes sure there's only one instance 
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        print(collectibleText);
    }

    public void SetPlayer(Player player)
    {
        currentPlayer = player;
        collectibleText = GameObject.Find("Canvas/Orange/Count").GetComponent<TextMeshProUGUI>();
        UpdateCollectibleUI();
    }

    public void CollectItem()
    {
        collectedItems++;
        UpdateCollectibleUI();

        if (collectedItems == 5)
        {
            ChangeSkin();
            collectedItems = 0;
            UpdateCollectibleUI();
        }
    }

    public void UpdateCollectibleUI()
    {
        if (collectibleText != null)
        {
            collectibleText.text = ":" + collectedItems.ToString();
        }
    }

    private void ChangeSkin()
    {
        if (currentPlayer != null && newSkinAnimator != null)
        {
            currentPlayer.GetComponent<Animator>().runtimeAnimatorController = newSkinAnimator;
        }
    }
}
