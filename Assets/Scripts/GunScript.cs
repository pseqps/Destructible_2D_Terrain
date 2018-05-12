using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : MonoBehaviour {

    public GameObject bulType;
    public float forceX;
    public float forceY;

    // Use this for initialization
    void Start () {
       //Fire(bulType);
    }
	
    void Fire (GameObject bullet)
    {
        Transform bulTransf = bullet.GetComponent<Transform>().transform;
        bulTransf.position = gameObject.GetComponent<Transform>().position;// + new Vector3(1f,1f,0)*0.7f;
        GameObject newBullet = Instantiate(bullet);
        // Rigidbody2D bulBody = bullet.GetComponent<Rigidbody2D>();
        //bulBody.AddForce(new Vector2(5, 5), ForceMode2D.Impulse);

        newBullet.GetComponent<Rigidbody2D>().AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
        //newBullet.GetComponent<Rigidbody2D>().AddForce(new Vector2(forceX, forceY * Random.value), ForceMode2D.Impulse);


    }

    // Update is called once per frame
    void Update () {
        /*
        if (Input.GetKeyDown("z"))
        {
           Fire(bulType);
        } 
        */
		if (Input.GetMouseButtonDown(0))
        {
           Fire(bulType);
        }

    }
}
