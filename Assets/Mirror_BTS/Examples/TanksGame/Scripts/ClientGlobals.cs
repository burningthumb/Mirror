using UnityEngine;

public class ClientGlobals : MonoBehaviour
{
    private static int s_selectedTank = 0;

    // Static getter and setter
    public static int SelectedTank
    {
        get => s_selectedTank;
        set => s_selectedTank = value;
    }

}
