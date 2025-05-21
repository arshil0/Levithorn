using System.Collections;
using UnityEngine;
using TMPro;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager instance;
    public static bool[] collectedOrangesIds;

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
        collectedOrangesIds = new bool[GameObject.Find("Oranges").transform.childCount];

        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayer(Player player)
    {
        currentPlayer = player;
        collectibleText = GameObject.Find("Canvas/Orange/Count").GetComponent<TextMeshProUGUI>();
        UpdateCollectibleUI();
    }

    public void UpdateExistingOranges()
    {
        print("about to delete");
        GameObject oranges = GameObject.Find("Oranges");
        for (int i = 0; i < collectedOrangesIds.Length; i++)
        {
            if (collectedOrangesIds[i] == true)
            {
                Destroy(oranges.transform.Find(i.ToString()).gameObject);
            }
        }
    }

    public void CollectItem(string id)
    {
        collectedItems++;
        UpdateCollectibleUI();
        collectedOrangesIds[int.Parse(id)] = true;

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
