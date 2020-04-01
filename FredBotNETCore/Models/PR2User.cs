namespace FredBotNETCore.Models
{
    public class PR2User
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Rank { get; set; }
        public string Hats { get; set; }
        public string Group
        {
            get
            {
                if (_group == "0")
                {
                    _group = "Guest";
                }
                else if (_group == "1")
                {
                    _group = "Member";
                }
                else if (_group == "2")
                {
                    _group = "[Moderator](https://pr2hub.com/staff.php)";
                }
                else if (_group == "3")
                {
                    _group = "[Admin](https://pr2hub.com/staff.php)";
                }
                return _group;
            }
            set
            {
                _group = value;
            }
        }
        private string _group;
        public string Friend { get; set; }
        public string Ignored { get; set; }
        public string Status { get; set; }
        public string LoginDate { get; set; }
        public string RegisterDate
        {
            get
            {
                if (_registerDate.Equals("1/Jan/1970"))
                {
                    _registerDate = "Age of Heroes";
                }
                return _registerDate;
            }
            set
            {
                _registerDate = value;
            }
        }
        private string _registerDate;
        public string Hat { get; set; }
        public string Head { get; set; }
        public string Body { get; set; }
        public string Feet { get; set; }
        public string HatColor { get; set; }
        public string HeadColor { get; set; }
        public string BodyColor { get; set; }
        public string FeetColor { get; set; }
        public string GuildId { get; set; }
        public string GuildName { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
    }
}
