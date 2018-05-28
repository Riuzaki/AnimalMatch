using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{

    private AnimalItem[,] gridAnimalItems;
    private AnimalItem firstItemSelected;
    private AnimalItem helpAnimalItem;
    public AnimalItem tile;

    public List<Sprite> itemSprites = new List<Sprite>();
    private List<AnimalItem> listShift;

    private Color selectedColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    public delegate void SfxPlay(SfxEffect effect);
    public static event SfxPlay SfxPlayHandler;

    public delegate void ChangePoints(int isPaused);
    public static event ChangePoints ChangePointsHandler;

    private const int minMatch = 3;
    private const int pricePoint = 10;
    private const float timeHelp = 10.0f;

    public int xSize, ySize;
    private int pointsPlayer;

    private float timerHelp;
    private float offsetGridItem;

    private bool shiftAnimalItems;
    private bool helpItemIsActive;
    private bool activeMode;

    private void OnEnable()
    {
        AnimalItem.OnClickItemHandler += OnItemClick;
        GuiController.RestartGameHandler += RestartGame;
        GuiController.OnActiveGameHandler += SetActive;
    }

    private void OnDisable()
    {
        AnimalItem.OnClickItemHandler -= OnItemClick;
        GuiController.RestartGameHandler -= RestartGame;
        GuiController.OnActiveGameHandler -= SetActive;
    }

    void Start()
    {
        helpItemIsActive = false;
        shiftAnimalItems = false;
        activeMode = true;
        pointsPlayer = 0;
        timerHelp = 0.0f;
        offsetGridItem = 0.4f;
        gridAnimalItems = new AnimalItem[xSize, ySize];

        if (itemSprites.Count != 0)
        {
            Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
            InitGrid(offset.x + offsetGridItem, offset.y + offsetGridItem);
            CheckBoard();
        }

    }

    void Update()
    {
        if (activeMode && !shiftAnimalItems)
        {
            if (!helpItemIsActive)
                timerHelp += Time.deltaTime;
            if (timerHelp >= timeHelp)
            {
                HelpTime();
            }
        }

    }

    private void InitGrid(float offsetX, float offsetY)
    {
        float startX = transform.position.x;
        float startY = transform.position.y;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                AnimalItem newItem = null;
                if (!gridAnimalItems[x, y])
                    newItem = Instantiate(tile, new Vector3(startX + (offsetX * x), startY + (offsetY * y), transform.position.z), tile.transform.rotation);
                else
                    newItem = gridAnimalItems[x, y];
                gridAnimalItems[x, y] = newItem;
                gridAnimalItems[x, y].x = x;
                gridAnimalItems[x, y].y = y;
                newItem.transform.SetParent(transform);
                newItem.GetRenderer().sprite = itemSprites[Random.Range(0, itemSprites.Count)];
                while (isNeedFix(newItem))
                {
                    newItem.GetRenderer().sprite = itemSprites[Random.Range(0, itemSprites.Count)];
                }
            }
        }
    }

    private bool isNeedFix(AnimalItem item)
    {
        bool fix = false;
        int x = item.x;
        int y = item.y;
        if (x > 1)
        {
            if (gridAnimalItems[x - 1, y].GetRenderer().sprite == gridAnimalItems[x - 2, y].GetRenderer().sprite && gridAnimalItems[x - 2, y].GetRenderer().sprite == gridAnimalItems[x, y].GetRenderer().sprite)
                fix = true;
        }
        if (y > 1)
        {
            if (gridAnimalItems[x, y - 1].GetRenderer().sprite == gridAnimalItems[x, y - 2].GetRenderer().sprite && gridAnimalItems[x, y - 2].GetRenderer().sprite == gridAnimalItems[x, y].GetRenderer().sprite)
                fix = true;
        }

        return fix;
    }

    private void HelpTime()
    {
        AnimalItem item = HelpFind();
        if (item)
        {
            StartCoroutine(RotateForHelp(item));
            SfxPlayHandler(SfxEffect.Help);
            timerHelp = 0;
        }
        else
            CheckBoard();

    }

    private void CheckBoard()
    {
        AnimalItem item = HelpFind();
        while (!item)
        {
            MixBoard();
            item = HelpFind();
        }
    }

    public void OnItemClick(AnimalItem item)
    {

        if (activeMode && !shiftAnimalItems)
            if (firstItemSelected != null && firstItemSelected != item)
            {
                SwapAnimal(item);
            }
            else
            {
                Select(item);
            }
    }

    private void SetActive(bool active)
    {
        activeMode = active;
    }

    private void Select(AnimalItem item)
    {
        item.GetRenderer().color = selectedColor;
        firstItemSelected = item;
        SfxPlayHandler(SfxEffect.Select);
    }

    private void Deselect(AnimalItem item)
    {
        item.GetRenderer().color = Color.white;
        firstItemSelected = null;
    }

    private void SwapAnimal(AnimalItem item)
    {
        if (IsNeighbor(item))
        {

            Sprite swapSprite = firstItemSelected.GetRenderer().sprite;
            firstItemSelected.GetRenderer().sprite = item.GetRenderer().sprite;
            item.GetRenderer().sprite = swapSprite;
            List<List<AnimalItem>> matchList = new List<List<AnimalItem>>();
            matchList.Add(GetVerticalMatch(firstItemSelected));
            matchList.Add(GetHorizaontalMatch(firstItemSelected));
            matchList.Add(GetVerticalMatch(item));
            matchList.Add(GetHorizaontalMatch(item));
            bool foundMatch = false;
            for (int i = 0; i < matchList.Count; i++)
            {
                if (matchList[i].Count >= minMatch)
                {
                    foundMatch = true;
                    ClearMatches(matchList[i]);
                }
            }
            if (foundMatch)
            {
                if (helpItemIsActive)
                {
                    StopAllCoroutines();
                    ReRotateItem(helpAnimalItem);
                    helpItemIsActive = false;
                }
                FillingBoard();
                timerHelp = 0;
            }
            else
            {
                SfxPlayHandler(SfxEffect.CantSwap);
                swapSprite = firstItemSelected.GetRenderer().sprite;
                firstItemSelected.GetRenderer().sprite = item.GetRenderer().sprite;
                item.GetRenderer().sprite = swapSprite;
            }
            Deselect(firstItemSelected);
        }
        else
        {
            SfxPlayHandler(SfxEffect.CantSwap);
            Deselect(firstItemSelected);
        }

    }

    private bool CanSwap(AnimalItem item, AnimalItem item2)
    {
        bool can = false;
        Sprite swapSprite = item2.GetRenderer().sprite;
        item2.GetRenderer().sprite = item.GetRenderer().sprite;
        item.GetRenderer().sprite = swapSprite;
        if (GetVerticalMatch(item).Count >= minMatch || GetHorizaontalMatch(item).Count >= minMatch || GetHorizaontalMatch(item2).Count >= minMatch || GetVerticalMatch(item2).Count >= minMatch)
            can = true;
        else
            can = false;
        swapSprite = item2.GetRenderer().sprite;
        item2.GetRenderer().sprite = item.GetRenderer().sprite;
        item.GetRenderer().sprite = swapSprite;
        return can;

    }

    private AnimalItem HelpFind()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (x == xSize - 1 && y == ySize - 1)
                {
                    break;
                }
                else if (x == xSize - 1)
                {
                    if (CanSwap(gridAnimalItems[x, y], gridAnimalItems[x, y + 1]))
                        return gridAnimalItems[x, y];
                }
                else if (y == ySize - 1)
                {
                    if (CanSwap(gridAnimalItems[x, y], gridAnimalItems[x + 1, y]))
                        return gridAnimalItems[x, y];
                }
                else
                {
                    if (CanSwap(gridAnimalItems[x, y], gridAnimalItems[x, y + 1]))
                        return gridAnimalItems[x, y];
                    if (CanSwap(gridAnimalItems[x, y], gridAnimalItems[x + 1, y]))
                        return gridAnimalItems[x, y];
                }

            }
        }

        return null;
    }

    private void MixBoard()
    {
        SfxPlayHandler(SfxEffect.MixBoard);
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                int x2 = Random.Range(0, xSize - 1);
                int y2 = Random.Range(0, ySize - 1);
                while ((CanSwap(gridAnimalItems[x, y], gridAnimalItems[x2, y2])))
                {
                    x2 = Random.Range(0, xSize - 1);
                    y2 = Random.Range(0, ySize - 1);
                }
                Sprite swapSprite = gridAnimalItems[x, y].GetRenderer().sprite;
                gridAnimalItems[x, y].GetRenderer().sprite = gridAnimalItems[x2, y2].GetRenderer().sprite;
                gridAnimalItems[x2, y2].GetRenderer().sprite = swapSprite;

            }
        }

    }

    private void ClearMatches(List<AnimalItem> listItem)
    {
        ChangePointsHandler(pointsPlayer += listItem.Count * pricePoint);

        for (int i = 0; i < listItem.Count; i++)
        {
            listItem[i].GetRenderer().sprite = null;
        }
        SfxPlayHandler(SfxEffect.Clear);
    }

    private List<AnimalItem> FillingBoard()
    {
        List<AnimalItem> itemList = new List<AnimalItem>();
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (!gridAnimalItems[x, y].GetRenderer().sprite)
                {
                    itemList.Add(gridAnimalItems[x, y]);

                    if(shiftAnimalItems)
                    if (!listShift.Contains(gridAnimalItems[x, y]))
                    {
                        listShift.Add(gridAnimalItems[x, y]);
                    }

                }
            }
        }
        if (!shiftAnimalItems)
            listShift = itemList;
        StartCoroutine(ItemDown(itemList));
        return itemList;
    }

    private bool IsNeighbor(AnimalItem item)
    {
        return (item.y == firstItemSelected.y || item.x == firstItemSelected.x) && Mathf.Abs(item.y - firstItemSelected.y) <= 1 && Mathf.Abs(item.x - firstItemSelected.x) <= 1;
    }

    private List<AnimalItem> GetHorizaontalMatch(AnimalItem item)
    {
        List<AnimalItem> matchList = new List<AnimalItem>();
        matchList.Add(item);
        for (int i = item.x - 1; i >= 0; i--)
        {
            if (gridAnimalItems[i, item.y].GetRenderer().sprite == item.GetRenderer().sprite)
                matchList.Add(gridAnimalItems[i, item.y]);
            else break;
        }
        for (int i = item.x + 1; i < xSize; i++)
        {
            if (gridAnimalItems[i, item.y].GetRenderer().sprite == item.GetRenderer().sprite)
                matchList.Add(gridAnimalItems[i, item.y]);
            else break;
        }
        return matchList;
    }

    private List<AnimalItem> GetVerticalMatch(AnimalItem item)
    {
        List<AnimalItem> matchList = new List<AnimalItem>();
        matchList.Add(item);
        for (int i = item.y - 1; i >= 0; i--)
        {
            if (gridAnimalItems[item.x, i].GetRenderer().sprite == item.GetRenderer().sprite)
                matchList.Add(gridAnimalItems[item.x, i]);
            else break;
        }
        for (int i = item.y + 1; i < ySize; i++)
        {
            if (gridAnimalItems[item.x, i].GetRenderer().sprite == item.GetRenderer().sprite)
                matchList.Add(gridAnimalItems[item.x, i]);
            else break;
        }
        return matchList;
    }

    private bool FindMatch(List<AnimalItem> itemList)
    {
        bool isMatch = false;
        List<AnimalItem> horizontalMatch;
        List<AnimalItem> verticalMatch;
        for (int i = 0; i < itemList.Count; i++)
        {
            horizontalMatch = GetHorizaontalMatch(itemList[i]);
            verticalMatch = GetVerticalMatch(itemList[i]);
            if (horizontalMatch.Count >= minMatch)
            {
                ClearMatches(horizontalMatch);
                isMatch = true;
            }
            if (verticalMatch.Count >= minMatch)
            {
                ClearMatches(verticalMatch);
                isMatch = true;
            }
        }
        if (isMatch)
            FillingBoard();
        return isMatch;
    }

    private void ReRotateItem(AnimalItem item)
    {
        item.GetRenderer().transform.rotation = Quaternion.identity;
    }

    private void ItemsDown()
    {
        if (shiftAnimalItems)
        {
            if (FillingBoard().Count == 0)
            {
                shiftAnimalItems = false;
                FindMatch(listShift);
                CheckBoard();
            }
        }
    }

    private void RestartGame()
    {
        pointsPlayer = 0;
        timerHelp = 0.0f;
        StopAllCoroutines();
        if (helpItemIsActive)
            ReRotateItem(helpAnimalItem);

        helpItemIsActive = false;
        shiftAnimalItems = false;

        if (itemSprites.Count != 0)
        {
            Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
            InitGrid(offset.x + offsetGridItem, offset.y + offsetGridItem);
        }
    }

    IEnumerator ItemDown(List<AnimalItem> listItem)
    {
        shiftAnimalItems = true;
        if (listItem.Count != 0)
        {
            for (int i = 0; i < listItem.Count; i++)
            {
                int x = listItem[i].x;
                int y = listItem[i].y;
                if (y != ySize - 1)
                {
                    listItem[i].GetRenderer().sprite = gridAnimalItems[x, y + 1].GetRenderer().sprite;
                    gridAnimalItems[x, y + 1].GetRenderer().sprite = null;
                }
                else
                {
                    listItem[i].GetRenderer().sprite = itemSprites[Random.Range(0, itemSprites.Count)];
                }
            }
        }
        else
        {
            yield return null;
        }
        yield return new WaitForSeconds(.08f);
        ItemsDown();

    }

    IEnumerator RotateForHelp(AnimalItem item)
    {
        helpAnimalItem = item;
        helpItemIsActive = true;
        float deltaAngle = 5f;
        float maxAngle = 20f;

        for (float angle = 0; angle <= maxAngle; angle += deltaAngle)
        {
            item.GetRenderer().transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            yield return new WaitForSeconds(.05f);
        }
        for (float angle = maxAngle; angle >= -maxAngle; angle -= deltaAngle)
        {
            item.GetRenderer().transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            yield return new WaitForSeconds(.05f);
        }
        for (float angle = -maxAngle; angle <= 0; angle += deltaAngle)
        {
            item.GetRenderer().transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            yield return new WaitForSeconds(.05f);
        }
        helpItemIsActive = false;
    }

}
