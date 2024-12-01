using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using System.Security.Cryptography;

namespace Isac.Isql
{
    public class Credentials 
    {         
        public string UserID = "main";
        public string Pwd = "null";
        public CharEncoding CharSet = CharEncoding.Default;
        
        internal byte[] Key = { 232, 146, 37, 83,
        						161, 186, 153, 135,
        						232, 77, 208, 100,
        						3, 120, 126, 164,
        						62, 183, 199, 87,
        						150, 211, 140, 170,
        						239, 115, 180, 156,
        						2, 131, 142, 83 };
        
        internal byte[] IV = {109, 252, 75, 8,
        						149, 132, 217, 145,
        						70, 186, 172, 95,
        						203, 109, 144, 151};
        
        public Credentials(string UserID, string Pwd, CharEncoding CharSet)
        {
        	this.UserID = UserID;
        	this.Pwd = Pwd;
        	this.CharSet = CharSet;
        	
        	Aes cry = Aes.Create();

        	cry.GenerateIV();
        	cry.GenerateKey();
        	this.IV = cry.IV;
        	this.Key = cry.Key;
        	cry.Dispose();
        }
    }
}

public enum CharEncoding{
	UTF7, UTF8, UTF32, ASCII, Unicode, BigEndianUnicode, Default
}




