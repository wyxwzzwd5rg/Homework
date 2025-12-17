using UnityEngine;

/// <summary>
/// 溶解油画并显示隐藏道具（伞）。可与InteractableObject的 onInteractSuccess 绑定。
/// </summary>
public class PaintingDissolveController : MonoBehaviour
{
    public GameObject paintingRoot;      // 要隐藏/溶解的油画
    public GameObject hiddenItemObject;  // 溶解后显示的伞物体（带ItemClickHandler）
    public string umbrellaItemId = "item_umbrella";
    public Sprite umbrellaSprite;        // 若无场景物体，可直接发放占位素材
    public Color placeholderColor = new Color(0.6f, 0.8f, 1f, 1f);

    private bool _hasDissolved = false;

    public void DissolveAndReveal()
    {
        if (_hasDissolved) return;
        _hasDissolved = true;

        if (paintingRoot != null) paintingRoot.SetActive(false);

        if (hiddenItemObject != null)
        {
            // 如果场景中已经放置了伞物体，确保可拾取
            hiddenItemObject.SetActive(true);
            ItemClickHandler item = hiddenItemObject.GetComponent<ItemClickHandler>();
            if (item != null) item.CheckAndSetActive();
            Debug.Log("油画溶解，显示场景内的伞物体");
        }
        else
        {
            // 没有场景物体则直接发放背包道具
            GiveUmbrellaToBackpack();
        }
    }

    private void GiveUmbrellaToBackpack()
    {
        if (GameData.IsItemCollected(umbrellaItemId))
        {
            Debug.Log("伞已领取过");
            return;
        }

        Sprite spriteToUse = EnsureSprite();
        if (BackpackManager.Instance != null)
        {
            BackpackManager.Instance.CollectItem(spriteToUse);
            GameData.AddCollectedItem(umbrellaItemId);
            Debug.Log("直接发放伞道具到背包");
        }
        else
        {
            Debug.LogError("找不到BackpackManager实例，无法发放伞道具");
        }
    }

    private Sprite EnsureSprite()
    {
        if (umbrellaSprite != null) return umbrellaSprite;

        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++) colors[i] = placeholderColor;
        tex.SetPixels(colors);
        tex.Apply();
        Sprite generated = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        generated.name = "yusan";
        Debug.LogWarning("未找到伞素材，生成占位Sprite：yusan");
        return generated;
    }
}

