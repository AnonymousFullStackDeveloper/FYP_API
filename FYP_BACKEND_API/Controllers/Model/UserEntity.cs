namespace FYP_BACKEND_API.Controllers.Model
{
    public class UserEntity
    {
        public int User_Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CreatedAt { get; set; }


        public UserDetails UserDetails { get; set; }


    }
}
