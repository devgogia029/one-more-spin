using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachineManager : MonoBehaviour
{
    [Header("Game Settings")]

    public int startingBalance = 1000; //number of coins the player starts with
    public int minimumBet = 10; //smallest amount the player can bet
    public int maximumBet = 100; //largest amount the player can bet
    public int betStep = 10; //amount added or removed whenever the player changes the bet

    [Header("Slot Symbols")]

    public Sprite[] symbols; //stores the four slot symbols in the correct order: 7, Cherries, Bell, BAR
    public string[] symbolNames = { "7", "Cherries", "Bell", "BAR" }; //stores the names used when displaying the winning result
    public int[] payoutMultipliers = { 30, 15, 10, 5 }; //stores the payout multiplier for each symbol in the same order; these give a return of about 94% over many spins

    [Header("Reels")]

    public SlotReel[] reels; //stores references to the three reels in the scene

    [Header("UI References")]

    public TMP_Text balanceText; //reference to the text that displays the player's balance
    public TMP_Text betText; //reference to the text that displays the current bet
    public TMP_Text resultText; //reference to the text that displays wins and losses

    public Button spinButton; //reference to the invisible button placed over the lever; clicking the lever starts a spin
    public Button increaseBetButton; //reference to the button that increases the bet
    public Button decreaseBetButton; //reference to the button that decreases the bet

    [Header("Lever")]

    public Animator leverAnimator; //reference to the Animator on the lever image; plays the pull animation

    [Header("Game Over")]

    public GameObject gameOverPopup; //reference to the popup shown when the player runs out of coins
    public Button playAgainButton; //reference to the Yes button on the popup that restarts the game

    [Header("Audio")]

    public AudioManager audioManager; //reference to the object that plays the sound effects and the background music

    private int balance; //stores the player's current balance
    private int currentBet; //stores the amount the player has selected to bet
    private bool isGameRunning; //true while the reels are spinning; false when the game is ready for another spin

    void Start()
    {
        if (!CheckSetup()) //checks whether all the required objects and settings have been assigned
        {
            enabled = false; //disables this script if something important is missing
            return; //stops the rest of Start so the game does not run with an incomplete setup
        }

        balance = startingBalance; //sets the player's balance to the starting amount

        currentBet = minimumBet; //starts the player at the minimum bet

        for (int i = 0; i < reels.Length; i++) //goes through all three reels
        {
            reels[i].Initialize(symbols); //gives each reel the four symbols and creates its scrolling strip
        }

        spinButton.onClick.AddListener(Spin); //calls Spin whenever the player clicks the lever

        increaseBetButton.onClick.AddListener(IncreaseBet); //calls IncreaseBet whenever the player clicks the increase button

        decreaseBetButton.onClick.AddListener(DecreaseBet); //calls DecreaseBet whenever the player clicks the decrease button

        playAgainButton.onClick.AddListener(RestartGame); //calls RestartGame whenever the player clicks Yes on the game over popup

        gameOverPopup.SetActive(false); //hides the game over popup when the game starts

        audioManager.PlayMusic(); //starts the looping background music

        resultText.text = "Place your bet and spin!"; //displays the starting message

        UpdateUI(); //updates the balance, bet and button states when the game starts
    }

    bool CheckSetup()
    {
        if (startingBalance < minimumBet) //checks whether the starting balance is enough to place the minimum bet
        {
            Debug.LogError("Starting balance must be at least the minimum bet."); //shows an error if the starting balance is too low
            return false; //stops the game setup
        }

        if (minimumBet <= 0 || maximumBet < minimumBet || betStep <= 0) //checks whether the bet settings make sense
        {
            Debug.LogError("Check your minimum bet, maximum bet and bet step settings."); //shows an error if the bet settings are incorrect
            return false; //stops the game setup
        }

        if (symbols == null || symbols.Length != 4) //checks whether exactly four symbols have been assigned
        {
            Debug.LogError("Assign exactly four symbols to the Game Manager."); //shows an error if the symbol array is incorrect
            return false; //stops the game setup
        }

        if (symbolNames == null || symbolNames.Length != 4 || payoutMultipliers == null || payoutMultipliers.Length != 4) //checks whether the names and payout values match the four symbols
        {
            Debug.LogError("You need four symbol names and four payout multipliers."); //shows an error if the payout setup is incomplete
            return false; //stops the game setup
        }

        for (int i = 0; i < symbols.Length; i++) //checks every symbol
        {
            if (symbols[i] == null || string.IsNullOrWhiteSpace(symbolNames[i]) || payoutMultipliers[i] < 0) //checks whether the symbol, name or payout is invalid
            {
                Debug.LogError("Check the symbol, symbol name and payout settings at index " + i); //shows which symbol needs to be fixed
                return false; //stops the game setup
            }
        }

        if (reels == null || reels.Length != 3) //checks whether exactly three reels have been assigned
        {
            Debug.LogError("Assign exactly three reels to the Game Manager."); //shows an error if the reel array is incorrect
            return false; //stops the game setup
        }

        for (int i = 0; i < reels.Length; i++) //checks each reel reference
        {
            if (reels[i] == null) //checks whether a reel has not been assigned
            {
                Debug.LogError("One or more reels are missing from the Game Manager."); //shows an error if a reel reference is empty
                return false; //stops the game setup
            }

            for (int j = i + 1; j < reels.Length; j++) //checks the remaining reel references for duplicates
            {
                if (reels[i] == reels[j]) //checks whether the same reel has been assigned more than once
                {
                    Debug.LogError("Each reel must be a separate object."); //shows an error if a reel has been assigned multiple times
                    return false; //stops the game setup
                }
            }
        }

        if (balanceText == null || betText == null || resultText == null) //checks whether all three text references have been assigned
        {
            Debug.LogError("Assign all three text objects to the Game Manager."); //shows an error if a text reference is missing
            return false; //stops the game setup
        }

        if (spinButton == null || increaseBetButton == null || decreaseBetButton == null) //checks whether all three button references have been assigned
        {
            Debug.LogError("Assign all three buttons to the Game Manager."); //shows an error if a button reference is missing
            return false; //stops the game setup
        }

        if (leverAnimator == null) //checks whether the lever Animator has been assigned
        {
            Debug.LogError("Assign the lever Animator to the Game Manager."); //shows an error if the lever Animator is missing
            return false; //stops the game setup
        }

        if (gameOverPopup == null || playAgainButton == null) //checks whether the game over popup and its Yes button have been assigned
        {
            Debug.LogError("Assign the game over popup and the play again button to the Game Manager."); //shows an error if the popup references are missing
            return false; //stops the game setup
        }

        if (audioManager == null) //checks whether the Audio Manager has been assigned
        {
            Debug.LogError("Assign the Audio Manager to the Game Manager."); //shows an error if the Audio Manager is missing
            return false; //stops the game setup
        }

        return true; //tells Start that the setup is valid
    }

    public Sprite GetSymbol(int symbolIndex)
    {
        return symbols[symbolIndex]; //returns the sprite at the requested symbol index
    }

    void Spin()
    {
        if (isGameRunning || balance < currentBet) //checks whether the game is already running or the player cannot afford the bet
            return; //stops the spin if either condition is true

        leverAnimator.SetTrigger("Pull"); //plays the lever pull animation
        audioManager.PlayLever(); //plays the lever sound
        audioManager.PlayMusic(); //makes sure the music is playing, because browsers only allow sound after the player has clicked something
        StartCoroutine(SpinRoutine()); //starts the complete slot machine spin
    }

    IEnumerator SpinRoutine()
    {
        isGameRunning = true; //prevents the player from starting another spin
        balance -= currentBet; //deducts the selected bet from the player's balance
        UpdateUI(); //updates the balance and disables the buttons while the reels are spinning
        resultText.text = "Spinning..."; //displays a message while the reels are moving
        int[] results = new int[reels.Length]; //creates an array to store the final result of each reel
        bool[] hasStopped = new bool[reels.Length]; //remembers which reels have already played their stop sound
        for (int i = 0; i < reels.Length; i++) //goes through each reel
        {
            results[i] = Random.Range(0, symbols.Length); //selects a random symbol index for this reel

            reels[i].SpinTo(results[i], i * 0.4f); //starts this reel and adds a small delay so the reels stop one after another
        }

        while (AreReelsSpinning()) //keeps waiting while at least one reel is still moving
        {
            PlayReelStopSounds(hasStopped); //plays a stop sound for any reel that has just finished
            yield return null; //waits for the next frame before checking again
        }

        PlayReelStopSounds(hasStopped); //catches the last reel, which stops at the moment the loop ends
        CheckResult(results); //checks whether the player has won or lost

        while (currentBet > balance && currentBet - betStep >= minimumBet) //keeps lowering the bet while the player cannot afford it and a lower bet is still allowed
        {
            currentBet -= betStep; //lowers the bet by one step so the player is never stuck with a bet they cannot afford
        }

        if (balance < minimumBet) //checks whether the player can no longer afford even the smallest bet
        {
            yield return new WaitForSeconds(1.5f); //pauses briefly so the player can read the final result first

            ShowGameOver(); //shows the game over popup
        }

        isGameRunning = false; //allows the player to start another spin
        UpdateUI(); //updates the balance, bet and buttons after the spin has finished
    }

    bool AreReelsSpinning()
    {
        for (int i = 0; i < reels.Length; i++) //checks each reel one by one
        {
            if (reels[i].IsSpinning) //checks whether this reel is still moving
                return true; //tells the Game Manager that the spin is not finished yet
        }

        return false; //tells the Game Manager that all reels have stopped
    }

    void PlayReelStopSounds(bool[] hasStopped)
    {
        for (int i = 0; i < reels.Length; i++) //checks each reel one by one
        {
            if (!hasStopped[i] && reels[i].HasLanded) //checks whether this reel has visually landed and has not played its sound yet
            {
                hasStopped[i] = true; //remembers that this reel has played its stop sound so it never plays twice

                audioManager.PlayReelStop(); //plays the reel stop sound
            }
        }
    }

    void CheckResult(int[] results)
    {
        bool isWin = results[0] == results[1] && results[1] == results[2]; //checks whether all three reels landed on the same symbol
        if (isWin) //runs this section when all three symbols match
        {
            int winningSymbol = results[0]; //gets the index of the matching symbol
            int winnings = currentBet * payoutMultipliers[winningSymbol]; //calculates the total payout using the matching symbol's multiplier
            balance += winnings; //adds the winnings to the player's balance
            resultText.text = "JACKPOT! " + symbolNames[winningSymbol] + " x3! +" + winnings + " coins"; //displays the winning symbol and payout in a short message that fits the result box
            audioManager.PlayWin(); //plays the win sound
        }
        else //runs this section when the three symbols do not match
        {
            resultText.text = "No win this time! Try again."; //displays the losing message
            audioManager.PlayLose(); //plays the lose sound
        }
    }

    void ShowGameOver()
    {
        resultText.text = "You ran out of coins!"; //tells the player why the game has stopped
        gameOverPopup.SetActive(true); //shows the game over popup
        audioManager.PlayGameOver(); //plays the game over sound
    }

    void RestartGame()
    {
        balance = startingBalance; //gives the player the starting balance again
        currentBet = minimumBet; //puts the bet back to the minimum
        gameOverPopup.SetActive(false); //hides the game over popup
        resultText.text = "Place your bet and spin!"; //displays the starting message again
        UpdateUI(); //updates the balance, bet and button states for the new game
    }

    void IncreaseBet()
    {
        if (isGameRunning) //checks whether the reels are currently spinning
            return; //stops the bet from changing during a spin

        if (currentBet + betStep <= maximumBet && currentBet + betStep <= balance) //checks whether the next bet is within the maximum and affordable
        {
            currentBet += betStep; //increases the current bet
            UpdateUI(); //updates the bet text and button states
        }
    }

    void DecreaseBet()
    {
        if (isGameRunning) //checks whether the reels are currently spinning
            return; //stops the bet from changing during a spin

        if (currentBet - betStep >= minimumBet) //checks whether the next bet stays above the minimum
        {
            currentBet -= betStep; //decreases the current bet
            UpdateUI(); //updates the bet text and button states
        }
    }

    void UpdateUI()
    {
        balanceText.text = "Balance: " + balance; //updates the balance text whenever the player wins or loses coins
        betText.text = "Bet: " + currentBet; //updates the bet amount displayed on the screen
        spinButton.interactable = !isGameRunning && balance >= currentBet; //disables the lever while the reels are moving or the player cannot afford the bet
        increaseBetButton.interactable = !isGameRunning && currentBet + betStep <= maximumBet && currentBet + betStep <= balance; //disables Increase Bet when the next bet is too high or unaffordable
        decreaseBetButton.interactable = !isGameRunning && currentBet - betStep >= minimumBet; //disables Decrease Bet when the player has already reached the minimum bet
    }

    void OnDestroy()
    {
        if (spinButton != null) //checks whether the lever button still exists
            spinButton.onClick.RemoveListener(Spin); //removes the Spin listener when this object is destroyed

        if (increaseBetButton != null) //checks whether the Increase Bet button still exists
            increaseBetButton.onClick.RemoveListener(IncreaseBet); //removes the Increase Bet listener when this object is destroyed

        if (decreaseBetButton != null) //checks whether the Decrease Bet button still exists
            decreaseBetButton.onClick.RemoveListener(DecreaseBet); //removes the Decrease Bet listener when this object is destroyed

        if (playAgainButton != null) //checks whether the Play Again button still exists
            playAgainButton.onClick.RemoveListener(RestartGame); //removes the Restart listener when this object is destroyed
    }
}