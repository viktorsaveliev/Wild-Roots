using UnityEngine;
using UnityEngine.UI;

public class LobbySlot : MonoBehaviour
{
    [SerializeField] private GameObject _loading;
    [SerializeField] private GameObject _loaded;
    [SerializeField] private Text _nickname;
    [SerializeField] private Text _level;
    [SerializeField] private GameObject _character;

    public bool IsUsed { get; private set; }

    public void TakeSlot(string nickname, int level)
    {
        if (IsUsed)
        {
            print("[ERROR #131] Can not take a slot");
            return;
        }
        _loading.SetActive(false);

        _nickname.text = nickname;
        _level.text = level.ToString();
        IsUsed = true;

        _character.SetActive(true);
    }

    public void ResetSlot()
    {
        if (!IsUsed)
        {
            print("[ERROR #131] Slot specified incorrectly");
            return;
        }
        _character.SetActive(false);
        _loaded.SetActive(false);

        IsUsed = false;
        _loading.SetActive(true);
    }
}
