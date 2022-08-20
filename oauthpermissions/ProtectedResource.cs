using System.Collections.Generic;

namespace ApiPermissions
{
    public class ProtectedResource
    {
        // Permission -> (Methods,Scheme) -> Path  (Darrel's format)
        // (Schemes -> Permissions) -> restriction -> target  (Kanchan's format)
        // target -> restrictions -> schemes -> Ordered Permissions (CSDL Format) 

        // path -> Method -> Schemes -> Permissions  (Inverted format) 
        
        // (Path, Method) -> Schemes -> Permissions (Docs)
        // (Path, Method) -> Scheme(delegated) -> Permissions (Graph Explorer Tab)
        // Permissions(delegated) (Graph Explorer Permissions List)
        // Schemas -> Permissions ( AAD Onboarding)



        public string Url { get; set; }
        public Dictionary<string, Dictionary<string,List<AcceptableClaim>>> SupportedMethods { get; set; } = new Dictionary<string, Dictionary<string, List<AcceptableClaim>>>();

        public ProtectedResource(string url)
        {
            Url = url;
        }

        public void AddRequiredClaims(string permission, PathSet pathSet)
        {
            foreach (var supportedMethod in pathSet.Methods)
            {
                var supportedSchemes = new Dictionary<string, List<AcceptableClaim>>();
                foreach (var supportedScheme in pathSet.SchemeKeys)
                {
                    if (!supportedSchemes.ContainsKey(supportedScheme))
                    {
                        supportedSchemes.Add(supportedScheme, new List<AcceptableClaim>());
                    }
                    supportedSchemes[supportedScheme].Add(new AcceptableClaim(permission, pathSet.AlsoRequires));
                }
                if (!this.SupportedMethods.ContainsKey(supportedMethod))
                {
                    this.SupportedMethods.Add(supportedMethod, supportedSchemes);
                } else
                {
                    Update(this.SupportedMethods[supportedMethod], supportedSchemes);
                };
            }
        }

        private void Update(Dictionary<string, List<AcceptableClaim>> existingSchemes, Dictionary<string, List<AcceptableClaim>> newSchemes)
        {
            
            foreach(var newScheme in newSchemes)
            {
                if (existingSchemes.TryGetValue(newScheme.Key, out var existingScheme))
                {
                    existingScheme.AddRange(newScheme.Value);
                } 
                else
                {
                    existingSchemes[newScheme.Key] = newScheme.Value;
                }
            }
        }
    }
}
