using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : MonoBehaviour {

    public GameObject bulType;

    // Use this for initialization
    void Start () {

    }
	
    void Fire (GameObject bullet)
    {
        //размещаем снаряд под объектом
        bullet.transform.position = gameObject.GetComponent<Transform>().position + Vector3.down * gameObject.transform.localScale.y / 2;
        //и создаем его
        Instantiate(bullet);     
    }

    private void OnMouseDown()
    {
        //при нажатии - дырявим поверхность. У объекта должен быть коллайдер
        Fire(bulType);
    }



    // Update is called once per frame
    void Update () {
        //перемещаем за курсором
        gameObject.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
    }
}
