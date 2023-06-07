namespace LicentaBun.Models
{
    public class LoginResponse
    {

        public string Nickname { get; set; }
        public int UniqueID { get; set; }
        public string Token { get; set; }

        public LoginResponse() { ; }

        public LoginResponse(string nickname, int uniqueID, string token)
        {
            Nickname = nickname;
            UniqueID = uniqueID;
            Token = token;
        }
    }
}
