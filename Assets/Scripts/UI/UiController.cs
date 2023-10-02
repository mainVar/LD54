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
    [SerializeField] private GameObject _winLevelWindow;
    [SerializeField] private TextMeshProUGUI KillsCount;

    private Animator comboAnimator;
    private Entity playerEntity;

    [Inject] private GameService gameService;

    private bool inited;

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
            if (hp <= 0)
            {
                GameOver();
            }
        }
    }
   
    public void GameOver()
    {
        _gameOver.SetActive(true);
        Time.timeScale = 0;
    }

    public void PlayerWin()
    {
        Time.timeScale = 0;
        _winLevelWindow.SetActive(true);
    }
    
}