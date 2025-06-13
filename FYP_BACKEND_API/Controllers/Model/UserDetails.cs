using FYP_Backend.Controllers.Model;

namespace FYP_BACKEND_API.Controllers.Model
{
    public class UserDetails
    {
        public List<GroupMemberEntity> groupMembers { get; set; }
        public List<User_Location_History> userLocationsHistory {  get; set; }
        //public List<UserGeofences> userGeofences { get; set; }
        public List<UserFriends> userFriends { get; set; }


    }
}
