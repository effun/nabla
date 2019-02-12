using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Web.Mvc.Html.Bootstrap
{
    public class ProgressBarOptions : ComponentOptions
    {
        public bool ShowLabel { get; set; }

        public string LabelFormatString { get; set; }

        public bool Striped { get; set; }

        public bool Animated { get; set; }
    }
}
