using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u22559711_HW03.Models
{
    public class CombinedReportViewModel
    {
        public List<u22559711_HW03.Models.FileModel> FileList { get; set; }
        public u22559711_HW03.Models.FileModel SingleFile { get; set; }
    }
}