using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public int damage = 25;
    public int criticalDamage = 40;


    public float destroyTime = 5f;

    Transform parent;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    public void init(Transform parent)
    {

        this.parent = parent;



    }


    private int rollDice(int rollQuantity, int diceSides)
    {

        int damage = 0;
        for (int i = 0; i < rollQuantity; i++)
        {
            damage += Random.Range(1, diceSides + 1);
        }


        return damage;
    }



    private void OnCollisionEnter(Collision collision)
    {

        if (collision.collider.transform == parent)
            return;


        var target = collision.collider.transform.GetComponent<Health>();

        if (target != null)
        {
            Debug.Log("HIT");



            damage = 0;
            damage = 0 + rollDice(1, 6);
            damage = Mathf.Min(damage, 0 + rollDice(1, 6));
            damage = Mathf.Max(damage, 0 + rollDice(1, 6));
            if (Random.Range(1, 101) < 50)
            {
                damage += 0 + rollDice(1, 6);
                Debug.Log("critical HIT:   " + damage);
                target.TakeDamage(damage);
            }
            else {

                Debug.Log("Normal: " + damage);
                target.TakeDamage(damage);

            }


           // Debug.Log("Normal: " + damage);
           // target.TakeDamage(damage);




            //Debug.Log("HIiiiiiT");
            Destroy(gameObject);
        }

    }
}

