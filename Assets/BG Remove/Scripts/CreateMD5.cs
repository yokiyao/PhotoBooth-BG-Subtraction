using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
public class CreateMD5 : MonoBehaviour {

    #region encode filename
    string hashString;

    public string callEncry(string inputString)
    {
        MD5 md5Hash = MD5.Create();
        hashString = GetMD5Hash(md5Hash, inputString);
        return hashString;
    }


    private string GetMD5Hash(MD5 md5Hash, string input)
    {
        //Convert the input string to a byte array and compute the hash.
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        //Create a new StringBuilder to collect the bytes and create a string.
        StringBuilder builder = new StringBuilder();

        //Loop through each byte of the hashed data and format each one as a hexadecimal strings.
        for (int cnt = 0; cnt < 3; cnt++)
        {
            builder.Append(data[cnt].ToString("x2"));
        }

        //Return the hexadecimal string
        return builder.ToString();
    }

    #endregion
}
