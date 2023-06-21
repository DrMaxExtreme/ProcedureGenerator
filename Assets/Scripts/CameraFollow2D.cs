using UnityEngine;
public class CameraFollow2D : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string playerTag;
    [SerializeField][Range(0.5f, 7.5f)] private float movingSpeed = 1.5f;

    private void Awake()
    {
        if (playerTransform == null)
        {
            if (playerTag == "")
            {
                playerTag = "Player";
            }

            playerTransform = GameObject.FindGameObjectWithTag(playerTag).transform;
        }

        transform.position = new Vector3()
        {
            x = playerTransform.position.x,
            y = playerTransform.position.y + 3,
            z = playerTransform.position.z - 2,
        };
    }

    private void Update()
    {
        if (playerTransform)
        {
            Vector3 target = new Vector3()
            {
                x = playerTransform.position.x,
                y = playerTransform.position.y + 3,
                z = playerTransform.position.z - 2,
            };

            Vector3 pos = Vector3.Lerp(transform.position, target, movingSpeed * Time.deltaTime);

            transform.position = pos;
        }
    }

}
