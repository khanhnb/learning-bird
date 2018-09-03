using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour {

    public delegate void GameDelegate();
    public event GameDelegate OnDie;

    public float tapForce = 250;
    public float tiltSmooth = 2;
    public int score = 0;
    public bool isDead = false;
    public Vector3 startPos;
    Rigidbody2D bird2D;
    Quaternion downRotation;
    Quaternion forwardRotation;


    void Start () {
        bird2D = GetComponent<Rigidbody2D>();
        downRotation = Quaternion.Euler(0, 0, -90);
        forwardRotation = Quaternion.Euler(0, 0, 30);
    }
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
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
            isDead = true;
            OnDie();
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
}
