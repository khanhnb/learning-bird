using UnityEngine;
using NeuralNetwork;
using System;

public class Bird : MonoBehaviour
{
    public delegate void GameDelegate(int index);
    public event GameDelegate OnDie;
    public float tapForce = 250;
    public float tiltSmooth = 2;
    public int score = 0;
    public bool isDead = false;
    public NeuralNet brain;
    int index;
    Rigidbody2D bird2D;
    Quaternion downRotation;
    Quaternion forwardRotation;
    float height, width;
    public double fitness = 0.01;
    void Awake()
    {
        bird2D = GetComponent<Rigidbody2D>();
        downRotation = Quaternion.Euler(0, 0, -90);
        forwardRotation = Quaternion.Euler(0, 0, 30);
        brain = new NeuralNet(GameManager.INPUT_SIZE, GameManager.HIDDEN_SIZE, GameManager.OUTPUT_SIZE);

        height = 2f * Camera.main.orthographicSize;
        width = height * Camera.main.aspect;
        var renderer = GetComponent<SpriteRenderer>();
        renderer.color = new Color( UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f );
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            fitness += Time.deltaTime;
            //Debug.Log("fitness: " + fitness);
            transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
            // decide to flap or not

            // calc inputs
            var inputs = new double[GameManager.INPUT_SIZE];
            var nearest = GetNearestPipe();
            //Debug.Log("nearest: " + nearest.position.x);
            // height of bird
            inputs[0] = 2 * bird2D.transform.position.y / height;
            // distance between bird and pipe
            inputs[1] = 2 * (nearest.position.x + 3.6/4) / width;
            
            // inputs[1] = 2 * (nearest.position.x) / width;
            // height of top pipe
            inputs[2] = 2 * (nearest.GetChild(1).position.y - 21.6 / 4.0) / height;
            // height of bottom pipe
            inputs[3] = 2 * (nearest.GetChild(0).position.y + 21.6 / 4.0) / height;

            inputs[4] = bird2D.velocity.y / 8.0;
            // calc output
            //Debug.Log("velocity: " + inputs[4]);
            var outputs = brain.Compute(inputs);
            //Debug.Log(inputs[0] + " " + inputs[1] + " " + inputs[2] + " " + inputs[3]);
            // take action from output
            if (outputs[0] > 0.5) Flap();
        }
    }

    Transform GetNearestPipe()
    {
        Transform nearest = GameManager.pipes[0].transform;
        double min = width;

        foreach (var pipe in GameManager.pipes)
        {
            //var pipeX = pipe.transform.position.x;
            // x of scoreZone
            //var pipeX = pipe.transform.GetChild(2).transform.position.x;
            var pipeX = pipe.transform.position.x + 3.6/4;
            if (pipeX > bird2D.transform.position.x && pipeX < min)
            {
                nearest = pipe.transform;
                min = pipeX;
            }
        }
        return nearest;
    }

    public void Flap()
    {
        transform.rotation = forwardRotation;
        bird2D.velocity = Vector3.zero;
        bird2D.AddForce(Vector2.up * tapForce, ForceMode2D.Force);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "DeadZone")
        {
            bird2D.simulated = false;
            bird2D.velocity = Vector3.zero;
            bird2D.transform.position = GameManager.initPos;
            isDead = true;
            OnDie(index);
        }
        if (col.gameObject.tag == "ScoreZone")
        {
            score++;
        }
    }

    
    public void SetSimulated(bool a)
    {
        bird2D.simulated = a;
    }

    public void SetIndex(int a)
    {
        index = a;
    }
}
