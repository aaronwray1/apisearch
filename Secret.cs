using System;
using Azure.Search.Documents.Indexes;

namespace searchapi
{
    public class Secret
    {
        [SimpleField(IsKey = true)]
        public string ID { get; set; }

        [SearchableField(IsSortable = true, IsFacetable = true)]
        public string Username { get; set; }

        [SearchableField(IsSortable = true, IsFacetable = true)]
        public string SecretName { get; set; }

        [SearchableField(IsSortable = true, IsFacetable = true)]
        public string Password { get; set; }

        [SearchableField(IsSortable = true, IsFacetable = true)]
        public string OrganisationID { get; set; }

        public Organisation Organisation { get; set; }


    }

}
