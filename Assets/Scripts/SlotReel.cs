using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SlotReel : MonoBehaviour
{
    public float spinDuration = 2.5f; //how long the reel takes to finish spinning
    public float symbolHeight = 140f; //height of each symbol; matching the size of the reel window
    public int totalSymbols = 30; //number of symbols in the scrolling strip

    private RectTransform symbolStrip; //reference to the vertical strip that holds all the symbols
    private Image[] symbolImages; //stores all the symbol Images so they can be changed without searching again
    private Sprite[] reelSymbols; //stores the four symbols assigned to this reel
    private bool isSpinning; //true when the reel is spinning; false when stopped
    private bool isReady; //true when symbol strip has been created successfully

    public bool IsSpinning => isSpinning; //lets the manager check whether this reel is still spinning
    public bool IsReady => isReady; //lets the Game Manager check if the reel has been set up correctly

    void Awake()
    {
        RectMask2D mask = GetComponent<RectMask2D>(); //checks whether this reel already has a Rect Mask 2D component

        if (mask == null) //if the reel does not have a Rect Mask 2D
            gameObject.AddComponent<RectMask2D>(); //adds one so the symbols stay inside the reel window

        Image reelImage = GetComponent<Image>(); //gets the Image component used for the reel window

        if (reelImage != null) //checks whether an Image component exists
            reelImage.color = new Color(1f, 1f, 1f, 0f); //makes the reel background transparent so it does not cover the symbols
    }

    public void Initialize(Sprite[] symbols)
    {
        if (isReady) //checks whether this reel has already been initialised
            return; //stops the method so another symbol strip is not created

        if (symbols == null || symbols.Length != 4) //checks whether exactly four symbols have been assigned
        {
            Debug.LogError("Slot needs exactly four symbols."); //shows an error in the Console if the symbol setup is incorrect
            return; //stops the method because the reel needs four symbols
        }

        for (int i = 0; i < symbols.Length; i++) //goes through every symbol in the array
        {
            if (symbols[i] == null) //checks whether a symbol is missing
            {
                Debug.LogError("One or more slot symbols are missing."); //shows an error in the Console
                return; //stops the method because a missing sprite would cause an empty symbol
            }
        }

        if (symbolHeight <= 0f || totalSymbols < 2 || spinDuration <= 0f) //checks whether the reel settings are valid
        {
            Debug.LogError("Check the reel settings on " + gameObject.name); //shows which reel has incorrect settings
            return; //stops the method because the reel cannot work correctly
        }

        reelSymbols = symbols; //stores the four symbols so this reel can access them whenever it needs to

        CreateSymbolStrip(symbols); //creates the vertical strip and fills it with the four symbols

        isReady = true; //tells the Game Manager that this reel is ready to use
    }

    void CreateSymbolStrip(Sprite[] symbols)
    {
        GameObject stripObject = new GameObject("SymbolStrip", typeof(RectTransform)); //creates an empty object with a Rect Transform to hold all the symbol images

        stripObject.transform.SetParent(transform, false); //makes the strip a child of this reel without changing its local transform

        symbolStrip = stripObject.GetComponent<RectTransform>(); //gets the Rect Transform of the new strip

        symbolStrip.anchorMin = new Vector2(0.5f, 1f); //anchors the strip to the top centre of the reel
        symbolStrip.anchorMax = new Vector2(0.5f, 1f); //keeps both anchors at the top centre
        symbolStrip.pivot = new Vector2(0.5f, 1f); //sets the pivot to the top centre so scrolling is easier to calculate
        symbolStrip.anchoredPosition = Vector2.zero; //starts the strip at the top of the reel
        symbolStrip.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, symbolHeight * totalSymbols); //sets the strip width to reel width and height to fit every symbol

        symbolImages = new Image[totalSymbols]; //creates an array to store every symbol Image in the strip

        for (int i = 0; i < totalSymbols; i++) //creates each symbol Image one by one
        {
            GameObject symbolObject = new GameObject("Symbol_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); //creates a new UI object for this symbol

            symbolObject.transform.SetParent(symbolStrip, false); //makes the symbol a child of the scrolling strip

            Image symbolImage = symbolObject.GetComponent<Image>(); //gets the Image component so the symbol sprite can be displayed

            RectTransform symbolRect = symbolObject.GetComponent<RectTransform>(); //gets the Rect Transform of the new symbol

            symbolRect.anchorMin = new Vector2(0f, 1f); //anchors the symbol to the top-left of the strip
            symbolRect.anchorMax = new Vector2(1f, 1f); //stretches the symbol across the width of the strip
            symbolRect.pivot = new Vector2(0.5f, 1f); //sets the pivot to the top centre of the symbol
            symbolRect.anchoredPosition = new Vector2(0f, -i * symbolHeight); //places each symbol below the previous one
            symbolRect.sizeDelta = new Vector2(0f, symbolHeight); //sets the symbol height while keeping its width stretched

            symbolImage.preserveAspect = true; //keeps the original proportions of the symbol when displaying it
            symbolImage.raycastTarget = false; //stops the symbol Image from blocking button clicks
            symbolImage.sprite = symbols[Random.Range(0, symbols.Length)]; //gives the symbol a random sprite when the strip is first created

            symbolImages[i] = symbolImage; //stores the Image reference so it can be updated later
        }
    }

    public void SpinTo(int targetSymbolIndex, float startDelay)
    {
        if (!isReady || isSpinning) //checks whether the reel is ready and not already spinning
            return; //stops the spin if the reel is unavailable

        if (targetSymbolIndex < 0 || targetSymbolIndex >= reelSymbols.Length) //checks whether the selected symbol index is valid
            return; //stops the spin if the requested symbol does not exist

        StartCoroutine(SpinRoutine(targetSymbolIndex, startDelay)); //starts the spinning animation
    }

    IEnumerator SpinRoutine(int targetSymbolIndex, float startDelay)
    {
        isSpinning = true; //marks this reel as spinning before the animation begins

        for (int i = 0; i < symbolImages.Length - 1; i++) //goes through every symbol except the final one
        {
            symbolImages[i].sprite = reelSymbols[Random.Range(0, reelSymbols.Length)]; //gives each position a random symbol for this spin
        }

        symbolImages[symbolImages.Length - 1].sprite = reelSymbols[targetSymbolIndex]; //sets the final position to the result selected by the Game Manager

        symbolStrip.anchoredPosition = Vector2.zero; //resets the strip to the top before starting the new spin

        yield return new WaitForSeconds(Mathf.Max(0f, startDelay)); //waits before starting so the reels can stop one after another

        float elapsedTime = 0f; //tracks how much time has passed during the animation

        float finalPosition = (totalSymbols - 1) * symbolHeight; //calculates how far the strip must move to show the final symbol

        while (elapsedTime < spinDuration) //keeps animating until the spin duration has passed
        {
            elapsedTime += Time.deltaTime; //adds the time passed since the previous frame

            float progress = Mathf.Clamp01(elapsedTime / spinDuration); //calculates the animation progress from zero to one

            float easedProgress = Mathf.SmoothStep(0f, 1f, progress); //slows the reel smoothly as it approaches its final position

            float currentPosition = Mathf.Lerp(0f, finalPosition, easedProgress); //calculates the strip's current position

            symbolStrip.anchoredPosition = new Vector2(0f, currentPosition); //moves the strip to the calculated position

            yield return null; //waits for the next frame before continuing the animation
        }

        symbolStrip.anchoredPosition = new Vector2(0f, finalPosition); //makes sure the strip ends at the exact final position

        isSpinning = false; //tells the Game Manager that this reel has stopped
    }
}