using UnityEngine;
// using Mirror;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine.UI;
using System.Threading.Tasks;
using NullSave.TOCK.Inventory;


public class FishHealth : MonoBehaviourPun
{
    public RectTransform canvas;
    public Slider healthBar;
    public float pull = 25f;
    // [SyncVar]
    [SerializeField] FishEntity fishEntity;
    private float initialStamina;
    private float initialHealth;
    float maxHealth;
    public float currentHealth;
    private void Start()
    {
        maxHealth = 100f;
        initialHealth = maxHealth;
        if (PhotonNetwork.IsMasterClient)
        {
            initialStamina = fishEntity.controller.stamina;
        }
    }

    void Update()
    {
        if (fishEntity.controller == null)
        return;

        if (fishEntity.HookedTo != null)
        {
            Vector3 canvasPosition = new Vector3(transform.position.x, canvas.transform.position.y, transform.position.z);
            canvas.transform.position = canvasPosition;
            Vector3 directionToCamera = Camera.main.transform.position - canvas.transform.position;
            directionToCamera.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
            canvas.rotation = targetRotation;

            // currentHealth = Mathf.Clamp(initialHealth - (initialHealth * (1f - fishEntity.controller.HealthBar)), 0f, maxHealth);
            if (fishEntity.controller.HealthBar >= 1f)
            {
                currentHealth = maxHealth; // Reset the health to maxHealth
            }
            else
            {
                // Calculate the current health based on fishEntity's controller
                currentHealth = Mathf.Clamp(initialHealth - (initialHealth * (1f - fishEntity.controller.HealthBar)), 0f, maxHealth);
            }

            // Calculate the fill amount for the health bar
            float fillAmount = currentHealth / maxHealth;

            // Update the health bar's fill amount
            healthBar.value = fillAmount;
            // Debug.Log("Stamina is" + fishEntity.controller.stamina);
        }
    }

}
