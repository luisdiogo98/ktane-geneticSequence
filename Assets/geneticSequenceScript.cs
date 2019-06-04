using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class geneticSequenceScript : MonoBehaviour
{
    public KMBombInfo bomb;
    public KMAudio Audio;

    public KMSelectable[] btns;
    public GameObject[] boxes;
    public Material[] mats;
    Dictionary<string, KeyValuePair<string, string>[]> map = new Dictionary<string, KeyValuePair<string, string>[]>();
    Dictionary<string, char[]> decoder = new Dictionary<string, char[]>();

    static System.Random rnd = new System.Random();

    bool reverse = false;
    char[] btnLabels = { 'A', 'T', 'C', 'G' };
    string[] AASequence = new string[4];
    char[] solution = new char[12];
    int inputPos = 0;
    char[] input = new char[12];

    static int moduleIdCounter = 1;
    int moduleId;
    bool moduleSolved = false;
    bool checking = false;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        btns[0].OnInteract += delegate () { PressATGCButton(btns[0]); return false; };
        btns[1].OnInteract += delegate () { PressATGCButton(btns[1]); return false; };
        btns[2].OnInteract += delegate () { PressATGCButton(btns[2]); return false; };
        btns[3].OnInteract += delegate () { PressATGCButton(btns[3]); return false; };
        btns[4].OnInteract += delegate () { PressButtonOk(); return false; };
        btns[5].OnInteract += delegate () { PressButtonClr(); return false; };

        PopulateMap();
        PopulateDecoder();
    }

    void Start()
    {
        RandomizeLabels();
        CalcStartingAA();
        CalcAASequence();
        CalcSolution();
    }

    void Update()
    {

    }

    void PressATGCButton(KMSelectable btn)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        btn.AddInteractionPunch(.5f);
        if (inputPos < 12 && !moduleSolved && !checking)
        {
            input[inputPos] = btn.GetComponentInChildren<TextMesh>().text[0];
            boxes[0].GetComponentsInChildren<Renderer>()[inputPos].material = ColorConversionGlow(input[inputPos]);
            inputPos++;
        }
    }

    void PressButtonOk()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        btns[4].AddInteractionPunch();
        if (inputPos == 12 && !moduleSolved && !checking)
        {
            checking = true;
            StartCoroutine("CheckingAnimation");
        }
    }

    void PressButtonClr()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        btns[4].AddInteractionPunch();

        if (!moduleSolved && !checking)
        {
            inputPos = 0;
            input = new char[12];
            Renderer[] entries = boxes[0].GetComponentsInChildren<Renderer>();

            foreach (Renderer entry in entries)
            {
                entry.material = mats[8];
            }
        }
    }

    void PopulateMap()
    {
        map.Add("Phe", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("BOB", "Asn"), new KeyValuePair<string, string>("FRQ", "Trp"), new KeyValuePair<string, string>("SND", "Cys") });
        map.Add("Leu", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("FRK", "His"), new KeyValuePair<string, string>("FRQ", "Lys"), new KeyValuePair<string, string>("TRN", "Val") });
        map.Add("Ile", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("FRK", "Val"), new KeyValuePair<string, string>("MSA", "Asn"), new KeyValuePair<string, string>("NSA", "Arg"), new KeyValuePair<string, string>("TRN", "Tyr") });
        map.Add("Met", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("CLR", "Asp"), new KeyValuePair<string, string>("MSA", "Arg"), new KeyValuePair<string, string>("TRN", "Gly") });
        map.Add("Val", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("BOB", "Ala"), new KeyValuePair<string, string>("FRK", "Ile"), new KeyValuePair<string, string>("TRN", "Leu") });
        map.Add("Ser", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("BOB", "Cys"), new KeyValuePair<string, string>("FRQ", "Gln"), new KeyValuePair<string, string>("SND", "Thr") });
        map.Add("Pro", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("CAR", "Cys"), new KeyValuePair<string, string>("CLR", "Asn"), new KeyValuePair<string, string>("FRK", "Gln") });
        map.Add("Thr", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("IND", "Lys"), new KeyValuePair<string, string>("MSA", "Gly"), new KeyValuePair<string, string>("NSA", "Glu"), new KeyValuePair<string, string>("SND", "Ser") });
        map.Add("Ala", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("BOB", "Val"), new KeyValuePair<string, string>("CAR", "Glu"), new KeyValuePair<string, string>("CLR", "His") });
        map.Add("Tyr", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("SIG", "His"), new KeyValuePair<string, string>("SND", "Lys"), new KeyValuePair<string, string>("TRN", "Ile") });
        map.Add("His", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("CLR", "Ala"), new KeyValuePair<string, string>("FRK", "Leu"), new KeyValuePair<string, string>("SIG", "Tyr") });
        map.Add("Gln", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("FRK", "Pro"), new KeyValuePair<string, string>("FRQ", "Ser"), new KeyValuePair<string, string>("SIG", "Arg") });
        map.Add("Asn", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("BOB", "Phe"), new KeyValuePair<string, string>("CLR", "Pro"), new KeyValuePair<string, string>("MSA", "Ile") });
        map.Add("Lys", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("IND", "Thr"), new KeyValuePair<string, string>("FRQ", "Leu"), new KeyValuePair<string, string>("SND", "Tyr") });
        map.Add("Asp", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("CLR", "Met"), new KeyValuePair<string, string>("IND", "Gly"), new KeyValuePair<string, string>("SIG", "Trp") });
        map.Add("Glu", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("CAR", "Ala"), new KeyValuePair<string, string>("IND", "Trp"), new KeyValuePair<string, string>("NSA", "Thr") });
        map.Add("Cys", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("BOB", "Ser"), new KeyValuePair<string, string>("CAR", "Pro"), new KeyValuePair<string, string>("SND", "Phe") });
        map.Add("Trp", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("FRQ", "Phe"), new KeyValuePair<string, string>("IND", "Glu"), new KeyValuePair<string, string>("SIG", "Asp") });
        map.Add("Arg", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("MSA", "Met"), new KeyValuePair<string, string>("NSA", "Ile"), new KeyValuePair<string, string>("SIG", "Gln") });
        map.Add("Gly", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("IND", "Asp"), new KeyValuePair<string, string>("MSA", "Thr"), new KeyValuePair<string, string>("TRN", "Met") });
    }

    void PopulateDecoder()
    {
        decoder.Add("Phe", new char[] { 'T', 'T', 'T' });
        decoder.Add("Leu", new char[] { 'T', 'T', 'A' });
        decoder.Add("Ile", new char[] { 'A', 'T', 'C' });
        decoder.Add("Met", new char[] { 'A', 'T', 'G' });
        decoder.Add("Val", new char[] { 'G', 'T', 'A' });
        decoder.Add("Ser", new char[] { 'T', 'C', 'G' });
        decoder.Add("Pro", new char[] { 'C', 'C', 'A' });
        decoder.Add("Thr", new char[] { 'A', 'C', 'C' });
        decoder.Add("Ala", new char[] { 'G', 'C', 'T' });
        decoder.Add("Tyr", new char[] { 'T', 'A', 'T' });
        decoder.Add("His", new char[] { 'C', 'A', 'T' });
        decoder.Add("Gln", new char[] { 'C', 'A', 'G' });
        decoder.Add("Asn", new char[] { 'A', 'A', 'C' });
        decoder.Add("Lys", new char[] { 'A', 'A', 'A' });
        decoder.Add("Asp", new char[] { 'G', 'A', 'T' });
        decoder.Add("Glu", new char[] { 'G', 'A', 'A' });
        decoder.Add("Cys", new char[] { 'T', 'G', 'C' });
        decoder.Add("Trp", new char[] { 'T', 'G', 'G' });
        decoder.Add("Arg", new char[] { 'C', 'G', 'C' });
        decoder.Add("Gly", new char[] { 'G', 'G', 'G' });
    }

    Material ColorConversion(char c)
    {
        switch (c)
        {
            case 'A':
                return mats[0];
            case 'T':
                return mats[1];
            case 'G':
                return mats[2];
            case 'C':
                return mats[3];
        }

        return null;
    }

    Material ColorConversionGlow(char c)
    {
        switch (c)
        {
            case 'A':
                return mats[4];
            case 'T':
                return mats[5];
            case 'G':
                return mats[6];
            case 'C':
                return mats[7];
        }

        return null;
    }

    void RandomizeLabels()
    {
        btnLabels = btnLabels.OrderBy(x => rnd.Next()).ToArray();

        for (int i = 0; i < 4; i++)
        {
            btns[i].GetComponentInChildren<TextMesh>().text = btnLabels[i].ToString();
            btns[i].GetComponentInChildren<Renderer>().material = ColorConversion(btnLabels[i]);
        }

        Debug.LogFormat("[Genetic Sequence #{0}] The button labels are {1}{2}{3}{4}.", moduleId, btnLabels[0], btnLabels[1], btnLabels[2], btnLabels[3]);
    }

    void CalcStartingAA()
    {
        if (btnLabels[0] == 'A' && btnLabels[1] == 'C' && btnLabels[2] == 'G' && btnLabels[3] == 'T')
            AASequence[0] = "Trp";
        else if ((btnLabels[0] == 'A' && btnLabels[1] == 'C' && btnLabels[2] == 'T') || (btnLabels[1] == 'A' && btnLabels[2] == 'C' && btnLabels[3] == 'T'))
            AASequence[0] = "Gly";
        else if (btnLabels[3] == 'G')
            AASequence[0] = "Tyr";
        else if (btnLabels[0] == 'T')
            AASequence[0] = "Cys";
        else if ((Array.FindIndex(btnLabels, c => c == 'C') < Array.FindIndex(btnLabels, a => a == 'A')) && (Array.FindIndex(btnLabels, c => c == 'C') < Array.FindIndex(btnLabels, t => t == 'T')))
            AASequence[0] = "Arg";
        else if (Math.Abs(Array.FindIndex(btnLabels, a => a == 'A') - Array.FindIndex(btnLabels, t => t == 'T')) == 1)
            AASequence[0] = "Leu";
        else if (Math.Abs(Array.FindIndex(btnLabels, c => c == 'C') - Array.FindIndex(btnLabels, g => g == 'G')) == 1)
            AASequence[0] = "Ala";
        else
            AASequence[0] = "Asn";

        Debug.LogFormat("[Genetic Sequence #{0}] The starting amino acid is {1}.", moduleId, AASequence[0]);
    }

    void CalcAASequence()
    {
        FollowFirstPath(1, AASequence[0]);
        FollowFirstPath(2, AASequence[1]);
        FollowFirstPath(3, AASequence[2]);
        Debug.LogFormat("[Genetic Sequence #{0}] The amino acid sequence is {1}-{2}-{3}-{4}.", moduleId, AASequence[0], AASequence[1], AASequence[2], AASequence[3]);
    }

    void FollowFirstPath(int pos, string from)
    {
        KeyValuePair<string, string>[] to;
        map.TryGetValue(from, out to);

        if (!FollowLitPath(pos, from, to))
            if (!FollowUnlitPath(pos, from, to))
                FollowDefaultPath(pos, from, to);
    }

    bool FollowLitPath(int pos, string from, KeyValuePair<string, string>[] to)
    {
        for (int i = 0; i < to.Length; i++)
        {
            if (to[i].Key != null && bomb.IsIndicatorOn(to[i].Key))
            {
                AASequence[pos] = to[i].Value;
                RemoveMapPath(from, to[i].Key);
                RemoveMapPath(to[i].Value, to[i].Key);
                return true;
            }
        }

        return false;
    }

    bool FollowUnlitPath(int pos, string from, KeyValuePair<string, string>[] to)
    {
        for (int i = 0; i < to.Length; i++)
        {
            if (to[i].Key != null && bomb.IsIndicatorOff(to[i].Key))
            {
                AASequence[pos] = to[i].Value;
                RemoveMapPath(from, to[i].Key);
                RemoveMapPath(to[i].Value, to[i].Key);
                reverse = !reverse;
                return true;
            }
        }

        return false;
    }

    void FollowDefaultPath(int pos, string from, KeyValuePair<string, string>[] to)
    {
        AASequence[pos] = to[0].Value;
        RemoveMapPath(from, to[0].Key);
        RemoveMapPath(to[0].Value, to[0].Key);
    }

    void RemoveMapPath(string from, string path)
    {
        KeyValuePair<string, string>[] to;
        map.TryGetValue(from, out to);

        KeyValuePair<string, string>[] new_to = new KeyValuePair<string, string>[to.Length];
        int pos = 0;

        foreach (KeyValuePair<string, string> dest in to)
        {
            if (dest.Key != null && !dest.Key.Equals(path))
            {
                new_to[pos] = dest;
                pos++;
            }
        }

        map.Remove(from);
        map.Add(from, new_to);
    }

    void CalcSolution()
    {
        int pos = 0;

        foreach (string aa in AASequence)
        {
            char[] seq;
            decoder.TryGetValue(aa, out seq);

            foreach (char b in seq)
            {
                solution[pos] = b;
                pos++;
            }
        }

        if (reverse)
            ReverseSolution();

        Debug.LogFormat("[Genetic Sequence #{0}] The solution is {1}{2}{3}-{4}{5}{6}-{7}{8}{9}-{10}{11}{12}.", moduleId, solution[0], solution[1], solution[2], solution[3], solution[4], solution[5], solution[6], solution[7], solution[8], solution[9], solution[10], solution[11]);
    }

    void ReverseSolution()
    {
        for (int i = 0; i < solution.Length; i++)
        {
            switch (solution[i])
            {
                case 'A':
                {
                    solution[i] = 'T';
                    break;
                }
                case 'T':
                {
                    solution[i] = 'A';
                    break;
                }
                case 'C':
                {
                    solution[i] = 'G';
                    break;
                }
                case 'G':
                {
                    solution[i] = 'C';
                    break;
                }
            }
        }

        Array.Reverse(solution);
    }

    void SolutionCheckResult()
    {
        if (CheckSolution())
        {
            Debug.LogFormat("[Genetic Sequence #{0}] Solved! The input sequence was {1}{2}{3}-{4}{5}{6}-{7}{8}{9}-{10}{11}{12}.", moduleId, input[0], input[1], input[2], input[3], input[4], input[5], input[6], input[7], input[8], input[9], input[10], input[11]);
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();

            Audio.PlaySoundAtTransform("correct", transform);
            foreach (Renderer entry in boxes[1].GetComponentsInChildren<Renderer>())
            {
                entry.material = mats[10];
            }
        }
        else
        {
            Debug.LogFormat("[Genetic Sequence #{0}] Strike! The input sequence was {1}{2}{3}-{4}{5}{6}-{7}{8}{9}-{10}{11}{12}.", moduleId, input[0], input[1], input[2], input[3], input[4], input[5], input[6], input[7], input[8], input[9], input[10], input[11]);
            inputPos = 0;
            input = new char[12];
            GetComponent<KMBombModule>().HandleStrike();

            foreach (Renderer entry in boxes[1].GetComponentsInChildren<Renderer>())
            {
                entry.material = mats[11];
            }
        }
    }

    bool CheckSolution()
    {
        for (int i = 0; i < solution.Length; i++)
        {
            if (solution[i] != input[i])
                return false;
        }

        return true;
    }

    IEnumerator CheckingAnimation()
    {
        for (int i = 0; i < 12; i++)
        {
            Audio.PlaySoundAtTransform("check", transform);
            boxes[0].GetComponentsInChildren<Renderer>()[i].material = mats[8];
            boxes[1].GetComponentsInChildren<Renderer>()[i].material = mats[9];
            yield return new WaitForSeconds(0.1f);
        }

        SolutionCheckResult();

        if (!moduleSolved)
        {
            yield return new WaitForSeconds(1f);
            foreach (Renderer entry in boxes[1].GetComponentsInChildren<Renderer>())
            {
                entry.material = mats[8];
            }
        }

        checking = false;
    }


#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} 1234OC [O = OK, C = CLR, buttons are 1–4 in reading order]";
#pragma warning restore 414

    public List<KMSelectable> ProcessTwitchCommand(string command)
    {
        var presses = new List<KMSelectable>();
        for (int i = 0; i < command.Length; i++)
        {
            if (command[i] >= '1' && command[i] <= '4')
                presses.Add(btns[command[i] - '1']);
            else if (command[i] == 'O' || command[i] == 'o')
                presses.Add(btns[4]);
            else if (command[i] == 'C' || command[i] == 'c')
                presses.Add(btns[5]);
            else if (!char.IsWhiteSpace(command[i]) && command[i] != ',' && command[i] != ';')
                return null;
        }
        return presses;
    }
}
