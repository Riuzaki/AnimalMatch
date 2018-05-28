using UnityEngine;

public class AnimalItem : MonoBehaviour
{

    private SpriteRenderer itemRenderer;

    public int x;
    public int y;

    public delegate void OnClickItem(AnimalItem item);
    public static event OnClickItem OnClickItemHandler;

    void Awake()
    {
        itemRenderer = GetComponent<SpriteRenderer>();
        itemRenderer.size = new Vector2(3, 3);
    }

    void OnMouseDown()
    {
        OnClickItemHandler(this);
    }
    
    public SpriteRenderer GetRenderer()
    {
        return itemRenderer;
    }

}
