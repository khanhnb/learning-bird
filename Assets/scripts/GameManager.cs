using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeuralNetwork;
using System.Linq;
using System;
public class GameManager : MonoBehaviour
{
    public delegate void GameDelegate();
    public static event GameDelegate OnReset;
    public static readonly Vector3 initPos = Vector3.one * 100;
    readonly Vector3 startPos = new Vector3(-3.36F, -0.25F, 9.826611F);
    public static GameManager Instance;
    public static Parallaxer.PoolObject[] pipes;
    public Text scoreText;
    public Text generationText;
    public Text aliveText;
    public Text highestScoreText;
    public GameObject Prefab;
    public int NUM_BIRDS;
    public int NUM_CHILD = 1;
    public double ELITISM = 0.2;
    public double RANDOM = 0.2;
    public double MUTATION_RATE = 0.1;
    public double MUTATION_RANGE = 0.5;
    public static int INPUT_SIZE = 5;
    public static int HIDDEN_SIZE = 4;
    public static int OUTPUT_SIZE = 1;
    public static int NUM_HIDDEN = 1;
    List<GameObject> birdPool;
    int deadBirds = 0;
    int generation = 1;
    int highestScore = 0;
    int numWeights = INPUT_SIZE * HIDDEN_SIZE + (NUM_HIDDEN - 1) * HIDDEN_SIZE * HIDDEN_SIZE + HIDDEN_SIZE * OUTPUT_SIZE;
    double[] champion;
    public static float timeScale = 2f;
    void Awake()
    {
        Instance = this;
        birdPool = new List<GameObject>();
        champion = new double[numWeights];
        for (int i = 0; i < NUM_BIRDS; i++)
        {
            var go = Instantiate(Prefab) as GameObject;
            var bird = go.GetComponent<Bird>();
            bird.OnDie += OnDie;
            bird.SetIndex(i);
            go.transform.position = initPos;
            birdPool.Add(go);
        }
        
    }

    void OnEnable()
    {

    }

    void OnDisable()
    {
        for (int i = 0; i < NUM_BIRDS; i++)
        {
            birdPool[i].GetComponent<Bird>().OnDie -= OnDie;
        }
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        Time.timeScale = timeScale;
        var score = 0;
        foreach (var go in birdPool)
        {
            var bird = go.GetComponent<Bird>();
            if (bird.score > score) score = bird.score;
        }
        scoreText.text = "" + score;
    }

    void OnDie(int index)
    {
        deadBirds++;
        aliveText.text = "Alive: " + (NUM_BIRDS - deadBirds) +"/"+NUM_BIRDS;
        birdPool[index].GetComponent<Bird>().transform.position = initPos;
        //Debug.Log("Number of dead birds: " + deadBirds);
        if (deadBirds == NUM_BIRDS)
        {
            // reset pipes
            OnReset();
            // calc fitness and sort birds by fitness
            CalcFitness();
            // create new generation
            var nextGeneration = NewGeneration();
            for (int i = 0; i < nextGeneration.Count; i++) birdPool[i].GetComponent<Bird>().brain.SetWeights(nextGeneration[i]);
            StartGame();
        }
    }
    void StartGame()
    {
        generationText.text = "Generation: " + generation;
        highestScoreText.text = "Highest Score: " + highestScore;
        aliveText.text = "Alive: " + NUM_BIRDS +"/"+NUM_BIRDS;
        scoreText.text = "0";
        deadBirds = 0;
        for (int i = 0; i < NUM_BIRDS; i++)
        {
            var bird = birdPool[i].GetComponent<Bird>();
            bird.transform.position = startPos;
            bird.isDead = false;
            bird.SetSimulated(true);
            //bird.brain.SetWeights(GenerateRandomWeights());
            bird.fitness = 0.01;
            bird.score = 0;
        }
    }
    List<double[]> NewGeneration()
    {
        var weightPool = new List<double[]>();
        var oldWeightPool = new List<double[]>();
        generation++;

        foreach (var go in birdPool)
        {
            var bird = go.GetComponent<Bird>();
            double[] tmp = new double[numWeights];
            // tmp
            Array.Copy(bird.brain.GetWeights(), tmp, numWeights);
            oldWeightPool.Add(tmp);
        }
        //weightPool.Add(champion);
        // keep some best genes
        for (int i = 0; i < Math.Round(ELITISM * NUM_BIRDS); i++)
        {
            var tmp = new double[numWeights];
            Array.Copy(birdPool[i].GetComponent<Bird>().brain.GetWeights(), tmp, numWeights);
            weightPool.Add(tmp);
        }
        // generate some new genes
        for (int i = 0; i < Math.Round(RANDOM * NUM_BIRDS); i++)
        {
            weightPool.Add(GenerateRandomWeights());
        }
        var max = 0;
        while (true)
        {
            // breed two parents
            for (int i = 0; i < max; i++)
            {
                var childs = Breed(oldWeightPool[i], oldWeightPool[max]);
                foreach (var child in childs)
                {
                    weightPool.Add(child);
                    if (weightPool.Count >= NUM_BIRDS) return weightPool;
                }
            }
            max++;
            if (max >= NUM_BIRDS) max = 0;
        }
        // var childs = Breed(oldWeightPool[0], oldWeightPool[1]);
        // foreach(var child in childs){
        //     weightPool.Add(child);
        //     if(weightPool.Count >= NUM_BIRDS) return weightPool;
        // }
        // return weightPool;
    }

    void CalcFitness()
    {
        // normalize fitness
        double sum = 0.0;
        birdPool = birdPool.OrderBy(d => -d.GetComponent<Bird>().fitness).ToList();
        var birdNo1 = birdPool[0].GetComponent<Bird>();
        if (highestScore < birdNo1.score)
        {
            highestScore = birdNo1.score;
            Array.Copy(birdNo1.brain.GetWeights(), champion, numWeights);
        }

        birdPool.Sum(a => a.GetComponent<Bird>().fitness);
        for (int i = 0; i < birdPool.Count; i++)
        {
            var bird = birdPool[i].GetComponent<Bird>();
            bird.fitness /= sum;
            bird.SetIndex(i);
        }
    }

    public double[] GenerateRandomWeights()
    {
        var weights = new double[numWeights];
        for (int i = 0; i < numWeights; i++)
        {
            weights[i] = NeuralNet.GetRandom();
        }
        return weights;
    }

    public List<double[]> Breed(double[] m, double[] f)
    {
        var childs = new List<double[]>();
        var random = new System.Random();
        for (int j = 0; j < NUM_CHILD; j++)
        {
            var offspring = new double[m.Length];
            Array.Copy(m, offspring, offspring.Length);

            for (int i = 0; i < offspring.Length; i++)
            {
                if (random.NextDouble() <= 0.5) offspring[i] = f[i];
                if (random.NextDouble() <= MUTATION_RATE)
                {
                    var tmp = offspring[i] + MUTATION_RANGE * NeuralNet.GetRandom();
                    if (tmp > 1) offspring[i] = 1;
                    else if (tmp < -1) offspring[i] = -1;
                    else offspring[i] += MUTATION_RANGE * NeuralNet.GetRandom();
                }
            }
            childs.Add(offspring);
        }

        return childs;
    }
}
