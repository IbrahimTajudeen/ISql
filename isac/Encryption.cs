using System;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Isac
{
    internal sealed class Encryption 
    { 
    	
    	internal byte[] KEY = { 102, 145, 111, 120,
    							189, 70, 99, 134,
    							198, 127, 121, 133, 
    							181, 124, 230, 238,
    							207, 36, 22, 237, 
    							92, 154, 196, 6, 
    							68, 97, 210, 218, 
    							122, 249, 103, 122 };
    							
    	internal byte[] IV = { 68, 231, 177, 173,
    						   238, 24, 198, 171,
    						   64, 130, 131, 202,
    						   130, 243, 69, 137 };
    	
        public byte[] EncryptData(string data, byte[] key, byte[] iv, Encoding encode)
        {
        	byte[] encrypt_data; //data = $@"{data}";
        	using(Aes myAes = Aes.Create())
        	{
        		myAes.Key = key; myAes.IV = iv;
        		ICryptoTransform transform = myAes.CreateEncryptor(myAes.Key, myAes.IV);
        		byte[] bytes = encode.GetBytes(data.ToCharArray());
        		using(MemoryStream ms = new MemoryStream())
        		{
        			using(CryptoStream cryptoStream = new CryptoStream(ms, transform, CryptoStreamMode.Write))
        			{
        				using(StreamWriter writer = new StreamWriter(cryptoStream, encode))
        				{
        					writer.Write(data);
        				}
        				encrypt_data = ms.ToArray();
        			}
        		}
        	}
        	
        	return encrypt_data;
        }
        
        public string DecryptData(byte[] data, byte[] key, byte[] iv, Encoding encode)
        {
        	string decrypt_data;
        	using(Aes myAes = Aes.Create())
        	{
        		myAes.Key = key; myAes.IV = iv;
        		ICryptoTransform transform = myAes.CreateDecryptor(myAes.Key, myAes.IV);
        		
        		using(MemoryStream ms = new MemoryStream(data))
        		{
        			using(CryptoStream cryptoStream = new CryptoStream(ms, transform, CryptoStreamMode.Read))
        			{
        				using(StreamReader reader = new StreamReader(cryptoStream, encode))
        				{
        					decrypt_data = reader.ReadLine(); //$@"{reader.ReadLine()}";
        				}
        			}
        		}
        	}
        	
        	return decrypt_data;
        }
         
        public byte[] EncryptLine(string data, byte[] key, byte[] iv, Encoding encode)
        {
        	byte[] _data = encode.GetBytes("\r\n".ToCharArray());
        	return encode.GetBytes(string.Join(",", EncryptData(data, key, iv, encode))).Concat(_data).ToArray<byte>();
        }
        
        public string DecryptLine(byte[] data, byte[] key, byte[] iv, Encoding encode)
        {
        	byte[] com = encode.GetBytes("\r\n".ToCharArray());

        	if (com.Length < data.Length)
        	{
        		List<byte> cobyte = new List<byte>();
        		cobyte.AddRange(data.Skip(data.Length - com.Length));
        		
        		if (cobyte.SequenceEqual(com.ToList<byte>()))
        			data = data.Take(data.Length - com.Length).ToArray<byte>();
        	}
        	
         	List<byte> ldata = new List<byte>();
        	foreach(var m in encode.GetString(data).Split(new string[] { @"," }, StringSplitOptions.RemoveEmptyEntries))
        	{
        		try{
        			if(m.Trim() != string.Empty)
        				ldata.Add(byte.Parse(m.ToString()));
        		}catch(FormatException)
        		{
        			foreach(var j in encode.GetBytes(m))
        				ldata.Add(j);
        		}
        	}
        	
        	string decrypted = "0,0";
        	
        	if(!ldata.ToArray<byte>().SequenceEqual(new byte[] {0,0}))
        		decrypted = DecryptData(ldata.ToArray<byte>(), key, iv, encode);
        	
        	return decrypted;
        }
        
        public string GetEncryptBytes(object[] objectData, Encoding encode)
        {
        	string endata = ""; int count = 0;
        	
        	foreach (var d in objectData)
        	{
        		if(count == 0 && count < objectData.Length - 1)
        			endata += $@"{d.ToString()}" + @"\t";
        		else if(count >= objectData.Length - 1)
        			endata += $@"{d.ToString()}";
        		else if (count < objectData.Length - 1)
        			endata += $@"{d.ToString()}" + @"\t";
        		
        		count++;
        	}
        	
        	endata = $@"{endata}";
        	return endata;
        }
        
        public string[] GetDecryptBytes(string data, Encoding encode)
        {
        	
        	List<string> rdata = new List<string>();
        	if(data.Contains(@"\t"))
        	{
        		string[] byteArr = data.Split(new string[] { @"\t" }, StringSplitOptions.RemoveEmptyEntries);
        		foreach(var d in byteArr)
        		{
        			/*if(d.Contains(@","))
        			{
        				List<byte> barr = new List<byte>();
        				foreach(var cd in d.Split(new string[] { @"," }, StringSplitOptions.RemoveEmptyEntries));
        					//barr.Add(cd);

        				rdata.Add(d);
        			}
        			else rdata.Add(d);*/
        			rdata.Add(d);
        		}
        	}
        	/*else if(data.Contains(@","))
        	{
        		List<byte> barr = new List<byte>();
        		foreach(var cd in data.Split(new string[] { @"," }, StringSplitOptions.RemoveEmptyEntries))
        			barr.Add(byte.Parse(cd));
        			
        		rdata.Add(data);
        	}*/
        	else rdata.Add(data);
        	
        	
        	
        	return rdata.ToArray<string>();
        }
        
        public byte[] EncryptAll(object[] data, byte[] key, byte[] iv, Encoding encode)
        {
        	return	EncryptLine(
        			GetEncryptBytes(data, encode),
        			key, iv, encode);
        }
        
        public string[] DecryptAll(string data, byte[] key, byte[] iv, Encoding encode)
        {
        	return	GetDecryptBytes(
        			DecryptLine(encode.GetBytes(
        			data.ToCharArray()),
        			key, iv, encode), encode);
        }
       
    }
}





