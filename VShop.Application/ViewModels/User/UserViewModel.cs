namespace VShop.Application.ViewModels.User
{
    public class UserViewModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Cedula { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Role { get; set; }
        public bool isVerified { get; set; }
        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
