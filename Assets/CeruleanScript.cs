using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class CeruleanScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Renderer[] segs;
    public Material[] io;

    private readonly bool[,] disp = new bool[16, 7]
    {
        { true, true, true, false, true, true, true },
        { false, false, true, false, false, true, false},
        { true, false, true, true, true, false, true},
        { true, false, true, true, false, true, true},
        { false, true, true, true, false, true, false},
        { true, true, false, true, false, true, true},
        { true, true, false, true, true, true, true},
        { true, false, true, false, false, true, false},
        { true, true, true, true, true, true, true},
        { true, true, true, true, false, true, true},
        { true, true, true, true, true, true, false},
        { false, true, false, true, true, true, true},
        { true, true, false, false, true, false, true},
        { false, false, true, true, true, true, true},
        { true, true, false, true, true, false, true},
        { true, true, false, true, true, false, false}
    };
    private readonly string alph = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private Vector2Int[] pos = new Vector2Int[3];
    private int z = 1;
    private int m;
    private bool doorkey;
    private bool key;
    private List<Vector2Int> visited = new List<Vector2Int> { };
    private bool[] dir = new bool[4];

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private bool[] Gate(bool[] a, bool[] b, int o)
    {
        bool[] c = new bool[16];
        for (int i = 0; i < 16; i++)
            switch (o)
            {
                case 0: c[i] = a[i] && b[i]; break;
                case 2: c[i] = a[i] || b[i]; break;
                case 4: c[i] = a[i] ^ b[i]; break;
                default: c[i] = !a[i] || b[i]; break;
            }
        return c;
    }

    private void GenerateMaze()
    {
        string[] op = new string[8] { "&nbsp;aNd&nbsp;", "&nbsp;NaNd&nbsp;", "&nbsp;OR&nbsp;", "&nbsp;NOR&nbsp;", "&nbsp;XOR&nbsp;", "&nbsp;XNOR&nbsp;", "&nbsp;IMP&nbsp;", "&nbsp;NIMP&nbsp;" };
        List<string> statements = new List<string> { "A", "B", "C", "D" };
        List<bool[]> truth = new List<bool[]> { new bool[16], new bool[16], new bool[16], new bool[16] };
        for (int j = 0; j < 16; j++)
        {
            bool[] s = new bool[] { j >= 8, (j / 4) % 2 == 1, (j / 2) % 2 == 1, j % 2 == 1 };
            for (int k = 0; k < 4; k++)
                truth[k][j] = s[k];
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (i == j)
                    continue;
                for (int k = i < j ? 0 : 6; k < 8; k++)
                {
                    statements.Add(statements[i] + op[k] + statements[j]);
                    if (k % 2 == 0)
                        truth.Add(Gate(truth[i], truth[j], k));
                    else
                        truth.Add(truth.Last().Select(x => !x).ToArray());
                }
            }
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 4; j < 64; j++)
            {
                if (statements[j].Contains(statements[i]))
                    continue;
                for (int k = 0; k < 8; k++)
                {
                    statements.Add(statements[i] + op[k] + "(" + statements[j] + ")");
                    if (k % 2 == 0)
                        truth.Add(Gate(truth[i], truth[j], k));
                    else
                        truth.Add(truth.Last().Select(x => !x).ToArray());
                }
            }
        }
        for (int i = 4; i < 64; i++)
        {
            for (int j = 4; j < 64; j++)
            {
                if (statements[j].Any(x => "ABCD".Contains(x.ToString()) && statements[i].Contains(x.ToString())))
                    continue;
                for (int k = i < j ? 0 : 6; k < 8; k++)
                {
                    statements.Add("(" + statements[i] + ")" + op[k] + "(" + statements[j] + ")");
                    if (k % 2 == 0)
                        truth.Add(Gate(truth[i], truth[j], k));
                    else
                        truth.Add(truth.Last().Select(x => !x).ToArray());
                }
            }
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 64; j < 1024; j++)
            {
                if (statements[j].Contains(statements[i]))
                    continue;
                for (int k = 0; k < 8; k++)
                {
                    statements.Add(statements[i] + op[k] + "(" + statements[j] + ")");
                    if (k % 2 == 0)
                        truth.Add(Gate(truth[i], truth[j], k));
                    else
                        truth.Add(truth.Last().Select(x => !x).ToArray());
                }
            }
        }
        for (int i = 0; i < statements.Count(); i++)
        {
            if (truth[i].Count(x => x) > 12 || truth[i].Count(x => x) < 4 || truth.Take(i).Any(x => x.SequenceEqual(truth[i])))
            {
                statements.RemoveAt(i);
                truth.RemoveAt(i);
                i--;
            }
        }
        int[][] r = new int[2][];
        r[0] = Enumerable.Range(0, 2290).ToArray().Shuffle().ToArray();
        r[1] = r[0].TakeLast(1296).ToArray().Shuffle().ToArray();
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 36; j++)
            {
                string d = "<tr><th class=\"g\">" + "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[j] + "</th>";
                string p = "{ ";
                for (int k = 0; k < 36; k++)
                {
                    int q = (36 * j) + k;
                    d += "<td>" + statements[r[i][q]] + "</td>";
                    p += "{ " + string.Join(", ", truth[r[i][q]].Select(x => x ? "true" : "false").ToArray()) + "},";
                }
                d += "</tr>";
                p += "},";
                Debug.Log(d);
                Debug.Log(p);
            }
        }
    }

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        int[] sn = info.GetSerialNumber().Select(x => alph.IndexOf(x)).ToArray();
        pos[0] = new Vector2Int(sn[0], sn[1]);
        pos[1] = new Vector2Int(sn[2] + sn[3] < 36 ? sn[2] + sn[3] : Mathf.Max(sn[2], sn[3]), sn[4] + sn[5] < 36 ? sn[4] + sn[5] : Mathf.Max(sn[4], sn[5]));
        Debug.Log(string.Join(" ", pos.Take(2).Select(x => "(" + x.x + "," + x.y + ")").ToArray()));
        if (pos[0].x != pos[1].x || pos[0].y != pos[1].y)
            while (Mathf.Abs(pos[1].x - pos[0].x) + Mathf.Abs(pos[1].y - pos[0].y) <= 5)
            {
                int x = (pos[1].x - pos[0].x);
                int y = (pos[1].y - pos[0].y);
                pos[1] = new Vector2Int((pos[1].x + x) % 36, (pos[1].y + y) % 36);
            }
        else
            doorkey = true;
        int[] c = new int[] { info.GetBatteryHolderCount() % 2, info.GetPortPlateCount() % 2, info.GetBatteryCount() % 2, info.GetPortCount() % 2 };
        m = (c[0] ^ c[1]) ^ (c[2] ^ c[3]);
        Debug.LogFormat("[Cerulean Maze #{0}] The starting location is {1}-{2}.", moduleID, alph[sn[0]], alph[sn[1]]);
        Debug.LogFormat("[Cerulean Maze #{0}] The key location is {1}-{2}.", moduleID, alph[pos[1].x], alph[pos[1].y]);
        pos[2] = pos[0];
        Moves();
        foreach (KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved)
                {
                    button.AddInteractionPunch(0.7f);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    switch (b)
                    {
                        case 6:
                            pos[2] = pos[0];
                            Debug.LogFormat("[Cerulean Maze #{0}] Reset to {1}-{2}.", moduleID, alph[pos[2].x], alph[pos[2].y]);
                            if (key)
                                StartCoroutine(KeyGet());
                            else
                                Moves();
                            break;
                        case 5:
                            if (doorkey && !key ? Mathf.Abs(pos[2].x - pos[1].x) + Mathf.Abs(pos[2].y - pos[1].y) > 9 : pos[2] == pos[1])
                            {
                                if (key)
                                {
                                    moduleSolved = true;
                                    module.HandlePass();
                                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                                    StartCoroutine(Solve());
                                    Debug.LogFormat("[Cerulean Maze #{0}] Exit maze.", moduleID);
                                }
                                else
                                {
                                    key = true;
                                    m ^= 1;
                                    if (doorkey)
                                        pos[0] = pos[2];
                                    else
                                    {
                                        Vector2Int t;
                                        t = pos[0];
                                        pos[0] = pos[1];
                                        pos[1] = t;
                                    }
                                    StartCoroutine(KeyGet());
                                    Debug.LogFormat("[Cerulean Maze #{0}] {1}", moduleID, doorkey ? "Trap disarmed." : "Key obtained.");
                                }
                            }
                            else
                            {
                                module.HandleStrike();
                                Debug.LogFormat("[Cerulean Maze #{0}] Submitted wrong space.", moduleID);
                                Moves();
                            }
                            break;
                        case 4:
                            if (dir.Any(x => x))
                            {
                                module.HandleStrike();
                                Debug.LogFormat("[Cerulean Maze #{0}] Not stuck.", moduleID);
                            }
                            else
                            {
                                Vector2Int k = new Vector2Int(pos[1].x - pos[2].x, pos[1].y - pos[2].y);
                                int x = Mathf.Abs(k.x);
                                int y = Mathf.Abs(k.y);
                                if (x < Mathf.Abs(k.y))
                                {
                                    k.x = 0;
                                    k.y /= y;
                                }
                                else if (x > Mathf.Abs(k.y))
                                {
                                    k.y = 0;
                                    k.x /= x;
                                }
                                else
                                {
                                    k.x /= x;
                                    k.y /= x;
                                }
                                visited.Add(pos[2]);
                                pos[2] += k;
                                Debug.LogFormat("[Cerulean Maze #{0}] Moved to {1}-{2}.", moduleID, alph[pos[2].x], alph[pos[2].y]);
                            }
                            Moves();
                            break;
                        default:
                            if (!dir[b])
                            {
                                module.HandleStrike();
                                Debug.LogFormat("[Cerulean Maze #{0}] Cannot move {1}.", moduleID, new string[] { "Up", "Down", "Left", "Right" }[b]);
                            }
                            else
                            {
                                visited.Add(pos[2]);
                                switch (b)
                                {
                                    case 0: pos[2].x -= z; break;
                                    case 1: pos[2].x += z; break;
                                    case 2: pos[2].y -= z; break;
                                    default: pos[2].y += z; break;
                                }
                                Debug.LogFormat("[Cerulean Maze #{0}] Moved to {1}-{2}.", moduleID, alph[pos[2].x], alph[pos[2].y]);
                            }
                            Moves();
                            break;
                    }
                }
                return false;
            };
        }
    }

    private void Moves()
    {
        if (!doorkey)
        {
            int x = Mathf.Abs(pos[1].x - pos[2].x);
            int y = Mathf.Abs(pos[1].y - pos[2].y);
            z = Mathf.Max(1, ((x + y - 1) / 5) + 1);
        }
        List<int> disps = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        Vector2Int[] spaces = new Vector2Int[4] { new Vector2Int(pos[2].x - z, pos[2].y), new Vector2Int(pos[2].x + z, pos[2].y), new Vector2Int(pos[2].x, pos[2].y - z), new Vector2Int(pos[2].x, pos[2].y + z) };
        bool[][] moves = new bool[4][] { new bool[16], new bool[16], new bool[16], new bool[16] };
        for (int i = 0; i < 4; i++)
        {
            if (spaces[i].x >= 0 && spaces[i].x < 36 && spaces[i].y >= 0 && spaces[i].y < 36)
            {
                for (int j = 0; j < 16; j++)
                    moves[i][j] = Mazes.grids[m, spaces[i].x, spaces[i].y, j];
            }
        }
        bool[] v = new bool[4];
        for (int i = 0; i < 4; i++)
            v[i] = visited.Any(x => x.x == spaces[i].x && x.y == spaces[i].y);
        if (v.Any(x => x) && !v.All(x => x))
            for (int i = 0; i < 16; i++)
                if (moves.Where((x, j) => !x[i] || v[j]).Count() > 3)
                    disps.Remove(i);
        int d = disps.PickRandom();
        for (int i = 0; i < 4; i++)
            dir[i] = spaces[i].x >= 0 && spaces[i].x < 36 && spaces[i].y >= 0 && spaces[i].y < 36 && Mazes.grids[m, spaces[i].x, spaces[i].y, d];
        StartCoroutine(Display(d));
        Debug.LogFormat("[Cerulean Maze #{0}] {1} is displayed. Possible moves: {2}", moduleID, alph[d], dir.All(x => !x) ? "None. Press Stuck." : string.Join(", ", Enumerable.Range(0, 4).Select(x => new string[] { "Up", "Down", "Left", "Right" }[x] + " to " + alph[(spaces[x].x + 36) % 36] + "-" + alph[(spaces[x].y + 36) % 36]).Where((x, i) => dir[i]).ToArray()));
    }

    private IEnumerator Display(int x)
    {
        foreach (Renderer r in segs)
            r.material = io[0];
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < 7; i++)
            segs[i].material = io[disp[x, i] ? 1 : 0];
    }

    private IEnumerator KeyGet()
    {
        Audio.PlaySoundAtTransform("Key", segs[3].transform);
        int[] s = new int[] { 0, 2, 5, 6, 4, 1 };
        foreach (Renderer r in segs)
            r.material = io[0];
        for (int i = 0; i < 24; i++)
        {
            segs[s[i % 6]].material = io[1];
            segs[s[(i + 5) % 6]].material = io[0];
            yield return new WaitForSeconds(1 / 24f);
        }
        Moves();
    }

    private IEnumerator Solve()
    {
        int[] s = new int[] { 0, 2, 3, 4, 6, 5, 3, 1 };
        foreach (Renderer r in segs)
            r.material = io[0];
        int i = 0;
        while (module.gameObject.activeSelf)
        {
            segs[s[i]].material = io[1];
            segs[s[(i + 7) % 8]].material = io[0];
            i++;
            i %= 8;
            yield return new WaitForSeconds(1 / 24f);
        };
    }

    private readonly List<string> comms = new List<string> { "U", "D", "L", "R", "STUCK", "SUBMIT", "RESET", "UP", "DOWN", "LEFT", "RIGHT"};

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} U/D/L/R | !{0} stuck | !{0} submit | !{0} reset";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        int d = comms.IndexOf(command) % 7;
        if(d < 0)
        {
            yield return "sendtochaterror!f " + command + " is not an invalid command.";
            yield break;
        }
        yield return null;
        buttons[d].OnInteract();
    }
}
