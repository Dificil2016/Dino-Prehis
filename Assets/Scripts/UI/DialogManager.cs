using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;

    public event Action OnDialogFinished;

    public static DialogManager Instance {  get; private set; }

    int currentLine = 0;
    Dialog currentDialog;
    bool isTyping = false;
    public bool isShowing { get; private set; }

    public void Awake()
    {
        Instance = this;
        isShowing = false;
        dialogBox.SetActive(false);
    }

    public void HandleUpdate()
    {
        if (GameController.Instance.state == GameState.Dialog)
        {
            if (Input.GetKeyDown(KeyCode.Space) && isTyping == false)
            {
                currentLine++;
                if (currentLine < currentDialog.Lines.Count)
                { StartCoroutine(TypeDialog(currentDialog.Lines[currentLine])); }
                else
                {
                    dialogBox.SetActive(false);
                    isShowing = false;
                    OnDialogFinished?.Invoke();
                    GameController.Instance.CloseDialog();
                }
            }
        }
    }

    public IEnumerator ShowDialog(Dialog dialog, Action OnFinished=null)
    {
        yield return new WaitForEndOfFrame();
        isShowing = true;
        GameController.Instance.ShowDialog();
        currentLine = 0;
        isTyping = false;
        OnDialogFinished = OnFinished;

        currentDialog = dialog;

        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(currentDialog.Lines[currentLine]));
    }

    public void SetDialog(String text)
    {
        isShowing = true;
        currentLine = 0;

        dialogBox.SetActive(true);
        dialogText.text = text;
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        isShowing = false;
        OnDialogFinished?.Invoke();
        GameController.Instance.CloseDialog();
    }

    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        isTyping = false;
    }
}
