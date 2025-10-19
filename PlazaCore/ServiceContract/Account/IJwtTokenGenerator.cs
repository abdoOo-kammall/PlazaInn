using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.ServiceContract.Account
{
    public interface IJwtTokenGenerator
    {
        public Task<string> GenerateToken(ApplicationUser user) ;
    }
}
