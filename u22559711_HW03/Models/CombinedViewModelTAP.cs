using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u22559711_HW03.Models
{
    public class CombinedViewModelTAP
    {
        public PagedList.IPagedList<types> Type { get; set; }
        public PagedList.IPagedList<authors> Author { get; set; }
        public PagedList.IPagedList<borrows> Borrow { get; set; }
    }
}