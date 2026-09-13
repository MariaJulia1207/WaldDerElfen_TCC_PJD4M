using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroDirector : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private IntroTextWriter introTextWriter;

    public void StartIntro()
    {
        if (introTextWriter != null)
        {
            introTextWriter.IniciarIntroducao();
        }
    }

    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level1");
    }
}