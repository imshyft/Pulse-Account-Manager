using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Core.Models
{
    public class Account
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public char Symbol => (char)SymbolCode;
        public int SymbolCode { get; set; }
    }
}
