using UnityEngine;
using UnityEngine.Playables;

public class TutorialUIContoller : MonoBehaviour
{
    [SerializeField, Required] Canvas _tutorialCanvas = default!;
    [SerializeField, Required] PlayableDirector _timelime = default!;

    private void Awake()
    {
        _tutorialCanvas.enabled = false;
    }

    public void StartTutorial()
    {
        _tutorialCanvas.enabled = true;
        _timelime.Play();
    }

    public void FinishTutorial()
    {
        _tutorialCanvas.enabled = false;
        _timelime.Stop();
    }
}
