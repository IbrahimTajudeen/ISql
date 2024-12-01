using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac.Isql
{
    public sealed class Permission 
    {   
        private bool canWrite;
        private bool canRead;
        private bool canUpdate;
        private bool canDelete;
        private bool canModifyUsers;
        
        internal bool allow_modify = true;
        
        #region Encapsulation
        public bool CanModifyUsers
        {
        	get { return canModifyUsers; }
        	set 
        	{
        		if(!allow_modify)
        			throw new ISqlModifierException($"Error: this property can not be modified");
        		canModifyUsers = value;
        	}
        }
        
        public bool CanDelete
        {
        	get { return canDelete; }
        	set 
        	{
        		if(!allow_modify)
        			throw new ISqlModifierException($"Error: this property can not be modified");
        		canDelete = value;
        	}
        }
        
        public bool CanUpdate
        {
        	get { return canUpdate; }
        	set 
        	{
        		if(!allow_modify)
        			throw new ISqlModifierException($"Error: this property can not be modified");
        		canUpdate = value;
        	}
        }
        
        public bool CanRead
        {
        	get { return canRead; }
        	set 
        	{
        		if(!allow_modify)
        			throw new ISqlModifierException($"Error: this property can not be modified");
        		canRead = value;
        	}
        }
        
        public bool CanWrite
        {
        	get { return canWrite; }
        	set 
        	{
        		if(!allow_modify)
        			throw new ISqlModifierException($"Error: this property can not be modified");
        		canWrite = value;
        	}
        }
        #endregion
        
        public static Permission Parse(string permissionString)
        {
        	permissionString = permissionString.Trim();
        	Permission permission = new Permission(); 
        	string authenticate = permissionString.Substring(0, permissionString.IndexOf(permissionString.Contains(@"<") ? @"<" : "<"));
        	string[] datas = permissionString.Substring(permissionString.IndexOf(permissionString.Contains(@"<") ? @"<" : "<") + 1, permissionString.LastIndexOf(permissionString.Contains(@">") ? @">" : ">") - permissionString.IndexOf(permissionString.Contains(@"<") ? @"<" : "<") - 1).Split(permissionString.Contains(@"\t") ? new string[] { @"\t" } : new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
        	
        	foreach(string data in datas)
        	{
        		if(data.ToLower().StartsWith(@"canwrite:"))
        		{
                    if (data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
        				throw new ISqlArguementException($"Error: invalid arguement to parse '{data}'");
        				
        			permission.CanWrite = bool.Parse(data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
        			continue;
        		}
        			
        		else if(data.ToLower().StartsWith(@"canread:"))
        		{
        			if(data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
        				throw new ISqlArguementException($"Error: invalid arguement to parse");
        				
        			permission.CanRead = bool.Parse(data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
        			continue;
        		}
        		
        		else if(data.ToLower().StartsWith(@"candelete:"))
        		{
        			if(data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
        				throw new ISqlArguementException($"Error: invalid arguement to parse");
        				
        			permission.CanDelete = bool.Parse(data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
        			continue;
        		}
        		
        		else if(data.ToLower().StartsWith(@"canupdate:"))
        		{
        			if(data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
        				throw new ISqlArguementException($"Error: invalid arguement to parse");
        				
        			permission.CanUpdate = bool.Parse(data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
        			continue;
        		}
        		
        		else if(data.ToLower().StartsWith(@"canmodifyusers:"))
        		{
        			if(data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
        				throw new ISqlArguementException($"Error: invalid arguement to parse");
        				
        			permission.CanModifyUsers = bool.Parse(data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
        			continue;
        		}
        		else throw new ISqlArguementException($"Error: invalid permission object\nData: '{data}'");
        	}
        	
        	if(!(authenticate.ToLower().Trim() == "permission" || authenticate.ToLower().Trim() == "permissions"))
        		throw new ISqlArguementException($"Error: invalid object string passed\n'{authenticate}'");
        		
        	return permission;
        }
        
        public override string ToString()
        {
        	return $@"Permission<CanWrite:{CanWrite}\tCanRead:{CanRead}\tCanUpdate:{CanUpdate}\tCanDelete:{CanDelete}\tCanModifyUsers:{CanModifyUsers}>";
        }
        
        public Permission()
        {
        	CanWrite = true;
        	CanRead = true;
        	CanUpdate = true;
        	CanDelete = true;
        	CanModifyUsers = false;
        }
    }
}




