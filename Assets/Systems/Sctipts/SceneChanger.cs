using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;


public class SceneChanger : MonoBehaviour
{

    [SerializeField] Graphic _Bg;

    #region Singleton

    private static SceneChanger instance;

    public static SceneChanger Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SceneChanger)FindObjectOfType(typeof(SceneChanger));

                if (instance == null)
                {
                    Debug.LogError(typeof(SceneChanger) + "is nothing");
                }
            }

            return instance;
        }
    }

    #endregion Singleton

    public void Awake()
    {
        if (this != Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public async void LoadScene(string scene)
    {
        var token = this.GetCancellationTokenOnDestroy();

        _Bg.gameObject.SetActive(true);
        await FadeOut(_Bg, 1.0f, token);

        //暗転中にシーン切り替える
        SceneManager.LoadScene(scene);

        await FadeIn(_Bg, 1.0f, token);
        _Bg.gameObject.SetActive(false);
    }

    public async UniTask FadeIn(Graphic g, float fadeTime, CancellationToken ct = default)
    {
        float start = 1f;
        float end = 0f;

        await Fade(_Bg, fadeTime: fadeTime, start: start, end: end, ct);
    }

    public async UniTask FadeOut(Graphic g, float fadeTime, CancellationToken ct = default)
    {
        float start = 0f;
        float end = 1f;

        await Fade(_Bg, fadeTime: fadeTime, start: start, end: end, ct);
    }

    private async UniTask Fade(Graphic g, float fadeTime, float start, float end, CancellationToken ct = default)
    {
        float time = 0f;
        var c = g.color;

        g.color = new Color(c.r, c.g, c.b, start);


        while (time < fadeTime)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(start, end, time / fadeTime);
            g.color = new Color(c.r, c.g, c.b, a);
            await UniTask.Yield(PlayerLoopTiming.Update, ct); //次のUpdateまで待機
        }

        g.color = new Color(c.r, c.g, c.b, end);
    }
}
