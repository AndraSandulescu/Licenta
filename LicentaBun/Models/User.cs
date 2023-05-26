using System;
using System.Collections.Generic;

namespace LicentaBun.Models
{
    public partial class User
    {
        public int PkUsers { get; set; }
        public string Nickname { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
