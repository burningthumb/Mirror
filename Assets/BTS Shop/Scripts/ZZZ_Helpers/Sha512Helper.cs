using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Sha512Helper : MonoBehaviour
{
    		public static string GetSHA512(string text)
		{
		    SHA512 sha = new SHA512Managed();
     
		    //compute hash from the bytes of text
		    sha.ComputeHash(Encoding.ASCII.GetBytes(text));
       
		    //get hash result after compute it
		    byte[] result = sha.Hash;
     
		    StringBuilder strBuilder = new StringBuilder();
		    for (int i = 0; i < result.Length; i++)
		    {
		      //change it into 2 hexadecimal digits
		      //for each byte
		      strBuilder.Append(result[i].ToString("x2"));
		    }
         
		  return strBuilder.ToString();
		}
}
