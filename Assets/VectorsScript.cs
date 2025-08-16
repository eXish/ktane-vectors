using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class VectorsScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;

    public Text display;

    public KMSelectable button;
    public TextMesh buttonDisp;

    private string[] pickedRColors;
    public Renderer ring;

    public Renderer led1;
    public Renderer led2;
    public Renderer led3;
    public Material notdone;
    public Material correct;

    private Coroutine graphMov;
    public GameObject graph;
    public GameObject ypostext;
    public GameObject ynegtext;
    public GameObject xpostext;
    public GameObject xnegtext;
    public GameObject zpostext;
    public GameObject znegtext;
    private float ry1;
    private float ry2;
    private float ry3;
    private float r2y1;
    private float r2y2;
    private float r2y3;
    private float rx1;
    private float rx2;
    private float rx3;
    private float r2x1;
    private float r2x2;
    private float r2x3;
    private float rz1;
    private float rz2;
    private float rz3;
    private float r2z1;
    private float r2z2;
    private float r2z3;

    public GameObject btnDispObj;
    public GameObject btnConnectorObj;

    private double secondNum;

    private double necessary;

    private int held;
    private bool holding;

    public Material[] vectorColors;
    public GameObject[] vecCyls;
    public GameObject[] vecTops;
    private string[] colors;
    private double[] magnitudes;
    private double[] xcomps;
    private double[] ycomps;
    private double[] zcomps;
    private int[] vectorsPicked;
    private int vectorct;

    private string[] autoscroller;
    private Coroutine autoscroll;

    private string missingcomp;

    private Coroutine ringSeq;
    private Color original;

    private Coroutine time;

    private int ans;
    //private int streak;

    private bool unicorn;
    //private bool breaker;
    private bool animating;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        //streak = 1;
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        button.OnInteract += delegate () { PressButton(button); return false; };
        button.OnInteractEnded += delegate () { ReleaseButton(button); };
        graphMov = StartCoroutine(graphMovement());
        setRotFloats();
        StartCoroutine(textRot());
    }

    void Start () {
        hideAllVectors();
        holding = false;
        unicorn = false;
        //breaker = false;
        ans = 0;
        necessary = 0;
        secondNum = 0;
        buttonDisp.text = "0";
        missingcomp = "";
        pickedRColors = new string[3];
        autoscroller = new string[3];
        colors = new string[24];
        magnitudes = new double[24];
        xcomps = new double[24];
        ycomps = new double[24];
        zcomps = new double[24];
        vectorsPicked = new int[3];
        randomizeRingColors();
        ringSeq = StartCoroutine(ringSequence());
        Debug.LogFormat("[Vectors #{0}] -------------------------------------", moduleId);
        Debug.LogFormat("[Vectors #{0}] Generating Vectors...", moduleId);
        Debug.LogFormat("[Vectors #{0}] -------------------------------------", moduleId);
        int rand = UnityEngine.Random.Range(0, 3);
        if(rand == 0)
        {
            vectorct = 1;
        }else if (rand == 1)
        {
            vectorct = 2;
        }else if (rand == 2)
        {
            vectorct = 3;
        }
        Debug.LogFormat("[Vectors #{0}] Number of Vectors: {1}", moduleId, vectorct);
        randomizeVectors();
        if(unicorn != true)
        {
            finalCalc();
        }
    }

    void PressButton(KMSelectable pressed)
    {
        if(moduleSolved != true && !holding)
        {
            pressed.AddInteractionPunch(0.5f);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, pressed.transform);
            holding = true;
            time = StartCoroutine(timer());
            StartCoroutine(downButton());
        }
    }

    void ReleaseButton(KMSelectable pressed)
    {
        if (moduleSolved != true && holding)
        {
            pressed.AddInteractionPunch(0.25f);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, pressed.transform);
            holding = false;
            StartCoroutine(upButton());
            StopCoroutine(ringSeq);
            if(autoscroll != null)
            {
                StopCoroutine(autoscroll);
            }
            ring.material.color = original;
            if (held == ans)
            {
                //these were in unicorn if originally
                moduleSolved = true;
                led1.material = correct;
                led2.material = correct;
                led3.material = correct;
                display.text = "";
                hideAllVectors();
                StopCoroutine(graphMov);
                StartCoroutine(solvedGraph());
                if (unicorn == true)
                {
                    Debug.LogFormat("[Vectors #{0}] Unicorn successfully performed! Module Disarmed!", moduleId);
                }
                //this was not here originally
                else
                {
                    Debug.LogFormat("[Vectors #{0}] The button was held for the correct amount of time of {1} seconds! Module Disarmed!", moduleId, ans);
                }
                /*else if (streak == 3)
                {
                    moduleSolved = true;
                    led3.material = correct;
                    display.text = "";
                    hideAllVectors();
                    StopCoroutine(graphMov);
                    StartCoroutine(solvedGraph());
                    Debug.LogFormat("[Vectors #{0}] Stage 3 was correct! Module Disarmed!", moduleId);
                }
                else
                {
                    Debug.LogFormat("[Vectors #{0}] The button was held for the correct amount of time of {1} seconds! Advancing to Stage {2}!", moduleId, ans, streak+1);
                    if(streak == 1)
                    {
                        led1.material = correct;
                    }else if (streak == 2)
                    {
                        led2.material = correct;
                    }
                    streak++;
                    Start();
                }*/
            }
            else
            {
                //Debug.LogFormat("[Vectors #{0}] The button was held for an incorrect amount of time of {1} seconds! Strike! Resetting Stage {2}!", moduleId, held, streak);
                Debug.LogFormat("[Vectors #{0}] The button was held for an incorrect amount of time of {1} seconds! Strike! Resetting!", moduleId, held);
                GetComponent<KMBombModule>().HandleStrike();
                Start();
            }
        }
    }

    private void hideAllVectors()
    {
        for(int i = 0; i < vecCyls.Length; i++)
        {
            vecCyls[i].GetComponent<Renderer>().enabled = false;
            vecTops[i].GetComponent<Renderer>().enabled = false;
        }
    }

    private void randomizeVectors()
    {
        for(int i = 0; i < vectorct; i++)
        {
            int rand1 = UnityEngine.Random.Range(0, 8);
            int rand2 = UnityEngine.Random.Range(8, 16);
            int rand3 = UnityEngine.Random.Range(16, 24);
            if(vectorct == 1)
            {
                vectorsPicked[i] = rand1;
                if(rand1 == 0)
                {
                    int ran = UnityEngine.Random.Range(0, 4);
                    if (ran == 0)
                    {
                        missingcomp = "mag";
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 1)
                    {
                        missingcomp = "x";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0){
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        xcomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 2)
                    {
                        missingcomp = "y";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        ycomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 3)
                    {
                        missingcomp = "z";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        zcomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)), 1);
                    }
                }
                else if (rand1 == 1)
                {
                    int ran = UnityEngine.Random.Range(0, 4);
                    if (ran == 0)
                    {
                        missingcomp = "mag";
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 1)
                    {
                        missingcomp = "x";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        xcomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 2)
                    {
                        missingcomp = "y";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        ycomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 3)
                    {
                        missingcomp = "z";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        zcomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)), 1);
                    }
                }
                else if (rand1 == 2)
                {
                    int ran = UnityEngine.Random.Range(0, 4);
                    if (ran == 0)
                    {
                        missingcomp = "mag";
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 1)
                    {
                        missingcomp = "x";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        xcomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 2)
                    {
                        missingcomp = "y";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        ycomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 3)
                    {
                        missingcomp = "z";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        zcomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)), 1);
                    }
                }
                else if (rand1 == 3)
                {
                    int ran = UnityEngine.Random.Range(0, 4);
                    if (ran == 0)
                    {
                        missingcomp = "mag";
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 1)
                    {
                        missingcomp = "x";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        xcomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 2)
                    {
                        missingcomp = "y";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        ycomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 3)
                    {
                        missingcomp = "z";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        zcomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)), 1);
                    }
                }
                else if (rand1 == 4)
                {
                    int ran = UnityEngine.Random.Range(0, 4);
                    if (ran == 0)
                    {
                        missingcomp = "mag";
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 1)
                    {
                        missingcomp = "x";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        xcomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 2)
                    {
                        missingcomp = "y";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        ycomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 3)
                    {
                        missingcomp = "z";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        zcomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)), 1);
                    }
                }
                else if (rand1 == 5)
                {
                    int ran = UnityEngine.Random.Range(0, 4);
                    if (ran == 0)
                    {
                        missingcomp = "mag";
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 1)
                    {
                        missingcomp = "x";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        xcomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 2)
                    {
                        missingcomp = "y";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        ycomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 3)
                    {
                        missingcomp = "z";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        zcomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)), 1);
                    }
                }
                else if (rand1 == 6)
                {
                    int ran = UnityEngine.Random.Range(0, 4);
                    if (ran == 0)
                    {
                        missingcomp = "mag";
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 1)
                    {
                        missingcomp = "x";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        xcomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 2)
                    {
                        missingcomp = "y";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                        }
                        ycomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 3)
                    {
                        missingcomp = "z";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        zcomps[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)), 1);
                    }
                }else if (rand1 == 7)
                {
                    int ran = UnityEngine.Random.Range(0, 4);
                    if (ran == 0)
                    {
                        missingcomp = "mag";
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 1)
                    {
                        missingcomp = "x";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        xcomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 2)
                    {
                        missingcomp = "y";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        ycomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (ran == 3)
                    {
                        missingcomp = "z";
                        magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                        xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        while ((Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)) < 0)
                        {
                            magnitudes[vectorsPicked[i]] = UnityEngine.Random.Range(2, 172);
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                        }
                        zcomps[vectorsPicked[i]] = -Math.Round(Math.Sqrt(Math.Pow(magnitudes[vectorsPicked[i]], 2) - Math.Pow(xcomps[vectorsPicked[i]], 2) - Math.Pow(ycomps[vectorsPicked[i]], 2)), 1);
                    }
                }
            }
            else if (vectorct > 1)
            {
                if(i == 0)
                {
                    vectorsPicked[i] = rand1;
                    if(rand1 == 0)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = ""+Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand1 == 1)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand1 == 2)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand1 == 3)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand1 == 4)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand1 == 5)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand1 == 6)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand1 == 7)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                }
                else if (i == 1)
                {
                    vectorsPicked[i] = rand2;
                    if (rand2 == 8)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand2 == 9)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand2 == 10)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand2 == 11)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand2 == 12)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand2 == 13)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand2 == 14)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand2 == 15)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                }
                else if (i == 2)
                {
                    vectorsPicked[i] = rand3;
                    if (rand3 == 16)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand3 == 17)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand3 == 18)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand3 == 19)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand3 == 20)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand3 == 21)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand3 == 22)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(1, 100);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                    else if (rand3 == 23)
                    {
                        string temp = ".";
                        while (temp.Contains('.'))
                        {
                            xcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            ycomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            zcomps[vectorsPicked[i]] = UnityEngine.Random.Range(-99, 0);
                            temp = "" + Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                        }
                        magnitudes[vectorsPicked[i]] = Math.Round(Math.Sqrt(Math.Pow(xcomps[vectorsPicked[i]], 2) + Math.Pow(ycomps[vectorsPicked[i]], 2) + Math.Pow(zcomps[vectorsPicked[i]], 2)), 1);
                    }
                }
            }
            vecCyls[vectorsPicked[i]].GetComponent<Renderer>().enabled = true;
            vecTops[vectorsPicked[i]].GetComponent<Renderer>().enabled = true;
            int colrand = UnityEngine.Random.Range(0, 6);
            if(colrand == 0)
            {
                colors[vectorsPicked[i]] = "Red";
                vecCyls[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[0];
                vecTops[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[0];
            }else if (colrand == 1)
            {
                colors[vectorsPicked[i]] = "Orange";
                vecCyls[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[1];
                vecTops[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[1];
            }else if (colrand == 2)
            {
                colors[vectorsPicked[i]] = "Yellow";
                vecCyls[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[2];
                vecTops[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[2];
            }else if (colrand == 3)
            {
                colors[vectorsPicked[i]] = "Green";
                vecCyls[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[3];
                vecTops[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[3];
            }else if (colrand == 4)
            {
                colors[vectorsPicked[i]] = "Blue";
                vecCyls[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[4];
                vecTops[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[4];
            }else if (colrand == 5)
            {
                colors[vectorsPicked[i]] = "Purple";
                vecCyls[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[5];
                vecTops[vectorsPicked[i]].GetComponent<Renderer>().material = vectorColors[5];
            }
        }
        bool bluevec = false;
        for(int j = 0; j < vectorct; j++)
        {
            if (colors[vectorsPicked[j]].EqualsIgnoreCase("Blue"))
            {
                bluevec = true;
            }
        }
        if (bomb.GetBatteryCount() == 2 && bomb.GetPortPlateCount() > 2 && bomb.IsIndicatorPresent("SND") && bluevec == true)
        {
            unicorn = true;
            ans = 0;
            string[] randomtext = { "Do you really need vector info for this?", "Oh boy its one of these scenarios", "Um... what?", "Yay unicorns! :)" };
            int rand = UnityEngine.Random.Range(0, 4);
            display.text = randomtext[rand];
            Debug.LogFormat("[Vectors #{0}] Unicorn conditions met (including a blue colored vector)! Button must be held for 0 seconds!", moduleId);
            return;
        }
        if(vectorct == 1)
        {
            double missing = 0;
            if(missingcomp.Equals("mag"))
            {
                Debug.LogFormat("[Vectors #{0}] The Vector data values are... | Color: {1} | Magnitude: {2} | X-Component: {3} | Y-Component: {4} | Z-Component: {5}", moduleId, colors[vectorsPicked[0]], "?", xcomps[vectorsPicked[0]], ycomps[vectorsPicked[0]], zcomps[vectorsPicked[0]]);
                Debug.LogFormat("[Vectors #{0}] The missing data value is 'Magnitude', the equation M = sqrt(x^2 + y^2 + z^2) must be used", moduleId);
                Debug.LogFormat("[Vectors #{0}] Equation Substitution: M = sqrt(({1})^2 + ({2})^2 + ({3})^2)", moduleId, xcomps[vectorsPicked[0]], ycomps[vectorsPicked[0]], zcomps[vectorsPicked[0]]);
                Debug.LogFormat("[Vectors #{0}] The solution to the equation is {1}, making this the missing 'Magnitude' value", moduleId, magnitudes[vectorsPicked[0]]);
                missing = magnitudes[vectorsPicked[0]];
                display.text = "Vector 1 ("+colors[vectorsPicked[0]]+") Magnitude: ? X-Component: "+ Math.Abs(xcomps[vectorsPicked[0]])+" Y-Component: "+ Math.Abs(ycomps[vectorsPicked[0]]) +" Z-Component: "+ Math.Abs(zcomps[vectorsPicked[0]]);
            }
            else if (missingcomp.Equals("x"))
            {
                Debug.LogFormat("[Vectors #{0}] The Vector data values are... | Color: {1} | Magnitude: {2} | X-Component: {3} | Y-Component: {4} | Z-Component: {5}", moduleId, colors[vectorsPicked[0]], magnitudes[vectorsPicked[0]], "?", ycomps[vectorsPicked[0]], zcomps[vectorsPicked[0]]);
                Debug.LogFormat("[Vectors #{0}] The missing data value is 'X-Component', the equation A = sqrt(M^2 - B^2 - C^2) must be used", moduleId);
                Debug.LogFormat("[Vectors #{0}] Equation Substitution: A = sqrt(({1})^2 - ({2})^2 - ({3})^2)", moduleId, magnitudes[vectorsPicked[0]], ycomps[vectorsPicked[0]], zcomps[vectorsPicked[0]]);
                Debug.LogFormat("[Vectors #{0}] The solution to the equation is {1}, making this the missing 'X-Component' value", moduleId, xcomps[vectorsPicked[0]]);
                missing = xcomps[vectorsPicked[0]];
                display.text = "Vector 1 (" + colors[vectorsPicked[0]] + ") Magnitude: "+magnitudes[vectorsPicked[0]]+" X-Component: ? Y-Component: " + Math.Abs(ycomps[vectorsPicked[0]]) + " Z-Component: " + Math.Abs(zcomps[vectorsPicked[0]]);
            }
            else if (missingcomp.Equals("y"))
            {
                Debug.LogFormat("[Vectors #{0}] The Vector data values are... | Color: {1} | Magnitude: {2} | X-Component: {3} | Y-Component: {4} | Z-Component: {5}", moduleId, colors[vectorsPicked[0]], magnitudes[vectorsPicked[0]], xcomps[vectorsPicked[0]], "?", zcomps[vectorsPicked[0]]);
                Debug.LogFormat("[Vectors #{0}] The missing data value is 'Y-Component', the equation A = sqrt(M^2 - B^2 - C^2) must be used", moduleId);
                Debug.LogFormat("[Vectors #{0}] Equation Substitution: A = sqrt(({1})^2 - ({2})^2 - ({3})^2)", moduleId, magnitudes[vectorsPicked[0]], xcomps[vectorsPicked[0]], zcomps[vectorsPicked[0]]);
                Debug.LogFormat("[Vectors #{0}] The solution to the equation is {1}, making this the missing 'Y-Component' value", moduleId, ycomps[vectorsPicked[0]]);
                missing = ycomps[vectorsPicked[0]];
                display.text = "Vector 1 (" + colors[vectorsPicked[0]] + ") Magnitude: " + magnitudes[vectorsPicked[0]] + " X-Component: "+ Math.Abs(xcomps[vectorsPicked[0]])+" Y-Component: ? Z-Component: " + Math.Abs(zcomps[vectorsPicked[0]]);
            }
            else if (missingcomp.Equals("z"))
            {
                Debug.LogFormat("[Vectors #{0}] The Vector data values are... | Color: {1} | Magnitude: {2} | X-Component: {3} | Y-Component: {4} | Z-Component: {5}", moduleId, colors[vectorsPicked[0]], magnitudes[vectorsPicked[0]], xcomps[vectorsPicked[0]], ycomps[vectorsPicked[0]], "?");
                Debug.LogFormat("[Vectors #{0}] The missing data value is 'Z-Component', the equation A = sqrt(M^2 - B^2 - C^2) must be used", moduleId);
                Debug.LogFormat("[Vectors #{0}] Equation Substitution: A = sqrt(({1})^2 - ({2})^2 - ({3})^2)", moduleId, magnitudes[vectorsPicked[0]], xcomps[vectorsPicked[0]], ycomps[vectorsPicked[0]]);
                Debug.LogFormat("[Vectors #{0}] The solution to the equation is {1}, making this the missing 'Z-Component' value", moduleId, zcomps[vectorsPicked[0]]);
                missing = zcomps[vectorsPicked[0]];
                display.text = "Vector 1 (" + colors[vectorsPicked[0]] + ") Magnitude: " + magnitudes[vectorsPicked[0]] + " X-Component: " + Math.Abs(xcomps[vectorsPicked[0]]) + " Y-Component: "+ Math.Abs(ycomps[vectorsPicked[0]])+" Z-Component: ?";
            }
            if (colors[vectorsPicked[0]].Equals("Red"))
            {
                Debug.LogFormat("[Vectors #{0}] Because the Vector's color is Red, the second number is calculated with ('# of batteries' * 5) + 6", moduleId);
                secondNum = (bomb.GetBatteryCount() * 5) + 3;
                Debug.LogFormat("[Vectors #{0}] Equation substitution: ({1} * 5) + 3 => {2} is the second number", moduleId, bomb.GetBatteryCount(), secondNum);
            }else if (colors[vectorsPicked[0]].Equals("Orange"))
            {
                Debug.LogFormat("[Vectors #{0}] Because the Vector's color is Orange, the second number is calculated with 'missing data value'^3 + 16 - '# of RCA ports'", moduleId);
                secondNum = Math.Pow(missing, 3) + 16 - portCount("StereoRCA");
                secondNum = Math.Round(secondNum, 1);
                Debug.LogFormat("[Vectors #{0}] Equation substitution: ({1})^3 + 16 - {2} => {3} is the second number", moduleId, missing, portCount("StereoRCA"), secondNum);
            }else if (colors[vectorsPicked[0]].Equals("Yellow"))
            {
                Debug.LogFormat("[Vectors #{0}] Because the Vector's color is Yellow, the second number is calculated with (('# of battery holders' * 14) % 5) + 1", moduleId);
                secondNum = (bomb.GetBatteryHolderCount() * 14 % 5) + 1;
                Debug.LogFormat("[Vectors #{0}] Equation substitution: (({1} * 14) % 5) + 1 => {2} is the second number", moduleId, bomb.GetBatteryHolderCount(), secondNum);
            }else if (colors[vectorsPicked[0]].Equals("Green"))
            {
                Debug.LogFormat("[Vectors #{0}] Because the Vector's color is Green, the second number is calculated with '# of RJ ports' + 204", moduleId);
                secondNum = portCount("RJ45") + 204;
                Debug.LogFormat("[Vectors #{0}] Equation substitution: {1} + 204 => {2} is the second number", moduleId, portCount("RJ45"), secondNum);
            }else if (colors[vectorsPicked[0]].Equals("Blue"))
            {
                Debug.LogFormat("[Vectors #{0}] Because the Vector's color is Blue, the second number is calculated with 8 * ((5 + 'vector's magnitude') + 6)", moduleId);
                secondNum = 8 * (5 + magnitudes[vectorsPicked[0]] + 6);
                secondNum = Math.Round(secondNum, 1);
                Debug.LogFormat("[Vectors #{0}] Equation substitution: 8 * ((5 + {1}) + 6) => {2} is the second number", moduleId, magnitudes[vectorsPicked[0]], secondNum);
            }else if (colors[vectorsPicked[0]].Equals("Purple"))
            {
                Debug.LogFormat("[Vectors #{0}] Because the Vector's color is Purple, the second number is calculated with ('vector's z-component' + 6) % 3", moduleId);
                secondNum = (zcomps[vectorsPicked[0]] + 6) % 3;
                if(secondNum < 0)
                {
                    secondNum += 3;
                }
                secondNum = Math.Round(secondNum, 1);
                Debug.LogFormat("[Vectors #{0}] Equation substitution: ({1} + 6) % 3 => {2} is the second number", moduleId, zcomps[vectorsPicked[0]], secondNum);
            }
        }
        else if(vectorct == 2)
        {
            autoscroller[0] = "Vector 1 (" + colors[vectorsPicked[0]] + ") Magnitude: " + magnitudes[vectorsPicked[0]] + " X-Component: "+Math.Abs(xcomps[vectorsPicked[0]])+" Y-Component: " + Math.Abs(ycomps[vectorsPicked[0]]) + " Z-Component: " + Math.Abs(zcomps[vectorsPicked[0]]);
            autoscroller[1] = "Vector 2 (" + colors[vectorsPicked[1]] + ") Magnitude: " + magnitudes[vectorsPicked[1]] + " X-Component: " + Math.Abs(xcomps[vectorsPicked[1]]) + " Y-Component: " + Math.Abs(ycomps[vectorsPicked[1]]) + " Z-Component: " + Math.Abs(zcomps[vectorsPicked[1]]);
            autoscroll = StartCoroutine(cycleInfo());
            Debug.LogFormat("[Vectors #{0}] Vector 1's data values are... | Color: {1} | Magnitude: {2} | X-Component: {3} | Y-Component: {4} | Z-Component: {5}", moduleId, colors[vectorsPicked[0]], magnitudes[vectorsPicked[0]], xcomps[vectorsPicked[0]], ycomps[vectorsPicked[0]], zcomps[vectorsPicked[0]]);
            Debug.LogFormat("[Vectors #{0}] Vector 2's data values are... | Color: {1} | Magnitude: {2} | X-Component: {3} | Y-Component: {4} | Z-Component: {5}", moduleId, colors[vectorsPicked[1]], magnitudes[vectorsPicked[1]], xcomps[vectorsPicked[1]], ycomps[vectorsPicked[1]], zcomps[vectorsPicked[1]]);
        }
        else if (vectorct == 3)
        {
            autoscroller[0] = "Vector 1 (" + colors[vectorsPicked[0]] + ") Magnitude: " + magnitudes[vectorsPicked[0]] + " X-Component: " + Math.Abs(xcomps[vectorsPicked[0]]) + " Y-Component: " + Math.Abs(ycomps[vectorsPicked[0]]) + " Z-Component: " + Math.Abs(zcomps[vectorsPicked[0]]);
            autoscroller[1] = "Vector 2 (" + colors[vectorsPicked[1]] + ") Magnitude: " + magnitudes[vectorsPicked[1]] + " X-Component: " + Math.Abs(xcomps[vectorsPicked[1]]) + " Y-Component: " + Math.Abs(ycomps[vectorsPicked[1]]) + " Z-Component: " + Math.Abs(zcomps[vectorsPicked[1]]);
            autoscroller[2] = "Vector 3 (" + colors[vectorsPicked[2]] + ") Magnitude: " + magnitudes[vectorsPicked[2]] + " X-Component: " + Math.Abs(xcomps[vectorsPicked[2]]) + " Y-Component: " + Math.Abs(ycomps[vectorsPicked[2]]) + " Z-Component: " + Math.Abs(zcomps[vectorsPicked[2]]);
            autoscroll = StartCoroutine(cycleInfo());
            Debug.LogFormat("[Vectors #{0}] Vector 1's data values are... | Color: {1} | Magnitude: {2} | X-Component: {3} | Y-Component: {4} | Z-Component: {5}", moduleId, colors[vectorsPicked[0]], magnitudes[vectorsPicked[0]], xcomps[vectorsPicked[0]], ycomps[vectorsPicked[0]], zcomps[vectorsPicked[0]]);
            Debug.LogFormat("[Vectors #{0}] Vector 2's data values are... | Color: {1} | Magnitude: {2} | X-Component: {3} | Y-Component: {4} | Z-Component: {5}", moduleId, colors[vectorsPicked[1]], magnitudes[vectorsPicked[1]], xcomps[vectorsPicked[1]], ycomps[vectorsPicked[1]], zcomps[vectorsPicked[1]]);
            Debug.LogFormat("[Vectors #{0}] Vector 3's data values are... | Color: {1} | Magnitude: {2} | X-Component: {3} | Y-Component: {4} | Z-Component: {5}", moduleId, colors[vectorsPicked[2]], magnitudes[vectorsPicked[2]], xcomps[vectorsPicked[2]], ycomps[vectorsPicked[2]], zcomps[vectorsPicked[2]]);
        }
        if(vectorct > 1)
        {
            int priCount = 0;
            int secCount = 0;
            for (int i = 0; i < vectorct; i++)
            {
                if (colors[vectorsPicked[i]].EqualsIgnoreCase("Red") || colors[vectorsPicked[i]].EqualsIgnoreCase("Yellow") || colors[vectorsPicked[i]].EqualsIgnoreCase("Blue"))
                {
                    priCount++;
                }
                else
                {
                    secCount++;
                }
            }
            double total = 0;
            if (priCount > secCount)
            {
                Debug.LogFormat("[Vectors #{0}] There are more vectors with primary colors then secondary", moduleId);
                if (vectorct == 2)
                {
                    double tot = 0;
                    tot = magnitudes[vectorsPicked[0]] + magnitudes[vectorsPicked[1]];
                    Debug.LogFormat("[Vectors #{0}] The sum of the vector's magnitudes is {1}", moduleId, tot);
                    double tot2 = 0;
                    foreach (int n in bomb.GetSerialNumberNumbers())
                    {
                        tot2 += n;
                    }
                    Debug.LogFormat("[Vectors #{0}] The sum of the serial number's digits is {1}", moduleId, tot2);
                    total = 0;
                    total = tot * tot2;
                    Debug.LogFormat("[Vectors #{0}] The necessary number is {1} * {2} => {3}", moduleId, tot, tot2, total);
                }
                else if (vectorct == 3)
                {
                    double tot = 0;
                    tot = magnitudes[vectorsPicked[0]] + magnitudes[vectorsPicked[1]] + magnitudes[vectorsPicked[2]];
                    Debug.LogFormat("[Vectors #{0}] The sum of the vector's magnitudes is {1}", moduleId, tot);
                    double tot2 = 0;
                    foreach (int n in bomb.GetSerialNumberNumbers())
                    {
                        tot2 += n;
                    }
                    Debug.LogFormat("[Vectors #{0}] The sum of the serial number's digits is {1}", moduleId, tot2);
                    total = 0;
                    total = tot * tot2;
                    Debug.LogFormat("[Vectors #{0}] The necessary number is {1} * {2} => {3}", moduleId, tot, tot2, total);
                }
            }
            else if (priCount < secCount)
            {
                Debug.LogFormat("[Vectors #{0}] There are more vectors with secondary colors then primary", moduleId);
                if (vectorct == 2)
                {
                    double tot = 0;
                    tot = magnitudes[vectorsPicked[0]] * magnitudes[vectorsPicked[1]];
                    Debug.LogFormat("[Vectors #{0}] The multiplication of the vector's magnitudes resulted in {1}", moduleId, tot);
                    double num = 0;
                    bool firstit = true;
                    foreach (int n in bomb.GetSerialNumberNumbers())
                    {
                        if (firstit == true)
                        {
                            firstit = false;
                            num = n;
                        }
                    }
                    Debug.LogFormat("[Vectors #{0}] The first digit of the serial number is {1}", moduleId, num);
                    if (num == 0)
                    {
                        num = 1;
                        Debug.LogFormat("[Vectors #{0}] Because the first serial number digit is 0, 1 will be substituted instead", moduleId);
                    }
                    total = 0;
                    total = Math.Round(tot / num, 1);
                    Debug.LogFormat("[Vectors #{0}] The necessary number is {1} / {2} => {3}", moduleId, tot, num, total);
                }
                else if (vectorct == 3)
                {
                    double tot = 0;
                    tot = magnitudes[vectorsPicked[0]] * magnitudes[vectorsPicked[1]] * magnitudes[vectorsPicked[2]];
                    Debug.LogFormat("[Vectors #{0}] The multiplication of the vector's magnitudes resulted in {1}", moduleId, tot);
                    double num = 0;
                    bool firstit = true;
                    foreach (int n in bomb.GetSerialNumberNumbers())
                    {
                        if (firstit == true)
                        {
                            firstit = false;
                            num = n;
                        }
                    }
                    Debug.LogFormat("[Vectors #{0}] The first digit of the serial number is {1}", moduleId, num);
                    if (num == 0)
                    {
                        num = 1;
                        Debug.LogFormat("[Vectors #{0}] Because the first serial number digit is 0, 1 will be substituted instead", moduleId);
                    }
                    total = 0;
                    total = Math.Round(tot / num, 1);
                    Debug.LogFormat("[Vectors #{0}] The necessary number is {1} / {2} => {3}", moduleId, tot, num, total);
                }
            }
            else
            {
                Debug.LogFormat("[Vectors #{0}] There is an equal amount of vectors that have primary and secondary colors", moduleId);
                double temp = 0;
                double.TryParse(bomb.GetSerialNumber().Substring(bomb.GetSerialNumber().Length-1, 1), out temp);
                temp *= temp;
                Debug.LogFormat("[Vectors #{0}] The last digit of the serial number squared is {1}", moduleId, temp);
                double xcmp = 0;
                for (int j = 0; j < vectorct; j++)
                {
                    if (colors[vectorsPicked[j]].EqualsIgnoreCase("Green") || colors[vectorsPicked[j]].EqualsIgnoreCase("Orange") || colors[vectorsPicked[j]].EqualsIgnoreCase("Purple"))
                    {
                        xcmp = xcomps[vectorsPicked[j]];
                    }
                }
                Debug.LogFormat("[Vectors #{0}] The x-component of the vector with a secondary color is {1}", moduleId, xcmp);
                total = 0;
                total = temp + xcmp;
                Debug.LogFormat("[Vectors #{0}] The necessary number is {1} + {2} => {3}", moduleId, temp, xcmp, total);
            }
            necessary = total;
            if(vectorct == 2)
            {
                double sumx = xcomps[vectorsPicked[0]] + xcomps[vectorsPicked[1]];
                double sumy = ycomps[vectorsPicked[0]] + ycomps[vectorsPicked[1]];
                double sumz = zcomps[vectorsPicked[0]] + zcomps[vectorsPicked[1]];
                necessary += sumx;
                necessary += sumy;
                necessary -= sumz;
                Debug.LogFormat("[Vectors #{0}] The sum of the vector's x-components is {1}", moduleId, sumx);
                Debug.LogFormat("[Vectors #{0}] The sum of the vector's y-components is {1}", moduleId, sumy);
                Debug.LogFormat("[Vectors #{0}] The sum of the vector's z-components is {1}", moduleId, sumz);
                Debug.LogFormat("[Vectors #{0}] The new necessary number is ({1} + {2} + {3}) - {4} => {5}", moduleId, total, sumx, sumy, sumz, necessary);
            }else if (vectorct == 3)
            {
                double sumx = xcomps[vectorsPicked[0]] + xcomps[vectorsPicked[1]] + xcomps[vectorsPicked[2]];
                double sumy = ycomps[vectorsPicked[0]] + ycomps[vectorsPicked[1]] + ycomps[vectorsPicked[2]];
                double sumz = zcomps[vectorsPicked[0]] + zcomps[vectorsPicked[1]] + zcomps[vectorsPicked[2]];
                necessary += sumx;
                necessary += sumy;
                necessary -= sumz;
                Debug.LogFormat("[Vectors #{0}] The sum of the vector's x-components is {1}", moduleId, sumx);
                Debug.LogFormat("[Vectors #{0}] The sum of the vector's y-components is {1}", moduleId, sumy);
                Debug.LogFormat("[Vectors #{0}] The sum of the vector's z-components is {1}", moduleId, sumz);
                Debug.LogFormat("[Vectors #{0}] The new necessary number is ({1} + {2} + {3}) - {4} => {5}", moduleId, total, sumx, sumy, sumz, necessary);
            }
        }
    }

    private IEnumerator cycleInfo()
    {
        if (vectorct == 2)
        {
            display.text = autoscroller[0];
            yield return new WaitForSeconds(1.5f);
            display.text = autoscroller[1];
            yield return new WaitForSeconds(1.5f);
        }
        else if (vectorct == 3)
        {
            display.text = autoscroller[0];
            yield return new WaitForSeconds(1.5f);
            display.text = autoscroller[1];
            yield return new WaitForSeconds(1.5f);
            display.text = autoscroller[2];
            yield return new WaitForSeconds(1.5f);
        }
        StopCoroutine(autoscroll);
        autoscroll = StartCoroutine(cycleInfo());
    }

    private int portCount(string s)
    {
        int count = 0;
        for(int i = 0; i < bomb.GetPortCount(); i++)
        {
            if (bomb.GetPorts().ElementAt(i).Equals(s))
            {
                count++;
            }
        }
        return count;
    }
    private void finalCalc()
    {
        Debug.LogFormat("[Vectors #{0}] ----Final Calculation----", moduleId);
        double s = 0;
        if(vectorct == 1)
        {
            if(missingcomp == "mag")
            {
                s = magnitudes[vectorsPicked[0]] + secondNum;
                Debug.LogFormat("[Vectors #{0}] S = missing data value + second number => {1} + {2} => {3}", moduleId, magnitudes[vectorsPicked[0]], secondNum, s);
            }
            else if (missingcomp == "x")
            {
                s = xcomps[vectorsPicked[0]] + secondNum;
                Debug.LogFormat("[Vectors #{0}] S = missing data value + second number => {1} + {2} => {3}", moduleId, xcomps[vectorsPicked[0]], secondNum, s);
            }
            else if (missingcomp == "y")
            {
                s = ycomps[vectorsPicked[0]] + secondNum;
                Debug.LogFormat("[Vectors #{0}] S = missing data value + second number => {1} + {2} => {3}", moduleId, ycomps[vectorsPicked[0]], secondNum, s);
            }
            else if (missingcomp == "z")
            {
                s = zcomps[vectorsPicked[0]] + secondNum;
                Debug.LogFormat("[Vectors #{0}] S = missing data value + second number => {1} + {2} => {3}", moduleId, zcomps[vectorsPicked[0]], secondNum, s);
            }
        }else if (vectorct > 1)
        {
            double arrowmods = 0;
            for(int i = 0; i < bomb.GetModuleNames().Count; i++)
            {
                if (bomb.GetModuleNames().ElementAt(i).Equals("Red Arrows") || bomb.GetModuleNames().ElementAt(i).Equals("Orange Arrows") || bomb.GetModuleNames().ElementAt(i).Equals("Brown Arrows") || bomb.GetModuleNames().ElementAt(i).Equals("Yellow Arrows") || bomb.GetModuleNames().ElementAt(i).Equals("Green Arrows") || bomb.GetModuleNames().ElementAt(i).Equals("Teal Arrows") || bomb.GetModuleNames().ElementAt(i).Equals("Blue Arrows") || bomb.GetModuleNames().ElementAt(i).Equals("Purple Arrows"))
                {
                    arrowmods++;
                }
            }
            s = necessary + arrowmods;
            Debug.LogFormat("[Vectors #{0}] S = necessary number + '# of Arrow modules by eXish' => {1} + {2} => {3}", moduleId, necessary, arrowmods, s);
        }
        for(int i = 0; i < 3; i++)
        {
            Debug.LogFormat("[Vectors #{0}] Ring {1} is {2}", moduleId, i+1, pickedRColors[i]);
            if (pickedRColors[i].Equals("red"))
            {
                s += 122;
            }
            else if (pickedRColors[i].Equals("blue"))
            {
                s += (bomb.GetPortPlateCount() + bomb.GetIndicators().Count());
            }
            else if (pickedRColors[i].Equals("green"))
            {
                s -= digitalRoot(bomb.GetModuleNames().Count);
            }
            else if (pickedRColors[i].Equals("white"))
            {
                s -= 27;
            }
            Debug.LogFormat("[Vectors #{0}] After applying the rule for ring color {1}, S is now {2}", moduleId, pickedRColors[i], s);
        }
        string temp = "" + s;
        if (temp.Contains('.'))
        {
            temp = temp.Substring(0, temp.IndexOf('.'));
        }
        double.TryParse(temp, out s);
        double step1 = s;
        s = Math.Abs(s);
        double step2 = s;
        s %= 15;
        double step3 = s;
        s += 1;
        double step4 = s;
        ans = (int)s;
        Debug.LogFormat("[Vectors #{0}] S removed decimals = {1} | S absolute value = {2} | S % 15 = {3} | S + 1 = {4}", moduleId, step1, step2, step3, step4);
        Debug.LogFormat("[Vectors #{0}] The calculated time to hold the button for is {1} seconds", moduleId, ans);
    }

    private int digitalRoot(int dig)
    {
        string combo = "" + dig;
        while (combo.Length > 1)
        {
            int total = 0;
            for (int i = 0; i < combo.Length; i++)
            {
                int temp = 0;
                int.TryParse(combo.Substring(i, 1), out temp);
                total += temp;
            }
            combo = total + "";
        }
        int temp2 = 0;
        int.TryParse(combo, out temp2);
        return temp2;
    }

    private void randomizeRingColors()
    {
        for(int i = 0; i < 3; i++)
        {
            int rand = UnityEngine.Random.Range(0, 4);
            switch (rand)
            {
                case 0: pickedRColors[i] = "red"; break;
                case 1: pickedRColors[i] = "green"; break;
                case 2: pickedRColors[i] = "blue"; break;
                case 3: pickedRColors[i] = "white"; break;
            }
        }
    }

    private void setRotFloats()
    {
        float[] validfloats = { -0.50f, -0.40f, -0.30f, 0.30f, 0.40f, 0.50f };
        int y1;
        int y2;
        int y3;
        int ny1;
        int ny2;
        int ny3;
        int x1;
        int x2;
        int x3;
        int nx1;
        int nx2;
        int nx3;
        int z1;
        int z2;
        int z3;
        int nz1;
        int nz2;
        int nz3;
        y1 = UnityEngine.Random.Range(0, 6);
        y2 = UnityEngine.Random.Range(0, 6);
        y3 = UnityEngine.Random.Range(0, 6);
        ny1 = UnityEngine.Random.Range(0, 6);
        ny2 = UnityEngine.Random.Range(0, 6);
        ny3 = UnityEngine.Random.Range(0, 6);
        x1 = UnityEngine.Random.Range(0, 6);
        x2 = UnityEngine.Random.Range(0, 6);
        x3 = UnityEngine.Random.Range(0, 6);
        nx1 = UnityEngine.Random.Range(0, 6);
        nx2 = UnityEngine.Random.Range(0, 6);
        nx3 = UnityEngine.Random.Range(0, 6);
        z1 = UnityEngine.Random.Range(0, 6);
        z2 = UnityEngine.Random.Range(0, 6);
        z3 = UnityEngine.Random.Range(0, 6);
        nz1 = UnityEngine.Random.Range(0, 6);
        nz2 = UnityEngine.Random.Range(0, 6);
        nz3 = UnityEngine.Random.Range(0, 6);
        switch (y1)
        {
            case 0: ry1 = validfloats[0]; break;
            case 1: ry1 = validfloats[1]; break;
            case 2: ry1 = validfloats[2]; break;
            case 3: ry1 = validfloats[3]; break;
            case 4: ry1 = validfloats[4]; break;
            case 5: ry1 = validfloats[5]; break;
            default: ry1 = 0; break;
        }
        switch (y2)
        {
            case 0: ry2 = validfloats[0]; break;
            case 1: ry2 = validfloats[1]; break;
            case 2: ry2 = validfloats[2]; break;
            case 3: ry2 = validfloats[3]; break;
            case 4: ry2 = validfloats[4]; break;
            case 5: ry2 = validfloats[5]; break;
            default: ry2 = 0; break;
        }
        switch (y3)
        {
            case 0: ry3 = validfloats[0]; break;
            case 1: ry3 = validfloats[1]; break;
            case 2: ry3 = validfloats[2]; break;
            case 3: ry3 = validfloats[3]; break;
            case 4: ry3 = validfloats[4]; break;
            case 5: ry3 = validfloats[5]; break;
            default: ry3 = 0; break;
        }
        switch (ny1)
        {
            case 0: r2y1 = validfloats[0]; break;
            case 1: r2y1 = validfloats[1]; break;
            case 2: r2y1 = validfloats[2]; break;
            case 3: r2y1 = validfloats[3]; break;
            case 4: r2y1 = validfloats[4]; break;
            case 5: r2y1 = validfloats[5]; break;
            default: r2y1 = 0; break;
        }
        switch (ny2)
        {
            case 0: r2y2 = validfloats[0]; break;
            case 1: r2y2 = validfloats[1]; break;
            case 2: r2y2 = validfloats[2]; break;
            case 3: r2y2 = validfloats[3]; break;
            case 4: r2y2 = validfloats[4]; break;
            case 5: r2y2 = validfloats[5]; break;
            default: r2y2 = 0; break;
        }
        switch (ny3)
        {
            case 0: r2y3 = validfloats[0]; break;
            case 1: r2y3 = validfloats[1]; break;
            case 2: r2y3 = validfloats[2]; break;
            case 3: r2y3 = validfloats[3]; break;
            case 4: r2y3 = validfloats[4]; break;
            case 5: r2y3 = validfloats[5]; break;
            default: r2y3 = 0; break;
        }
        switch (x1)
        {
            case 0: rx1 = validfloats[0]; break;
            case 1: rx1 = validfloats[1]; break;
            case 2: rx1 = validfloats[2]; break;
            case 3: rx1 = validfloats[3]; break;
            case 4: rx1 = validfloats[4]; break;
            case 5: rx1 = validfloats[5]; break;
            default: rx1 = 0; break;
        }
        switch (x2)
        {
            case 0: rx2 = validfloats[0]; break;
            case 1: rx2 = validfloats[1]; break;
            case 2: rx2 = validfloats[2]; break;
            case 3: rx2 = validfloats[3]; break;
            case 4: rx2 = validfloats[4]; break;
            case 5: rx2 = validfloats[5]; break;
            default: rx2 = 0; break;
        }
        switch (x3)
        {
            case 0: rx3 = validfloats[0]; break;
            case 1: rx3 = validfloats[1]; break;
            case 2: rx3 = validfloats[2]; break;
            case 3: rx3 = validfloats[3]; break;
            case 4: rx3 = validfloats[4]; break;
            case 5: rx3 = validfloats[5]; break;
            default: rx3 = 0; break;
        }
        switch (nx1)
        {
            case 0: r2x1 = validfloats[0]; break;
            case 1: r2x1 = validfloats[1]; break;
            case 2: r2x1 = validfloats[2]; break;
            case 3: r2x1 = validfloats[3]; break;
            case 4: r2x1 = validfloats[4]; break;
            case 5: r2x1 = validfloats[5]; break;
            default: r2x1 = 0; break;
        }
        switch (nx2)
        {
            case 0: r2x2 = validfloats[0]; break;
            case 1: r2x2 = validfloats[1]; break;
            case 2: r2x2 = validfloats[2]; break;
            case 3: r2x2 = validfloats[3]; break;
            case 4: r2x2 = validfloats[4]; break;
            case 5: r2x2 = validfloats[5]; break;
            default: r2x2 = 0; break;
        }
        switch (nx3)
        {
            case 0: r2x3 = validfloats[0]; break;
            case 1: r2x3 = validfloats[1]; break;
            case 2: r2x3 = validfloats[2]; break;
            case 3: r2x3 = validfloats[3]; break;
            case 4: r2x3 = validfloats[4]; break;
            case 5: r2x3 = validfloats[5]; break;
            default: r2x3 = 0; break;
        }
        switch (z1)
        {
            case 0: rz1 = validfloats[0]; break;
            case 1: rz1 = validfloats[1]; break;
            case 2: rz1 = validfloats[2]; break;
            case 3: rz1 = validfloats[3]; break;
            case 4: rz1 = validfloats[4]; break;
            case 5: rz1 = validfloats[5]; break;
            default: rz1 = 0; break;
        }
        switch (z2)
        {
            case 0: rz2 = validfloats[0]; break;
            case 1: rz2 = validfloats[1]; break;
            case 2: rz2 = validfloats[2]; break;
            case 3: rz2 = validfloats[3]; break;
            case 4: rz2 = validfloats[4]; break;
            case 5: rz2 = validfloats[5]; break;
            default: rz2 = 0; break;
        }
        switch (z3)
        {
            case 0: rz3 = validfloats[0]; break;
            case 1: rz3 = validfloats[1]; break;
            case 2: rz3 = validfloats[2]; break;
            case 3: rz3 = validfloats[3]; break;
            case 4: rz3 = validfloats[4]; break;
            case 5: rz3 = validfloats[5]; break;
            default: rz3 = 0; break;
        }
        switch (nz1)
        {
            case 0: r2z1 = validfloats[0]; break;
            case 1: r2z1 = validfloats[1]; break;
            case 2: r2z1 = validfloats[2]; break;
            case 3: r2z1 = validfloats[3]; break;
            case 4: r2z1 = validfloats[4]; break;
            case 5: r2z1 = validfloats[5]; break;
            default: r2z1 = 0; break;
        }
        switch (nz2)
        {
            case 0: r2z2 = validfloats[0]; break;
            case 1: r2z2 = validfloats[1]; break;
            case 2: r2z2 = validfloats[2]; break;
            case 3: r2z2 = validfloats[3]; break;
            case 4: r2z2 = validfloats[4]; break;
            case 5: r2z2 = validfloats[5]; break;
            default: r2z2 = 0; break;
        }
        switch (nz3)
        {
            case 0: r2z3 = validfloats[0]; break;
            case 1: r2z3 = validfloats[1]; break;
            case 2: r2z3 = validfloats[2]; break;
            case 3: r2z3 = validfloats[3]; break;
            case 4: r2z3 = validfloats[4]; break;
            case 5: r2z3 = validfloats[5]; break;
            default: r2z3 = 0; break;
        }
    }

    private IEnumerator upButton()
    {
        int movement = 0;
        while (movement != 10)
        {
            yield return new WaitForSeconds(0.0001f);
            button.transform.localPosition = button.transform.localPosition + Vector3.up * 0.001f;
            btnConnectorObj.transform.localPosition = btnConnectorObj.transform.localPosition + Vector3.up * 0.001f;
            btnDispObj.transform.localPosition = btnDispObj.transform.localPosition + Vector3.up * 0.001f;
            movement++;
        }
        StopCoroutine("upButton");
    }

    private IEnumerator downButton()
    {
        int movement = 0;
        while (movement != 10)
        {
            yield return new WaitForSeconds(0.0001f);
            button.transform.localPosition = button.transform.localPosition + Vector3.up * -0.001f;
            btnConnectorObj.transform.localPosition = btnConnectorObj.transform.localPosition + Vector3.up * -0.001f;
            btnDispObj.transform.localPosition = btnDispObj.transform.localPosition + Vector3.up * -0.001f;
            movement++;
        }
        StopCoroutine("downButton");
    }

    private IEnumerator ringSequence()
    {
        yield return new WaitForSeconds(1.0f);
        while (moduleSolved != true)
        {
            original = ring.material.color;
            Color temp = original;
            if (pickedRColors[0].Equals("white"))
            {
                while(temp.r < 1.0f || temp.b < 1.0f || temp.g < 1.0f)
                {
                    temp.r += 0.01f;
                    temp.b += 0.01f;
                    temp.g += 0.01f;
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r > original.r || temp.b > original.b || temp.g > original.g)
                {
                    temp.r -= 0.01f;
                    temp.b -= 0.01f;
                    temp.g -= 0.01f;
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (pickedRColors[0].Equals("red"))
            {
                while (temp.r < 1.0f || temp.b > 0.0f || temp.g > 0.0f)
                {
                    temp.r += 0.01f;
                    if(temp.b > 0.0f)
                    {
                        temp.b -= 0.01f;
                    }
                    if (temp.g > 0.0f)
                    {
                        temp.g -= 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r > original.r || temp.b < original.b || temp.g < original.g)
                {
                    temp.r -= 0.01f;
                    if (temp.b < original.b)
                    {
                        temp.b += 0.01f;
                    }
                    if (temp.g < original.g)
                    {
                        temp.g += 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (pickedRColors[0].Equals("blue"))
            {
                while (temp.r > 0.0f || temp.b < 1.0f || temp.g > 0.0f)
                {
                    temp.b += 0.01f;
                    if (temp.r > 0.0f)
                    {
                        temp.r -= 0.01f;
                    }
                    if (temp.g > 0.0f)
                    {
                        temp.g -= 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r < original.r || temp.b > original.b || temp.g < original.g)
                {
                    temp.b -= 0.01f;
                    if (temp.r < original.r)
                    {
                        temp.r += 0.01f;
                    }
                    if (temp.g < original.g)
                    {
                        temp.g += 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (pickedRColors[0].Equals("green"))
            {
                while (temp.r > 0.0f || temp.b > 0.0f || temp.g < 1.0f)
                {
                    temp.g += 0.01f;
                    if (temp.b > 0.0f)
                    {
                        temp.b -= 0.01f;
                    }
                    if (temp.r > 0.0f)
                    {
                        temp.r -= 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r < original.r || temp.b < original.b || temp.g > original.g)
                {
                    temp.g -= 0.01f;
                    if (temp.b < original.b)
                    {
                        temp.b += 0.01f;
                    }
                    if (temp.r < original.r)
                    {
                        temp.r += 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            yield return new WaitForSeconds(1.0f);
            if (pickedRColors[1].Equals("white"))
            {
                while (temp.r < 1.0f || temp.b < 1.0f || temp.g < 1.0f)
                {
                    temp.r += 0.01f;
                    temp.b += 0.01f;
                    temp.g += 0.01f;
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r > original.r || temp.b > original.b || temp.g > original.g)
                {
                    temp.r -= 0.01f;
                    temp.b -= 0.01f;
                    temp.g -= 0.01f;
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (pickedRColors[1].Equals("red"))
            {
                while (temp.r < 1.0f || temp.b > 0.0f || temp.g > 0.0f)
                {
                    temp.r += 0.01f;
                    if (temp.b > 0.0f)
                    {
                        temp.b -= 0.01f;
                    }
                    if (temp.g > 0.0f)
                    {
                        temp.g -= 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r > original.r || temp.b < original.b || temp.g < original.g)
                {
                    temp.r -= 0.01f;
                    if (temp.b < original.b)
                    {
                        temp.b += 0.01f;
                    }
                    if (temp.g < original.g)
                    {
                        temp.g += 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (pickedRColors[1].Equals("blue"))
            {
                while (temp.r > 0.0f || temp.b < 1.0f || temp.g > 0.0f)
                {
                    temp.b += 0.01f;
                    if (temp.r > 0.0f)
                    {
                        temp.r -= 0.01f;
                    }
                    if (temp.g > 0.0f)
                    {
                        temp.g -= 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r < original.r || temp.b > original.b || temp.g < original.g)
                {
                    temp.b -= 0.01f;
                    if (temp.r < original.r)
                    {
                        temp.r += 0.01f;
                    }
                    if (temp.g < original.g)
                    {
                        temp.g += 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (pickedRColors[1].Equals("green"))
            {
                while (temp.r > 0.0f || temp.b > 0.0f || temp.g < 1.0f)
                {
                    temp.g += 0.01f;
                    if (temp.b > 0.0f)
                    {
                        temp.b -= 0.01f;
                    }
                    if (temp.r > 0.0f)
                    {
                        temp.r -= 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r < original.r || temp.b < original.b || temp.g > original.g)
                {
                    temp.g -= 0.01f;
                    if (temp.b < original.b)
                    {
                        temp.b += 0.01f;
                    }
                    if (temp.r < original.r)
                    {
                        temp.r += 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            yield return new WaitForSeconds(1.0f);
            if (pickedRColors[2].Equals("white"))
            {
                while (temp.r < 1.0f || temp.b < 1.0f || temp.g < 1.0f)
                {
                    temp.r += 0.01f;
                    temp.b += 0.01f;
                    temp.g += 0.01f;
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r > original.r || temp.b > original.b || temp.g > original.g)
                {
                    temp.r -= 0.01f;
                    temp.b -= 0.01f;
                    temp.g -= 0.01f;
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (pickedRColors[2].Equals("red"))
            {
                while (temp.r < 1.0f || temp.b > 0.0f || temp.g > 0.0f)
                {
                    temp.r += 0.01f;
                    if (temp.b > 0.0f)
                    {
                        temp.b -= 0.01f;
                    }
                    if (temp.g > 0.0f)
                    {
                        temp.g -= 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r > original.r || temp.b < original.b || temp.g < original.g)
                {
                    temp.r -= 0.01f;
                    if (temp.b < original.b)
                    {
                        temp.b += 0.01f;
                    }
                    if (temp.g < original.g)
                    {
                        temp.g += 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (pickedRColors[2].Equals("blue"))
            {
                while (temp.r > 0.0f || temp.b < 1.0f || temp.g > 0.0f)
                {
                    temp.b += 0.01f;
                    if (temp.r > 0.0f)
                    {
                        temp.r -= 0.01f;
                    }
                    if (temp.g > 0.0f)
                    {
                        temp.g -= 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r < original.r || temp.b > original.b || temp.g < original.g)
                {
                    temp.b -= 0.01f;
                    if (temp.r < original.r)
                    {
                        temp.r += 0.01f;
                    }
                    if (temp.g < original.g)
                    {
                        temp.g += 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (pickedRColors[2].Equals("green"))
            {
                while (temp.r > 0.0f || temp.b > 0.0f || temp.g < 1.0f)
                {
                    temp.g += 0.01f;
                    if (temp.b > 0.0f)
                    {
                        temp.b -= 0.01f;
                    }
                    if (temp.r > 0.0f)
                    {
                        temp.r -= 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
                while (temp.r < original.r || temp.b < original.b || temp.g > original.g)
                {
                    temp.g -= 0.01f;
                    if (temp.b < original.b)
                    {
                        temp.b += 0.01f;
                    }
                    if (temp.r < original.r)
                    {
                        temp.r += 0.01f;
                    }
                    ring.material.color = temp;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            yield return new WaitForSeconds(3.0f);
        }
        StopCoroutine("ringSequence");
    }

    private IEnumerator solvedGraph()
    {
        animating = true;
        int movement = 0;
        while (movement != 100)
        {
            yield return new WaitForSeconds(0.025f);
            graph.transform.localPosition = graph.transform.localPosition + Vector3.up * -0.001f;
            movement++;
        }
        animating = false;
        GetComponent<KMBombModule>().HandlePass();
        StopCoroutine("solvedGraph");
    }

    private IEnumerator graphMovement()
    {
        int movement = 0;
        float temp = 0.1f;
        while (movement != 35)
        {
            if(movement < 15)
            {
                temp -= 0.0027f;
            }
            else
            {
                temp += 0.002025f;
            }
            yield return new WaitForSeconds(temp);
            graph.transform.localPosition = graph.transform.localPosition + Vector3.up * -0.00015f;
            movement++;
        }
        movement = 0;
        temp = 0.1f;
        while (movement != 35)
        {
            if (movement < 15)
            {
                temp -= 0.0027f;
            }
            else
            {
                temp += 0.002025f;
            }
            yield return new WaitForSeconds(temp);
            graph.transform.localPosition = graph.transform.localPosition + Vector3.up * 0.00015f;
            movement++;
        }
        StopCoroutine(graphMov);
        graphMov = StartCoroutine(graphMovement());
    }

    private IEnumerator textRot()
    {
        int movement = 0;
        while (movement != 180)
        {
            yield return new WaitForSeconds(0.005f);
            ypostext.transform.localRotation = Quaternion.Euler(ry1, 0.0f, 0.0f) * ypostext.transform.localRotation;
            ypostext.transform.localRotation = Quaternion.Euler(0.0f, ry2, 0.0f) * ypostext.transform.localRotation;
            ypostext.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, ry3) * ypostext.transform.localRotation;
            ynegtext.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, r2y1) * ynegtext.transform.localRotation;
            ynegtext.transform.localRotation = Quaternion.Euler(0.0f, r2y2, 0.0f) * ynegtext.transform.localRotation;
            ynegtext.transform.localRotation = Quaternion.Euler(r2y3, 0.0f, 0.0f) * ynegtext.transform.localRotation;

            xpostext.transform.localRotation = Quaternion.Euler(0.0f, rx1, 0.0f) * xpostext.transform.localRotation;
            xpostext.transform.localRotation = Quaternion.Euler(rx2, 0.0f, 0.0f) * xpostext.transform.localRotation;
            xpostext.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, rx3) * xpostext.transform.localRotation;
            xnegtext.transform.localRotation = Quaternion.Euler(r2x1, 0.0f, 0.0f) * xnegtext.transform.localRotation;
            xnegtext.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, r2x2) * xnegtext.transform.localRotation;
            xnegtext.transform.localRotation = Quaternion.Euler(0.0f, r2x3, 0.0f) * xnegtext.transform.localRotation;

            zpostext.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, rz1) * zpostext.transform.localRotation;
            zpostext.transform.localRotation = Quaternion.Euler(rz2, 0.0f, 0.0f) * zpostext.transform.localRotation;
            zpostext.transform.localRotation = Quaternion.Euler(0.0f, rz3, 0.0f) * zpostext.transform.localRotation;
            znegtext.transform.localRotation = Quaternion.Euler(0.0f, r2z1, 0.0f) * znegtext.transform.localRotation;
            znegtext.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, r2z2) * znegtext.transform.localRotation;
            znegtext.transform.localRotation = Quaternion.Euler(r2z3, 0.0f, 0.0f) * znegtext.transform.localRotation;
            movement++;
        }
        StopCoroutine("textRot");
        StartCoroutine(textRot());
    }

    private IEnumerator timer()
    {
        held = 0;
        double held2 = 0;
        while (holding)
        {
            yield return null;
            held2 += Time.deltaTime;
            if(held2 >= 1)
            {
                held2 = 0;
                held += 1;
                if(held <= 99)
                {
                    buttonDisp.text = "" + held;
                }
            }
        }
        if(moduleSolved == true)
        {
            buttonDisp.text = "GG";
        }
        else
        {
            buttonDisp.text = "0";
        }
        StopCoroutine(time);
    }

    //twitch plays
    private bool inputIsValid(string s)
    {
        int temp = 0;
        bool check = int.TryParse(s, out temp);
        if(check == true)
        {
            if(temp >= 0 && temp < 16)
            {
                return true;
            }
        }
        return false;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} hold for <#> [Holds the button for the specifed number of seconds, valid hold times range from 0-15] | !{0} zoom tilt u/d/l/r [Zoom in at different angles on the module to see 3D graph clearly, this is a general TP command]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*hold\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 3)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 3)
            {
                if (Regex.IsMatch(parameters[1], @"^\s*for\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    if (inputIsValid(parameters[2]))
                    {
                        button.OnInteract();
                        while (buttonDisp.text != parameters[2]) yield return null;
                        button.OnInteractEnded();
                        if (moduleSolved)
                            yield return "solve";
                    }
                    else
                    {
                        yield return "sendtochaterror!f The specified time to hold the button for '" + parameters[2] + "' is invalid!";
                    }
                }
                else
                {
                    yield return "sendtochaterror!f Expected the word 'for' but received '" + parameters[1] + "'!";
                }
            }
            else if (parameters.Length == 2)
            {
                if (Regex.IsMatch(parameters[1], @"^\s*for\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return "sendtochaterror Please specify a number of seconds to hold the button for!";
                }
                else
                {
                    yield return "sendtochaterror!f Expected the word 'for' but received '" + parameters[1] + "'!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the word 'for' and a number of seconds to hold the button for!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!moduleSolved)
        {
            if (!holding)
            {
                button.OnInteract();
                if (unicorn)
                    yield return new WaitForSeconds(0.1f);
            }
            else if (held > ans)
            {
                button.AddInteractionPunch(0.25f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform);
                moduleSolved = true;
                StopCoroutine(time);
                StartCoroutine(upButton());
                StopCoroutine(ringSeq);
                if (autoscroll != null)
                {
                    StopCoroutine(autoscroll);
                }
                ring.material.color = original;
                buttonDisp.text = "GG";
                led1.material = correct;
                led2.material = correct;
                led3.material = correct;
                display.text = "";
                hideAllVectors();
                StopCoroutine(graphMov);
                StartCoroutine(solvedGraph());
            }
            while (buttonDisp.text != (ans + "")) yield return null;
            button.OnInteractEnded();
        }
        while (animating) { yield return true; }
    }
}
