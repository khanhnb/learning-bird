using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public delegate void GameDelegate();
    public static event GameDelegate OnReset;

    public static readonly Vector3 initPos = Vector3.one * 100;
    readonly Vector3 startPos = new Vector3(-3.36F, -0.25F, 9.826611F);


	public static GameManager Instance;
	public Text scoreText;
    public GameObject Prefab;
    public int NUM_BIRDS = 2;
    List<GameObject> birdPool;
    int deadBirds = 0;
	void Awake(){
		Instance = this;
        birdPool = new List<GameObject>();
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
        if(Input.GetMouseButtonDown(0)){
            birdPool[0].GetComponent<Bird>().Flap();
        }
    }

    void OnDie(int index)
    {
        deadBirds++;
        birdPool[index].GetComponent<Bird>().transform.position = initPos;
        Debug.Log("Number of dead birds: " + deadBirds);
        if (deadBirds == NUM_BIRDS)
        {
            OnReset();
            StartGame();
        }

    }
    void StartGame(){
        deadBirds = 0;
        for (int i = 0; i < NUM_BIRDS; i++)
        {
            var bird = birdPool[i].GetComponent<Bird>();
            bird.transform.position = startPos;
            bird.isDead = false;
            bird.SetSimulated(true);
        }
    }
}
