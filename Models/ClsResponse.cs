using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    public class ClsResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<ClsError> Errors { get; set; }
        public string WhatsappUrl { get; set; }
        public int WhitelabelType { get; set; }
        public string UserType { get; set; }
        public ClsData Data { get; set; }
    }

    public class ClsError
    {
        public string Message { get; set; }
        public string Id { get; set; }
    }
}