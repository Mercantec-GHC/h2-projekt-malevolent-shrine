using API.Models;

namespace API.Services
{
    //  по категории выбираем роль-исполнителя
    public class TicketRoutingService
    {
        public string ResolveTargetRoleName(TicketCategory category)
        {
            return category switch
            {
                TicketCategory.Cleaning => RoleNames.Rengøring,
                TicketCategory.Technical => RoleNames.Manager,
                TicketCategory.General => RoleNames.Manager,
                _ => RoleNames.Manager
            };
        }
    }
}
