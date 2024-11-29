using UnityEngine;

namespace ERang
{
    public class SpriteBillboard : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        }
    }
}