using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace OneNightWebolution
{
    public class OneNightWebolutionHub : Hub
    {
        public void CreateParty()
        {
            Clients.All.hello();
        }
    }
}