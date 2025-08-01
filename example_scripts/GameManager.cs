using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int maxRounds = 30;
    public float roundTime = 115f;
    public float freezeTime = 15f;
    public float bombTimer = 40f;
    
    [Header("Teams")]
    public Team terroristTeam;
    public Team counterTerroristTeam;
    
    public GameState currentState = GameState.WaitingForPlayers;
    public int currentRound = 0;
    public float roundTimeRemaining;
    public bool bombPlanted = false;
    public float bombTimeRemaining;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeGame()
    {
        terroristTeam = new Team("Terrorists", TeamSide.Terrorist);
        counterTerroristTeam = new Team("Counter-Terrorists", TeamSide.CounterTerrorist);
    }
    
    private void Update()
    {
        if (currentState == GameState.RoundActive)
        {
            UpdateRoundTimer();
        }
        
        if (bombPlanted && !bombTimeRemaining <= 0)
        {
            UpdateBombTimer();
        }
    }
    
    private void UpdateRoundTimer()
    {
        roundTimeRemaining -= Time.deltaTime;
        UIManager.Instance.UpdateRoundTime(roundTimeRemaining);
        
        if (roundTimeRemaining <= 0 && !bombPlanted)
        {
            EndRound(TeamSide.CounterTerrorist, RoundEndReason.TimeExpired);
        }
    }
    
    private void UpdateBombTimer()
    {
        bombTimeRemaining -= Time.deltaTime;
        UIManager.Instance.UpdateBombTimer(bombTimeRemaining);
        
        if (bombTimeRemaining <= 0)
        {
            EndRound(TeamSide.Terrorist, RoundEndReason.BombExploded);
        }
    }
    
    public void StartRound()
    {
        currentRound++;
        currentState = GameState.FreezeTime;
        roundTimeRemaining = roundTime;
        bombPlanted = false;
        bombTimeRemaining = 0f;
        
        // Reset player positions
        RespawnAllPlayers();
        
        // Start freeze time countdown
        Invoke(nameof(StartRoundActive), freezeTime);
        
        // Update UI
        UIManager.Instance.UpdateRoundInfo(currentRound, terroristTeam.score, counterTerroristTeam.score);
        
        // Play round start audio
        AudioManager.Instance.OnRoundStart(GetLocalPlayerTeam());
    }
    
    private void StartRoundActive()
    {
        currentState = GameState.RoundActive;
        
        // Enable player movement
        EnableAllPlayers();
    }
    
    public void EndRound(TeamSide winner, RoundEndReason reason)
    {
        currentState = GameState.RoundEnd;
        
        if (winner == TeamSide.Terrorist)
            terroristTeam.score++;
        else
            counterTerroristTeam.score++;
            
        // Award money based on performance
        EconomySystem.Instance.CalculateRoundRewards(winner, reason);
        
        // Show round end message
        bool playerTeamWon = (GetLocalPlayerTeam() == winner);
        string message = playerTeamWon ? "Round Won!" : "Round Lost!";
        UIManager.Instance.ShowMessage(message);
        
        // Play round end audio
        AudioManager.Instance.OnRoundEnd(playerTeamWon);
        
        // Check if match is over
        if (IsMatchOver())
        {
            EndMatch();
        }
        else
        {
            Invoke(nameof(StartRound), 5f);
        }
    }
    
    private bool IsMatchOver()
    {
        int roundsToWin = (maxRounds / 2) + 1;
        return terroristTeam.score >= roundsToWin || 
               counterTerroristTeam.score >= roundsToWin;
    }
    
    private void EndMatch()
    {
        currentState = GameState.MatchEnd;
        
        TeamSide winner = terroristTeam.score > counterTerroristTeam.score ? 
                         TeamSide.Terrorist : TeamSide.CounterTerrorist;
        
        string winnerName = winner == TeamSide.Terrorist ? "Terrorists" : "Counter-Terrorists";
        UIManager.Instance.ShowMessage($"Match Over! {winnerName} Win!");
        
        // Return to main menu after delay
        Invoke(nameof(ReturnToMainMenu), 10f);
    }
    
    private void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    public void CheckRoundEnd()
    {
        if (currentState != GameState.RoundActive) return;
        
        // Check if all players eliminated
        if (CountAlivePlayers(TeamSide.Terrorist) == 0)
        {
            EndRound(TeamSide.CounterTerrorist, RoundEndReason.EliminateEnemies);
            return;
        }
        
        if (CountAlivePlayers(TeamSide.CounterTerrorist) == 0)
        {
            EndRound(TeamSide.Terrorist, RoundEndReason.EliminateEnemies);
            return;
        }
    }
    
    private void RespawnAllPlayers()
    {
        NetworkPlayerController[] players = FindObjectsOfType<NetworkPlayerController>();
        foreach (NetworkPlayerController player in players)
        {
            player.Respawn();
        }
    }
    
    private void EnableAllPlayers()
    {
        NetworkPlayerController[] players = FindObjectsOfType<NetworkPlayerController>();
        foreach (NetworkPlayerController player in players)
        {
            player.SetMovementEnabled(true);
        }
    }
    
    private int CountAlivePlayers(TeamSide team)
    {
        NetworkPlayerController[] players = FindObjectsOfType<NetworkPlayerController>();
        int count = 0;
        
        foreach (NetworkPlayerController player in players)
        {
            if (player.team == team && player.isAlive)
                count++;
        }
        
        return count;
    }
    
    private TeamSide GetLocalPlayerTeam()
    {
        NetworkPlayerController localPlayer = FindLocalPlayer();
        return localPlayer != null ? localPlayer.team : TeamSide.CounterTerrorist;
    }
    
    private NetworkPlayerController FindLocalPlayer()
    {
        NetworkPlayerController[] players = FindObjectsOfType<NetworkPlayerController>();
        foreach (NetworkPlayerController player in players)
        {
            if (player.isLocalPlayer)
                return player;
        }
        return null;
    }
}

public enum GameState
{
    WaitingForPlayers,
    FreezeTime,
    RoundActive,
    RoundEnd,
    MatchEnd
}

public enum TeamSide
{
    Terrorist,
    CounterTerrorist
}

public enum RoundEndReason
{
    TimeExpired,
    BombExploded,
    BombDefused,
    EliminateEnemies,
    HostagesRescued
}

[System.Serializable]
public class Team
{
    public string name;
    public TeamSide side;
    public int score;
    public List<NetworkPlayerController> players;
    
    public Team(string teamName, TeamSide teamSide)
    {
        name = teamName;
        side = teamSide;
        score = 0;
        players = new List<NetworkPlayerController>();
    }
    
    public void AddPlayer(NetworkPlayerController player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }
    }
    
    public void RemovePlayer(NetworkPlayerController player)
    {
        players.Remove(player);
    }
}