using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace INCHEQS.Security
{

    public class CustomPrincipal : IPrincipal
    {        
        public AccountModel account;
        public CustomPrincipal(AccountModel account)
        {
            this.account = account;
        }

        public IIdentity Identity
        {
            get { return new GenericIdentity(this.account.UserId); }
        }
                
        public bool IsInRole(string role)
        {
            List<string> roles = role.Split(new char[] { ',' }).ToList();
            return roles.Any(r => this.account.TaskIds.ContainsKey(r));
        }
    }

}