using UnityEngine.UI;
using UnityEngine;

public class TurnNumber : MonoBehaviour
{
    [SerializeField] Text _number;

    public void SetNumber(int turnNumber)
    {
        _number.text = turnNumber.ToString();
    }
}
