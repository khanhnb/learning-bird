using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour {

    public delegate void GameDelegate(int index);
    public event GameDelegate OnDie;

    public float tapForce = 250;
    public float tiltSmooth = 2;
    public int score = 0;
    public bool isDead = false;

    int index;

    Rigidbody2D bird2D;
    Quaternion downRotation;
    Quaternion forwardRotation;

    void Awake()
    {
        bird2D = GetComponent<Rigidbody2D>();
        downRotation = Quaternion.Euler(0, 0, -90);
        forwardRotation = Quaternion.Euler(0, 0, 30);
    }
	
	// Update is called once per frame
	void Update () {
        if(!isDead) transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
    }

    public void Flap(){
        Debug.Log("Flapped");
        transform.rotation = forwardRotation;
        bird2D.velocity = Vector3.zero;
        bird2D.AddForce(Vector2.up * tapForce, ForceMode2D.Force);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "DeadZone")
        {
            bird2D.simulated = false;
            bird2D.velocity = Vector2.zero;
            bird2D.transform.position = GameManager.initPos;
            isDead = true;
            OnDie(index);
            Debug.Log("DeadZone");
        }
        if (col.gameObject.tag == "ScoreZone")
        {
            score++;
            Debug.Log("ScoreZone");
        }
    }

    public void SetSimulated(bool a){
        bird2D.simulated = a;
    }

    public void SetIndex(int a){
        index = a;
    }
}
