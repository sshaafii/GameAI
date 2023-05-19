using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    // Start is called before the first frame update
    public int maxHealth = 100;
    private int currentHealth;
    // public TextMeshProUGUI HealthText;
    [SerializeField] TextMesh healthText;

  //  public Image health;
    //public Slider healthSlider;
    //public Gradient _gradient;

    private void Start()
    {
        

        currentHealth = maxHealth;



    }

    private void Update()
    {
        healthText.text = $"Health : {currentHealth}";
    }

    public void TakeDamage(int damage)
    {


        currentHealth -= damage;
        Debug.Log("Current Health: " + currentHealth);
      //  healthSlider.value = currentHealth;
       // health.color = _gradient.Evaluate(healthSlider.normalizedValue);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healingAmount)
    {
        currentHealth += (int)healingAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"{currentHealth}");
    }

    // New method to get the current health ratio
    public float GetCurrentHealthRatio()
    {
        return (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        // Implement any desired death behavior here, such as playing an animation or destroying the object

        Debug.Log($"{transform.name} was killed!");
       Destroy(gameObject);
       // gameObject.SetActive(false);
    }
}
