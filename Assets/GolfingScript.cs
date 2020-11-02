using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class GolfingScript : MonoBehaviour {
    public KMBombInfo bomb;
    public KMAudio audio;
    public KMBombModule module;
    public KMSelectable[] arrows;
    public KMSelectable[] balls;
    public KMSelectable clubSelect;
    public Material[] clubs;
    public Color[] colorOfBalls;
    public MeshRenderer mesh;
    public KMSelectable[] NumberButtons;
    public TextMesh NumberForStageThree;
    public KMSelectable NumberSubmit;
    private static int moduleId = 0;
    string alphabetPos = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    int col1;
    int[] row1 = {0, 0, 0, 0, 0};
    int col2;
    int row2;
    int move = 0;
    int stage = 1;
    int correctClub = 0;
    static readonly int[][] clubTable = new int[][]{ new int[]{ 4, 2, 3, 12, 1, 8}, new int[] { 6, 3, 1, 4, 2, 9}, new int[] { 7, 1, 6, 8, 13, 5}, new int[] { 4, 2, 3, 12, 1, 8}, new int[] { 12, 10, 18, 5, 7, 3}};
    int addedNumberForClub = 0;
    int ballValue = 1;
    int swingPower = 0;
    int[] correctStages = { 0, 0, 0 };
    static readonly char[,] numberTable = new char[5,5]{
        {'A', 'B', 'C', 'D', 'E'},
        {'C', 'B', 'D', 'E', 'F'},
        {'E', 'I', 'B',	'C', 'F'},
        {'G', 'A', 'B', 'E', 'J'},
        {'D', 'A', 'J', 'C', 'G'}
    };

    void Start()
    {
        moduleId++;
        move = Rnd.Range(0, 4);
        mesh.material = clubs[move]; // For the first step.
        clubGet();
        for (int i = 0; i < balls.Length; i++)
        {
            balls[i].gameObject.SetActive(false);
            balls[i].GetComponent<MeshRenderer>().material.color = colorOfBalls[Rnd.Range(0, colorOfBalls.Length)];
        }
        NumberButtons[0].gameObject.SetActive(false);
        NumberButtons[1].gameObject.SetActive(false);
        NumberForStageThree.gameObject.SetActive(false);
        NumberSubmit.gameObject.SetActive(false);
        arrows[0].OnInteract += Left;
        arrows[1].OnInteract += Right;
        clubSelect.OnInteract += delegate
        {
            clubSubmit();
            return false;
        };
        for (int i = 0; i < balls.Length; i++){
            int j = i;
            balls[i].OnInteract += delegate(){
                ballSubmit(j);
                return false;
            };
        }
        NumberButtons[0].OnInteract += Up;
        NumberButtons[1].OnInteract += Down;
        NumberSubmit.OnInteract += delegate{
            numberSubmit();
            return false;
        };
    }
    bool Left()
    {
        move--;
        move = move < 0 ? 0 : move;
        mesh.material = clubs[move];
        return false;
    }
    bool Right()
    {
        move++;
        move = move > 4 ? 4 : move;
        mesh.material = clubs[move];
        return false;
    }
    bool Up(){
       NumberForStageThree.text = ""+(int.Parse(NumberForStageThree.text)+1);
       NumberForStageThree.text = int.Parse(NumberForStageThree.text) > 26 ? ""+(int.Parse(NumberForStageThree.text)-1) : ""+(int.Parse(NumberForStageThree.text));
       return false;
    }
    bool Down(){
        NumberForStageThree.text = ""+(int.Parse(NumberForStageThree.text)-1);
        NumberForStageThree.text = int.Parse(NumberForStageThree.text) < 1 ? ""+(int.Parse(NumberForStageThree.text)+1) : ""+(int.Parse(NumberForStageThree.text));
        return false;
    }
    static char caesarCipher(char ch, int key) {  
        if (!char.IsLetter(ch)) {  
            return ch;  
        }
        char d = char.IsUpper(ch) ? 'A' : 'a';  
        return (char)((((ch + key) - d) % 26) + d);
    } 
    static char atBash(char ch){
        string actual = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string atbased = "ZYXWVUTSRQPONMLKJIHGFEDCBA";
        int yes = 0;
        for (int i = 0; i < actual.Length; i++){
            if (actual[i] == ch){
                yes = i;
            }
        }
        return atbased[yes];
    }   
    void clubGet()
    {
        // Getting the row of table 1.
        if (bomb.GetOnIndicators().Count() > bomb.GetOffIndicators().Count())
        {
            row1[0] = 1;
        }
        if (bomb.GetPortCount() > bomb.GetBatteryCount())
        {
            row1[1] = 1;
        }
        if (bomb.GetIndicators().Count() < bomb.GetSolvableModuleNames().Count())
        {
            row1[2] = 1;
        }
        Debug.LogFormat("[Hole in one #{0}] Modules = {1} and Battery Holders = {2}", moduleId, bomb.GetModuleNames().Count(), bomb.GetBatteryHolderCount());
        if (bomb.GetBatteryHolderCount() == bomb.GetModuleNames().Count())
        {
            row1[3] = 1;
        }
        if (row1[0] == 0 && row1[1] == 0 && row1[2] == 0 && row1[3] == 0)
        {
            row1[4] = 1;
        }
        Debug.LogFormat("[Hole in one #{0}] Rows: {1}", moduleId, row1.Join());

        // Getting the column of table 1.
        int solvedModule = bomb.GetSolvedModuleNames().Count();
        if (solvedModule >= 0 && solvedModule <= 3)
        {
            col1 = 0;
        }
        else if (solvedModule >= 4 && solvedModule <= 6)
        {
            col1 = 1;
        }
        else if (solvedModule >= 7 && solvedModule <= 9)
        {
            col1 = 2;
        }
        else if (solvedModule >= 10 && solvedModule <= 12)
        {
            col1 = 3;
        }
        else if (solvedModule >= 13 && solvedModule <= 15)
        {
            col1 = 4;
        }
        else
        {
            col1 = 5;
        }
        Debug.LogFormat("[Hole in one #{0}] Column: {1}", moduleId, col1);
        for (int i = 0; i < clubTable.Length; i++)
        {
            for (int j = 0; j < clubTable[i].Length; j++)
            {
                if (row1[i] == 1 && j == col1)
                {
                    addedNumberForClub += clubTable[i][j];
                }
            }
        }
        addedNumberForClub %= 5;
        Debug.LogFormat("[Hole in one #{0}] Club position (index 1) to choose: {1}", moduleId, addedNumberForClub+1);
    }
    void ballGet(){
        int rValue = 0;
        int gValue = 0;
        int bValue = 0;
        for (int i = 0; i < balls.Length; i++){
            Debug.LogFormat("[Hole in one #{0}] Ball {1}'s color components are (r, g, b): {2}, {3}, {4}", moduleId, i, balls[i].GetComponent<MeshRenderer>().material.color.r, balls[i].GetComponent<MeshRenderer>().material.color.g, balls[i].GetComponent<MeshRenderer>().material.color.b);
            if (balls[i].GetComponent<MeshRenderer>().material.color.r == 1){
                rValue++;
            } 
            if (balls[i].GetComponent<MeshRenderer>().material.color.g == 1){
                gValue++;
            } 
            if (balls[i].GetComponent<MeshRenderer>().material.color.b == 1){
                bValue++;
            }
        }
        Debug.LogFormat("[Hole in one #{0}] Red value: {1}, Green value: {2}, Blue Value: {3}", moduleId, rValue, gValue, bValue);
        
        ballValue *= (rValue != 0 ? rValue : 1);
        ballValue *= (gValue != 0 ? gValue : 1);
        ballValue *= (bValue != 0 ? bValue : 1);
        ballValue%=5;
        Debug.LogFormat("[Hole in one #{0}] Ball to press (index 1): {1}", moduleId, (ballValue%5)+1);
    }
    void numberGet(){
        char letterFromTable, CipheredLetter;
        int alphaYes = 0;
        letterFromTable = numberTable[ballValue, addedNumberForClub];
        CipheredLetter = atBash(caesarCipher(letterFromTable, addedNumberForClub));
        Debug.LogFormat("[Hole in one #{0}] Table letter: {1}, Ciphered letter: {2}", moduleId, letterFromTable, CipheredLetter);
        alphaYes = alphabetPos.IndexOf(CipheredLetter)+1;
        Debug.LogFormat("[Hole in one #{0}] Number for stage 3: {1}", moduleId, alphaYes);
        swingPower = alphaYes;
    }
    void clubSubmit(){
        if (stage == 1)
        {
            if (move == addedNumberForClub)
            {
                correctStages[0] = 1;
                Debug.LogFormat("[Hole in one #{0}] Stage 1 correct!", moduleId);
            }
            for (int i = 0; i < balls.Length; i++)
            {
                balls[i].gameObject.SetActive(true);
            }
            stage = 2;
            Debug.LogFormat("[Hole in one #{0}] Stage {1}.", moduleId, stage);
            ballGet();
        }
    }
    void ballSubmit(int some){
        if (stage == 2){
            if (some == ballValue){
                correctStages[1] = 1;
                Debug.LogFormat("[Hole in one #{0}] Stage 2 correct!", moduleId);
            }
            NumberButtons[0].gameObject.SetActive(true);
            NumberButtons[1].gameObject.SetActive(true);
            NumberForStageThree.text = ""+Rnd.Range(1, 26);
            NumberForStageThree.gameObject.SetActive(true);
            NumberSubmit.gameObject.SetActive(true);
            stage = 3;
            Debug.LogFormat("[Hole in one #{0}] Stage {1}.", moduleId, stage);
            numberGet();
        }
    }
    void numberSubmit(){
        if (stage == 3){
            if (swingPower == int.Parse(NumberForStageThree.text)){
                correctStages[2] = 1;
                Debug.LogFormat("[Hole in one #{0}] Stage 3 correct!", moduleId);
            }
            ActualSubmitting();
        }
    }
    void ActualSubmitting(){
        if (correctStages[0] == 1 && correctStages[1] == 1 && correctStages[2] == 1){
            module.HandlePass();
        }
        else{
            for (int i = 0; i < correctStages.Length; i++){
                if (correctStages[i] == 0){
                    Debug.LogFormat("[Hole in one #{0}] Stage {1} was incorrect.", moduleId, i);
                }
                else{
                    Debug.LogFormat("[Hole in one #{0}] Stage {1} was correct.", moduleId, i);
                }
            }
            module.HandleStrike();
            for (int i = 0; i < balls.Length; i++)
            {
                balls[i].gameObject.SetActive(false);
                balls[i].GetComponent<MeshRenderer>().material.color = colorOfBalls[Rnd.Range(0, colorOfBalls.Length)];
            }
            stage = 1;
            NumberButtons[0].gameObject.SetActive(false);
            NumberButtons[1].gameObject.SetActive(false);
            NumberForStageThree.gameObject.SetActive(false);
            NumberSubmit.gameObject.SetActive(false);
            addedNumberForClub = 0;
            ballValue = 1;
            swingPower = 0;
            for (int i = 0; i <= 2; i++){correctStages[i] = 0;}
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"If you're on stage 1: do '!{0} r #' to cycle the clubs right # of times, and '!{0} l #' to cycle the clubs left # of times, and '!{0} display' to press the display. If you're on stage 2: '!{0} ball #' to press a ball. If you're on stage 3: '!{0} u #' to increment the number by #, '!{0} d #' to decrement the number by #, and '!{0} submit' to submit the number.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command.ToLower();
        string[] parameters = command.Split(' ');
        if (parameters.Length < 2 && (parameters[0] != "submit" || parameters[0] != "Submit") && (parameters[0] != "display" || parameters[0] != "Display"))
        {
            yield return null;
            yield return "sendtochaterror Please specify a correct command.";
            yield break;
        }
        else if (parameters.Length > 2){
            yield return null;
            yield return "sendtochaterror There are too many parameters.";
            yield break;
        }
        if (stage == 1 && (parameters[0] == "left" || parameters[0] == "l"))
        {
            yield return null;
            for (int i = 0; i < int.Parse(parameters[1]); i++)
            {
                arrows[0].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if (stage == 1 && (parameters[0] == "right" || parameters[0] == "r"))
        {
            yield return null;
            for (int i = 0; i < int.Parse(parameters[1]); i++)
            {
                arrows[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if (stage == 3 && (parameters[0] == "up" || parameters[0] == "u"))
        {
            yield return null;
            for (int i = 0; i < int.Parse(parameters[1]); i++)
            {
                NumberButtons[0].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if (stage == 1 && (parameters[0] == "down" || parameters[0] == "d"))
        {
            yield return null;
            for (int i = 0; i < int.Parse(parameters[1]); i++)
            {
                NumberButtons[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if (stage == 2 && (parameters[0] == "ball" || parameters[1] == "Ball")){
            yield return null;
            balls[int.Parse(parameters[1])].OnInteract();
        }
        else if (stage == 1 && (parameters[0] == "display" || parameters[0] == "Display")){
            yield return null;
            clubSelect.OnInteract();
        }
        else if (stage == 3 && (parameters[0] == "submit" || parameters[0] == "Submit"))
        {
            yield return null;
            NumberSubmit.OnInteract();
        }
        else{
            yield return null;
            yield return "sendtochaterror This command is invalid.";
            yield break;
        }
    }
}