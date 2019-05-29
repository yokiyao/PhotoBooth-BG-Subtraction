using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DoozyUI;
public class KeyboardInput : MonoBehaviour
{

    string inputText = null;

    string addString;
    string deleteString;
    List<char> inputfieldTextChar;
    public InputField inputfield = null;
    // Use this for initialization

    public UIButton[] letterImages;
    public UIButton[] numberSymbolImages;
    public UIButton capsImage;
    public UIButton shiftImage;
    public Sprite[] letterSprites;
    public Sprite[] numberSymbolSprites;
    public Sprite[] letterCapSprites;
    public Sprite[] numberCapSymbolSprites;
    public Sprite capsSprite;
    public Sprite capsCapSprite;
    public Sprite shiftSprite;
    public Sprite shiftCapSprite;
    int myCaretPosition;
    private void Start()
    {
        inputfieldTextChar = new List<char>();
    }

    public void Update()
    {
        if (inputfield.isFocused)
        {
           myCaretPosition = inputfield.caretPosition;
        
        }
        
        //if (inputText!=null) print(inputfield.caretPosition + "   " + inputText.Length);
    }

    public void AddLetter(string alphabet)
    {
        char[] keepchar = alphabet.ToCharArray();
       
        if (isCapSelected || isShiftSelected)
        {
            keepchar[0] = char.ToUpper(keepchar[0]);
            if (isShiftSelected)
            {
                ShiftTrigger();
            }
        }

        char onechar = keepchar[0];

        InsertNewletter(onechar);
    
    }

    int length;
    bool isCutMiddle = false;

    void CheckIfCutAtMiddle()
    {
        if (inputText == null)
        {
            length = 0;
        }
        else
        {
            length = inputText.Length;
        }

        if (myCaretPosition != 0 && myCaretPosition != length)
        {
            isCutMiddle = true;
        }
        else if (myCaretPosition == length)
        {
            isCutMiddle = false;
        }

    }


   
    public void AddSymbol(UIButton thisButton)
    {
       
        if (!isShiftSelected)
        {
            char[] keepchar = thisButton.GetComponent<SymbolString>().symbol.ToCharArray();
           
            char onechar = keepchar[0];
            InsertNewletter(onechar);
        }
        else
        {
            ShiftTrigger();
            char[] keepchar = thisButton.GetComponent<SymbolString>().shiftedSymbol.ToCharArray();
            char onechar = keepchar[0];
           
            InsertNewletter(onechar);
        }

        

       
        
    }

    void InsertNewletter(char onechar)
    {

        CheckIfCutAtMiddle();

        if (!isCutMiddle)
        {
            myCaretPosition++;
            inputfieldTextChar.Add(onechar);
            addString = inputfieldTextChar[inputfieldTextChar.Count - 1].ToString();
            inputText = inputText + addString;
        }
        else
        {
            inputfieldTextChar.Insert(myCaretPosition, onechar);
            myCaretPosition++;
            char[] newchars = new char[inputfieldTextChar.Count];
            for (int i = 0; i < inputfieldTextChar.Count; i++)
            {
                newchars[i] = inputfieldTextChar[i];

            }
            string newstring = new string(newchars);
            inputText = newstring;

        }



        inputfield.text = inputText;
    }


    public void DeleteLetter()
    {
     
        if (inputfieldTextChar.Count > 0 && myCaretPosition != 0)
        {
           
            inputfieldTextChar.RemoveAt(myCaretPosition - 1);
            myCaretPosition--;
            char[] newchars = new char[inputfieldTextChar.Count];
            for (int i = 0; i < inputfieldTextChar.Count; i++)
            {
                newchars[i] = inputfieldTextChar[i];

            }
            string newstring = new string(newchars);
            inputText = newstring;

        }
        inputfield.text = inputText;

    }


    bool isCapSelected = false;
    bool isShiftSelected = false;

    public void CapsTrigger()
    {
        isCapSelected = !isCapSelected;
        if (isCapSelected)
        {
            for (int i = 0; i < letterImages.Length; i++)
            {
                letterImages[i].GetComponentInChildren<Image>().sprite = letterCapSprites[i];
            }

            capsImage.GetComponentInChildren<Image>().sprite = capsCapSprite;
        }
        else
        {
            for (int i = 0; i < letterImages.Length; i++)
            {
                letterImages[i].GetComponentInChildren<Image>().sprite = letterSprites[i];
            }
            capsImage.GetComponentInChildren<Image>().sprite = capsSprite;
        }
       
    }

    public void ShiftTrigger()
    {
        isShiftSelected = !isShiftSelected;
        if (isShiftSelected)
        {
            for (int i = 0; i < letterImages.Length; i++)
            {
                letterImages[i].GetComponentInChildren<Image>().sprite = letterCapSprites[i];
            }
            
            for (int j = 0; j < numberSymbolImages.Length; j++)
            {
                numberSymbolImages[j].GetComponentInChildren<Image>().sprite = numberCapSymbolSprites[j];
            }

            shiftImage.GetComponentInChildren<Image>().sprite = shiftCapSprite;
        }
        else
        {
            for (int i = 0; i < letterImages.Length; i++)
            {
                letterImages[i].GetComponentInChildren<Image>().sprite = letterSprites[i];
            }

            for (int j = 0; j < numberSymbolImages.Length; j++)
            {
                numberSymbolImages[j].GetComponentInChildren<Image>().sprite = numberSymbolSprites[j];
            }
            shiftImage.GetComponentInChildren<Image>().sprite = shiftSprite;
        }

    }

    public void ResetContent()
    {
        if (isCapSelected)
        {
            CapsTrigger();
        }
        if (isShiftSelected)
        {
            ShiftTrigger();
        }
        inputfieldTextChar.Clear();
        //textIndex = -1;
        myCaretPosition = 0;
        inputText = null;
        inputfield.text = inputText;
    }

}