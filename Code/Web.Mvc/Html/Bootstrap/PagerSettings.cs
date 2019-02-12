using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Web.Mvc.Html.Bootstrap
{
    public class PagerSettings : ICloneable
    {
        public string FirstPageText { get; set; } = "First Page";

        public string PreviousPageText { get; set; } = "Previous Page";

        public string NextPageText { get; set; } = "Next Page";

        public string LastPageText { get; set; } = "Last Page";

        public string PageLinkHref { get; set; } = "#";

        public int MaxButtons { get; set; } = 8;

        public BootstrapSize Size { get; set; }

        public PagerSettings Clone()
        {
            return (PagerSettings)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
