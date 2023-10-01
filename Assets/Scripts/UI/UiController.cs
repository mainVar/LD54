using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wargon.DI;
using Wargon.ezs;
using LD54;


public class UiController : MonoBehaviour
{
    [SerializeField] private Image _newGameButton;
    [SerializeField] private GameObject _gameOver;
    [SerializeField] private TextMeshProUGUI KillsCount;

    private Animator comboAnimator;
    private Entity playerEntity;

    [Inject] private GameService gameService;

    private bool inited;
    private int killCount;

    IEnumerator Start()
    {
        Injector.ResolveObject(this);
        
        Time.timeScale = 1;
        KillsCount.text = 0.ToString();
        yield return new WaitForSeconds(1f);
        inited = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!inited) return;
        playerEntity = gameService.PlayerEntity;
        if (playerEntity.IsNULL()) return;
        if (playerEntity != null)
        {
            float hp = playerEntity.Get<Health>().current;
            float maxHp = playerEntity.Get<Health>().max;
            _newGameButton.fillAmount = hp / maxHp;
            if (hp <= 2)
            {
                GameOver();
            }
        }
    }


    public void AddKills()
    {
        killCount++;
        KillsCount.text = killCount.ToString();
    }

    public void ShowCombo(int size)
    {
        if (comboAnimPlaying) return;
        StartCoroutine(ComboAnimDelay(0.5f));
        comboAnimator.Play("ComboShow");
    }

    private bool comboAnimPlaying;

    private IEnumerator ComboAnimDelay(float deley)
    {
        comboAnimPlaying = true;
        yield return new WaitForSeconds(deley);
        comboAnimPlaying = false;
    }

    public void GameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        _gameOver.SetActive(true);
        Time.timeScale = 0;
    }
}