using LD54.Sound;
using UnityEngine;
using Wargon.DI;

public class StaticClear : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnApplicationQuit() {
        MenuSound.hasStarted = false;
        Injector.DisposeAll();
    }
}
