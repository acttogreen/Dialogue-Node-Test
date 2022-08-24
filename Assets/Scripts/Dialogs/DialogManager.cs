using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VIDE_Data;
using System;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public GameObject container_NPC, container_Player;
    public TextMeshProUGUI NPC_Text;
    public bool NPC_animateText;
    public float NPC_secsPerLetter;
    public TextMeshProUGUI[] choices_Texts;

    public AudioSource audioSource;
    public Image playerSprite, NPCSprite;
    public TextMeshProUGUI playerAlias, NPCAlias;

    public KeyCode interact;
    private IEnumerator TextAnimator;

    private void Start()
    {
        container_Player.SetActive(false);
        container_NPC.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(interact))
        {
            if (!VD.isActive) Begin();
            else
            {
                //if (animatingText) { CutTextAnim(); return; }
                if (!dialoguePaused) VD.Next();
            }
        }
    }

    private void Begin()
    {
        VD.OnNodeChange += UpdateUI;
        VD.OnEnd += End;
        VD.BeginDialogue(GetComponent<VIDE_Assign>());

        SetSpritePlayer(null);
        SetSpriteNPC(null);
    }
    private void UpdateUI(VD.NodeData data)
    {
        NPCSprite.sprite = null;
        playerSprite.sprite = null;
        container_Player.SetActive(false);
        container_NPC.SetActive(false);

        if (data.isPlayer)
        {
            SetSpritePlayer(data);
            container_Player.SetActive(true);
            for (int i = 0; i < choices_Texts.Length; i++)
            {
                if (i < data.comments.Length)
                {
                    choices_Texts[i].transform.parent.gameObject.SetActive(true);
                    choices_Texts[i].text = data.comments[i];
                }
                else
                {
                    choices_Texts[i].transform.parent.gameObject.SetActive(false);
                }
            }
            choices_Texts[0].transform.parent.GetComponent<Button>().Select();
            if (data.tag.Length > 0) playerAlias.SetText(data.tag);
        }
        else
        {
            container_NPC.SetActive(true);
            SetSpriteNPC(data);

            if (NPC_animateText)
            {
                //This coroutine animates the NPC text instead of displaying it all at once
                TextAnimator = AnimateNPCText(data.comments[data.commentIndex]);
                StartCoroutine(TextAnimator);
            }
            else NPC_Text.text = data.comments[data.commentIndex];
        }

        if (data.audios[data.commentIndex])
        {
            audioSource.PlayOneShot(data.audios[data.commentIndex]);
        }
    }

    private void SetSpriteNPC(VD.NodeData data)
    {
        if (data == null)
        {
            NPCSprite.sprite = null;
            return;
        }
        if (data.sprites[data.commentIndex] != null) NPCSprite.sprite = data.sprite;
        else if (data.extraVars.ContainsKey("sprite"))
        {
            if (data.commentIndex == (int)data.extraVars["sprite"]) NPCSprite.sprite = data.sprite;
            else NPCSprite.sprite = VD.assigned.defaultNPCSprite; //If not there yet, set default dialogue sprite
        }
    }
    private void SetSpritePlayer(VD.NodeData data)
    {
        if (data == null)
        {
            playerSprite.sprite = null;
            return;
        }
        if (data.sprite != null) playerSprite.sprite = data.sprite;
        else if (VD.assigned.defaultPlayerSprite != null) playerSprite.sprite = VD.assigned.defaultPlayerSprite;
    }

    private void End(VD.NodeData data)
    {
        container_Player.SetActive(false);
        container_NPC.SetActive(false);
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= End;
        VD.EndDialogue();
    }

    private void OnDisable()
    {
        if (container_NPC) End(null);
    }

    public void SetPlayerChoice(int choice)
    {
        VD.nodeData.commentIndex = choice;
        if (Input.GetMouseButtonUp(0)) VD.Next();
    }

    private bool animatingText, dialoguePaused;
    IEnumerator AnimateNPCText(string text)
    {
        animatingText = true;
        Debug.Log("Animate Texts");
        string[] words = text.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (i != words.Length - 1) word += " ";

            string previousText = NPC_Text.text;

            float lastHeight = NPC_Text.preferredHeight;
            NPC_Text.text += word;
            if (NPC_Text.preferredHeight > lastHeight)
            {
                previousText += System.Environment.NewLine;
            }

            for (int j = 0; j < word.Length; j++)
            {
                NPC_Text.text = previousText + word.Substring(0, j + 1);
                yield return new WaitForSeconds(NPC_secsPerLetter);
            }
        }
        NPC_Text.text = text;
        animatingText = false;
    }

    private void CutTextAnim()
    {
        StopCoroutine(TextAnimator);
        if (!VD.nodeData.isPlayer)
        {
            NPC_Text.text = VD.nodeData.comments[VD.nodeData.commentIndex]; //Now just copy full text	
        }
        else
        {

        }
        animatingText = false;
    }
}
