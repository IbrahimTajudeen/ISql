using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql
{
	public sealed class User
	{
		private int id;
		private string userID;
		private AccountType userType;
		private string pwd;
		private Permission userPermission;

		internal bool allow_modify = true;
		
		#region Encapsulation
		public Permission UserPermission
		{
			get { return userPermission; }
			set
			{
				if (!allow_modify)
					throw new ISqlModifierException($"Error: this property can not be modified");
				if (value == null)
					throw new ISqlArguementException($"Error: UserPermission can not null");
				userPermission = value;
			}
		}

		public string Pwd
		{
			get
			{
				if (!allow_modify)
					throw new ISqlException($"Error: this property can not be retrived.");
				return pwd;
			}
			set
			{
				if (!allow_modify)
					throw new ISqlModifierException($"Error: this property can not be modified");
				if (value.Trim() == "" || value == null)
					throw new ISqlArguementException($"Error: Pwd can not be empty");
				pwd = value;
			}
		}

		public AccountType UserType
		{
			get { return userType; }
			set
			{
				if (!allow_modify)
					throw new ISqlModifierException($"Error: this property can not be modified");
				userType = value;
			}
		}

		public string UserID
		{
			get { return userID; }
			set
			{
				if (!allow_modify)
					throw new ISqlModifierException($"Error: this property can not be modified");
				if (value.Trim() == "" || value == null)
					throw new ISqlArguementException($"Error: UserID can not be empty string");
				userID = value;
			}
		}

		public int ID
		{
			get { return id; }
			set
			{
				if (!allow_modify)
					throw new ISqlModifierException($"Error: this property can not be modified");
				if (value < 0)
					throw new ISqlArguementException($"Error: value cannot be negetive");

				id = value;
			}
		}
		#endregion

		public static User Parse(string userString)
		{
			userString = userString.Trim();

			User user = new User();
			user.ID = int.Parse(userString.Substring(0, userString.IndexOf(userString.Contains(@"\t") ? @"\t" : "\t")));
			int v = (userString.Contains(@"\t")) ? 2 : 1;
			string authenticate = userString.Substring(userString.IndexOf(userString.Contains(@"\t") ? @"\t" : "\t") + v, userString.IndexOf(@"<") - userString.IndexOf(userString.Contains(@"\t") ? @"\t" : "\t") - v);
			
			string[] userPer = (!userString.ToLower().Contains(@"permission<")) ? userString.Substring(userString.IndexOf(userString.Contains(@"<") ? @"<" : "<") + 1,
											userString.LastIndexOf(userString.Contains(@">") ? @">" : ">")
											- userString.IndexOf(userString.Contains(@"<") ? @"<" : "<") - 1)
											.Split(userString.Contains(@"\t") ? new string[] { @"\t" } : new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries) : userString.Split(new string[] { @"Permission<", @"PERMISSION<", @"permission<" }, StringSplitOptions.RemoveEmptyEntries);
											
			//string[] userPer = (userString.ToLower().Contains(@"permission<")) ? userString.Split(new string[] { @"Permission<" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { userString };
			if(userPer[0].EndsWith(@"\t") || userPer[0].EndsWith("\t"))
			{
				userPer[0] = (userPer[0].EndsWith(@"\t")) ? userPer[0].Remove(userPer[0].LastIndexOf(@"\t")) : userPer[0].Remove(userPer[0].LastIndexOf("\t"));
			}
			userPer[0] += (userPer[0].EndsWith(@">") ? @"" : ">");
			
			if (userPer.Length > 1)
			{
				userPer[1] = (userPer[1].EndsWith(">>")) ? userPer[1].Remove(userPer[1].Length - 1, 1) : userPer[1];
				userPer[1] = "Permission<" + userPer[1];
			}
			
			string[] datas = userPer[0].Substring(userPer[0].IndexOf("<") + 1,
											userPer[0].LastIndexOf(userPer[0].Contains(@">") ? @">" : ">")
											- userPer[0].IndexOf(userPer[0].Contains(@"<") ? @"<" : "<") - 1)
											.Split(userPer[0].Contains(@"\t") ? new string[] { @"\t" } : new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
			
			foreach (string data in datas)
			{
				if (data == "") continue;

				if (data.ToLower().StartsWith(@"userid:"))
				{
					if (data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
						throw new ISqlArguementException($"Error: invalid arguement to parse");

					user.UserID = data.Split(new string[] { ":" } , StringSplitOptions.RemoveEmptyEntries)[1].Trim();
					continue;
				}

				else if (data.ToLower().StartsWith(@"usertype:"))
				{
					if (data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
						throw new ISqlArguementException($"Error: invalid arguement to parse");

					user.UserType = (AccountType)Enum.Parse(typeof(AccountType), data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
					continue;
				}

				else if (data.ToLower().StartsWith(@"userpwd:") || data.ToLower().StartsWith(@"pwd:"))
				{
					if (data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
						throw new ISqlArguementException($"Error: invalid arguement to parse");

					user.Pwd = data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
					continue;
				}

				else throw new ISqlArguementException($"Error: invalid user object\nData: '{data}'");
			}

			if (userString.ToLower().Contains(@"permission<"))
			{
				string permission = userPer[1].Substring(0, userPer[1].LastIndexOf(@">") + 1);
				user.UserPermission = Permission.Parse(permission);
			}


			if (authenticate.ToLower().Trim() != "user")
				throw new ISqlArguementException($"Error: invalid object string passed '{authenticate}'");

			return user;
		}

		public User()
		{
			ID = 0; UserID = "main"; UserType = AccountType.Standard;
			Pwd = "null"; UserPermission = new Permission();
		}

		public override string ToString()
		{
			return $@"{ID}\tUser<UserID:{UserID}\tUserType:{UserType}\tUserPwd:{Pwd}\t{UserPermission.ToString()}>";
		}
	}

	public enum AccountType
	{
		Standard = 1, Admin = 0
	}
}





