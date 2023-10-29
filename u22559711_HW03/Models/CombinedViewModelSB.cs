using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace u22559711_HW03.Models
{
    public class CombinedViewModelSB
    {
        public PagedList.IPagedList<students> Students { get; set; }
        public PagedList.IPagedList<books> Books { get; set; }
    }
}