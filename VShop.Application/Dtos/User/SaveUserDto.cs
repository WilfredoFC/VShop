using System;
using System.Collections.Generic;
using System.Text;

namespace VShop.Application.Dtos.User
{
    public class SaveUserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Cedula { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public bool Status { get; set; }

    }
}
