using System.Collections.Generic;
using System.Linq;

namespace Kibali
{
    public class AcceptableClaimComparer : IEqualityComparer<AcceptableClaim>
    {
        public bool Equals(AcceptableClaim x, AcceptableClaim y)
        {
            if (x == null || y == null)
                return false;

            // Check if Permission is the same
            if (x.Permission != y.Permission)
                return false;

            // Check if AlsoRequires is the same
            if (x.AlsoRequires == null && y.AlsoRequires == null)
                return true;

            if (x.AlsoRequires == null || y.AlsoRequires == null)
                return false;

            // Compare the contents of the AlsoRequires arrays
            return x.AlsoRequires.SequenceEqual(y.AlsoRequires);
        }

        public int GetHashCode(AcceptableClaim obj)
        {
            if (obj?.Permission == null)
                return 0;

            int hash = obj.Permission.GetHashCode();

            if (obj.AlsoRequires != null)
            {
                foreach (var item in obj.AlsoRequires)
                {
                    hash ^= item.GetHashCode();
                }
            }

            return hash;
        }
    }
}
