using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.dto.responses
{
    public class UserRoleDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? DateOfBirth { get; set; }
        public string? CreationDate { get; set; }
        public bool IsDisabled { get;set; }
        public IList<string>? Roles { get; set; }
    }
}
