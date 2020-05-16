using UnityEngine;

public class LineManager : MonoBehaviour
{
    [SerializeField]
    private GameManager game = null;

    [SerializeField]
    private Transform playerTransform = null;

    [SerializeField]
    private Line[] lines = null;

    private void OnEnable()
    {
        lines = game.level.GetChild(0).GetComponentsInChildren<Line>();
        HomogenizeLineColors();
    }

    private void Update()
    {
        foreach (Line line in lines)
        {
            line.MakeEndsVisibleBasedOnHowFarThePlayerIsFromItInDegrees(playerTransform.forward);
        }
    }

    private void HomogenizeLineColors()
    {
        foreach (Line line in lines)
        {
            line.SetParentPosition();
            line.SetPlayColor();
        }
    }
}
