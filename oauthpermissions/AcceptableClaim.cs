using System;
using System.Linq;

namespace ApiPermissions
{
    public class AcceptableClaim
    {
        public AcceptableClaim(string permission, string alsoRequires)
        {
            this.Permission = permission;
            this.AlsoRequires = alsoRequires;
        }
        public string Permission { get; }
        public string AlsoRequires { get;  }

        internal bool IsAuthorized(string[] providedPermissions)
        {
            return providedPermissions.Contains(this.Permission);  //TODO: add support for alsoRequires
        }
    }
}
