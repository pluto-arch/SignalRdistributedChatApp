using ChatService.Application.Models.Generics;

namespace ChatService.Application.Permission.Models;

public class GetPagedPermissionRequest : PageRequest
{
    public string PermissionName { get; set; }
}