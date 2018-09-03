using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public delegate void GameDelegate();
    public static event GameDelegate OnReset;
	//public static event GameDelegate OnGameStarted;
	//public static event GameDelegate OnGameOverConfirmed;

	public static GameManager Instance;
	public Text scoreText;
    public GameObject Prefab;
    public int NUM_BIRDS = 2;
    List<GameObject> birdPool;
    int deadBirds = 0;
	void Awake(){
		Instance = this;
        birdPool = new List<GameObject>();
        Debug.Log("Game manager Started");
        for (int i = 0; i < NUM_BIRDS; i++)
        {
            var go = Instantiate(Prefab) as GameObject;
            var bird = go.GetComponent<Bird>();
            bird.OnDie += OnDie;
            go.transform.position = Vector3.one * 10;
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

    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            birdPool[0].GetComponent<Bird>().Flap();
        }
    }

    void OnDie()
    {
        // TODO: add args in events, 
        // return bird to initialize position;
        deadBirds++;
        Debug.Log("Number of dead birds: " + deadBirds);
        if (deadBirds == NUM_BIRDS) NewGeneration();
    }

    void NewGeneration()
    {
        deadBirds = 0;
        // generate new generation
    }
}
